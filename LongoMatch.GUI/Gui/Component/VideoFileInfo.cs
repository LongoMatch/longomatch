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
using System.Linq;
using Mono.Unix;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using Misc = LongoMatch.Gui.Helpers.Misc;
using Gtk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class VideoFileInfo : Gtk.Bin
	{
		public event EventHandler Changed;

		MediaFileSet fileSet;
		MediaFile mediaFile;
		bool disableChanges;
		
		public VideoFileInfo ()
		{
			this.Build ();
			eventbox3.ButtonPressEvent += HandleButtonPressEvent;
			HeightRequest = 100;
			filelabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (Config.Style.PaletteText));
		}

		public void SetMediaFileSet (MediaFileSet files, MediaFile file)
		{
			fileSet = files;
			SetMediaFile (file, true);
		}

		public void SetMediaFile (MediaFile file, bool editable = false)
		{
			mediaFile = file;
			disableChanges = !editable;
			UpdateMediaFile ();
		}

		void UpdateMediaFile ()
		{
			if (mediaFile == null) {
				Visible = false;
				return;
			}
			namelabel.Text = mediaFile.Name;
			if (mediaFile.FilePath == Constants.FAKE_PROJECT) {
				filelabel.Text = Catalog.GetString ("No video file associated yet for live project");
				snapshotimage.Pixbuf = Misc.LoadIcon ("longomatch-video-device-fake", 80);
				table1.Visible = false;
				disableChanges = true;
				return;
			}
			table1.Visible = true;
			filelabel.Text = mediaFile.FilePath;
			if (mediaFile.Preview != null) {
				snapshotimage.Pixbuf = mediaFile.Preview.Value;
			} else {
				snapshotimage.Pixbuf = Misc.LoadIcon ("longomatch-video-file", 80);
			}
			if (mediaFile.Duration != null) {
				durationlabel.Text = String.Format ("{0}: {1}", Catalog.GetString ("Duration"),
				                                    mediaFile.Duration.ToSecondsString ());
			} else {
				durationlabel.Text = Catalog.GetString ("Missing duration info, reload this file.");
			}
			formatlabel.Text = String.Format ("{0}: {1}x{2}@{3}fps", Catalog.GetString ("Format"),
			                                  mediaFile.VideoWidth, mediaFile.VideoHeight, mediaFile.Fps);
			videolabel.Text = String.Format ("{0}: {1}", Catalog.GetString ("Video codec"),
			                                 mediaFile.VideoCodec);
			audiolabel.Text = String.Format ("{0}: {1}", Catalog.GetString ("Audio codec"),
			                                 mediaFile.AudioCodec);
			containerlabel.Text = String.Format ("{0}: {1}", Catalog.GetString ("Container"),
			                                     mediaFile.Container);
			offsetlabel.Text = String.Format ("{0}: {1}", Catalog.GetString ("Offset"),
				mediaFile.Offset.ToMSecondsString ());
		}
		
		void HandleButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button != 1 || disableChanges) {
				return;
			}
			MediaFile file = Misc.OpenFile (this);
			if (file != null) {
				if (mediaFile != null) {
					file.Offset = mediaFile.Offset;
				}
				fileSet.Replace (mediaFile, file);
				mediaFile = file;
				UpdateMediaFile ();
				if (Changed != null) {
					Changed (this, new EventArgs ());
				}
			}
		}
	}
}

