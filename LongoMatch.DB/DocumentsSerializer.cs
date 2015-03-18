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
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Couchbase.Lite;
using LongoMatch.Core.Interfaces;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using System.Globalization;
using LongoMatch.Core.Common;
using System.Reflection;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.DB
{
	public static class DocumentsSerializer
	{

		public static void SaveObject (IStorable obj, Database db, IDReferenceResolver resolver = null)
		{
			Document doc = db.GetDocument (obj.ID.ToString ());
			doc.Update ((UnsavedRevision rev) => {
				JObject jo = SerializeObject (obj, rev, db, resolver);
				IDictionary<string, object> props = jo.ToObject<IDictionary<string, object>> ();
				/* SetProperties sets a new properties dictionary, removing the attachments we
					 * added in the serialization */
				if (rev.Properties.ContainsKey ("_attachments")) {
					props ["_attachments"] = rev.Properties ["_attachments"];
				}
				rev.SetProperties (props);
				return true;
			});
		}

		public static object LoadObject (Type objType, Guid id, Database db, IDReferenceResolver resolver = null)
		{
			Document doc = db.GetExistingDocument (id.ToString ());
			return DeserializeObject (objType, doc, db, resolver);
		}

		/// <summary>
		/// Serializes an object into a <c>JObject</c>.
		/// </summary>
		/// <returns>A new object serialized.</returns>
		/// <param name="obj">The <c>IStorable</c> to serialize.</param>
		/// <param name="rev">The document revision to serialize.</param>
		/// <param name="localStorables">A list of <see cref="LongoMatch.Core.Interfaces.IStorable"/>
		/// types that should be serialized as local referencies instead of by document ID.</param>
		internal static JObject SerializeObject (IStorable obj, Revision rev, Database db,
		                                         IDReferenceResolver resolver = null)
		{
			JsonSerializer serializer = GetSerializer (obj.GetType (), rev, db,
				                            resolver, GetLocalTypes (obj.GetType ()));

			JObject jo = JObject.FromObject (obj, serializer);
			jo ["DocType"] = obj.GetType ().Name;
			return jo;
		}

		/// <summary>
		/// Deserializes a <c>Document</c>
		/// </summary>
		/// <returns>A new object deserialized.</returns>
		/// <param name="db">The <c>Database</c> where the Document is stored.</param>
		/// <param name="doc">The document to deserialize.</param>
		/// <param name = "serializer">The serializer to use when deserializing the object</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		internal static object DeserializeObject (Type type, Document doc, Database db,
		                                          IDReferenceResolver resolver = null)
		{
			JObject jo = JObject.FromObject (doc.Properties);
			JsonSerializer serializer = GetSerializer (type, doc.CurrentRevision, db,
				                            resolver, GetLocalTypes (type));
			return jo.ToObject (type, serializer);
		}

		internal static List<Type> GetLocalTypes (Type objType)
		{
			List<Type> localStorables = new List<Type> ();
			if (objType == typeof(Project)) {
				localStorables.Add (typeof(Team));
				localStorables.Add (typeof(Dashboard));
				localStorables.Add (typeof(Player));
				localStorables.Add (typeof(AnalysisEventType));
			}
			return localStorables;
		}

		static JsonSerializer GetSerializer (Type serType, Revision rev, Database db,
		                                     IDReferenceResolver resolver, List<Type> localTypes)
		{
			if (resolver == null) {
				resolver = new IDReferenceResolver ();
			}
			if (localTypes == null) {
				localTypes = new List<Type> ();
			}
			localTypes.Add (serType);
			JsonSerializerSettings settings = new JsonSerializerSettings ();
			settings.Formatting = Formatting.Indented;
			settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			settings.TypeNameHandling = TypeNameHandling.Objects;
			settings.Converters.Add (new ImageConverter (rev));
			settings.Converters.Add (new VersionConverter ());
			settings.Converters.Add (new StorablesConverter (db, resolver, localTypes));
			settings.Converters.Add (new LongoMatchConverter (false));
			settings.ReferenceResolver = resolver;
			return JsonSerializer.Create (settings);
		}
	}


	/// <summary>
	/// Converts fields with <see cref="LongoMatch.Core.Common.Image"/> objects 
	/// into Attachments, using as field value the name of the attachment prefixed
	/// with the <c>attachment::</c> string.
	/// In the desrialization process, it loads <see cref="LongoMatch.Core.Common.Image"/>
	/// from the attachment with the same as the set in the property.
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

	public class IDReferenceResolver: IReferenceResolver
	{
		int _references;
		readonly Dictionary<string, object> _idtoobjects;
		readonly Dictionary<object, string> _objectstoid;

		public IDReferenceResolver ()
		{
			_references = 0;
			_idtoobjects = new Dictionary<string, object> ();
			_objectstoid = new Dictionary<object, string> ();
		}

		public bool IsCached (string id)
		{
			return _idtoobjects.ContainsKey (id);
		}

		#region IReferenceResolver implementation

		public object ResolveReference (object context, string reference)
		{
			object p;
			_idtoobjects.TryGetValue (reference, out p);
			return p;
		}

		public string GetReference (object context, object value)
		{
			string referenceStr;
			if (value is IIDObject) {
				referenceStr = (value as IIDObject).ID.ToString ();
			} else {
				if (!_objectstoid.TryGetValue (value, out referenceStr)) {
					_references++;
					referenceStr = _references.ToString (CultureInfo.InvariantCulture); 
				}
			}
			_idtoobjects [referenceStr] = value;
			_objectstoid [value] = referenceStr;
			return referenceStr;
		}

		public bool IsReferenced (object context, object value)
		{
			/* IStorable are always serialized by ID and not by reference */
			if (value is IStorable) {
				return false;
			}
			return _objectstoid.ContainsKey (value);
		}

		public void AddReference (object context, string reference, object value)
		{
			_idtoobjects [reference] = value;
			_objectstoid [value] = reference;
		}

		#endregion
	}

	/// <summary>
	/// Serialize objects matching any of the types lists passed in the constructor
	/// using their object ID.
	/// </summary>
	public class StorablesConverter : JsonConverter
	{
		List<Type> localTypes;
		Database db;
		IDReferenceResolver idResolver;

		public StorablesConverter (Database db, IDReferenceResolver idResolver, List<Type> localTypes)
		{
			this.db = db;
			this.idResolver = idResolver;
			this.localTypes = localTypes;
			if (this.localTypes == null) {
				this.localTypes = new List<Type> ();
			}
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			IStorable storable = value as IStorable;
			writer.WriteValue (storable.ID);
			/* Only persist objects once per session */
			if (!idResolver.IsCached (storable.ID.ToString ())) {
				DocumentsSerializer.SaveObject (storable, db, idResolver);
				idResolver.AddReference (this, storable.ID.ToString (), storable);
			}
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string idStr = reader.Value as string;
			object obj;

			/* Return the cached object instance instead a new one */
			obj = idResolver.ResolveReference (this, idStr);
			if (obj == null) {
				obj = DocumentsSerializer.LoadObject (objectType, Guid.Parse (idStr), db, idResolver);
				idResolver.AddReference (this, idStr, obj);
			}
			return obj;
		}

		public override bool CanConvert (Type objectType)
		{
			bool ret;

			if (!typeof(IStorable).IsAssignableFrom (objectType)) {
				ret = false;
			} else {
				ret = !localTypes.Contains (objectType);
			}
			return ret;
		}

	}
}

