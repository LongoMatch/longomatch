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
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store;
using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObject
{
	public class CategoryObject: BaseCanvasObject, ICanvasSelectableObject
	{
		public CategoryObject (Category category)
		{
			Category = category;
		}
		
		public Category Category {
			get;
			set;
		}
		
		public Point Position {
			get;
			set;
		}
		
		public double Width {
			get;
			set;
		}
		
		public double Height {
			get;
			set;
		}
		
		public int NRows {
			get {
				/* Header + recoder */
				int rows = 2;
				foreach (SubCategory subcat in Category.SubCategories) {
					rows += subcat.Options.Count / Common.CATEGORY_SUBCATEGORIES_COLUMNS + 1;
				} 
				return rows;
			}
		}
		
		public override void Draw (IDrawingToolkit tk, Area area) {
			double heightPerRow;
			double ptr = 0;
			
			tk.Begin();
			heightPerRow = NRows / Height;

			/* Draw header */
			tk.DrawRectangle (Position, Width,  heightPerRow);
			tk.DrawText (Position, Width, heightPerRow, Category.Name);
			ptr += heightPerRow;
			/* Draw Tagger */
			
			tk.End();
		}
		
		public Selection GetSelection (Point point, double precision) {
			return null;
		}

		public void Move (Selection s, Point p, Point start) {
		}

	}
}

