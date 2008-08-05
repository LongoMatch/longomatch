// TimeReferenceWidget.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using Gtk;
using Gdk;
using Cairo;

namespace LongoMatch
{
	
	
	public partial class TimeReferenceWidget : Gtk.DrawingArea
	{
		ushort frameRate;
		uint currentFrame;
		uint frames;
		uint pixelRatio=1;//Número de frames por poixel
		public TimeReferenceWidget(uint frames,ushort frameRate)
		{
			this.frameRate = frameRate;
			this.frames = frames;
			this.pixelRatio = 1;
			this.Size((int)(frames/pixelRatio+25), 30);
			this.HeightRequest= 30;

		}
				
		public uint CurrentFrame{
			get{return this.currentFrame;}
			set{this.currentFrame = value;}
		}
		
		public uint PixelRatio{
			get {return pixelRatio;}
			set {
				this.pixelRatio = value;
				this.Size((int)(frames/pixelRatio),30);
			
										
			}
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{
			int height;
			int width;	
			Time time;
			
			
			evnt.Window.GetSize(out width, out height);	
			evnt.Window.Resize((int)(frames/pixelRatio), height);
			this.Size((int)(frames/pixelRatio),height);
			evnt.Window.GetSize(out width, out height);	
			
			time = new Time();
			
			using (Cairo.Context g = Gdk.CairoHelper.Create (evnt.Window)){
				
				g.Color = new Cairo.Color(1,1,1);
				g.MoveTo(currentFrame/pixelRatio,height);
				/*g.LineTo(currentFrame/pixelRatio+5,height-15);
				g.LineTo(currentFrame/pixelRatio-5,height-15);
				g.ClosePath();
				g.Fill();
				g.Stroke();*/
				g.MoveTo(new PointD(0,height));
				g.LineTo(new PointD(width,height));
				g.LineWidth = 2;
				g.Stroke();
				g.MoveTo(new PointD(0,height-20));
				g.ShowText("0");
				
				for (int i=10*frameRate; i<=frames/pixelRatio; ){
					g.MoveTo(new PointD(i,height));
					g.LineTo(new PointD(i,height-10));
					g.LineWidth = 2;
					g.Stroke();	
					g.MoveTo(new PointD(i-13,height-20));
					time.MSeconds = (int)(i/frameRate*pixelRatio);
					g.ShowText(time.ToSecondsString());
					i=i+10*frameRate;				
				}
				for (int i=0; i<=frames/pixelRatio; ){
					g.MoveTo(new PointD(i,height));
					g.LineTo(new PointD(i,height-5));
					g.LineWidth = 1;
					g.Stroke();					
					i=i+frameRate;					
				}
				
				
			}
			return base.OnExposeEvent (evnt);
		}

		

	}
}
