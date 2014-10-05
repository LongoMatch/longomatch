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

		public override Color BackgroundColor {
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

		public virtual int NRows {
			get {
				return 1;
			}
		}

		public Time Start {
			get;
			set;
		}
	}
}

