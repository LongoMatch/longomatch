// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using VAS.Core.Store;
using VASFilters = VAS.Core.Filters;

namespace LongoMatch.Core.Filters
{
	public class EventsFilter : VASFilters.EventsFilter
	{
		new ProjectLongoMatch project;

		public EventsFilter (ProjectLongoMatch project) : base (project)
		{
			this.project = project;
		}

		protected override void UpdateVisiblePlayers ()
		{
			if (playersFilter.Count == 0) {
				VisiblePlayers = project.LocalTeamTemplate.PlayingPlayersList.Union (
					project.VisitorTeamTemplate.PlayingPlayersList).Cast<Player> ().ToList ();
			} else {
				VisiblePlayers = playersFilter.ToList ();
			}
		}

		protected override void UpdateVisiblePlays ()
		{
			bool cat_match = true, tag_match = true, player_match = true;
			bool period_match = true, timer_match = true;

			VisiblePlays = new List<TimelineEvent> ();
				
			foreach (TimelineEventLongoMatch play in project.Timeline) {
				cat_match = false;
				if (VisibleEventTypes.Contains (play.EventType)) {
					cat_match = true;
					if (eventsFilter.ContainsKey (play.EventType)) {
						List<Tag> tags = eventsFilter [play.EventType];
						if (tags.Count == 0 || tags.Intersect (play.Tags).Any ()) {
							cat_match = true;
						} else {
							cat_match = false;
						}
					}
				}

				if (tagsFilter.Count > 0) {
					if (play.Tags.Count > 0 && play.Tags [0].Value == "Layup") {
						Console.WriteLine (tagsFilter.Intersect (play.Tags).Count ());
					}
					if (!tagsFilter.Intersect (play.Tags).Any ()) {
						tag_match = false;
					} else {
						tag_match = true;
					}
				}
					
				if (play.Players.Count == 0 &&
				    VisiblePlayers.Count == project.LocalTeamTemplate.PlayingPlayersList.Union (
					    project.VisitorTeamTemplate.PlayingPlayersList).Count<Player> ()) {
					player_match = true;

				} else {
					player_match = VisiblePlayers.Intersect (play.Players).Any ();
				}

				if (timersFilter.Count != 0) {
					timer_match = false;
				}
				foreach (Timer t in timersFilter) {
					foreach (TimeNode tn in t.Nodes) {
						if (tn.Join (play) != null) {
							timer_match = true;
						}
					}
				}

				if (periodsFilter.Count != 0) {
					period_match = false;
				}
				foreach (Period p in periodsFilter) {
					if (p.PeriodNode.Join (play) != null) {
						period_match = true;
					}
				}

				if (player_match && cat_match && tag_match && period_match && timer_match) {
					VisiblePlays.Add (play);
				}
			}
		}


	}
}

