// 
//  Copyright (C) 2010 Andoni Morales Alastruey
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
using Gtk;
using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.DB;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;
using LongoMatch.IO;
using LongoMatch.TimeNodes;
using LongoMatch.Video;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Utils;

namespace LongoMatch.Utils
{
	
	
	public class ProjectUtils
	{
		
		public static void SaveFakeLiveProject (Project project, Window window){
			int response;
			MessageDialog md = new MessageDialog(window, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok,
			                                     Catalog.GetString("The project will be saved to a file. You can insert it later into the database using the "+
			                                                     "\"Import project\" function once you copied the video file to your computer"));			                                           
			response = md.Run();
			md.Destroy();
			if (response == (int)ResponseType.Cancel)
				return;		
			                                                                       
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Save Project"),
			                window,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.HomeDir());
			FileFilter filter = new FileFilter();
			filter.Name = "LongoMatch Project";
			filter.AddPattern("*.lpr");

			fChooser.AddFilter(filter);
			if (fChooser.Run() == (int)ResponseType.Accept) {
				Project.Export(project, fChooser.Filename);
				MessagePopup.PopupMessage(window, MessageType.Info, 
				                          Catalog.GetString("Project saved successfully."));			  
			}
			fChooser.Destroy();
		}
		
		public static void ImportProject(Window window){
			Project project;
			bool isFake, exists;
			int res;
			string fileName;
			FileFilter filter;
			NewProjectDialog npd;
			FileChooserDialog fChooser;
						
			/* Show a file chooser dialog to select the file to import */
			fChooser = new FileChooserDialog(Catalog.GetString("Import Project"),
			                                                   window,
			                                                   FileChooserAction.Open,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.HomeDir());
			filter = new FileFilter();
			filter.Name = "LongoMatch Project";
			filter.AddPattern("*.lpr");			
			fChooser.AddFilter(filter);
			
			
			res = fChooser.Run();
			fileName = fChooser.Filename;	
			fChooser.Destroy();		
			/* return if the user cancelled */
			if (res != (int)ResponseType.Accept)
				return;			
			
			/* try to import the project and show a message error is the file
			 * is not a valid project */
			try{
				project = Project.Import(fileName);
			}
			catch (Exception ex){
				MessagePopup.PopupMessage(window, MessageType.Error,
				                          Catalog.GetString("Error importing project:")+
				                          "\n"+ex.Message);	
				return;
			}
			
			isFake = (project.File.FilePath == Constants.FAKE_PROJECT);
			
			/* If it's a fake live project prompt for a video file and
			 * create a new PreviewMediaFile for this project */
			if (isFake){				
				project.File = null;
				npd = new NewProjectDialog();						
				npd.TransientFor = window;
				npd.Use = ProjectType.EditProject;
				npd.Project = project;
				int response = npd.Run();
				while (true){
					if (response != (int)ResponseType.Ok){
						npd.Destroy();
						return;
					} else if (npd.Project == null) {
						MessagePopup.PopupMessage(window, MessageType.Info,
						                          Catalog.GetString("Please, select a video file."));
						response=npd.Run();
					} else {
						project = npd.Project;
						npd.Destroy();	
						break;
					}
				}			
			}			
			
			/* If the project exists ask if we want to overwrite it */
			if (MainClass.DB.Exists(project)){
				MessageDialog md = new MessageDialog(window,
				                                     DialogFlags.Modal,
				                                     MessageType.Question,
				                                     Gtk.ButtonsType.YesNo,
				                                     Catalog.GetString("A project already exists for the file:")+project.File.FilePath+
				                                     "\n"+Catalog.GetString("Do you want to overwrite it?"));
				md.Icon=Stetic.IconLoader.LoadIcon(window, "longomatch", Gtk.IconSize.Dialog, 48);
				res = md.Run();
				md.Destroy();
				if (res != (int)ResponseType.Yes)
					return;
				exists = true;
			} else
				exists = false;
		
			if (isFake)
				CreateThumbnails(window, project);
			if (exists)
				MainClass.DB.UpdateProject(project);
			else
				MainClass.DB.AddProject(project);
				
			
			MessagePopup.PopupMessage(window, MessageType.Info, 
			                          Catalog.GetString("Project successfully imported."));			
		}
		
