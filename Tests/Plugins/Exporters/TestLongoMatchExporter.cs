//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.Threading.Tasks;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Plugins;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;
using VAS.Core.Store;

namespace Tests.Plugins.Exporters
{
	public class TestLongoMatchExporter
	{
		[Test]
		public async Task ExportProject_NoLimitation_ExportFinishedOk ()
		{
			// Arrange
			LMProject p = null;
			string tmp = Path.GetTempFileName ();
			if (File.Exists (tmp)) {
				File.Delete (tmp);
			}

			try {
				p = Utils.CreateProject (false);
				DummyLongoMAtchExporterExporter exporter = new DummyLongoMAtchExporterExporter ();

				// Act
				await exporter.Export (p, tmp);

				// Assert
				Assert.IsTrue (File.Exists (tmp));
				Assert.IsTrue (exporter.ExportDone);

			} finally {
				Utils.DeleteProject (p);
				if (File.Exists (tmp)) {
					File.Delete (tmp);
				}
			}
		}
	}

	class DummyLongoMAtchExporterExporter : LongoMatchExporter
	{
		public async Task Export (Project project, string filename)
		{
			await this.ExportProject (project, filename);
		}
	}
}
