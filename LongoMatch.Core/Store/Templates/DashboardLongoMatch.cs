//
//  Copyright (C) 2016 dfernandez
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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.Interfaces;
using System.Linq;

namespace LongoMatch.Core.Store.Templates
{
	[Serializable]
	public class DashboardLongoMatch : Dashboard, ITemplate<DashboardLongoMatch>
	{
		/// <summary>
		/// Creates a new dashboard with a default set of buttons
		/// </summary>
		/// <returns>the new dashboadrd.</returns>
		/// <param name="count">Number of <see cref="AnalysisEventButton"/> to add.</param>
		public static DashboardLongoMatch DefaultTemplate (int count)
		{
			TagButton tagbutton;
			TimerButtonLongoMatch timerButton;
			PenaltyCardButton cardButton;
			ScoreButton scoreButton;
			DashboardLongoMatch template = new DashboardLongoMatch ();

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

			timerButton = new TimerButtonLongoMatch {
				Timer = new TimerLongoMatch { Name = Catalog.GetString ("Ball playing") },
				Position = new Point (10 + (10 + CAT_WIDTH) * 6, 10)
			};
			template.List.Add (timerButton);
			return template;
		}


		/// <summary>
		/// Creates a deep copy of this dashboard
		/// </summary>
		DashboardLongoMatch ITemplate<DashboardLongoMatch>.Copy (string newName)
		{
			Load ();
			DashboardLongoMatch newDashboard = this.Clone ();
			newDashboard.ID = Guid.NewGuid ();
			newDashboard.DocumentID = null;
			newDashboard.Name = newName;
			foreach (AnalysisEventButton evtButton in newDashboard.List.OfType<AnalysisEventButton> ()) {
				evtButton.EventType.ID = Guid.NewGuid ();
			}
			return newDashboard;
		}
	}
}

