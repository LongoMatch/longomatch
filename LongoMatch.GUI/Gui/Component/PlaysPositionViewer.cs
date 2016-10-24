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
using LongoMatch.Gui.Menus;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Store;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlaysPositionViewer : Gtk.Bin
	{
		SportsPlaysMenu menu;
		ProjectLongoMatch project;

		public PlaysPositionViewer ()
		{
			this.Build ();
			field.Tagger.FieldPosition = FieldPositionType.Field;
			hfield.Tagger.FieldPosition = FieldPositionType.HalfField;
			goal.Tagger.FieldPosition = FieldPositionType.Goal;
			field.Tagger.ShowMenuEvent += HandleShowMenuEvent;
			hfield.Tagger.ShowMenuEvent += HandleShowMenuEvent;
			goal.Tagger.ShowMenuEvent += HandleShowMenuEvent;
			App.Current.EventsBroker.Subscribe<EventLoadedEvent> (HandlePlayLoaded);
			menu = new SportsPlaysMenu ();
		}

		public void LoadProject (ProjectLongoMatch project, EventsFilter filter)
		{
			this.project = project;
			if (project != null) {
				var timeLine = project.Timeline.OfType<TimelineEventLongoMatch> ();

				field.Tagger.Project = project;
				hfield.Tagger.Project = project;
				goal.Tagger.Project = project;
				field.Tagger.Background = project.GetBackground (FieldPositionType.Field);
				hfield.Tagger.Background = project.GetBackground (FieldPositionType.HalfField);
				goal.Tagger.Background = project.GetBackground (FieldPositionType.Goal);
				field.Tagger.Plays = timeLine;
				hfield.Tagger.Plays = timeLine;
				goal.Tagger.Plays = timeLine;
				field.Tagger.Filter = filter;
				hfield.Tagger.Filter = filter;
				goal.Tagger.Filter = filter;
			}
		}

		public void AddPlay (TimelineEventLongoMatch play)
		{
			field.Tagger.AddPlay (play);
			hfield.Tagger.AddPlay (play);
			goal.Tagger.AddPlay (play);
			QueueDraw ();
		}

		public void RemovePlays (List<TimelineEventLongoMatch> plays)
		{
			field.Tagger.RemovePlays (plays);
			hfield.Tagger.RemovePlays (plays);
			goal.Tagger.RemovePlays (plays);
			QueueDraw ();
		}

		void HandlePlayLoaded (EventLoadedEvent e)
		{
			if (e.TimelineEvent != null) {
				field.Tagger.SelectPlay (e.TimelineEvent as TimelineEventLongoMatch);
				hfield.Tagger.SelectPlay (e.TimelineEvent as TimelineEventLongoMatch);
				goal.Tagger.SelectPlay (e.TimelineEvent as TimelineEventLongoMatch);
			} else {
				field.Tagger.ClearSelection ();
				hfield.Tagger.ClearSelection ();
				goal.Tagger.ClearSelection ();
			}
		}

		void HandleShowMenuEvent (IEnumerable<TimelineEvent> plays)
		{
			if (plays == null || !plays.Any ()) {
				return;
			}
			menu.ShowMenu (project, plays.ToList ());
		}

		protected override void OnDestroyed ()
		{
			field.Destroy ();
			hfield.Destroy ();
			goal.Destroy ();
			App.Current.EventsBroker.Unsubscribe<EventLoadedEvent> (HandlePlayLoaded);
			base.OnDestroyed ();
		}
	}
}
