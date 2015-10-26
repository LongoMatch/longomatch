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
using System.Reflection;
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Addins;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Drawing;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;

namespace Tests.Integration
{
	[TestFixture]
	public class TestPostGameAnalysis
	{
		Mock<IDrawingToolkit> drawingToolkitMock;
		Mock<IMultimediaToolkit> multimediaToolkitMock;
		Mock<IGUIToolkit> guiToolkitMock;
		Mock<IAnalysisWindow> analysisWindowMock;
		Mock<IPlayerController> playerControllerMock;
		Mock<IFramesCapturer> capturerMock;
		string tmpPath, homePath;

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
			playerControllerMock = new Mock<IPlayerController> ();
			playerControllerMock.Setup (p => p.StreamLength).Returns (new Time { TotalSeconds = 60000 });
			playerControllerMock.Setup (p => p.CamerasConfig).Returns (
				new ObservableCollection<CameraConfig> { new CameraConfig (0) });

			multimediaToolkitMock = new Mock<IMultimediaToolkit> ();
			multimediaToolkitMock.Setup (m => m.GetFramesCapturer ()).Returns (capturerMock.Object);

			guiToolkitMock = new Mock<IGUIToolkit> ();
			guiToolkitMock.Setup (g => g.RenderingStateBar).Returns (() => new Mock<IRenderingStateBar> ().Object);
			guiToolkitMock.Setup (g => g.SelectMediaFiles (It.IsAny<Project> ())).Returns (true);
			guiToolkitMock.Setup (g => g.BusyDialog(It.IsAny<string>(), It.IsAny<object> ())).Returns(
				() => new DummyBusyDialog ());

			analysisWindowMock = new Mock<IAnalysisWindow> ();
			analysisWindowMock.Setup (a => a.Player).Returns (() => playerControllerMock.Object);
			IAnalysisWindow aw = analysisWindowMock.Object;
			guiToolkitMock.Setup (g => g.OpenProject (It.IsAny<Project> (), It.IsAny<ProjectType> (),
				It.IsAny<CaptureSettings> (), It.IsAny<EventsFilter> (), out aw));
		}

		[TearDown]
		public void Delete ()
		{
			CoreServices.Stop ();
			try {
				foreach (var db in Config.DatabaseManager.Databases) {
					db.Delete ();
				}
				Directory.Delete (tmpPath, true);
			} catch {
			}
		}

		[Test ()]
		public void TestGameAnalysis ()
		{
			Guid projectID;
			CoreServices.Init ();
			AddinsManager.Initialize (Config.PluginsConfigDir, Config.PluginsDir);
			AddinsManager.LoadConfigModifierAddins ();
			Config.EventsBroker = new EventsBroker ();
			Config.DrawingToolkit = drawingToolkitMock.Object;
			Config.MultimediaToolkit = multimediaToolkitMock.Object;
			Config.GUIToolkit = guiToolkitMock.Object;
			Config.AutoSave = true;
			CoreServices.Start (Config.GUIToolkit, Config.MultimediaToolkit);
			AddinsManager.LoadImportProjectAddins (CoreServices.ProjectsImporter);

			// Start importing templates
			Config.TeamTemplatesProvider.Save (
				Config.TeamTemplatesProvider.LoadFile (SaveResource ("spain.ltt"))); 
			Config.TeamTemplatesProvider.Save (
				Config.TeamTemplatesProvider.LoadFile (SaveResource ("france.ltt"))); 
			Config.CategoriesTemplatesProvider.Save (
				Config.CategoriesTemplatesProvider.LoadFile (SaveResource ("basket.lct"))); 
			Assert.AreEqual (4, Config.TeamTemplatesProvider.TemplatesNames.Count);
			Assert.AreEqual (2, Config.CategoriesTemplatesProvider.TemplatesNames.Count);

			// Create a new project and open it
			Project p = CreateProject ();
			projectID = p.ID;
			Config.DatabaseManager.ActiveDB.AddProject (p);
			Config.EventsBroker.EmitOpenProjectID (p.ID, p);

			// Tag some events
			Assert.AreEqual (0, p.Timeline.Count);
			AddEvent (p, 5, 3000, 3050, 3025);
			Assert.AreEqual (1, p.Timeline.Count);
			Project savedP = Config.DatabaseManager.ActiveDB.GetProject (p.ID);
			Assert.AreEqual (1, savedP.Timeline.Count);
			AddEvent (p, 6, 3000, 3050, 3025);
			AddEvent (p, 7, 3000, 3050, 3025);
			AddEvent (p, 8, 3000, 3050, 3025);
			AddEvent (p, 5, 3000, 3050, 3025);
			Assert.AreEqual (5, p.Timeline.Count);
			savedP = Config.DatabaseManager.ActiveDB.GetProject (p.ID);
			Assert.AreEqual (5, savedP.Timeline.Count);

			// Delete some events
			Config.EventsBroker.EmitEventsDeleted (new List<TimelineEvent> { p.Timeline [0], p.Timeline [1] });
			Assert.AreEqual (3, p.Timeline.Count);
			savedP = Config.DatabaseManager.ActiveDB.GetProject (p.ID);
			Assert.AreEqual (3, savedP.Timeline.Count);

			// Now create a new Project with the same templates
			p = CreateProject ();
			Config.DatabaseManager.ActiveDB.AddProject (p);
			Assert.AreEqual (2, Config.DatabaseManager.ActiveDB.Count);
			Config.EventsBroker.EmitOpenProjectID (p.ID, p);

			// Add some events and than remove it from the DB
			AddEvent (p, 6, 3000, 3050, 3025);
			AddEvent (p, 7, 3000, 3050, 3025);
			AddEvent (p, 8, 3000, 3050, 3025);
			AddEvent (p, 5, 3000, 3050, 3025);
			Config.DatabaseManager.ActiveDB.RemoveProject (p);

			// Reopen the old project
			savedP = Config.DatabaseManager.ActiveDB.GetAllProjects ().FirstOrDefault (pr => pr.ID == projectID);
			Config.EventsBroker.EmitOpenProjectID (savedP.ID, savedP);
			Config.EventsBroker.EmitSaveProject (savedP, ProjectType.FileProject);

			// Export this project to a new file
			savedP = Config.DatabaseManager.ActiveDB.GetProject (projectID);
			Assert.AreEqual (3, savedP.Timeline.Count);
			Assert.AreEqual (12, savedP.LocalTeamTemplate.List.Count);
			Assert.AreEqual (12, savedP.VisitorTeamTemplate.List.Count);
			string tmpFile = Path.Combine (tmpPath, "longomatch.lgm"); 
			guiToolkitMock.Setup (g => g.SaveFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string[]> ())).Returns (tmpFile);
			Config.EventsBroker.EmitExportProject (p);
			Assert.IsTrue (File.Exists (tmpFile));
			savedP = Project.Import (tmpFile);
			Assert.IsNotNull (savedP);

