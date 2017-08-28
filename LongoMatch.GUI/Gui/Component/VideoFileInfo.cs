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
using Gtk;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class VideoFileInfo : Gtk.Bin, IView<MediaFileSetVM>
	{
		public event EventHandler Changed;

		const int PREVIEW_SIZE = 80;
		MediaFileSetVM viewModel;
		MediaFileVM mediaFile;
		bool disableChanges;

		public VideoFileInfo ()
		{
			this.Build ();
			eventbox3.ButtonPressEvent += HandleButtonPressEvent;
			HeightRequest = 100;
			snapshotimage.SetSize (100);
			filelabel.ModifyFg (StateType.Normal, Misc.ToGdkColor (App.Current.Style.PaletteText));
		}

		public MediaFileSetVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (MediaFileSetVM)viewModel;
		}

		public void SetMediaFileSet (MediaFileSetVM files, MediaFileVM file)
		{
			viewModel = files;
			SetMediaFile (file, true);
		}

		public void SetMediaFile (MediaFileVM file, bool editable = false)
		{
			mediaFile = file;
			disableChanges = !editable;
			UpdateMediaFile ();
		}

		void UpdateMediaFile ()
		{
			if (mediaFile == null || mediaFile.Model == null) {
				Visible = false;
				return;
			}
			namelabel.Text = mediaFile.Name;
			if (mediaFile.IsFakeCapture) {
				filelabel.Text = Catalog.GetString ("No video file associated yet for live project");
				snapshotimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-video-device-fake", PREVIEW_SIZE);
				table1.Visible = false;
				disableChanges = true;
				return;
			}
			table1.Visible = true;
			filelabel.Text = mediaFile.FilePath;
			if (mediaFile.Preview != null) {
				snapshotimage.Image = mediaFile.Preview;
			} else {
				snapshotimage.Image = App.Current.ResourcesLocator.LoadIcon ("lm-video-file", PREVIEW_SIZE);
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
			offsetlabel.Markup = String.Format ("<span foreground=\"{0}\">{1}: {2}</span>",
				App.Current.Style.PaletteActive.ToRGBString (false), Catalog.GetString ("Offset"),
				mediaFile.Offset.ToMSecondsString ());
		}

		async void HandleButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if (args.Event.Button != 1 || disableChanges) {
				return;
			}
			MediaFileVM file = await App.Current.EventsBroker.PublishWithReturn<ReplaceMediaFileEvent, MediaFileVM> (new ReplaceMediaFileEvent {
				OldFileSet = ViewModel,
				OldFile = mediaFile
			});
			mediaFile = file;
			UpdateMediaFile ();
			if (Changed != null) {
				Changed (this, new EventArgs ());
			}
		}
	}
}
