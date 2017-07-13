//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.Dynamic;
using System.Threading.Tasks;
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;

namespace LongoMatch.Services.Controller
{
	/// <summary>
	/// Controller that offers graphical event edition
	/// </summary>
	[Controller (ProjectAnalysisState.NAME)]
	[Controller (LiveProjectAnalysisState.NAME)]
	[Controller (FakeLiveProjectAnalysisState.NAME)]
	public class EventEditorController : ControllerBase
	{
		ProjectVM project;

		public override void SetViewModel (IViewModel viewModel)
		{
			project = ((IProjectDealer)viewModel).Project;
		}

		public override async Task Start ()
		{
			await base.Start ();
			App.Current.EventsBroker.SubscribeAsync<EditEventEvent> (HandleEditEvent);
		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.UnsubscribeAsync<EditEventEvent> (HandleEditEvent);
		}

		async Task HandleEditEvent (EditEventEvent e)
		{
			PlayEventEditionSettings settings = new PlayEventEditionSettings () {
				EditTags = true, EditNotes = true, EditPlayers = true, EditPositions = true
			};

			dynamic properties = new ExpandoObject ();
			properties.project = project;
			properties.play = e.TimelineEvent;

			if (e.TimelineEvent is StatEvent) {
				await App.Current.StateController.MoveToModal (SubstitutionsEditorState.NAME, properties, true);
			} else {
				properties.settings = settings;
				await App.Current.StateController.MoveToModal (PlayEditorState.NAME, properties, true);
			}

			await App.Current.EventsBroker.Publish (
				new EventEditedEvent {
					TimelineEvent = e.TimelineEvent
				}
			);
		}
	}
}
