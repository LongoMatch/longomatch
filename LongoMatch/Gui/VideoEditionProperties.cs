// VideoEditionProperties.cs
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
using LongoMatch.Video.Editor;

namespace LongoMatch.Gui.Dialog
{
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class VideoEditionProperties : Gtk.Dialog
	{
		VideoQuality vq;

		
		public VideoEditionProperties()
		{

			this.Build();
			
			
		}
		
		public VideoQuality VideoQuality{
			get{
				return this.vq;
			}
		}
		
		public string Filename{
			get{
				return this.fileentry.Text;
			}
		}

		

		protected virtual void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			if (this.combobox1.ActiveText == Catalog.GetString("Low")){
				this.vq = VideoQuality.Low;
			}
			if (this.combobox1.ActiveText == Catalog.GetString("Normal")){
				this.vq = VideoQuality.Normal;
			}
			if (this.combobox1.ActiveText == Catalog.GetString("Good")){
				this.vq = VideoQuality.Good;
			}
			if (this.combobox1.ActiveText == Catalog.GetString("Extra")){
				this.vq = VideoQuality.Extra;
			}
						
			this.Hide();
		}


		protected virtual void OnOpenbuttonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save Video As ..."),
			                                                   this,
			                                                   FileChooserAction.Save,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.VideosDir());
			fChooser.CurrentName = "NewVideo.avi";
			fChooser.DoOverwriteConfirmation = true;
			FileFilter filter = new FileFilter();
			filter.Name = "Avi File";
			filter.AddPattern("*.avi");
			fChooser.Filter = filter;
			if (fChooser.Run() == (int)ResponseType.Accept){						
				this.fileentry.Text = fChooser.Filename;
			}
		
			fChooser.Destroy();
		}
	}
}
