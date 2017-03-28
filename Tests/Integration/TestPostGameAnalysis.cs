//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Addins;
using LongoMatch.Core.Events;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.DB;
using LongoMatch.Plugins;
using LongoMatch.Services;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace Tests.Integration
{
	[TestFixture]
	public class TestPostGameAnalysis
	{
		Mock<IDrawingToolkit> drawingToolkitMock;
		Mock<IMultimediaToolkit> multimediaToolkitMock;
		Mock<IGUIToolkit> guiToolkitMock;
		Mock<IVideoPlayerController> playerControllerMock;
		Mock<IFramesCapturer> capturerMock;
		Mock<IVideoPlayer> playerMock;
		Mock<IDialogs> mockDialogs;
		string tmpPath, homePath;
		CoreEventsController eventsController;
		ToolsManager toolsManager;

		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			// Register the events manager
			eventsController = new CoreEventsController ();
			toolsManager = new ToolsManager ();
		}

		[TestFixtureTearDown ()]
		public void FixtureTearDown ()
		{
			eventsController.Stop ();
			toolsManager.Stop ();
		}

		[SetUp]
		public void Init ()
		{
			tmpPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			homePath = Path.Combine (tmpPath, "LongoMatch");
			Directory.CreateDirectory (tmpPath);
			Environment.SetEnvironmentVariable ("LONGOMATCH_HOME", tmpPath);
			Environment.SetEnvironmentVariable ("LGM_UNINSTALLED", "1");
			drawingToolkitMock = new Mock<IDrawingToolkit> ();

			capturerMock = new Mock<IFramesCapturer> ();
			playerControllerMock = new Mock<IVideoPlayerController> ();
			playerControllerMock.Setup (p => p.StreamLength).Returns (new Time { TotalSeconds = 60000 });
			playerControllerMock.Setup (p => p.CamerasConfig).Returns (
				new ObservableCollection<CameraConfig> { new CameraConfig (0) });

			playerMock = new Mock<IVideoPlayer> ();
			playerMock.SetupAllProperties ();

			multimediaToolkitMock = new Mock<IMultimediaToolkit> ();
			multimediaToolkitMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);
			multimediaToolkitMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);

			guiToolkitMock = new Mock<IGUIToolkit> ();
			mockDialogs = new Mock<IDialogs> ();
			guiToolkitMock.Setup (g => g.SelectMediaFiles (It.IsAny<MediaFileSet> ())).Returns (true);
			mockDialogs.Setup (g => g.BusyDialog (It.IsAny<string> (), It.IsAny<object> ())).Returns (
				() => new DummyBusyDialog ());
		}

		[TearDown]
		public void Delete ()
		{
			try {
				foreach (var db in App.Current.DatabaseManager.Databases) {
					db.Reset ();
				}
				Directory.Delete (tmpPath, true);
			} catch {
			}
			CoreServices.Stop ();
			SetupClass.Initialize ();
		}

		[Test ()]
		public void TestGameAnalysis ()
		{
			Guid projectID;
			App.Init ();
			CoreServices.Init ();
			App.Current.DependencyRegistry.Register<VAS.DB.IFileStorage, FileStorage> (0);

			AddinsManager.Initialize (App.Current.PluginsConfigDir, App.Current.PluginsDir);
			AddinsManager.LoadConfigModifierAddins ();
			App.Current.DrawingToolkit = drawingToolkitMock.Object;
			App.Current.MultimediaToolkit = multimediaToolkitMock.Object;
			App.Current.GUIToolkit = guiToolkitMock.Object;
			App.Current.Dialogs = mockDialogs.Object;
			App.Current.Config.AutoSave = true;
			CoreServices.Start (App.Current.GUIToolkit, App.Current.MultimediaToolkit);
			AddinsManager.LoadImportProjectAddins (CoreServices.ProjectsImporter);

			// Do controllers start in here since the EventBroker is instantiante again in the test
			eventsController.Start ();
			toolsManager.Start ();

			// Start importing templates
			App.Current.TeamTemplatesProvider.Save (
				App.Current.TeamTemplatesProvider.LoadFile (Utils.SaveResource ("spain.ltt", tmpPath)));
			App.Current.TeamTemplatesProvider.Save (
				App.Current.TeamTemplatesProvider.LoadFile (Utils.SaveResource ("france.ltt", tmpPath)));
			App.Current.CategoriesTemplatesProvider.Save (
				App.Current.CategoriesTemplatesProvider.LoadFile (Utils.SaveResource ("basket.lct", tmpPath)));
			Assert.AreEqual (4, App.Current.TeamTemplatesProvider.Templates.Count);
			Assert.AreEqual (2, App.Current.CategoriesTemplatesProvider.Templates.Count);

			// Create a new project and open it
			LMProject p = CreateProject ();
			projectID = p.ID;
			LMProjectAnalysisVM viewModel;
			viewModel = new LMProjectAnalysisVM ();
			IVideoPlayerController playerController = new Mock<IVideoPlayerController> ().Object;
			viewModel.Project = new LMProjectVM { Model = p };
			viewModel.VideoPlayer = new VideoPlayerVM { Player = playerController };
			eventsController.SetViewModel (viewModel);

			App.Current.DatabaseManager.ActiveDB.Store<LMProject> (p, true);

			// Tag some events
			Assert.AreEqual (0, p.Timeline.Count);
			AddEvent (p, 5, 3000, 3050, 3025);
			Assert.AreEqual (1, p.Timeline.Count);
			LMProject savedP = App.Current.DatabaseManager.ActiveDB.Retrieve<LMProject> (p.ID);
			Assert.AreEqual (1, savedP.Timeline.Count);
			AddEvent (p, 6, 3000, 3050, 3025);
			AddEvent (p, 7, 3000, 3050, 3025);
			AddEvent (p, 8, 3000, 3050, 3025);
			AddEvent (p, 5, 3000, 3050, 3025);
			Assert.AreEqual (5, p.Timeline.Count);
			savedP = App.Current.DatabaseManager.ActiveDB.Retrieve<LMProject> (p.ID);
			Assert.AreEqual (5, savedP.Timeline.Count);

			// Delete some events
			App.Current.EventsBroker.Publish<EventsDeletedEvent> (
				new EventsDeletedEvent {
					TimelineEvents = new List<TimelineEvent> {
						p.Timeline [0],
						p.Timeline [1]
					}
				}
			);
			Assert.AreEqual (3, p.Timeline.Count);
			savedP = App.Current.DatabaseManager.ActiveDB.Retrieve<LMProject> (p.ID);
			Assert.AreEqual (3, savedP.Timeline.Count);

			// Now create a new LMProject with the same templates
			LMProject newProject = CreateProject ();
			viewModel.Project.Model = newProject;
			App.Current.DatabaseManager.ActiveDB.Store<LMProject> (newProject);
			Assert.AreEqual (2, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());

			// Add some events and than remove it from the DB
			AddEvent (newProject, 6, 3000, 3050, 3025);
			AddEvent (newProject, 7, 3000, 3050, 3025);
			AddEvent (newProject, 8, 3000, 3050, 3025);
			AddEvent (newProject, 5, 3000, 3050, 3025);
			App.Current.DatabaseManager.ActiveDB.Delete<LMProject> (newProject);

			// Reopen the old project
			savedP = App.Current.DatabaseManager.ActiveDB.RetrieveAll<LMProject> ().FirstOrDefault (pr => pr.ID == projectID);
			viewModel.Project.Model = savedP;

			App.Current.EventsBroker.Publish<SaveProjectEvent> (
				new SaveProjectEvent {
					Project = savedP,
					ProjectType = ProjectType.FileProject
				}
			);

			// Export this project to a new file
			savedP = App.Current.DatabaseManager.ActiveDB.Retrieve<LMProject> (projectID);
			Assert.AreEqual (3, savedP.Timeline.Count);
			Assert.AreEqual (12, savedP.LocalTeamTemplate.List.Count);
			Assert.AreEqual (12, savedP.VisitorTeamTemplate.List.Count);
			string tmpFile = Path.Combine (tmpPath, "longomatch.lgm");
			mockDialogs.Setup (g => g.SaveFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string []> ())).Returns (tmpFile);
			App.Current.EventsBroker.Publish<ExportProjectEvent> (new ExportProjectEvent { Project = p });
			Assert.IsTrue (File.Exists (tmpFile));
			savedP = Project.Import (tmpFile) as LMProject;
			Assert.IsNotNull (savedP);

			// Import a new project
			LongoMatchImporter importPlugin = new LongoMatchImporter ();
			ProjectImporter importer = new ProjectImporter {
				Description = importPlugin.Description + " test ",
				ImportFunction = new Func<Project> (importPlugin.ImportProject),
				FilterName = importPlugin.FilterName,
				Extensions = importPlugin.FilterExtensions,
				NeedsEdition = importPlugin.NeedsEdition,
				CanOverwrite = importPlugin.CanOverwrite,
			};
			CoreServices.toolsManager.ProjectImporters.Add (importer);
			p = null;
			string projectPath = Utils.SaveResource ("spain_france_test.lgm", tmpPath);
			mockDialogs.Setup (g => g.ChooseOption (
				It.IsAny<Dictionary<string, object>> (), It.IsAny<string> (), It.IsAny<object> ())).
					   Returns (Task.Factory.StartNew (() => (object)importer)
			);
			mockDialogs.Setup (g => g.OpenFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string []> ())).Returns (projectPath);
			App.Current.EventsBroker.Subscribe<OpenProjectIDEvent> ((e) => {
				p = e.Project as LMProject;
				viewModel.Project.Model = p;
			});
			App.Current.EventsBroker.Publish<ImportProjectEvent> (new ImportProjectEvent ());
			Assert.IsNotNull (p);
			Assert.AreEqual (2, App.Current.DatabaseManager.ActiveDB.Count<LMProject> ());
			int eventsCount = p.Timeline.Count;

			AddEvent (p, 2, 3000, 3050, 3025);
			AddEvent (p, 3, 3000, 3050, 3025);
			App.Current.EventsBroker.Publish (new CloseOpenedProjectEvent ());
			savedP = App.Current.DatabaseManager.ActiveDB.Retrieve<LMProject> (p.ID);
			Assert.AreEqual (eventsCount + 2, savedP.Timeline.Count);
		}

		void AddEvent (LMProject p, int idx, int start, int stop, int eventTime)
		{
			App.Current.EventsBroker.Publish<NewEventEvent> (
				new NewEventEvent {
					EventType = p.EventTypes [idx],
					Players = null,
					Teams = new ObservableCollection<Team> { p.LocalTeamTemplate },
					Tags = null,
					Start = new Time { TotalSeconds = start },
					Stop = new Time { TotalSeconds = stop },
					EventTime = new Time { TotalSeconds = eventTime }
				}
			);
		}

		LMProject CreateProject ()
		{
			var project = new LMProject { Description = new ProjectDescription () };
			project.LocalTeamTemplate = (LMTeam)App.Current.TeamTemplatesProvider.Templates.FirstOrDefault (t => t.Name == "spain");
			Assert.IsNotNull (project.LocalTeamTemplate);
			project.VisitorTeamTemplate = (LMTeam)App.Current.TeamTemplatesProvider.Templates.FirstOrDefault (t => t.Name == "france");
			Assert.IsNotNull (project.VisitorTeamTemplate);
			project.Dashboard = App.Current.CategoriesTemplatesProvider.Templates.FirstOrDefault (t => t.Name == "basket") as LMDashboard;
			Assert.IsNotNull (project.Dashboard);
			project.Description.Competition = "Liga";
			project.Description.MatchDate = DateTime.UtcNow;
			project.Description.Description = "Created by LongoMatch";
			project.Description.Season = "2015-2016";
			project.Description.LocalName = project.LocalTeamTemplate.TeamName;
			project.Description.VisitorName = project.VisitorTeamTemplate.TeamName;
			project.Description.FileSet = new MediaFileSet ();
			project.Description.FileSet.Add (new MediaFile {
				FilePath = "Test.mp4",
				Duration = new Time { TotalSeconds = 60000 }
			});
			project.UpdateEventTypesAndTimers ();
			return project;
		}
	}
}

