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
using LongoMatch.Drawing.CanvasObjects.Timeline;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects.Timeline;
using LMTimeline = LongoMatch.Drawing.CanvasObjects.Timeline;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.Widgets
{
	public class TimersTimeline: SelectionCanvas
	{
	
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event ShowTimerMenuHandler ShowTimerMenuEvent;

		double secondsPerPixel;
		TimerTimeline timertimeline;
		Time duration;
		Dictionary <Timer, TimerTimeline> timers;

		public TimersTimeline (IWidget widget) : base (widget)
		{
			secondsPerPixel = 0.1;
			Accuracy = VASDrawing.Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			HeightRequest = VASDrawing.Constants.TIMER_HEIGHT;
		}

		public TimersTimeline () : this (null)
		{
		}

		public void LoadPeriods (List<Period> periods, Time duration)
		{
			LoadTimers (periods.Select (p => p as Timer).ToList (), duration);
		}

		public void LoadTimers (List<Timer> timers, Time duration)
		{
			ClearObjects ();
			this.timers = new Dictionary<Timer, TimerTimeline> ();
			this.duration = duration;
			FillCanvas (timers);
			widget?.ReDraw ();
		}

		public TimerTimeline TimerTimeline {
			get {
				return timertimeline;
			}
		}

		public Time CurrentTime {
			set {
				foreach (TimerTimeline tl in timers.Values) {
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

			width = duration.TotalSeconds / SecondsPerPixel + StyleConf.TimelinePadding;
			foreach (TimelineObject tl in timers.Values) {
				tl.Width = width;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
			WidthRequest = (int)width;
		}

		void FillCanvas (List<Timer> timers)
		{
			timertimeline = new TimerTimeline (timers, true, NodeDraggingMode.All, true, duration, 0,
				Config.Style.PaletteBackground,
				Config.Style.PaletteBackgroundLight);
			foreach (Timer t in timers) {
				this.timers [t] = timertimeline;
			}
			AddObject (timertimeline);
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
				TimeNode tn = (sel.Drawable as LMTimeline.TimeNodeObject).TimeNode;

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
					LMTimeline.TimerTimeNodeObject to = Selections.Last ().Drawable as LMTimeline.TimerTimeNodeObject; 
					t = to.Timer;
				} 
				ShowTimerMenuEvent (t, VASDrawing.Utils.PosToTime (coords, SecondsPerPixel));
			}
		}
	}
}
