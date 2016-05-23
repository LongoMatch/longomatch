//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Core.Stats
{
	public class TeamStats
	{
		ProjectLongoMatch project;
		SportsTeam template;
		TeamType team;
		EventsFilter filter;

		public TeamStats (ProjectLongoMatch project, EventsFilter filter, TeamType team)
		{
			this.project = project;
			this.filter = filter;
			this.team = team;
			if (team == TeamType.LOCAL) {
				this.template = project.LocalTeamTemplate;
			} else {
				this.template = project.VisitorTeamTemplate;
			}
			PlayersStats = new List<PlayerStats> ();
			foreach (PlayerLongoMatch p in this.template.List) {
				PlayersStats.Add (new PlayerStats (project, filter, p));
			}
		}

		public List<PlayerStats> PlayersStats {
			get;
			set;
		}

		public void Update ()
		{
			foreach (PlayerStats stats in PlayersStats) {
				stats.Update ();
			}
		}
	}
}

