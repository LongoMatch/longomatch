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
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.CanvasObjects.Timeline;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Drawing.CanvasObjects.Timeline;
using LMCommon = VAS.Core.Common;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.Widgets
{
	public class PlaysTimeline : VAS.Drawing.Widgets.PlaysTimeline
	{
		public PlaysTimeline (IWidget widget) : base (widget)
		{
			
		}

		protected override void FillCanvas ()
		{
			TimelineObject tl;
			int i = 0;

			tl = new TimerTimeline (project.Periods.Select (p => p as Timer).ToList (),
				true, NodeDraggingMode.All, false, duration,
				i * StyleConf.TimelineCategoryHeight,
				VASDrawing.Utils.ColorForRow (i), Config.Style.PaletteBackgroundDark);
			AddTimeline (tl, null);
			PeriodsTimeline = tl as TimerTimeline;
			i++;

			foreach (Timer t in project.Timers) {
				tl = new TimerTimeline (new List<Timer> { t }, false, NodeDraggingMode.All, false, duration,
					i * StyleConf.TimelineCategoryHeight,
					VASDrawing.Utils.ColorForRow (i), Config.Style.PaletteBackgroundDark);
				AddTimeline (tl, t);
			}
			                        
			foreach (EventType type in project.EventTypes) {
				List<TimelineEvent> timelineEventList = project.EventsByType (type);
				var timelineEventLongoMatchList = new List<TimelineEvent> ();
				timelineEventList.ForEach (x => timelineEventLongoMatchList.Add (x));
				tl = new CategoryTimeline (project, timelineEventLongoMatchList, duration,
					i * StyleConf.TimelineCategoryHeight,
					VASDrawing.Utils.ColorForRow (i), playsFilter);
				AddTimeline (tl, type);
				i++;
			}
			UpdateVisibleCategories ();
			Update ();
			HeightRequest = Objects.Count * StyleConf.TimelineCategoryHeight;
		}
	}
}
