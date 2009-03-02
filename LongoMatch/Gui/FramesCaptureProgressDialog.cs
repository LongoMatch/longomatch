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
using Mono.Posix;
using LongoMatch.Video.Utils;

namespace LongoMatch.Gui.Dialog
{
	
	
	public partial class FramesCaptureProgressDialog : Gtk.Dialog
	{
		FramesSeriesCapturer capturer;
		
		public FramesCaptureProgressDialog(FramesSeriesCapturer capturer)
		{
			this.Build();
			this.capturer = capturer;
			capturer.Progress += new LongoMatch.Video.Handlers.FramesProgressHandler(Update);
			this.Deletable = false;
		}		
		
		
		public void Update (int actual, int total){
			progressbar.Text= Catalog.GetString("Capturing frame: ")+actual+"/"+total;
			progressbar.Fraction = (double)actual/(double)total;
			
		}

		protected virtual void OnButtonCancelClicked (object sender, System.EventArgs e)
		{
			capturer.Cancel();
			this.Destroy();

		}
		
		
		public void Run(){
			capturer.Start();
			this.Destroy();
			
		}
	}
		
	
}
