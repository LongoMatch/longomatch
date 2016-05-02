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
using System.Linq;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Core.Stats
{
	public class PlayerEventTypeStats
	{
		EventsFilter filter;
		Player player;
		Project project;
		List<TimelineEventLongoMatch> events;

		public PlayerEventTypeStats (Project project, EventsFilter filter, Player player, EventType evType)
		{
			this.filter = filter;
			this.player = player;
			this.project = project;
			this.EventType = evType;
		}

		public int TotalCount {
			get;
			set;
		}

		public EventType EventType {
			get;
			set;
		}

		public void Update ()
		{
			events = filter.VisiblePlays.Where (e => ((TimelineEventLongoMatch)e).Players.Contains (player) &&
			e.EventType.Equals (EventType)).Cast<TimelineEventLongoMatch> ().ToList ();
			
			SubcategoriesStats = new List<SubCategoryStat> ();

			if (EventType is ScoreEventType) {
				ScoreEventType scoreEventType = EventType as ScoreEventType;
				// Total number of points
				TotalCount = events.Count * scoreEventType.Score.Points;
				int eventsCount = events.Count ();
			} else if (EventType is PenaltyCardEventType) {
				TotalCount = events.Count;
			} else {
				AnalysisEventType evType = EventType as AnalysisEventType;
				TotalCount = events.Count;
				
				SubcategoriesStats = new List<SubCategoryStat> ();
				var tagsByGroup = evType.TagsByGroup;
				foreach (string grp in tagsByGroup.Keys) {
					SubCategoryStat substat = new SubCategoryStat (grp);
					foreach (Tag t in tagsByGroup[grp]) {
						int count;
						count = events.Count (e => e.Tags.Contains (t));
						PercentualStat pStat = new PercentualStat (t.Value, count, 0, 0, events.Count);
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

		public List<Coordinates> GetFieldCoordinates (FieldPositionType pos)
		{
			switch (pos) {
			case FieldPositionType.Field:
				return events.Where (e => e.FieldPosition != null).Select (e => e.FieldPosition).ToList ();
			case FieldPositionType.HalfField:
				return events.Where (e => e.HalfFieldPosition != null).Select (e => e.HalfFieldPosition).ToList ();
			default:
				return events.Where (e => e.GoalPosition != null).Select (e => e.GoalPosition).ToList ();
			}
		}
	}
}

