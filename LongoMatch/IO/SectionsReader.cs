// Config.cs
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
using LongoMatch.DB;
using Gdk;
using LongoMatch.TimeNodes;

namespace LongoMatch.IO
{


	public class SectionsReader : XMLReader
	{

		#region Constructors

		public SectionsReader(string filePath) : base(filePath)
		{
		}
		#endregion

		#region Private methods
		private string GetName(int section) {
			return this.GetStringValue("configuration","Name"+(section));
		}

		private Time GetStartTime(int section) {
			return new Time(GetIntValue("configuration","Start"+(section))*Time.SECONDS_TO_TIME);
		}

		private Time GetStopTime(int section) {
			return new Time(GetIntValue("configuration","Stop"+(section))*Time.SECONDS_TO_TIME);
		}

		private Color GetColor(int section) {
			ushort red,green,blue;
			red = GetUShortValue("configuration","Red"+(section));
			green = GetUShortValue("configuration","Green"+(section));
			blue = GetUShortValue("configuration","Blue"+(section));
			Color col = new Color();
			col.Red = red;
			col.Blue = blue;
			col.Green = green;
			return col;
		}

		private HotKey GetHotKey(int section) {
			HotKey hotkey = new HotKey();
			hotkey.Modifier= (ModifierType)GetIntValue("configuration","Modifier"+(section));
			hotkey.Key = (Gdk.Key)GetIntValue("configuration","Key"+(section));
			return hotkey;
		}

		#endregion

		#region Public methods
		public Sections GetSections() {
			Sections sections = new Sections();
			bool tryNext = true;
			string name;
			SectionsTimeNode tn;
			for (int i=1;tryNext;i++) {
				name = GetName(i);
				if (name != null) {
					tn = new SectionsTimeNode(name, GetStartTime(i), GetStopTime(i), GetHotKey(i), GetColor(i));
					sections.AddSection(tn);
				}
				else tryNext=false;
			}
			return sections;
		}
		#endregion

	}
}
