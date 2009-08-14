// FramesCaptureProgressDialog.cs
//
//  Copyright (C) 2008-2009 Andoni Morales Alastruey
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
using Mono.Unix;
using LongoMatch.Video.Utils;
using LongoMatch.Video.Handlers;

namespace LongoMatch.Gui.Dialog
{
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class FramesCaptureProgressDialog : Gtk.Dialog
	{
		private FramesSeriesCapturer capturer;
		private const int THUMBNAIL_MAX_HEIGHT=250;
		private const int THUMBNAIL_MAX_WIDTH=300;
		
		public FramesCaptureProgressDialog(FramesSeriesCapturer capturer)
		{
			this.Build();
			this.Deletable = false;
			this.capturer = capturer;
			capturer.Progress += new FramesProgressHandler(Update);			
			capturer.Start();	
		}				
		
		protected virtual void Update (int actual, int total,Pixbuf frame){
			if (actual <= total){
				progressbar.Text= Catalog.GetString("Capturing frame: ")+actual+"/"+total;
				progressbar.Fraction = (double)actual/(double)total;
				if (frame != null){
					int h = frame.Height;
					int w = frame.Width;
					double rate = (double)w/(double)h;
					if (h>w)
						image.Pixbuf = frame.ScaleSimple((int)(THUMBNAIL_MAX_HEIGHT*rate),THUMBNAIL_MAX_HEIGHT,InterpType.Bilinear);
					else
						image.Pixbuf = frame.ScaleSimple(THUMBNAIL_MAX_WIDTH,(int)(THUMBNAIL_MAX_WIDTH/rate),InterpType.Bilinear);
				}
			}
			if (actual == total){
				progressbar.Text= Catalog.GetString("Done");
				cancelbutton.Visible = false;
				okbutton.Visible = true;
			}				
		}

		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			capturer.Cancel();
		}				
	}
}
