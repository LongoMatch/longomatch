// PlayListWidget.cs
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using Gtk;
using Gdk;
using LongoMatch.Video.Editor;
using Mono.Unix;
using System.IO;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Player;
using LongoMatch.Video;
using LongoMatch.Video.Common;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;
using LongoMatch.Playlist;



namespace LongoMatch.Gui.Component
{

	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PlayListWidget : Gtk.Bin
	{
		public event PlayListNodeSelectedHandler PlayListNodeSelected;
		public event ApplyCurrentRateHandler ApplyCurrentRate;
		public event ProgressHandler Progress;

		private PlayerBin player;
		private PlayListTimeNode plNode;
		private PlayList playList;
		private uint timeout;
		private object lock_node;
		private bool clock_started = false;
		private IVideoEditor videoEditor;
		private MultimediaFactory factory;


		public PlayListWidget ()
		{
			this.Build ();
			lock_node = new System.Object ();
			factory = new MultimediaFactory ();
			playlisttreeview1.Reorderable = true;
			playlisttreeview1.RowActivated += OnPlaylisttreeview1RowActivated;
			playlisttreeview1.ApplyCurrentRate += OnApplyRate;
			savebutton.Sensitive = false;
		}

		public void SetPlayer(PlayerBin player) {
			this.player = player;
			closebutton.Hide();
			newvideobutton.Hide();
		}

		public void Load(string filePath) {
			try {
				playList = new PlayList(filePath);
				Model = playList.GetModel();
				label1.Visible = false;
				newvideobutton.Show();
				playlisttreeview1.PlayList = playList;
				playlisttreeview1.Sensitive = true;
				savebutton.Sensitive = true;
			} catch {
				MessagePopup.PopupMessage(this,MessageType.Error,Catalog.GetString("The file you are trying to load is not a playlist or it's not compatible with the current version"));
			}
		}

		public ListStore Model {
			set {
				playlisttreeview1.Model = value;
			}
			get {
				return (ListStore)playlisttreeview1.Model;
			}
		}

		public void Add(PlayListTimeNode plNode) {
			if (playList!=null) {
				Model.AppendValues(plNode);
				playList.Add(plNode);
			}
		}

		public PlayListTimeNode Next() {
			if (playList.HasNext()) {
				plNode = playList.Next();
				playlisttreeview1.Selection.SelectPath(new TreePath(playList.GetCurrentIndex().ToString()));
				playlisttreeview1.LoadedPlay = plNode;
				if (PlayListNodeSelected != null && plNode.Valid) {
					PlayListNodeSelected(plNode,playList.HasNext());
					StartClock();
				}
				else
					Next();
				return plNode;
			}
			else {
				return null;
			}
		}

		public void Prev() {
			if ((player.AccurateCurrentTime - plNode.Start.MSeconds) < 500) {
				//Seleccionando el elemento anterior si no han pasado más 500ms
				if (playList.HasPrev()) {
					plNode = playList.Prev();
					playlisttreeview1.Selection.SelectPath(new TreePath(playList.GetCurrentIndex().ToString()));
					playlisttreeview1.LoadedPlay = plNode;
					if (PlayListNodeSelected != null)
						PlayListNodeSelected(plNode,playList.HasNext());
					StartClock();
				}
			}
			else {
				//Nos situamos al inicio del segmento
				player.SeekTo(plNode.Start.MSeconds,true);
				player.Rate=plNode.Rate;
			}
		}

		public void StopEdition() {
			if (videoEditor != null)
				videoEditor.Cancel();
		}

		public void Stop() {
			StopClock();
		}

		void StartClock()	{
			if (player!=null && !clock_started) {
				timeout = GLib.Timeout.Add(20,CheckStopTime);
				clock_started=true;
			}
		}

		private void StopClock() {
			if (clock_started) {
				GLib.Source.Remove(timeout);
				clock_started = false;
			}
		}

		private bool CheckStopTime() {
			lock (lock_node) {
				if (player != null) {
					if (player.AccurateCurrentTime >= plNode.Stop.MSeconds-200) {
						if (Next() == null)
							StopClock();
					}
				}
				return true;
			}
		}
		private PlayListTimeNode SelectPlayListNode(TreePath path) {

			plNode = playList.Select(Int32.Parse(path.ToString()));
			if (PlayListNodeSelected != null && plNode.Valid) {
				PlayListNodeSelected(plNode,playList.HasNext());
				StartClock();
			}
			return plNode;
		}

		private FileFilter FileFilter {
			get {
				FileFilter filter = new FileFilter();
				filter.Name = "LGM playlist";
				filter.AddPattern("*.lgm");
				return filter;
			}
		}

		private void LoadEditor() {
			videoEditor = factory.getVideoEditor();
			videoEditor.Progress += new ProgressHandler(OnProgress);
		}

		protected virtual void OnPlaylisttreeview1RowActivated(object o, Gtk.RowActivatedArgs args)
		{
			playlisttreeview1.LoadedPlay = SelectPlayListNode(args.Path);
		}


		protected virtual void OnSavebuttonClicked(object sender, System.EventArgs e)
		{
			if (playList != null) {
				playList.Save();
			}
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Open playlist"),
			                (Gtk.Window)this.Toplevel,
			                FileChooserAction.Open,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());
			fChooser.AddFilter(FileFilter);
			fChooser.DoOverwriteConfirmation = true;
			if (fChooser.Run() == (int)ResponseType.Accept)
				Load(fChooser.Filename);
			fChooser.Destroy();
		}

