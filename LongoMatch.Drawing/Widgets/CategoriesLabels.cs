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
using LongoMatch.Store;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Common;
using LongoMatch.Drawing.CanvasObject;

namespace LongoMatch.Drawing.Widgets
{
	public class CategoriesLabels: Canvas
	{
		Project project;

		public CategoriesLabels (IWidget widget): base (widget)
		{
		}
		
		public double Scroll {
			set {
				foreach (var o in Objects) {
					CategoryLabel cl = o as CategoryLabel;
					cl.Scroll = value; 
				}
			}
		}
		
		public Project Project {
			set {
				Objects.Clear ();
				project = value;
				if (project != null)
					FillCanvas ();
			}
		}
		
		void FillCanvas () {
			Point offset;
			
			widget.Width = Common.CATEGORY_WIDTH;
			
			offset = new Point (0, 0);
			
			/* Start from bottom to top  with categories */
			foreach (Category cat in project.Categories) {
				CategoryLabel l;
				Point cOffset;
				
				/* Add the category label */
				cOffset = new Point (offset.X, offset.Y);
				l = new CategoryLabel (cat, Common.CATEGORY_WIDTH,
				                       Common.CATEGORY_HEIGHT, cOffset);
				Objects.Add (l);
				offset.Y += Common.CATEGORY_HEIGHT;
			}
		}
	}
}

