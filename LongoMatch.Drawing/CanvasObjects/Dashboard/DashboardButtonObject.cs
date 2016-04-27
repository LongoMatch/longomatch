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
using LongoMatch.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	public class DashboardButtonObject: ButtonObject, ICanvasSelectableObject
	{
		protected LinkAnchorObject anchor;

		public DashboardButtonObject (DashboardButton tagger)
		{
			Button = tagger;
			SupportsLinks = true;
			anchor = new LinkAnchorObject (this, null, new Point (0, 0));
			anchor.RedrawEvent += (co, area) => {
				EmitRedrawEvent (anchor, area);
			};
		}

		protected override void Dispose (bool disposing)
		{
			anchor.Dispose ();
			base.Dispose (disposing);
		}

		public DashboardMode Mode {
			get;
			set;
		}

		public bool SupportsLinks {
			get;
			set;
		}

		public bool ShowLinks {
			get;
			set;
		}

		public DashboardButton Button {
			get;
			set;
		}

		public bool EditActionLinks {
			get;
			set;
		}

		public override bool DrawsSelectionArea {
			get {
				return Mode == DashboardMode.Edit;
			}
		}

		public override Point Position {
			get {
				return Button.Position;
			}
			set {
				Button.Position = value;
			}
		}

		public override double Width {
			get {
				return Button.Width;
			}
			set {
				Button.Width = (int)value;
			}
		}

		public override double Height {
			get {
				return Button.Height;
			}
			set {
				Button.Height = (int)value;
			}
		}

		public Time Start {
			get;
			set;
		}

		public override Color BackgroundColor {
			get {
				return Button.BackgroundColor;
			}
		}

		public override Color BackgroundColorActive {
			get {
				return Button.DarkColor;
			}
		}

		public override Color BorderColor {
			get {
				return Button.TextColor;
			}
		}

		public override Color TextColor {
			get {
				return Button.TextColor;
			}
		}

		public override Image BackgroundImage {
			get {
				return Button.BackgroundImage;
			}
		}

		public override Image BackgroundImageActive {
			get {
				return Button.BackgroundImage;
			}
		}

		public override bool Active {
			get {
				return base.Active;
			}
			set {
				if (Mode != DashboardMode.Edit) {
					base.Active = value;
				}
			}
		}

		public virtual int NRows {
			get {
				return 1;
			}
		}

		public virtual LinkAnchorObject GetAnchor (IList<Tag> sourceTags)
		{
			return anchor;
		}

		protected void DrawAnchor (IDrawingToolkit tk, Area area)
		{
			if (ShowLinks && SupportsLinks) {
				anchor.Draw (tk, area);
			}
		}

		public override void ResetDrawArea ()
		{
			anchor.ResetDrawArea ();
			base.ResetDrawArea ();
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			base.Draw (tk, area);
			DrawAnchor (tk, area);
		}

		public override Selection GetSelection (Point p, double precision, bool inMotion = false)
		{
			if (ShowLinks && SupportsLinks) {
				Selection sel = anchor.GetSelection (p, precision, inMotion);
				if (sel != null)
					return sel;
			}
			return base.GetSelection (p, precision, inMotion);
		}
	}

	public class TimedTaggerObject: DashboardButtonObject
	{
		Time currentTime;

		public TimedTaggerObject (TimedDashboardButton button) : base (button)
		{
			TimedButton = button;
			currentTime = new Time (0);
			Start = null;
		}

		public TimedDashboardButton TimedButton {
			get;
			set;
		}

		public Time CurrentTime {
			get {
				return currentTime;
			}
			set {
				Time prevCurrentTime = currentTime;
				currentTime = value;
				if (Start != null) {
					bool secsChanged = (prevCurrentTime - Start).TotalSeconds != (value - Start).TotalSeconds;
					/* Add a tolerance of 100ms, as sometimes after pausing and restarting
					 * the clocks goes backwards by a few ms. So if a event has started recording
					 * with the player paused, resuming playback might cancel the event started */
					if (currentTime.MSeconds + 100 < Start.MSeconds) {
						Clear ();
					} else if (secsChanged) {
						ReDraw ();
					}
				}
			}
		}

		protected bool Recording {
			get;
			set;
		}

		public override void ClickReleased ()
		{
			if (TimedButton.TagMode == TagMode.Predefined) {
				Active = !Active;
				EmitClickEvent ();
			} else if (!Recording) {
				StartRecording ();
			} else {
				EmitClickEvent ();
				Clear ();
			}
		}

		protected void StartRecording ()
		{
			Recording = true;
			if (Start == null) {
				Start = CurrentTime;
			}
			Active = true;
			ReDraw ();
		}

		protected virtual void Clear ()
		{
			Recording = false;
			Start = null;
			Active = false;
		}
	}
}

