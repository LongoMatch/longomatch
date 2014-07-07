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
using System.Linq;
using System.Collections.Generic;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;
using LongoMatch.Drawing.CanvasObject;

namespace LongoMatch.Drawing
{
	public class Canvas
	{
		protected IDrawingToolkit tk;
		protected IWidget widget;
		
		public Canvas (IWidget widget)
		{
			this.widget = widget;
			tk = Config.DrawingToolkit;
			Objects = new List<ICanvasObject>();
			widget.DrawEvent += HandleDraw;
		}
		
		public List<ICanvasObject> Objects {
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
		
		protected virtual void HandleDraw (object context, Area area) {
			tk.Context = context;
			foreach (ICanvasObject o in Objects) {
				if (o.Visible) {
					o.Draw (tk, area);
				}
			}
			tk.Context = null;
		}
	}
	
	public abstract class SelectionCanvas: Canvas
	{
		protected bool moving;
		protected Point start; 
		uint lastTime;
		
		public SelectionCanvas (IWidget widget): base (widget) {
			Selections = new List<Selection>();
			widget.ButtonPressEvent += HandleButtonPressEvent;
			widget.ButtonReleasedEvent += HandleButtonReleasedEvent;
			widget.MotionEvent += HandleMotionEvent;
		}
		
		public double Accuracy {
			get;
			set;
		}
		
		public bool MultipleSelection {
			get;
			set;
		}
		
		protected List<Selection> Selections {
			get;
			set;
		}
		
		protected abstract void StartMove (Selection sel);
		protected abstract void SelectionMoved (Selection sel);
		protected abstract void StopMove ();
		protected abstract void ItemSelected (Selection sel);
		protected abstract void ShowMenu (Point coords);
		
		void ClearSelection () {
			foreach (Selection sel in Selections) {
				ICanvasSelectableObject po = sel.Drawable as ICanvasSelectableObject;
				po.Selected = false;
				widget.ReDraw (po);
			}
			Selections.Clear ();
		}
		
		void UpdateSelection (Selection sel) {
			ICanvasSelectableObject so = sel.Drawable as ICanvasSelectableObject;
			Selection seldup = Selections.FirstOrDefault (s => s.Drawable == sel.Drawable);
			
			if (seldup != null) {
				so.Selected = false;
				Selections.Remove (seldup);
			} else {
				so.Selected = true;
				Selections.Add (sel);
				ItemSelected (sel);
			}
			widget.ReDraw (so);
		}
		
		protected virtual void HandleLeftButton (Point coords, ButtonModifier modif) {
			Selection sel = null;

			foreach (object o in Objects) {
				ICanvasSelectableObject co = o as ICanvasSelectableObject;
				sel = co.GetSelection (coords, Accuracy);
				if (sel != null) {
					break;
				}
			}

			if (MultipleSelection && (modif == ButtonModifier.Control ||
			                          modif == ButtonModifier.Shift)) {
				if (sel != null) {
					sel.Position = SelectionPosition.All;
					UpdateSelection (sel);
				}
			} else {
				ClearSelection ();
				if (sel == null) {
					return;
				}
				moving = true;
				start = coords;
				UpdateSelection (sel);
				StartMove (sel);
			}
		}
		
		protected virtual void HandleRightButton (Point coords, ButtonModifier modif) {
			ShowMenu (coords);
		}
		
		void HandleMotionEvent (Point coords)
		{
			Selection sel;

			if (!moving)
				return;
			
			sel = Selections[0];
			sel.Drawable.Move (sel, coords, start);  
			widget.ReDraw (sel.Drawable);
			SelectionMoved (sel);
			start = coords;
		}

		void HandleButtonReleasedEvent (Point coords, ButtonType type, ButtonModifier modifier)
		{
			moving = false;
			StopMove ();
		}

		void HandleButtonPressEvent (Point coords, uint time, ButtonType type, ButtonModifier modifier)
		{
			if (time - lastTime < 500) {
				return;
			}
			if (type == ButtonType.Left) {
				HandleLeftButton (coords, modifier);
			} else if (type == ButtonType.Right) {
				HandleRightButton (coords, modifier);
			}
			lastTime = time;
		}
		
	}
}

