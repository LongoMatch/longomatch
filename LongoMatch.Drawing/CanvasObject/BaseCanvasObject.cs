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
using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObject
{
	public abstract class BaseCanvasObject: ICanvasObject
	{
		public BaseCanvasObject ()
		{
			Visible = true;
		}
		
		public virtual string Description {
			get;
			set;
		}
		
		public bool Visible {
			get;
			set;
		}
		
		public bool Selected {
			set;
			get;
		}
		
		public abstract void Draw (IDrawingToolkit tk, Area area);
	}
	
	public abstract class BaseCanvasDrawableObject<T>: BaseCanvasObject where T:Drawable
	{
		public T Drawable {
			get;
			set;
		}
		
		public Canvas Parent {
			get;
			set;
		}
		
		public Selection GetSelection (Point point, double precision) {
			return Drawable.GetSelection (point, precision);
		}
		
		public void Move (Selection s, Point p, Point start) {
			Drawable.Move (s, p, start);
		}
	}
}