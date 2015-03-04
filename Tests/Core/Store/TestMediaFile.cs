//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using NUnit.Framework;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;

namespace Tests.Core.Store
{
	[TestFixture()]
	public class TestMediaFile
	{
		[Test()]
		public void TestSerialization ()
		{
			MediaFile mf = new MediaFile ("path", 34000, 25, true, true, "mp4", "h264",
			                              "aac", 320, 240, 1.3, null);
			Utils.CheckSerialization (mf);
			
			MediaFile newmf = Utils.SerializeDeserialize (mf);
			Assert.AreEqual (mf.FilePath, newmf.FilePath);
			Assert.AreEqual (mf.Duration, newmf.Duration);
			Assert.AreEqual (mf.Fps, newmf.Fps);
			Assert.AreEqual (mf.HasAudio, newmf.HasAudio);
			Assert.AreEqual (mf.HasVideo, newmf.HasVideo);
			Assert.AreEqual (mf.Container, newmf.Container);
			Assert.AreEqual (mf.VideoCodec, newmf.VideoCodec);
			Assert.AreEqual (mf.AudioCodec, newmf.AudioCodec);
			Assert.AreEqual (mf.VideoWidth, newmf.VideoWidth);
			Assert.AreEqual (mf.VideoHeight, newmf.VideoHeight);
			Assert.AreEqual (mf.Par, newmf.Par);
			Assert.AreEqual (mf.Offset, new Time (0));
				
		}
		
		[Test()]
		public void TestShortDescription ()
		{
			MediaFile mf = new MediaFile {VideoWidth = 320, VideoHeight = 240, Fps = 25};
			Assert.AreEqual (mf.ShortDescription, "320x240@25fps");
		}
		
		[Test()]
		public void TestExists ()
		{
			string path = Path.GetTempFileName ();
			MediaFile mf = new MediaFile ();
			try {
				Assert.IsFalse (mf.Exists ());
				mf.FilePath = path;
				Assert.IsTrue (mf.Exists ());
			} finally {
				File.Delete (path);
			}
		}
	}
}

