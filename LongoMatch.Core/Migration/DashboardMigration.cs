//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store.Templates;

namespace LongoMatch.Core.Migration
{
	public static class DashboardMigration
	{
		/// <summary>
		/// Migrate the specified dashboard to the current version format.
		/// </summary>
		/// <param name="dashboard">Dashboard.</param>
		public static void Migrate (Dashboard dashboard)
		{
			/* Apply all the migration steps starting from the current version*/
			switch (dashboard.Version) {
			case 0:
				Migrate0 (dashboard);
				break;
			default:
				return;
			}
			Migrate (dashboard);
		}

		#pragma warning disable 0618
		public static void Migrate0 (Dashboard dashboard, IDictionary<string, Guid> dashboardNameToID = null,
		                             IDictionary<string, Guid> scoreNameToID = null,
		                             IDictionary<string, Guid> penaltyNameToID = null)
		{
			Guid id;

			if (dashboard.Version != 0) {
				return;
			}

			if (dashboardNameToID == null) {
				dashboardNameToID = new Dictionary<string, Guid> ();
			}
			if (scoreNameToID == null) {
				scoreNameToID = new Dictionary<string, Guid> ();
			}
			if (penaltyNameToID == null) {
				penaltyNameToID = new Dictionary<string, Guid> ();
			}

			if (dashboard.ID == Guid.Empty) {
				if (!dashboardNameToID.TryGetValue (dashboard.Name, out id)) {
					dashboardNameToID [dashboard.Name] = id = Guid.NewGuid ();
				}
				dashboard.ID = id;
			}

			foreach (ScoreButton button in dashboard.List.OfType<ScoreButton> ()) {
				Score score = button.OldScore;

				if (!scoreNameToID.TryGetValue (score.Name, out id)) {
					scoreNameToID [score.Name] = id = Guid.NewGuid ();
				}

				var toAdd = button.EventType.Clone ();
				toAdd.ID = id;
				button.EventType = toAdd;
				button.ScoreEventType.Score = score;
			}

			foreach (PenaltyCardButton button in dashboard.List.OfType<PenaltyCardButton> ()) {
				PenaltyCard penalty = button.OldPenaltyCard;

				if (!penaltyNameToID.TryGetValue (penalty.Name, out id)) {
					penaltyNameToID [penalty.Name] = id = Guid.NewGuid ();
				}

				var toAdd = button.EventType.Clone ();
				toAdd.ID = id;

				button.EventType = toAdd;
				button.PenaltyCardEventType.PenaltyCard = penalty;
			}
			dashboard.Version = 1;
		}
		#pragma warning restore 0618
	}
}

