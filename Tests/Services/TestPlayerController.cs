//
//  Copyright (C) 2015 Fluendo S.A.
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
using LongoMatch.Core.Interfaces.Multimedia;
using Moq;
using NUnit.Framework;
using LongoMatch.Services;
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using System.Collections.Generic;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestPlayerController
	{
		Mock<IPlayer> playerMock;
		PlayerController player;
		Time currentTime, streamLength;
		double rate;

		[SetUp ()]
		public void Setup ()
		{
			playerMock = new Mock<IPlayer> ();
			playerMock.SetupAllProperties ();
			/* Mock properties without setter */
			playerMock.Setup (p => p.CurrentTime).Returns (() => currentTime);
			playerMock.Setup (p => p.StreamLength).Returns (() => streamLength);

			var mtk = Mock.Of<IMultimediaToolkit> (m => m.GetPlayer () == playerMock.Object);
			Config.MultimediaToolkit = mtk;

			Config.EventsBroker = new EventsBroker ();

			player = new PlayerController ();
		}

		[Test ()]
		public void TestPropertiesProxy ()
		{
			player.Volume = 10;
			Assert.AreEqual (10, player.Volume);

			currentTime = new Time (20);
			Assert.AreEqual (20, player.CurrentTime.MSeconds);

			streamLength = new Time (40);
			Assert.AreEqual (40, player.StreamLength.MSeconds);
		}

		[Test ()]
		public void TestSetRate ()
		{
			float r = 0;

			player.PlaybackRateChangedEvent += (rate) => r = 10;
			player.Rate = 1;
			/* Event is not raised */
			Assert.AreEqual (0, r);
			Assert.AreEqual (1, player.Rate);
		}

		[Test ()]
		public void TestCurrentMiniatureFrame ()
		{
			var img = player.CurrentMiniatureFrame;
			playerMock.Verify (p => p.GetCurrentFrame (Constants.MAX_THUMBNAIL_SIZE,
				Constants.MAX_THUMBNAIL_SIZE));
		}

		[Test ()]
		public void TestCurrentFrame ()
		{
			var img = player.CurrentFrame;
			playerMock.Verify (p => p.GetCurrentFrame (-1, -1));
		}

		[Test ()]
		public void TestOpened ()
		{
			Assert.IsFalse (player.Opened);
			player.Open (new MediaFileSet ());
			Assert.IsTrue (player.Opened);
		}

		[Test ()]
		public void Open ()
		{
			float par = 0;
			int parCount = 0, timeCount = 0;
			bool multimediaError = false;
			Time curTime = null, duration = null;
			MediaFileSet mfs = new MediaFileSet ();
			mfs.Add (new MediaFile { FilePath = "test1", VideoWidth = 320, VideoHeight = 240, Par = 1 });
			mfs.Add (new MediaFile { FilePath = "test2", VideoWidth = 320, VideoHeight = 240, Par = 1 });

			player.PARChangedEvent += (w, p) => {
				par = p;
				parCount++;
			};
			player.TimeChangedEvent += (c, d, seekable) => {
				curTime = c;
				duration = d;
				timeCount++;
			};

			/* Open but view is not ready */
			player.Open (mfs);
			Assert.AreEqual (mfs, player.FileSet);
			playerMock.Verify (p => p.Open (mfs), Times.Never ());
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Never ());

			/* Open with an invalid camera configuration */
			Config.EventsBroker.MultimediaError += (message) => {
				multimediaError = true;
			};
			player.Ready ();
			player.Open (mfs);
			Assert.IsTrue (multimediaError);
			Assert.IsNull (player.FileSet);
			Assert.IsFalse (player.Opened);

			/* Open with the view ready */
			streamLength = new Time { TotalSeconds = 5000 };
			player.CamerasVisible = new List<int> { 0, 1 };
			player.WindowHandles = new List<IntPtr> { IntPtr.Zero, IntPtr.Zero };
			player.Ready ();
			player.Open (mfs);
			playerMock.Verify (p => p.Open (mfs), Times.Once ());
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Never ());
			Assert.AreEqual (2, parCount);
			Assert.AreEqual (1, timeCount);
			Assert.AreEqual ((float)320 / 240, par);
			Assert.AreEqual (streamLength, duration);
			Assert.AreEqual (new Time (0), curTime);
		}
	}
}

