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
using System.Linq;
using System.Reflection;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using LongoMatch.Store;
using NUnit.Framework;

namespace Tests.Integration
{
	[TestFixture]
	public class TestDatabaseMigrationV1
	{
		[TearDown]
		public void Reset ()
		{
			SetupClass.Initialize ();
		}

		[Test]
		public void TestMigration ()
		{
			string dir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (dir);

			var assembly = Assembly.GetExecutingAssembly ();
			using (Stream fs = assembly.GetManifestResourceStream ("longomatch.tar.gz")) {
				using (Stream gzipStream = new GZipInputStream (fs)) {
					using (TarArchive tarArchive = TarArchive.CreateInputTarArchive (gzipStream)) {
						tarArchive.ExtractContents (dir);
					}
				}
			}

			CouchbaseStorageLongoMatch storage = new CouchbaseStorageLongoMatch (dir, "longomatch");
			Assert.AreEqual (2, storage.RetrieveAll<SportsTeam> ().Count ());
			Assert.AreEqual (1, storage.RetrieveAll<DashboardLongoMatch> ().Count ());
			Assert.AreEqual (1, storage.RetrieveAll<ProjectLongoMatch> ().Count ());

			SportsTeam team = storage.RetrieveAll<SportsTeam> ().First ();
			Assert.DoesNotThrow (team.Load);

			DashboardLongoMatch dashboard = storage.RetrieveAll<DashboardLongoMatch> ().First ();
			Assert.DoesNotThrow (dashboard.Load);

			ProjectLongoMatch project = storage.RetrieveAll<ProjectLongoMatch> ().First ();
			Assert.DoesNotThrow (project.Load);
		}
	}
}

