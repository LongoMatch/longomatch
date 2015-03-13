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
		/// <param name="preserveReferences">If set to <c>true</c> preserve references.</param>
		/// <param name="refTypes">A list of types that should be serialized by ID.</param>
		/// <param name="skipFields">Extra fields that shouldn't be serialized.</param>
		public static JObject SerializeObject (IStorable obj, Revision rev, bool preserveReferences,
		                                       Type[] refTypes)
		{
			JObject jo = JObject.FromObject (obj,
				GetSerializer (null, rev, preserveReferences, refTypes));
			jo["DocType"] = obj.GetType ().Name;
			return jo;
		}

		/// <summary>
		/// Deserializes a <c>Document</c>
		/// </summary>
		/// <returns>A new object deserialized.</returns>
		/// <param name="db">The <c>Database</c> where the Document is stored.</param>
		/// <param name="doc">The document to deserialize.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T DeserializeObject<T> (Database db, Document doc)
		{
			JObject jo = JObject.FromObject (doc.Properties);
			return jo.ToObject<T> (GetSerializer (db, doc.CurrentRevision, true, null));
		}

		static JsonSerializer GetSerializer (Database db, Revision rev,
		                                     bool preserveReferences, Type[] refTypes)
		{
			JsonSerializerSettings settings = new JsonSerializerSettings ();
			settings.Formatting = Formatting.Indented;
			if (preserveReferences) {
				settings.PreserveReferencesHandling = PreserveReferencesHandling.Objects;
			}
			settings.TypeNameHandling = TypeNameHandling.Objects;
			settings.ContractResolver = new ImagePropertiesContractResolver (rev);
			settings.Converters.Add (new ImageConverter (rev));
			settings.Converters.Add (new VersionConverter ());
			settings.Converters.Add (new DocumentsIDConverter (refTypes));
			settings.Converters.Add (new LongoMatchConverter (false));
			//settings.ReferenceResolver = new IDReferenceResolver (db);
			return JsonSerializer.Create (settings);
		}
	}


	/// <summary>
	/// Prevents serializing properties with <c>Image</c> objects that should be stored
	/// as attachments
	/// </summary>
	class ImagePropertiesContractResolver : DefaultContractResolver
	{
		ImageConverter imgConverter;

		public ImagePropertiesContractResolver (Revision rev)
		{
			imgConverter = new ImageConverter (rev);
		}

		protected override JsonProperty CreateProperty (MemberInfo member,
		                                                MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty (member, memberSerialization);

			if (property.PropertyType == typeof(Image)) {
				property.ShouldSerialize = d => true;
			}
			return property;
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

		public ImageConverter (Revision rev)
		{
			this.rev = rev;
		}

		public override void WriteJson (JsonWriter writer, object value, JsonSerializer serializer)
		{
			(rev as UnsavedRevision).SetAttachment (writer.Path, "image/png",
				(value as Image).Serialize ());
			writer.WriteValue (ATTACHMENT + writer.Path);
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

