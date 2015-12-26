//
//  Copyright (C) 2015 Fluendo S.A.
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Drawing.CanvasObjects;

namespace LongoMatch.Drawing
{

	/// <summary>
	/// A selection canvas supports selecting <see cref="ICanvasSelectableObject"/>
	/// objects from the canvas and moving, resizing them.
	/// </summary>
	public class SelectionCanvas: Canvas
	{

		Selection clickedSel;

		public SelectionCanvas (IWidget widget) : base (widget)
		{
			Selections = new List<Selection> ();
			SelectionMode = MultiSelectionMode.Single;
			Accuracy = 1;
			ClickRepeatMS = 100;
			ObjectsCanMove = true;
			IgnoreClicks = false;
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

		/// <summary>
		/// Clears the objects.
		/// </summary>
		protected override void ClearObjects ()
		{
			// Make sure we don't maintain a selection with invalid objects.
			ClearSelection ();
			base.ClearObjects ();
		}

		/// <summary>
		/// Maximum time in milliseconds where 2 mouse clicks are
		/// considered a single one
		/// </summary>
		public int ClickRepeatMS {
			get;
			set;
		}

		/// <summary>
		/// Set the tolerance for clicks in the dashboards. An accuracy of 5
		/// lets select objects with clicks 5 points away from their position.
		/// </summary>
		public double Accuracy {
			get;
			set;
		}

		/// <summary>
		/// Set the selection mode.
		/// </summary>
		public MultiSelectionMode SelectionMode {
			get;
			set;
		}

		/// <summary>
		/// A list of objects for which multiple selection is disabled.
		/// </summary>
		public List<Type> SingleSelectionObjects {
			get;
			set;
		}

		/// <summary>
		/// If <c>true</c> objects can moved in the canvas
		/// </summary>
		public bool ObjectsCanMove {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this canvas is clickable. When set to <c>flase</c> clicks are ignored
		/// </summary>
		public bool IgnoreClicks {
			get;
			set;
		}

		/// <summary>
		/// A list with all the selected objects
		/// </summary>
		protected List<Selection> Selections {
			get;
			set;
		}

		/// <summary>
		/// The object that is currently highlited (mouse is over the object)
		/// </summary>
		public CanvasObject HighlightedObject {
			get;
			set;
		}

		/// <summary>
		/// The start point from which the object was moved.
		/// It can be used to determine the distance of the move action.
		/// </summary>
		public Point MoveStart {
			get;
			set;
		}

		/// <summary>
		/// When set to <c>true</c> it indicates an object has been moved
		/// between the clik pressed + mouse move + click released.
		/// </summary>
		public bool Moved {
			get;
			set;
		}

		/// <summary>
		/// When set to <c>true</c> it indicates when in the middle of a move action.
		/// </summary>
		public bool Moving {
			get;
			set;
		}

		/// <summary>
		/// Called when the cursor is being moved.
		/// Highlights objects when the cursor passes over them. 
		/// </summary>
		/// <param name="coords">Coords.</param>
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

			if (current != HighlightedObject) {
				if (HighlightedObject != null) {
					HighlightedObject.Highlighted = false;
				}
				if (current != null) {
					current.Highlighted = true;
				}
				HighlightedObject = current;
			}
		}

		/// <summary>
		/// Notifies subclasses when an object starts to be moved.
		/// </summary>
		/// <param name="sel">The selection moved.</param>
		protected virtual void StartMove (Selection sel)
		{
		}

		/// <summary>
		/// Notifies subclasses when an object has been moved.
		/// </summary>
		/// <param name="sel">The selection moved.</param>
		protected virtual void SelectionMoved (Selection sel)
		{
		}

		/// <summary>
		/// Notifies subclass when the move process stops.
		/// </summary>
		/// <param name="moved">If set to <c>true</c>, the object position changed.</param>
		protected virtual void StopMove (bool moved)
		{
		}

		/// <summary>
		/// Notifies subclasses when the selected objects has changed.
		/// </summary>
		/// <param name="sel">List of selected objects.</param>
		protected virtual void SelectionChanged (List<Selection> sel)
		{
		}

		/// <summary>
		/// Notifies subclasses a menu should be displayed.
		/// Canvas' with menus should override it to display their menu here.
		/// </summary>
		/// <param name="coords">Position where the click happens.</param>
		protected virtual void ShowMenu (Point coords)
		{
		}

		/// <summary>
		/// Reset the list of select objects
		/// </summary>
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

		/// <summary>
		/// Updates the current selection. If <paramref name="sel"/> is <c>null</c>,
		/// it clears the current selection. If <paramref name="sel"/> wasn't previously
		/// selected, it's added to the list of selected objects, otherwise it's removed
		/// from the list.
		/// </summary>
		/// <param name="sel">The selection.</param>
		/// <param name="notify">If set to <c>true</c>, notifies about the changes.</param>
		protected virtual void UpdateSelection (Selection sel, bool notify = true)
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

		protected virtual Selection GetSelection (Point coords, bool inMotion = false, bool skipSelected = false)
		{
			Selection sel = null;
			Selection selected = null;

			if (Selections.Count > 0) {
				selected = Selections.LastOrDefault ();
				/* Try with the selected item first */
				if (!skipSelected)
					sel = selected.Drawable.GetSelection (coords, Accuracy, inMotion);
			}

			/* Iterate over all the objects now */
			if (sel == null) {
				foreach (ICanvasSelectableObject co in Objects) {
					sel = co.GetSelection (coords, Accuracy, inMotion);
					if (sel == null)
						continue;
					if (skipSelected && selected != null && sel.Drawable == selected.Drawable)
						continue;
					break;
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
				MoveStart = coords;
				UpdateSelection (sel);
				StartMove (sel);
				Moving = Selections.Count > 0 && ObjectsCanMove;
			}
		}

		protected virtual void HandleDoubleClick (Point coords, ButtonModifier modif)
		{
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
			Point userCoords;

			userCoords = ToUserCoords (coords);
			if (Moving && Selections.Count != 0) {
				sel = Selections [0];
				sel.Drawable.Move (sel, userCoords, MoveStart);  
				widget.ReDraw (sel.Drawable);
				SelectionMoved (sel);
				Moved = true;
			} else {
				CursorMoved (userCoords);
			}
			MoveStart = ToUserCoords (coords);
		}

		void HandleButtonReleasedEvent (Point coords, ButtonType type, ButtonModifier modifier)
		{
			if (IgnoreClicks) {
				return;
			}
		
			Moving = false;
			if (clickedSel != null) {
				(clickedSel.Drawable as ICanvasSelectableObject).ClickReleased ();
				clickedSel = null;
			}
			StopMove (Moved);
			Moved = false;
		}

		void HandleButtonPressEvent (Point coords, uint time, ButtonType type, ButtonModifier modifier, ButtonRepetition repetition)
		{
			if (IgnoreClicks) {
				return;
			}

			coords = ToUserCoords (coords); 
			if (repetition == ButtonRepetition.Single) {
				if (type == ButtonType.Left) {
					/* For OS X CTRL+Left emulating right click */
					if (modifier == ButtonModifier.Meta) {
						HandleRightButton (coords, modifier);
					}
					HandleLeftButton (coords, modifier);
				} else if (type == ButtonType.Right) {
					HandleRightButton (coords, modifier);
				}
			} else {
				HandleDoubleClick (coords, modifier);
			}
		}
	}
	
}
