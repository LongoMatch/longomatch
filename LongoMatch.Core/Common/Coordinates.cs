// 
//  Copyright (C) 2013 Andoni Morales Alastruey
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

namespace LongoMatch.Common
{
	[Serializable]
	public class Coordinates: List<Point>
	{
		public Coordinates ()
		{
		}
		
		public override bool Equals (object obj)
		{
			Coordinates c = obj as Coordinates;
            if (c == null)
				return false;
				
			if (c.Count != Count)
				return false;
			
			for (int i=0; i<Count; i++) {
				if (c[i] != this[i])
					return false;
			}
			return true;
		}
		
		public override int GetHashCode ()
		{
			string s = "";
			
			for (int i=0; i<Count; i++) {
				s += this[i].X.ToString() +  this[i].Y.ToString();
			}
			
			return int.Parse(s);
		}
	}
	
	[Serializable]
	public class Point {

		public Point (double x, double y) {
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
		
		public double Distance (Point p) {
			return Math.Sqrt (Math.Pow (this.X - p.X, 2) - Math.Pow (this.Y - Y, 2));
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
			return (X.ToString() + "-" + Y.ToString()).GetHashCode();
		}
		
		public static bool operator < (Point p1, Point p2) {
			return p1.X < p2.X && p1.Y < p2.Y;
		}
		
		public static bool operator > (Point p1, Point p2) {
			return p1.X > p2.X && p1.Y > p2.Y;
		}
	}
}

