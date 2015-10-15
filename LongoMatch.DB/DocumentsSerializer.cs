//
//  Copyright (C) 2015 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Couchbase.Lite;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using LongoMatch.Core.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace LongoMatch.DB
{
	public static class DocumentsSerializer
	{
		public const string DOC_TYPE = "DocType";
		public const string OBJ_TYPE = "ObjType";
		public const string PARENT_PROPNAME = "Parent";
		public const char ID_SEP_CHAR = '&';

		/// <summary>
		/// Saves a storable object in the database.
		/// </summary>
		/// <param name="obj">Storable object to save.</param>
		/// <param name="db">Database.</param>
		/// <param name="context">Serialization context.</param>
		/// <param name="saveChildren">If set to <c>false</c>, <see cref="IStorable"/> children are not saved.</param>
		public static void SaveObject (IStorable obj, Database db, SerializationContext context = null,
		                               bool saveChildren = true)
		{
			if (context == null) {
				context = new SerializationContext (db, obj.GetType ());
				context.RootID = obj.ID;
			}
			context.SaveChildren = saveChildren;
			context.Stack.Push (obj);
			Document doc = db.GetDocument (DocumentsSerializer.StringFromID (obj.ID, context.RootID));
			doc.Update ((UnsavedRevision rev) => {
				JObject jo = SerializeObject (obj, rev, context);
				IDictionary<string, object> props = jo.ToObject<IDictionary<string, object>> ();

				/* SetProperties sets a new properties dictionary, removing the attachments we
					 * added in the serialization */
				if (rev.Properties.ContainsKey ("_attachments")) {
					props ["_attachments"] = rev.Properties ["_attachments"];
				}
				rev.SetProperties (props);
				return true;
			});
			context.Stack.Pop ();
		}

		/// <summary>
		/// Loads and object from the database for a given type and ID.
		/// </summary>
		/// <returns>The storable object.</returns>
		/// <param name="objType">Object type.</param>
		/// <param name="id">Object ID.</param>
		/// <param name="db">Database.</param>
		/// <param name="context">Serialization context.</param>
		public static IStorable LoadObject (Type objType, Guid id, Database db, SerializationContext context = null)
		{
			return LoadObject (objType, id.ToString (), db, context);
		}

		/// <summary>
		/// Fills a partial storable object reusing the same instance passed in <paramref name="storable"/>.
		/// </summary>
		/// <param name="storable">Storable to fill.</param>
		/// <param name="db">Database to use.</param>
		public static void FillObject (IStorable storable, Database db)
		{
			Log.Debug ("Filling object " + storable);
			SerializationContext context = new SerializationContext (db, storable.GetType ());
			Document doc = db.GetExistingDocument (storable.ID.ToString ());
			JsonSerializer serializer = GetSerializer (storable.GetType (), context, doc.CurrentRevision);
			context.ContractResolver = new StorablesStackContractResolver (context, storable, true);
			serializer.ContractResolver = context.ContractResolver;
			DeserializeObject (doc, storable.GetType (), context, serializer);
		}

		/// <summary>
		/// Deserializes and object from its json string representation.
		/// </summary>
		/// <returns>The deserialized object.</returns>
		/// <param name="json">Object json string.</param>
		/// <param name="db">Database.</param>
		/// <param name="rev">Document revision.</param>
		/// <typeparam name="T">Object type.</typeparam>
		public static T DeserializeFromJson<T> (string json, Database db, Revision rev)
		{
			JsonSerializerSettings settings = GetSerializerSettings (typeof(T),
				                                  new SerializationContext (db, typeof(T)), rev);
			return JsonConvert.DeserializeObject<T> (json, settings);
		}

		/// <summary>
		/// Return the object ID from the document ID string, which can be <ID> or <ParentID>&<ID>.
		/// </summary>
		/// <returns>The object id.</returns>
		/// <param name="id">The document id.</param>
		public static string IDStringFromString (string id)
		{
			if (id == null) {
				return id;
			}
			string[] ids = id.Split (ID_SEP_CHAR);
			if (ids.Length == 1) {
				id = ids [0];
			} else {
				id = ids [1];
			}
			return id;
		}

		/// <summary>
		/// Return the object ID from the document ID string, which can be <ID> or <ParentID>&<ID>.
		/// </summary>
		/// <returns>The object id.</returns>
		/// <param name="id">The document id.</param>
		public static Guid IDFromString (string id)
		{
			return Guid.Parse (IDStringFromString (id));
		}

		/// <summary>
		/// Return the document ID for an object, prepending the parent's ID.
		/// </summary>
		/// <returns>The document id.</returns>
		/// <param name="id">The object ID.</param>
		/// <param name="parentID">The parent ID.</param>
		public static string StringFromID (Guid id, Guid parentID)
		{
			if (parentID != Guid.Empty && parentID != id) {
				return String.Format ("{0}{1}{2}", parentID, ID_SEP_CHAR, id);
			} else {
				return id.ToString ();
			}
		}

		/// <summary>
		/// Serializes an object into a <c>JObject</c>.
		/// </summary>
		/// <returns>A new object serialized.</returns>
		/// <param name="obj">The <c>IStorable</c> to serialize.</param>
		/// <param name="rev">The document revision to serialize.</param>
		/// <param name="context">The serialization context"/>
		internal static JObject SerializeObject (IStorable obj, Revision rev, SerializationContext context)
		{
			JObject jo = JObject.FromObject (obj, GetSerializer (obj.GetType (), context, rev));
			jo [DOC_TYPE] = GetDocumentType (obj);
			jo [OBJ_TYPE] = jo ["$type"];
			jo.Remove ("$type");
			if (context != null && context.RootID != Guid.Empty) {
				jo [PARENT_PROPNAME] = context.RootID;
			}
			return jo;
		}

		/// <summary>
		/// Deserializes a <c>Document</c>
		/// </summary>
		/// <returns>A new object deserialized.</returns>
		/// <param name="doc">The document to deserialize.</param>
		/// <param name = "objType"><see cref="Type"/> of the object to deserialize</param>
		/// <param name="context">The serialization context"/>
		internal static object DeserializeObject (Document doc, Type objType,
		                                          SerializationContext context, JsonSerializer serializer = null)
		{
			if (serializer == null) {
				serializer = GetSerializer (objType, context, doc.CurrentRevision); 
				serializer.ContractResolver = context.ContractResolver;
			}
			JObject jo = JObject.FromObject (doc.Properties);
			var o = jo.ToObject (objType, serializer);
			return o;
		}

		internal static IStorable LoadObject (Type objType, string idStr, Database db, SerializationContext context = null)
		{
			IStorable storable = null, parent;
			Document doc;
			Guid id;

			id = DocumentsSerializer.IDFromString (idStr);
			if (context == null) {
				context = new SerializationContext (db, objType);
				context.ContractResolver = new StorablesStackContractResolver (context, null);
				context.RootID = id;
			}

			doc = db.GetExistingDocument (idStr);
			if (doc != null) {
				Type realType = Type.GetType (doc.Properties [OBJ_TYPE] as string);
				if (realType == null) {
					/* Should never happen */
					Log.Error ("Error getting type " + doc.Properties [OBJ_TYPE] as string);
					realType = objType;
				} else if (!objType.IsAssignableFrom (realType)) {
					Log.Error ("Types mismatch " + objType.FullName + " vs " + realType.FullName);
					return null;
				}
				storable = DeserializeObject (doc, realType, context) as IStorable;
				if (context.Stack.Count != 0) {
					parent = context.Stack.Peek ();
					parent.SavedChildren.Add (storable);
				}
			}
			return storable;
		}

		internal static JsonSerializerSettings GetSerializerSettings (Type objType,
		                                                              SerializationContext context, Revision rev)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings ();
			settings.Formatting = Formatting.Indented;
			settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			settings.TypeNameHandling = TypeNameHandling.Objects;
			settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
			settings.Converters.Add (new ImageConverter (rev));
			settings.Converters.Add (new VersionConverter ());
			settings.Converters.Add (new LongoMatchConverter (false));
			settings.Converters.Add (new StorablesConverter (objType, context));
			return settings;
		}

		internal static JsonSerializer GetSerializer (Type objType, SerializationContext context, Revision rev)
		{
			return JsonSerializer.Create (GetSerializerSettings (objType, context, rev));
		}

		static string GetDocumentType (IStorable storable)
		{
			Type type;

			if (storable is EventType) {
				type = typeof(EventType);
			} else {
				type = storable.GetType ();
			}
			return type.Name;
		}
	}

	/// <summary>
	/// Converts fields with <see cref="LongoMatch.Core.Common.Image"/> objects 
	/// into Attachments, using as field value the name of the attachment prefixed
	/// with the <c>attachment::</c> string.
	/// In the deserialization process, it loads the <see cref="LongoMatch.Core.Common.Image"/>
	/// from the attachment with the same as the one set in the property.
	/// </summary>
	class ImageConverter : JsonConverter
	{
		Revision rev;
		const string ATTACHMENT = "attachment::";
		Dictionary<string, int> attachmentNamesCount;

		public ImageConverter (Revision rev)
		{
			this.rev = rev;
			attachmentNamesCount = new Dictionary<string, int> ();
		}

		string GetAttachmentName (JsonWriter writer)
		{
			string propertyName;
			if (writer.WriteState == WriteState.Array) {
				propertyName = ((writer as JTokenWriter).Token.Last as JProperty).Name;
			} else {
				propertyName = writer.Path;
			}
			if (!attachmentNamesCount.ContainsKey (propertyName)) {
				attachmentNamesCount [propertyName] = 0;
			}
			attachmentNamesCount [propertyName]++;
			return string.Format ("{0}_{1}", propertyName, attachmentNamesCount [propertyName]);
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			string attachName = GetAttachmentName (writer);
			(rev as UnsavedRevision).SetAttachment (attachName, "image/png",
				(value as Image).Serialize ());
			writer.WriteValue (ATTACHMENT + attachName);
		}

		public override object ReadJson (JsonReader reader, Type objectType,
		                                 object existingValue, JsonSerializer serializer)
		{
			if (objectType == typeof(Image)) {
				string valueString = reader.Value as string;

				if (valueString == null) {
					return null;
				}
				if (valueString.StartsWith (ATTACHMENT)) {
					string attachmentName = valueString.Replace (ATTACHMENT, "");
					Attachment attachment = rev.GetAttachment (attachmentName);
					if (attachment == null) {
						return null;
					}
					return Image.Deserialize (attachment.Content.ToArray ());
				} else {
					throw new InvalidCastException ();
				}
			}
			return reader.Value;
		}

		public override bool CanConvert (Type objectType)
		{
			return objectType.Equals (typeof(Image));
		}
	}



	/// <summary>
	/// Serializes and desrializes IStorable objects by ID using a new Document for
	/// each IStorable object.
	/// </summary>
	public class StorablesConverter : JsonConverter
	{
		SerializationContext context;
		Type objType;

		public StorablesConverter (Type objType, SerializationContext context)
		{
			this.context = context;
			this.objType = objType;
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			IStorable storable = value as IStorable;
			if (context.SaveChildren && !context.Stack.Contains (value) && !context.Cache.IsCached (storable.ID)) {
				DocumentsSerializer.SaveObject (storable, context.DB, context);
				context.Cache.AddReference (storable);
			}
			writer.WriteValue (DocumentsSerializer.StringFromID (storable.ID, context.RootID));
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue,
		                                 JsonSerializer serializer)
		{
			IStorable storable;
			Guid id;
			string idStr;

			idStr = reader.Value as string; 
			if (idStr == null) {
				return null;
			}
			id = DocumentsSerializer.IDFromString (idStr);

			/* Check if it's a circular reference and the object is currently being deserialized. In this scenario
			 * the oject is not yet in the cache, but we have a reference of the object in the stack.
			 * eg: TimelineEvent.Project where the TimelineEvent is a children of Project and Project is in the stack
			 * as being deserialized */
			storable = context.Stack.FirstOrDefault (e => e.ID == id);
			if (storable == null) {
				/* Now check in the Cache and return the cached object instance instead a new one */
				storable = context.Cache.ResolveReference (id);
			}

			/* If the object is being deserialized for the first retrieve from the DB document */
			if (storable == null) {
				storable = DocumentsSerializer.LoadObject (objectType, idStr, context.DB, context) as IStorable;
				if (storable == null) {
					throw new StorageException (
						String.Format ("Referenced object with id: {0} was not found in the DB", id));
				}
				context.Cache.AddReference (storable);
			}
			return storable;
		}

		public override bool CanConvert (Type objectType)
		{
			if (typeof(IStorable).IsAssignableFrom (objectType)) {
				if (objectType != objType)
					return true;
				else
					return false;
			}
			return false;
		}
	}


	/// <summary>
	/// This custom <see cref="IContractResolver"/> is used for the following purposes:
	/// <list type="bullet">
	/// <item>
	/// <description>Create a stack of deserialized <see cref="IStorable"/> objects</description>
	/// </item>
	/// <item>
	/// <description>Re-use a partial <see cref="IStorable"/> to fill it instead of creating a new instance.</description>
	/// </item>
	/// </list>
	/// 
	/// The stack is updated overriding the default contructor to push the new object and adding
	/// a deserialized callback to pop it.
	/// 
	/// When a storable is provided in the constructor, each time a new object of the same type is created this
	/// storable is used instead of creating a new one. It's used to fill partial <see cref="IStorable"/> objects
	/// calling <see cref="IStorage.Fill"/>, assuming they haven't children with the same type.
	/// </summary>
	public class StorablesStackContractResolver : DefaultContractResolver
	{
		SerializationContext context;
		IStorable parentStorable;
		bool preservePreloadProperties;

		/// <summary>
		/// Initializes a new instance of the <see cref="LongoMatch.DB.StorablesStackContractResolver"/> class.
		/// If <paramref name="parentStorable"/> is not null, this storable will be used instead of creating
		/// a new instance.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="parentStorable">The partially loaded storable that is going to be filled.</param>
		/// <param name = "preservePreloadProperties">If <c>true</c> reloaded properties are preserved instead of
		/// re-read from the db</param>
		public StorablesStackContractResolver (SerializationContext context, IStorable parentStorable,
		                                       bool preservePreloadProperties = false)
		{
			this.context = context;
			this.parentStorable = parentStorable;
			this.preservePreloadProperties = preservePreloadProperties;
		}

		protected override JsonProperty CreateProperty (MemberInfo member, MemberSerialization memberSerialization)
		{
			// When filling a partial object, do not overwrite the preloaded properties so changes made in
			// the preloaded object are not overwritten.
			JsonProperty property = base.CreateProperty (member, memberSerialization);
			if (property.DeclaringType == context.ParentType) {
				if (preservePreloadProperties &&
				    property.AttributeProvider.GetAttributes (typeof(LongoMatchPropertyPreload), true).Any ()) {
					property.Ignored = true;
				}
			}
			return property;
		}

		protected override JsonContract CreateContract (Type type)
		{
			JsonContract contract = base.CreateContract (type);
			if (typeof(IChanged).IsAssignableFrom (type)) {
				contract.OnDeserializedCallbacks.Add (
					(o, context) => {
						(o as IChanged).IsChanged = false;
					});
			}
			if (typeof(IStorable).IsAssignableFrom (type)) {
				contract.OnDeserializedCallbacks.Add (
					(o, context) => this.context.Stack.Pop ());
				if (parentStorable != null && type == parentStorable.GetType ()) {
					contract.DefaultCreator = () => {
						context.Stack.Push (parentStorable);
						parentStorable.SavedChildren = new List<IStorable> ();
						return parentStorable;
					};
				} else {
					var defaultCreator = contract.DefaultCreator;
					contract.DefaultCreator = () => {
						IStorable storable = defaultCreator () as IStorable;
						storable.SavedChildren = new List<IStorable> ();
						context.Stack.Push (storable);
						return storable;
					};
				}
			}
			return contract;
		}
	}
}

