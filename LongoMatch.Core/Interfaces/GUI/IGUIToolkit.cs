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
using System.Threading.Tasks;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using Image = VAS.Core.Common.Image;

namespace LongoMatch.Core.Interfaces.GUI
{
	public interface IGUIToolkit
	{
		/* Plugable views */
		void Register <I, C> (int priority);

		IPlayerView GetPlayerView ();

		IMainController MainController { get; }

		IRenderingStateBar RenderingStateBar { get; }

		bool FullScreen { set; }

		void Quit ();
		
		/* Messages */
		void InfoMessage (string message, object parent = null);

		void WarningMessage (string message, object parent = null);

		void ErrorMessage (string message, object parent = null);

		Task<bool> QuestionMessage (string message, string title, object parent = null);

		Task<string> QueryMessage (string key, string title = null, string value = "", object parent = null);

		Task<bool> NewVersionAvailable (Version currentVersion, Version latestVersion,
		                                string downloadURL, string changeLog, object parent = null);
		
		/* Files/Folders IO */
		string SaveFile (string title, string defaultName, string defaultFolder,
		                 string filterName, string[] extensionFilter);

		string OpenFile (string title, string defaultName, string defaultFolder,
		                 string filterName = null, string[] extensionFilter = null);

		List<string> OpenFiles (string title, string defaultName, string defaultFolder,
		                        string filterName, string[] extensionFilter);

		string SelectFolder (string title, string defaultName, string defaultFolder,
		                     string filterName, string[] extensionFilter);

		Task<object> ChooseOption (Dictionary<string, object> options, object parent = null);

		IBusyDialog BusyDialog (string message, object parent = null);

		List<EditionJob> ConfigureRenderingJob (Playlist playlist);

		void ExportFrameSeries (ProjectLongoMatch openenedProject, TimelineEvent play, string snapshotDir);

		void OpenProject (Project project, ProjectType projectType, 
		                  CaptureSettings props, EventsFilter filter,
		                  out IAnalysisWindow analysisWindow);

		void CloseProject ();

		void SelectProject (List<ProjectLongoMatch> projects);

		Project ChooseProject (List<Project> projects);

		void Welcome ();

		void LoadPanel (IPanel panel);

		void CreateNewProject (Project project = null);

		void ShowProjectStats (Project project);

		void OpenProjectsManager (Project openedProject);

		void OpenCategoriesTemplatesManager ();

		void OpenTeamsTemplatesManager ();

		void OpenDatabasesManager ();

		void OpenPreferencesEditor ();

		void ManageJobs ();

		Task EditPlay (TimelineEvent play, Project project, bool editTags, bool editPositions, bool editPlayers, bool editNotes);

		void DrawingTool (Image pixbuf, TimelineEvent play, FrameDrawing drawing, CameraConfig config, Project project);

		string RemuxFile (string filePath, string outputFile, VideoMuxerType muxer);

		Task<DateTime> SelectDate (DateTime date, object widget);

		EndCaptureResponse EndCapture (bool isCapturing);

		bool SelectMediaFiles (MediaFileSet fileSet);

		HotKey SelectHotkey (HotKey hotkey, object parent = null);

		void Invoke (EventHandler handler);
	}
}

