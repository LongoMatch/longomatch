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
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Drawing.CanvasObjects;

namespace LongoMatch.Drawing
{
	public class Canvas: ICanvas
	{
		protected IDrawingToolkit tk;
		protected IWidget widget;
		protected double scaleX, scaleY;
		protected Point translation;
		bool disposed;

		public Canvas (IWidget widget)
		{
			this.widget = widget;
			tk = Config.DrawingToolkit;
			Objects = new List<ICanvasObject> ();
			widget.DrawEvent += Draw;
			widget.SizeChangedEvent += HandleSizeChangedEvent;
			scaleX = 1;
			scaleY = 1;
			translation = new Point (0, 0);
		}

		~ Canvas ()
		{
			if (! disposed) {
				Log.Error (String.Format ("Canvas {0} was not disposed correctly", this));
				Dispose (true);
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			// FIXME: Should we check if we are disposed already ?
			if (disposing) {
				ClearObjects ();
				Objects = null;
				disposed = true;
			}
		}

		protected virtual void ClearObjects ()
		{
			if (Objects != null) {
				foreach (ICanvasObject co in Objects) {
					co.RedrawEvent -= HandleRedrawEvent;
					co.Dispose ();
				}
				Objects.Clear ();
			}
		}

		public List<ICanvasObject> Objects {
			get;
			set;
		}

		public void AddObject (ICanvasObject co)
		{
			Objects.Add (co);
			co.RedrawEvent += HandleRedrawEvent;
		}

		public void RemoveObject (ICanvasObject co)
		{
			co.RedrawEvent -= HandleRedrawEvent;
			Objects.Remove (co);
		}

		protected Point ToUserCoords (Point p)
		{
			return new Point ((p.X - translation.X) / scaleX,
			                  (p.Y - translation.Y) / scaleY);
		
		}

		void HandleRedrawEvent (ICanvasObject co, Area area)
		{
			widget.ReDraw (area);
		}

		void HandleSizeChangedEvent ()
		{
			foreach (CanvasObject to in Objects) {
				to.ResetDrawArea ();
			}
		}

		public virtual void Draw (IContext context, Area area)
		{
			List<CanvasObject> highlighted = new List<CanvasObject> ();
			tk.Context = context;
			tk.Begin ();
			tk.TranslateAndScale (translation, new Point (scaleX, scaleY));
			foreach (ICanvasObject co in Objects) {
				if (co.Visible) {
					if (co is ICanvasSelectableObject) {
						if ((co as ICanvasSelectableObject).Selected) {
							continue;
						}
						if ((co as CanvasObject).Highlighted) {
							highlighted.Add (co as CanvasObject);
							continue;
						}
					}
					co.Draw (tk, area);
				}
			}
			foreach (ICanvasSelectableObject co in Objects.OfType<ICanvasSelectableObject>()) {
				if (co.Selected && co.Visible) {
					co.Draw (tk, area);
				}
			}
			foreach (CanvasObject co in highlighted) {
				co.Draw (tk, area);
			}
			tk.End ();
			tk.Context = null;
		}
	}

	public class SelectionCanvas: Canvas
	{
		protected bool moving, moved;
		protected Point start;
		uint lastTime;
		Selection clickedSel;
		CanvasObject highlighted;

		public SelectionCanvas (IWidget widget): base (widget)
		{
			Selections = new List<Selection> ();
			SelectionMode = MultiSelectionMode.Single;
			Accuracy = 1;
			ClickRepeatMS = 100;
			MoveWithoutSelection = false;
			ObjectsCanMove = true;
			SingleSelectionObjects = new List<Type> ();
			
			widget.ButtonPressEvent += HandleButtonPressEvent;
			widget.ButtonReleasedEvent += HandleButtonReleasedEvent;
			widget.MotionEvent += HandleMotionEvent;
			widget.ShowTooltipEvent += HandleShowTooltipEvent;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				widget.Dispose ();
			base.Dispose (disposing);
		}

		protected override void ClearObjects ()
		{
			// Make sure we don't maintain a selection with invalid objects.
			ClearSelection ();
			base.ClearObjects ();
		}

		public int ClickRepeatMS {
			get;
			set;
		}

		public double Accuracy {
			get;
			set;
		}

		public MultiSelectionMode SelectionMode {
			get;
			set;
		}

		public List<Type> SingleSelectionObjects {
			get;
			set;
		}

		public bool ObjectsCanMove {
			get;
			set;
		}

		protected bool MoveWithoutSelection {
			get;
			set;
		}

		protected List<Selection> Selections {
			get;
			set;
		}

		protected virtual void StartMove (Selection sel)
		{
		}

		protected virtual void CursorMoved (Point coords)
		{
			CanvasObject current;
			Selection sel;

			sel = GetSelection (coords, true);
			if (sel == null) {
				current = null;
			} else {
				current = sel.Drawable as CanvasObject;
			}

			if (current != highlighted) {
				if (highlighted != null) {
					highlighted.Highlighted = false;
				}
				if (current != null) {
					current.Highlighted = true;
				}
				highlighted = current;
			}
		}

