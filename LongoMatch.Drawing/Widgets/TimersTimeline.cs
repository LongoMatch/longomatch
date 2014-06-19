using System.Linq;
using LongoMatch.Store;
using LongoMatch.Drawing.CanvasObject;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;
using System.Collections.Generic;

namespace LongoMatch.Drawing.Widgets
{
	public class TimersTimeline: SelectionCanvas
	{
	
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event PlaySelectedHandler TimeNodeSelected;
		public event ShowTimelineMenuHandler ShowMenuEvent;

		double secondsPerPixel;
		Time duration;
		Dictionary <Timer, TimerTimeline> timers;
		
		public TimersTimeline (IWidget widget): base(widget)
		{
			secondsPerPixel = 0.1;
			Accuracy = Common.TIMELINE_ACCURACY;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
		}

		public void LoadTimers (List<Timer> timers, Time duration, bool splitTimers) {
			Objects.Clear();
			this.timers = new Dictionary<Timer, TimerTimeline>();
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
		
		void Update () {
			double width = duration.Seconds / SecondsPerPixel;
			widget.Width = width + 10;
			foreach (TimelineObject tl in timers.Values) {
				tl.Width = width + 10;
				tl.SecondsPerPixel = SecondsPerPixel;
			}
		}
		
		void FillCanvas (List<Timer> timers, bool splitTimers) {
			if (!splitTimers) {
				widget.Height = Common.TIMER_HEIGHT;
				TimerTimeline tl = new TimerTimeline (timers, duration, 0, Color.White);
				foreach (Timer t in timers) {
					this.timers[t] = tl;
				}
				Objects.Add (tl);
			} else {
				widget.Height = timers.Count * Common.TIMER_HEIGHT;
			}
			Update ();
		}
		
		protected override void SelectionChanged (List<Selection> selections) {
		}
		
		protected override void StartMove (Selection sel) {
			if (sel.Position != SelectionPosition.All) {
				widget.SetCursor (CursorType.DoubleArrow);
			}
		}
		
		protected override void StopMove () {
			widget.SetCursor (CursorType.Arrow);
		}

		protected override void ShowMenu (Point coords) {
		}
		
		protected override void SelectionMoved (Selection sel) {
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
