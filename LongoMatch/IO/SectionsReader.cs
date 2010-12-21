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
using Gdk;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.IO
{


	public class CategoriesReader : XMLReader
	{

		#region Constructors

		public CategoriesReader(string filePath) : base(filePath)
		{
		}
		#endregion

		#region Private methods
		private string GetName(int section) {
			return this.GetStringValue("configuration","Name"+(section));
		}

		private Time GetStartTime(int section) {
			return new Time {Seconds = GetIntValue("configuration","Start"+(section))};
		}

		private Time GetStopTime(int section) {
			return new Time {Seconds = GetIntValue("configuration","Stop"+(section))};
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
		
		private String GetSortMethod(int section) {
			return GetStringValue("configuration","SortMethod"+(section));
		}

		#endregion

		#region Public methods
		public Categories GetCategories() {
			Categories categories = new Categories();
			bool tryNext = true;
			string name;
			Category cat;
			for (int i=1;tryNext;i++) {
				name = GetName(i);
				if (name != null) {
					cat = new Category{
					                  Name = name,
					                  Start = GetStartTime(i),
					                  Stop = GetStopTime(i),
					                  HotKey = GetHotKey(i), 
					                  Color  = GetColor(i)};
					cat.SortMethodString = GetSortMethod(i);
					categories.AddCategory(cat);
				}
				else tryNext=false;
			}
			return categories;
		}
		#endregion

	}
}
