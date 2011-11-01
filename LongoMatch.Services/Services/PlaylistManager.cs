// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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

using LongoMatch.Gui.Component;
using LongoMatch.Gui;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Common;
using Mono.Unix;


namespace LongoMatch.Services
{
	public class PlaylistManager
	{
		IPlaylistWidget playlistWidget;
		IPlayList playlist;
		PlayerBin player;
		/* FIXME */
		Project openedProject;
		TimeNode selectedTimeNode;
		
		bool clockStarted;
		uint timeout;
		
		public PlaylistManager (MainWindow mainWindow)
		{
			playlistWidget = mainWindow.Playlist;
			player = mainWindow.Player;
			BindEvents(mainWindow, mainWindow.Player);
		}
		
		public void Stop() {
			StopClock();
		}
		
		public void Load(string filePath) {
			try {
				playlist = PlayList.Load(filePath);
				playlistWidget.Load(playlist);
			} catch (Exception e){
				Log.Exception (e);
				MessagePopup.PopupMessage(playlistWidget as Gtk.Widget ,MessageType.Error, 
				                          Catalog.GetString("The file you are trying to load is not a playlist or it's not compatible with the current version"));
			}
		}
		
		private void BindEvents(MainWindow mainWindow, PlayerBin player) {
			/* Track loaded element */
			mainWindow.PlaySelectedEvent += (p) => {selectedTimeNode = p;};
			player.SegmentClosedEvent += () => {selectedTimeNode = null;};
			
			/* Handle New/Open/Save playlist */
			mainWindow.OpenPlaylistEvent += OnOpenPlaylist;
			mainWindow.NewPlaylistEvent += OnNewPlaylist;
			mainWindow.SavePlaylistEvent += OnSavePlaylist;
			
			/* Handle Add/Select/Rate events from other widgets */
			mainWindow.PlayListNodeAddedEvent += OnPlayListNodeAdded;
			mainWindow.PlayListNodeSelectedEvent += LoadPlaylistPlay;
			mainWindow.ApplyRateEvent += (p) => {p.Rate = player.Rate;};
			
			/* Handle Next/Prev from the player */
			player.Next += () => {Next();};
			player.Prev += () => {
				if(selectedTimeNode is PlayListPlay)
					Prev();
			};
		}
		
		private void Add(PlayListPlay plNode) {
			playlist.Add(plNode);
			playlistWidget.Add(plNode);
		}
		
		private void LoadPlaylistPlay(PlayListPlay play)
		{
			StartClock();
			player.SetPlayListElement(play.MediaFile.FilePath, play.Start.MSeconds,
			                          play.Stop.MSeconds, play.Rate, playlist.HasNext());
			selectedTimeNode = play;
		}
		
		private bool Next() {
			if(openedProject != null) {
				MessagePopup.PopupMessage(playlistWidget as Gtk.Widget, MessageType.Error,
				                          Catalog.GetString("Please, close the opened project to play the playlist."));
				Stop();
				return false;
			}
			
			if (!playlist.HasNext()) {
				Stop();
				return false;
			}
			
			var plNode = playlist.Next();
			playlistWidget.SetActivePlay(plNode, playlist.GetCurrentIndex());
			
			if (!plNode.Valid)
				return Next();
			
			LoadPlaylistPlay(plNode);
			return true;
		}

		private void Prev() {
			/* Select the previous element if we haven't played 500ms */
			if ((player.AccurateCurrentTime - selectedTimeNode.Start.MSeconds) < 500) {
				if (playlist.HasPrev()) {
					var play = playlist.Prev();
					playlistWidget.SetActivePlay(selectedTimeNode as PlayListPlay,
					                             playlist.GetCurrentIndex());
					LoadPlaylistPlay(play);
				}
			} else {
				/* Seek to the beginning of the segment */
				player.SeekTo(selectedTimeNode.Start.MSeconds,true);
				player.Rate = selectedTimeNode.Rate;
			}
		}
		
		private void StartClock()	{
			if(player!=null && !clockStarted) {
				timeout = GLib.Timeout.Add(20,CheckStopTime);
				clockStarted=true;
			}
		}

		private void StopClock() {
			if(clockStarted) {
				GLib.Source.Remove(timeout);
				clockStarted = false;
			}
		}

		private bool CheckStopTime() {
			if(player.AccurateCurrentTime >= selectedTimeNode.Stop.MSeconds-200)
				Next();
			return true;
		}
		
		private FileFilter FileFilter {
			get {
				FileFilter filter = new FileFilter();
				filter.Name = Catalog.GetString("LongoMatch playlist");
				filter.AddPattern("*" + Constants.PLAYLIST_EXT);
				return filter;
			}
		}
		
		protected virtual void OnPlayListNodeAdded(Play play)
		{
			Add(new PlayListPlay {
				MediaFile = openedProject.Description.File,
				Drawings = play.Drawings,
				Start = play.Start,
				Stop = play.Stop,
				Name = play.Name,
				Rate = 1.0f,
			});
		}
		
		protected virtual void OnSavePlaylist()
		{
			if(playlist != null) {
				playlist.Save();
			}
		}

		protected virtual void OnOpenPlaylist()
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Open playlist"),
			                (Gtk.Window)(playlistWidget as Gtk.Widget).Toplevel,
			                FileChooserAction.Open,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.PlayListDir());
			fChooser.AddFilter(FileFilter);
			fChooser.DoOverwriteConfirmation = true;
			if(fChooser.Run() == (int)ResponseType.Accept)
				Load(fChooser.Filename);
			fChooser.Destroy();
		}

		protected virtual void OnNewPlaylist()
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("New playlist"),
			                (Gtk.Window)(playlistWidget as Gtk.Widget).Toplevel,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(Config.PlayListDir());
			fChooser.AddFilter(FileFilter);

			if(fChooser.Run() == (int)ResponseType.Accept)
				Load(fChooser.Filename);
			fChooser.Destroy();
		}
	}
}

