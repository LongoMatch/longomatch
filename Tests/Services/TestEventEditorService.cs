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
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services;
using LongoMatch.Services.Controller;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;

namespace Tests.Services
{
	[TestFixture]
	public class TestEventEditorService
	{
		Mock<IStateController> mockStateControler;

		[SetUp]
		public void Setup ()
		{
			mockStateControler = new Mock<IStateController> ();
			App.Current.StateController = mockStateControler.Object;
		}

		[Test]
		public async Task ShowAsync_StatEvent_MoveToSubstitutionsEditorOk ()
		{
			// Arrange
			var projectVM = new LMProjectAnalysisVM ();
			projectVM.Project.Model = Utils.CreateProject ();
			EventEditorController eventEditor = new EventEditorController ();
			eventEditor.SetViewModel (projectVM);
			eventEditor.Start ();

			// Act
			await App.Current.EventsBroker.Publish (new EditEventEvent {
				TimelineEvent = new LMTimelineEventVM () { Model = new StatEvent () }
			});

			// Assert
			mockStateControler.Verify (e => e.MoveToModal (SubstitutionsEditorState.NAME, It.IsAny<object> (), true), Times.Once);
			eventEditor.Stop ();
		}

		[Test]
		public async Task ShowAsync_StatEvent_MoveToPlayEditorOk ()
		{
			// Arrange
			var projectVM = new LMProjectAnalysisVM ();
			projectVM.Project.Model = Utils.CreateProject ();
			EventEditorController eventEditor = new EventEditorController ();
			eventEditor.SetViewModel (projectVM);
			eventEditor.Start ();

			// Act
			await App.Current.EventsBroker.Publish (new EditEventEvent {
				TimelineEvent = new LMTimelineEventVM () { Model = new LMTimelineEvent () }
			});

			// Assert
			mockStateControler.Verify (e => e.MoveToModal (PlayEditorState.NAME, It.IsAny<object> (), true), Times.Once);
			eventEditor.Stop ();
		}
	}
}
