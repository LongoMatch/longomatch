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
using System.Linq;
using System.Threading.Tasks;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Services;
using VAS.Services.Controller;
using VAS.Services.ViewModel;

namespace LongoMatch.Services
{
	public class PlaylistManager : PlaylistController, IService
	{
		EventToken newPlaylistEventToken;

		public PlaylistManager () : base (new PlayerVM (false))
		{
		}

		public IPlayerController Player {
			get { return PlayerVM.Player; }
			set {
				PlayerVM = new PlayerVM (false) {
					Player = value
				};
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

		public new bool Start ()
		{
			base.Start ();
			newPlaylistEventToken = App.Current.EventsBroker.Subscribe<NewPlaylistEvent> ((e) => HandleNewPlaylist (e));
			App.Current.EventsBroker.Subscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			App.Current.EventsBroker.Subscribe<OpenedPresentationChangedEvent> (HandleOpenedPresentationChanged);
			App.Current.EventsBroker.Subscribe<PreviousPlaylistElementEvent> (HandlePrev);
			App.Current.EventsBroker.Subscribe<NextPlaylistElementEvent> (HandleNext);
			App.Current.EventsBroker.Subscribe<LoadEventEvent> (HandleLoadPlayEvent);
			App.Current.EventsBroker.Subscribe<LoadCameraEvent> (HandleLoadCameraEvent);
			App.Current.EventsBroker.Subscribe<PlaybackRateChangedEvent> (HandlePlaybackRateChanged);
			App.Current.EventsBroker.Subscribe<TimeNodeChangedEvent> (HandlePlayChanged);
			App.Current.EventsBroker.Subscribe<TogglePlayEvent> (HandleTogglePlayEvent);
			return true;
		}

		public new bool Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<NewPlaylistEvent> (newPlaylistEventToken);
			App.Current.EventsBroker.Unsubscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			App.Current.EventsBroker.Unsubscribe<OpenedPresentationChangedEvent> (HandleOpenedPresentationChanged);
			App.Current.EventsBroker.Unsubscribe<PreviousPlaylistElementEvent> (HandlePrev);
			App.Current.EventsBroker.Unsubscribe<NextPlaylistElementEvent> (HandleNext);
			App.Current.EventsBroker.Unsubscribe<LoadEventEvent> (HandleLoadPlayEvent);
			App.Current.EventsBroker.Unsubscribe<LoadCameraEvent> (HandleLoadCameraEvent);
			App.Current.EventsBroker.Unsubscribe<PlaybackRateChangedEvent> (HandlePlaybackRateChanged);
			App.Current.EventsBroker.Unsubscribe<TimeNodeChangedEvent> (HandlePlayChanged);
			App.Current.EventsBroker.Unsubscribe<TogglePlayEvent> (HandleTogglePlayEvent);
			return true;
		}

		#endregion


		void LoadPlay (TimelineEvent play, Time seekTime, bool playing)
		{
			if (play != null && Player != null) {
				play.Playing = true;
				Player.LoadEvent (
					play, seekTime, playing);
				if (playing) {
					Player.Play ();
				}
			}
		}

		void LoadCameraPlay (TimelineEvent play, Time seekTime, bool playing)
		{
			if (play != null && Player != null) {
				play.Playing = true;
				(Player as PlayerController)?.LoadCameraEvent (
					play, seekTime, playing);
				if (playing) {
					Player.Play ();
				}
			}
		}

		void HandleOpenedProjectChanged (OpenedProjectEvent e)
		{
			var player = e.AnalysisWindow?.Player;
			if (player == null && Player == null) {
				return;
			} else if (player != null) {
				Player = player;
			}

			OpenedProject = e.Project;
			OpenedProjectType = e.ProjectType;
			Player.LoadedPlaylist = null;
		}

		/// <summary>
		/// Set the playlistManager with a presentation (<see cref="Playlist"/> not attached to a <see cref="Project"/>)
		/// If player is null, the last one will be used (if there is one).
		/// </summary>
		/// <param name="presentation">Presentation.</param>
		/// <param name="player">Player.</param>
		void HandleOpenedPresentationChanged (OpenedPresentationChangedEvent e)
		{
			if (e.Player == null && Player == null) {
				return;
			} else if (e.Player != null) {
				Player = e.Player;
			}

			OpenedProject = null;
			Player.Switch (null, e.Presentation, null);

			OpenedProjectType = ProjectType.None;
			Filter = null;
		}

		void HandlePlayChanged (TimeNodeChangedEvent e)
		{
			if (e.TimeNode is TimelineEvent) {
				LoadPlay (e.TimeNode as TimelineEvent, e.Time, false);
				if (Filter != null) {
					Filter.Update ();
				}
			}
		}

		void HandleLoadCameraEvent (LoadCameraEvent e)
		{
			if (OpenedProject == null || OpenedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}

			if (e.CameraTlEvent != null) {
				Time seekTime = Player.CurrentTime - e.CameraTlEvent.Start;
				seekTime = seekTime.Clamp (new Time (0), e.CameraTlEvent.Stop);
				LoadCameraPlay (e.CameraTlEvent, seekTime, Player.Playing);
			} else if (Player != null) {
				Player.UnloadCurrentEvent ();
			}
		}

		void HandleNext (NextPlaylistElementEvent e)
		{
			Player.Next ();
		}

		void HandlePrev (PreviousPlaylistElementEvent e)
		{
			Player.Previous ();
		}

		void HandlePlaybackRateChanged (PlaybackRateChangedEvent e)
		{
		}

		void HandleTogglePlayEvent (TogglePlayEvent e)
		{
			if (Player != null) {
				if (e.Playing) {
					Player.Play ();
				} else {
					Player.Pause ();
				}
			}
		}

		//FIXME: fix playlist/project logic and use PlaylistController HandleAddPlaylistElement after MVVMC refactor
		protected override Task HandleAddPlaylistElement (AddPlaylistElementEvent e)
		{
			if (e.Playlist == null) {
				e.Playlist = HandleNewPlaylist (
					new NewPlaylistEvent {
						Project = OpenedProject
					}
				);
				if (e.Playlist == null) {
					return AsyncHelpers.Return (true);
				}
			}

			foreach (var item in e.PlaylistElements) {
				e.Playlist.Elements.Add (item);
			}
			return AsyncHelpers.Return (true);
		}

		//FIXME: fix playlist/project logic and use PlaylistController CreateNewPlaylist after MVVMC refactor
		Playlist HandleNewPlaylist (NewPlaylistEvent e)
		{
			string name = Catalog.GetString ("New playlist");
			Playlist playlist = null;
			bool done = false;
			if (e.Project != null) {
				while (name != null && !done) {
					name = App.Current.Dialogs.QueryMessage (Catalog.GetString ("Playlist name:"), null, name).Result;
					if (name != null) {
						done = true;
						if (e.Project.Playlists.Any (p => p.Name == name)) {
							string msg = Catalog.GetString ("A playlist already exists with the same name");
							App.Current.Dialogs.ErrorMessage (msg);
							done = false;
						}
					}
				}
				if (name != null) {
					playlist = new Playlist { Name = name };
					e.Project.Playlists.Add (playlist);
				}
			}
			return playlist;
		}

		void HandleLoadPlayEvent (LoadEventEvent e)
		{
			if (OpenedProject == null || OpenedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}

			if (e.TimelineEvent?.Duration.MSeconds == 0) {
				// These events don't have duration, we start playing as if it was a seek
				Player.Switch (null, null, null);
				Player.UnloadCurrentEvent ();
				Player.Seek (e.TimelineEvent.EventTime, true);
				Player.Play ();
			} else {
				if (e.TimelineEvent != null) {
					LoadPlay (e.TimelineEvent, new Time (0), true);
				} else if (Player != null) {
					Player.UnloadCurrentEvent ();
				}
			}
		}
	}
}
