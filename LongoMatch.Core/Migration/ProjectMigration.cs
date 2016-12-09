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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using VAS.Core.Store;
using VAS.Core.Store.Templates;

namespace LongoMatch.Core.Migration
{
	public static class ProjectMigration
	{
		/// <summary>
		/// Migrate the specified project to the current version format.
		/// </summary>
		/// <param name="project">Project.</param>
		public static void Migrate (LMProject project)
		{
			/* Apply all the migration steps starting from the current version*/
			switch (project.Version) {
			case 0:
				Migrate0 (project);
				break;
			default:
				return;
			}
			Migrate (project);
		}

		#pragma warning disable 0618
		public static void Migrate0 (LMProject project, IDictionary<string, Guid> scoreNameToID = null,
		                             IDictionary<string, Guid> penaltyNameToID = null,
		                             IDictionary<string, Guid> teamNameToID = null,
		                             IDictionary<string, Guid> dashboardNameToID = null)
		{
			Guid id;
			Dashboard dashboard = project.Dashboard;
			Dictionary<Score, ScoreEventType> scoreToEventType;
			Dictionary<PenaltyCard, PenaltyCardEventType> penaltyToEventType;

			if (project.Version != 0) {
				return;
			}

			if (teamNameToID == null) {
				teamNameToID = new Dictionary<string, Guid> ();
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

			TeamMigration.Migrate0 (project.LocalTeamTemplate, teamNameToID);
			TeamMigration.Migrate0 (project.VisitorTeamTemplate, teamNameToID);
			DashboardMigration.Migrate0 (dashboard, dashboardNameToID, scoreNameToID, penaltyNameToID);

			/* Migrate Score and PenaltyCard events */
			scoreToEventType = dashboard.List.OfType<ScoreButton> ().ToDictionary (b => b.Score, b => b.ScoreEventType);
			penaltyToEventType = dashboard.List.OfType<PenaltyCardButton> ().
				ToDictionary (b => b.PenaltyCard, b => b.PenaltyCardEventType);

			// Re-asign the new EventType or create a new one of the button was removed from the dashboard
			foreach (ScoreEvent evt in project.Timeline.OfType<ScoreEvent> ()) {
				if (scoreToEventType.ContainsKey (evt.Score)) {
					evt.EventType = scoreToEventType [evt.Score];
				} else {
					if (!scoreNameToID.TryGetValue (evt.Score.Name, out id)) {
						scoreNameToID [evt.Score.Name] = id = Guid.NewGuid ();
					}
					evt.EventType = new ScoreEventType {
						ID = id,
						Score = evt.Score,
					};
					scoreToEventType.Add (evt.Score, evt.ScoreEventType);
				}
			}
			foreach (PenaltyCardEvent evt in project.Timeline.OfType<PenaltyCardEvent> ()) {
				if (penaltyToEventType.ContainsKey (evt.PenaltyCard)) {
					evt.EventType = penaltyToEventType [evt.PenaltyCard];
				} else {
					if (!penaltyNameToID.TryGetValue (evt.PenaltyCard.Name, out id)) {
						penaltyNameToID [evt.PenaltyCard.Name] = id = Guid.NewGuid ();
					}
					evt.EventType = new PenaltyCardEventType {
						PenaltyCard = evt.PenaltyCard,
						ID = id,
					};
					penaltyToEventType.Add (evt.PenaltyCard, evt.PenaltyCardEventType);
				}
			}

			// Convert old Team tags to Teams
			foreach (LMTimelineEvent evt in project.Timeline.Where (e => (e as LMTimelineEvent).Team != TeamType.NONE)) {
				if (evt.Team == TeamType.LOCAL || evt.Team == TeamType.BOTH) {
					evt.Teams.Add (project.LocalTeamTemplate);
				}
				if (evt.Team == TeamType.VISITOR || evt.Team == TeamType.BOTH) {
					evt.Teams.Add (project.VisitorTeamTemplate);
				}
			}

			foreach (TimelineEvent evt in project.Timeline) {
				evt.FileSet = project.Description.FileSet;
			}

			project.Version = 1;
		}
		#pragma warning restore 0618
	}
}

