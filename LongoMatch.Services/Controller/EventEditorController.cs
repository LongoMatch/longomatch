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
using VAS.Core.Interfaces.Services;
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
	// TODO [LON-995]: Don't inherit from controller
	public class EventEditorService : ControllerBase, IEventEditorService
	{
		public override void SetViewModel (IViewModel viewModel)
		{
			// FIXME: REMOVE THIS
		}

		public async Task EditEvent (TimelineEventVM timelineEvent)
		{
			PlayEventEditionSettings settings = new PlayEventEditionSettings () {
				EditTags = true,
				EditNotes = true,
				EditPlayers = true,
				EditPositions = true
			};

			await ShowEditionView (settings, timelineEvent);

			// FIXME [LON-995]: Should we call the service directly?
			await App.Current.EventsBroker.Publish (
				new EventEditedEvent {
					TimelineEvent = timelineEvent
				}
			);
		}

		protected virtual async Task ShowEditionView (PlayEventEditionSettings settings, TimelineEventVM timelineEvent)
		{
			dynamic properties = new ExpandoObject ();
			properties.project = timelineEvent.Model.Project;
			properties.play = timelineEvent;

			if (timelineEvent.Model is StatEvent) {
				await App.Current.StateController.MoveToModal (SubstitutionsEditorState.NAME, properties, true);
			} else {
				properties.settings = settings;
				await App.Current.StateController.MoveToModal (PlayEditorState.NAME, properties, true);
			}
		}
	}
}
