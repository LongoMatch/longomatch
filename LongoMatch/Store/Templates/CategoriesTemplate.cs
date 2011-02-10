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
using System.Linq;
using Gdk;
using Mono.Unix;
using LongoMatch.Common;

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
	public class Categories: List<Category>
	{

		/// <summary>
		/// Creates a new template
		/// </summary>
		public Categories() {}

		public void Save(string filePath) {
			SerializableObject.Save(this, filePath);
		}

		public static Categories Load(string filePath) {
			return SerializableObject.Load<Categories>(filePath);
		}

		public static Categories DefaultTemplate() {
			Categories defaultTemplate = new Categories();
			defaultTemplate.FillDefaultTemplate();
			return defaultTemplate;
		}

		private void FillDefaultTemplate() {
			Color c = new Color((Byte)255, (Byte)0, (Byte)0);
			HotKey h = new HotKey();


			for(int i=1; i<=20; i++) {
				PlayerSubCategory localplayers, visitorplayers;
				TeamSubCategory team;
				List<Team> teams, lplayers, vplayers;

				teams = new List<Team>();
				teams.Add(Team.NONE);
				teams.Add(Team.LOCAL);
				teams.Add(Team.NONE);
				team = new TeamSubCategory {
					Name = Catalog.GetString("Team"),
					Options = teams
				};

				lplayers = new List<Team>();
				lplayers.Add(Team.LOCAL);
				localplayers = new PlayerSubCategory {
					Name = Catalog.GetString("Local Team Players"),
					Options = lplayers,
				};

				vplayers = new List<Team>();
				vplayers.Add(Team.VISITOR);
				visitorplayers = new PlayerSubCategory {
					Name = Catalog.GetString("Visitor Team Players"),
					Options = vplayers,
				};

				Category cat =  new Category {
					Name = "Category " + i,
					Color = c,
					Start = new Time{Seconds = 10},
					Stop = new Time {Seconds = 10},
					SortMethod = SortMethodType.SortByStartTime,
					HotKey = h,
					Position = i-1,
				};
				cat.SubCategories.Add(team);
				cat.SubCategories.Add(localplayers);
				cat.SubCategories.Add(visitorplayers);
				Add(cat);
			}
		}
	}
}
