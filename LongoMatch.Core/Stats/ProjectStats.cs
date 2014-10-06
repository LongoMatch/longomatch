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
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Core.Stats
{
	public class ProjectStats: IDisposable
	{
		List<EventTypeStats> catStats;
		EventsFilter filter;
		Project project;
		
		public ProjectStats (Project project)
		{
			catStats = new List<EventTypeStats>();
			this.project = project;
			filter = new EventsFilter (project);
			CreateStats ();
		}
		
		public void Dispose ()
		{
		}
		
		public Project Project {
			get;
			protected set;
		}

		public List<EventTypeStats> EventTypeStats {
			get;
			protected set;
		}
		
		public EventsFilter Filter {
			set {
				filter = value;
				UpdateStats ();
			}
		}
		
		public void CreateStats () {
			EventTypeStats = new List <EventTypeStats> ();
			
			foreach (EventType evt in project.EventTypes) {
				EventTypeStats evstats = new EventTypeStats (project, filter, evt);
				evstats.Update ();
				EventTypeStats.Add (evstats);
			}
		}

		public void UpdateStats ()
		{
			foreach (EventTypeStats e in EventTypeStats) {
				e.Update();
			}
		}
	}
}

