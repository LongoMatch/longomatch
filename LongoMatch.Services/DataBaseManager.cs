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
using VAS.Core.Common;
using VAS.Core.Interfaces;
using Constants = LongoMatch.Core.Common.Constants;

namespace LongoMatch.Services
{
	public class DataBaseManager: IService
	{
		const int SUPPORTED_MAJOR_VERSION = Constants.DB_VERSION;

		public IStorageManager Manager {
			get;
			set;
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
			Manager = CreateStorageManager (App.Current.DBDir);
			App.Current.DatabaseManager = Manager;
			Manager.UpdateDatabases ();
			Manager.SetActiveByName (App.Current.Config.CurrentDatabase);
			return true;
		}

		public bool Stop ()
		{
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
			return App.Current.DependencyRegistry.Retrieve<IStorageManager> (InstanceType.Default, storageDir);
		}

	}
}
