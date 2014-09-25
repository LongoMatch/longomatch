// 
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using LongoMatch.Core.Common;
using LongoMatch.Gui;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Dialog
{
	public partial class VideoConversionTool : Gtk.Dialog
	{
		ListStore store;
		ListStore stdStore;
		ListStore bitStore;
		uint maxHeight;
		VideoStandard selectedVideoStandard;
		VideoStandard[] supportedVideoStandards;

		public VideoConversionTool ()
		{
			this.Build ();
			SetTreeView ();
			buttonOk.Sensitive = false;
			Files = new List<MediaFile> ();
			supportedVideoStandards = VideoStandards.Transcode;
			maxHeight = supportedVideoStandards [0].Height;
			mediafilechooser1.FileChooserMode = FileChooserMode.File;
			mediafilechooser1.ChangedEvent += HandleFileChanges;
			FillStandards ();
			FillBitrates ();
			addbutton.Clicked += OnAddbuttonClicked;
			removebutton.Clicked += OnRemovebuttonClicked;
			buttonOk.Clicked += OnButtonOkClicked;
		}

		public List<MediaFile> Files {
			get;
			set;
		}

		public EncodingSettings EncodingSettings {
			get;
			set;
		}

		void CheckStatus ()
		{
			buttonOk.Sensitive = mediafilechooser1.CurrentPath != null && Files.Count != 0;
		}

		void SetTreeView ()
		{
			TreeViewColumn mediaFileCol = new TreeViewColumn ();
			mediaFileCol.Title = Catalog.GetString ("Input files");
			CellRendererText mediaFileCell = new CellRendererText ();
			mediaFileCol.PackStart (mediaFileCell, true);
			mediaFileCol.SetCellDataFunc (mediaFileCell, new TreeCellDataFunc (RenderMediaFile));
			treeview1.AppendColumn (mediaFileCol);
			
			store = new ListStore (typeof(MediaFile));
			treeview1.Model = store;
			
		}

		void FillStandards ()
		{
			int index = 0, active = 0;

			stdStore = new ListStore (typeof(string), typeof(VideoStandard));
			foreach (VideoStandard std in supportedVideoStandards) {
				if (std.Height <= maxHeight) {
					stdStore.AppendValues (std.Name, std);
					if (std == selectedVideoStandard) {
						active = index; 
					}
					index ++;
				}
			}
			sizecombobox.Model = stdStore;
			sizecombobox.Active = active;
		}

		void FillBitrates ()
		{
			bitStore = new ListStore (typeof(string), typeof(EncodingQuality));
			foreach (EncodingQuality qual in EncodingQualities.Transcode) {
				bitStore.AppendValues (qual.Name, qual);
			}
			bitratecombobox.Model = bitStore;
			bitratecombobox.Active = 1;
		}

		protected void OnAddbuttonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;

			var msg = Catalog.GetString ("Add file");
			List<string> paths = FileChooserHelper.OpenFiles (this, msg, null,
			                                                  Config.HomeDir, null, null);
			List<string> errors = new List<string> ();
			foreach (string path in paths) {
				try {
					MediaFile file = Config.MultimediaToolkit.DiscoverFile (path, false);
					store.AppendValues (file);
					Files.Add (file);
				} catch (Exception) {
					errors.Add (path);
				}
			}
			if (errors.Count != 0) {
				string s = Catalog.GetString ("Error adding files:");
				foreach (string p in errors) {
					s += '\n' + p;
				}
				GUIToolkit.Instance.ErrorMessage (s);
			}
			CheckStatus ();

			maxHeight = Files.Max (f => f.VideoHeight);
			sizecombobox.GetActiveIter (out iter);
			selectedVideoStandard = stdStore.GetValue (iter, 1) as VideoStandard;
			FillStandards ();
		}

		void HandleFileChanges (object sender, EventArgs e)
		{
			CheckStatus ();
		}

		protected void OnRemovebuttonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			treeview1.Selection.GetSelected (out iter);
			Files.Remove (store.GetValue (iter, 0) as MediaFile);
			CheckStatus ();
			store.Remove (ref iter);
		}

		private void RenderMediaFile (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MediaFile file = (MediaFile)store.GetValue (iter, 0);

			(cell as Gtk.CellRendererText).Text = String.Format ("{0} {1}x{2} (Video:'{3}' Audio:'{4}')",
			                                                     System.IO.Path.GetFileName (file.FilePath),
			                                                     file.VideoWidth, file.VideoHeight,
			                                                     file.VideoCodec, file.AudioCodec);
		}

		protected void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			EncodingSettings encSettings;
			EncodingQuality qual;
			TreeIter iter;
			VideoStandard std;
			uint fps_n, fps_d;
			
			sizecombobox.GetActiveIter (out iter);
			std = (VideoStandard)stdStore.GetValue (iter, 1);

			bitratecombobox.GetActiveIter (out iter);
			qual = bitStore.GetValue (iter, 1) as EncodingQuality;
			
			var rates = new HashSet<uint> (Files.Select (f => (uint)f.Fps));
			if (rates.Count == 1) {
				fps_n = rates.First ();
				fps_d = 1;
			} else {
				fps_n = Config.FPS_N;
				fps_d = Config.FPS_D;
			}
			
			if (fps_n == 50) {
				fps_n = 25;
			} else if (fps_n == 60) {
				fps_n = 30;
			}
			encSettings = new EncodingSettings (std, EncodingProfiles.MP4, qual, fps_n, fps_d,
			                                    mediafilechooser1.CurrentPath, true, false, 0);
			
			EncodingSettings = encSettings;
			Respond (ResponseType.Ok);
		}
	}
}

