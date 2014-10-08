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

		double secondsPerPixel;
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
			LoadTimers (periods.Select (p => p as Timer).ToList (), duration, false);
		}

		public void LoadTimers (List<Timer> timers, Time duration, bool splitTimers = true)
		{
			ClearObjects ();
			this.timers = new Dictionary<Timer, TimerTimeline> ();
			this.duration = duration;
			FillCanvas (timers, splitTimers);
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

		void FillCanvas (List<Timer> timers, bool splitTimers)
		{
			if (!splitTimers) {
				widget.Height = Constants.TIMER_HEIGHT;
				TimerTimeline tl = new TimerTimeline (timers, true, false, true, duration, 0,
				                                      Config.Style.PaletteBackground,
				                                      Config.Style.PaletteBackgroundLight);
				foreach (Timer t in timers) {
					this.timers [t] = tl;
				}
				AddObject (tl);
			} else {
				widget.Height = timers.Count * Constants.TIMER_HEIGHT;
			}
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

		protected override void StopMove ()
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
	}
}
