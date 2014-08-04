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
	public class Categories: ITemplate<TaggerButton>
	{

		const int CAT_WIDTH = 120;
		const int CAT_HEIGHT = 80;
		
		/// <summary>
		/// Creates a new template
		/// </summary>
		public Categories() {
			FieldBackground = Config.FieldBackground;
			HalfFieldBackground = Config.HalfFieldBackground;
			GoalBackground = Config.GoalBackground;
			ID = Guid.NewGuid ();
			List = new List<TaggerButton>();
		}
		
		public Guid ID {
			get;
			set;
		}
		
		public List<TaggerButton> List {
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
		
		[JsonIgnore]
		public List<Score> Scores {
			get {
				return List.OfType<Score>().ToList();
			}
		}
		
		[JsonIgnore]
		public List<PenaltyCard> PenaltyCards {
			get {
				return List.OfType<PenaltyCard>().ToList();
			}
		}
		
		[JsonIgnore]
		public List<Timer> Timers {
			get {
				return List.OfType<Timer>().ToList();
			}
		}
		
		[JsonIgnore]
		public List<Category> CategoriesList {
			get {
				return List.OfType<Category>().ToList();
			}
		}
		
		[JsonIgnore]
		public List<TagButton> CommonTags {
			get {
				return List.OfType<TagButton>().ToList();
			}
		}

		[JsonIgnore]
		public int CanvasWidth {
			get {
				return (int) List.Max (c => c.Position.X + c.Width);
			}
		}
		
		[JsonIgnore]
		public int CanvasHeight {
			get {
				return (int) List.Max (c => c.Position.Y + c.Height);
			}
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
				/* Leave the first row for the timers and score */
				Position = new Point (10 + (index % 7) * (CAT_WIDTH + 10),
				                      10 + (index / 7 + 1) * (CAT_HEIGHT + 10)),
				Width = CAT_WIDTH,
				Height = CAT_HEIGHT,
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
			Score score;
			Timer timer;
			PenaltyCard card;
			TagButton tag;
			List<string> periods = new List<string>();
			Categories template = new Categories();
			
			template.FillDefaultTemplate(count);
			periods.Add ("1");
			periods.Add ("2");
			template.GamePeriods = periods; 
			tag = new TagButton (new Tag (Catalog.GetString ("Attack"),
			                              Constants.COMMON_TAG));
			tag.Position = new Point (10, 10);
			template.List.Add (tag);
			
			tag = new TagButton (new Tag (Catalog.GetString ("Defense"),
			                              Constants.COMMON_TAG));
			tag.Position = new Point (10 + (10 + CAT_WIDTH) * 1, 10);
			template.List.Add (tag);

			card = new PenaltyCard (Catalog.GetString ("Red"),
			                        Color.Red, CardShape.Rectangle);
			card.Position = new Point (10 + (10 + CAT_WIDTH) * 2, 10);
			template.List.Add (card);

			card = new PenaltyCard (Catalog.GetString ("Yellow"),
			                        Color.Yellow, CardShape.Rectangle);
			card.Position = new Point (10 + (10 + CAT_WIDTH) * 3, 10);
			template.List.Add (card);
			
			score = new Score (Catalog.GetString ("Field goal"), 1);
			score.Position = new Point (10 + (10 + CAT_WIDTH) * 4, 10);
			template.List.Add (score);
			
			score = new Score (Catalog.GetString ("Penalty goal"), 1);
			score.Position = new Point (10 + (10 + CAT_WIDTH) * 5, 10);
			template.List.Add (score);
			
			timer = new Timer {Name = Catalog.GetString ("Ball playing")};
			timer.Position = new Point (10 + (10 + CAT_WIDTH) * 6, 10);
			template.List.Add (timer);
			return template;
		}

		private void FillDefaultTemplate(int count) {
			for(int i=1; i<=count; i++)
				AddDefaultItem(i-1);
		}
	}
}
