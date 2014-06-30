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
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObject
{
	public abstract class TimelineObject: CanvasObject, ICanvasSelectableObject
	{
		Color background;
		List<TimeNodeObject> nodes;
		double secondsPerPixel;
		protected Time maxTime;
		
		public TimelineObject (Time maxTime, double offsetY, Color background)
		{
			this.background = background;
			this.nodes = new List<TimeNodeObject> ();
			this.maxTime = maxTime;
			Visible = true;
			CurrentTime = new Time (0);
			OffsetY  = offsetY;
			SecondsPerPixel = 0.1;
		}
		
		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				foreach (TimeNodeObject to in nodes) {
					to.SecondsPerPixel = secondsPerPixel;
				}
			}
			protected get {
				return secondsPerPixel;
			}
		}
		
		public Time CurrentTime {
			set;
			protected get;
		}
		
		public double Width {
			set;
			protected get;
		}
		
		public double OffsetY {
			set;
			get;
		}
		
		public void AddNode (TimeNodeObject o) {
			nodes.Add (o);
		}
		
		public void RemoveNode (TimeNode node) {
			nodes.RemoveAll (po => po.TimeNode == node);
		}

		public override void Draw (IDrawingToolkit tk, Area area) {
			double position;
			List<TimeNodeObject> selected;
			
			selected = new List<TimeNodeObject>();

			tk.Begin ();
			tk.FillColor = background;
			tk.StrokeColor = background;
			tk.LineWidth = 1;
			tk.DrawRectangle (new Point (0, OffsetY), Width,
			                  Constants.CATEGORY_HEIGHT);
			foreach (TimeNodeObject p in nodes) {
				if (p.Selected) {
					selected.Add (p);
					continue;
				}
				p.Draw (tk, area);
			}
			foreach (TimeNodeObject p in selected) {
				p.Draw (tk, area);
			}
			
			tk.FillColor = Constants.TIMELINE_LINE_COLOR;
			tk.StrokeColor = Constants.TIMELINE_LINE_COLOR;
			tk.LineWidth = Constants.TIMELINE_LINE_WIDTH;
			position = Utils.TimeToPos (CurrentTime, secondsPerPixel);
			tk.DrawLine (new Point (position, OffsetY),
			             new Point (position, OffsetY + Constants.CATEGORY_HEIGHT));
			
			tk.End();
		}
		
		public Selection GetSelection (Point point, double precision) {
			Selection selection = null;

			if (point.Y >= OffsetY && point.Y < OffsetY + Constants.CATEGORY_HEIGHT) {
				foreach (TimeNodeObject po in nodes) {
					Selection tmp;
					tmp = po.GetSelection (point, precision);
					if (tmp == null) {
						continue;
					}
					if (tmp.Accuracy == 0) {
						selection = tmp;
						break;
					}
					if (selection == null || tmp.Accuracy < selection.Accuracy) {
						selection = tmp;
					}
				}
			}
			if (selection != null) {
				(selection.Drawable as ICanvasSelectableObject).Selected = true;
			}
			return selection;
		}
		
		public void Move (Selection s, Point p, Point start) {
			s.Drawable.Move (s, p, start);
		}
	}
	
	public class CategoryTimeline: TimelineObject {

		public CategoryTimeline (List<Play> plays, Time maxTime, double offsetY, Color background):
			base (maxTime, offsetY, background)
		{
			foreach (Play p in plays) {
				AddPlay (p);
			}
		}
		
		public void AddPlay (Play play){
			PlayObject po = new PlayObject (play);
			po.OffsetY = OffsetY;
			po.SecondsPerPixel = SecondsPerPixel;
			po.MaxTime = maxTime;
			AddNode (po);
		}

	}
	
	public class TimerTimeline: TimelineObject {

		public TimerTimeline (List<Timer> timers, Time maxTime, double offsetY, Color background):
			base (maxTime, offsetY, background)
		{
			foreach (Timer t in timers) {
				foreach (TimeNode tn in t.Nodes) {
					TimeNodeObject to = new TimeNodeObject (tn);
					to.OffsetY = OffsetY;
					to.SecondsPerPixel = SecondsPerPixel;
					to.MaxTime = maxTime;
					to.SelectWhole = false;
					AddNode (to);
				}
			}
		}
	}
}

