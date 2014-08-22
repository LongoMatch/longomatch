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
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Common;
using LongoMatch.Drawing.CanvasObjects;

namespace LongoMatch.Drawing.Widgets
{
	public class CategoriesLabels: Canvas
	{
		Project project;
		PlaysFilter filter;
		Dictionary<Category, CategoryLabel> categories;

		public CategoriesLabels (IWidget widget): base (widget)
		{
			categories = new Dictionary<Category, CategoryLabel> ();
		}

		public double Scroll {
			set {
				foreach (var o in Objects) {
					CategoryLabel cl = o as CategoryLabel;
					cl.Scroll = value; 
				}
			}
		}

		public void LoadProject (Project project, PlaysFilter filter)
		{
			Objects.Clear ();
			this.project = project;
			this.filter = filter;
			if (project != null) {
				FillCanvas ();
				UpdateVisibleCategories ();
				filter.FilterUpdated += UpdateVisibleCategories;
			}
		}

		void FillCanvas ()
		{
			int i = 0, w, h;
			
			w = StyleConf.TimelineLabelsWidth;
			h = StyleConf.TimelineCategoryHeight;
			widget.Width = w;
			
			/* Start from bottom to top  with categories */
			foreach (Category cat in project.Categories.CategoriesList) {
				CategoryLabel l;
				
				/* Add the category label */
				l = new CategoryLabel (cat, w, h, i * h);
				categories [cat] = l;
				Objects.Add (l);
				i++;
			}
		}

		void UpdateVisibleCategories ()
		{
			int i = 0;

			foreach (Category cat in categories.Keys) {
				CategoryLabel label = categories [cat];

				if (filter.VisibleCategories.Contains (cat)) {
					label.OffsetY = i * label.Height;
					label.Visible = true;
					if (i % 2 == 0) {
						label.Even = true;
					}
					i++;
				} else {
					label.Visible = false;
				}
			}
			widget.ReDraw ();
		}
		
		public override void Draw (IContext context, Area area)
		{
			tk.Context = context;
			tk.Begin ();
			tk.Clear (Config.Style.PaletteBackground);
			tk.End ();

			base.Draw (context, area);
		}
	}
}

