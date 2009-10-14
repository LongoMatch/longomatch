// 
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Gdk;
using Gtk;
using Cairo;


namespace LongoMatch.Gui.Component
{
	public enum DrawTool{
		PEN,
		LINE,
		CIRCLE,
		RECTANGLE,
		CROSS,
		ERASER
	}
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class DrawingWidget : Gtk.Bin
	{
		private Surface source;
		private Surface drawings;
		
		private Cairo.Color lineColor;
		private int lineWidth;
		private double transparency;
		private DrawTool selectedTool;
		
		private Cairo.PointD initialPoint,finalPoint;
	
		private int sourceWidth,sourceHeight;
		
		private bool loaded;
		private bool visible;
		private bool preview;
		
		
		//Offset values to keep image in center
		private int xOffset;
		private int yOffset;
		
		//Mouse motion
		private double lastx=0;
		private double lasty=0;
		
		private const double ARROW_DEGREES = 0.5;
		private const int ARROW_LENGHT = 15;
		
		public DrawingWidget()
		{
			this.Build();	
			lineColor = new Cairo.Color(Double.MaxValue,0,0);
			lineWidth = 6;
			Transparency=0.8;
			selectedTool = DrawTool.PEN;
			loaded = false;
			visible = true;
			preview=false;
		}
		
		public Pixbuf SourceImage{
			set{
				sourceWidth = value.Width;
				sourceHeight = value.Height;
				source = new ImageSurface(Format.RGB24,sourceWidth,sourceHeight);
				drawings = new ImageSurface (Format.ARGB32,sourceWidth,sourceHeight);
				using (Context sourceCR = new Context(source)){					
					CairoHelper.SetSourcePixbuf(sourceCR,value,0,0);
					sourceCR.Paint();
				}
				drawingarea.WidthRequest=sourceWidth;
				drawingarea.HeightRequest=sourceHeight;
				if (GdkWindow != null)
					GdkWindow.Resize(sourceWidth,sourceHeight);
				value.Dispose();
				loaded = true;
			}			
		}
				
		public int LineWidth{
			set{lineWidth = value;}
		}
		
		public double Transparency{
			set{
				if (value >=0 && value <=1){
					transparency = value;
					drawingarea.QueueDraw();
				}
			}
		}
		
		public Gdk.Color LineColor{
			set{lineColor = new Cairo.Color(value.Red,value.Green,value.Blue);}
		}
				
		public DrawTool DrawTool{
			set{
				this.selectedTool = value;
				switch (selectedTool){
					case DrawTool.LINE:
						drawingarea.GdkWindow.Cursor = new Cursor(CursorType.DraftSmall);
						break;
					case DrawTool.RECTANGLE:
						drawingarea.GdkWindow.Cursor = new Cursor(CursorType.Dotbox);
						break;
					case DrawTool.CIRCLE:
						drawingarea.GdkWindow.Cursor = new Cursor(CursorType.Circle);
						break;
					case DrawTool.CROSS:
						drawingarea.GdkWindow.Cursor = new Cursor(CursorType.XCursor);
						break;
					case DrawTool.ERASER:
						drawingarea.GdkWindow.Cursor = new Cursor(CursorType.Target);
						break;
					case DrawTool.PEN:
						drawingarea.GdkWindow.Cursor = new Cursor(CursorType.Pencil);
						break;
					default:
						drawingarea.GdkWindow.Cursor = new Cursor(CursorType.Arrow);
						break;					
				}
			}
			get{return selectedTool;}
		}
		
		public bool DrawingsVisible{
			set{visible = value;}
		}
		
		public void ClearDrawing(){
			drawings.Destroy();
			drawings = new ImageSurface (Format.ARGB32,sourceWidth,sourceHeight);
			QueueDraw();
		}
		
		public void SaveAll(string filename){
			Save(filename,true,true);
		}
		
		public void SaveDrawings(string filename){
			Save(filename,false,true);
		}
		
		private void Save(string filename,bool bSource,bool bDrawings){
			Surface pngSurface = new ImageSurface (Format.ARGB32,sourceWidth,sourceHeight);
			using (Context c = new Context(pngSurface)){
				if (bSource){
					c.SetSourceSurface(source,0,0);
					c.Paint();
				}
				if (bDrawings){
					c.SetSourceSurface(drawings,0,0);
					c.PaintWithAlpha(transparency);
				}
			}
			pngSurface.WriteToPng(filename);
		}
		
		private void SetContextProperties(Context c){			
			c.LineCap = LineCap.Round;
			c.LineJoin = LineJoin.Round;
			if (selectedTool != DrawTool.ERASER){
				c.Color = lineColor;
				c.LineWidth = lineWidth;
				c.Operator = Operator.Over;
			}else{
				c.Color = new Cairo.Color(0,0,0,0);
				c.LineWidth = lineWidth*2;
				c.Operator = Operator.Source;
			}
		}
			
		private void DrawLine(Context c, double x1, double y1, double x2, double y2){		
			SetContextProperties(c);
			c.MoveTo(x1-xOffset,y1-yOffset);
			c.LineTo(x2-xOffset,y2-yOffset);
			c.Stroke();
			c.Fill();		
		}
		
		private void DrawArrow(Context c, double x1, double y1, double x2, double y2){
			double vx1,vy1,vx2,vy2;
			double angle = Math.Atan2(y2 - y1, x2 - x1) + Math.PI;

			vx1 = x2 + (ARROW_LENGHT + lineWidth) * Math.Cos(angle - ARROW_DEGREES);
			vy1 = y2 + (ARROW_LENGHT + lineWidth) * Math.Sin(angle - ARROW_DEGREES);
			vx2 = x2 + (ARROW_LENGHT + lineWidth) * Math.Cos(angle + ARROW_DEGREES);
			vy2 = y2 + (ARROW_LENGHT + lineWidth) * Math.Sin(angle + ARROW_DEGREES);
			
			c.MoveTo(x2-xOffset,y2-yOffset);
			c.LineTo(vx1-xOffset,vy1-yOffset);
			c.MoveTo(x2-xOffset,y2-yOffset);
			c.LineTo(vx2-xOffset,vy2-yOffset);
			c.Stroke();
			c.Fill();		
    }
		
		private void DrawRectangle(Context c, double x1, double y1, double x2, double y2){	
			SetContextProperties(c);
			c.Rectangle(x1-xOffset,y1-yOffset,x2-x1,y2-y1);
			c.Stroke();
			c.Fill();
		}
		
		private void DrawCross(Context c, double x1, double y1, double x2, double y2){	
			DrawLine(c, x1, y1, x2, y2);
			DrawLine(c, x2, y1, x1, y2);
		}
		
		private void DrawCircle(Context c, double x1, double y1, double x2, double y2){	
			double xc,yc,radius,angle1,angle2;
			
			xc=x1+(x2-x1)/2 - xOffset;
			yc=y1+(y2-y1)/2 - yOffset;
			radius = Math.Sqrt(Math.Pow((x2-x1),2)+
			                   Math.Pow((y2-y1),2));
			radius /=2;
			angle1 = 0.0 *  (Math.PI/180);
			angle2 = 360.0 *  (Math.PI/180);
			
			SetContextProperties(c);
			c.Arc(xc,yc,radius,angle1,angle2);
			
			c.Stroke();
			c.Fill();
		}			

		protected virtual void OnDrawingareaExposeEvent (object o, Gtk.ExposeEventArgs args)
		{
			if (!loaded)
				return;
			drawingarea.GdkWindow.Clear();
					
			using (Context c = CairoHelper.Create(drawingarea.GdkWindow)){
				c.SetSourceSurface(source,xOffset,yOffset);
				c.Paint();
				if (visible){
					c.SetSourceSurface(drawings,xOffset,yOffset);
					c.PaintWithAlpha(transparency);					
				}
				
				//Preview
				if (preview){
				switch (selectedTool){
						case DrawTool.LINE:
							DrawLine(c,initialPoint.X+xOffset,initialPoint.Y+yOffset,finalPoint.X+xOffset,finalPoint.Y+yOffset);
							DrawArrow(c,initialPoint.X+xOffset,initialPoint.Y+yOffset,finalPoint.X+xOffset,finalPoint.Y+yOffset);
							break;
						case DrawTool.RECTANGLE:
							DrawRectangle(c,initialPoint.X+xOffset,initialPoint.Y+yOffset,finalPoint.X+xOffset,finalPoint.Y+yOffset);
							break;
						case DrawTool.CIRCLE:
							DrawCircle(c,initialPoint.X+xOffset,initialPoint.Y+yOffset,finalPoint.X+xOffset,finalPoint.Y+yOffset);
							break;
						case DrawTool.CROSS:
							DrawCross(c,initialPoint.X+xOffset,initialPoint.Y+yOffset,finalPoint.X+xOffset,finalPoint.Y+yOffset);
							break;
						default:
							break;						
					}
				}
			}	
		}
		
		protected virtual void OnDrawingareaButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			preview = true;
			initialPoint = new Cairo.PointD(args.Event.X,args.Event.Y);
			
			if (selectedTool == DrawTool.PEN || selectedTool == DrawTool.ERASER ){
				lastx = args.Event.X;
				lasty = args.Event.Y;
			
				if (args.Event.Button == 1){
					using (Context c = new Context(drawings)){
						DrawLine(c,args.Event.X,args.Event.Y,args.Event.X,args.Event.Y);
					}
				}
			}
		}

