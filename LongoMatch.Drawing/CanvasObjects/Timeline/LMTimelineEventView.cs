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
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Drawing.CanvasObjects.Timeline;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	[View ("TimelineEventView")]
	public class LMTimelineEventView : TimelineEventView, ICanvasObjectView<LMTimelineEventVM>
	{
		public LMTimelineEventVM ViewModel {
			get {
				return (LMTimelineEventVM)TimelineEvent;
			}
			set {
				TimelineEvent = value;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTimelineEventVM)viewModel;
		}

		protected override void DrawBorders (IDrawingToolkit tk, double start, double stop, int lineWidth)
		{
			Color color;
			double y1, y2;

			tk.LineWidth = lineWidth;
			List<LMTeam> teams = ViewModel.TypedModel.TaggedTeams;
			if (teams.Count == 1) {
				color = teams [0].Color;
			} else {
				color = App.Current.Style.TextBase;
			}

			tk.FillColor = color;
			tk.StrokeColor = color;
			y1 = OffsetY + 6;
			y2 = OffsetY + Height - 6;
			tk.DrawLine (new Point (start, y1), new Point (start, y2));
			tk.DrawLine (new Point (stop, y1), new Point (stop, y2));
		}
	}
}

