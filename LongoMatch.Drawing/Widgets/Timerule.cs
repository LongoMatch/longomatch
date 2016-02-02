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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Drawing.CanvasObjects.Timeline;

namespace LongoMatch.Drawing.Widgets
{
	public class Timerule: SelectionCanvas
	{
		public event EventHandler CenterPlayheadClicked;
		public event SeekEventHandler SeekEvent;

		const int BIG_LINE_HEIGHT = 15;
		const int SMALL_LINE_HEIGHT = 5;
		const int TEXT_WIDTH = 20;
		const int TIME_SPACING = 100;
		NeedleObject needle;
		double scroll;
		double secondsPerPixel;
		Time currentTime;
		Time duration;
		IPlayerController player;

		public Timerule (IWidget widget) : base (widget)
		{
			needle = new NeedleObject ();
			AddObject (needle);
			SecondsPerPixel = 0.1;
			currentTime = new Time (0);
			AdjustSizeToDuration = false;
			ContinuousSeek = true;
			BackgroundColor = Config.Style.PaletteBackgroundDark;
			Accuracy = 5.0f;
		}

		public Timerule () : this (null)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				Player = null;
			}
			base.Dispose (disposing);
		}

		public double Scroll {
			set {
				scroll = value;
				needle.ResetDrawArea ();
			}
			protected get {
				return scroll;
			}
		}

		public Time Duration {
			set {
				duration = value;
				needle.ResetDrawArea ();
				widget?.ReDraw ();
			}
			protected get {
				return duration;
			}
		}

		public Time CurrentTime {
			get {
				return currentTime;
			}
			set {
				Area area;
				double start, stop, timeX;

				timeX = Utils.TimeToPos (value, SecondsPerPixel) - Scroll;
				if (needle.X < timeX) {
					start = needle.X;
					stop = timeX;
				} else {
					start = timeX;
					stop = needle.X;
				}
				start -= needle.Width / 2;
				stop += needle.Width / 2;
				area = new Area (new Point (start - 1, needle.TopLeft.Y), stop - start + 2, needle.Height);
				currentTime = value;
				needle.ResetDrawArea ();
				widget?.ReDraw (area);
			}
		}

		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				needle.ResetDrawArea ();
			}
			get {
				return secondsPerPixel;
			}
		}

		public IPlayerController Player {
			get {
				return player;
			}
			set {
				if (player != null) {
					player.PlaybackStateChangedEvent -= HandlePlaybackStateChanged;
				}
				player = value;
				if (player != null) {
					player.PlaybackStateChangedEvent += HandlePlaybackStateChanged;
				}
			}
		}

		/// <summary>
		/// Flag to set the mode to AdjustSizeToDuration.
		/// AdjustSizeToDuration mode means that the timerule area will include the whole duration, without scroll.
		/// </summary>
		public bool AdjustSizeToDuration {
			set;
			get;
		}

		/// <summary>
		/// Flag to set the mode to presentation.
		/// Presentation mode means that seeks will be made on StopMove, and not on SelectionMove
		/// </summary>
		/// <value><c>true</c> if presentation mode; otherwise, <c>false</c>.</value>
		public bool ContinuousSeek {
			set;
			get;
		}

		bool PlayingState {
			get;
			set;
		}

		bool WasPlaying {
			get;
			set;
		}

		protected override void StartMove (Selection sel)
		{
			WasPlaying = PlayingState;
			Config.EventsBroker.EmitTogglePlayEvent (false);
		}

		protected override void StopMove (bool moved)
		{
			if (moved && !ContinuousSeek) {
				if (SeekEvent != null) {
					SeekEvent (Utils.PosToTime (new Point (needle.X + Scroll, 0), SecondsPerPixel),
						true);
				}
			}
			Config.EventsBroker.EmitTogglePlayEvent (WasPlaying);
		}

		protected override void SelectionMoved (Selection sel)
		{
			if (ContinuousSeek) {
				if (SeekEvent != null) {
					SeekEvent (Utils.PosToTime (new Point (needle.X + Scroll, 0), SecondsPerPixel),
						false, throttled: true);
				}
			}
		}

		protected override void HandleLeftButton (Point coords, ButtonModifier modif)
		{
			base.HandleLeftButton (coords, modif);

			if (!Selections.Any ()) {
				needle.X = coords.X;
				if (SeekEvent != null) {
					SeekEvent (Utils.PosToTime (new Point (needle.X + Scroll, 0), SecondsPerPixel),
						true);
				}
				needle.ReDraw ();
			}
		}

		protected override void HandleDoubleClick (Point coords, ButtonModifier modif)
		{
			base.HandleDoubleClick (coords, modif);

			if (Selections.Any ()) {
				if (CenterPlayheadClicked != null) {
					CenterPlayheadClicked (this, new EventArgs ());
				}
			}
		}

		void HandlePlaybackStateChanged (object sender, bool playing)
		{
			PlayingState = playing;
		}

		public override void Draw (IContext context, Area area)
		{
			int startX, start, stop;
			double tpos, height, width;
			
			if (Duration == null) {
				return;
			}
			
			height = widget.Height;
			width = widget.Width;

			if (AdjustSizeToDuration) {
				SecondsPerPixel = Duration.TotalSeconds / width;
			}

			Begin (context);
			DrawBackground ();

			tk.StrokeColor = Config.Style.PaletteWidgets;
			tk.FillColor = Config.Style.PaletteWidgets;
			tk.LineWidth = Constants.TIMELINE_LINE_WIDTH;
			tk.FontSlant = FontSlant.Normal;
			tk.FontSize = StyleConf.TimelineRuleFontSize;
			tk.DrawLine (new Point (area.Start.X, height),
				new Point (area.Start.X + area.Width, height));

			startX = (int)(area.Start.X + Scroll);
			start = (startX - (startX % TIME_SPACING));
			stop = (int)(startX + area.Width + TIME_SPACING);

			/* Draw big lines each 10 * secondsPerPixel */
			for (int i = start; i <= stop; i += TIME_SPACING) {
				double pos = i - Scroll;
				tk.DrawLine (new Point (pos, height),
					new Point (pos, height - BIG_LINE_HEIGHT));
				tk.DrawText (new Point (pos - TEXT_WIDTH / 2, 2), TEXT_WIDTH, height - BIG_LINE_HEIGHT - 2,
					new Time { TotalSeconds = (int)(i * SecondsPerPixel) }.ToSecondsString ());
			}

			start = (startX - (startX % (TIME_SPACING / 10))) + (TIME_SPACING / 10);
			/* Draw small lines each 1 * secondsPerPixel */
			for (int i = start; i <= stop; i += TIME_SPACING / 10) {
				double pos;
				
				if (i % TIME_SPACING == 0)
					continue;
					
				pos = i - Scroll;
				tk.DrawLine (new Point (pos, height),
					new Point (pos, height - SMALL_LINE_HEIGHT));
			}
			
			/* Draw position triangle */
			needle.TimelineHeight = height;
			if (!Moving) {
				tpos = Utils.TimeToPos (CurrentTime, SecondsPerPixel);
				tpos -= Scroll;
				needle.X = tpos;
			}
			needle.Draw (tk, area);
			End ();
		}
	}
}