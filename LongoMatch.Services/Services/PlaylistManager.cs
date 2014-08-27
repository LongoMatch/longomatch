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
using System.Collections.Generic;
using System.Threading;
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using LongoMatch.Store.Playlists;
using Mono.Unix;
using Timer = System.Threading.Timer;

namespace LongoMatch.Services
{
	public class PlaylistManager
	{
		IGUIToolkit guiToolkit;
		IPlayerBin player;
		IRenderingJobsManager videoRenderer;
		Project openedProject;
		IPlaylistElement loadedElement;
		Playlist loadedPlaylist;
		Play loadedPlay;

		public PlaylistManager (IGUIToolkit guiToolkit, IRenderingJobsManager videoRenderer)
		{
			this.videoRenderer = videoRenderer;
			this.guiToolkit = guiToolkit;
			BindEvents ();
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 PlaysFilter filter, IAnalysisWindow analysisWindow)
		{
			openedProject = project;
			if (project != null) {
				player = analysisWindow.Player;
			}
		}

		void BindEvents ()
		{
			Config.EventsBroker.NewPlaylistEvent += HandleNewPlaylist;
			Config.EventsBroker.AddPlaylistElementEvent += HandleAddPlaylistElement;
			Config.EventsBroker.RenderPlaylist += HandleRenderPlaylist;
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			Config.EventsBroker.PreviousPlaylistElementEvent += HandlePrev;
			Config.EventsBroker.NextPlaylistElementEvent += HandleNext;
			Config.EventsBroker.PlaySelected += HandlePlaySelected;
			Config.EventsBroker.PlaylistElementSelectedEvent += HandlePlaylistElementSelected;
		}

		void Switch (Play play, Playlist playlist, IPlaylistElement element)
		{
			if (loadedElement != null) {
				loadedElement.Selected = false;
			}
			if (element != null) {
				element.Selected = true;
			}
			
			loadedPlay = play;
			loadedPlaylist = playlist;
			loadedElement = element;
		}
		
		void HandlePlaylistElementSelected (Playlist playlist, IPlaylistElement element)
		{
			Switch (null, playlist, element);
			if (element != null) {
				playlist.SetActive (element);
			}
			player.LoadPlayListPlay (playlist, element);
			
		}

		void HandlePlaySelected (Play play)
		{
			Switch (play, null, null);
		}

		void HandleNext (Playlist playlist)
		{
			if (playlist != null && playlist.HasNext ()) {
				Config.EventsBroker.EmitPlaylistElementSelected (playlist, playlist.Next());
			}
		}
		
		void HandlePrev (Playlist playlist)
		{
			/* Select the previous element if it's a regular play */
			if (playlist == null && loadedPlay != null) {
				player.Seek (loadedPlay.Start, true);
				return;
			}
			
			if (loadedElement != null) {
				/* Select the previous element if we haven't played 500ms */
				if (loadedElement is PlaylistPlayElement) {
					Play play = (loadedElement as PlaylistPlayElement).Play;
					if ((player.CurrentTime - play.Start).MSeconds > 500) {
						player.Seek (play.Start, true);
						return;
					}
				}
				/* Load the next playlist element */
				if (playlist.HasPrev ()) {
					Config.EventsBroker.EmitPlaylistElementSelected (playlist, playlist.Prev());
				}
			}
		}

		void HandleAddPlaylistElement (Playlist playlist, List<IPlaylistElement> element)
		{
			if (playlist == null) {
				playlist = HandleNewPlaylist (openedProject);
			}
			playlist.Elements.AddRange (element);
			Config.EventsBroker.EmitPlaylistsChanged (this);
		}

		Playlist HandleNewPlaylist (Project project)
		{
			string name;
			Playlist playlist = null;
			
			name = Config.GUIToolkit.QueryMessage (Catalog.GetString ("Playlist name:"), null,
			                                       Catalog.GetString ("New playlist"));
			if (name != null) {
				playlist = new Playlist {Name = name};
				project.Playlists.Add (playlist);
				Config.EventsBroker.EmitPlaylistsChanged (this);
			}
			return playlist;
		}

		void HandleRenderPlaylist (Playlist playlist)
		{
			List<EditionJob> jobs = guiToolkit.ConfigureRenderingJob (playlist);
			if (jobs == null)
				return;
			foreach (Job job in jobs)
				videoRenderer.AddJob (job);
		}
	}
}
