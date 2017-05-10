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

		public EventsFilter (LMProject project) : base (project)
		{
		}

		LMProject Project {
			get {
				return project as LMProject;
			}
		}

		protected override void UpdateVisiblePlayers ()
		{
			if (playersFilter.Count == 0) {
				VisiblePlayers = Project.LocalTeamTemplate.CalledPlayersList.Union (
					Project.VisitorTeamTemplate.CalledPlayersList).Cast<Player> ().ToList ();
			} else {
				VisiblePlayers = playersFilter.ToList ();
			}
		}

		protected override bool IsVisibleByPlayer (TimelineEvent play)
		{
			if (play.Players.Count == 0 &&
			    VisiblePlayers.Count == Project.LocalTeamTemplate.CalledPlayersList.Union (
				    Project.VisitorTeamTemplate.CalledPlayersList).Count<Player> ()) {
				return true;

			} else {
				return VisiblePlayers.Intersect (play.Players).Any ();
			}
		}

		protected override bool IsVisibleByPeriod (TimelineEvent play)
		{
			if (periodsFilter.Count == 0)
				return true;

			bool period_match = false;
			foreach (Period p in periodsFilter) {
				if (p.PeriodNode.Join (play) != null) {
					period_match = true;
				}
			}
			return period_match;
		}


	}
}

