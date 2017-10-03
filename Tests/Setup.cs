//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.Threading;
using ICSharpCode.SharpZipLib;
using LongoMatch;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using LongoMatch.Services.State;
using LongoMatch.Services.States;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.DB;
using VAS.Tests;
using LMDB = LongoMatch.DB;
using Timer = VAS.Core.Common.Timer;

namespace Tests
{
	[SetUpFixture]
	public class SetupClass
	{
		[OneTimeSetUp]
		public static void SetUp ()
		{
			// Initialize LongoMath.Core by using a type, this will call the module initialization
			var st = new LMTeam ();
			VFS.SetCurrent (new FileSystem ());

			VAS.App.Current = App.Current = new App ();
			App.InitDependencies ();
			App.Current.Config = new Config ();
			App.InitConstants ();
			App.Current.DependencyRegistry.Register<ITimer, Timer> (1);
			App.Current.DependencyRegistry.Register<IStorageManager, CouchbaseManagerLongoMatch> (1);
			App.Current.DependencyRegistry.Register<IFileStorage, LMDB.FileStorage> (0);
			App.Current.Dialogs = new Mock<IDialogs> ().Object;
			var navigation = new Mock<INavigation> ();
			navigation.Setup (x => x.Push (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			navigation.Setup (x => x.PushModal (It.IsAny<IPanel> (), It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			navigation.Setup (x => x.PopModal (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = navigation.Object;
			RegisterScreenState (HomeState.NAME);

			RegisterScreenState (HomeState.NAME);
			RegisterScreenState (NewProjectState.NAME);
			RegisterScreenState (TeamsManagerState.NAME);
			RegisterScreenState (PreferencesState.NAME);
			RegisterScreenState (ProjectsManagerState.NAME);
			RegisterScreenState (OpenProjectState.NAME);
			RegisterScreenState (PlayEditorState.NAME);
			RegisterScreenState (SubstitutionsEditorState.NAME);
			App.Current.StateController.SetHomeTransition (HomeState.NAME, null);
			App.Current.ResourcesLocator = new DummyResourcesLocator ();
			App.Current.FileSystemManager = new FileSystemManager ();
			Mock<IGUIToolkit> mockGuiToolkit = new Mock<IGUIToolkit> ();
			mockGuiToolkit.Setup (g => g.DeviceScaleFactor).Returns (1.0f);
			App.Current.GUIToolkit = mockGuiToolkit.Object;
		}

		static void RegisterScreenState (string name)
		{
			App.Current.StateController.Register (name, () => CreateScreenState (name));
		}

		static IScreenState CreateScreenState (string name)
		{
			var screenStateMock = new Mock<IScreenState> ();
			screenStateMock.Setup (x => x.LoadState (It.IsAny<object> ())).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.ShowState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.UnloadState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.HideState ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.Panel).Returns (new Mock<IPanel> ().Object);
			screenStateMock.SetupGet (x => x.Name).Returns (name);

			return screenStateMock.Object;
		}
	}

	/// <summary>
	/// Prism's UI thread option works by invoking Post on the current synchronization context.
	/// When we do that, base.Post actually looses SynchronizationContext.Current
	/// because the work has been delegated to ThreadPool.QueueUserWorkItem.
	/// This implementation makes our async-intended call behave synchronously,
	/// so we can preserve and verify sync contexts for callbacks during our unit tests.
	/// </summary>
	internal class MockSynchronizationContext : SynchronizationContext
	{
		public override void Post (SendOrPostCallback d, object state)
		{
			d (state);
		}

		public override void Send (SendOrPostCallback d, object state)
		{
			d (state);
		}
	}
}

