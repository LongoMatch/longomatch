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
using System.Runtime.Remoting;
using LongoMatch.Common;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Store;
using LongoMatch.Drawing.CanvasObject;
using LongoMatch.Store.Drawables;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;

namespace LongoMatch.Drawing.Widgets
{
	public class Blackboard: BackgroundCanvas
	{
	
		public event ShowDrawToolMenuHandler ShowMenuEvent;
		public event ConfigureDrawingObjectHandler ConfigureObjectEvent;
		public event DrawableChangedHandler DrawableChangedEvent;
		DrawTool tool;
		FrameDrawing drawing;
		ISurface backbuffer;
		bool handdrawing;

		public Blackboard (IWidget widget): base(widget)
		{
			Accuracy = 5;
			SelectionMode = MultiSelectionMode.Single;
			LineWidth = 2;
			Color = Color.Red1;
			LineStyle = LineStyle.Normal;
			LineType = LineType.Arrow;
			tool = DrawTool.Selection;
			
		}
		
		public FrameDrawing Drawing {
			set {
				drawing = value;
				foreach (IBlackboardObject d in value.Drawables) {
					Add (d);
				}
				if (backbuffer !=  null) {
					backbuffer.Dispose ();
				}
				backbuffer = tk.CreateSurface (Background.Width, Background.Height,
				                               drawing.Freehand);
			}
		}

		public Color Color {
			get;
			set;
		}
		
		public Color TextColor {
			get;
			set;
		}
		
		public Color TextBackgroundColor {
			get;
			set;
		}

		public LineStyle LineStyle {
			get;
			set;
		}
		
		public LineType LineType {
			get;
			set;
		}

		public int LineWidth {
			get;
			set;
		}
		
		public DrawTool Tool {
			get {
				return tool;
			}
			set {
				tool = value;
				widget.SetCursorForTool (tool);
			}
		}
		
		public void DeleteSelection () {
			foreach (ICanvasDrawableObject o in Selections.Select (s => s.Drawable)) {
				Objects.Remove (o);
				drawing.Drawables.Remove ((Drawable)o.IDrawableObject);
			}
			ClearSelection ();
			UpdateCounters ();
			widget.ReDraw ();
		}
		
		public void Clear () {
			ClearSelection ();
			drawing.Drawables.Clear ();
			Objects.Clear ();
			backbuffer.Dispose ();
			backbuffer = tk.CreateSurface (Background.Width, Background.Height);
			widget.ReDraw ();
		}
		
		public Image Save () {
			ClearSelection ();
			drawing.Freehand = backbuffer.Copy();
			return tk.Copy (this, Background.Width, Background.Height);
		}

		public void Save (string filename) {
			ClearSelection ();
			tk.Save (this, Background.Width, Background.Height, filename);
		}
		
		ICanvasSelectableObject Add (IBlackboardObject drawable) {
			string objecttype = String.Format ("LongoMatch.Drawing.CanvasObject.{0}Object",
			                                   drawable.GetType().ToString().Split('.').Last());
			ObjectHandle handle = Activator.CreateInstance(null, objecttype);
			ICanvasDrawableObject d = (ICanvasDrawableObject) handle.Unwrap();
			d.IDrawableObject = drawable;
			Objects.Add (d);
			return d;
		}
		
