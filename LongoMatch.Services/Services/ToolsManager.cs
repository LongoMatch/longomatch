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
using System.IO;
using System.Linq;
using LongoMatch.Addins;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using Mono.Unix;
using System.Reflection;
using System.Diagnostics;

namespace LongoMatch.Services
{
	public class ToolsManager: IProjectsImporter
	{
		
		Project openedProject;
		IGUIToolkit guiToolkit;
		IDataBaseManager dbManager;

		public ToolsManager (IGUIToolkit guiToolkit, IDataBaseManager dbManager)
		{
			this.guiToolkit = guiToolkit;
			this.dbManager = dbManager;
			ProjectImporters = new List<ProjectImporter> ();
			
			RegisterImporter (Project.Import, Constants.PROJECT_NAME,
			                  new string[] {"*"+Constants.PROJECT_EXT }, false);

			Config.EventsBroker.OpenedProjectChanged += (pr, pt, f, a) => {
				this.openedProject = pr;
			};
			
			Config.EventsBroker.EditPreferencesEvent += () => {
				guiToolkit.OpenPreferencesEditor ();
			};
			
			Config.EventsBroker.ManageCategoriesEvent += () => {
				if (openedProject == null || Config.EventsBroker.EmitCloseOpenedProject ()) {
					guiToolkit.OpenCategoriesTemplatesManager ();
				}
			};
			
			Config.EventsBroker.ManageTeamsEvent += () => {
				if (openedProject == null || Config.EventsBroker.EmitCloseOpenedProject ()) {
					guiToolkit.OpenTeamsTemplatesManager ();
				}
			};
			
			Config.EventsBroker.ManageProjectsEvent += () => {
				if (openedProject == null || Config.EventsBroker.EmitCloseOpenedProject ()) {
					guiToolkit.OpenProjectsManager (this.openedProject);
				}
			};
			
			Config.EventsBroker.MigrateDB += HandleMigrateDB;
			
			Config.EventsBroker.ExportProjectEvent += ExportProject;
			Config.EventsBroker.ImportProjectEvent += ImportProject;
		}

		public void RegisterImporter (Func<string, Project> importFunction,
		                              string filterName, string[] extensions, 
		                              bool needsEdition)
		{
			ProjectImporter importer = new ProjectImporter {
				ImportFunction=importFunction,
				FilterName=filterName,
				Extensions=extensions,
				NeedsEdition=needsEdition
			};
			ProjectImporters.Add (importer);
		}

		List<ProjectImporter> ProjectImporters {
			get;
			set;
		}

		void ExportProject (Project project)
		{
			if (project == null) {
				Log.Warning ("Opened project is null and can't be exported");
			}
			
			string filename = guiToolkit.SaveFile (Catalog.GetString ("Save project"), null,
			                                       Config.HomeDir, Constants.PROJECT_NAME, new string[] { Constants.PROJECT_EXT });
			
			if (filename == null)
				return;
			
			System.IO.Path.ChangeExtension (filename, Constants.PROJECT_EXT);
			
			try {
				Project.Export (project, filename);
				guiToolkit.InfoMessage (Catalog.GetString ("Project exported successfully"));
			} catch (Exception ex) {
				guiToolkit.ErrorMessage (Catalog.GetString ("Error exporting project"));
				Log.Exception (ex);
			}
		}

		void ImportProject ()
		{
			Project project;
			ProjectImporter importer;
			string fileName, filterName;
			string[] extensions;
			IDatabase DB = Config.DatabaseManager.ActiveDB;
			
			
			Log.Debug ("Importing project");
			filterName = String.Join ("\n", ProjectImporters.Select (p => p.FilterName));
			extensions = ExtensionMethods.Merge (ProjectImporters.Select (p => p.Extensions).ToList ()); 
			/* Show a file chooser dialog to select the file to import */
			fileName = guiToolkit.OpenFile (Catalog.GetString ("Import project"), null, Config.HomeDir,
			                                filterName, extensions);
				
			if (fileName == null)
				return;

			/* try to import the project and show a message error is the file
			 * is not a valid project */
			try {
				string extension = "*" + Path.GetExtension (fileName);
				importer = ProjectImporters.Where (p => p.Extensions.Contains (extension)).FirstOrDefault ();
				if (importer != null) {
					project = importer.ImportFunction (fileName);
				} else {
					throw new Exception (Catalog.GetString ("Plugin not found"));
				}
				if (importer.NeedsEdition) {
					Config.EventsBroker.EmitNewProject (project);
				} else {
					if (!project.Description.FileSet.CheckFiles ()) {
						if (!guiToolkit.SelectMediaFiles (project)) {
							guiToolkit.ErrorMessage ("No valid video files associated. The project will not be imported");
							return;
						}
					}
					/* If the project exists ask if we want to overwrite it */
					if (DB.Exists (project)) {
						var res = guiToolkit.QuestionMessage (Catalog.GetString ("A project already exists for this ID:") +
						                                      project.ID + "\n" +	Catalog.GetString ("Do you want to overwrite it?"), null);
						if (!res)
							return;
						DB.UpdateProject (project);
					} else {
						DB.AddProject (project);
					}
					Config.EventsBroker.EmitOpenProjectID (project.ID);
				}
			} catch (Exception ex) {
				guiToolkit.ErrorMessage (Catalog.GetString ("Error importing project:") +
				                         "\n" + ex.Message);
				Log.Exception (ex);
				return;
			}
		}
		
		void HandleMigrateDB ()
		{
			string db4oPath = Path.Combine (Config.baseDirectory, "lib", "cli", "Db4objects.Db4o-8.0");
			string monoPath = Path.GetFullPath (Config.LibsDir) + Path.PathSeparator + Path.GetFullPath (db4oPath);
			string migrationExe = Path.Combine (Config.LibsDir, "migration", "LongoMatch.exe");
			ProcessStartInfo startInfo = new ProcessStartInfo ();
			startInfo.CreateNoWindow = true;
			startInfo.UseShellExecute = false;
			startInfo.Arguments = "\"" + migrationExe + "\"";
			startInfo.WorkingDirectory = Path.GetFullPath (Path.Combine (Config.baseDirectory, "bin"));
			if (System.Environment.OSVersion.Platform == PlatformID.Win32NT) {
				startInfo.FileName = "mono";
			} else {
				startInfo.FileName = "mono-sgen";
			}
			if (startInfo.EnvironmentVariables.ContainsKey ("MONO_PATH")) {
				startInfo.EnvironmentVariables["MONO_PATH"] += Path.PathSeparator + monoPath;
			} else {
				startInfo.EnvironmentVariables.Add ("MONO_PATH", monoPath);
			}
			using (Process exeProcess = Process.Start(startInfo)) {
				exeProcess.WaitForExit ();
				dbManager.UpdateDatabases ();
				dbManager.SetActiveByName (dbManager.ActiveDB.Name);
			}
		}
	}

	public class ProjectImporter
	{
		public Func<string, Project> ImportFunction {
			get;
			set;
		}

		public string [] Extensions {
			get;
			set;
		}

		public string FilterName {
			get;
			set;
		}

		public bool NeedsEdition {
			get;
			set;
		}
	}
}

