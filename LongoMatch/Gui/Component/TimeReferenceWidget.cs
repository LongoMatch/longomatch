// TimeReferenceWidget.cs
//
//  Copyright (C2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gtk;
using Gdk;
using Cairo;
using LongoMatch.TimeNodes;
using Pango;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TimeReferenceWidget : Gtk.DrawingArea
	{
		private const int SECTION_HEIGHT = 30;
		double scroll;
		uint frames;
		uint pixelRatio=10;//Número de frames por pixel
		Pango.Layout layout;

		public TimeReferenceWidget()
		{
			Frames = 1;
			PixelRatio = 1;
			FrameRate = 1;
			
			this.HeightRequest= SECTION_HEIGHT;
			layout = new Pango.Layout(this.PangoContext);
		}

		public uint CurrentFrame {
			get;
			set;
		}
		
		public uint Frames {
			set{
				frames = value;
			}
		}
		
		public ushort FrameRate {
			set;
			get;
		}
		
		public double Scroll {
			get {
				return scroll;
			}
			set {
				scroll = value;
				QueueDraw();
			}
		}

		public uint PixelRatio {
			get {
				return pixelRatio;
			}
			set {
				pixelRatio = value;
			}
		}

		protected override bool OnExposeEvent(EventExpose evnt)
		{
			int height;
			int width;

			Gdk.Window win = evnt.Window;

			win.GetSize(out width, out height);
			win.Resize((int)(frames/pixelRatio), height);
			win.GetSize(out width, out height);

			if (Environment.OSVersion.Platform == PlatformID.Unix)
				this.CairoDraw(evnt,height,width);
			else
				this.GdkDraw(evnt,height,width);




			return base.OnExposeEvent(evnt);
		}
		private void CairoDraw(EventExpose evnt,int height,int width) {
			Time time = new Time();
			using(Cairo.Context g = Gdk.CairoHelper.Create(evnt.Window)) {
				// Drawing main line
				g.Color = new Cairo.Color(0,0,0);
				g.MoveTo(currentFrame/pixelRatio,height);
				g.LineTo(currentFrame/pixelRatio+5,height-15);
				g.LineTo(currentFrame/pixelRatio-5,height-15);
				g.ClosePath();
				g.Fill();
				g.Stroke();
				g.MoveTo(new PointD(0,height));
				g.LineTo(new PointD(width,height));
				g.LineWidth = 2;
				g.Stroke();
				g.MoveTo(new PointD(0,height-20));
				g.ShowText("0");

				for (int i=10*frameRate; i<=frames/pixelRatio;) {
					g.MoveTo(new PointD(i,height));
					g.LineTo(new PointD(i,height-10));
					g.LineWidth = 2;
					g.Stroke();


					g.MoveTo(new PointD(i-13,height-20));
					time.MSeconds = (int)(i/frameRate*pixelRatio);
					g.ShowText(time.ToSecondsString());
					i=i+10*frameRate;
				}
				for (int i=0; i<=frames/pixelRatio;) {
					g.MoveTo(new PointD(i,height));
					g.LineTo(new PointD(i,height-5));
					g.LineWidth = 1;
					g.Stroke();
					i=i+frameRate;
				}
			}
		}
		private void GdkDraw(EventExpose evnt,int height,int width) {
			Time time = new Time();
			layout.SetMarkup("0");
			this.GdkWindow.DrawLayout(this.Style.TextGC(StateType.Normal),0,height-23,layout);

			Gdk.Point topL= new Gdk.Point((int)(currentFrame/pixelRatio-5),height-15);
			Gdk.Point topR= new Gdk.Point((int)(currentFrame/pixelRatio+5),height-15);
			Gdk.Point bottom= new Gdk.Point((int)(currentFrame/pixelRatio),height);
			this.GdkWindow.DrawPolygon(this.Style.TextGC(StateType.Normal),true,new Gdk.Point[] {topL,topR,bottom});

			for (int i=10*frameRate; i<=frames/pixelRatio;) {
				// Drawing separator line
				evnt.Window.DrawLine(Style.DarkGC(StateType.Normal),i,height,i,height-10);
				time.MSeconds = (int)(i/frameRate*pixelRatio);
				layout.SetMarkup(time.ToSecondsString());
				this.GdkWindow.DrawLayout(this.Style.TextGC(StateType.Normal),i-13,height-23,layout);
				//g.ShowText(time.ToSecondsString());
				i=i+10*frameRate;
			}

			for (int i=0; i<=frames/pixelRatio;) {
				evnt.Window.DrawLine(Style.DarkGC(StateType.Normal),i,height,i,height-5);
				i=i+frameRate;
			}
			// Drawing main line
			evnt.Window.DrawLine(Style.DarkGC(StateType.Normal),0,height,width,height);
		}


	}
}
