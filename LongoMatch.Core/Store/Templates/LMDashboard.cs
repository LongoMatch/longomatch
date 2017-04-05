//
//  Copyright (C) 2016 Fluendo S.A.
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using System.Collections.ObjectModel;
using System.Linq;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace LongoMatch.Core.Store.Templates
{
	[Serializable]
	public class LMDashboard : Dashboard, ITemplate<DashboardButton>
	{
		new const int MIN_WIDTH = 320;
		new const int MIN_HEIGHT = 240;

		/// <summary>
		/// Creates a new dashboard with a default set of buttons
		/// </summary>
		/// <returns>the new dashboadrd.</returns>
		/// <param name="count">Number of <see cref="AnalysisEventButton"/> to add.</param>
		public static LMDashboard DefaultTemplate (int count)
		{
			TagButton tagbutton;
			TimerButton timerButton;
			PenaltyCardButton cardButton;
			ScoreButton scoreButton;
			LMDashboard template = new LMDashboard ();

			template.FillDefaultTemplate (count);
			template.GamePeriods = new ObservableCollection<string> { "1", "2" };

			tagbutton = new TagButton {
				Tag = new Tag (Catalog.GetString ("Attack"), ""),
				Position = new Point (10, 10)
			};
			template.List.Add (tagbutton);

			tagbutton = new TagButton {
				Tag = new Tag (Catalog.GetString ("Defense"), ""),
				Position = new Point (10 + (10 + CAT_WIDTH) * 1, 10)
			};
			template.List.Add (tagbutton);

			cardButton = new PenaltyCardButton {
				PenaltyCard = new PenaltyCard (Catalog.GetString ("Red card"),
					Color.Red, CardShape.Rectangle),
				Position = new Point (10 + (10 + CAT_WIDTH) * 2, 10)
			};
			template.List.Add (cardButton);

			cardButton = new PenaltyCardButton {
				PenaltyCard = new PenaltyCard (Catalog.GetString ("Yellow card"),
					Color.Yellow, CardShape.Rectangle),
				Position = new Point (10 + (10 + CAT_WIDTH) * 3, 10)
			};
			template.List.Add (cardButton);

			scoreButton = new ScoreButton {
				Position = new Point (10 + (10 + CAT_WIDTH) * 4, 10),
				BackgroundColor = StyleConf.ButtonScoreColor,
				Score = new Score (Catalog.GetString ("Free play goal"), 1),
			};
			template.List.Add (scoreButton);

			scoreButton = new ScoreButton {
				BackgroundColor = StyleConf.ButtonScoreColor,
				Position = new Point (10 + (10 + CAT_WIDTH) * 5, 10),
				Score = new Score (Catalog.GetString ("Penalty goal"), 1),
			};
			template.List.Add (scoreButton);

			timerButton = new TimerButton {
				Timer = new LMTimer { Name = Catalog.GetString ("Ball playing") },
				Position = new Point (10 + (10 + CAT_WIDTH) * 6, 10)
			};
			template.List.Add (timerButton);
			return template;
		}

		/// <summary>
		/// Create a new <see cref="AnalysisEventButton"/> with the default values
		/// </summary>
		/// <returns>A new button.</returns>
		/// <param name="index">Index of this button used to name it</param>
		public override AnalysisEventButton CreateDefaultItem (int index)
		{
			AnalysisEventButton button;
			AnalysisEventType evtype;
			Color c = StyleConf.ButtonEventColor;
			HotKey h = new HotKey ();

			evtype = new AnalysisEventType {
				Name = "Event Type " + index,
				SortMethod = SortMethodType.SortByStartTime,
				Color = c
			};
			AddDefaultTags (evtype);

			button = new AnalysisEventButton {
				EventType = evtype,
				Start = new Time { TotalSeconds = 10 },
				Stop = new Time { TotalSeconds = 10 },
				HotKey = h,
				/* Leave the first row for the timers and score */
				Position = new Point (10 + (index % 7) * (CAT_WIDTH + 10),
					10 + (index / 7 + 1) * (CAT_HEIGHT + 10)),
				Width = CAT_WIDTH,
				Height = CAT_HEIGHT,
				ShowIcon = true,
			};
			return button;
		}
	}
}

