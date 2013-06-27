// VideoEditionProperties.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gtk;
using Mono.Unix;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Common;
using LongoMatch.Common;
using LongoMatch.Gui.Helpers;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Dialog
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(false)]
	public partial class VideoEditionProperties : Gtk.Dialog
	{
		EncodingSettings encSettings;
		ListStore stdStore, encStore, qualStore;

		#region Constructors
		public VideoEditionProperties()
		{
			this.Build();
			encSettings = new EncodingSettings();
			stdStore = Misc.FillImageFormat (sizecombobox, Config.RenderVideoStandard);
			encStore = Misc.FillEncodingFormat (formatcombobox, Config.RenderEncodingProfile);
			qualStore = Misc.FillQuality (qualitycombobox, Config.RenderEncodingQuality);
		}
		#endregion

		#region Properties

		public EncodingSettings EncodingSettings{
			get {
				return encSettings;
			}
		}

		public bool EnableAudio {
			get {
				return audiocheckbutton.Active;
			}
		}

		public bool TitleOverlay {
			get {
				return descriptioncheckbutton.Active;
			}
		}
		
		public String OutputDir {
			get;
			set;
		}
		
		public bool SplitFiles {
			get;
			set;
		}

		#endregion Properties

		#region Private Methods

		private string GetExtension() {
			TreeIter iter;
			formatcombobox.GetActiveIter(out iter);
			return ((EncodingProfile) encStore.GetValue(iter, 1)).Extension;
		}

		#endregion

		
		protected virtual void OnButtonOkClicked(object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			/* Get size info */
			sizecombobox.GetActiveIter(out iter);
			encSettings.VideoStandard = (VideoStandard) stdStore.GetValue(iter, 1);
			
			/* Get encoding profile info */
			formatcombobox.GetActiveIter(out iter);
			encSettings.EncodingProfile = (EncodingProfile) encStore.GetValue(iter, 1);
			
			/* Get quality info */
			qualitycombobox.GetActiveIter(out iter);
			encSettings.EncodingQuality = (EncodingQuality) qualStore.GetValue(iter, 1);
			
			encSettings.OutputFile = filelabel.Text;
			
			encSettings.Framerate_n = Config.FPS_N;
			encSettings.Framerate_d = Config.FPS_D;
			
			encSettings.TitleSize = 20; 
			
			Hide();
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save Video As ..."),
			                this,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.VideosDir);
			fChooser.CurrentName = "NewVideo."+GetExtension();
			fChooser.DoOverwriteConfirmation = true;
			FileFilter filter = new FileFilter();
			filter.Name = "Multimedia Files";
			filter.AddPattern("*.mkv");
			filter.AddPattern("*.mp4");
			filter.AddPattern("*.ogg");
			filter.AddPattern("*.avi");
			filter.AddPattern("*.mpg");
			filter.AddPattern("*.vob");
			fChooser.Filter = filter;
			if(fChooser.Run() == (int)ResponseType.Accept) {
				filelabel.Text = fChooser.Filename;
			}
			fChooser.Destroy();
		}
		
		protected virtual void OnButtonCancelClicked(object sender, System.EventArgs e)
		{
			this.Destroy();
		}


		protected void OnSplitfilesbuttonClicked (object sender, System.EventArgs e)
		{
			dirbox.Visible = splitfilesbutton.Active;
			filebox.Visible = !splitfilesbutton.Active;
			SplitFiles = splitfilesbutton.Active;
		}

		protected void OnOpendirbuttonClicked (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new  FileChooserDialog(Catalog.GetString("Output folder ..."),
			                this,
			                FileChooserAction.SelectFolder,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.VideosDir);
			fChooser.CurrentName = "Playlist";
			if(fChooser.Run() == (int)ResponseType.Accept) {
				dirlabel.Text = fChooser.Filename;
				OutputDir = fChooser.Filename;
			}
			fChooser.Destroy();
		}
	}
}
