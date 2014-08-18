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
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Common;
using LongoMatch.Store.Drawables;

namespace LongoMatch.Drawing.CanvasObjects
{
	public abstract class CanvasObject: ICanvasObject
	{
		public delegate void CanvasHandler (CanvasObject co);
		public event CanvasHandler ClickedEvent;

		protected CanvasObject ()
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

		public virtual bool Selected {
			set;
			get;
		}

		public virtual void ClickPressed (Point p, ButtonModifier modif)
		{
		}

		public virtual void ClickReleased ()
		{
		}

		protected void EmitClickEvent ()
		{
			if (ClickedEvent != null) {
				ClickedEvent (this);
			}
		}

		public abstract void Draw (IDrawingToolkit tk, Area area);
	}

	public abstract class CanvasButtonObject: CanvasObject {
	
		public bool Toggle {
			get;
			set;
		}

		public bool Active {
			get;
			set;
		}

		public override void ClickPressed (Point p, ButtonModifier modif)
		{
			Active = !Active;
		}

		public override void ClickReleased ()
		{
			if (!Toggle) {
				Active = !Active;
			}
			EmitClickEvent ();
		}
	}
	
	public abstract class CanvasDrawableObject<T>: CanvasObject, ICanvasDrawableObject where T: IBlackboardObject
	{
		
		public IBlackboardObject IDrawableObject {
			get {
				return Drawable;
			}
			set {
				Drawable = (T)value;
			}
		}

		public T Drawable {
			get;
			set;
		}

		public override bool Selected {
			get {
				return Drawable.Selected;
			}
			set {
				Drawable.Selected = value;
			}
		}

		public Selection GetSelection (Point point, double precision)
		{
			Selection sel = Drawable.GetSelection (point, precision);
			if (sel != null) {
				sel.Drawable = this;
			}
			return sel;
		}

		public void Move (Selection s, Point p, Point start)
		{
			s.Drawable = Drawable;
			Drawable.Move (s, p, start);
			s.Drawable = this;
		}

		protected void DrawCornerSelection (IDrawingToolkit tk, Point p)
		{
			tk.StrokeColor = tk.FillColor = Constants.SELECTION_INDICATOR_COLOR;
			tk.LineStyle = LineStyle.Normal;
			tk.DrawRectangle (new Point (p.X - 3, p.Y - 3), 6, 6);
		}

		protected void DrawCenterSelection (IDrawingToolkit tk, Point p)
		{
			tk.StrokeColor = tk.FillColor = Constants.SELECTION_INDICATOR_COLOR;
			tk.LineStyle = LineStyle.Normal;
			tk.DrawCircle (p, 3);
		}

		protected void DrawSelectionArea (IDrawingToolkit tk)
		{
			Area area;
			
			area = Drawable.Area;
			if (!Selected || area == null) {
				return;
			}
			tk.StrokeColor = Constants.SELECTION_INDICATOR_COLOR;
			tk.StrokeColor = Constants.SELECTION_AREA_COLOR;
			tk.FillColor = null;
			tk.LineStyle = LineStyle.Dashed;
			tk.LineWidth = 1;
			tk.DrawRectangle (area.Start, area.Width, area.Height);
			foreach (Point p in area.Vertices) {
				DrawCornerSelection (tk, p);
			}
			foreach (Point p in area.VerticesCenter) {
				DrawCenterSelection (tk, p);
			}
		}
	}
}