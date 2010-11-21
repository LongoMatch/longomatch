// SectionsWriter.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Configuration;
using System.IO;
using System.Xml;
using Mono.Unix;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using Gdk;

namespace LongoMatch.IO
{


	public class CategoriesWriter
	{



		public static void CreateNewTemplate(string templateName) {

			XmlDocument configXml = new XmlDocument();
			string fConfig = Path.Combine(MainClass.TemplatesDir(), templateName);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
			sb.Append("<configuration>");

			for (int i=1;i<21;i++) {
				sb.Append("<add key=\"Name"+i+"\" value=\"Data "+i+"\" />");
				sb.Append("<add key=\"Start"+i+"\" value=\"10\" />");
				sb.Append("<add key=\"Stop"+i+"\" value=\"10\" />");
				sb.Append("<add key=\"Red"+i+"\" value=\"65535\" />");
				sb.Append("<add key=\"Green"+i+"\" value=\"0\" />");
				sb.Append("<add key=\"Blue"+i+"\" value=\"0\" />");
				sb.Append("<add key=\"Modifier"+i+"\" value=\"-1\" />");
				sb.Append("<add key=\"Key"+i+"\" value=\"-1\" />");
				sb.Append("<add key=\"SortMethod"+i+"\" value=\""+Catalog.GetString("Sort by name")+"\" />");
			}

			sb.Append("</configuration>");
			configXml.LoadXml(sb.ToString());
			configXml.Save(fConfig);
		}


		public static void SetValue(XmlDocument configXml,string section, string clave, string valor)
		{
			XmlNode n;
			n = configXml.SelectSingleNode(section + "/add[@key=\"" + clave + "\"]");
			if (n != null)    {
				n.Attributes["value"].Value = valor;
			}
		}



		public static void UpdateTemplate(string templateName,Categories categories) {

			string fConfig = Path.Combine(MainClass.TemplatesDir(), templateName);
			XmlDocument configXml = new XmlDocument();
			int i=1;

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
			sb.Append("<configuration>");

			foreach (var cat in categories.CategoriesList) {
				sb.Append(String.Format("<add key=\"Name{0}\" value=\"{1}\" />",i,cat.Name));
				sb.Append(String.Format("<add key=\"Start{0}\" value=\"{1}\" />",i,cat.Start.Seconds));
				sb.Append(String.Format("<add key=\"Stop{0}\" value=\"{1}\" />",i,cat.Stop.Seconds));
				sb.Append(String.Format("<add key=\"Red{0}\" value=\"{1}\" />",i,cat.Color.Red));
				sb.Append(String.Format("<add key=\"Green{0}\" value=\"{1}\" />",i,cat.Color.Green));
				sb.Append(String.Format("<add key=\"Blue{0}\" value=\"{1}\" />",i,cat.Color.Blue));
				sb.Append(String.Format("<add key=\"Modifier{0}\" value=\"{1}\" />",i,(int)(cat.HotKey.Modifier)));
				sb.Append(String.Format("<add key=\"Key{0}\" value=\"{1}\" />",i,(int)(cat.HotKey.Key)));
				sb.Append(String.Format("<add key=\"SortMethod{0}\" value=\"{1}\" />",i,cat.SortMethodString));
				i++;
			}
			sb.Append("</configuration>");
			configXml.LoadXml(sb.ToString());
			configXml.Save(fConfig);
		}
	}
}
