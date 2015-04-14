//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	public class LinkAnchorObject: CanvasObject, ICanvasSelectableObject
	{

		readonly Circle circle;
		const int radius = 5;

		public LinkAnchorObject (DashboardButtonObject button, List<Tag> tags, Point relPos)
		{
			RelativePosition = relPos;
			Width = button.Width;
			Height = button.Height;
			Button = button;
			circle = new Circle (Center, radius);
			if (tags == null)
				tags = new List<Tag> ();
			Tags = tags;
		}

		public DashboardButtonObject Button {
			get;
			set;
		}

		public Point RelativePosition {
			get;
			set;
		}

		public double Width {
			get;
			set;
		}

		public double Height {
			get;
			set;
		}

		public List<Tag> Tags {
			get;
			set;
		}

		public Point Position {
			get {
				return Button.Position + RelativePosition;
			}
		}

		public Point Center {
			get {
				Point pos = Position;
				pos.X += Width / 2;
				pos.Y += Height / 2;
				return pos;
			}
		}

		public bool CanLink (LinkAnchorObject anchor)
		{
			if (anchor == null)
				return false;
			else if (this == anchor)
				return false;
			else if (Button == anchor.Button)
				return false;
			else if (Button is TimerObject && anchor.Button is TimerObject)
				return true;
			else if (Button is TagObject && anchor.Button is TagObject)
				return true;
			else if (Button.Button is EventButton && anchor.Button.Button is EventButton)
				return true;
			return false;
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection sel;

			circle.Center = Center;
			sel = circle.GetSelection (point, precision, inMotion);
			if (sel != null) {
				sel.Drawable = this;
				sel.Position = SelectionPosition.All;
			}
			return sel;
		}

		public void Move (Selection s, Point dst, Point start)
		{
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color color = Color.Red1;
			if (Highlighted) {
				color = Config.Style.PaletteActive;
			}

			tk.Begin ();
			tk.LineWidth = 2;
			tk.FillColor = color;
			tk.StrokeColor = color;
			tk.DrawCircle (Center, 5);
			tk.End ();
		}
	}
}

