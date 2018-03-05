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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LongoMatch;
using LongoMatch.Core.Hotkeys;
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
using VAS.Services;

namespace Tests.Controller
{
	[TestFixture]
	public class TestLMTaggingController
	{
		LMTaggingController controller;
		LMTeamTaggerController teamController;
		LMProjectVM projectVM;
		Mock<IGUIToolkit> mockToolkit;
		Mock<ITimer> timer;
		AutoResetEvent resetEvent;

		[OneTimeSetUp]
		public void OnetimeSetup ()
		{
			timer = new Mock<ITimer> ();
			App.Current.DependencyRegistry.Register<ITimer> (timer.Object, 1);
		}

		[SetUp]
		public async Task Setup ()
		{
			App.Current.HotkeysService = new HotkeysService ();
			LMGeneralUIHotkeys.RegisterDefaultHotkeys ();
			mockToolkit = new Mock<IGUIToolkit> ();
			mockToolkit.SetupGet (o => o.DeviceScaleFactor).Returns (1.0f);
			App.Current.GUIToolkit = mockToolkit.Object;

			VideoPlayerVM videoPlayer = new VideoPlayerVM {
				CamerasConfig = new ObservableCollection<CameraConfig> ()
			};

			LMProject project = Utils.CreateProject ();

			projectVM = new LMProjectVM { Model = project };

			var projectAnalysisVM = new LMProjectAnalysisVM { VideoPlayer = videoPlayer, Project = projectVM };

			controller = new LMTaggingController ();
			controller.SetViewModel (projectAnalysisVM);
			await controller.Start ();

			teamController = new LMTeamTaggerController ();
			teamController.SetViewModel (projectAnalysisVM);
			await teamController.Start ();

			resetEvent = new AutoResetEvent (false);
			mockToolkit.Setup (x => x.Invoke (It.IsAny<EventHandler> ())).Callback ((EventHandler e) => {
				Task actionExecution = Task.Factory.StartNew (() => e (null, null));
				actionExecution.Wait ();
				resetEvent.Set ();
			});
		}

		[TearDown]
		public async Task TearDown ()
		{
			await controller.Stop ();
			await teamController.Stop ();
		}

		[Test]
		public void GetDefaultKeyActions_GetKeyActions_AllDashboardButtonsRegistered ()
		{
			// Arrange
			KeyContext context = new KeyContext ();

			// Action
			context.KeyActions.AddRange ((List<KeyAction>)controller.GetDefaultKeyActions ());

			// Assert
			Assert.AreEqual (19, context.KeyActions.Count);
		}

		[Test]
		public void KeyActions_HotkeyPerformed_ActionRaised ()
		{
			// Arrange
			KeyContext context = new KeyContext ();
			context.KeyActions.AddRange ((List<KeyAction>)controller.GetDefaultKeyActions ());
			App.Current.KeyContextManager.AddContext (context);
			HotKey key = App.Current.Keyboard.ParseName ("h");
			projectVM.Dashboard.ViewModels [12].HotKey.Model = key;
			HotKey key2 = App.Current.Keyboard.ParseName ("y");
			projectVM.Dashboard.ViewModels [14].HotKey.Model = key2;
			HotKey key3 = App.Current.Keyboard.ParseName ("j");
			projectVM.Dashboard.ViewModels [16].HotKey.Model = key3;

			int taggedElements = 0;
			App.Current.EventsBroker.Subscribe<NewTagEvent> ((x) => {
				taggedElements++;
			});

			// Act
			App.Current.KeyContextManager.HandleKeyPressed (key);
			ExpireTime ();
			App.Current.KeyContextManager.HandleKeyPressed (key2);
			ExpireTime ();

			// Assert
			Assert.AreEqual (2, taggedElements);
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
			projectVM.Dashboard.ViewModels [0].HotKey.Model = key;
			((AnalysisEventButtonVM)projectVM.Dashboard.ViewModels [0]).Tags.First ().HotKey.Model = subkey;
			((AnalysisEventButtonVM)projectVM.Dashboard.ViewModels [0]).CurrentTime = new Time ();

			int taggedElements = 0;
			bool newTagEventCreated = false;
			App.Current.EventsBroker.Subscribe<NewTagEvent> ((x) => {
				newTagEventCreated = true;
				taggedElements = x.Tags.Count ();
			});

			// Act
			int existentContexts = App.Current.KeyContextManager.CurrentKeyContexts.Count;
			App.Current.KeyContextManager.HandleKeyPressed (key);
			App.Current.KeyContextManager.HandleKeyPressed (subkey);
			ExpireTime ();

			// Assert
			Assert.IsTrue (newTagEventCreated);
			Assert.AreEqual (1, taggedElements);
			Assert.AreEqual (0, ((AnalysisEventButtonVM)projectVM.Dashboard.ViewModels [0]).SelectedTags.Count);
		}

