//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Store;
using Constants = LongoMatch.Core.Common.Constants;
using LMCommon = LongoMatch.Core.Common;
using LongoMatch.Services.State;
using LongoMatch.Core.ViewModel;

namespace LongoMatch.Services
{
	public class ToolsManager : IProjectsImporter, IService
	{
		LMProject openedProject;

		public ToolsManager ()
		{
			ProjectImporters = new List<ProjectImporter> ();
		}

		public void RegisterImporter (Func<Project> importFunction,
									  string description, string filterName,
									  string [] extensions, bool needsEdition,
									  bool canOverwrite)
		{
			ProjectImporter importer = new ProjectImporter {
				Description = description,
				ImportFunction = importFunction,
				FilterName = filterName,
				Extensions = extensions,
				NeedsEdition = needsEdition,
				CanOverwrite = canOverwrite,
			};
			ProjectImporters.Add (importer);
		}

		public List<ProjectImporter> ProjectImporters {
			get;
			set;
		}

		void ExportProject (ExportProjectEvent e)
		{
			string filename;

			if (e.Project == null) {
				Log.Warning ("Opened project is null and can't be exported");
			}

			filename = App.Current.Dialogs.SaveFile (Catalog.GetString ("Save project"),
				Utils.SanitizePath (e.Project.Description.Title + Constants.PROJECT_EXT),
				App.Current.HomeDir, Constants.PROJECT_NAME,
				new [] { "*" + Constants.PROJECT_EXT });

			if (filename == null)
				return;

			Path.ChangeExtension (filename, Constants.PROJECT_EXT);

			try {
				Project.Export (e.Project, filename);
				App.Current.Dialogs.InfoMessage (Catalog.GetString ("Project exported successfully"));
			} catch (Exception ex) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error exporting project"));
				Log.Exception (ex);
			}
		}

		ProjectImporter ChooseImporter (IEnumerable<ProjectImporter> importers)
		{
			Dictionary<string, object> options = importers.ToDictionary (i => i.Description, i => (object)i);
			return (ProjectImporter)App.Current.Dialogs.ChooseOption (options).Result;
		}

		void ImportProject (ImportProjectEvent e)
		{
			LMProject project;
			ProjectImporter importer;
			IStorage DB = App.Current.DatabaseManager.ActiveDB;

			Log.Debug ("Importing project");
			/* try to import the project and show a message error is the file
			 * is not a valid project */
			try {
				if (ProjectImporters.Count () == 0) {
					throw new Exception (Catalog.GetString ("Plugin not found"));
				} else if (ProjectImporters.Count () == 1) {
					importer = ProjectImporters.First ();
				} else {
					importer = ChooseImporter (ProjectImporters);
				}

				if (importer == null) {
					return;
				}

				project = importer.ImportFunction () as LMProject;
				if (project == null) {
					return;
				}
				if (importer.NeedsEdition) {
					App.Current.StateController.MoveTo (NewProjectState.NAME, new LMProjectVM { Model = project });
				} else {
					/* If the project exists ask if we want to overwrite it */
					if (!importer.CanOverwrite && DB.Exists (project)) {
						var res = App.Current.Dialogs.QuestionMessage (Catalog.GetString ("A project already exists for this ID:") +
								  project.ID + "\n" +
								  Catalog.GetString ("Do you want to overwrite it?"), null).Result;
						if (!res)
							return;
					}
					DB.Store<LMProject> (project, true);
					App.Current.EventsBroker.Publish<OpenProjectIDEvent> (
						new OpenProjectIDEvent {
							ProjectID = project.ID,
							Project = project
						}
					);
				}
			} catch (Exception ex) {
				App.Current.Dialogs.ErrorMessage (Catalog.GetString ("Error importing project:") +
				"\n" + ex.Message);
				Log.Exception (ex);
				return;
			}
		}

		void HandleMigrateDB (MigrateDBEvent e)
		{
			string db4oPath = Path.Combine (App.Current.baseDirectory, "lib", "cli", "Db4objects.Db4o-8.0");
			string monoPath = Path.GetFullPath (App.Current.LibsDir) + Path.PathSeparator + Path.GetFullPath (db4oPath);
			string migrationExe = Path.GetFullPath (Path.Combine (App.Current.LibsDir, "migration", "LongoMatch.exe"));
			ProcessStartInfo startInfo = new ProcessStartInfo ();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.Arguments = "\"" + migrationExe + "\"";
			startInfo.WorkingDirectory = Path.GetFullPath (Path.Combine (App.Current.baseDirectory, "bin"));
			if (System.Environment.OSVersion.Platform == PlatformID.Win32NT) {
				startInfo.FileName = Path.Combine (App.Current.baseDirectory, "bin", "mono-sgen.exe");
				startInfo.EnvironmentVariables ["MONO_CFG_DIR"] = Path.GetFullPath (
					Path.Combine (App.Current.baseDirectory, "etc"));
			} else {
				startInfo.FileName = "mono-sgen";
			}
			if (startInfo.EnvironmentVariables.ContainsKey ("MONO_PATH")) {
				startInfo.EnvironmentVariables ["MONO_PATH"] += Path.PathSeparator + monoPath;
			} else {
				startInfo.EnvironmentVariables.Add ("MONO_PATH", monoPath);
			}
			Log.Information (String.Format ("Launching migration tool {0} {1}",
				startInfo.FileName,
				startInfo.EnvironmentVariables ["MONO_PATH"]));
			using (Process exeProcess = Process.Start (startInfo)) {
				exeProcess.WaitForExit ();
				App.Current.DatabaseManager.UpdateDatabases ();
				App.Current.DatabaseManager.SetActiveByName (App.Current.DatabaseManager.ActiveDB.Info.Name);
			}
		}

		#region IService

		public int Level {
			get {
				return 50;
			}
		}

		public string Name {
			get {
				return "Tools";
			}
		}

		public bool Start ()
		{
			openedProjectEventToken = App.Current.EventsBroker.Subscribe<OpenedProjectEvent> ((e) => {
				this.openedProject = e.Project as LMProject;
			});

			App.Current.EventsBroker.Subscribe<MigrateDBEvent> (HandleMigrateDB);
			App.Current.EventsBroker.Subscribe<ExportProjectEvent> (ExportProject);
			App.Current.EventsBroker.Subscribe<ImportProjectEvent> (ImportProject);

			return true;
		}

		public bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<MigrateDBEvent> (HandleMigrateDB);
			App.Current.EventsBroker.Unsubscribe<ExportProjectEvent> (ExportProject);
			App.Current.EventsBroker.Unsubscribe<ImportProjectEvent> (ImportProject);

			App.Current.EventsBroker.Unsubscribe<OpenedProjectEvent> (openedProjectEventToken);

			return true;
		}

		#endregion

		//subscriber tokens for lambdas
		EventToken openedProjectEventToken;
	}
}

