//
//  Copyright (C) 2015 Fluendo S.A.
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

namespace LongoMatch.Core.Store
{
	[Serializable]
	public class StorageInfo: StorableBase
	{
		public StorageInfo ()
		{
			// Force only one StorageInfo object per storage.
			ID = Guid.Empty;
			LastBackup = DateTime.UtcNow;
			LastCleanup = DateTime.UtcNow;
		}

		/// <summary>
		/// Name of the storage.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Date of the last backup.
		/// </summary>
		public DateTime LastBackup { get; set; }

		/// <summary>
		/// Date of the last Database cleanup
		/// </summary>
		public DateTime LastCleanup { get; set; }

		/// <summary>
		/// Version of the storage.
		/// </summary>
		public Version Version { get; set; }
	}
}

