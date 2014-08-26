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
using Mono.Unix;

namespace LongoMatch.Drawing.Widgets
{
	public class CategoriesLabels: Canvas
	{
		Project project;
		PlaysFilter filter;
		Dictionary<TaggerButton, CategoryLabel> categories;

		public CategoriesLabels (IWidget widget): base (widget)
		{
			categories = new Dictionary<TaggerButton, CategoryLabel> ();
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
			ClearObjects ();
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
			CategoryLabel l;
			int i = 0, w, h;
			
			w = StyleConf.TimelineLabelsWidth;
			h = StyleConf.TimelineCategoryHeight;
			widget.Width = w;
			
			/* Add the scores label */
			if (project.Categories.Scores.Count > 0) {
				l = new CategoryLabel (new TaggerButton { Name = Catalog.GetString ("Score") },
				                       w, h, i * h);
				Objects.Add (l);
				i++;
				foreach (Score s in project.Categories.Scores) {
					categories [s] = l;
				}
			}
			
			/* Add the penalty cards label */
			if (project.Categories.PenaltyCards.Count > 0) {
				l = new CategoryLabel (new TaggerButton {Name = Catalog.GetString ("Penalty cards")},
				                       w, h, i * h);
				Objects.Add (l);
				i++;
				foreach (PenaltyCard pc in project.Categories.PenaltyCards) {
					categories [pc] = l;
				}
			}

			/* Start from bottom to top  with categories */
			foreach (TaggerButton cat in project.Categories.CategoriesList) {
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

			foreach (CategoryLabel ct in categories.Values) {
				ct.Visible = false;
				ct.OffsetY = -1;
			}

			foreach (TaggerButton cat in categories.Keys) {
				CategoryLabel label = categories [cat];

				if (filter.VisibleCategories.Contains (cat)) {
					label.Visible |= true;
					if (label.OffsetY == -1) {
						label.OffsetY = i * label.Height;
						if (i % 2 == 0) {
							label.Even = true;
						}
						i++;
					}
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

