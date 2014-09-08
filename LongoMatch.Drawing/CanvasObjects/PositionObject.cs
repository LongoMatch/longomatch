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
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Interfaces;
using System.Collections.Generic;

namespace LongoMatch.Drawing.CanvasObjects
{
	public class PositionObject:  CanvasObject, ICanvasSelectableObject
	{

		public PositionObject (List<Point> points, int width, int height)
		{
			Points = points;
			Width = width;
			Height = height;
		}

		public override string Description {
			get {
				if (Play != null) {
					return Play.Name;
				}
				return null;
			}
		}

		public int Width {
			get;
			set;
		}

		public int Height {
			get;
			set;
		}

		public TimelineEvent Play {
			get;
			set;
		}

		public List<Point> Points {
			get;
			set;
		}

		Point Start {
			get {
				return Points [0].Denormalize (Width, Height);
			}
			set {
				Points [0] = value.Normalize (Width, Height);
			}
		}

		Point Stop {
			get {
				return Points [1].Denormalize (Width, Height);
			}
			set {
				Points [1] = value.Normalize (Width, Height);
			}
		}

		public Selection GetSelection (Point point, double precision, bool inMotion=false)
		{
			if (point.Distance (Start) < precision) {
				return new Selection (this, SelectionPosition.LineStart);
			} else if (Points.Count == 2 && point.Distance (Stop) < precision) {
				return new Selection (this, SelectionPosition.LineStop);
			}
			return null;
		}

		public void Move (Selection sel, Point p, Point start)
		{
			switch (sel.Position) {
			case SelectionPosition.LineStart:
				Start = p;
				break;
			case SelectionPosition.LineStop:
				Stop = p;
				break;
			default:
				throw new Exception ("Unsupported move for circle:  " + sel.Position);
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			Color color, scolor;
			double relSize;
			
			relSize = Math.Max (1, (double)Width / 200);
			
			tk.Begin ();
			if (Play != null) {
				color = Play.Color;
			} else {
				color = Constants.TAGGER_POINT_COLOR;
			}
			scolor = color;
			
			if (Selected) {
				scolor = Constants.TAGGER_SELECTION_COLOR;
				color = Constants.TAGGER_SELECTION_COLOR;
			}
			tk.FillColor = color;
			tk.StrokeColor = scolor;
			tk.LineWidth = (int)relSize;
			tk.DrawCircle (Start, (int)relSize * 2);
			if (Points.Count == 2) {
				tk.LineWidth = (int)relSize * 2;
				tk.DrawLine (Start, Stop);
				tk.DrawArrow (Start, Stop, 10, 0.3, true);
			}
			tk.End ();
		}
	}
}