		protected override void StartMove (Selection sel)
		{
			Drawable drawable = null;
			SelectionPosition pos = SelectionPosition.BottomRight;
			bool resize = true, copycolor = true;

			if (sel != null || Tool == DrawTool.Selection)
				return;
				
			switch (Tool) {
			case DrawTool.Line:
				drawable = new Line (start, new Point (start.X + 1, start.Y + 1),
				                   LineType, LineStyle);
				drawable.FillColor = Color;
				pos = SelectionPosition.LineStop;
				break;
			case DrawTool.Cross:
				drawable = new Cross (start, new Point (start.X + 1, start.Y + 1),
				                      LineStyle);
				break;
			case DrawTool.Ellipse:
				drawable = new Ellipse (start, 2, 2);
				break;
			case DrawTool.Rectangle:
				drawable = new Rectangle (start, 2, 2);
				break;
			case DrawTool.CircleArea:
				drawable = new Ellipse (start, 2, 2);
				drawable.FillColor = Color.Copy();
				drawable.FillColor.A = byte.MaxValue / 2;
				break;
			case DrawTool.RectangleArea:
				drawable = new Rectangle (start, 2, 2);
				drawable.FillColor = Color.Copy();
				drawable.FillColor.A = byte.MaxValue / 2;
				break;
			case DrawTool.Counter:
				drawable = new Counter (start, 10, 10, 0);
				drawable.FillColor = Color.Copy();
				drawable.FillColor.A = byte.MaxValue / 2;
				(drawable as Counter).TextColor = Color.Grey2;
				resize = false;
				break;
			case DrawTool.Text: {
				Text text = new Text (start, 1, 20, "");
				if (ConfigureObjectEvent != null) {
					ConfigureObjectEvent (text);
				}
				if (text.Value == null) {
					return;
				}
				text.TopRight.X += text.Value.Length * 12;
				text.BottomRight.X += text.Value.Length * 12;
				text.TextColor = TextColor.Copy();
				text.FillColor = text.StrokeColor = TextBackgroundColor.Copy();
				resize = copycolor = false;
				drawable = text;
				break;
			}
			case DrawTool.Pen: 
			case DrawTool.Eraser: 
				handdrawing = true;
				break;
			}
			
			if (drawable != null) {
				if (copycolor) {
					drawable.StrokeColor = Color.Copy ();
				}
				drawable.LineWidth = LineWidth;
				drawable.Style = LineStyle;
				var selo = Add (drawable);
				drawing.Drawables.Add (drawable);
				if (Tool == DrawTool.Counter) {
					UpdateCounters ();
				}
				if (resize) {
					UpdateSelection (new Selection (selo, pos, 5));
				} else {
					UpdateSelection (new Selection (selo, SelectionPosition.All, 5));
				}
				widget.ReDraw ();
			}
		}
		
		protected override void StopMove ()
		{
			Selection sel = Selections.FirstOrDefault();
			if (sel != null) {
				(sel.Drawable as ICanvasDrawableObject).IDrawableObject.Reorder();
			}
			handdrawing = false;
		}
		
		protected override void ShowMenu (Point coords)
		{
			Selection sel = Selections.FirstOrDefault ();
			if (sel != null && ShowMenuEvent != null) {
				ShowMenuEvent ((sel.Drawable as ICanvasDrawableObject).IDrawableObject);
			}
			
		}
		
		protected override void SelectionChanged (System.Collections.Generic.List<Selection> sel)
		{
			if (sel != null && sel.Count > 0 && DrawableChangedEvent != null) {
				DrawableChangedEvent ((sel[0].Drawable as ICanvasDrawableObject).IDrawableObject);
			} else {
				DrawableChangedEvent (null);
			}
		}

		void UpdateCounters () {
			int index = 1;
			
			foreach (IBlackboardObject bo in
			         Objects.Select  (o => (o as ICanvasDrawableObject).IDrawableObject)) {
				if (bo is Counter) {
					(bo as Counter).Count = index;
					index ++;
				}
			}
		}
		
		protected override void CursorMoved (Point coords)
		{
			if (handdrawing) {
				tk.Context = backbuffer.Context;
				tk.Begin ();
				tk.LineStyle = LineStyle.Normal;
				tk.LineWidth = LineWidth;
				if (tool == DrawTool.Eraser) {
					tk.StrokeColor = tk.FillColor = new Color (0, 0, 0, 255);
					tk.LineWidth = LineWidth * 4;
					tk.Clear = true;
				} else {
					tk.StrokeColor = tk.FillColor = Color;
				}
				tk.DrawLine (start, coords);
				tk.End ();
				widget.ReDraw();
			}
		}
		
		public override void Draw (IContext context, Area area)
		{
			base.Draw (context, area);
			if (backbuffer != null) {
				tk.Context = context;
				tk.Begin ();
				tk.DrawSurface (backbuffer);
				tk.End ();
			}
		}
	}
}

