// XMLReader.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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
using System.IO;
using System.Xml;

namespace LongoMatch
{
	
	
	public class XMLReader
	{
		private XmlDocument configXml;
		
		public XMLReader(string filePath)
		{
			configXml = new XmlDocument();
			if (!File.Exists(filePath)){
				//manejar el error!!!
			}
			
			configXml.Load(filePath);
		}
		
		protected string GetStringValue(string section, string clave) 
		{    
    		XmlNode n;
    		n = configXml.SelectSingleNode(section + "/add[@key=\"" + clave + "\"]");
    		object result = n.Attributes["value"].Value;
    		return (result is string ) ? (string)result : null;
			
		}
		
		protected int GetIntValue(string section, string clave) 
		{    
    		XmlNode n;
    		n = configXml.SelectSingleNode(section + "/add[@key=\"" + clave + "\"]");
			if (n != null){
				object result = n.Attributes["value"].Value;				
				return int.Parse(result.ToString());
    		}
			else return -1;
			
		}
		protected bool GetBoolValue(string section, string clave){
			XmlNode n;
    		n = configXml.SelectSingleNode(section + "/add[@key=\"" + clave + "\"]");

			if (n != null){
				object result = n.Attributes["value"].Value;

				return bool.Parse(result.ToString());
    		}
			else return false;
		}
		protected ushort GetUShortValue(string section, string clave){
			XmlNode n;
    		n = configXml.SelectSingleNode(section + "/add[@key=\"" + clave + "\"]");
			if (n != null){
				object result = n.Attributes["value"].Value;				
				return ushort.Parse(result.ToString());
    		}
			else return 254;
		}
	}
}
