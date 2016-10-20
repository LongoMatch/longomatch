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
using LMDB = LongoMatch.DB;

namespace Tests
{
	[SetUpFixture]
	public class SetupClass
	{
		[SetUp]
		public void Setup ()
		{
			// Initialize LongoMath.Core by using a type, this will call the module initialization
			var st = new SportsTeam ();
			VFS.SetCurrent (new FileSystem ());
			Initialize ();
		}

		public static void Initialize ()
		{
			VAS.App.Current = App.Current = new App ();
			App.InitDependencies ();
			App.Current.Config = new Config ();
			App.InitConstants ();

			App.Current.DependencyRegistry.Register<IStorageManager, CouchbaseManagerLongoMatch> (1);
			App.Current.DependencyRegistry.Register<IFileStorage, LMDB.FileStorage> (0);
			App.Current.Dialogs = new Mock<IDialogs> ().Object;
			var navigation = new Mock<INavigation> ();
			navigation.Setup (x => x.Push (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			navigation.Setup (x => x.PushModal (It.IsAny<IPanel> (), It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			navigation.Setup (x => x.PopModal (It.IsAny<IPanel> ())).Returns (AsyncHelpers.Return (true));
			App.Current.Navigation = navigation.Object;
			App.Current.StateController.Register (HomeState.NAME, () => CreateScreenState ());
			App.Current.StateController.Register (NewProjectState.NAME, () => CreateScreenState ());
			App.Current.StateController.Register (TeamsManagerState.NAME, () => CreateScreenState ());
			App.Current.StateController.Register (PreferencesState.NAME, () => CreateScreenState ());
			App.Current.StateController.Register (ProjectsManagerState.NAME, () => CreateScreenState ());
			App.Current.StateController.Register (OpenProjectState.NAME, () => CreateScreenState ());
			App.Current.StateController.SetHomeTransition (HomeState.NAME, null);
		}

		static IScreenState CreateScreenState ()
		{
			var screenStateMock = new Mock<IScreenState> ();
			screenStateMock.Setup (x => x.PreTransition (It.IsAny<object> ())).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.PostTransition ()).Returns (AsyncHelpers.Return (true));
			screenStateMock.Setup (x => x.Panel).Returns (new Mock<IPanel> ().Object);
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

