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
using System.Linq;
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
			mf.Add (new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset"));
			mf.Add (new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
				"aac", 320, 240, 1.3, null, "Test asset 2"));
			Utils.CheckSerialization (mf);
		}

		[Test()]
		public void TestMigration ()
		{
			String old_json = @"""FileSet"": { 
							      ""$id"": ""88"",
							      ""$type"": ""LongoMatch.Core.Store.MediaFileSet, LongoMatch.Core"",
							      ""Files"": { 
							        ""$id"": ""1"",
							        ""$type"": ""System.Collections.Generic.Dictionary`2[[LongoMatch.Core.Common.MediaFileAngle, LongoMatch.Core],[LongoMatch.Core.Store.MediaFile, LongoMatch.Core]], mscorlib"",
							        ""Angle1"": { ""$id"": ""2"", ""$type"": ""LongoMatch.Core.Store.MediaFile, LongoMatch.Core"", ""FilePath"": ""test.mp4"", ""Duration"": null, ""HasVideo"": false, ""HasAudio"": false, ""Container"": null, ""VideoCodec"": null, ""AudioCodec"": null, ""VideoWidth"": 640, ""VideoHeight"": 480, ""Fps"": 25, ""Par"": 1.0, ""Preview"": null, ""Offset"": 0 },
							        ""Angle2"": { ""$id"": ""3"", ""$type"": ""LongoMatch.Core.Store.MediaFile, LongoMatch.Core"", ""FilePath"": ""test2.mp4"", ""Duration"": null, ""HasVideo"": false, ""HasAudio"": false, ""Container"": null, ""VideoCodec"": null, ""AudioCodec"": null, ""VideoWidth"": 640, ""VideoHeight"": 480, ""Fps"": 25, ""Par"": 1.0, ""Preview"": null, ""Offset"": 0 },
							        ""Angle3"": null,
							        ""Angle4"": null
							      }
								}";
			MemoryStream stream = new MemoryStream();
			StreamWriter writer = new StreamWriter(stream);
			writer.Write(old_json);
			writer.Flush();
			stream.Position = 0;

			// Deserialize and check the FileSet
			var newobj = Serializer.Load<MediaFileSet> (stream, SerializationType.Json);

			Assert.AreEqual (2, newobj.Count);

			MediaFile mf = newobj.First ();

			Assert.AreEqual ("test.mp4", mf.FilePath);
			Assert.AreEqual ("Main camera angle", mf.Name);

			mf = newobj [1];

			Assert.AreEqual ("test2.mp4", mf.FilePath);
			Assert.AreEqual ("Angle 1", mf.Name);
		}
		
		[Test()]
		public void TestPreview ()
		{
			MediaFileSet mf = new MediaFileSet ();
			Assert.IsNull (mf.Preview);
			mf.Add (new MediaFile {Preview = Utils.LoadImageFromFile (), Name = "Test asset"});
			Assert.IsNotNull (mf.Preview);
		}
		
		[Test()]
		public void TestDuration ()
		{
			MediaFileSet mf = new MediaFileSet ();
			Assert.AreEqual (mf.Duration.MSeconds, 0);
			mf.Add (new MediaFile {Duration = new Time (2000), Name = "Test asset"});
			Assert.AreEqual (mf.Duration.MSeconds, 2000); 
			mf.Replace ("Test asset", new MediaFile {Duration = new Time (2001), Name = "Test asset 2"});
			Assert.AreEqual (mf.Duration.MSeconds, 2001);
		}
		
		[Test()]
		public void TestCheckFiles ()
		{
			string path = Path.GetTempFileName ();
			MediaFileSet mf = new MediaFileSet ();
			Assert.IsFalse (mf.CheckFiles());
			mf.Add (new MediaFile {FilePath = path, Name = "Test asset"});
			try {
				Assert.IsTrue (mf.CheckFiles ());
			} finally {
				File.Delete (path);
			}
		}
	}
}

