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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using LongoMatch.Drawing.CanvasObjects.Timeline;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects.Timeline;
using VAS.Services.ViewModel;
using Timer = VAS.Core.Store.Timer;

namespace LongoMatch.Drawing.Widgets
{
	public class CamerasTimelineView : SelectionCanvas, ICanvasView<CameraSynchronizationVM>
	{
		public event ShowTimerMenuHandler ShowTimerMenuEvent;

		double secondsPerPixel;
		Time currentTime;
		List<TimelineView> timelines;
		CameraSynchronizationVM viewModel;

		public CamerasTimelineView (IWidget widget) : base (widget)
		{
			secondsPerPixel = 0.1;
			Accuracy = VAS.Drawing.Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			currentTime = new Time (0);
		}

		public CamerasTimelineView () : this (null)
		{
		}

		public CameraSynchronizationVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.VideoPlayer.PropertyChanged -= HandlePropertyChanged;
					viewModel.Project.Periods.GetNotifyCollection ().CollectionChanged
							 -= HandlePeriodsCollectionChanged;
				}
				viewModel = value;
				timelines = new List<TimelineView> ();
				ClearObjects ();
				FillCanvas ();
				if (viewModel != null) {
					viewModel.VideoPlayer.PropertyChanged += HandlePropertyChanged;
					viewModel.Project.Periods.GetNotifyCollection ().CollectionChanged
							 += HandlePeriodsCollectionChanged;
				}
				widget?.ReDraw ();
			}
		}

		public PeriodsTimelineView PeriodsTimeline {
			get;
			set;
		}

		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				Update ();
			}
			get {
				return secondsPerPixel;
			}
		}

		public CameraView SelectedCamera {
			get {
				return Selections.FirstOrDefault ()?.Drawable as CameraView;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (CameraSynchronizationVM)viewModel;
		}

		Time CurrentTime {
			set {
				Area area;
				double start, stop;

				foreach (TimelineView tl in timelines) {
					tl.CurrentTime = value;
				}
				if (currentTime < value) {
					start = VAS.Drawing.Utils.TimeToPos (currentTime, SecondsPerPixel);
					stop = VAS.Drawing.Utils.TimeToPos (value, SecondsPerPixel);
				} else {
					start = VAS.Drawing.Utils.TimeToPos (value, SecondsPerPixel);
					stop = VAS.Drawing.Utils.TimeToPos (currentTime, SecondsPerPixel);
				}
				currentTime = value;
				if (widget != null) {
					area = new Area (new Point (start - 1, 0), stop - start + 2, widget.Height);
					widget.ReDraw (area);
				}
			}
		}

		void Update ()
		{
			int height = 0;
			if (ViewModel == null)
				return;

			double width = ViewModel.Project.FileSet.Duration.TotalSeconds / SecondsPerPixel + StyleConf.TimelinePadding;
			foreach (TimelineView tl in timelines) {
				tl.Width = width;
				tl.SecondsPerPixel = SecondsPerPixel;
				height += (int)tl.Height;
			}
			WidthRequest = (int)width;
			HeightRequest = height;
		}

		void AddTimeLine (TimelineView tl)
		{
			AddObject (tl);
			timelines.Add (tl);
		}

		void FillCanvas ()
		{
			int i = 0;
			// Add the timeline for periods
			PeriodsTimeline = new PeriodsTimelineView {
				ShowLine = true,
				Duration = ViewModel.Project.FileSet.Duration,
				DraggingMode = NodeDraggingMode.All,
				Height = StyleConf.TimelineCameraHeight,
				OffsetY = 0,
				LineColor = App.Current.Style.ThemeContrastDisabled,
				BackgroundColor = App.Current.Style.ScreenBase
			};
			PeriodsTimeline.ViewModel = ViewModel.Project.Periods;
			AddTimeLine (PeriodsTimeline);
			i++;

			// Now add the timeline for the secondary cameras.
			// The main camera does not have a timeline since its the master camera and secondary cameras are synced
			// with respect of the main camera.
			foreach (MediaFileVM fileVM in ViewModel.Project.FileSet.Skip (1)) {
				CameraTimelineView cameraTimeLine = new CameraTimelineView {
					ShowName = false,
					ShowLine = true,
					Height = StyleConf.TimelineCameraHeight,
					OffsetY = i * StyleConf.TimelineCameraHeight,
					LineColor = App.Current.Style.ThemeContrastDisabled,
					BackgroundColor = App.Current.Style.ScreenBase,
				};
				cameraTimeLine.ViewModel = fileVM;
				AddTimeLine (cameraTimeLine);
				i++;
			}
			Update ();
		}

		protected override void StartMove (Selection sel)
		{
			if (sel == null || sel.Drawable as TimeNodeView == null)
				return;

			(sel.Drawable as TimeNodeView).ClippingMode = NodeClippingMode.NoStrict;

			if (sel.Position == SelectionPosition.All) {
				widget.SetCursor (CursorType.Selection);
			} else {
				widget.SetCursor (CursorType.DoubleArrow);
			}
		}

		protected override void StopMove (bool moved)
		{
			widget.SetCursor (CursorType.Arrow);
		}

		protected override void ShowMenu (Point coords)
		{
			if (ShowTimerMenuEvent != null &&
				coords.Y >= PeriodsTimeline.OffsetY &&
				coords.Y <= PeriodsTimeline.OffsetY + PeriodsTimeline.Height) {
				Timer t = null;
				if (Selections.Count > 0) {
					TimerTimeNodeView to = Selections.Last ().Drawable as TimerTimeNodeView;
					t = to.Timer.Model;
				}
				ShowTimerMenuEvent (t, VAS.Drawing.Utils.PosToTime (coords, SecondsPerPixel));
			}
		}

		void HandlePropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof (VideoPlayerVM.CurrentTime)) {
				CurrentTime = ViewModel.VideoPlayer.CurrentTime;
			}
		}

		void HandlePeriodsCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Remove) {
				foreach (var timerVM in e.OldItems.OfType<TimerVM> ()) {
					Selections.RemoveAll (s => (s.Drawable as TimerTimeNodeView).Timer == timerVM);
				}
			}
		}
	}
}
