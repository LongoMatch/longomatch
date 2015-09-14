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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Store.Drawables
{
	[Serializable]
	public class MultiPoints: Rectangle
	{
		ObservableCollection<Point> points;
		
		public MultiPoints ()
		{
		}
		
		public MultiPoints (List<Point> points)
		{
			Points = new ObservableCollection<Point> (points);
		}
		
		public ObservableCollection<Point> Points {
			get {
				return points;
			}
			set {
				if (points != null) {
					points.CollectionChanged -= ListChanged;
				}
				points = value;
				if (points != null) {
					points.CollectionChanged += ListChanged;
				}
				UpdateArea ();
			}
		}
		
		public override Selection GetSelection (Point p, double pr, bool inMotion=false)
		{
			Selection s = base.GetSelection (p, pr);
			if (s != null) {
				s.Position = SelectionPosition.All;
			}
			return s;
		}
	
		public override void Move (Selection sel, Point p, Point moveStart) {
			switch (sel.Position) {
			case SelectionPosition.All: {
				double xdiff, ydiff;
				
				xdiff = p.X - moveStart.X;
				ydiff = p.Y - moveStart.Y;
				foreach (Point point in Points) {
					point.X += xdiff;
					point.Y += ydiff;
				}
				break;
			}
			default:
				throw new Exception ("Unsupported move for multipoints:  " + sel.Position);
			}
		}
		
		void UpdateArea () {
			double xmin, xmax, ymin, ymax;
			List<Point> px, py;

			if (Points == null) {
				TopLeft = null;
				TopRight = null;
				BottomLeft = null;
				BottomRight = null;
				return;
			}

			px = Points.OrderBy (p => p.X).ToList();
			py = Points.OrderBy (p => p.X).ToList();
			xmin = px[0].X;
			xmax = px[px.Count-1].X;
			ymin = py[0].Y;
			ymax = py[py.Count-1].Y; 
			TopLeft = new Point (xmin, ymin);
			TopRight = new Point (xmax, ymin);
			BottomLeft = new Point (xmin, ymax);
			BottomRight = new Point (xmax, ymax);
		} 

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}
}

