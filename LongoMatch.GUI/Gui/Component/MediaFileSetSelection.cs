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
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using Gtk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class MediaFileSetSelection : Gtk.Bin
	{
		MediaFileSet fileSet;
		bool ignoreChanges;

		public MediaFileSetSelection ()
		{
			this.Build ();
			frame2.NoShowAll = true;
			frame3.NoShowAll = true;
			frame4.NoShowAll = true;
			mediafilechooser1.ChangedEvent += HandleFileChangedEvent;
			mediafilechooser2.ChangedEvent += HandleFileChangedEvent;
			mediafilechooser3.ChangedEvent += HandleFileChangedEvent;
			mediafilechooser4.ChangedEvent += HandleFileChangedEvent;
			delfile2button.Clicked += HandleFileRemoved;
			delfile3button.Clicked += HandleFileRemoved;
			delfile4button.Clicked += HandleFileRemoved;
			filetable.RowSpacing = StyleConf.NewTableHSpacing;
			filetable.ColumnSpacing = StyleConf.NewTeamsSpacing; 
		}

		public MediaFileSet FileSet {
			set {
				fileSet = value;
				UpdateMediaFile (mediafilechooser1);
				UpdateMediaFile (mediafilechooser2);
				UpdateMediaFile (mediafilechooser3);
				UpdateMediaFile (mediafilechooser4);
			}
			get {
				return fileSet;
			}
		}

		void UpdateMediaFile (MediaFileChooser filechooser, MediaFile file = null, bool delete = false)
		{
			MediaFileAngle angle = MediaFileAngle.Angle1;
			Button delbutton = null;

			ignoreChanges = true;
			if (filechooser == mediafilechooser1) {
				angle = MediaFileAngle.Angle1;
			} else if (filechooser == mediafilechooser2) {
				delbutton = delfile2button;
				angle = MediaFileAngle.Angle2;
			} else if (filechooser == mediafilechooser3) {
				delbutton = delfile3button;
				angle = MediaFileAngle.Angle3;
			} else if (filechooser == mediafilechooser4) {
				delbutton = delfile4button;
				angle = MediaFileAngle.Angle4;
			}
			
			if (delete) {
				FileSet.SetAngle (angle, null);
				filechooser.MediaFile = null;
			} else {
				if (file == null) {
					filechooser.MediaFile = FileSet.GetAngle (angle);
				} else {
					FileSet.SetAngle (angle, file);
					filechooser.MediaFile = file;
				}
			}

			if (delbutton != null) {
				delbutton.Visible = filechooser.MediaFile != null;
			}


			ignoreChanges = false;
		}

		void HandleFileChangedEvent (object sender, EventArgs e)
		{
			if (ignoreChanges) {
				return;
			}
			MediaFileChooser filechooser = sender as MediaFileChooser;
			UpdateMediaFile (filechooser, filechooser.MediaFile);
		}

		void HandleFileRemoved (object sender, EventArgs e)
		{
			MediaFileChooser filechooser = null;

			if (sender == delfile2button) {
				filechooser = mediafilechooser2;
			} else if (sender == delfile3button) {
				filechooser = mediafilechooser3;
			} else if (sender == delfile4button) {
				filechooser = mediafilechooser4;
			}
			UpdateMediaFile (filechooser, null, true);
		}

	}
}

