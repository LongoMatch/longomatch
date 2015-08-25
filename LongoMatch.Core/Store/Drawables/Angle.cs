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
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Store.Drawables
{
	[Serializable]
	public class Angle: Drawable
	{
		public Angle ()
		{
		}

		public Angle (Point start, Point center, Point stop)
		{
			Start = start;
			Center = center;
			Stop = stop;
		}

		public Point Start {
			get;
			set;
		}

		public Point Center {
			get;
			set;
		}

		public Point Stop {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public double Degrees {
			get {
				double a = Math.Atan2 (Stop.Y, Stop.X) - Math.Atan2 (Start.Y, Start.X);
				if (a < 0) {
					a += 2 * Math.PI;
				}
				return a;
			}
		}

		public override Selection GetSelection (Point p, double pr = 0.05, bool inMotion = false)
		{
			if (p.Distance (Start) < pr) {
				return new Selection (this, SelectionPosition.AngleStart, p.Distance (Start));
			} else if (p.Distance (Stop) < pr) {
				return new Selection (this, SelectionPosition.AngleStop, p.Distance (Stop));
			} else if (p.Distance (Center) < pr) {
				return new Selection (this, SelectionPosition.AngleCenter, p.Distance (Center));
			} else {
				return null;
			}
		}

		public override void Move (Selection sel, Point p, Point start)
		{
			switch (sel.Position) {
			case SelectionPosition.AngleStart:
				Start = p;
				break;
			case SelectionPosition.AngleStop:
				Stop = p;
				break;
			case SelectionPosition.AngleCenter:
				Center = p;
				break;
			default:
				throw new Exception ("Unsupported move for angle:  " + sel.Position);
			}
		}
	}
}

