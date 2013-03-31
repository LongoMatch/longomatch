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
using System.Collections.Generic;
using Gtk;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Gui;
using LongoMatch.Video.Utils;
using LongoMatch.Store;
using LongoMatch.Interfaces;

namespace LongoMatch.Gui.Dialog
{
	public partial class VideoConversionTool : Gtk.Dialog
	{
		ListStore store;
		ListStore stdStore;
		string outputFile;
		
		public VideoConversionTool ()
		{
			this.Build ();
			SetTreeView ();
			FillStandards();
			buttonOk.Sensitive = false;
			Files = new List<MediaFile>();
		}
		
		public List<MediaFile> Files {
			get;
			set;
		}
		
		public EncodingSettings EncodingSettings {
			get;
			set;
		}
		
		void CheckStatus () {
			buttonOk.Sensitive = outputFile != null && Files.Count != 0;
		}
		
		void SetTreeView () {
			TreeViewColumn mediaFileCol = new TreeViewColumn();
			mediaFileCol.Title = Catalog.GetString("Input files");
			CellRendererText mediaFileCell = new CellRendererText();
			mediaFileCol.PackStart(mediaFileCell, true);
			mediaFileCol.SetCellDataFunc(mediaFileCell, new TreeCellDataFunc(RenderMediaFile));
			treeview1.AppendColumn(mediaFileCol);
			
			store = new ListStore(typeof(PreviewMediaFile));
			treeview1.Model = store;
		}
		
		void FillStandards () {
			stdStore = new ListStore(typeof(string), typeof (VideoStandard));
			stdStore.AppendValues(VideoStandards.P720.Name, VideoStandards.P720);
			stdStore.AppendValues(VideoStandards.P480.Name, VideoStandards.P480);
			sizecombobox.Model = stdStore;
			sizecombobox.Active = 0;
		}
		
		protected void OnAddbuttonClicked (object sender, System.EventArgs e)
		{
			List<string> paths = GUIToolkit.Instance.OpenFiles (Catalog.GetString("Add file"), null,
			                                                    Config.HomeDir(), null, null);
			List<string> errors = new List<string>();
			foreach (string path in paths) {
				try {
					MediaFile file = PreviewMediaFile.DiscoverFile(path);
					store.AppendValues (file);
					Files.Add (file);
				} catch (Exception) {
					errors.Add (path);
				}
			}
			if (errors.Count != 0) {
				string s = Catalog.GetString("Error adding files:");
				foreach (string p in errors) {
					s += '\n' + p;
				}
				GUIToolkit.Instance.ErrorMessage (s);
			}
			CheckStatus ();
		}

		protected void OnRemovebuttonClicked (object sender, System.EventArgs e)
		{
			TreeIter iter;
			
			treeview1.Selection.GetSelected (out iter);
			Files.Remove (store.GetValue (iter, 0) as MediaFile);
			CheckStatus ();
			store.Remove(ref iter);
		}

		protected void OnOpenbuttonClicked (object sender, System.EventArgs e)
		{
			string path = GUIToolkit.Instance.SaveFile (Catalog.GetString("Add file"),
			                                            "NewVideo.mp4",
			                                            Config.VideosDir(),
			                                            Catalog.GetString("MP4 file"),
			                                            "mp4");
			outputFile = System.IO.Path.ChangeExtension (path, "mp4");
			filelabel.Text = outputFile;
			CheckStatus ();
		}
		
		private void RenderMediaFile(Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.TreeModel model, Gtk.TreeIter iter)
		{
			MediaFile file = (MediaFile) store.GetValue(iter, 0);

			(cell as Gtk.CellRendererText).Text = String.Format("{0} {1}x{2} (Video:'{3}' Audio:'{4}')",
			                                                    System.IO.Path.GetFileName (file.FilePath),
			                                                    file.VideoWidth, file.VideoHeight,
			                                                    file.VideoCodec, file.AudioCodec);
		}

		protected void OnButtonOkClicked (object sender, System.EventArgs e)
		{
			EncodingSettings encSettings;
			TreeIter iter;
			VideoStandard std;
			
			sizecombobox.GetActiveIter(out iter);
			std = (VideoStandard) stdStore.GetValue(iter, 1);
			
			encSettings = new EncodingSettings(std, EncodingProfiles.MP4, 25, 1, 4000,
			                                   128, outputFile, 0);
			EncodingSettings = encSettings;
			Respond (ResponseType.Ok);
		}
	}
}

