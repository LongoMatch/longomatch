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
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LongoMatch.Core.Common
{
	
	[Serializable]
	public class Point
	{

		public Point (double x, double y)
		{
			X = x;
			Y = y;
		}

		public double X {
			get;
			set;
		}

		public double Y {
			get;
			set;
		}

		public double Distance (Point p)
		{
			return Math.Sqrt (Math.Pow (X - p.X, 2) + Math.Pow (Y - p.Y, 2));
		}

		public Point Normalize (int width, int height)
		{
			return new Point (Math.Min (X, width) / width,
				Math.Min (Y, height) / height);
		}

		public Point Denormalize (int width, int height)
		{
			return new Point (X * width, Y * height);
		}

		public Point Copy ()
		{
			return new Point (X, Y);
		}

		public override string ToString ()
		{
			return string.Format ("[Point: X={0}, Y={1}]", X, Y);
		}

		public override bool Equals (object obj)
		{
			Point p = obj as Point;
			if (p == null)
				return false;
				
			return p.X == X && p.Y == Y;
		}

		public override int GetHashCode ()
		{
			return (X.ToString () + "-" + Y.ToString ()).GetHashCode ();
		}

		public static Point operator + (Point p1, Point p2)
		{
			return new Point (p1.X + p2.X, p1.Y + p2.Y);
		}

		public static Point operator - (Point p1, Point p2)
		{
			return new Point (p1.X - p2.X, p1.Y - p2.Y);
		}
	}
}
