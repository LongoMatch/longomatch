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
	public class CategoryTimeline: BaseCanvasObject, ICanvasSelectableObject
	{
		Color background;
		List<PlayObject> plays;
		double secondsPerPixel;
		Time maxTime;
		
		public CategoryTimeline (List<Play> plays, Time maxTime, double offsetY, Color background)
		{
			this.background = background;
			this.plays = new List<PlayObject> ();
			this.maxTime = maxTime;
			Visible = true;
			CurrentTime = new Time (0);
			OffsetY  = offsetY;
			foreach (Play p in plays) {
				AddPlay (p);
			}
			SecondsPerPixel = 0.1;
		}
		
		public double SecondsPerPixel {
			set {
				secondsPerPixel = value;
				foreach (PlayObject po in plays) {
					po.SecondsPerPixel = secondsPerPixel;
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
		
		public void AddPlay (Play play) {
			PlayObject po = new PlayObject (play);
			po.OffsetY = OffsetY;
			po.SecondsPerPixel = SecondsPerPixel;
			po.MaxTime = maxTime;
			plays.Add (po);
		}
		
		public void RemovePlay (Play play) {
			plays.RemoveAll (po => po.Play == play);
		}

		public override void Draw (IDrawingToolkit tk, Area area) {
			double position;
			List<PlayObject> selected;
			
			selected = new List<PlayObject>();

			tk.Begin ();
			tk.FillColor = background;
			tk.StrokeColor = background;
			tk.LineWidth = 1;
			tk.DrawRectangle (new Point (0, OffsetY), Width,
			                  Common.CATEGORY_HEIGHT);
			foreach (PlayObject p in plays) {
				if (p.Selected) {
					selected.Add (p);
					continue;
				}
				p.Draw (tk, area);
			}
			foreach (PlayObject p in selected) {
				p.Draw (tk, area);
			}
			
			tk.FillColor = Common.TIMELINE_LINE_COLOR;
			tk.StrokeColor = Common.TIMELINE_LINE_COLOR;
			tk.LineWidth = Common.TIMELINE_LINE_WIDTH;
			position = Common.TimeToPos (CurrentTime, secondsPerPixel);
			tk.DrawLine (new Point (position, OffsetY),
			             new Point (position, OffsetY + Common.CATEGORY_HEIGHT));
			
			tk.End();
		}
		
		public Selection GetSelection (Point point, double precision) {
			Selection selection = null;

			if (point.Y >= OffsetY && point.Y < OffsetY + Common.CATEGORY_HEIGHT) {
				foreach (PlayObject po in plays) {
					Selection tmp;
					tmp = po.GetSelection (point, precision);
					if (tmp == null) {
						continue;
					}
					if (tmp.Position != SelectionPosition.None) {
						if (tmp.Accuracy == 0) {
							selection = tmp;
							break;
						}
						if (selection == null || tmp.Accuracy < selection.Accuracy) {
							selection = tmp;
						}
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
}

