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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Store;
using LongoMatch.Core.Handlers;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class TaggerObject: ButtonObject, ICanvasSelectableObject
	{

		public TaggerObject (DashboardButton tagger)
		{
			Tagger = tagger;
		}

		public DashboardButton Tagger {
			get;
			set;
		}

		public override Point Position {
			get {
				return Tagger.Position;
			}
			set {
				Tagger.Position = value;
			}
		}

		public override double Width {
			get {
				return Tagger.Width;
			}
			set {
				Tagger.Width = (int)value;
			}
		}

		public override double Height {
			get {
				return Tagger.Height;
			}
			set {
				Tagger.Height = (int)value;
			}
		}

		public Time Start {
			get;
			set;
		}

		public override Color BackgroundColor {
			get {
				return Tagger.BackgroundColor;
			}
		}

		public override Color BackgroundColorActive {
			get {
				return Tagger.DarkColor;
			}
		}

		public override Color BorderColor {
			get {
				return Tagger.BackgroundColor;
			}
		}

		public override Color TextColor {
			get {
				return Tagger.TextColor;
			}
		}

		public override Image BackgroundImage {
			get {
				return Tagger.BackgroundImage;
			}
		}

		public override Image BackgroundImageActive {
			get {
				return Tagger.BackgroundImage;
			}
		}

		public override bool Active {
			get {
				return base.Active;
			}
			set {
				if (Mode != TagMode.Edit) {
					base.Active = value;
				}
			}
		}

		public virtual int NRows {
			get {
				return 1;
			}
		}
	}

	public class TimedTaggerObject: TaggerObject
	{
		Time currentTime;

		public TimedTaggerObject (TimedDashboardButton button): base (button)
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
					if (currentTime < Start) {
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

