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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using Timer = System.Threading.Timer;

namespace LongoMatch.Services
{
	public class PlaylistManager: IService
	{
		Project openedProject;
		Playlist openedPresentation;
		ProjectType openedProjectType;
		IPlaylistElement loadedElement;
		Playlist loadedPlaylist;
		TimelineEvent loadedPlay;
		EventsFilter filter;

		public PlaylistManager ()
		{
		}

		public IPlayerController Player {
			get;
			set;
		}

		void LoadPlay (TimelineEvent play, Time seekTime, bool playing)
		{
			if (play != null && openedProject != null && Player != null) {
				play.Selected = true;
				Player.LoadEvent (openedProject?.Description.FileSet,
					play, seekTime, playing);
				loadedPlay = play;
				if (playing) {
					Player.Play ();
				}
			}
		}

		void Switch (TimelineEvent play, Playlist playlist, IPlaylistElement element)
		{
			if (loadedElement != null) {
				loadedElement.Selected = false;
			}
			if (loadedPlay != null) {
				loadedPlay.Selected = false;
			}

			loadedPlay = play;
			loadedPlaylist = playlist;
			loadedElement = element;

			if (element != null) {
				element.Selected = true;
			}
			if (play != null) {
				play.Selected = true;
			}
		}

		void HandlePlayChanged (TimeNode tNode, Time time)
		{
			if (tNode is TimelineEvent) {
				LoadPlay (tNode as TimelineEvent, time, false);
				filter?.Update ();
			}
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 EventsFilter filter, IAnalysisWindow analysisWindow)
		{
			openedProject = project;
			openedProjectType = projectType;
			if (project != null) {
				Player = analysisWindow.Player;
				this.filter = filter;
			}
		}

		void HandleOpenedPresentationChanged (Playlist presentation, IPlayerController player)
		{
			openedProject = null;
			openedPresentation = presentation;
			openedProjectType = ProjectType.None;
			filter = null;
			Player = player;
		}

		void HandlePlaylistElementSelected (Playlist playlist, IPlaylistElement element)
		{
			Switch (null, playlist, element);
			if (element != null) {
				playlist.SetActive (element);
			}
			if (playlist.Elements.Count > 0 && Player != null)
				Player.LoadPlaylistEvent (playlist, element);
		}

