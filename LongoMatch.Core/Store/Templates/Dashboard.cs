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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;

using Image = LongoMatch.Core.Common.Image;
using System.Collections.ObjectModel;

namespace LongoMatch.Core.Store.Templates
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
	public class Dashboard: ITemplate
	{

		const int CAT_WIDTH = 120;
		const int CAT_HEIGHT = 80;
		
		/// <summary>
		/// Creates a new template
		/// </summary>
		public Dashboard() {
			FieldBackground = Config.FieldBackground;
			HalfFieldBackground = Config.HalfFieldBackground;
			GoalBackground = Config.GoalBackground;
			ID = Guid.NewGuid ();
			List = new List<DashboardButton>();
		}
		
		public Guid ID {
			get;
			set;
		}
		
		public List<DashboardButton> List {
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
		public List<Timer> Timers {
			get {
				return List.OfType<Timer>().ToList();
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
	
		public void AddDefaultTags (AnalysisEventType ev) {
			ev.Tags.Add (new Tag (Catalog.GetString ("Success"),
			                      Catalog.GetString ("Outcome")));
			ev.Tags.Add (new Tag (Catalog.GetString ("Failure"),
			                      Catalog.GetString ("Outcome")));
		}	
		
		public AnalysisEventButton AddDefaultItem (int index) {
			AnalysisEventButton button;
			AnalysisEventType evtype;
			Color c = Color.Red;
			HotKey h = new HotKey();
			
			evtype = new AnalysisEventType {
				Name = "Event Type " + index,
				SortMethod = SortMethodType.SortByStartTime,
				Color = c
			};
			AddDefaultTags(evtype);

			button = new  AnalysisEventButton {
				EventType = evtype,
				Start = new Time{Seconds = 10},
				Stop = new Time {Seconds = 10},
				HotKey = h,
				/* Leave the first row for the timers and score */
				Position = new Point (10 + (index % 7) * (CAT_WIDTH + 10),
				                      10 + (index / 7 + 1) * (CAT_HEIGHT + 10)),
				Width = CAT_WIDTH,
				Height = CAT_HEIGHT,
			};
			List.Insert(index, button);
			return button;
		}

		public static Dashboard Load(string filePath) {
			Dashboard cat = Serializer.LoadSafe<Dashboard>(filePath);
			if (cat.GamePeriods == null) {
				cat.GamePeriods = new List<string>();
				cat.GamePeriods.Add ("1");
				cat.GamePeriods.Add ("2");
			}
			return cat;
		}

		public static Dashboard DefaultTemplate(int count) {
			TagButton tagbutton;
			TimerButton timerButton;
			PenaltyCardButton cardButton;
			ScoreButton scoreButton;
			List<string> periods = new List<string>();
			Dashboard template = new Dashboard();
			
			template.FillDefaultTemplate(count);
			periods.Add ("1");
			periods.Add ("2");
			template.GamePeriods = periods; 

			tagbutton = new TagButton {
				Tag = new Tag (Catalog.GetString ("Attack"), Constants.COMMON_TAG),
				Position = new Point (10, 10)};
			template.List.Add (tagbutton);
			
			tagbutton = new TagButton {
				Tag = new Tag (Catalog.GetString ("Deffense"), Constants.COMMON_TAG),
				Position = new Point (10 + (10 + CAT_WIDTH) * 1, 10)};
			template.List.Add (tagbutton);

			cardButton = new PenaltyCardButton {
				PenaltyCard = new PenaltyCard (Catalog.GetString ("Red"),
				                               Color.Red, CardShape.Rectangle),
				Position = new Point (10 + (10 + CAT_WIDTH) * 2, 10)};
			template.List.Add (cardButton);

			cardButton = new PenaltyCardButton {
				PenaltyCard = new PenaltyCard (Catalog.GetString ("Yellow"),
				                               Color.Yellow, CardShape.Rectangle),
				Position = new Point (10 + (10 + CAT_WIDTH) * 3, 10)};
			template.List.Add (cardButton);
			
			scoreButton = new ScoreButton {
				Score = new Score (Catalog.GetString ("Field goal"), 1),
				Position = new Point (10 + (10 + CAT_WIDTH) * 4, 10)};
			template.List.Add (scoreButton);
			
			scoreButton = new ScoreButton {
				Score = new Score (Catalog.GetString ("Penalty goal"), 1),
				Position = new Point (10 + (10 + CAT_WIDTH) * 5, 10)};
			template.List.Add (scoreButton);
			
			timerButton = new TimerButton {
				Timer = new Timer {Name = Catalog.GetString ("Ball playing")},
				Position = new Point (10 + (10 + CAT_WIDTH) * 6, 10)};
			template.List.Add (timerButton);
			return template;
		}

		private void FillDefaultTemplate(int count) {
			for(int i=1; i<=count; i++)
				AddDefaultItem(i-1);
		}
	}
}
