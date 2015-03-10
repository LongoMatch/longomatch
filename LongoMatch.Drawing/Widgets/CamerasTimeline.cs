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
using System.Linq;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.CanvasObjects;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store.Drawables;
using System.Collections.Generic;

namespace LongoMatch.Drawing.Widgets
{
	public class CamerasTimeline: SelectionCanvas
	{
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event ShowTimerMenuHandler ShowTimerMenuEvent;

		double secondsPerPixel;
		Time duration;

		List<TimelineObject> timelines;
		List<Timer> timers;

		MediaFileSet fileSet;

		public CamerasTimeline (IWidget widget): base(widget)
		{
			secondsPerPixel = 0.1;
			Accuracy = Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
		}

		public void Load (List<Period> periods, MediaFileSet fileSet, Time duration)
		{
			timelines = new List<TimelineObject> ();
			// Store periods as a list of timers
			this.timers = periods.Select (p => p as Timer).ToList ();
			// And the file set
			this.fileSet = fileSet;
			this.duration = duration;

			ClearObjects ();
			FillCanvas ();
			widget.ReDraw ();
		}

		public TimerTimeline TimerTimeline {
			get {
				return PeriodsTimeline;
			}
		}

		public TimerTimeline PeriodsTimeline {
			get;
			set;
		}

		public Time CurrentTime {
			set {
				foreach (TimelineObject tl in timelines) {
					tl.CurrentTime = value;
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

		void Update ()
		{
			double width;

			if (duration == null)
				return;
			width = duration.TotalSeconds / SecondsPerPixel;
			widget.Width = width + 10;
			foreach (TimelineObject tl in timelines) {
				tl.Width = width + 10;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}

		void AddTimeLine (TimelineObject tl)
		{
			AddObject (tl);
			timelines.Add (tl);
		}

		void FillCanvas ()
		{
			// Calculate height depending on number of cameras - 1 (for the main camera) + the line for periods
			widget.Height = Constants.TIMER_HEIGHT * fileSet.Count;

			// Add the timeline for periods
			PeriodsTimeline = new TimerTimeline (timers, true, NodeSelectionMode.All, true, duration, 0,
				Config.Style.PaletteBackground,
				Config.Style.PaletteBackgroundLight);
			AddTimeLine (PeriodsTimeline);

			// And for the cameras
			for (int i = 1; i < fileSet.Count; i++) {
				CameraTimeline cameraTimeLine = new CameraTimeline (fileSet [i], true, NodeSelectionMode.Segment, true, duration, i * StyleConf.TimelineCategoryHeight,
					Config.Style.PaletteBackground,
					Config.Style.PaletteBackgroundLight);
				AddTimeLine (cameraTimeLine);
			}
			Update ();
		}

		protected override void StartMove (Selection sel)
		{
			if (sel == null)
				return;

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

		protected override void ShowMenu (Point coords)
		{
			if (ShowTimerMenuEvent != null) {
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