		protected virtual void OnDrawingareaButtonReleaseEvent (object o, Gtk.ButtonReleaseEventArgs args)
		{
			preview = false;
			finalPoint = new Cairo.PointD(args.Event.X,args.Event.Y);
			using (Context c = new Context(drawings)){
				switch (selectedTool){
					case DrawTool.LINE:
						DrawLine(c,initialPoint.X,initialPoint.Y,finalPoint.X,finalPoint.Y);
						DrawArrow(c,initialPoint.X,initialPoint.Y,finalPoint.X,finalPoint.Y);
						break;
					case DrawTool.RECTANGLE:
						DrawRectangle(c,initialPoint.X,initialPoint.Y,finalPoint.X,finalPoint.Y);
						break;
					case DrawTool.CIRCLE:
						DrawCircle(c,initialPoint.X,initialPoint.Y,finalPoint.X,finalPoint.Y);
						break;
					case DrawTool.CROSS:
						DrawCross(c,initialPoint.X,initialPoint.Y,finalPoint.X,finalPoint.Y);
						break;
					default: //ERASER and PEN
						lastx=0;
						lasty=0;
						break;						
				}
			}
			QueueDraw();			
		}

		protected virtual void OnDrawingareaMotionNotifyEvent (object o, Gtk.MotionNotifyEventArgs args)
		{
			finalPoint = new Cairo.PointD(args.Event.X,args.Event.Y);
			
			if (selectedTool == DrawTool.PEN || selectedTool == DrawTool.ERASER){
				using (Context c = new Context(drawings)){
					DrawLine (c,lastx,lasty,args.Event.X,args.Event.Y);
				}
				lastx = args.Event.X;
				lasty = args.Event.Y;
			}
			QueueDraw();
		}

		protected virtual void OnDrawingareaSizeAllocated (object o, Gtk.SizeAllocatedArgs args)
		{
			// Center the source in the drawing area if its size
			// is smaller than the drawing area one
			if (args.Allocation.Width > sourceWidth)
				xOffset = (Allocation.Width - sourceWidth) / 2;
			else 
				xOffset = 0;

			if (args.Allocation.Height > sourceHeight)
				yOffset = (Allocation.Height -sourceHeight) / 2;
			else
				yOffset = 0;		
		}

		protected virtual void OnDrawingareaConfigureEvent (object o, Gtk.ConfigureEventArgs args)
		{
			// We can't set the cursor until the Gdk.Window exists
			DrawTool = selectedTool;
			if (loaded)
				GdkWindow.Resize(sourceWidth,sourceHeight);
		}	
	}
}