		protected virtual void SelectionMoved (Selection sel)
		{
		}

		protected virtual void StopMove (bool moved)
		{
		}

		protected virtual void SelectionChanged (List<Selection> sel)
		{
		}

		protected virtual void ShowMenu (Point coords)
		{
		}

		public void ClearSelection ()
		{
			foreach (Selection sel in Selections) {
				ICanvasSelectableObject po = sel.Drawable as ICanvasSelectableObject;
				po.Selected = false;
			}
			if (Objects != null) {
				foreach (ICanvasSelectableObject cso in Objects) {
					cso.Selected = false;
				}
			}
			Selections.Clear ();
		}

		protected void UpdateSelection (Selection sel, bool notify=true)
		{
			ICanvasSelectableObject so;
			Selection seldup;

			if (sel == null) {
				ClearSelection ();
				if (notify) {
					SelectionChanged (Selections);
				}
				return;
			}

			so = sel.Drawable as ICanvasSelectableObject;
			if (Selections.Count > 0) {
				if (SingleSelectionObjects.Contains (so.GetType ()) ||
					SingleSelectionObjects.Contains (Selections [0].Drawable.GetType ())) {
					return;
				}
			}

			seldup = Selections.FirstOrDefault (s => s.Drawable == sel.Drawable);
			
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
		}

		Selection GetSelection (Point coords, bool inMotion=false)
		{
			Selection sel = null;

			/* Try with the selected item first */
			if (Selections.Count > 0) {
				sel = Selections.LastOrDefault ().Drawable.GetSelection (coords, Accuracy, inMotion);
			}
			if (sel == null) {
				foreach (ICanvasSelectableObject co in Objects) {
					sel = co.GetSelection (coords, Accuracy, inMotion);
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
				if (co != null && co.Description != null) {
					widget.ShowTooltip (co.Description);
				}
			}
		}

		protected virtual void HandleLeftButton (Point coords, ButtonModifier modif)
		{
			Selection sel;
			
			sel = GetSelection (coords);
			
			clickedSel = sel;
			if (sel != null) {
				(sel.Drawable as ICanvasObject).ClickPressed (coords, modif);
			}

			if ((SelectionMode == MultiSelectionMode.Multiple) ||
				(SelectionMode == MultiSelectionMode.MultipleWithModifier &&
				(modif == ButtonModifier.Control ||
				modif == ButtonModifier.Shift))) {
				if (sel != null) {
					sel.Position = SelectionPosition.All;
					UpdateSelection (sel);
				}
			} else {
				ClearSelection ();
				start = coords;
				UpdateSelection (sel);
				StartMove (sel);
				moving = Selections.Count > 0 && ObjectsCanMove;
			}
		}

		protected virtual void HandleRightButton (Point coords, ButtonModifier modif)
		{
			if (Selections.Count <= 1) {
				ClearSelection ();
				UpdateSelection (GetSelection (coords));
			}
			ShowMenu (coords);
		}

		protected virtual void HandleMotionEvent (Point coords)
		{
			Selection sel;

			coords = ToUserCoords (coords);
			if (moving && Selections.Count != 0) {
				sel = Selections [0];
				sel.Drawable.Move (sel, coords, start);  
				widget.ReDraw (sel.Drawable);
				SelectionMoved (sel);
				start = coords;
				moved = true;
			} else {
				CursorMoved (coords);
				start = coords;
			}
		}

		void HandleButtonReleasedEvent (Point coords, ButtonType type, ButtonModifier modifier)
		{
			moving = false;
			if (clickedSel != null) {
				(clickedSel.Drawable as ICanvasSelectableObject).ClickReleased ();
				clickedSel = null;
			}
			StopMove (moved);
			moved = false;
		}

		void HandleButtonPressEvent (Point coords, uint time, ButtonType type, ButtonModifier modifier)
		{
			if (time - lastTime < ClickRepeatMS) {
				return;
			}
			coords = ToUserCoords (coords); 
			if (type == ButtonType.Left) {
				/* For OS X CTRL+Left emulating right click */
				if (modifier == ButtonModifier.Meta) {
					HandleRightButton (coords, modifier);
				}
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

		public BackgroundCanvas (IWidget widget): base (widget)
		{
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
				background.ScaleFactor ((int)widget.Width, (int)widget.Height, out scaleX,
				                        out scaleY, out translation);
			}
		}

		public override void Draw (IContext context, Area area)
		{
			if (Background != null) {
				tk.Context = context;
				tk.Begin ();
				tk.TranslateAndScale (translation, new Point (scaleX, scaleY));
				tk.DrawImage (Background);
				tk.End ();
			}
			base.Draw (context, area);
		}
	}
}
