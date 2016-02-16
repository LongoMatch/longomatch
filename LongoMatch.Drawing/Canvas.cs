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
		int widthRequest, heightRequest;

		public Canvas (IWidget widget)
		{
			tk = Config.DrawingToolkit;
			Objects = new List<ICanvasObject> ();
			ScaleX = 1;
			ScaleY = 1;
			Translation = new Point (0, 0);
			BackgroundColor = Config.Style.PaletteBackground;
			SetWidget (widget);
		}

		public Canvas () : this (null)
		{
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
				SetWidget (null);
				ClearObjects ();
				Objects = null;
				disposed = true;
			}
		}

		public virtual void SetWidget (IWidget newWidget)
		{
			if (widget != null) {
				widget.DrawEvent -= Draw;
				widget.SizeChangedEvent -= HandleSizeChangedEvent;
			}
			this.widget = newWidget;
			if (widget != null) {
				widget.DrawEvent += Draw;
				widget.SizeChangedEvent += HandleSizeChangedEvent;
				if (WidthRequest != 0) {
					widget.Width = WidthRequest;
				}
				if (HeightRequest != 0) {
					widget.Height = HeightRequest;
				}
				widget.ReDraw ();
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

		/// Gets or sets the width required by the canvas.
		/// </summary>
		public int WidthRequest {
			get {
				return widthRequest;
			}
			set {
				widthRequest = value;
				if (widget != null) {
					widget.Width = widthRequest;
				}
			}
		}

		/// <summary>
		/// Gets or sets the height required by the canvas.
		/// </summary>
		public int HeightRequest {
			get {
				return heightRequest;
			}
			set {
				heightRequest = value;
				if (widget != null) {
					widget.Height = heightRequest;
				}
			}
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		public Color BackgroundColor {
			get;
			set;
		}

		/// <summary>
		/// Gets a value indicating whether this canvas has a translation or zoom applied.
		/// </summary>
		protected bool HasTranslationOrZoom {
			get {
				return Translation != new Point (0, 0) || ScaleX != 1 || ScaleY != 1;
			}
		}

		void HandleRedrawEvent (ICanvasObject co, Area area)
		{
			if (!IgnoreRedraws) {
				widget?.ReDraw (area);
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

		protected void DrawBackground ()
		{
			tk.Clear (BackgroundColor);
		}

		protected void DrawObjects (Area area)
		{
			List<CanvasObject> highlighted = new List<CanvasObject> ();
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
			// If there is translation or zoom, the background needs be painted before applying it
			if (HasTranslationOrZoom) {
				tk.Context = context;
				DrawBackground ();
			}
			Begin (context);
			if (!HasTranslationOrZoom) {
				DrawBackground ();
			}
			DrawObjects (area);
			End ();
		}
	}
}
