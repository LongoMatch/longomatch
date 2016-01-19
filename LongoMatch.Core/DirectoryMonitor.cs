//
//  Copyright (C) 2016 Fluendo S.A.
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
using System.IO;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers.Misc;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;

namespace LongoMatch.Core
{
	public class DirectoryMonitor: IDirectoryMonitor
	{
		public event FileChangedHandler FileChangedEvent;

		FileSystemWatcher watcher;

		public DirectoryMonitor (string directoryPath)
		{
			DirectoryPath = directoryPath;
		}

		#region IDirectoryMonitor implementation

		public string DirectoryPath {
			get;
			set;
		}

		public void Start ()
		{
			watcher = new FileSystemWatcher (DirectoryPath);
			watcher.Deleted += HandleChanged;
			watcher.Created += HandleChanged;
			watcher.Renamed += HandleRenamed;
			watcher.EnableRaisingEvents = true;
		}

		public void Stop ()
		{
			watcher.EnableRaisingEvents = false;
			watcher.Dispose ();
		}

		#endregion

		void EmitChange (FileChangeType changeType, string path)
		{
			if (FileChangedEvent == null) {
				return;
			}

			FileChangedEvent (changeType, path);
		}

		void HandleChanged (object sender, FileSystemEventArgs e)
		{
			if (e.ChangeType == WatcherChangeTypes.Created) {
				EmitChange (FileChangeType.Created, e.FullPath);
			} else if (e.ChangeType == WatcherChangeTypes.Deleted) {
				EmitChange (FileChangeType.Deleted, e.FullPath);
			}
		}

		void HandleRenamed (object sender, RenamedEventArgs e)
		{
			EmitChange (FileChangeType.Deleted, e.OldFullPath);
			EmitChange (FileChangeType.Created, e.FullPath);
		}
	}
}

