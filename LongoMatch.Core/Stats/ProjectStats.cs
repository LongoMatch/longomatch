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
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using VAS.Core.Store;

namespace LongoMatch.Core.Stats
{
	public class ProjectStats: IDisposable
	{
		EventsFilter filter;

		public ProjectStats (LMProject project)
		{
			Project = project;
			filter = new EventsFilter (project);
			CreateStats ();
		}

		public void Dispose ()
		{
		}

		public LMProject Project {
			get;
			protected set;
		}

		public List<EventTypeStats> EventTypeStats {
			get;
			protected set;
		}

		public List<TimerStats> TimersStats {
			get;
			protected set;
		}

		public TeamStats HomeTeamStats {
			get;
			set;
		}

		public TeamStats AwayTeamStats {
			get;
			set;
		}

		public EventsFilter Filter {
			set {
				filter = value;
				UpdateStats ();
			}
			get {
				return filter;
			}
		}

		public PlayerStats GetPlayerStats (Player p)
		{
			LMProject LMProject = Project as LMProject;
			if (LMProject.LocalTeamTemplate.List.Contains (p)) {
				return HomeTeamStats.PlayersStats.FirstOrDefault (ps => ps.Player == p);
			} else {
				return AwayTeamStats.PlayersStats.FirstOrDefault (ps => ps.Player == p);
			}
		}

		public void CreateStats ()
		{
			EventTypeStats = new List <EventTypeStats> ();
			
			foreach (EventType evt in Project.EventTypes) {
				if (evt is AnalysisEventType) {
					EventTypeStats evstats = new EventTypeStats (Project, filter, evt);
					EventTypeStats.Add (evstats);
				}
			}

			TimersStats = new List<TimerStats> ();
			foreach (LMTimer t in Project.Timers) {
				TimersStats.Add (new TimerStats (Project, t));
			}

			HomeTeamStats = new TeamStats (Project, filter, TeamType.LOCAL);
			AwayTeamStats = new TeamStats (Project, filter, TeamType.VISITOR);
			UpdateStats ();
		}

		public void UpdateStats ()
		{
			foreach (EventTypeStats e in EventTypeStats) {
				e.Update ();
			}
			foreach (TimerStats st in TimersStats) {
				st.Update ();
			}
			HomeTeamStats.Update ();
			AwayTeamStats.Update ();
		}
	}
}