		[Test]
		public void Test_TagAndUntag_HomeTeam ()
		{
			// Arrange
			KeyContext context = new KeyContext ();
			context.KeyActions.AddRange ((List<KeyAction>)controller.GetDefaultKeyActions ());
			App.Current.KeyContextManager.AddContext (context);
			HotKey key = App.Current.Keyboard.ParseName ("<Shift_L>+n");

			// Act
			bool taggedStart = projectVM.HomeTeam.Tagged;
			App.Current.KeyContextManager.HandleKeyPressed (key);
			ExpireTime ();

			bool taggedOnce = projectVM.HomeTeam.Tagged;
			App.Current.KeyContextManager.HandleKeyPressed (key);
			ExpireTime ();
			bool taggedTwice = projectVM.HomeTeam.Tagged;

			// Assert
			Assert.AreEqual (taggedStart, !taggedOnce);
			Assert.AreEqual (taggedOnce, !taggedTwice);
		}

		[Test]
		public void Test_TagAndUntag_HomePlayer1 ()
		{
			// Arrange
			KeyContext context = new KeyContext ();
			context.KeyActions.AddRange ((List<KeyAction>)controller.GetDefaultKeyActions ());
			App.Current.KeyContextManager.AddContext (context);
			HotKey homeTeamKey = App.Current.Keyboard.ParseName ("<Shift_L>+n");
			HotKey player1Key = App.Current.Keyboard.ParseName ("1");

			// Act
			bool taggedStart = projectVM.HomeTeam.ViewModels
									.FirstOrDefault (x => ((LMPlayerVM)x).Number == Convert.ToInt32 ("1"))
									.Tagged;

			App.Current.KeyContextManager.HandleKeyPressed (homeTeamKey);
			App.Current.KeyContextManager.HandleKeyPressed (player1Key);
			ExpireTime ();

			bool taggedOnce = projectVM.HomeTeam.ViewModels
									.FirstOrDefault (x => ((LMPlayerVM)x).Number == Convert.ToInt32 ("1"))
									.Tagged;

			App.Current.KeyContextManager.HandleKeyPressed (homeTeamKey);
			App.Current.KeyContextManager.HandleKeyPressed (player1Key);
			ExpireTime ();

			bool taggedTwice = projectVM.HomeTeam.ViewModels
									.FirstOrDefault (x => ((LMPlayerVM)x).Number == Convert.ToInt32 ("1"))
									.Tagged;

			// Assert
			Assert.AreEqual (taggedStart, !taggedOnce);
			Assert.AreEqual (taggedOnce, !taggedTwice);
		}

		/// <summary>
		/// Expires the time to perform hotkey actions.
		/// </summary>
		void ExpireTime ()
		{
			resetEvent.Reset ();
			Thread.Sleep (1000); // time has to be expired
			Task.Factory.StartNew (() => timer.Raise (x => x.Elapsed += null, new EventArgs () as ElapsedEventArgs));
			resetEvent.WaitOne (1000);
		}
	}
}