		protected virtual void OnNewbuttonClicked(object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("New playlist"),
			                (Gtk.Window)this.Toplevel,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());
			fChooser.AddFilter(FileFilter);

			if (fChooser.Run() == (int)ResponseType.Accept)
				Load(fChooser.Filename);
			fChooser.Destroy();
		}

		protected virtual void OnPlaylisttreeview1DragEnd(object o, Gtk.DragEndArgs args)
		{
			playList.SetModel((ListStore)playlisttreeview1.Model);
		}

		protected virtual void OnNewvideobuttonClicked(object sender, System.EventArgs e)
		{
			VideoEditionProperties vep;
			int response;

			if (playList.Count == 0) {
				MessagePopup.PopupMessage(this,MessageType.Warning,
				                          Catalog.GetString("The playlist is empty!"));
				return;
			}

			vep = new VideoEditionProperties();
			vep.TransientFor = (Gtk.Window)this.Toplevel;
			response = vep.Run();
			while (response == (int)ResponseType.Ok && vep.Filename == "") {
				MessagePopup.PopupMessage(this, MessageType.Warning,
				                          Catalog.GetString("Please, select a video file."));
				response=vep.Run();
			}
			if (response ==(int)ResponseType.Ok) {
				//FIXME:Create a new instance of the video editor until we fix the audio swith enable/disabled
				LoadEditor();
				//videoEditor.ClearList();
				foreach (PlayListTimeNode segment in playList) {
					if (segment.Valid)
						videoEditor.AddSegment(segment.MediaFile.FilePath,
						                       segment.Start.MSeconds,
						                       segment.Duration.MSeconds,
						                       segment.Rate,
						                       segment.Name,
						                       segment.MediaFile.HasAudio);
				}
				try {
					videoEditor.VideoQuality = vep.VideoQuality;
					videoEditor.AudioQuality = AudioQuality.Good;
					videoEditor.VideoFormat = vep.VideoFormat;
					videoEditor.AudioEncoder = vep.AudioEncoderType;
					videoEditor.VideoEncoder = vep.VideoEncoderType;
					videoEditor.OutputFile = vep.Filename;
					videoEditor.EnableTitle = vep.TitleOverlay;
					videoEditor.EnableAudio = vep.EnableAudio;
					videoEditor.VideoMuxer = vep.VideoMuxer;
					videoEditor.Start();
					closebutton.Show();
					newvideobutton.Hide();
				}
				catch (Exception ex) {
					MessagePopup.PopupMessage(this, MessageType.Error, Catalog.GetString(ex.Message));
				}
			vep.Destroy();
			}
		}

		protected virtual void OnClosebuttonClicked(object sender, System.EventArgs e)
		{
			videoEditor.Cancel();
			closebutton.Hide();
			newvideobutton.Show();
		}

		protected virtual void OnProgress(float progress) {
			if (Progress!= null)
				Progress(progress);

			if (progress ==1) {
				closebutton.Hide();
				newvideobutton.Show();
			}
		}

		protected virtual void OnApplyRate(PlayListTimeNode plNode) {
			if (ApplyCurrentRate != null)
				ApplyCurrentRate(plNode);
		}

		~PlayListWidget() {
			videoEditor.Cancel();
		}

	}
}
