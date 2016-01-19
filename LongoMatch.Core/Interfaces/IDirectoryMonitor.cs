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
using System.Threading;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers.Misc;

namespace LongoMatch.Core.Interfaces
{

	/// <summary>
	/// An interface for monitoring file changes in a directory.
	/// </summary>
	public interface IDirectoryMonitor
	{
		event FileChangedHandler FileChangedEvent;

		/// <summary>
		/// The path of the directory to monitor.
		/// </summary>
		string DirectoryPath { get; set; }

		/// <summary>
		/// Starts monitoring the directory
		/// </summary>
		void Start ();

		/// <summary>
		/// Stops monitoring the directory
		/// </summary>
		void Stop ();
	}
}

