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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Drawing.CanvasObjects;

namespace LongoMatch.Drawing
{
	/// <summary>
	/// A canvas stores <see cref="ICanvasObject"/>'s and draws them.
	/// </summary>
	public class Canvas: ICanvas
	{
		protected IDrawingToolkit tk;
		protected IWidget widget;
		bool disposed;

		public Canvas (IWidget widget)
		{
			this.widget = widget;
			tk = Config.DrawingToolkit;
			Objects = new List<ICanvasObject> ();
			widget.DrawEvent += Draw;
			widget.SizeChangedEvent += HandleSizeChangedEvent;
			ScaleX = 1;
			ScaleY = 1;
			Translation = new Point (0, 0);
		}

		~ Canvas ()
		{
			if (!disposed) {
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
			IgnoreRedraws = true;
			if (disposing) {
				ClearObjects ();
				Objects = null;
				disposed = true;
			}
		}

		/// <summary>
		/// Removes all the objects from the canvas.
		/// </summary>
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

		/// <summary>
		/// A list of the first level objects stored in the canvas.
		/// Objects including other objects should take care of forwarding
		/// the redraw events their self.
		/// </summary>
		public List<ICanvasObject> Objects {
			get;
			set;
		}

		/// <summary>
		/// Adds a new object to the canvas and a listener to its redraw event.
		/// </summary>
		/// <param name="co">The object to add.</param>
		public void AddObject (ICanvasObject co)
		{
			Objects.Add (co);
			co.RedrawEvent += HandleRedrawEvent;
		}

		/// <summary>
		/// Removes and object from the canvas.
		/// </summary>
		/// <param name="co">The object to remove.</param>
		public void RemoveObject (ICanvasObject co)
		{
			co.RedrawEvent -= HandleRedrawEvent;
			Objects.Remove (co);
			co.Dispose ();
		}

		/// <summary>
		/// Converts a point to the original position removing the applied
		/// tanslation and invering the scale.
		/// </summary>
		/// <returns>The converted point.</returns>
		/// <param name="p">The point to convert.</param>
		protected Point ToUserCoords (Point p)
		{
			return new Point ((p.X - Translation.X) / ScaleX,
				(p.Y - Translation.Y) / ScaleY);
		
		}

		/// <summary>
		/// Defines a clip region, any drawing outside this region
		/// will not be drawn.
		/// </summary>
		protected Area ClipRegion {
			get;
			set;
		}

		/// <summary>
		/// When set to <c>true</c> redraws events are not ignored
		/// </summary>
		protected bool IgnoreRedraws {
			get;
			set;
		}

		/// <summary>
		/// Applied scale on the X axis
		/// </summary>
		protected double ScaleX {
			get;
			set;
		}

		/// <summary>
		/// Applied scale on the Y axis.
		/// </summary>
		protected double ScaleY {
			get;
			set;
		}

		/// <summary>
		/// Applied XY translation.
		/// </summary>
		protected Point Translation {
			get;
			set;
		}

		void HandleRedrawEvent (ICanvasObject co, Area area)
		{
			if (!IgnoreRedraws) {
				widget.ReDraw (area);
			}
		}

		protected virtual void HandleSizeChangedEvent ()
		{
			/* After a resize objects are rescalled and we need to invalidate
			 * their cached surfaces */
			foreach (CanvasObject to in Objects) {
				to.ResetDrawArea ();
			}
		}

		/// <summary>
		/// Must be called before any drawing operation is performed
		/// to apply transformation, scalling and clipping.
		/// </summary>
		/// <param name="context">Context to draw</param>
		protected void Begin (IContext context)
		{
			tk.Context = context;
			tk.Begin ();
			if (ClipRegion != null) {
				tk.Clip (ClipRegion);
			}
			tk.TranslateAndScale (Translation, new Point (ScaleX, ScaleY));
		}

		/// <summary>
		/// Must be called after drawing operations to restore the context
		/// </summary>
		protected void End ()
		{
			tk.End ();
			tk.Context = null;
		}

		/// <summary>
		/// Draws the canvas objects the specified context and area.
		/// Object are drawn in the following order:
		///  1) Regular objects
		///  2) Selected objects
		///  3) Highlithed objects
		/// </summary>
		/// <param name="context">The context where the canvas is drawn.</param>
		/// <param name="area">The affected area.</param>
		public virtual void Draw (IContext context, Area area)
		{
			List<CanvasObject> highlighted = new List<CanvasObject> ();
			Begin (context);
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
			End ();
		}
	}

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

	public abstract class BackgroundCanvas: SelectionCanvas
	{
		public event EventHandler RegionOfInterestChanged;

		Image background;
		Area regionOfInterest;

		public BackgroundCanvas (IWidget widget) : base (widget)
		{
		}

		/// <summary>
		/// Sets the background image of the canvas.
		/// This property is not optional
		/// </summary>
		public Image Background {
			set {
				background = value;
				HandleSizeChangedEvent ();
			}
			get {
				return background;
			}
		}

		/// <summary>
		/// Defines an area with the region of interest, which can
		/// be used to zoom into the canvas.
		/// </summary>
		/// <value>The region of interest.</value>
		public Area RegionOfInterest {
			set {
				regionOfInterest = value;
				HandleSizeChangedEvent ();
				widget.ReDraw ();
				if (RegionOfInterestChanged != null) {
					RegionOfInterestChanged (this, null);
				}
			}
			get {
				return regionOfInterest;
			}
		}

		protected override void HandleSizeChangedEvent ()
		{
			if (background != null) {
				double scaleX, scaleY;
				Point translation;

				/* Add black borders to the canvas to keep the DAR of the background image */
				background.ScaleFactor ((int)widget.Width, (int)widget.Height, out scaleX,
					out scaleY, out translation);
				ClipRegion = new Area (new Point (translation.X, translation.Y),
					background.Width * scaleX, background.Height * scaleY);
				ScaleX = scaleX;
				ScaleY = scaleY;
				Translation = translation;

				/* If there is a region of interest set, combine the transformation */
				if (RegionOfInterest != null && !RegionOfInterest.Empty) {
					ScaleX *= background.Width / RegionOfInterest.Width;
					ScaleY *= background.Height / RegionOfInterest.Height;
					Translation -= new Point (RegionOfInterest.Start.X * ScaleX,
						RegionOfInterest.Start.Y * ScaleY);
				}
			}
			base.HandleSizeChangedEvent ();
		}

		public override void Draw (IContext context, Area area)
		{
			if (Background != null) {
				Begin (context);
				tk.DrawImage (Background);
				End ();
			}
			base.Draw (context, area);
		}
	}
}
