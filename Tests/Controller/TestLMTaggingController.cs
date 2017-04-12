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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.Controller;
using Moq;
using NUnit.Framework;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace Tests.Controller
{
	[TestFixture]
	public class TestLMTaggingController
	{
		LMTaggingController controller;
		PlayerVM player1;
		PlayerVM player2;
		TeamVM team1;
		List<TeamVM> teams;
		LMProjectVM projectVM;
		Mock<IGUIToolkit> mockToolkit;
		Mock<ITimer> timer;

		[SetUp]
		public void Setup ()
		{
			VideoPlayerVM videoPlayer = new VideoPlayerVM {
				CamerasConfig = new ObservableCollection<CameraConfig> ()
			};

			LMProject project = Utils.CreateProject ();

			player1 = new PlayerVM { Model = new Utils.PlayerDummy () };
			player2 = new PlayerVM { Model = new Utils.PlayerDummy () };

			team1 = new TeamVM ();
			team1.ViewModels.Add (player1);
			team1.ViewModels.Add (player2);

			teams = new List<TeamVM> { team1 };

			projectVM = new LMProjectVM { Model = project };

			controller = new LMTaggingController ();
			controller.SetViewModel (new ProjectAnalysisVM<LMProjectVM> { VideoPlayer = videoPlayer, Project = projectVM });
			controller.Start ();

			mockToolkit = new Mock<IGUIToolkit> ();
			App.Current.GUIToolkit = mockToolkit.Object;
			timer = new Mock<ITimer> ();
			App.Current.DependencyRegistry.Register<ITimer> (timer.Object, 1);
		}

		[TearDown]
		public void TearDown ()
		{
			controller.Stop ();
		}

		[Test]
		public void HandleSubcategories_OneSubElementTagged_TempContextCreated ()
		{
			// Arrange
			KeyContext context = new KeyContext ();
			context.KeyActions.AddRange ((List<KeyAction>)controller.GetDefaultKeyActions ());
			App.Current.KeyContextManager.AddContext (context);
			HotKey key = App.Current.Keyboard.ParseName ("h");
			HotKey subkey = App.Current.Keyboard.ParseName ("t");
			projectVM.Dashboard.ViewModels [0].HotKey = key;
			((AnalysisEventButtonVM)projectVM.Dashboard.ViewModels [0]).Tags.First ().HotKey = subkey;
			((AnalysisEventButtonVM)projectVM.Dashboard.ViewModels [0]).CurrentTime = new Time ();

			AutoResetEvent resetEvent = new AutoResetEvent (false);
			mockToolkit.Setup (x => x.Invoke (It.IsAny<EventHandler> ())).Callback ((EventHandler e) => {
				Task actionExecution = Task.Factory.StartNew (() => e (null, null));
				actionExecution.Wait ();
				resetEvent.Set ();
			});

			int taggedElements = 0;
			bool newTagEventCreated = false;
			App.Current.EventsBroker.Subscribe<NewTagEvent> ((x) => {
				newTagEventCreated = true;
				taggedElements = x.Tags.Count;
			});

			// Act
			int existentContexts = App.Current.KeyContextManager.CurrentKeyContexts.Count;
			App.Current.KeyContextManager.HandleKeyPressed (key);
			App.Current.KeyContextManager.HandleKeyPressed (subkey);
			Thread.Sleep (1000); // time has to be expired
			Task.Factory.StartNew (() => timer.Raise (x => x.Elapsed += null, new EventArgs () as ElapsedEventArgs));
			resetEvent.WaitOne (1000);

			// Assert
			Assert.IsTrue (newTagEventCreated);
			Assert.AreEqual (1, taggedElements);
			Assert.AreEqual (0, ((AnalysisEventButtonVM)projectVM.Dashboard.ViewModels [0]).SelectedTags.Count);
		}
	}
}
