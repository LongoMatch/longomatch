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
using Couchbase.Lite;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace LongoMatch.DB
{
	public static class DocumentsSerializer
	{
		public const string DOC_TYPE = "DocType";
		public const string OBJ_TYPE = "ObjType";

		public static void SaveObject (IStorable obj, Database db, SerializationContext context = null)
		{
			if (context == null) {
				context = new SerializationContext (db, obj.GetType ());
			}
			Document doc = db.GetDocument (obj.ID.ToString ());
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
		}

		public static object LoadObject (Type objType, Guid id, Database db, SerializationContext context = null)
		{
			if (context == null) {
				context = new SerializationContext (db, objType);
			}
			Document doc = db.GetExistingDocument (id.ToString ());
			Type realType = Type.GetType (doc.Properties [OBJ_TYPE] as string);
			if (realType == null) {
				/* Should never happen */
				Log.Error ("Error getting type " + doc.Properties [OBJ_TYPE] as string);
				realType = objType;
			}
			return DeserializeObject (doc, realType, context);
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
			jo [DOC_TYPE] = obj.GetType ().Name;
			jo [OBJ_TYPE] = jo ["$type"];
			jo.Remove ("$type");
			return jo;
		}

		/// <summary>
		/// Deserializes a <c>Document</c>
		/// </summary>
		/// <returns>A new object deserialized.</returns>
		/// <param name="doc">The document to deserialize.</param>
		/// <param name = "objType"><see cref="Type"/> of the object to deserialize</param>
		/// <param name="context">The serialization context"/>
		internal static object DeserializeObject (Document doc, Type objType, SerializationContext context)
		{
			JObject jo = JObject.FromObject (doc.Properties);
			return jo.ToObject (objType, GetSerializer (objType, context, doc.CurrentRevision));
		}

		internal static JsonSerializer GetSerializer (Type objType, SerializationContext context, Revision rev)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings ();
			settings.Formatting = Formatting.Indented;
			settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			settings.TypeNameHandling = TypeNameHandling.Objects;
			settings.Converters.Add (new ImageConverter (rev));
			settings.Converters.Add (new VersionConverter ());
			settings.Converters.Add (new LongoMatchConverter (false));
			settings.Converters.Add (new StorablesConverter (objType, context));
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

			if (!context.Cache.IsCached (storable.ID)) {
				DocumentsSerializer.SaveObject (storable, context.DB, context);
				context.Cache.AddReference (storable);
			}
			writer.WriteValue (storable.ID);
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue,
		                                 JsonSerializer serializer)
		{
			Guid id;
			IStorable storable;

			id = Guid.Parse (reader.Value as string);
			/* Return the cached object instance instead a new one */
			storable = context.Cache.ResolveReference (id);
			if (storable == null) {
				storable = DocumentsSerializer.LoadObject (objectType, id, context.DB, context) as IStorable;
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
}

