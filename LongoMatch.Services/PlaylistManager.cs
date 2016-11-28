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

		public PlaylistManager () : base (new VideoPlayerVM (false))
		{
		}

		public IVideoPlayerController Player {
			get { return PlayerVM.Player; }
			set {
				PlayerVM = new VideoPlayerVM (false) {
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
			App.Current.EventsBroker.Subscribe<OpenedPresentationChangedEvent> (HandleOpenedPresentationChanged);
			App.Current.EventsBroker.Subscribe<PreviousPlaylistElementEvent> (HandlePrev);
			App.Current.EventsBroker.Subscribe<NextPlaylistElementEvent> (HandleNext);

			App.Current.EventsBroker.Subscribe<PlaybackRateChangedEvent> (HandlePlaybackRateChanged);
			App.Current.EventsBroker.Subscribe<TogglePlayEvent> (HandleTogglePlayEvent);
			return true;
		}

		public new bool Stop ()
		{
			base.Stop ();
			App.Current.EventsBroker.Unsubscribe<NewPlaylistEvent> (newPlaylistEventToken);
			App.Current.EventsBroker.Unsubscribe<OpenedPresentationChangedEvent> (HandleOpenedPresentationChanged);
			App.Current.EventsBroker.Unsubscribe<PreviousPlaylistElementEvent> (HandlePrev);
			App.Current.EventsBroker.Unsubscribe<NextPlaylistElementEvent> (HandleNext);

			App.Current.EventsBroker.Unsubscribe<PlaybackRateChangedEvent> (HandlePlaybackRateChanged);
			App.Current.EventsBroker.Unsubscribe<TogglePlayEvent> (HandleTogglePlayEvent);
			return true;
		}

		#endregion

		protected override void HandleLoadPlayEvent (LoadEventEvent e)
		{
			if (OpenedProject == null || OpenedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}
			base.HandleLoadPlayEvent (e);
		}

		protected override void HandleLoadCameraEvent (LoadCameraEvent e)
		{
			if (OpenedProject == null || OpenedProjectType == ProjectType.FakeCaptureProject) {
				return;
			}
			base.HandleLoadCameraEvent (e);
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
	}
}
