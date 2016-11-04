//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Filters;
using VAS.Core.Store;
using VAS.Drawing.CanvasObjects.Timeline;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	/// <summary>
	/// A timeline that renders timeline events using <see cref="TimelineEventObject"/>
	/// </summary>
	public class SportCategoryTimeline : CategoryTimeline
	{
		public SportCategoryTimeline (ProjectLongoMatch project, List<TimelineEvent> plays, Time maxTime,
									  double offsetY, Color background, EventsFilter filter) : base (project, plays, maxTime,
																									 offsetY, background, filter)
		{
		}

		public override void AddPlay (TimelineEvent play)
		{
			TimelineEventObject po = new TimelineEventObject (play, project);
			po.SelectionLeft = selectionBorderL;
			po.SelectionRight = selectionBorderR;
			po.OffsetY = OffsetY;
			po.Height = Height;
			po.SecondsPerPixel = SecondsPerPixel;
			po.MaxTime = maxTime;
			AddNode (po);
		}
	}
}
