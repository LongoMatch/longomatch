// Config.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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

namespace LongoMatch
{
	
	
	public class SectionsReader
	{
		private XmlDocument configXml;
		private string fConfig;

		
		public SectionsReader(string filePath)
		{
		
			configXml = new XmlDocument();
			
			
			if (!File.Exists(filePath)){
				//manejar el error!!!
			}
			fConfig = Path.Combine (MainClass.TemplatesDir(), filePath);
			configXml.Load(fConfig);
	
		}
		
		private string GetStringValue(string section, string clave) 
		{    
    		XmlNode n;
    		n = configXml.SelectSingleNode(section + "/add[@key=\"" + clave + "\"]");
    		object result = n.Attributes["value"].Value;
    		return (result is string ) ? (string)result : null;
			
		}
		
		private int GetIntValue(string section, string clave) 
		{    
    		XmlNode n;
    		n = configXml.SelectSingleNode(section + "/add[@key=\"" + clave + "\"]");
			if (n != null){
				object result = n.Attributes["value"].Value;				
				return int.Parse(result.ToString());
    		}
			else return -1;
			
		}
			

		private String[] GetNames(){
			String[] names = new String[20];
			for (int i=0;i<20;i++){
				names[i] = this.GetStringValue("configuration","Name"+(i+1));
			}
			return names;
		}
		
		private Time[] GetStartTimes(){
			Time[] startTimes = new Time [20];
			for (int i=0;i<20;i++){
				
				startTimes[i] = new Time(GetIntValue("configuration","Stop"+(i+1))*Time.SECONDS_TO_TIME);
		
			}
			return startTimes;
		
		}
		
		private Time[] GetStopTimes(){
			Time[] stopTimes = new Time [20];
			for (int i=0;i<20;i++){
				stopTimes[i] = new Time(GetIntValue("configuration","Stop"+(i+1))*Time.SECONDS_TO_TIME);

			}
			return stopTimes;	
		
		}
		
		public Sections GetSections(){
			Sections sections = new Sections(20);
			this.GetStartTimes();
			sections.SetTimeNodes(this.GetNames(),this.GetStartTimes(),this.GetStopTimes());
			return sections;
		}
		
		
	}
}
