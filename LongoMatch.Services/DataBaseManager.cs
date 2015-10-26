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
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.DB;

namespace LongoMatch.Services
{
	public class DataBaseManager: IService
	{
		const int SUPPORTED_MAJOR_VERSION = Constants.DB_MAYOR_VERSION;

		public DataBaseManager ()
		{
		}

		public Project OpenedProject {
			get;
			set;
		}

		public IDataBaseManager Manager {
			get;
			set;
		}

		void HandleManageDatabase ()
		{
			if (OpenedProject != null) {
				var msg = Catalog.GetString ("Close the current project to open the database manager");
				Config.GUIToolkit.ErrorMessage (msg);
			} else {
				Config.GUIToolkit.OpenDatabasesManager ();
			}
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType, EventsFilter filter,
			IAnalysisWindow analysisWindow)
		{
			OpenedProject = project;
		}

		#region IService

		public int Level {
			get {
				return 20;
			}
		}

		public string Name {
			get {
				return "Database";
			}
		}

		public bool Start ()
		{
			Config.EventsBroker.ManageDatabasesEvent += HandleManageDatabase;
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			Manager = new CouchbaseManager (Config.DBDir);
			Config.DatabaseManager = Manager;
			Manager.UpdateDatabases ();
			Manager.SetActiveByName (Config.CurrentDatabase);
			return true;
		}

		public bool Stop ()
		{
			Config.EventsBroker.ManageDatabasesEvent -= HandleManageDatabase;
			Config.EventsBroker.OpenedProjectChanged -= HandleOpenedProjectChanged;
			Config.DatabaseManager = Manager = null;
			return true;
		}

		#endregion
	}
}

