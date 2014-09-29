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
using Mono.Unix;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Common;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MediaFileChooser : Gtk.Bin
	{
		public event EventHandler ChangedEvent;

		MediaFile mediaFile;
		string path;

		public MediaFileChooser ()
		{
			this.Build ();

			addbuttonimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-browse", Gtk.IconSize.Button, 0);
			FilterName = "MP4";
			FilterExtensions = new string[] { "*.mp4" }; 
			FileChooserMode = FileChooserMode.MediaFile;
			UpdateFile ();
			addbutton.Clicked += HandleClicked;
		}

		public FileChooserMode FileChooserMode {
			get;
			set;
		}

		public string CurrentPath {
			set {
				path = value;
				UpdateFile ();
			}
			get {
				return path;
			}
		}

		public string FilterName {
			get;
			set;
		}

		public string[] FilterExtensions {
			get;
			set;
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
				fileentry.TooltipText = mediaFile.FilePath;
			} else if (path != null) {
				fileentry.Text = System.IO.Path.GetFileName (path);
				fileentry.TooltipText = path;
			} else {
				if (FileChooserMode == FileChooserMode.Directory) {
					fileentry.Text = Catalog.GetString ("Select folder...");
				} else {
					fileentry.Text = Catalog.GetString ("Select file...");
				}
			}
		}

		void HandleClicked (object sender, EventArgs e)
		{
			if (FileChooserMode == FileChooserMode.MediaFile) {
				MediaFile = Misc.OpenFile (this);
			} else if (FileChooserMode == FileChooserMode.File) {
				string filename = String.Format ("LongoMatch-{0}.mp4",
				                                 DateTime.Now.ToShortDateString ().Replace ('/', '-'));
				CurrentPath = FileChooserHelper.SaveFile (this, Catalog.GetString ("Output file"), filename,
				                                          Config.LastRenderDir, FilterName, FilterExtensions);
				if (CurrentPath != null) {
					Config.LastRenderDir = System.IO.Path.GetDirectoryName (CurrentPath);
				}
			} else if (FileChooserMode == FileChooserMode.Directory) {
				string filename = String.Format ("LongoMatch-{0}",
				                                 DateTime.Now.ToShortDateString ().Replace ('/', '-'));
				CurrentPath = FileChooserHelper.SelectFolder (this, Catalog.GetString ("Output folder"), filename,
				                                              Config.LastRenderDir, null, null);
			}
			if (ChangedEvent != null) {
				ChangedEvent (this, null);
			}
		}
	}
}

