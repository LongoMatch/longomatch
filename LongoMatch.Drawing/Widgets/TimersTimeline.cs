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
	public class TimersTimeline: SelectionCanvas
	{
	
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event ShowTimerMenuHandler ShowTimerMenuEvent;

		double secondsPerPixel;
		TimerTimeline timertimeline;
		Time duration;
		Dictionary <Timer, TimerTimeline> timers;

		public TimersTimeline (IWidget widget): base(widget)
		{
			secondsPerPixel = 0.1;
			Accuracy = Constants.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
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
			widget.ReDraw ();
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
			width = duration.Seconds / SecondsPerPixel;
			widget.Width = width + 10;
			foreach (TimelineObject tl in timers.Values) {
				tl.Width = width + 10;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}

		void FillCanvas (List<Timer> timers)
		{
			widget.Height = Constants.TIMER_HEIGHT;
			timertimeline = new TimerTimeline (timers, true, true, true, duration, 0,
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

			if (sel.Position != SelectionPosition.All) {
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
