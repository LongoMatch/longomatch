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

namespace LongoMatch.DB
{
	public static class DocumentsSerializer
	{
		/// <summary>
		/// Serializes an object into a <c>JObject</c>.
		/// </summary>
		/// <returns>A new object serialized.</returns>
		/// <param name="obj">The <c>IStorable</c> to serialize.</param>
		/// <param name="rev">The document revision to serialize.</param>
		/// <param name="localStorables">A list of <see cref="LongoMatch.Core.Interfaces.IStorable"/>
		/// types that should be serialized as local referencies instead of by document ID.</param>
		public static JObject SerializeObject (IStorable obj, Revision rev, List<Type> localStorables)
		{
			JObject jo = JObject.FromObject (obj,
				GetSerializer (obj.GetType (), null, rev, localStorables));
			jo["DocType"] = obj.GetType ().Name;
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
		public static T DeserializeObject<T> (Database db, Document doc, JsonSerializer serializer = null)
		{
			JObject jo = JObject.FromObject (doc.Properties);
			if (serializer == null) {
				serializer = GetSerializer (typeof(T), db, doc.CurrentRevision, null);
			}
			return jo.ToObject<T> (serializer);
		}

		static JsonSerializer GetSerializer (Type serType, Database db, Revision rev, List<Type> localTypes)
		{
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
			//settings.Converters.Add (new DocumentsIDConverter (localTypes));
			settings.Converters.Add (new LongoMatchConverter (false));
			//settings.ReferenceResolver = new IDReferenceResolver (db);
			return JsonSerializer.Create (settings);
		}
	}


	class IdReferenceResolver : IReferenceResolver
	{
		int _references;
		readonly Dictionary<string, object> _idtoobjects;
		readonly Dictionary<object, string> _objectstoid;

		public IdReferenceResolver ()
		{
			_references = 0;
			_idtoobjects = new Dictionary<string, object> ();
			_objectstoid = new Dictionary<object, string> ();
		}

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
				IIDObject p = (IIDObject)value;
				referenceStr = p.ID.ToString ();
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
			string reference;
			return _objectstoid.TryGetValue (value, out reference);
		}

		public void AddReference (object context, string reference, object value)
		{
			_idtoobjects [reference] = value;
			_objectstoid [value] = reference;
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

		string GetAttachmentName (JsonWriter writer) {
			string propertyName;
			if (writer.WriteState == WriteState.Array) {
				propertyName = ((writer as JTokenWriter).Token.Last as JProperty).Name;
			} else {
				propertyName = writer.Path;
			}
			if (!attachmentNamesCount.ContainsKey (propertyName)) {
				attachmentNamesCount [propertyName] = 0;
			}
			attachmentNamesCount [propertyName] ++;
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
	/// Serialize objects matching any of the types lists passed in the constructor
	/// using their object ID.
	/// </summary>
	class DocumentsIDConverter : JsonConverter
	{
		Type[] refTypes;

		public DocumentsIDConverter (Type[] refTypes)
		{
			this.refTypes = refTypes;
			if (this.refTypes == null) {
				this.refTypes = new Type[0] { };
			}
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			writer.WriteValue ((value as IIDObject).ID);
		}

		public override object ReadJson (JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			throw new NotImplementedException ();
		}

		public override bool CanConvert (Type objectType)
		{
			if (refTypes == null) {
				return false;
			}
			return refTypes.Contains (objectType);
		}
	}
}

