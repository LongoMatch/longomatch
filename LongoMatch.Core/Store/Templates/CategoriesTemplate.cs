// Sections.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Store.Templates
{

	/// <summary>
	/// I am a template for the analysis categories used in a project.
	/// I describe each one of the categories and provide the default values
	/// to use to create plys in a specific category.
	/// The position of the category in the index is very important and should
	/// respect the same index used in the plays list inside a project.
	/// The <see cref="LongoMatch.DB.Project"/> must handle all the changes
	/// </summary>
	[Serializable]
	public class Categories: List<Category>, ITemplate, ITemplate<Category>
	{
		/* Database additions */
		GameUnitsList gameUnits;

		/// <summary>
		/// Creates a new template
		/// </summary>
		public Categories() {
		}

		public string Name {
			get;
			set;
		}
		
		public GameUnitsList GameUnits {
			set {
				gameUnits = value;
			}
			get {
				if (gameUnits == null) {
					gameUnits = new GameUnitsList();
				}
				return gameUnits;
			}
		}
		
		public void Save(string filePath) {
			SerializableObject.Save(this, filePath);
		}
		
		public void AddDefaultItem (int index) {
			PlayerSubCategory localplayers, visitorplayers;
			TagSubCategory period;
			TeamSubCategory team;
			Color c = Color.FromArgb(255, 0, 0);
			HotKey h = new HotKey();
			
			localplayers = new PlayerSubCategory {
				Name = Catalog.GetString("Local Team Players"),
				AllowMultiple = true,
				FastTag = true};
			localplayers.Add(Team.LOCAL);
			
			visitorplayers = new PlayerSubCategory {
				Name = Catalog.GetString("Visitor Team Players"),
				AllowMultiple = true,
				FastTag = true};
			visitorplayers.Add(Team.VISITOR);	
			
			period = new TagSubCategory {
				Name = Catalog.GetString("Period"),
				AllowMultiple = false,
				FastTag = true,
			};
			period.Add("1");
			period.Add("2");
			
			Category cat =  new Category {
				Name = "Category " + index,
				Color = c,
				Start = new Time{Seconds = 10},
				Stop = new Time {Seconds = 10},
				SortMethod = SortMethodType.SortByStartTime,
				HotKey = h,
				Position = index-1,
			};
			cat.SubCategories.Add(localplayers);
			cat.SubCategories.Add(visitorplayers);
			cat.SubCategories.Add(period);
			Insert(index, cat);
		}

		public static Categories Load(string filePath) {
			return SerializableObject.Load<Categories>(filePath);
		}

		public static Categories DefaultTemplate(int count) {
			Categories defaultTemplate = new Categories();
			defaultTemplate.FillDefaultTemplate(count);
			return defaultTemplate;
		}

		private void FillDefaultTemplate(int count) {
			for(int i=1; i<=count; i++)
				AddDefaultItem(i-1);
		}
	}
}
