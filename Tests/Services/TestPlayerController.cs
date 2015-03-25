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
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store.Playlists;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestPlayerController
	{
		Mock<IPlayer> playerMock;
		MediaFileSet mfs;
		PlayerController player;
		Time currentTime, streamLength;
		TimelineEvent evt;
		PlaylistImage plImage;
		Playlist playlist;
		double rate;

		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			playerMock = new Mock<IPlayer> ();
			playerMock.SetupAllProperties ();
			/* Mock properties without setter */
			playerMock.Setup (p => p.CurrentTime).Returns (() => currentTime);
			playerMock.Setup (p => p.StreamLength).Returns (() => streamLength);
			playerMock.Setup (p => p.Play ()).Raises (p => p.StateChange += null, true);
			playerMock.Setup (p => p.Pause ()).Raises (p => p.StateChange += null, false);

			var mtk = Mock.Of<IMultimediaToolkit> (m => m.GetPlayer () == playerMock.Object);
			Config.MultimediaToolkit = mtk;

			var ftk = new Mock<IGUIToolkit> ();
			ftk.Setup (m => m.Invoke (It.IsAny<EventHandler> ())).Callback<EventHandler> (e => e (null, null));
			Config.GUIToolkit = ftk.Object;

			Config.EventsBroker = new EventsBroker ();

			mfs = new MediaFileSet ();
			mfs.Add (new MediaFile { FilePath = "test1", VideoWidth = 320, VideoHeight = 240, Par = 1 });
			mfs.Add (new MediaFile { FilePath = "test2", VideoWidth = 320, VideoHeight = 240, Par = 1 });

			evt = new TimelineEvent { Start = new Time (100), Stop = new Time (200) };
			plImage = new PlaylistImage (Utils.LoadImageFromFile (), new Time (5));
			playlist = new Playlist ();
			playlist.Elements.Add (new PlaylistPlayElement (evt));
			playlist.Elements.Add (plImage);
		}

		[SetUp ()]
		public void Setup ()
		{
			playerMock.ResetCalls ();
			player = new PlayerController ();
		}

		void PreparePlayer ()
		{
			player.CamerasVisible = new List<int> { 0, 1 };
			player.WindowHandles = new List<IntPtr> { IntPtr.Zero, IntPtr.Zero };
			player.Ready ();
			player.Open (mfs);
			playerMock.Raise (p => p.ReadyToSeek += null);
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
		public void Dispose ()
		{
			player.Dispose ();
			playerMock.Verify (p => p.Dispose (), Times.Once ());
			Assert.IsTrue (player.IgnoreTicks);
			Assert.IsNull (player.FileSet);
		}

		[Test ()]
		public void Open ()
		{
			float par = 0;
			int parCount = 0, timeCount = 0;
			bool multimediaError = false;
			Time curTime = null, duration = null;

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
			currentTime = new Time (0);
			PreparePlayer ();
			playerMock.Verify (p => p.Open (mfs), Times.Once ());
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Never ());
			Assert.AreEqual (2, parCount);
			Assert.AreEqual (2, timeCount);
			Assert.AreEqual ((float)320 / 240, par);
			Assert.AreEqual (streamLength, duration);
			Assert.AreEqual (new Time (0), curTime);
		}

		[Test ()]
		public void TestPlayPause ()
		{
			bool loadSent = false;
			bool playing = false;
			FrameDrawing drawing = null;


			player.PlaybackStateChangedEvent += (p) => {
				playing = p;
			};
			player.LoadDrawingsEvent += (f) => {
				loadSent = true;
				drawing = f;
			};

			/* Start playing */
			player.Play ();
			Assert.IsTrue (loadSent);
			Assert.IsNull (drawing);
			playerMock.Verify (p => p.Play (), Times.Once ());
			Assert.IsTrue (player.Playing);
			Assert.IsTrue (playing);

			/* Go to pause */
			loadSent = false;
			player.Pause ();
			Assert.IsFalse (loadSent);
			Assert.IsNull (drawing);
			playerMock.Verify (p => p.Pause (), Times.Once ());
			Assert.IsFalse (player.Playing);
			Assert.IsFalse (playing);

			/* Check now with a still image loaded */
			playerMock.ResetCalls ();
			player.LoadPlaylistEvent (playlist, plImage);
			player.Play ();
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Pause (), Times.Once ());
			Assert.IsTrue (player.Playing);

			/* Go to pause */
			playerMock.ResetCalls ();
			player.Pause ();
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Pause (), Times.Never ());
			Assert.IsFalse (player.Playing);
		}

		[Test ()]
		public void TestTogglePlay ()
		{
			player.TogglePlay ();
			Assert.IsTrue (player.Playing);
			player.TogglePlay ();
			Assert.IsFalse (player.Playing);
		}

		[Test ()]
		public void TestSeek ()
		{
			int drawingsCount = 0;
			int timeChanged = 0;
			Time curTime = new Time (0);
			Time strLenght = new Time (0);

			player.TimeChangedEvent += (c, d, s) => {
				timeChanged++;
				curTime = c;
				strLenght = d;
			};
			player.LoadDrawingsEvent += (f) => drawingsCount++;
			player.Ready ();
			player.Open (mfs);
			Assert.AreEqual (0, timeChanged);

			/* Not ready, seek queued */
			currentTime = new Time (2000);
			player.Seek (currentTime, false, false, false);
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			Assert.AreEqual (1, drawingsCount);
			Assert.AreEqual (0, timeChanged);
			playerMock.ResetCalls ();

			/* Once ready the seek kicks in */
			currentTime = new Time (2000);
			playerMock.Raise (p => p.ReadyToSeek += null);
			/* ReadyToSeek emits TimeChanged */
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime, false, false), Times.Once ());
			Assert.AreEqual (1, drawingsCount);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (strLenght, streamLength);
			playerMock.ResetCalls ();

			/* Seek when player ready to seek */
			currentTime = new Time (4000);
			player.Seek (currentTime, true, true, false);
			playerMock.Verify (p => p.Seek (currentTime, true, true), Times.Once ());
			Assert.AreEqual (2, drawingsCount);
			Assert.AreEqual (2, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (strLenght, streamLength);
			playerMock.ResetCalls ();

			currentTime = new Time (5000);
			player.LoadPlaylistEvent (playlist, plImage);
			player.Seek (currentTime, true, true, false);
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());
			Assert.AreEqual (2, drawingsCount);
			playerMock.ResetCalls ();
		}

		[Test ()]
		public void TestSeekProportional ()
		{
			int seekPos;
			int timeChanged = 0;
			Time curTime = new Time (0);
			Time strLenght = new Time (0);

			streamLength = new Time { TotalSeconds = 5000 };
			player.TimeChangedEvent += (c, d, s) => {
				timeChanged++;
				curTime = c;
				strLenght = d;
			};
			PreparePlayer ();

			/* Seek without any segment loaded */
			seekPos = (int)(streamLength.MSeconds * 0.1); 
			currentTime = new Time (seekPos);
			player.Seek (0.1f);
			playerMock.Verify (p => p.Seek (new Time (seekPos), false, false), Times.Once ());
			Assert.IsTrue (timeChanged != 0);
			Assert.AreEqual (seekPos, curTime.MSeconds);
			Assert.AreEqual (strLenght.MSeconds, streamLength.MSeconds);

			/* Seek with a segment loaded */
			timeChanged = 0;
			seekPos = (int)(evt.Start.MSeconds + evt.Duration.MSeconds * 0.5);
			currentTime = new Time (seekPos);
			player.LoadEvent (mfs, evt, evt.Start, true);
			playerMock.ResetCalls ();
			player.Seek (0.5f);
			playerMock.Verify (p => p.Seek (new Time (seekPos), true, false), Times.Once ());
			Assert.IsTrue (timeChanged != 0);
			/* current time is now relative to the loaded segment's duration */
			Assert.AreEqual (evt.Duration * 0.5, curTime);
			Assert.AreEqual (evt.Duration, strLenght);
		}

		[Test ()]
		public void TestStepping ()
		{
			int timeChanged = 0;
			Time curTime = new Time (0);
			Time strLenght = new Time (0);

			currentTime = new Time { TotalSeconds = 2000 };
			streamLength = new Time { TotalSeconds = 5000 };
			PreparePlayer ();
			player.TimeChangedEvent += (c, d, s) => {
				timeChanged++;
				curTime = c;
				strLenght = d;
			};

			player.SeekToNextFrame ();
			playerMock.Verify (p => p.SeekToNextFrame (), Times.Once ());
			Assert.AreEqual (1, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (streamLength, strLenght);

			player.SeekToPreviousFrame ();
			playerMock.Verify (p => p.SeekToPreviousFrame (), Times.Once ());
			Assert.AreEqual (2, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (streamLength, strLenght);
		}

	}
}

