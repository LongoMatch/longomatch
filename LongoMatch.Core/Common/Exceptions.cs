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
using System.Collections.Generic;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Core.Common
{
	public class DBLockedException: Exception
	{
		public DBLockedException (Exception innerException) :
			base (Catalog.GetString ("Database locked:" + innerException.Message),
			      innerException)
		{
		}
	}

	public class UnknownDBErrorException: Exception
	{
		public UnknownDBErrorException (Exception innerException) :
			base (Catalog.GetString ("Unknown database error:" + innerException),
			      innerException)
		{
		}
	}

	public class SubstitutionException: Exception
	{
		public SubstitutionException (string error) : base (error)
		{
		}
	}

	public class ProjectDeserializationException: Exception
	{
		public ProjectDeserializationException (Exception innerException) :
			base (Catalog.GetString ("Project loading failed:") + innerException,
			      innerException)
		{
		}
	}

	public class ProjectNotFoundException: Exception
	{
		public ProjectNotFoundException (string file) :
			base (Catalog.GetString ("Project file not found:\n") + file)
		{
		}
	}

	public class InvalidTemplateFilenameException: Exception
	{
		public InvalidTemplateFilenameException (List<char> invalidChars) :
			base (Catalog.GetString ("The name contains invalid characters: ") + String.Join (" ", invalidChars))
		{
		}
	}

	public class TemplateNotFoundException<T>: Exception where T: ITemplate
	{
		public TemplateNotFoundException (string name) :
			base (GenerateMessage (name))
		{
		}

		private static string GenerateMessage (string name)
		{
			if (typeof(T) == typeof(Team)) {
				return Catalog.GetString ("Team not found:\n") + name;
			} else if (typeof(T) == typeof(Dashboard)) {
				return Catalog.GetString ("Dashboard not found:\n") + name;
			} else {
				return Catalog.GetString ("Template not found:\n") + name;
			}
		}
	}

	public class HotkeyAlreadyInUse: Exception
	{
		public HotkeyAlreadyInUse (HotKey hotkey) :
			base (Catalog.GetString ("Hotkey already in use: ") + hotkey)
		{
		}
	}

	public class TimerNotRunningException: Exception
	{
	}

	public class AddinRequestShutdownException: Exception
	{
		public AddinRequestShutdownException (string reason) :
			base (reason)
		{
		}
	}

	public class CircularDependencyException: Exception
	{
	}

	public class ImportException: Exception
	{
		public ImportException (string reason) : base (reason)
		{
		}
	}

	public class InvalidQueryException: Exception
	{
		public InvalidQueryException (string reason) : base (reason)
		{
		}
	}

	public class StorageException: Exception
	{
		public StorageException (string reason) : base (reason)
		{
		}
	}

}

