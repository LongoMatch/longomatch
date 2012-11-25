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
using System.Threading;

using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using LongoMatch.Common;
using Mono.Unix;


namespace LongoMatch.Services
{
	public class PlaylistManager
	{
		IGUIToolkit guiToolkit;
		IPlaylistWidget playlistWidget;
		IPlayList playlist;
		IPlayer player;
		IRenderingJobsManager videoRenderer;
		/* FIXME */
		TimeNode selectedTimeNode;
		
		bool clockStarted;
		Timer timeout;
		
		public PlaylistManager (IGUIToolkit guiToolkit, IRenderingJobsManager videoRenderer)
		{
			this.videoRenderer = videoRenderer;
			this.guiToolkit = guiToolkit;
			playlistWidget = guiToolkit.MainWindow.Playlist;
			player = guiToolkit.MainWindow.Player;
			BindEvents(guiToolkit.MainWindow, guiToolkit.MainWindow.Player);
		}
		
		public void Stop() {
			StopClock();
		}
		
		public Project OpenedProject {
			get;
			set;
		}
		
		public void Load(string filePath) {
			try {
				playlist = PlayList.Load(filePath);
				playlistWidget.Load(playlist);
			} catch (Exception e){
				Log.Exception (e);
				guiToolkit.ErrorMessage(Catalog.GetString("The file you are trying to load " +
					"is not a playlist or it's not compatible with the current version"));
			}
		}
		
		private void BindEvents(IMainWindow mainWindow, IPlayer player) {
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
			mainWindow.RenderPlaylistEvent += OnRenderPlaylistEvent;
			
			/* Handle Next/Prev from the player */
			player.Next += () => {Next();};
			player.Prev += () => {
				if(selectedTimeNode is PlayListPlay)
					Prev();
			};
		}
		
		private void Add(PlayListPlay plNode) {
			if (playlist == null) {
				guiToolkit.InfoMessage(Catalog.GetString("You have not loaded any playlist yet."));
			} else {
				playlist.Add(plNode);
				playlistWidget.Add(plNode);
			}
		}
		
		private void LoadPlaylistPlay(PlayListPlay play)
		{
			if(OpenedProject != null) {
				guiToolkit.ErrorMessage(Catalog.GetString(
					"Please, close the opened project to play the playlist."));
				Stop();
				return;
			}
			
			StartClock();
			player.SetPlayListElement(play.MediaFile.FilePath, play.Start.MSeconds,
			                          play.Stop.MSeconds, play.Rate, playlist.HasNext());
			selectedTimeNode = play;
			playlist.SetActive (play);
			playlistWidget.SetActivePlay(play, playlist.GetCurrentIndex());
		}
		
		private bool Next() {
			if (!playlist.HasNext()) {
				Stop();
				return false;
			}
			
			var plNode = playlist.Next();
			
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
				timeout = new Timer(new TimerCallback(CheckStopTime), this, 20, 20);
				clockStarted=true;
			}
		}

		private void StopClock() {
			if(clockStarted) {
				timeout.Dispose();
				clockStarted = false;
			}
		}

		private void CheckStopTime(object self) {
			if(player.AccurateCurrentTime >= selectedTimeNode.Stop.MSeconds-200)
				Next();
			return;
		}
		
		protected virtual void OnRenderPlaylistEvent (IPlayList playlist)
		{
			Job job = guiToolkit.ConfigureRenderingJob(playlist);
			if (job != null)
				videoRenderer.AddJob(job);
		}
		
		protected virtual void OnPlayListNodeAdded(Play play)
		{
			Add(new PlayListPlay {
				MediaFile = OpenedProject.Description.File,
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
			string filename;
			
			filename = guiToolkit.OpenFile(Catalog.GetString("Open playlist"), null, Config.PlayListDir(),
				Constants.PROJECT_NAME + Catalog.GetString("playlists"),
				"*" + Constants.PLAYLIST_EXT);
			if (filename != null)
				Load(filename);
		}

		protected virtual void OnNewPlaylist()
		{
			string filename;
			
			filename = guiToolkit.SaveFile(Catalog.GetString("New playlist"), null, Config.PlayListDir(),
				Constants.PROJECT_NAME + Catalog.GetString("playlists"),
				"*" + Constants.PLAYLIST_EXT);

			if (filename != null)
				Load(filename);
		}
	}
}

