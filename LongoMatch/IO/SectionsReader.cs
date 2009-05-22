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
using LongoMatch.DB;
using Gdk;
using LongoMatch.TimeNodes;

namespace LongoMatch.IO
{
	
	
	public class SectionsReader : XMLReader
	{
		
#region Constructors
		
		public SectionsReader(string filePath) : base (filePath) 
		{						
		}
#endregion		
		
#region Private methods
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
				startTimes[i] = new Time(GetIntValue("configuration","Start"+(i+1))*Time.SECONDS_TO_TIME);
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
		
		private bool[] GetVisibility(){
			bool[] visibility = new bool [20];
			for (int i=0;i<20;i++){
				visibility[i] = GetBoolValue("configuration","Visible"+(i+1));

			}
			return visibility;		
		}
		
		private Color[] GetColors(){
			Color[] colors = new Color[20];
			ushort red,green,blue;
			for (int i=0;i<20;i++){
				red = GetUShortValue("configuration","Red"+(i+1));
				green = GetUShortValue("configuration","Green"+(i+1));
				blue = GetUShortValue("configuration","Blue"+(i+1));
				Color col = new Color();
				col.Red = red;
				col.Blue = blue;
				col.Green = green;
				colors[i] = col;					
			}
			return colors;			
		}
		
		private HotKey[] GetHotKeys(){
			HotKey[] hotkeys = new HotKey[20];
			for (int i=0;i<20;i++){
				HotKey hotkey = new HotKey();
				hotkey.Modifier= (ModifierType)GetIntValue("configuration","Modifier"+(i+1));
				hotkey.Key = (Gdk.Key)GetIntValue("configuration","Key"+(i+1));
				hotkeys[i]=hotkey;
			}
			return hotkeys;
		}
			
#endregion		

#region Public methods
		public Sections GetSections(){
			Sections sections = new Sections(20);
			this.GetStartTimes();		
			sections.SetTimeNodes(GetNames(),GetStartTimes(),GetStopTimes(),GetVisibility(),GetHotKeys(),GetColors());
			return sections;
		}
#endregion
		
	}
}
