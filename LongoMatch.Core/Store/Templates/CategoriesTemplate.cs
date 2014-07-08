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
using Newtonsoft.Json;

using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.Interfaces;

using Image = LongoMatch.Common.Image;

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
	public class Categories: ITemplate, ITemplate<Category>
	{

		/// <summary>
		/// Creates a new template
		/// </summary>
		public Categories() {
			FieldBackground = Config.FieldBackground;
			HalfFieldBackground = Config.HalfFieldBackground;
			GoalBackground = Config.GoalBackground;
			ID = Guid.NewGuid ();
			List = new List<Category>();
			Scores = new List<Score> ();
			PenaltyCards = new List<PenaltyCard> ();
			CommonTags = new List<Tag>();
		}
		
		public Guid ID {
			get;
			set;
		}
		
		public List<Category> List {
			get;
			set;
		}
		
		public List<Tag> CommonTags {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}
		
		public List<string> GamePeriods {
			get;
			set;
		}
		
		public List<Score> Scores {
			get;
			set;
		}
		
		public List<PenaltyCard> PenaltyCards {
			get;
			set;
		}
		
		public Image Image {
			get;
			set;
		}
		
		public Image FieldBackground {
			get;
			set;
		}
		
		public Image HalfFieldBackground {
			get;
			set;
		}
		
		public Image GoalBackground {
			get;
			set;
		}
		
		public void Save(string filePath) {
			Serializer.Save(this, filePath);
		}
	
		public void AddDefaultTags (Category cat) {
			cat.Tags.Add (new Tag (Catalog.GetString ("Good")));
			cat.Tags.Add (new Tag (Catalog.GetString ("Bad")));
		}	
		
		public Category AddDefaultItem (int index) {
			Color c = Color.Red;
			HotKey h = new HotKey();
			
			Category cat =  new Category {
				Name = "Category " + index,
				Color = c,
				Start = new Time{Seconds = 10},
				Stop = new Time {Seconds = 10},
				SortMethod = SortMethodType.SortByStartTime,
				HotKey = h,
				Position = index-1,
			};
			AddDefaultTags(cat);
			List.Insert(index, cat);
			return cat;
		}

		public static Categories Load(string filePath) {
			Categories cat = Serializer.LoadSafe<Categories>(filePath);
			if (cat.GamePeriods == null) {
				cat.GamePeriods = new List<string>();
				cat.GamePeriods.Add ("1");
				cat.GamePeriods.Add ("2");
			}
			return cat;
		}

		public static Categories DefaultTemplate(int count) {
			List<string> periods = new List<string>();
			Categories template = new Categories();
			
			template.FillDefaultTemplate(count);
			periods.Add ("1");
			periods.Add ("2");
			template.GamePeriods = periods; 
			template.CommonTags.Add (new Tag (Catalog.GetString ("Attack"),
			                                  Constants.COMMON_TAG));
			template.CommonTags.Add (new Tag (Catalog.GetString ("Defense"),
			                                  Constants.COMMON_TAG));
			template.PenaltyCards.Add (new PenaltyCard (Catalog.GetString ("Red"),
			                                            Color.Red, CardShape.Rectangle));
			template.PenaltyCards.Add (new PenaltyCard (Catalog.GetString ("Yellow"),
			                                            Color.Yellow, CardShape.Rectangle));
			return template;
		}

		private void FillDefaultTemplate(int count) {
			for(int i=1; i<=count; i++)
				AddDefaultItem(i-1);
		}
	}
}
