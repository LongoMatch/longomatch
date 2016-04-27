//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAS.Core.Common;
using VAS.Core.Serialization;

namespace LongoMatch.Addins
{
	public class ConfigurablePlugin
	{

		public ConfigurablePlugin ()
		{
			Properties = new List<AttributeAndProperty> ();
			foreach (var p in GetType ().GetProperties ()) {
				var attr = p.GetCustomAttributes (typeof(PreferencesAttribute), true);
				if (attr.Length != 1) {
					continue;
				}
				Properties.Add (new AttributeAndProperty { Property = p,
					Attribute = attr.First () as PreferencesAttribute
				});
			}
			Load ();
		}

		public virtual string Name {
			get;
			set;
		}

		public List <AttributeAndProperty> Properties {
			get;
			set;
		}

		string ConfigFile {
			get {
				string filename = String.Format ("{0}.config", Name);
				filename = filename.Replace (" ", "_"); 
				foreach (char c in Path.GetInvalidFileNameChars()) {
					filename = filename.Replace (c.ToString (), ""); 
				}
				return Path.Combine (Config.ConfigDir, filename);
			}
		}

		void Load ()
		{
			if (File.Exists (ConfigFile)) {
				using (StreamReader reader = File.OpenText (ConfigFile)) {
					JObject o;
					try {
						o = (JObject)JToken.ReadFrom (new JsonTextReader (reader));
					} catch {
						return;
					}
					foreach (AttributeAndProperty prop in Properties) {
						PropertyInfo info = prop.Property;
						try {
							var value = Convert.ChangeType (o [info.Name], info.PropertyType);
							info.SetValue (this, value, null);
						} catch (Exception ex) {
							Log.Exception (ex);
						}
					}
				}
			}
		}

		protected void Save ()
		{
			JObject o = new JObject ();
			foreach (AttributeAndProperty prop in Properties) {
				o [prop.Property.Name] = new JValue (prop.Property.GetValue (this, null));
			}
			try {
				Serializer.Instance.Save (o, ConfigFile);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}
	}

	public class AttributeAndProperty
	{

		public PreferencesAttribute Attribute {
			get;
			set;
		}

		public PropertyInfo Property {
			get;
			set;
		}
	}
}

