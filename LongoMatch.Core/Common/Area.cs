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
using Newtonsoft.Json;

namespace LongoMatch.Core.Common
{
	[Serializable]
	[JsonObject (MemberSerialization.OptIn)]
	public class Area
	{
		public Area ()
		{
			Start = new Point (0, 0);
			Width = 0;
			Height = 0;
		}

		public Area (double x, double y, double width, double height)
		{
			Start = new Point (x, y);
			Width = width;
			Height = height;
		}

		public Area (Point start, double width, double height)
		{
			Start = start;
			Width = width;
			Height = height;
		}

		[JsonProperty]
		public Point Start {
			get;
			set;
		}

		[JsonProperty]
		public double Width {
			get;
			set;
		}

		[JsonProperty]
		public double Height {
			get;
			set;
		}

		public double Left {
			get {
				return Start.X;
			}
		}

		public double Top {
			get {
				return Start.Y;
			}
		}

		public double Right {
			get {
				return Start.X + Width;
			}
		}

		public double Bottom {
			get {
				return Start.Y + Height;
			}
		}

		public Point TopLeft {
			get {
				return new Point (Left, Top);
			}
		}

		public Point TopRight {
			get {
				return new Point (Right, Top);
			}
		}

		public Point BottomLeft {
			get {
				return new Point (Left, Bottom);
			}
		}

		public Point BottomRight {
			get {
				return new Point (Right, Bottom);
			}
		}

		public Point Center {
			get {
				return new Point (Start.X + Width / 2, Start.Y + Height / 2);
			}
		}

		public Point[] Vertices {
			get {
				return new Point[] {
					TopLeft,
					TopRight,
					BottomRight,
					BottomLeft,
				};
			}
		}

		public Point[] VerticesCenter {
			get {
				Point[] points = Vertices;

				points [0].X += Width / 2;
				points [1].Y += Height / 2;
				points [2].X = points [0].X;
				points [3].Y = points [1].Y;
				return points;
			}
		}

		public bool IntersectsWith (Area area)
		{
			return !((Left >= area.Right) || (Right <= area.Left) ||
			(Top >= area.Bottom) || (Bottom <= area.Top));
		}

		public override bool Equals (object obj)
		{
			Area a = obj as Area;
			if (a == null)
				return false;
			if (a.Start != Start ||
			    a.Width != Width || a.Height != Height) {
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public static bool operator == (Area a1, Area a2)
		{
			if (Object.ReferenceEquals (a1, a2)) {
				return true;
			}

			if ((object)a1 == null || (object)a2 == null) {
				return false;
			}

			return a1.Equals (a2);
		}

		public static bool operator != (Area a1, Area a2)
		{
			return !(a1 == a2);
		}

		public override string ToString ()
		{
			return string.Format ("[Area: Start={0}, Width={1}, Height={2}]", Start, Width, Height);
		}
	}
}

