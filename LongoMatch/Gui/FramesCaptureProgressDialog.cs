// FramesCaptureProgressDialog.cs
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
		private FramesProgressHandler progressHandler;
		
		public FramesCaptureProgressDialog(FramesSeriesCapturer capturer)
		{
			this.Build();
			this.Deletable = false;
			this.capturer = capturer;
			progressHandler = new FramesProgressHandler(Update);
			capturer.Progress += progressHandler;			
			capturer.Start();			
		}				
		
		protected virtual void Update (int actual, int total){
			if (actual <= total){
				progressbar.Text= Catalog.GetString("Capturing frame: ")+actual+"/"+total;
				progressbar.Fraction = (double)actual/(double)total;
			}
			if (actual == total){
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