		void HandleLoadPlayEvent (TimelineEvent play)
		{
			if (Player == null) {
				return;
			}

			if (openedProject == null || openedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}

			if (play is SubstitutionEvent || play is LineupEvent) {
				Switch (null, null, null);
				Config.EventsBroker.EmitEventLoaded (null);
				Player.Seek (play.EventTime, true);
				Player.Play ();
			} else {
				Switch (play, null, null);
				if (play != null) {
					LoadPlay (play, play.Start, true);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
				Config.EventsBroker.EmitEventLoaded (play);
			}
		}

		void HandleNext (Playlist playlist)
		{
			if (playlist != null && playlist.HasNext ()) {
				Config.EventsBroker.EmitPlaylistElementSelected (playlist, playlist.Next ());
			}
		}

		void HandlePrev (Playlist playlist)
		{
			/* Select the previous element if it's a regular play */
			if (playlist == null && loadedPlay != null) {
				Player.Seek (loadedPlay.Start, true);
				return;
			}
			
			if (loadedElement != null) {
				/* Select the previous element if we haven't played 500ms */
				if (loadedElement is PlaylistPlayElement) {
					TimelineEvent play = (loadedElement as PlaylistPlayElement).Play;
					if ((Player.CurrentTime - play.Start).MSeconds > 500) {
						Player.Seek (play.Start, true);
						return;
					}
				}
				/* Load the next playlist element */
				if (playlist.HasPrev ()) {
					Config.EventsBroker.EmitPlaylistElementSelected (playlist, playlist.Prev ());
				}
			}
		}

		void HandlePlaybackRateChanged (float rate)
		{
			if (loadedElement != null && loadedElement is PlaylistPlayElement) {
				(loadedElement as PlaylistPlayElement).Rate = rate;
			} else if (loadedPlay != null) {
				loadedPlay.Rate = rate;
			}
		}

		void HandleAddPlaylistElement (Playlist playlist, List<IPlaylistElement> element)
		{
			if (playlist == null) {
				playlist = HandleNewPlaylist (openedProject);
				if (playlist == null) {
					return;
				}
			}

			foreach (var item in element) {
				playlist.Elements.Add (item);
			}
			Config.EventsBroker.EmitPlaylistsChanged (this);
		}

		Playlist HandleNewPlaylist (Project project)
		{
			string name = Catalog.GetString ("New playlist");
			Playlist playlist = null;
			bool done = false;
			if (project != null) {
				while (name != null && !done) {
					name = Config.GUIToolkit.QueryMessage (Catalog.GetString ("Playlist name:"), null, name).Result;
					if (name != null) {
						done = true;
						if (project.Playlists.Any (p => p.Name == name)) {
							string msg = Catalog.GetString ("A playlist already exists with the same name");
							Config.GUIToolkit.ErrorMessage (msg);
							done = false;
						}
					}
				}
				if (name != null) {
					playlist = new Playlist { Name = name };
					project.Playlists.Add (playlist);
					Config.EventsBroker.EmitPlaylistsChanged (this);
				}
			}
			return playlist;
		}

		void HandleRenderPlaylist (Playlist playlist)
		{
			List<EditionJob> jobs = Config.GUIToolkit.ConfigureRenderingJob (playlist);
			if (jobs == null)
				return;
			foreach (Job job in jobs)
				Config.RenderingJobsManger.AddJob (job);
		}

		void HandleSeekEvent (Time pos, bool accurate)
		{
			if (Player != null) {
				Player.Seek (pos, accurate);
			}
		}

		void HandleTogglePlayEvent (bool playing)
		{
			if (Player != null) {
				if (playing) {
					Player.Play ();
				} else {
					Player.Pause ();
				}
			}
		}

		void HandleKeyPressed (object sender, HotKey key)
		{
			if (openedProject == null && openedPresentation == null)
				return;

			if (openedProjectType != ProjectType.CaptureProject &&
			    openedProjectType != ProjectType.URICaptureProject &&
			    openedProjectType != ProjectType.FakeCaptureProject) {
				KeyAction action;
				if (Player == null)
					return;

				try {
					action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
				} catch (Exception ex) {
					/* The dictionary contains 2 equal values for different keys */
					Log.Exception (ex);
					return;
				}
				
				if (action == KeyAction.None) {
					return;
				}

				switch (action) {
				case KeyAction.FrameUp:
					Player.SeekToNextFrame ();
					return;
				case KeyAction.FrameDown:
					Player.SeekToPreviousFrame ();
					return;
				case KeyAction.JumpUp:
					Player.StepForward ();
					return;
				case KeyAction.JumpDown:
					Player.StepBackward ();
					return;
				case KeyAction.DrawFrame:
					TimelineEvent evt = loadedPlay;
					if (evt == null && loadedElement is PlaylistPlayElement) {
						evt = (loadedElement as PlaylistPlayElement).Play;
					}
					if (evt != null) {
						Config.EventsBroker.EmitDrawFrame (evt, -1, Player.CamerasConfig [0], true);
					} else {
						Config.EventsBroker.EmitDrawFrame (null, -1, null, true);
					}
					return;
				case KeyAction.TogglePlay:
					Player.TogglePlay ();
					return;
				case KeyAction.SpeedUp:
					Player.FramerateUp ();
					Config.EventsBroker.EmitPlaybackRateChanged ((float)Player.Rate);
					return;
				case KeyAction.SpeedDown:
					Player.FramerateDown ();
					Config.EventsBroker.EmitPlaybackRateChanged ((float)Player.Rate);
					return;
				case KeyAction.CloseEvent:
					Config.EventsBroker.EmitLoadEvent (null);
					return;
				case KeyAction.Prev:
					HandlePrev (loadedPlaylist);
					return;
				case KeyAction.Next:
					HandleNext (loadedPlaylist);
					return;
				}
			} else {
				//if (Capturer == null)
				//	return;
			}
		}

		#region IService

		public int Level {
			get {
				return 80;
			}
		}

		public string Name {
			get {
				return "Playlists";
			}
		}

		public bool Start ()
		{
			Config.EventsBroker.NewPlaylistEvent += HandleNewPlaylist;
			Config.EventsBroker.AddPlaylistElementEvent += HandleAddPlaylistElement;
			Config.EventsBroker.RenderPlaylist += HandleRenderPlaylist;
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			Config.EventsBroker.OpenedPresentationChanged += HandleOpenedPresentationChanged;
			Config.EventsBroker.PreviousPlaylistElementEvent += HandlePrev;
			Config.EventsBroker.NextPlaylistElementEvent += HandleNext;
			Config.EventsBroker.LoadEventEvent += HandleLoadPlayEvent;
			Config.EventsBroker.PlaylistElementSelectedEvent += HandlePlaylistElementSelected;
			Config.EventsBroker.PlaybackRateChanged += HandlePlaybackRateChanged;
			Config.EventsBroker.TimeNodeChanged += HandlePlayChanged;
			Config.EventsBroker.SeekEvent += HandleSeekEvent;
			Config.EventsBroker.TogglePlayEvent += HandleTogglePlayEvent;
			Config.EventsBroker.KeyPressed += HandleKeyPressed;

			return true;
		}

		public bool Stop ()
		{
			Config.EventsBroker.NewPlaylistEvent -= HandleNewPlaylist;
			Config.EventsBroker.AddPlaylistElementEvent -= HandleAddPlaylistElement;
			Config.EventsBroker.RenderPlaylist -= HandleRenderPlaylist;
			Config.EventsBroker.OpenedProjectChanged -= HandleOpenedProjectChanged;
			Config.EventsBroker.OpenedPresentationChanged -= HandleOpenedPresentationChanged;
			Config.EventsBroker.PreviousPlaylistElementEvent -= HandlePrev;
			Config.EventsBroker.NextPlaylistElementEvent -= HandleNext;
			Config.EventsBroker.LoadEventEvent -= HandleLoadPlayEvent;
			Config.EventsBroker.PlaylistElementSelectedEvent -= HandlePlaylistElementSelected;
			Config.EventsBroker.PlaybackRateChanged -= HandlePlaybackRateChanged;
			Config.EventsBroker.TimeNodeChanged -= HandlePlayChanged;
			Config.EventsBroker.SeekEvent -= HandleSeekEvent;
			Config.EventsBroker.TogglePlayEvent -= HandleTogglePlayEvent;
			Config.EventsBroker.KeyPressed -= HandleKeyPressed;

			return true;
		}

		#endregion
	}
}
