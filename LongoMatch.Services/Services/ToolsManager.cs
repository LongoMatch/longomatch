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
using Mono.Unix;

using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Services {

	public class ToolsManager
	{
		
		TemplatesService templatesService;
		Project openedProject;
		IGUIToolkit guiToolkit;
		IMultimediaToolkit multimediaToolkit;
		
		public ToolsManager (IGUIToolkit guiToolkit, IMultimediaToolkit multimediaToolkit,
		                     TemplatesService templatesService)
		{
			this.guiToolkit = guiToolkit;
			this.multimediaToolkit = multimediaToolkit;
			this.templatesService = templatesService;
			Config.EventsBroker.OpenedProjectChanged += (pr, pt, f, a) => {
				this.openedProject = pr;
			};
			
			Config.EventsBroker.EditPreferencesEvent += () => {
				guiToolkit.OpenPreferencesEditor();
			};
			
			Config.EventsBroker.ManageCategoriesEvent += () => {
				guiToolkit.OpenCategoriesTemplatesManager ();
			};
			
			Config.EventsBroker.ManageTeamsEvent += () => {
				guiToolkit.OpenTeamsTemplatesManager ();
			};
			
			Config.EventsBroker.ManageProjectsEvent += () => {
				guiToolkit.OpenProjectsManager(this.openedProject);
			};
			
			Config.EventsBroker.ImportProjectEvent += ImportProject;
			Config.EventsBroker.ExportProjectEvent += ExportProject;
		}
		
		void ExportProject (Project project) {
			if (project == null) {
				Log.Warning("Opened project is null and can't be exported");
			}
			
			string filename = guiToolkit.SaveFile (Catalog.GetString ("Save project"), null,
				Config.HomeDir, Constants.PROJECT_NAME, new string[] {Constants.PROJECT_EXT});
			
			if (filename == null)
				return;
			
			System.IO.Path.ChangeExtension (filename, Constants.PROJECT_EXT);
			
			try {
				Project.Export (project, filename);
				guiToolkit.InfoMessage (Catalog.GetString("Project exported successfully"));
			} catch (Exception ex) {
				guiToolkit.ErrorMessage (Catalog.GetString("Error exporting project"));
				Log.Exception (ex);
			}
		}
		
		private void ImportProject(string name, string filterName, string filter,
		                           Func<string, Project> importProject, bool requiresNewFile) {
			Project project;
			string fileName;
			IDatabase DB = Config.DatabaseManager.ActiveDB;

			Log.Debug("Importing project");
			/* Show a file chooser dialog to select the file to import */
			fileName = guiToolkit.OpenFile(name, null, Config.HomeDir, filterName,
			                               new string[] {filter});
				
			if(fileName == null)
				return;

			/* try to import the project and show a message error is the file
			 * is not a valid project */
			try {
				project = importProject (fileName);
			}
			catch(Exception ex) {
				guiToolkit.ErrorMessage (Catalog.GetString ("Error importing project:") +
				                         "\n"+ex.Message);
				Log.Exception(ex);
				return;
			}

			if (requiresNewFile) {
				string videofile;
				
				guiToolkit.InfoMessage (Catalog.GetString ("This project doesn't have any file associated.\n" +
				                                           "Select one in the next window"));
				videofile = guiToolkit.OpenFile (Catalog.GetString ("Select a video file"), null,
				                                 Config.HomeDir, null, null);
				if (videofile == null) {
					guiToolkit.ErrorMessage (Catalog.GetString ("Could not import project, you need a video file"));
					return;
				} else {
					try {
						project.Description.File = multimediaToolkit.DiscoverFile (videofile);
					} catch (Exception ex) {
						guiToolkit.ErrorMessage (ex.Message);
						return;
					}
					CreateThumbnails (project, guiToolkit, multimediaToolkit.GetFramesCapturer());
				}
			}
			
			/* If the project exists ask if we want to overwrite it */
			if (DB.Exists (project)) {
				var res = guiToolkit.QuestionMessage (Catalog.GetString ("A project already exists for the file:") +
				                                      project.Description.File.FilePath+ "\n" +
				                                      Catalog.GetString ("Do you want to overwrite it?"), null);
				if(!res)
					return;
				DB.UpdateProject(project);
			} else {
				DB.AddProject(project);
			}
			guiToolkit.InfoMessage(Catalog.GetString("Project successfully imported."));
		}
		
		public static void CreateThumbnails(Project project, IGUIToolkit guiToolkit, IFramesCapturer capturer) {
			IBusyDialog dialog;

			dialog = guiToolkit.BusyDialog(Catalog.GetString("Creating video thumbnails. This can take a while."));
			dialog.Show();
			dialog.Pulse();

			/* Create all the thumbnails */
			capturer.Open(project.Description.File.FilePath);
			foreach(Play play in project.AllPlays()) {
				try {
					capturer.Seek (play.Start + ((play.Stop - play.Start) / 2),
					                  true);
					play.Miniature = capturer.GetCurrentFrame (
						Constants.MAX_THUMBNAIL_SIZE,
						Constants.MAX_THUMBNAIL_SIZE);
					dialog.Pulse();

				} catch (Exception ex) {
					Log.Exception(ex);
				}
			}
			capturer.Dispose();
			dialog.Destroy();
		}
	}
}

