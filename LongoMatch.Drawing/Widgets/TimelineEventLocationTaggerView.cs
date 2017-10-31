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
using LongoMatch.Core.Handlers;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.CanvasObjects.Location;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.Widgets
{
	/// <summary>
	/// A view to set the location of a single timeline event in a map.
	/// </summary>
	public class TimelineEventLocationTaggerView : BackgroundCanvas, ICanvasView<LMTimelineEventVM>
	{
		public event ShowTaggerMenuHandler ShowMenuEvent;
		LMTimelineEventVM viewModel;
		TimelineEventLocationView positionView;
		FieldPositionType fieldPosition;

		public TimelineEventLocationTaggerView (IWidget widget) : base (widget)
		{
			Accuracy = VASDrawing.Constants.TAGGER_POINT_SIZE + 3;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			BackgroundColor = App.Current.Style.ScreenBase;

			positionView = new TimelineEventLocationView ();
			AddObject (positionView);
		}

		public TimelineEventLocationTaggerView () : this (null)
		{
		}

		public FieldPositionType FieldPosition {
			get {
				return fieldPosition;
			}
			set {
				fieldPosition = value;
				positionView.FieldPosition = value;
			}
		}

		public LMTimelineEventVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				positionView.BackgroundWidth = Background.Width;
				positionView.BackgroundHeight = Background.Height;
				positionView.ViewModel = viewModel;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTimelineEventVM)viewModel;
		}
	}
}