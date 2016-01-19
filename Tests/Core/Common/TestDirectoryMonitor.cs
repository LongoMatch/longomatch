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
using NUnit.Framework;
using System.IO;
using LongoMatch.Core;
using LongoMatch.Core.Common;

namespace Tests.Core.Common
{
	[TestFixture]
	public class TestDirectoryMonitor
	{
		string tmpDir;
		int addedChanged;
		int deletedChanged;
		DirectoryMonitor monitor;

		[SetUp]
		public void Setup ()
		{
			tmpDir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (tmpDir);
			addedChanged = 0;
			deletedChanged = 0;
			monitor = new DirectoryMonitor (tmpDir);
			monitor.FileChangedEvent += (changeType, path) => {
				if (changeType == FileChangeType.Created) {
					addedChanged++;
				} else if (changeType == FileChangeType.Deleted) {
					deletedChanged++;
				}
			};
		}

		[TearDown]
		public void TearDown ()
		{
			try {
				Directory.Delete (tmpDir, true);
			} catch {
			}
		}

		[Test]
		public void TestFileChanged ()
		{
			string file1, file2;

			monitor.Start ();
			file1 = Path.Combine (tmpDir, "test1");
			file2 = Path.Combine (tmpDir, "test2");
			File.OpenWrite (file1).Close ();
			// Ugly as hell...
			System.Threading.Thread.Sleep (100);
			Assert.AreEqual (1, addedChanged);
			Assert.AreEqual (0, deletedChanged);
			File.Move (file1, file2);
			System.Threading.Thread.Sleep (100);
			Assert.AreEqual (2, addedChanged);
			Assert.AreEqual (1, deletedChanged);
			File.Delete (file2);
			System.Threading.Thread.Sleep (100);
			Assert.AreEqual (2, addedChanged);
			Assert.AreEqual (2, deletedChanged);
			monitor.Stop ();
		}

		[Test]
		public void TestStartStop ()
		{
			string file1 = Path.Combine (tmpDir, "test1");
			string file2 = Path.Combine (tmpDir, "test2");

			File.OpenWrite (file1).Close ();
			System.Threading.Thread.Sleep (100);
			Assert.AreEqual (0, addedChanged);
			Assert.AreEqual (0, deletedChanged);
			monitor.Start ();
			File.Move (file1, file2);
			System.Threading.Thread.Sleep (100);
			Assert.AreEqual (1, addedChanged);
			Assert.AreEqual (1, deletedChanged);
			monitor.Stop ();
			File.Delete (file2);
			System.Threading.Thread.Sleep (100);
			Assert.AreEqual (1, addedChanged);
			Assert.AreEqual (1, deletedChanged);
		}
	}
}

