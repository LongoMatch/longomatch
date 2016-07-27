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
using LongoMatch.Core.Common;
using LongoMatch.Core.Events;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Store;
using LongoMatch.DB;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using Constants = LongoMatch.Core.Common.Constants;
using LMCommon = LongoMatch.Core.Common;
using VASFilters = VAS.Core.Filters;

namespace LongoMatch.Services
{
	public class DataBaseManager: IService
	{
		const int SUPPORTED_MAJOR_VERSION = Constants.DB_VERSION;

		public DataBaseManager ()
		{
		}

		public ProjectLongoMatch OpenedProject {
			get;
			set;
		}

		public IStorageManager Manager {
			get;
			set;
		}

		void HandleManageDatabase (ManageDatabasesEvent e)
		{
			if (OpenedProject != null) {
				var msg = Catalog.GetString ("Close the current project to open the database manager");
				App.Current.GUIToolkit.ErrorMessage (msg);
			} else {
				App.Current.GUIToolkit.OpenDatabasesManager ();
			}
		}

		void HandleOpenedProjectChanged (OpenedProjectEvent e)
		{
			OpenedProject = e.Project as ProjectLongoMatch;
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
			App.Current.EventsBroker.Subscribe<ManageDatabasesEvent> (HandleManageDatabase);
			App.Current.EventsBroker.Subscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			Manager = CreateStorageManager (App.Current.DBDir);
			App.Current.DatabaseManager = Manager;
			Manager.UpdateDatabases ();
			Manager.SetActiveByName (App.Current.Config.CurrentDatabase);
			return true;
		}

		public bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<ManageDatabasesEvent> (HandleManageDatabase);
			App.Current.EventsBroker.Unsubscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			App.Current.DatabaseManager = Manager = null;
			return true;
		}

		#endregion

		/// <summary>
		/// Creates a new storage manager.
		/// </summary>
		/// <returns>The created manager.</returns>
		/// <param name="storageDir">The directory used for the storages.</param>
		public static IStorageManager CreateStorageManager (string storageDir)
		{
			return App.Current.DependencyRegistry.Retrieve<IStorageManager> (storageDir);
		}

	}
}
