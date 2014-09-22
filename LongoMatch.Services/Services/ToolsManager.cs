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

namespace LongoMatch.Services
{
	public class ToolsManager: IProjectsImporter
	{
		
		TemplatesService templatesService;
		Project openedProject;
		IGUIToolkit guiToolkit;
		IMultimediaToolkit multimediaToolkit;
		AddinsManager addinsManager;

		public ToolsManager (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit,
		                     TemplatesService templatesService)
		{
			this.guiToolkit = guiToolkit;
			this.multimediaToolkit = multimediaToolkit;
			this.templatesService = templatesService;
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
				guiToolkit.OpenCategoriesTemplatesManager ();
			};
			
			Config.EventsBroker.ManageTeamsEvent += () => {
				guiToolkit.OpenTeamsTemplatesManager ();
			};
			
			Config.EventsBroker.ManageProjectsEvent += () => {
				guiToolkit.OpenProjectsManager (this.openedProject);
			};
			
			Config.EventsBroker.ExportProjectEvent += ExportProject;
			Config.EventsBroker.ImportProjectEvent += ImportProject;
			Config.EventsBroker.CreateThumbnailsEvent += CreateThumbnails;
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

		public static void AddVideoFile (Project project, bool createThumbnails)
		{
			string videofile;
			IGUIToolkit guiToolkit = Config.GUIToolkit;
			IMultimediaToolkit multimediaToolkit = Config.MultimediaToolkit;

			guiToolkit.InfoMessage (Catalog.GetString ("This project doesn't have any file associated.\n" +
				"Select one in the next window"));
			videofile = guiToolkit.OpenFile (Catalog.GetString ("Select a video file"), null,
			                                 Config.HomeDir);
			if (videofile == null) {
				guiToolkit.ErrorMessage (Catalog.GetString ("Could not import project, you need a video file"));
				return;
			} else {
				try {
					project.UpdateMediaFile (multimediaToolkit.DiscoverFile (videofile));
				} catch (Exception ex) {
					guiToolkit.ErrorMessage (ex.Message);
					return;
				}
				if (createThumbnails) {
					CreateThumbnails (project);
				}
			}
		}

		public static void CreateThumbnails (Project project)
		{
			IBusyDialog dialog;
			IFramesCapturer capturer;

			dialog = Config.GUIToolkit.BusyDialog (Catalog.GetString ("Creating video thumbnails. This can take a while."));
			dialog.Show ();
			dialog.Pulse ();

			/* Create all the thumbnails */
			capturer = Config.MultimediaToolkit.GetFramesCapturer ();
			capturer.Open (project.Description.File.FilePath);
			foreach (TimelineEvent play in project.Timeline) {
				try {
					play.Miniature = capturer.GetFrame (play.Start + ((play.Stop - play.Start) / 2),
					                                    true, Constants.MAX_THUMBNAIL_SIZE,
					                                    Constants.MAX_THUMBNAIL_SIZE);
					dialog.Pulse ();

				} catch (Exception ex) {
					Log.Exception (ex);
				}
			}
			capturer.Dispose ();
			dialog.Destroy ();
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
			} catch (Exception ex) {
				guiToolkit.ErrorMessage (Catalog.GetString ("Error importing project:") +
					"\n" + ex.Message);
				Log.Exception (ex);
				return;
			}

			if (importer.NeedsEdition) {
				Config.EventsBroker.EmitNewProject (project);
			} else {
				if (project.Description.File == null) {
					AddVideoFile (project, true);
				} else if (!File.Exists (project.Description.File.FilePath)) {
					AddVideoFile (project, false);
				}
				/* If the project exists ask if we want to overwrite it */
				if (DB.Exists (project)) {
					var res = guiToolkit.QuestionMessage (Catalog.GetString ("A project already exists for the file:") +
						project.Description.File.FilePath + "\n" +
						Catalog.GetString ("Do you want to overwrite it?"), null);
					if (!res)
						return;
					DB.UpdateProject (project);
				} else {
					DB.AddProject (project);
				}
				Config.EventsBroker.EmitOpenProjectID (project.ID);
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

