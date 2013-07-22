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

namespace LongoMatch.Interfaces.GUI
{
	public interface IGUIToolkit
	{
		IMainWindow MainWindow {get;}
		Version Version {get;}
	
		/* Messages */
		void InfoMessage(string message, Widget parent=null);
		void WarningMessage(string message, Widget parent=null);
		void ErrorMessage(string message, Widget parent=null);
		bool QuestionMessage(string message, string title, Widget parent=null);
		
		/* Files/Folders IO */
		string SaveFile(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
		string OpenFile(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
		List<string> OpenFiles(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
		string SelectFolder(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter);
			
		IBusyDialog BusyDialog(string message);
			
		List<EditionJob> ConfigureRenderingJob (IPlayList playlist);
		void ExportFrameSeries(Project openenedProject, Play play, string snapshotDir);
		
		ProjectDescription SelectProject(List<ProjectDescription> projects);
		ProjectType SelectNewProjectType();
		
		Project NewCaptureProject(IDatabase db, ITemplatesService ts,
			List<LongoMatch.Common.Device> devices, out CaptureSettings captureSettings);
		Project NewURICaptureProject(IDatabase db, ITemplatesService ts,
			out CaptureSettings captureSettings);
		Project NewFakeProject(IDatabase db, ITemplatesService ts);
		Project NewFileProject(IDatabase db, ITemplatesService ts);
		Project EditFakeProject(IDatabase db, Project project, ITemplatesService ts);
		void ShowProjectStats(Project project);
		
		void OpenProjectsManager(Project openedProject, IDatabase db, ITemplatesService ts);
		void OpenCategoriesTemplatesManager(ITemplatesService ts);
		void OpenTeamsTemplatesManager(ITeamTemplatesProvider tp);
		void OpenDatabasesManager(IDataBaseManager dm);
		void OpenPreferencesEditor();
		
		void ManageJobs(IRenderingJobsManager manager);
		
		void TagPlay(Play play, Categories categories, TeamTemplate local, TeamTemplate visitor, bool showAllTags);
		void DrawingTool(Image pixbuf, Play play, int stopTime);
	}
}

