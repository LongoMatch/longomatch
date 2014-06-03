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
using System.Collections.Generic;

using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using LongoMatch.Common;
using Mono.Unix;

using Timer = System.Threading.Timer;


namespace LongoMatch.Services
{
	public class PlaylistManager
	{
		IGUIToolkit guiToolkit;
		IPlaylistWidget playlistWidget;
		IPlayList playlist;
		IPlayerBin player;
		IRenderingJobsManager videoRenderer;
		IAnalysisWindow analysisWindow;
		/* FIXME */
		TimeNode selectedTimeNode;
		
		bool clockStarted;
		Timer timeout;
		Project openedProject;
		
		public PlaylistManager (IGUIToolkit guiToolkit, IRenderingJobsManager videoRenderer,
		                        ProjectsManager pManager)
		{
			this.videoRenderer = videoRenderer;
			this.guiToolkit = guiToolkit;
			pManager.OpenedProjectChanged += HandleOpenedProjectChanged;
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType, PlaysFilter filter, IAnalysisWindow analysisWindow, IProjectOptionsController projectOptions)
		{
			openedProject = project;
			if (project != null) {
				//playlistWidget = analysisWindow.Playlist;
				player = analysisWindow.Player;
				if (this.analysisWindow != analysisWindow) {
					BindEvents(analysisWindow);
					this.analysisWindow = analysisWindow;
				}
			}
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
				guiToolkit.ErrorMessage(Catalog.GetString("The file you are trying to load " +
					"is not a playlist or it's not compatible with the current version"));
			}
		}
		
		private void BindEvents(IAnalysisWindow analysisWindow) {
			/* Track loaded element */
			analysisWindow.PlaySelectedEvent += (p) => {selectedTimeNode = p;};
			analysisWindow.Player.SegmentClosedEvent += () => {selectedTimeNode = null;};
			
			/* Handle New/Open/Save playlist */
			analysisWindow.OpenPlaylistEvent += OnOpenPlaylist;
			analysisWindow.NewPlaylistEvent += OnNewPlaylist;
			analysisWindow.SavePlaylistEvent += OnSavePlaylist;
			
			/* Handle Add/Select/Rate events from other widgets */
			analysisWindow.PlayListNodeAddedEvent += OnPlayListNodeAdded;
			analysisWindow.PlayListNodeSelectedEvent += LoadPlaylistPlay;
			analysisWindow.RenderPlaylistEvent += OnRenderPlaylistEvent;
			
			/* Handle Next/Prev from the player */
			analysisWindow.Player.Next += () => {Next();};
			analysisWindow.Player.Prev += () => {
				if(selectedTimeNode is PlayListPlay)
					Prev();
			};
		}
		
		private void Add(List<Play> plays) {
			if (playlist == null) {
				guiToolkit.InfoMessage(Catalog.GetString("You have not loaded any playlist yet."));
			} else {
				foreach (Play p in plays) {
					PlayListPlay pl = new PlayListPlay (p, openedProject.Description.File, true);
					playlist.Add(pl);
					playlistWidget.Add(pl);
				}
			}
		}
		
		private void LoadPlaylistPlay(PlayListPlay play)
		{
			if(openedProject != null) {
				guiToolkit.ErrorMessage(Catalog.GetString(
					"Please, close the opened project to play the playlist."));
				Stop();
				return;
			}
			
			StartClock();
			player.LoadPlayListPlay (play, playlist.HasNext());
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
			if ((player.CurrentTime - selectedTimeNode.Start).MSeconds < 500) {
				if (playlist.HasPrev()) {
					var play = playlist.Prev();
					LoadPlaylistPlay(play);
				}
			} else {
				/* Seek to the beginning of the segment */
				player.Seek (selectedTimeNode.Start, true);
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
			if(player.CurrentTime >= selectedTimeNode.Stop - new Time  {MSeconds=200})
				Next();
			return;
		}
		
		protected virtual void OnRenderPlaylistEvent (IPlayList playlist)
		{
			List<EditionJob> jobs = guiToolkit.ConfigureRenderingJob(playlist);
			if (jobs == null)
				return;
			foreach (Job job in jobs)
				videoRenderer.AddJob(job);
		}
		
		protected virtual void OnPlayListNodeAdded(List<Play> plays)
		{
			Add (plays);
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
			
			filename = guiToolkit.OpenFile(Catalog.GetString("Open playlist"), null, Config.PlayListDir,
				Constants.PROJECT_NAME + Catalog.GetString("playlists"),
				new string [] {"*" + Constants.PLAYLIST_EXT});
			if (filename != null)
				Load(filename);
		}

		protected virtual void OnNewPlaylist()
		{
			string filename;
			
			filename = guiToolkit.SaveFile(Catalog.GetString("New playlist"), null, Config.PlayListDir,
				Constants.PROJECT_NAME + Catalog.GetString("playlists"),
				new string [] {"*" + Constants.PLAYLIST_EXT});

			if (filename != null)
				Load(filename);
		}
	}
}

