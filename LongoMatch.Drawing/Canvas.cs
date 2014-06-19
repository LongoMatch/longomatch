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
		protected double scaleX, scaleY;
		protected Point translation;
		
		public Canvas (IWidget widget)
		{
			this.widget = widget;
			tk = Config.DrawingToolkit;
			Objects = new List<ICanvasObject>();
			widget.DrawEvent += HandleDraw;
			scaleX = 1;
			scaleY = 1;
			translation = new Point (0, 0);
		}
		
		public List<ICanvasObject> Objects {
			get;
			set;
		}
		
		protected Point ToUserCoords (Point p) {
			return new Point ((p.X - translation.X) / scaleX,
			                  (p.Y - translation.Y) / scaleY);
		
		}
		
		protected virtual void HandleDraw (object context, Area area) {
			tk.Context = context;
			tk.TranslateAndScale (translation, new Point (scaleX, scaleY));
			tk.Begin ();
			for (int i=Objects.Count - 1; i >= 0; i--) {
				ICanvasObject o = Objects[i];
				if (o.Visible) {
					o.Draw (tk, area);
				}
			}
			tk.End ();
			tk.Context = null;
		}
	}
	
	public class SelectionCanvas: Canvas
	{
		protected bool moving;
		protected Point start; 
		uint lastTime;
		
		public SelectionCanvas (IWidget widget): base (widget) {
			Selections = new List<Selection>();
			SelectionMode = MultiSelectionMode.Single;
			Accuracy = 1;
			
			widget.ButtonPressEvent += HandleButtonPressEvent;
			widget.ButtonReleasedEvent += HandleButtonReleasedEvent;
			widget.MotionEvent += HandleMotionEvent;
			widget.ShowTooltipEvent += HandleShowTooltipEvent;
		}
		
		public double Accuracy {
			get;
			set;
		}
		
		public MultiSelectionMode SelectionMode {
			get;
			set;
		}
		
		protected List<Selection> Selections {
			get;
			set;
		}
		
		protected virtual void StartMove (Selection sel) {
		}
		
		protected virtual void SelectionMoved (Selection sel) {
		}
		
		protected virtual void StopMove () {
		}
		
		protected virtual void SelectionChanged (List<Selection> sel) {
		}
		
		protected virtual void ShowMenu (Point coords) {
		}
		
		public void ClearSelection () {
			foreach (Selection sel in Selections) {
				ICanvasSelectableObject po = sel.Drawable as ICanvasSelectableObject;
				po.Selected = false;
				widget.ReDraw (po);
			}
			foreach (ICanvasSelectableObject cso in Objects) {
				cso.Selected = false;
			}
			widget.ReDraw ();
			Selections.Clear ();
		}
		
		protected void UpdateSelection (Selection sel, bool notify=true) {
			ICanvasSelectableObject so = sel.Drawable as ICanvasSelectableObject;
			Selection seldup = Selections.FirstOrDefault (s => s.Drawable == sel.Drawable);
			
			if (seldup != null) {
				so.Selected = false;
				Selections.Remove (seldup);
			} else {
				so.Selected = true;
				Selections.Add (sel);
			}
			if (notify) {
				SelectionChanged (Selections);
			}
			widget.ReDraw (so);
		}

		Selection GetSelection (Point coords)
		{
			Selection sel = null;

			/* Try with the selected item first */if (Selections.Count > 0) {
				sel = Selections.LastOrDefault ().Drawable.GetSelection (coords, Accuracy);
			}
			if (sel == null) {
				foreach (ICanvasSelectableObject co in Objects) {
					sel = co.GetSelection (coords, Accuracy);
					if (sel != null) {
						break;
					}
				}
			}
			return sel;
		}
		
		void HandleShowTooltipEvent (Point coords)
		{
			Selection sel = GetSelection (ToUserCoords (coords)); 
			if (sel != null) {
				ICanvasObject co = sel.Drawable as ICanvasObject;
				if (co.Description != null) {
					widget.ShowTooltip (co.Description);
				}
			}
		}
		
		void HandleLeftButton (Point coords, ButtonModifier modif) {
			Selection sel;
			
			sel = GetSelection (coords);

			if ((SelectionMode == MultiSelectionMode.Multiple) ||
			    (SelectionMode == MultiSelectionMode.MultipleWithModifier &&
			    (modif == ButtonModifier.Control ||
			    modif == ButtonModifier.Shift)))
			{
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
		
		void HandleRightButton (Point coords, ButtonModifier modif) {
			ShowMenu (coords);
		}
		
		void HandleMotionEvent (Point coords)
		{
			Selection sel;

			if (!moving)
				return;
			
			coords = ToUserCoords (coords); 
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
			coords = ToUserCoords (coords); 
			if (type == ButtonType.Left) {
				HandleLeftButton (coords, modifier);
			} else if (type == ButtonType.Right) {
				HandleRightButton (coords, modifier);
			}
			lastTime = time;
		}
	}
	
	public abstract class BackgroundCanvas: SelectionCanvas
	{

		Image background;

		public BackgroundCanvas (IWidget widget): base (widget) {
			widget.SizeChangedEvent += HandleSizeChangedEvent;
		}

		public Image Background {
			set {
				background = value;
				HandleSizeChangedEvent ();
			}
			get {
				return background;
			}
		}
		
		protected virtual void HandleSizeChangedEvent ()
		{
			if (background != null) {
				background.ScaleFactor ((int) widget.Width, (int) widget.Height, out scaleX,
				                        out scaleY, out translation);
			}
		}
		
		protected override void HandleDraw (object context, Area area)
		{
			if (Background != null) {
				tk.Context = context;
				tk.Begin ();
				tk.TranslateAndScale (translation, new Point (scaleX, scaleY));
				tk.DrawImage (Background);
				tk.End ();
			}
			base.HandleDraw (context, area);
		}
	}
}
