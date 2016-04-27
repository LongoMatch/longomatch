//
//  Copyright (C) 2015 jl
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
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.DB
{
	public static class FileStorage
	{
		/// <summary>
		/// Retrieves an object of type T from a file path
		/// </summary>
		/// <returns>The object found.</returns>
		/// <param name="from">The file path to retrieve the object from</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public static T RetrieveFrom<T> (string from) where T : IStorable
		{
			Log.Information ("Loading " + from);
			T storable = Serializer.Instance.LoadSafe<T> (from);
			if (storable is Project) {
				ProjectMigration.Migrate (storable as ProjectLongoMatch);
			} else if (storable is Team) {
				TeamMigration.Migrate (storable as Team);
			} else if (storable is Dashboard) {
				DashboardMigration.Migrate (storable as Dashboard);
			}
			return storable;
		}

		/// <summary>
		/// Stores an object of type T at file at
		/// </summary>
		/// <param name="t">The object to store</param>
		/// <param name="at">The filename to store the object</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public static void StoreAt<T> (T t, string at) where T : IStorable
		{
			Log.Information ("Saving " + t.ID.ToString () + " to " + at);

			if (File.Exists (at)) {
				throw new Exception ("A file already exists at " + at);
			}

			if (!Directory.Exists (Path.GetDirectoryName (at))) {
				Directory.CreateDirectory (Path.GetDirectoryName (at));
			}

			/* Don't cach the Exception here to chain it up */
			Serializer.Instance.Save<T> ((T)t, at);
		}
	}
}

