// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using Gtk;
using System.Collections.Generic;

using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using Image = LongoMatch.Common.Image;
using LongoMatch.Stats;
using LongoMatch.Interfaces.Multimedia;

namespace LongoMatch.Interfaces.GUI
{
	public interface IGUIToolkit
	{
		IMainController MainController {get;}
		IRenderingStateBar RenderingStateBar {get;}
		Version Version {get;}
	
		void Quit ();
		
		/* Messages */
		void InfoMessage(string message, object parent=null);
		void WarningMessage(string message, object parent=null);
		void ErrorMessage(string message, object parent=null);
		bool QuestionMessage(string message, string title, object parent=null);
		
		/* Files/Folders IO */
		string SaveFile(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
		string OpenFile(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
		List<string> OpenFiles(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
		string SelectFolder(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
			
		IBusyDialog BusyDialog(string message, object parent=null);
			
		List<EditionJob> ConfigureRenderingJob (IPlayList playlist);
		void ExportFrameSeries(Project openenedProject, Play play, string snapshotDir);
		
		void OpenProject (Project project, ProjectType projectType, 
		                  CaptureSettings props, PlaysFilter filter,
		                  out IAnalysisWindow analysisWindow);
		void CloseProject ();
			                     
		void SelectProject(List<ProjectDescription> projects);
		
		void CreateNewProject (Project project=null);
		
		void ShowProjectStats (Project project);
		
		void OpenProjectsManager (Project openedProject);
		void OpenCategoriesTemplatesManager ();
		void OpenTeamsTemplatesManager ();
		void OpenDatabasesManager ();
		void OpenPreferencesEditor ();
		
		void ManageJobs ();
		
		void TagPlay (Play play, Project project);
		void DrawingTool(Image pixbuf, Play play, int drawingIndex);
		
		string RemuxFile (string filePath, string outputFile, VideoMuxerType muxer);
		
		DateTime SelectDate (DateTime date, object widget);
		
		EndCaptureResponse EndCapture (string filepath);

	}
}

