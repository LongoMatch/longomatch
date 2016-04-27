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
using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Drawing.CanvasObjects.Timeline;
using VAS.Core.Common;
using VAS.Core.Store.Drawables;
using VAS.Core.Store;

namespace LongoMatch.Drawing.Widgets
{
	public class CamerasTimeline: SelectionCanvas
	{
		// For cameras
		public event CameraDraggedHandler CameraDragged;
		public event EventHandler SelectedCameraChanged;
		// And periods
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event ShowTimerMenuHandler ShowTimerMenuEvent;

		double secondsPerPixel;
		Time currentTime, duration;

		List<TimelineObject> timelines;
		List<Timer> timers;

		MediaFileSet fileSet;

		public CamerasTimeline (IWidget widget) : base (widget)
		{
			secondsPerPixel = 0.1;
			Accuracy = Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			currentTime = new Time (0);
		}

		public CamerasTimeline () : this (null)
		{
		}

		public void Load (IList<Period> periods, MediaFileSet fileSet, Time duration)
		{
			timelines = new List<TimelineObject> ();
			// Store periods as a list of timers
			this.timers = periods.Select (p => p as Timer).ToList ();
			// And the file set
			this.fileSet = fileSet;
			this.duration = duration;

			ClearObjects ();
			FillCanvas ();
			widget?.ReDraw ();
		}

		public TimerTimeline PeriodsTimeline {
			get;
			set;
		}

		public Time CurrentTime {
			set {
				Area area;
				double start, stop;

				foreach (TimelineObject tl in timelines) {
					tl.CurrentTime = value;
				}
				if (currentTime < value) {
					start = Utils.TimeToPos (currentTime, SecondsPerPixel);
					stop = Utils.TimeToPos (value, SecondsPerPixel);
				} else {
					start = Utils.TimeToPos (value, SecondsPerPixel);
					stop = Utils.TimeToPos (currentTime, SecondsPerPixel);
				}
				currentTime = value;
				if (widget != null) {
					area = new Area (new Point (start - 1, 0), stop - start + 2, widget.Height);
					widget.ReDraw (area);
				}
			}
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

		public CameraObject SelectedCamera {
			get {
				Selection sel = Selections.FirstOrDefault ();

				if (sel != null && sel.Drawable is CameraObject)
					return sel.Drawable as CameraObject;
				else
					return null;
			}
		}

		void Update ()
		{
			if (duration == null)
				return;

			double width = duration.TotalSeconds / SecondsPerPixel + StyleConf.TimelinePadding;
			foreach (TimelineObject tl in timelines) {
				tl.Width = width;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
			WidthRequest = (int)width;
		}

		void AddTimeLine (TimelineObject tl)
		{
			AddObject (tl);
			timelines.Add (tl);
		}

		void FillCanvas ()
		{
			// Add the timeline for periods
			PeriodsTimeline = new TimerTimeline (timers, true, NodeDraggingMode.All, true, duration, StyleConf.TimelineCameraHeight, 0,
				Config.Style.PaletteBackground, Config.Style.PaletteBackgroundLight);
			AddTimeLine (PeriodsTimeline);

			// And for the cameras
			for (int i = 1; i < fileSet.Count; i++) {
				CameraTimeline cameraTimeLine = new CameraTimeline (fileSet [i], false, true, duration, i * StyleConf.TimelineCameraHeight,
					                                Config.Style.PaletteBackground,
					                                Config.Style.PaletteBackgroundLight);
				AddTimeLine (cameraTimeLine);
			}
			Update ();
			// Calculate height depending on number of cameras - 1 (for the main camera) + the line for periods
			HeightRequest = StyleConf.TimelineCameraHeight * fileSet.Count;
		}

		protected override void StartMove (Selection sel)
		{
			if (sel == null || sel.Drawable as TimeNodeObject == null)
				return;

			(sel.Drawable as TimeNodeObject).ClippingMode = NodeClippingMode.LeftStrict;

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

		protected override void SelectionMoved (Selection sel)
		{
			if (sel.Drawable is CameraObject) {
				if (CameraDragged != null) {
					CameraObject co = sel.Drawable as CameraObject;
					// Adjust offset
					co.MediaFile.Offset = new Time (-co.TimeNode.Start.MSeconds);
					// And notify
					CameraDragged (co.MediaFile, co.TimeNode);
				}
			} else {
				if (TimeNodeChanged != null) {
					Time moveTime;
					TimeNode tn = (sel.Drawable as TimeNodeObject).TimeNode;

					if (sel.Position == SelectionPosition.Right) {
						moveTime = tn.Stop;
					} else {
						moveTime = tn.Start;
					}
					TimeNodeChanged (tn, moveTime);
				}
			}
		}

		protected override void SelectionChanged (List<Selection> sel)
		{
			// Fire an event
			if (SelectedCameraChanged != null) {
				SelectedCameraChanged (this, new EventArgs ());
			}
		}

		protected override void ShowMenu (Point coords)
		{
			if (ShowTimerMenuEvent != null &&
			    coords.Y >= PeriodsTimeline.OffsetY &&
			    coords.Y <= PeriodsTimeline.OffsetY + PeriodsTimeline.Height) {
				Timer t = null;
				if (Selections.Count > 0) {
					TimerTimeNodeObject to = Selections.Last ().Drawable as TimerTimeNodeObject; 
					t = to.Timer;
				} 
				ShowTimerMenuEvent (t, Utils.PosToTime (coords, SecondsPerPixel));
			}
		}
	}
}
