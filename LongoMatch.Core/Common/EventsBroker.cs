//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.Handlers;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;

namespace LongoMatch.Core.Common
{
	public class EventsBroker : VAS.Core.Common.EventsBroker
	{
		/* IMainController */
		public event NewProjectHandler NewProjectEvent;
		public event OpenNewProjectHandler OpenNewProjectEvent;
		public event OpenProjectHandler OpenProjectEvent;
		public event OpenProjectIDHandler OpenProjectIDEvent;
		public event ImportProjectHandler ImportProjectEvent;
		public event ExportProjectHandler ExportProjectEvent;
		public event ManageJobsHandler ManageJobsEvent;
		public event ManageTeamsHandler ManageTeamsEvent;
		public event ManageDashboardsHandler ManageCategoriesEvent;
		public event ManageProjects ManageProjectsEvent;
		public event ManageDatabases ManageDatabasesEvent;
		public event EditPreferences EditPreferencesEvent;
		public event MigrateDBHandler MigrateDB;
		public event ShowProjectStats ShowProjectStatsEvent;

		/* Player and Capturer */
		public event TickHandler CapturerTick;
		public event ErrorHandler CaptureError;
		public event CaptureFinishedHandler CaptureFinished;

		public event PlayersSubstitutionHandler PlayerSubstitutionEvent;

		public event TeamsTagsChangedHandler TeamTagsChanged;

		/* Query handlers */
		public event QueryToolsHandler QueryTools;

		public void EmitSubstitutionEvent (SportsTeam team, PlayerLongoMatch p1, PlayerLongoMatch p2,
		                                   SubstitutionReason reason, Time time)
		{
			if (PlayerSubstitutionEvent != null) {
				PlayerSubstitutionEvent (team, p1, p2, reason, time);
			}
		}

		public void EmitTeamTagsChanged ()
		{
			if (TeamTagsChanged != null) {
				TeamTagsChanged ();
			}
		}

		public void EmitNewProject (ProjectLongoMatch project)
		{
			if (NewProjectEvent != null)
				NewProjectEvent (project);
		}

		public void EmitOpenProject ()
		{
			if (OpenProjectEvent != null)
				OpenProjectEvent ();
		}

		public void EmitEditPreferences ()
		{
			if (EditPreferencesEvent != null)
				EditPreferencesEvent ();
		}

		public void EmitManageJobs ()
		{
			if (ManageJobsEvent != null)
				ManageJobsEvent ();
		}

		public void EmitManageTeams ()
		{
			if (ManageTeamsEvent != null)
				ManageTeamsEvent ();
		}

		public void EmitManageProjects ()
		{
			if (ManageProjectsEvent != null)
				ManageProjectsEvent ();
		}

		public void EmitManageDatabases ()
		{
			if (ManageDatabasesEvent != null)
				ManageDatabasesEvent ();
		}

		public void EmitManageCategories ()
		{
			if (ManageCategoriesEvent != null)
				ManageCategoriesEvent ();
		}

		public void EmitImportProject ()
		{
			if (ImportProjectEvent != null)
				ImportProjectEvent ();
		}

		public void EmitExportProject (ProjectLongoMatch project)
		{
			if (ExportProjectEvent != null)
				ExportProjectEvent (project);
		}

		public void EmitOpenProjectID (Guid projectID, ProjectLongoMatch project)
		{
			if (OpenProjectIDEvent != null) {
				OpenProjectIDEvent (projectID, project);
			}
		}

		public void EmitOpenNewProject (ProjectLongoMatch project, ProjectType projectType, CaptureSettings captureSettings)
		{
			if (OpenNewProjectEvent != null) {
				OpenNewProjectEvent (project, projectType, captureSettings);
			}
		}

		public void EmitCapturerTick (Time currentTime)
		{
			if (CapturerTick != null) {
				CapturerTick (currentTime);
			}
		}

		/// <summary>
		/// Signals the current capture has finished.
		/// </summary>
		/// <param name="cancel">If set to <c>true</c> the capture was cancelled.</param>
		/// <param name="reopn">If set to <c>true</c> the finished project is reopened.</param>
		public void EmitCaptureFinished (bool cancel, bool reopen)
		{
			if (CaptureFinished != null) {
				CaptureFinished (cancel, reopen);
			}
		}

		public void EmitCaptureError (object sender, string message)
		{
			if (CaptureError != null) {
				CaptureError (sender, message);
			}
		}

		public void EmitMigrateDB ()
		{
			if (MigrateDB != null) {
				MigrateDB ();
			}
		}

		public void EmitShowProjectStats (Project project)
		{
			if (ShowProjectStatsEvent != null)
				ShowProjectStatsEvent (project);
		}

		#region Queries

		/// <summary>
		/// Emit the QueryTools event so that listeners can provide a list of available tools.
		/// </summary>
		/// <param name="tools">an empty list of Tools that will get popuplated.</param>
		public void EmitQueryTools (List<ITool> tools)
		{
			if (QueryTools != null) {
				QueryTools (tools);
			}
		}

		#endregion
	}
}