			// Import a new project
			string projectPath = SaveResource ("spain_france_test.lgm");
			ProjectImporter importer = CoreServices.toolsManager.ProjectImporters.FirstOrDefault
				(i => i.Description == "Import LongoMatch project"); 
			guiToolkitMock.Setup (g => g.ChooseOption (It.IsAny<Dictionary<string, object>> (), null)).Returns (
				Task.Factory.StartNew (() => (object)importer));
			guiToolkitMock.Setup (g => g.OpenFile (It.IsAny<string> (), It.IsAny<string> (), It.IsAny<string> (),
				It.IsAny<string> (), It.IsAny<string[]> ())).Returns (projectPath);
			Config.EventsBroker.OpenedProjectChanged += (project, pt, f, a) => {p = project;};
			Config.EventsBroker.EmitImportProject ();
			Assert.AreEqual (2, Config.DatabaseManager.ActiveDB.Count);
			int eventsCount = p.Timeline.Count;
			AddEvent (p, 2, 3000, 3050, 3025);
			AddEvent (p, 3, 3000, 3050, 3025);
			Config.EventsBroker.EmitCloseOpenedProject ();
			savedP = Config.DatabaseManager.ActiveDB.GetProject (p.ID);
			Assert.AreEqual (eventsCount + 2, savedP.Timeline.Count);
			CoreServices.Stop ();
		}

		void AddEvent (Project p, int idx, int start, int stop, int eventTime)
		{
			Config.EventsBroker.EmitNewEvent (p.EventTypes [idx], null, TeamType.LOCAL, null,
				new Time { TotalSeconds = start }, new Time { TotalSeconds = stop }, new Time { TotalSeconds = eventTime }, null, null);
		}

		string SaveResource (string name)
		{
			string filePath;
			var assembly = Assembly.GetExecutingAssembly ();
			using (Stream inS = assembly.GetManifestResourceStream (name)) {
				filePath = Path.Combine (tmpPath, name);
				using (Stream outS = new FileStream (filePath, FileMode.Create)) {
					inS.CopyTo (outS);
				}
			}
			return filePath;
		}

		Project CreateProject ()
		{
			Project project = new Project { Description = new ProjectDescription () };
			project.LocalTeamTemplate = Config.TeamTemplatesProvider.Load ("spain");
			project.VisitorTeamTemplate = Config.TeamTemplatesProvider.Load ("france");
			project.Dashboard = Config.CategoriesTemplatesProvider.Load ("basket");
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

