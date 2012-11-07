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
using LongoMatch.Common;
using LongoMatch.Store;
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
			set {
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
			
			if(Environment.OSVersion.Platform == PlatformID.Unix)
				this.CairoDraw(evnt,height,width);
			else
				this.GdkDraw(evnt,height,width);
			return base.OnExposeEvent(evnt);
		}

		private void CairoDraw(EventExpose evnt,int height,int width) {
			Time time = new Time();
			
			width = Math.Min (width,this.Allocation.Width);
			using(Cairo.Context g = Gdk.CairoHelper.Create(evnt.Window)) {
				Cairo.Color color = new Cairo.Color(0, 0, 0);
				/* Drawing position triangle */
				CairoUtils.DrawTriangle(g,CurrentFrame/pixelRatio-Scroll, height, 10, 15, color);
				/* Draw '0' */
				CairoUtils.DrawLine(g, 0-Scroll, height, width, height, 2, color);
				g.MoveTo(new PointD(0-Scroll,height-20));
				g.ShowText("0");

				for(int i=10*FrameRate; i<=frames/pixelRatio;) {
					CairoUtils.DrawLine(g, i-Scroll, height,i-Scroll,
					                    height-10, 2, color);
					g.MoveTo(new PointD(i-Scroll-13,height-20));
					time.Seconds = (int)(i/FrameRate*pixelRatio);
					g.ShowText(time.ToSecondsString());
					i=i+10*FrameRate;
				}
				for(int i=0; i<=frames/pixelRatio;) {
					CairoUtils.DrawLine(g, i-Scroll, height,i-Scroll,
					                    height-5, 1, color);
					i=i+FrameRate;
				}
			}
		}

		private void GdkDraw(EventExpose evnt,int height,int width) {
			Time time = new Time();
			width = Math.Min (width,this.Allocation.Width);
			layout.SetMarkup("0");
			this.GdkWindow.DrawLayout(this.Style.TextGC(StateType.Normal),0,height-23,layout);

			Gdk.Point topL= new Gdk.Point((int)(CurrentFrame/pixelRatio-Scroll-5),height-15);
			Gdk.Point topR= new Gdk.Point((int)(CurrentFrame/pixelRatio-Scroll+5),height-15);
			Gdk.Point bottom= new Gdk.Point((int)(CurrentFrame/pixelRatio-Scroll),height);
			this.GdkWindow.DrawPolygon(this.Style.TextGC(StateType.Normal),true,new Gdk.Point[] {topL,topR,bottom});

			for(int i=10*FrameRate; i<=frames/pixelRatio;) {
				// Drawing separator line
				evnt.Window.DrawLine(Style.DarkGC(StateType.Normal),i-(int)Scroll,height,i-(int)Scroll,height-10);
				time.Seconds = (int)(i/FrameRate*pixelRatio);
				layout.SetMarkup(time.ToSecondsString());
				this.GdkWindow.DrawLayout(this.Style.TextGC(StateType.Normal),i-(int)Scroll-13,height-23,layout);
				//g.ShowText(time.ToSecondsString());
				i=i+10*FrameRate;
			}

			for(int i=0; i<=frames/pixelRatio;) {
				evnt.Window.DrawLine(Style.DarkGC(StateType.Normal),i-(int)Scroll,height,i-(int)Scroll,height-5);
				i=i+FrameRate;
			}
			// Drawing main line
			evnt.Window.DrawLine(Style.DarkGC(StateType.Normal),0,height,width,height);
		}
	}
}
