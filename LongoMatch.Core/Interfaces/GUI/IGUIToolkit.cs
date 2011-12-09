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
using System.Collections.Generic;

using LongoMatch.Interfaces;
using LongoMatch.Common;
using LongoMatch.Store;
using LongoMatch.Store.Templates;

namespace LongoMatch.Interfaces.GUI
{
	public interface IGUIToolkit
	{
		IMainWindow MainWindow {get;}
	
		/* Messages */
		void InfoMessage(string message);
		void WarningMessage(string message);
		void ErrorMessage(string message);
		bool QuestionMessage(string message, string title);
		
		/* Files/Folders IO */
		string SaveFile(string title, string defaultName, string defaultFolder,
			string filterName, string extensionFilter);
		string OpenFile(string title, string defaultName, string defaultFolder,
			string filterName, string extensionFilter);
		string SelectFolder(string title, string defaultName, string defaultFolder,
			string filterName, string extensionFilter);
			
		IBusyDialog BusyDialog(string message);
			
		Job ConfigureRenderingJob (IPlayList playlist);
		void ExportFrameSeries(Project openenedProject, Play play, string snapshotDir);
		
		ProjectDescription SelectProject(List<ProjectDescription> projects);
		ProjectType SelectNewProjectType();
		
		Project NewCaptureProject(IDatabase db, ITemplatesService ts,
			List<LongoMatch.Common.Device> devices, out CaptureSettings captureSettings);
		Project NewFakeProject(IDatabase db, ITemplatesService ts);
		Project NewFileProject(IDatabase db, ITemplatesService ts);
		Project EditFakeProject(IDatabase db, Project project, ITemplatesService ts);
		
		void OpenProjectsManager(Project openedProject, IDatabase db, ITemplatesService ts);
		void OpenCategoriesTemplatesManager(ICategoriesTemplatesProvider tp);
		void OpenTeamsTemplatesManager(ITeamTemplatesProvider tp);
		
		void ManageJobs(IRenderingJobsManager manager);
		
		void TagPlay(Play play, TeamTemplate local, TeamTemplate visitor);
		void DrawingTool(Image pixbuf, Play play, int stopTime);
	}
}

