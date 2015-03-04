//
//  Copyright (C) 2015 Andoni Morales Alastruey
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
using LongoMatch.Core.Store;
using NUnit.Framework;
using LongoMatch.Core.Common;
using System.IO;

namespace Tests.Core.Store
{
	[TestFixture()]
	public class TestMediaFileSet
	{
		[Test()]
		public void TestSerialization ()
		{
			MediaFileSet mf = new MediaFileSet ();
			Utils.CheckSerialization (mf);
		}
		
		[Test()]
		public void TestPreview ()
		{
			MediaFileSet mf = new MediaFileSet ();
			Assert.IsNull (mf.Preview);
			mf.SetAngle (MediaFileAngle.Angle1,
			             new MediaFile {Preview = Utils.LoadImageFromFile ()});
			Assert.IsNotNull (mf.Preview);
		}
		
		[Test()]
		public void TestDuration ()
		{
			MediaFileSet mf = new MediaFileSet ();
			Assert.AreEqual (mf.Duration.MSeconds, 0);
			mf.SetAngle (MediaFileAngle.Angle1, new MediaFile {Duration = new Time (2000)});
			Assert.AreEqual (mf.Duration.MSeconds, 2000); 
			mf.SetAngle (MediaFileAngle.Angle1, new MediaFile {Duration = new Time (2001)});
			Assert.AreEqual (mf.Duration.MSeconds, 2001); 
		}
		
		[Test()]
		public void TestCheckFiles ()
		{
			string path = Path.GetTempFileName ();
			MediaFileSet mf = new MediaFileSet ();
			Assert.IsFalse (mf.CheckFiles());
			mf.SetAngle (MediaFileAngle.Angle1, new MediaFile {FilePath = path});
			try {
				Assert.IsTrue (mf.CheckFiles ());
			} finally {
				File.Delete (path);
			}
		}
		
		[Test()]
		public void TestGetSetAngles ()
		{
			MediaFileSet mfs = new MediaFileSet ();
			MediaFile mf = new MediaFile ();
			Assert.IsNull (mfs.GetAngle (MediaFileAngle.Angle1));
			mfs.SetAngle (MediaFileAngle.Angle1, mf);
			Assert.AreEqual (mfs.GetAngle (MediaFileAngle.Angle1), mf);
			mfs.SetAngle (MediaFileAngle.Angle2, mf);
			Assert.AreEqual (mfs.GetAngle (MediaFileAngle.Angle2), mf);
		}
	}
}

