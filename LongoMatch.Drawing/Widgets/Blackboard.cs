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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Drawables;
using System;

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
		bool handdrawing, inObjectCreation, inZooming;
		double currentZoom;

		public Blackboard (IWidget widget) : base (widget)
		{
			Accuracy = 5;
			SelectionMode = MultiSelectionMode.Single;
			LineWidth = 2;
			Color = Color.Red1;
			LineStyle = LineStyle.Normal;
			LineType = LineType.Arrow;
			FontSize = 12;
			tool = DrawTool.Selection;
			currentZoom = 1;
			MaxZoom = 4;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (backbuffer != null)
					backbuffer.Dispose ();
				backbuffer = null;
			}
			base.Dispose (disposing);
		}

		public FrameDrawing Drawing {
			set {
				Clear (false);
				drawing = value;
				if (backbuffer != null) {
					backbuffer.Dispose ();
				}
				if (drawing != null) {
					foreach (IBlackboardObject d in drawing.Drawables) {
						Add (d);
					}
					backbuffer = tk.CreateSurface (Background.Width, Background.Height,
						drawing.Freehand);
				} else {
					backbuffer = tk.CreateSurface (Background.Width, Background.Height);
				}
				Accuracy = Background.Width / 100;
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

		public int FontSize {
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
				UpdateSelection (null);
			}
		}

		public double MaxZoom {
			get;
			set;
		}

		public void DeleteSelection ()
		{
			foreach (ICanvasDrawableObject o in Selections.Select (s => s.Drawable)) {
				RemoveObject (o);
				drawing.Drawables.Remove ((Drawable)o.IDrawableObject);
			}
			ClearSelection ();
			UpdateCounters ();
			widget.ReDraw ();
		}

		public void Clear (bool resetDrawing = true)
		{
			ClearSelection ();
			ClearObjects ();
			if (drawing != null && resetDrawing) {
				drawing.Drawables.Clear ();
			}
			if (backbuffer != null) {
				using (IContext c = backbuffer.Context) {
					tk.Context = c;
					tk.Clear (new Color (0, 0, 0, 0));
					tk.Context = null;
				}
				;
			}
			widget.ReDraw ();
		}

		public Image Save ()
		{
			ClearSelection ();
			drawing.Freehand = backbuffer.Copy ();
			return tk.Copy (this, Background.Width, Background.Height);
		}

		public void Save (string filename)
		{
			ClearSelection ();
			tk.Save (this, Background.Width, Background.Height, filename);
		}

		public void Zoom (double zoom, Point center = null)
		{
			Area roi;
			double width, height;

			if (currentZoom == zoom)
				return;

			if (RegionOfInterest == null)
				roi = new Area (new Point (0, 0),
					Background.Width, Background.Height);
			else
				roi = RegionOfInterest;

			width = Background.Width / zoom;
			height = Background.Height / zoom;

			if (center == null) {
				roi.Start.X = roi.Start.X + roi.Width / 2 - width / 2; 
				roi.Start.Y = roi.Start.Y + roi.Height / 2 - height / 2; 
			} else {
				roi.Start.X = center.X - width / 2;
				roi.Start.Y = center.Y - height / 2;
			}
			roi.Width = width;
			roi.Height = height;
			ClipRoi (roi);
			RegionOfInterest = roi;
			currentZoom = zoom;
		}

		void ClipRoi (Area roi)
		{
			Point st = roi.Start;
			st.X = Math.Max (st.X, 0);
			st.Y = Math.Max (st.Y, 0);
			st.X = Math.Min (st.X, (Background.Width - roi.Width));
			st.Y = Math.Min (st.Y, (Background.Height - roi.Height));
		}

		ICanvasSelectableObject Add (IBlackboardObject drawable)
		{
			ICanvasSelectableObject cso = Utils.CanvasFromDrawableObject (drawable);
			AddObject (cso);
			return cso;
		}

		protected override void StartMove (Selection sel)
		{
			Drawable drawable = null;
			SelectionPosition pos = SelectionPosition.BottomRight;
			bool resize = true, copycolor = true, sele = true;

			if (Tool == DrawTool.Selection) {
				if (Selections.Count == 0 && currentZoom != 1) {
					widget.SetCursorForTool (DrawTool.Move);
					inZooming = true;
				}
				return;
			}

			if (sel != null) {
				ClearSelection ();
			}
				
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
				drawable.FillColor = Color.Copy ();
				drawable.FillColor.A = byte.MaxValue / 2;
				break;
			case DrawTool.RectangleArea:
				drawable = new Rectangle (start, 2, 2);
				drawable.FillColor = Color.Copy ();
				drawable.FillColor.A = byte.MaxValue / 2;
				break;
			case DrawTool.Counter:
				drawable = new Counter (start, 3 * LineWidth, 0);
				drawable.FillColor = Color.Copy ();
				(drawable as Counter).TextColor = Color.Grey2;
				resize = false;
				break;
			case DrawTool.Text:
			case DrawTool.Player:
				{
					int width, heigth;
					Text text = new Text (start, 1, 1, "");
					if (ConfigureObjectEvent != null) {
						ConfigureObjectEvent (text, Tool);
					}
					if (text.Value == null) {
						return;
					}
					Config.DrawingToolkit.MeasureText (text.Value, out width, out heigth,
						Config.Style.Font, FontSize, FontWeight.Normal);
					text.Update (new Point (start.X - width / 2, start.Y - heigth / 2),
						width, heigth);
					text.TextColor = TextColor.Copy ();
					text.FillColor = text.StrokeColor = TextBackgroundColor.Copy ();
					text.TextSize = FontSize;
					resize = copycolor = sele = false;
					drawable = text;
					break;
				}
			case DrawTool.Pen: 
			case DrawTool.Eraser: 
				handdrawing = true;
				break;
			case DrawTool.Zoom:
				Zoom (Math.Min (currentZoom + 0.1, MaxZoom), start);
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
				if (sele) {
					if (resize) {
						UpdateSelection (new Selection (selo, pos, 5));
					} else {
						UpdateSelection (new Selection (selo, SelectionPosition.All, 5));
					}
					inObjectCreation = true;
				}
				widget.ReDraw ();
			}
		}

		protected override void StopMove (bool moved)
		{
			Selection sel = Selections.FirstOrDefault ();

			if (inZooming) {
				widget.SetCursorForTool (DrawTool.CanMove);
			}

			if (sel != null) {
				(sel.Drawable as ICanvasDrawableObject).IDrawableObject.Reorder ();
			}
			if (inObjectCreation) {
				UpdateSelection (null);
				inObjectCreation = false;
			}
			handdrawing = false;
			inZooming = false;
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
			if (DrawableChangedEvent != null) {
				if (sel != null && sel.Count > 0) {
					DrawableChangedEvent ((sel [0].Drawable as ICanvasDrawableObject).IDrawableObject);
				} else {
					DrawableChangedEvent (null);
				}
			}
		}

		void UpdateCounters ()
		{
			int index = 1;
			
			foreach (IBlackboardObject bo in
			         Objects.Select  (o => (o as ICanvasDrawableObject).IDrawableObject)) {
				if (bo is Counter) {
					(bo as Counter).Count = index;
					index++;
				}
			}
		}

		protected override void CursorMoved (Point coords)
		{
			if (inZooming && RegionOfInterest != null) {
				Point diff = coords - start;
				RegionOfInterest.Start -= diff;
				ClipRoi (RegionOfInterest);
				RegionOfInterest = RegionOfInterest;
			} else if (handdrawing) {
				using (IContext c = backbuffer.Context) {
					tk.Context = c;
					tk.Begin ();
					tk.LineStyle = LineStyle.Normal;
					tk.LineWidth = LineWidth;
					if (tool == DrawTool.Eraser) {
						tk.StrokeColor = tk.FillColor = new Color (0, 0, 0, 255);
						tk.LineWidth = LineWidth * 4;
						tk.ClearOperation = true;
					} else {
						tk.StrokeColor = tk.FillColor = Color;
					}
					tk.DrawLine (start, coords);
					tk.End ();
				}
				widget.ReDraw ();
			} else {
				base.CursorMoved (coords);
				if (Tool == DrawTool.Selection) {
					DrawTool moveTool = currentZoom == 1 ? DrawTool.None : DrawTool.CanMove;
					if (highlighted == null) {
						widget.SetCursorForTool (moveTool);
					} else {
						widget.SetCursorForTool (DrawTool.Selection);
					}
				}
			}
		}

		public override void Draw (IContext context, Area area)
		{
			tk.Context = context;
			tk.Begin ();
			tk.Clear (Config.Style.PaletteBackground);
			tk.End ();
			
			base.Draw (context, area);
			if (backbuffer != null) {
				Begin (context);
				tk.DrawSurface (backbuffer);
				End ();
			}
		}
	}
}

