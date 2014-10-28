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
using System.Linq;
using System.Collections.Generic;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Stats
{
	public class EventTypeStats: Stat
	{
		List<TimelineEvent> events, homeEvents, awayEvents;
		EventType eventType;
		Project project;
		EventsFilter filter;

		public EventTypeStats (Project project, EventsFilter filter, EventType evType)
		{
			Name = evType.Name;
			events = new List<TimelineEvent> ();
			this.project = project;
			this.filter = filter;
			this.eventType = evType;
		}

		public void Update ()
		{
			events = project.EventsByType (eventType).Where (filter.IsVisible).ToList ();
			homeEvents = events.Where (e => project.PlayTaggedTeam (e) == Team.LOCAL).ToList ();
			awayEvents = events.Where (e => project.PlayTaggedTeam (e) == Team.VISITOR).ToList ();
			TotalCount = events.Count;
			LocalTeamCount = homeEvents.Count;
			VisitorTeamCount = awayEvents.Count;
			SubcategoriesStats = new List<SubCategoryStat> ();
			if (eventType is AnalysisEventType) {
				var tagsByGroup = (eventType as AnalysisEventType).TagsByGroup;
				foreach (string grp in tagsByGroup.Keys) {
					SubCategoryStat substat = new SubCategoryStat (grp);
					foreach (Tag t in tagsByGroup[grp]) {
						int count, localTeamCount, visitorTeamCount;
						count = events.Count (e => e.Tags.Contains (t));
						localTeamCount = homeEvents.Count (e => e.Tags.Contains (t));
						visitorTeamCount = awayEvents.Count (e => e.Tags.Contains (t));
						PercentualStat pStat = new PercentualStat (t.Value, count, localTeamCount,
						                                           visitorTeamCount, events.Count);
						substat.OptionStats.Add (pStat);
					}
					SubcategoriesStats.Add (substat);
				}
			}
		}

		public List<SubCategoryStat> SubcategoriesStats {
			get;
			protected set;
		}

		public EventType Category {
			get {
				return eventType;
			}
		}

		public bool HasPositionTags (Team team)
		{
			List<TimelineEvent> evts = EventsForTeam (team);
			return evts.Count (e => e.FieldPosition != null || e.HalfFieldPosition != null || e.GoalPosition != null) != 0; 
		}

		public List<Coordinates> GetFieldCoordinates (Team team, FieldPositionType pos)
		{
			List<TimelineEvent> evts = EventsForTeam (team);
			
			switch (pos) {
			case FieldPositionType.Field:
				return evts.Where (e => e.FieldPosition != null).Select (e => e.FieldPosition).ToList ();
			case FieldPositionType.HalfField:
				return evts.Where (e => e.HalfFieldPosition != null).Select (e => e.HalfFieldPosition).ToList ();
			default:
				return evts.Where (e => e.GoalPosition != null).Select (e => e.GoalPosition).ToList ();
			}
		}

		List<TimelineEvent> EventsForTeam (Team team)
		{
			List<TimelineEvent> evts;
			
			switch (team) {
			case Team.LOCAL:
				evts = homeEvents;
				break;
			case Team.VISITOR:
				evts = awayEvents;
				break;
			default:
				evts = events;
				break;
			}
			return evts;
		}
	}
}

