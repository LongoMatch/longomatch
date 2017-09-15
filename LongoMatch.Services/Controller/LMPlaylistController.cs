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
using System.Threading.Tasks;
using LongoMatch.Services.State;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace LongoMatch.Services
{
	/// <summary>
	/// This controller should be remove
	/// </summary>
	// FIXME: This controller should be removed once we add bidings to the few commands impleted here.
	[Controller (ProjectAnalysisState.NAME)]
	public class LMPlaylistController : PlaylistController
	{
		IVideoPlayerController Player {
			get {
				return PlayerVM.Player;
			}
		}

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.Subscribe<PreviousPlaylistElementEvent> (HandlePrev);
			App.Current.EventsBroker.Subscribe<NextPlaylistElementEvent> (HandleNext);
			App.Current.EventsBroker.Subscribe<TogglePlayEvent> (HandleTogglePlayEvent);
			App.Current.EventsBroker.Subscribe<MoveElementsEvent<PlaylistVM>> (HandleMovePlaylistEvent);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<PreviousPlaylistElementEvent> (HandlePrev);
			App.Current.EventsBroker.Unsubscribe<NextPlaylistElementEvent> (HandleNext);
			App.Current.EventsBroker.Unsubscribe<TogglePlayEvent> (HandleTogglePlayEvent);
			App.Current.EventsBroker.Unsubscribe<MoveElementsEvent<PlaylistVM>> (HandleMovePlaylistEvent);
		}

		public override void SetViewModel (IViewModel viewModel)
		{
			base.SetViewModel (viewModel);
			ProjectViewModel = ((IProjectDealer)viewModel).Project;
		}

		void HandleNext (NextPlaylistElementEvent e)
		{
			Player.Next ();
		}

		void HandlePrev (PreviousPlaylistElementEvent e)
		{
			Player.Previous ();
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

		protected override void HandleLoadPlayEvent (LoadEventEvent e)
		{
			if (ProjectViewModel == null || ProjectViewModel.ProjectType == ProjectType.FakeCaptureProject) {
				return;
			}
			base.HandleLoadPlayEvent (e);
		}

		void HandleMovePlaylistEvent (MoveElementsEvent<PlaylistVM> e)
		{
			int realIndex = e.Index;
			realIndex -= ViewModel.ViewModels.IndexOf (e.ElementToMove) < e.Index ? 1 : 0;
			ViewModel.ViewModels.Remove (e.ElementToMove);
			ViewModel.ViewModels.Insert (realIndex, e.ElementToMove);
		}
	}
}
