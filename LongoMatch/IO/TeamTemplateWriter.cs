// TeamTemplateWriter.cs
//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.Configuration;
using System.IO;
using System.Xml;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using Gdk;

namespace LongoMatch.IO
{
	
	
	public class TeamTemplateWriter
	{
		

		
		public static void WriteTemplate(string templateName){			
			
			XmlDocument configXml = new XmlDocument();
			string fConfig = Path.Combine (MainClass.TemplatesDir(), templateName);
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.Append("<configuration>");
            
			for (int i=1;i<21;i++){
				sb.Append("<add key=\"Name"+i+"\" value=\"Data "+i+"\" />");
				sb.Append("<add key=\"Start"+i+"\" value=\"10\" />");
				sb.Append("<add key=\"Stop"+i+"\" value=\"10\" />");
				sb.Append("<add key=\"Red"+i+"\" value=\"65535\" />");
				sb.Append("<add key=\"Green"+i+"\" value=\"0\" />");
				sb.Append("<add key=\"Blue"+i+"\" value=\"0\" />");
				sb.Append("<add key=\"Modifier"+i+"\" value=\"-1\" />");
				sb.Append("<add key=\"Key"+i+"\" value=\"-1\" />");
			}
			
			sb.Append("</configuration>");
			configXml.LoadXml(sb.ToString());
            configXml.Save(fConfig);			
		}

		
				
		public static void UpdateTemplate(string templateName,TeamTemplate tTemplate){
			
			string fConfig = Path.Combine (MainClass.TemplatesDir(), templateName);
			XmlDocument configXml = new XmlDocument();
			int i=1;
			byte[] photo;
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            sb.Append("<configuration>");
            
			foreach(Player player in tTemplate.GetPlayersList()){
				photo = player.Photo.SaveToBuffer();
				sb.Append(String.Format("<add key=\"Name{0}\" value=\"{1}\" />",i,player.Name));
				sb.Append(String.Format("<add key=\"Number{0}\" value=\"{1}\" />",i,player.Number));
				sb.Append(String.Format("<add key=\"Position{0}\" value=\"{1}\" />",i,player.Position));
				sb.Append(String.Format("<add key=\"Photo{0}\" value=\"{1}\" />",i,photo.ToString()));
				i++;
			}			
			sb.Append("</configuration>");
			configXml.LoadXml(sb.ToString());
            configXml.Save(fConfig);	
		}
		
	
		
	
		
	}
}