		public static void CreateNewProject(Window window, 
		                                    out Project project, 
		                                    out ProjectType projectType, 
		                                    out CapturePropertiesStruct captureProps){
			ProjectSelectionDialog psd;
			NewProjectDialog npd;	
			List<Device> devices = null;
			int response;
			
			/* The out parameters must be set before leaving the method */
			project = null;
			projectType = ProjectType.None;
			captureProps = new CapturePropertiesStruct();
			
			/* Show the project selection dialog */
			psd = new ProjectSelectionDialog();
			psd.TransientFor = window;
			response = psd.Run();
			psd.Destroy();
			if (response != (int)ResponseType.Ok)		
				return;
			projectType = psd.Type;
			
			if (projectType == ProjectType.CaptureProject){
				devices = Device.ListVideoDevices();
				if (devices.Count == 0){
					MessagePopup.PopupMessage(window, MessageType.Error,
					                          Catalog.GetString("No capture devices were found."));
					return;
				}
			}	
			
			/* Show the new project dialog and wait to get a valid project 
			 * or quit if the user cancel it.*/
			npd = new NewProjectDialog();
			npd.TransientFor = window;
			npd.Use = projectType;
			if (projectType == ProjectType.CaptureProject)
				npd.Devices = devices;
			response = npd.Run();
			while (true) {
				/* User cancelled: quit */
				if (response != (int)ResponseType.Ok){
					npd.Destroy();
					return;
				}
				/* No file chosen: display the dialog again */
				if (npd.Project == null)
					MessagePopup.PopupMessage(window, MessageType.Info,
					                          Catalog.GetString("Please, select a video file."));
				/* If a project with the same file path exists show a warning */
				else if (MainClass.DB.Exists(npd.Project))
					MessagePopup.PopupMessage(window, MessageType.Error,
					                          Catalog.GetString("This file is already used in another Project.")+"\n"+
					                          Catalog.GetString("Select a different one to continue."));
				
				else {
					/* We are now ready to create the new project */
					project = npd.Project;
					captureProps = npd.CaptureProperties;
					npd.Destroy();
					break;
				}
				response = npd.Run();
			}				
			if (projectType == ProjectType.FileProject) 
				/* We can safelly add the project since we already checked if 
				 * it can can added */
				MainClass.DB.AddProject(project);
		}
		
		public static void ExportToCSV(Window window, Project project){
			FileChooserDialog fChooser;
			FileFilter filter;
			string outputFile;
			CSVExport export;
			
			fChooser = new FileChooserDialog(Catalog.GetString("Select Export File"),
			                window,
			                FileChooserAction.Save,
			                "gtk-cancel",ResponseType.Cancel,
			                "gtk-save",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.HomeDir());
			fChooser.DoOverwriteConfirmation = true;
			filter = new FileFilter();
			filter.Name = "CSV File";
			filter.AddPattern("*.csv");
			fChooser.AddFilter(filter);
			if (fChooser.Run() == (int)ResponseType.Accept) {
				outputFile=fChooser.Filename;
				outputFile = System.IO.Path.ChangeExtension(outputFile,"csv");
				export = new CSVExport(project, outputFile);
				export.WriteToFile();
			}
			fChooser.Destroy();
		}
		
		public static void CreateThumbnails(Window window, Project project){
			MultimediaFactory factory;
			IFramesCapturer capturer;
			BusyDialog dialog;
			
			Console.WriteLine("start thumbnails");
			
			dialog = new BusyDialog();
			dialog.TransientFor = window;
			dialog.Message = Catalog.GetString("Creating video thumbnails. This can take a while."); 
			dialog.Show();
			dialog.Pulse();
			
			/* Create all the thumbnails */
			factory = new MultimediaFactory();
			capturer = factory.getFramesCapturer();
			capturer.Open (project.File.FilePath);
			foreach (List<MediaTimeNode> list in project.GetDataArray()){
				foreach (MediaTimeNode play in list){
					try{
						capturer.SeekTime(play.Start.MSeconds + ((play.Stop - play.Start).MSeconds/2),
						                  true);
						play.Miniature = capturer.GetCurrentFrame(Constants.THUMBNAIL_MAX_WIDTH,
						                                          Constants.THUMBNAIL_MAX_HEIGHT);
						dialog.Pulse();
						
					} catch {
						/* FIXME: Add log */
					}					
				}					
			}	
			capturer.Dispose();
			dialog.Destroy();
		}
	}
}
