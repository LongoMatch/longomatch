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
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using VAS.Core.Store;

namespace LongoMatch.Core.Filters
{
	public class EventsFilter
	{
		public event FilterUpdatedHandler FilterUpdated;

		Dictionary<EventType, List<Tag>> eventsFilter;
		List<Tag> tagsFilter;
		List<PlayerLongoMatch> playersFilter;
		List<Period> periodsFilter;
		List<Timer> timersFilter;
		ProjectLongoMatch project;

		public EventsFilter (ProjectLongoMatch project)
		{
			this.project = project;
			eventsFilter = new Dictionary<EventType, List<Tag>> ();
			playersFilter = new List<PlayerLongoMatch> (); 
			periodsFilter = new List<Period> ();
			tagsFilter = new List<Tag> ();
			timersFilter = new List<Timer> ();
			ClearAll ();
			UpdateFilters ();
		}

		public bool Silent {
			set;
			get;
		}

		public bool IgnoreUpdates {
			set;
			get;
		}

		public List<EventType> VisibleEventTypes {
			get;
			protected set;
		}

		public List<PlayerLongoMatch> VisiblePlayers {
			get;
			protected set;
		}

		public List<TimelineEventLongoMatch> VisiblePlays {
			get;
			protected set;
		}

		public void ClearAll (bool update = true)
		{
			eventsFilter.Clear ();
			playersFilter.Clear ();
			periodsFilter.Clear ();
			timersFilter.Clear ();
			tagsFilter.Clear ();
			if (update)
				Update ();
		}

		public void FilterPlayer (PlayerLongoMatch player, bool visible)
		{
			if (visible) {
				if (!playersFilter.Contains (player))
					playersFilter.Add (player);
			} else {
				if (playersFilter.Contains (player))
					playersFilter.Remove (player);
			}
			Update ();
		}

		public void FilterEventType (EventType evType, bool visible)
		{
			if (visible) {
				if (!eventsFilter.ContainsKey (evType))
					eventsFilter [evType] = new List<Tag> ();
			} else {
				if (eventsFilter.ContainsKey (evType))
					eventsFilter.Remove (evType);
			}
			Update ();
		}

		public void FilterPeriod (Period period, bool visible)
		{
			if (visible) {
				if (!periodsFilter.Contains (period))
					periodsFilter.Add (period);
			} else {
				if (periodsFilter.Contains (period))
					periodsFilter.Remove (period);
			}
			Update ();
		}

		public void FilterTag (Tag tag, bool visible)
		{
			if (visible) {
				if (!tagsFilter.Contains (tag))
					tagsFilter.Add (tag);
			} else {
				if (tagsFilter.Contains (tag))
					tagsFilter.Remove (tag);
			}
		}

		public void FilterTimer (Timer timer, bool visible)
		{
			if (visible) {
				if (!timersFilter.Contains (timer))
					timersFilter.Add (timer);
			} else {
				if (timersFilter.Contains (timer))
					timersFilter.Remove (timer);
			}
			Update ();
		}

		public void FilterEventTag (EventType evType, Tag tag, bool visible)
		{
			List<Tag> tags;

			if (visible) {
				FilterEventType (evType, true);
				tags = eventsFilter [evType];
				if (!tags.Contains (tag))
					tags.Add (tag);
			} else {
				if (eventsFilter.ContainsKey (evType)) {
					tags = eventsFilter [evType];
					if (tags.Contains (tag))
						tags.Remove (tag);
				}
			}
			Update ();
		}

		public bool IsVisible (object o)
		{
			if (o is Player) {
				return VisiblePlayers.Contains (o as Player);
			} else if (o is TimelineEvent) {
				return VisiblePlays.Contains (o as TimelineEvent);
			}
			return true;
		}

		public void Update ()
		{
			if (!IgnoreUpdates) {
				UpdateFilters ();
				EmitFilterUpdated ();
			}
		}

		void UpdateFilters ()
		{
			UpdateVisiblePlayers ();
			UpdateVisibleCategories ();
			UpdateVisiblePlays ();
		}

		void UpdateVisiblePlayers ()
		{
			if (playersFilter.Count == 0) {
				VisiblePlayers = project.LocalTeamTemplate.PlayingPlayersList.Concat (
					project.VisitorTeamTemplate.PlayingPlayersList).ToList ();
			} else {
				VisiblePlayers = playersFilter.ToList ();
			}
		}

		void UpdateVisibleCategories ()
		{
			if (eventsFilter.Count == 0) {
				VisibleEventTypes = project.EventTypes.ToList ();
			} else {
				VisibleEventTypes = eventsFilter.Keys.ToList ();
			}
		}

		void UpdateVisiblePlays ()
		{
			bool cat_match = true, tag_match = true, player_match = true;
			bool period_match = true, timer_match = true;

			VisiblePlays = new List<TimelineEventLongoMatch> ();
				
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

				if (play.Players.Count == 0 && VisiblePlayers.Count ==
				    project.LocalTeamTemplate.PlayingPlayersList.Count +
				    project.VisitorTeamTemplate.PlayingPlayersList.Count) {
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

		void EmitFilterUpdated ()
		{
			if (!Silent && FilterUpdated != null)
				FilterUpdated ();
		}
	}
}

