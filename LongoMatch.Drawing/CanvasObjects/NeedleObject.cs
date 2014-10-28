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
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Drawables;
using System.IO;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class NeedleObject: CanvasObject, ICanvasSelectableObject
	{
		static ISurface needle;

		public NeedleObject ()
		{
			if (needle == null) {
				string  path = Path.Combine (Config.IconsDir, StyleConf.TimelineNeedleResource); 
				Image img = Image.LoadFromFile (path);
				needle = Config.DrawingToolkit.CreateSurface (img.Width, img.Height, img, false);
			}
			Width = needle.Width;
			X = 0;
			TimelineHeight = 0;
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (needle != null) {
				needle.Dispose ();
				needle = null;
			}
		}

		public double X {
			get;
			set;
		}

		public double TimelineHeight {
			get;
			set;
		}

		public double Width {
			get;
			set;
		}

		public double Height {
			get {
				return needle.Height;
			}
		}
		
		public Point TopLeft {
			get {
				return new Point (X - Width / 2, TimelineHeight - needle.Height);
			}
		}
		
		Area Area {
			get {
				return new Area (TopLeft, Width, Width);
			}
		}
		
		public override void Draw (IDrawingToolkit tk, LongoMatch.Core.Common.Area area)
		{
			if (!UpdateDrawArea (tk, area, Area)) {
				return;
			};
			
			tk.Begin ();
			tk.DrawSurface (needle, TopLeft);
			tk.End ();
		}
		
		public Selection GetSelection (Point point, double precision, bool inMotion=false)
		{
			if ((Math.Abs (point.X - X) < Width / 2 + precision)) {
				return new Selection (this, SelectionPosition.All, 0);
			} else {
				return null;
			}
		}

		public void Move (Selection s, Point p, Point start)
		{
			if (s.Position == SelectionPosition.All) {
				X = p.X;
			}
		}
	}
}

