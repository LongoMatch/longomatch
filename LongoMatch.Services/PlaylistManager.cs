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
		EventsFilter filter;

		public PlaylistManager ()
		{
		}

		public IPlayerController Player {
			get;
			set;
		}

		public Project OpenedProject {
			get;
			set;
		}

		public ProjectType OpenedProjectType {
			get;
			set;
		}

		void LoadPlay (TimelineEvent play, Time seekTime, bool playing)
		{
			if (play != null && Player != null) {
				play.Selected = true;
				Player.LoadEvent (
					play, seekTime, playing);
				if (playing) {
					Player.Play ();
				}
			}
		}

		void HandlePlayChanged (TimeNode tNode, Time time)
		{
			if (tNode is TimelineEvent) {
				LoadPlay (tNode as TimelineEvent, time, false);
				if (filter != null) {
					filter.Update ();
				}
			}
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 EventsFilter filter, IAnalysisWindow analysisWindow)
		{
			var player = analysisWindow?.Player;
			if (player == null && Player == null) {
				return;
			} else if (player != null) {
				Player = player;
			}

			OpenedProject = project;
			OpenedProjectType = projectType;
			this.filter = filter;
			Player.LoadedPlaylist = null;
		}

		/// <summary>
		/// Set the playlistManager with a presentation (<see cref="Playlist"/> not attached to a <see cref="Project"/>)
		/// If player is null, the last one will be used (if there is one).
		/// </summary>
		/// <param name="presentation">Presentation.</param>
		/// <param name="player">Player.</param>
		void HandleOpenedPresentationChanged (Playlist presentation, IPlayerController player)
		{
			if (player == null && Player == null) {
				return;
			} else if (player != null) {
				Player = player;
			}

			OpenedProject = null;
			Player.Switch (null, presentation, null);

			OpenedProjectType = ProjectType.None;
			filter = null;
		}

		void HandlePlaylistElementSelected (Playlist playlist, IPlaylistElement element, bool playing = false)
		{
			if (element != null) {
				playlist.SetActive (element);
			}
			if (playlist.Elements.Count > 0 && Player != null)
				Player.LoadPlaylistEvent (playlist, element, playing);
		}

		void HandleLoadPlayEvent (TimelineEvent play)
		{
			if (OpenedProject == null || OpenedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}

			if (play is SubstitutionEvent || play is LineupEvent) {
				//FIXME: This switch bugs me, it's the only one here...
				Player.Switch (null, null, null);
				Config.EventsBroker.EmitEventLoaded (null);
				Player.Seek (play.EventTime, true);
				Player.Play ();
			} else {
				if (play != null) {
					LoadPlay (play, new Time (0), true);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
				Config.EventsBroker.EmitEventLoaded (play);
			}
		}

		void HandleNext (Playlist playlist)
		{
			Player.Next ();
		}

		void HandlePrev (Playlist playlist)
		{
			Player.Previous ();
		}

		void HandlePlaybackRateChanged (float rate)
		{
		}

		void HandleAddPlaylistElement (Playlist playlist, List<IPlaylistElement> element)
		{
			if (playlist == null) {
				playlist = HandleNewPlaylist (OpenedProject);
				if (playlist == null) {
					return;
				}
			}

			foreach (var item in element) {
				playlist.Elements.Add (item);
			}
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

		void HandleSeekEvent (Time pos, bool accurate, bool synchronous = false, bool throttled = false)
		{
			if (Player != null) {
				Player.Seek (pos, accurate, synchronous, throttled);
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
			if (OpenedProject == null && Player?.LoadedPlaylist == null)
				return;

			if ((OpenedProjectType != ProjectType.CaptureProject &&
			    OpenedProjectType != ProjectType.URICaptureProject &&
			    OpenedProjectType != ProjectType.FakeCaptureProject) || Player.LoadedPlaylist != null) {
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
					Player.DrawFrame ();
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
					HandlePrev (null);
					return;
				case KeyAction.Next:
					HandleNext (null);
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
