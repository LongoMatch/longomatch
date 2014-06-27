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
using LongoMatch.Common;

namespace LongoMatch.Common
{
	public class Area
	{
		public Area (Point start, double width, double height)
		{
			Start = start;
			Width = width;
			Height = height;
		}
		
		public Point Start {
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
		
		public Point Center {
			get {
				return new Point (Start.X + Width / 2, Start.Y + Height / 2);
			}
		}
		
		public Point[] Vertices {
			get {
				return new Point[] {
					new Point (Start.X, Start.Y),
					new Point (Start.X + Width, Start.Y),
					new Point (Start.X + Width, Start.Y + Height),
					new Point (Start.X, Start.Y + Height)};
			}
		}

		public Point[] VerticesCenter {
			get {
				Point [] points = Vertices;

				points[0].X += Width / 2;
				points[1].Y += Height / 2;
				points[2].X = points[0].X;
				points[3].Y = points[1].Y;
				return points;
			}
		}
	}
}

