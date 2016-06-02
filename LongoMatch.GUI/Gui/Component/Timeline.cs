//
//  Copyright (C) 2016 
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
using LongoMatch.Drawing.Widgets;
using LongoMatch.Gui.Menus;
using VAS.Core.Filters;
using VAS.Core.Store;
using VAS.Drawing.Cairo;
using Helpers = VAS.UI.Helpers;
using LMCommon = VAS.Core.Common;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class Timeline : VAS.UI.Component.Timeline
	{
		protected PeriodsMenu periodsmenu;

		public Timeline () : base ()
		{
			periodsmenu = new PeriodsMenu ();
		}

		protected override VASDrawing.Widgets.PlaysTimeline createPlaysTimeline ()
		{
			return new PlaysTimeline (new WidgetWrapper (getTimelinearea ()), Player);
		}

		protected override VASDrawing.Widgets.TimelineLabels createTimelineLabels ()
		{
			return new TimelineLabels (new WidgetWrapper (LabelsArea));
		}

		public override void SetProject (Project project, EventsFilter filter)
		{
			this.project = project;
			timeline.LoadProject (project, filter);
			labels.LoadProject (project, filter);

			if (project == null) {
				if (timeoutID != 0) {
					GLib.Source.Remove (timeoutID);
					timeoutID = 0;
				}
				return;
			}

			if (timeoutID == 0) {
				timeoutID = GLib.Timeout.Add (TIMEOUT_MS, UpdateTime);
			}

			FocusScale.Value = 6;
			timerule.Duration = project.FileSet.Duration;

			timeline.ShowMenuEvent += HandleShowMenu;
			timeline.ShowTimersMenuEvent += HandleShowTimersMenu;
			timeline.ShowTimerMenuEvent += HandleShowTimerMenuEvent;
			QueueDraw ();
		}

		protected void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			periodsmenu.ShowMenu (project, timer, time, timeline.PeriodsTimeline, timeline);
		}
	}
}
