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
using System.IO;
using Mono.Unix;
using LongoMatch.Store;
using LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MediaFileChooser : Gtk.Bin
	{
		public event EventHandler ChangedEvent;
		MediaFile mediaFile;
		string file;

		public MediaFileChooser ()
		{
			this.Build ();
			MediaFileMode = true;
			UpdateFile ();
			addbutton.Clicked += HandleClicked;
		}

		public bool MediaFileMode {
			get;
			set;
		}
		
		public string File {
			set {
				file = value;
				UpdateFile ();
			}
			get {
				return file;
			}
		}
		
		public MediaFile MediaFile {
			get {
				return mediaFile;
			}
			set {
				mediaFile = value;
				UpdateFile ();
			}
		}
		
		void UpdateFile ()
		{
			if (mediaFile != null) {
				fileentry.Text = System.IO.Path.GetFileName (mediaFile.FilePath);
			} else if (file != null) {
				fileentry.Text = System.IO.Path.GetFileName (file);
			} else {
				fileentry.Text = Catalog.GetString ("Select file...");
			}
		}
		
		void HandleClicked (object sender, EventArgs e)
		{
			if (MediaFileMode) {
				MediaFile = Misc.OpenFile (this);
			} else {
				File = FileChooserHelper.SaveFile (this, Catalog.GetString ("Output file"),
				                                   "Capture.mp4", Config.VideosDir, "MP4",
				                                   new string[] { "*.mp4" });
			}
			if (ChangedEvent != null) {
				ChangedEvent (this, null);
			}
		}
	}
}

