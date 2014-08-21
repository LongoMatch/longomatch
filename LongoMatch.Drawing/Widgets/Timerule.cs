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
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Interfaces.Drawing;
using LongoMatch.Interfaces;
using System.IO;

namespace LongoMatch.Drawing.Widgets
{
	public class Timerule:Canvas
	{
		const int BIG_LINE_HEIGHT = 15;
		const int SMALL_LINE_HEIGHT = 5;
		const int TEXT_WIDTH = 20;
		const int TIME_SPACING = 100;
		ISurface needle;

		public Timerule (IWidget widget):base (widget)
		{
			SecondsPerPixel = 0.1;
			CurrentTime = new Time (0);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (needle != null) {
				needle.Dispose ();
				needle = null;
			}
		}

		public double Scroll {
			set;
			protected get;
		}

		public Time Duration {
			set;
			protected get;
		}

		public Time CurrentTime {
			get;
			set;
		}

		public double SecondsPerPixel {
			set;
			get;
		}

		public override void Draw (IContext context, Area area)
		{
			double height = widget.Height;
			double width = widget.Width;
			double tpos;
			
			if (Duration == null) {
				return;
			}
			
			if (needle == null) {
				string  path = Path.Combine (Config.IconsDir, StyleConf.TimelineNeedleResource); 
				Image img = Image.LoadFromFile (path);
				needle = tk.CreateSurface (img.Width, img.Height, img);
			}

			tk.Context = context;
			tk.Begin ();
			tk.Clear (Config.Style.PaletteBackgroundDark);
			
			tk.StrokeColor = Config.Style.PaletteWidgets;
			tk.FillColor = Config.Style.PaletteWidgets;
			tk.LineWidth = Constants.TIMELINE_LINE_WIDTH;
			tk.FontSlant = FontSlant.Normal;
			tk.FontSize = 12;
			tk.DrawLine (new Point (0, height), new Point (width, height));
		
			/* Draw big lines each 10 * secondsPerPixel */
			for (int i=0; i <= Duration.Seconds / SecondsPerPixel; i += TIME_SPACING) {
				double pos = i - Scroll;
				tk.DrawLine (new Point (pos, height),
				             new Point (pos, height - BIG_LINE_HEIGHT));
				tk.DrawText (new Point (pos - TEXT_WIDTH / 2, 0), TEXT_WIDTH, height - BIG_LINE_HEIGHT - 4,
				             new Time { Seconds = (int) (i * SecondsPerPixel) }.ToSecondsString ());
			}
			
			/* Draw small lines each 1 * secondsPerPixel */
			for (int i=0; i<= Duration.Seconds / SecondsPerPixel; i+= TIME_SPACING / 10) {
				double pos;
				
				if (i % TIME_SPACING == 0)
					continue;
					
				pos = i - Scroll;
				tk.DrawLine (new Point (pos, height),
				             new Point (pos, height - SMALL_LINE_HEIGHT));
			}
			
			/* Draw position triangle */
			tpos = Utils.TimeToPos (CurrentTime, SecondsPerPixel);
			tpos -= Scroll;
			tpos -= needle.Width / 2;
			tk.DrawSurface (needle, new Point (tpos, widget.Height - needle.Height));
			tk.End ();
			tk.Context = null;
		}
	}
}

