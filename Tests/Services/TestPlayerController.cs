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
		Mock<IViewPort> viewPortMock;
		Mock<IMultimediaToolkit> mtkMock;
		MediaFileSet mfs;
		PlayerController player;
		Time currentTime, streamLength;
		TimelineEvent evt;
		PlaylistImage plImage;
		Playlist playlist;

		[TestFixtureSetUp ()]
		public void FixtureSetup ()
		{
			playerMock = new Mock<IPlayer> ();
			playerMock.SetupAllProperties ();
			/* Mock properties without setter */
			playerMock.Setup (p => p.CurrentTime).Returns (() => currentTime);
			playerMock.Setup (p => p.StreamLength).Returns (() => streamLength);
			playerMock.Setup (p => p.Play ()).Raises (p => p.StateChange += null, this, true);
			playerMock.Setup (p => p.Pause ()).Raises (p => p.StateChange += null, this, false);

			mtkMock = new Mock<IMultimediaToolkit> ();
			mtkMock.Setup (m => m.GetPlayer ()).Returns (playerMock.Object);
			mtkMock.Setup (m => m.GetMultiPlayer ()).Throws (new Exception ());
			Config.MultimediaToolkit = mtkMock.Object;

			var ftk = new Mock<IGUIToolkit> ();
			ftk.Setup (m => m.Invoke (It.IsAny<EventHandler> ())).Callback<EventHandler> (e => e (null, null));
			Config.GUIToolkit = ftk.Object;

			Config.EventsBroker = new EventsBroker ();

			mfs = new MediaFileSet ();
			mfs.Add (new MediaFile { FilePath = "test1", VideoWidth = 320, VideoHeight = 240, Par = 1 });
			mfs.Add (new MediaFile { FilePath = "test2", VideoWidth = 320, VideoHeight = 240, Par = 1 });

		}

		[SetUp ()]
		public void Setup ()
		{
			evt = new TimelineEvent { Start = new Time (100), Stop = new Time (200),
				CamerasConfig = new List<CameraConfig> { new CameraConfig (0) }
			};
			plImage = new PlaylistImage (Utils.LoadImageFromFile (), new Time (5000));
			playlist = new Playlist ();
			playlist.Elements.Add (new PlaylistPlayElement (evt));
			playlist.Elements.Add (plImage);
			currentTime = new Time (0);
			playerMock.ResetCalls ();
			player = new PlayerController ();
			playlist.SetActive (playlist.Elements [0]);
		}

		[TearDown ()]
		public void TearDown ()
		{
			player.Stop ();
			player.Dispose ();
		}

		void PreparePlayer (bool readyToSeek = true)
		{
			player.CamerasConfig = new List<CameraConfig> { new CameraConfig (0), new CameraConfig (1) };
			viewPortMock = new Mock <IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			player.Ready ();
			player.Open (mfs);
			if (readyToSeek) {
				playerMock.Raise (p => p.ReadyToSeek += null, this);
			}
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
		public void TestDispose ()
		{
			player.Dispose ();
			playerMock.Verify (p => p.Dispose (), Times.Once ());
			Assert.IsTrue (player.IgnoreTicks);
			Assert.IsNull (player.FileSet);
		}

		[Test ()]
		public void TestOpen ()
		{
			int timeCount = 0;
			bool multimediaError = false;
			Time curTime = null, duration = null;
			MediaFileSet fileSet = null;

			player.TimeChangedEvent += (c, d, seekable) => {
				curTime = c;
				duration = d;
				timeCount++;
			};
			player.MediaFileSetLoadedEvent += (fileset, cameras) => {
				fileSet = fileset;
			};

			/* Open but view is not ready */
			player.Open (mfs);
			Assert.AreEqual (mfs, player.FileSet);
			playerMock.Verify (p => p.Open (mfs [0]), Times.Never ());
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Never ());

			/* Open with an invalid camera configuration */
			Config.EventsBroker.MultimediaError += (o, message) => {
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
			playerMock.Verify (p => p.Open (mfs [0]), Times.Once ());
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Once ());
			Assert.AreEqual (2, timeCount);
			Assert.AreEqual ((float)320 / 240, viewPortMock.Object.Ratio);
			Assert.AreEqual (streamLength, duration);
			Assert.AreEqual (new Time (0), curTime);
			Assert.AreEqual (fileSet, mfs);
		}

		[Test ()]
		public void TestPlayPause ()
		{
			bool loadSent = false;
			bool playing = false;
			FrameDrawing drawing = null;


			player.PlaybackStateChangedEvent += (o, p) => {
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
			player.Ready ();
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
			playerMock.Raise (p => p.ReadyToSeek += null, this);
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
			player.Seek (0.1f);
			player.Seek (0.5f);
			// Seeks for loaded events are throtled by a timer.
			System.Threading.Thread.Sleep (100);
			// Check we got called only once
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), true, false), Times.Once ());
			// And with the last value
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
			int loadDrawingsChanged = 0;
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
			player.LoadDrawingsEvent += (f) => {
				if (f == null) {
					loadDrawingsChanged++;
				}
			};

			/* Without a segment loaded */

			player.SeekToNextFrame ();
			playerMock.Verify (p => p.SeekToNextFrame (), Times.Once ());
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (streamLength, strLenght);

			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.SeekToPreviousFrame ();
			playerMock.Verify (p => p.SeekToPreviousFrame (), Times.Once ());
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			Assert.AreEqual (currentTime, curTime);
			Assert.AreEqual (streamLength, strLenght);

			playerMock.ResetCalls ();
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.StepForward ();
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime + player.Step, true, false), Times.Once ());

			playerMock.ResetCalls ();
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.StepBackward ();
			Assert.AreEqual (1, loadDrawingsChanged);
			Assert.AreEqual (1, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime - player.Step, true, false), Times.Once ());

			/* Now with an image loaded */
			playerMock.ResetCalls ();
			loadDrawingsChanged = 0;
			timeChanged = 0;
			player.LoadPlaylistEvent (playlist, plImage);
			player.SeekToNextFrame ();
			playerMock.Verify (p => p.SeekToNextFrame (), Times.Never ());
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);

			player.SeekToPreviousFrame ();
			playerMock.Verify (p => p.SeekToPreviousFrame (), Times.Never ());
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);

			player.StepForward ();
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime + player.Step, true, false), Times.Never ());

			player.StepBackward ();
			Assert.AreEqual (0, loadDrawingsChanged);
			Assert.AreEqual (0, timeChanged);
			playerMock.Verify (p => p.Seek (currentTime - player.Step, true, false), Times.Never ());
		}

		[Test ()]
		public void TestChangeFramerate ()
		{
			float rate = 1;

			playerMock.Object.Rate = 1;
			player.PlaybackRateChangedEvent += (r) => rate = r;

			for (int i = 1; i < 5; i++) {
				player.FramerateUp ();
				playerMock.VerifySet (p => p.Rate = 1 + i);
				Assert.AreEqual (1 + i, rate);
			}
			/* Max is 5 */
			Assert.AreEqual (5, player.Rate);
			player.FramerateUp ();
			playerMock.VerifySet (p => p.Rate = 5);
			Assert.AreEqual (5, rate);

			player.Rate = 1;
			for (int i = 1; i < 25; i++) {
				player.FramerateDown ();
				double _rate = player.Rate;
				playerMock.VerifySet (p => p.Rate = _rate);
				Assert.AreEqual (1 - (double)i / 25, rate, 0.01);
			}

			/* Min is 1 / 30 */
			Assert.AreEqual ((double)1 / 25, player.Rate, 0.01);
			player.FramerateDown ();
			Assert.AreEqual ((double)1 / 25, player.Rate, 0.01);
		}

		[Test ()]
		public void TestNext ()
		{
			int nextSent = 0;
			PreparePlayer ();
			Config.EventsBroker.NextPlaylistElementEvent += (p) => nextSent++;

			player.Next ();
			Assert.AreEqual (0, nextSent);

			player.LoadPlaylistEvent (playlist, plImage);
			player.Next ();
			Assert.AreEqual (1, nextSent);

			playlist.Next ();
			Assert.IsFalse (playlist.HasNext ());
			player.Next ();
			Assert.AreEqual (1, nextSent);
		}

		[Test ()]
		public void TestPrevious ()
		{
			int prevSent = 0;
			currentTime = new Time (4000);
			PreparePlayer ();
			Config.EventsBroker.PreviousPlaylistElementEvent += (p) => prevSent++;

			player.Previous ();
			playerMock.Verify (p => p.Seek (new Time (0), true, false));
			Assert.AreEqual (0, prevSent);
	
			player.LoadEvent (mfs, evt, evt.Start, false);
			playerMock.ResetCalls ();
			player.Previous ();
			playerMock.Verify (p => p.Seek (evt.Start, true, false));
			Assert.AreEqual (0, prevSent);

			player.LoadPlaylistEvent (playlist, plImage);
			playerMock.ResetCalls ();
			player.Previous ();
			Assert.AreEqual (0, prevSent);
			playlist.Next ();
			player.Previous ();
			Assert.AreEqual (1, prevSent);
		}

		[Test ()]
		public void TestEOS ()
		{
			PreparePlayer ();
			playerMock.ResetCalls ();
			playerMock.Raise (p => p.Eos += null, this);
			playerMock.Verify (p => p.Seek (new Time (0), true, false), Times.Once ());
			playerMock.Verify (p => p.Pause (), Times.Once ());
			playerMock.ResetCalls ();

			TimelineEvent evtLocal = new TimelineEvent { Start = new Time (100), Stop = new Time (20000),
				CamerasConfig = new List<CameraConfig> { new CameraConfig (0) }
			};
			player.LoadEvent (mfs, evtLocal, evtLocal.Start, true);
			playerMock.ResetCalls ();
			playerMock.Raise (p => p.Eos += null, this);
			playerMock.Verify (p => p.Seek (evtLocal.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Pause (), Times.Once ());
		}

		[Test ()]
		public void TestError ()
		{
			string msg = null;

			Config.EventsBroker.MultimediaError += (o, message) => {
				msg = message;
			};
			playerMock.Raise (p => p.Error += null, this, "error");
			Assert.AreEqual ("error", msg);
		}

		[Test ()]
		public void TestUnloadEvent ()
		{
			int elementLoaded = 0;
			PreparePlayer ();
			player.ElementLoadedEvent += (element, hasNext) => {
				if (element == null) {
					elementLoaded--;
				} else {
					elementLoaded++;
				}
			};
			// Load
			player.LoadEvent (mfs, evt, evt.Start, true);
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (evt.CamerasConfig, player.CamerasConfig);
			// Unload
			player.UnloadCurrentEvent ();
			Assert.AreEqual (0, elementLoaded);
			// Check that cameras have been restored
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (1) }, player.CamerasConfig);

			/* Change again the cameras visible */
			player.CamerasConfig = new List<CameraConfig>  { new CameraConfig (2), new CameraConfig (3) };
			Assert.AreEqual (evt.CamerasConfig, new List <CameraConfig> { new CameraConfig (0) });
			player.LoadEvent (mfs, evt, evt.Start, true);
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (evt.CamerasConfig, player.CamerasConfig);
			/* And unload */
			player.UnloadCurrentEvent ();
			Assert.AreEqual (0, elementLoaded);
			// Check that cameras have been restored
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (2), new CameraConfig (3) }, player.CamerasConfig);
		}

		[Test ()]
		public void TestCamerasVisibleValidation ()
		{
			// Create an event referencing unknown MediaFiles in the set.
			TimelineEvent evt2 = new TimelineEvent { Start = new Time (150), Stop = new Time (200),
				CamerasConfig = new List<CameraConfig> {
					new CameraConfig (0),
					new CameraConfig (1),
					new CameraConfig (4),
					new CameraConfig (6)
				}
			};

			player.CamerasConfig = new List<CameraConfig> { new CameraConfig (1), new CameraConfig (0) };
			viewPortMock = new Mock <IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			player.Ready ();
			player.LoadEvent (mfs, evt2, evt2.Start, true);
			// Only valid cameras should be visible although no fileset was opened.
			Assert.AreEqual (2, player.CamerasConfig.Count);
			Assert.AreEqual (0, player.CamerasConfig [0].Index);
			Assert.AreEqual (1, player.CamerasConfig [1].Index);
			// Again now that the fileset is opened
			player.LoadEvent (mfs, evt2, evt2.Start, true);
			// Only valid cameras should be visible
			Assert.AreEqual (2, player.CamerasConfig.Count);
			Assert.AreEqual (0, player.CamerasConfig [0].Index);
			Assert.AreEqual (1, player.CamerasConfig [1].Index);
		}

		[Test ()]
		public void TestLoadEvent ()
		{
			int elementLoaded = 0;
			int prepareView = 0;

			player.ElementLoadedEvent += (element, hasNext) => {
				if (element != null) {
					elementLoaded++;
				}
			};
			player.PrepareViewEvent += () => prepareView++;

			/* Not ready to seek */
			player.CamerasConfig = new List<CameraConfig> { new CameraConfig (0), new CameraConfig (1) };
			viewPortMock = new Mock <IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			Assert.AreEqual (0, prepareView);

			/* Loading an event with the player not ready should trigger the
			 * PrepareViewEvent and wait until it's ready */
			player.LoadEvent (mfs, evt, evt.Start, true);
			Assert.AreEqual (1, prepareView);
			Assert.IsNull (player.FileSet);

			player.Ready ();
			Assert.AreEqual (1, elementLoaded);
			Assert.AreEqual (mfs, player.FileSet);

			player.LoadEvent (mfs, evt, evt.Start, true);
			Assert.AreEqual (mfs, player.FileSet);
			Assert.IsFalse (player.Playing);
			Assert.AreEqual (2, elementLoaded);
			playerMock.Verify (p => p.Seek (It.IsAny<Time> (), It.IsAny<bool> (), It.IsAny<bool> ()), Times.Never ());


			/* Ready to seek */
			currentTime = evt.Start;
			playerMock.Raise (p => p.ReadyToSeek += null, this);
			Assert.IsTrue (player.Playing);
			playerMock.Verify (p => p.Open (mfs [0]));
			playerMock.Verify (p => p.Seek (evt.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (), Times.Once ());
			playerMock.VerifySet (p => p.Rate = 1);
			Assert.AreEqual (2, elementLoaded);
			elementLoaded = 0;
			playerMock.ResetCalls ();

			/* Open with a new MediaFileSet and also check seekTime and playing values*/
			MediaFileSet nfs = Cloner.Clone (mfs);
			player.LoadEvent (nfs, evt, evt.Stop, false);
			Assert.AreEqual (1, elementLoaded);
			elementLoaded = 0;
			Assert.AreEqual (nfs, player.FileSet);
			playerMock.Verify (p => p.Open (nfs [0]));
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.Verify (p => p.Pause (), Times.Once ());
			playerMock.VerifySet (p => p.Rate = 1);
			playerMock.Raise (p => p.ReadyToSeek += null, this);
			playerMock.Verify (p => p.Seek (evt.Stop, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.ResetCalls ();

			/* Open another event with the same MediaFileSet and already ready to seek
			 * and check the cameras layout and visibility is respected */
			TimelineEvent evt2 = new TimelineEvent { Start = new Time (400), Stop = new Time (50000),
				CamerasConfig = new List<CameraConfig> { new CameraConfig (1), new CameraConfig (0) },
				CamerasLayout = "test"
			};
			player.LoadEvent (nfs, evt2, evt2.Start, true);
			Assert.AreEqual (1, elementLoaded);
			elementLoaded = 0;
			playerMock.Verify (p => p.Open (nfs [0]), Times.Never ());
			playerMock.Verify (p => p.Seek (evt2.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (), Times.Once ());
			playerMock.VerifySet (p => p.Rate = 1);
			Assert.AreEqual (evt2.CamerasConfig, player.CamerasConfig);
			Assert.AreEqual (evt2.CamerasLayout, player.CamerasLayout);
			playerMock.ResetCalls ();

		}

		[Test ()]
		public void TestLoadPlaylistEvent ()
		{
			int elementLoaded = 0;
			int prepareView = 0;
			MediaFileSet nfs;
			PlaylistPlayElement el1;

			player.ElementLoadedEvent += (element, hasNext) => {
				if (element != null) {
					elementLoaded++;
				}
			};
			player.PrepareViewEvent += () => prepareView++;

			/* Not ready to seek */
			player.CamerasConfig = new List<CameraConfig> { new CameraConfig (0), new CameraConfig (1) };
			viewPortMock = new Mock <IViewPort> ();
			viewPortMock.SetupAllProperties ();
			player.ViewPorts = new List<IViewPort> { viewPortMock.Object, viewPortMock.Object };
			Assert.AreEqual (0, prepareView);

			/* Load playlist timeline event element */
			nfs = mfs.Clone ();
			el1 = playlist.Elements [0] as PlaylistPlayElement;
			el1.FileSet = nfs;
			currentTime = el1.Play.Start;
			player.LoadPlaylistEvent (playlist, el1);
			Assert.AreEqual (0, elementLoaded);
			Assert.AreEqual (1, prepareView);

			player.Ready ();
			Assert.AreEqual (1, elementLoaded);
			elementLoaded = 0;
			Assert.AreEqual (el1.CamerasConfig, player.CamerasConfig);
			Assert.AreEqual (el1.CamerasLayout, player.CamerasLayout);
			playerMock.Verify (p => p.Open (nfs [0]), Times.Once ());
			playerMock.Verify (p => p.Seek (el1.Play.Start, true, false), Times.Never ());
			playerMock.Verify (p => p.Play (), Times.Never ());
			playerMock.VerifySet (p => p.Rate = 1);
			playerMock.Raise (p => p.ReadyToSeek += null, this);
			playerMock.Verify (p => p.Seek (el1.Play.Start, true, false), Times.Once ());
			playerMock.Verify (p => p.Play (), Times.Once ());

			/* Load still image */
			player.LoadPlaylistEvent (playlist, plImage);
			playerMock.ResetCalls ();
			Assert.IsTrue (player.Playing);
			player.Pause ();
			playerMock.Verify (p => p.Pause (), Times.Never ());
			Assert.IsFalse (player.Playing);
			player.Play ();
			playerMock.Verify (p => p.Play (), Times.Never ());
			Assert.IsTrue (player.Playing);

			/* Load drawings */
			PlaylistDrawing dr = new PlaylistDrawing (new FrameDrawing ());
			player.LoadPlaylistEvent (playlist, dr);
			playerMock.ResetCalls ();
			Assert.IsTrue (player.Playing);
			player.Pause ();
			playerMock.Verify (p => p.Pause (), Times.Never ());
			Assert.IsFalse (player.Playing);
			player.Play ();
			playerMock.Verify (p => p.Play (), Times.Never ());
			Assert.IsTrue (player.Playing);

			/* Load video */
			PlaylistVideo vid = new PlaylistVideo (mfs [0]);
			player.LoadPlaylistEvent (playlist, vid);
			Assert.AreNotEqual (mfs, player.FileSet);
			Assert.IsTrue (player.Playing);
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0) }, player.CamerasConfig);
		}

		[Test ()]
		public void TestStopTimes ()
		{
			int nextLoaded = 0;

			PreparePlayer ();

			Config.EventsBroker.NextPlaylistElementEvent += (pl) => {
				nextLoaded++;
			};

			/* Check the player is stopped when we pass the event stop time */
			currentTime = evt.Start;
			player.LoadEvent (mfs, evt, evt.Start, true);
			Assert.IsTrue (player.Playing);
			currentTime = evt.Stop + new Time (1000);
			player.Seek (currentTime, true, false);
			Assert.IsFalse (player.Playing);
			Assert.AreEqual (1, nextLoaded);

			/* Check the player is stopped when we pass the image stop time */
			currentTime = new Time (0);
			player.LoadPlaylistEvent (playlist, plImage);
			Assert.IsTrue (player.Playing);
			currentTime = plImage.Duration + 1000;
			player.Seek (currentTime, true, false);
			Assert.IsFalse (player.Playing);
			Assert.AreEqual (2, nextLoaded);
		}

		[Test ()]
		public void TestEventDrawings ()
		{
			FrameDrawing dr, drSent = null;

			player.LoadDrawingsEvent += (frameDrawing) => {
				drSent = frameDrawing;
			};

			dr = new FrameDrawing { Render = evt.Start + 50,
				CameraConfig = new CameraConfig (0),
			};
			currentTime = evt.Start;
			PreparePlayer ();

			/* Checks drawings are loaded when the clock reaches the render time */
			evt.Drawings.Add (dr);
			player.LoadEvent (mfs, evt, evt.Start, true);
			Assert.IsTrue (player.Playing);
			currentTime = dr.Render;
			player.Seek (currentTime, true, false);
			Assert.IsFalse (player.Playing);
			Assert.AreEqual (dr, drSent); 
			player.Play ();
			Assert.IsNull (drSent); 

			/* Check only drawings for the first camera are loaded */
			dr.CameraConfig = new CameraConfig (1);
			currentTime = evt.Start;
			player.LoadEvent (mfs, evt, evt.Start, true);
			Assert.IsTrue (player.Playing);
			currentTime = dr.Render;
			player.Seek (currentTime, true, false);
			Assert.IsTrue (player.Playing);
			Assert.IsNull (drSent); 
		}

		[Test ()]
		public void TestMultiplayerCamerasConfig ()
		{
			TimelineEvent evt1;
			List<CameraConfig> cams1, cams2;
			Mock<IMultiPlayer> multiplayerMock = new Mock<IMultiPlayer> ();

			mtkMock.Setup (m => m.GetMultiPlayer ()).Returns (multiplayerMock.Object);
			player = new PlayerController (true);
			PreparePlayer ();

			/* Only called internally in the openning */
			cams1 = new List<CameraConfig> { new CameraConfig (0), new CameraConfig (1) };
			cams2 = new List<CameraConfig> { new CameraConfig (1), new CameraConfig (0) };
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Never ());
			Assert.AreEqual (cams1, player.CamerasConfig);

			player.CamerasConfig = cams2;
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Now load an event */
			evt1 = new TimelineEvent { Start = new Time (100), Stop = new Time (200),
				CamerasConfig = new List<CameraConfig> { new CameraConfig (1), new CameraConfig (1) }
			};
			player.LoadEvent (mfs, evt1, evt1.Start, true);
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (evt1.CamerasConfig, player.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Change event cams config */
			player.CamerasConfig = new List<CameraConfig> { new CameraConfig (0), new CameraConfig (0) };
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (0) }, evt1.CamerasConfig);
			Assert.AreEqual (player.CamerasConfig, evt1.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Unload and check the original cams config is set back*/
			player.UnloadCurrentEvent ();
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (0) }, evt1.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* And changing the config does not affects the unloaded event */
			player.CamerasConfig = cams1;
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0), new CameraConfig (0) }, evt1.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Now load a playlist video */
			PlaylistVideo plv = new PlaylistVideo (mfs [0]);
			player.LoadPlaylistEvent (playlist, plv);
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Never ());
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0) }, player.CamerasConfig);
			multiplayerMock.ResetCalls ();
			player.UnloadCurrentEvent ();
			/* Called by Open internally () */
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Never ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			multiplayerMock.ResetCalls ();

			/* Now load a playlist event and make sure its config is loaded
			 * and not the event's one */
			PlaylistPlayElement ple = new PlaylistPlayElement (evt, mfs);
			ple.CamerasConfig = cams2;
			player.LoadPlaylistEvent (playlist, ple);
			multiplayerMock.Verify (p => p.ApplyCamerasConfig (), Times.Once ());
			Assert.AreEqual (cams2, player.CamerasConfig);
			multiplayerMock.ResetCalls ();
		}

		[Test ()]
		public void TestROICamerasConfig ()
		{
			TimelineEvent evt1;
			List<CameraConfig> cams;
			Mock<IMultiPlayer> multiplayerMock = new Mock<IMultiPlayer> ();

			mtkMock.Setup (m => m.GetMultiPlayer ()).Returns (multiplayerMock.Object);
			player = new PlayerController (true);
			PreparePlayer ();

			/* ROI should be empty */
			Assert.AreEqual (new Area (), player.CamerasConfig [0].RegionOfInterest);

			/* Modify ROI */
			cams = player.CamerasConfig;
			cams [0].RegionOfInterest = new Area (10, 10, 20, 20);
			/* And set */
			player.ApplyROI (cams [0]);

			/* Now create an event with current camera config */
			evt1 = new TimelineEvent { Start = new Time (100), Stop = new Time (200),
				CamerasConfig = player.CamerasConfig
			};
			/* Check that ROI was copied in event */
			Assert.AreEqual (new Area (10, 10, 20, 20), evt1.CamerasConfig [0].RegionOfInterest);

			/* Change ROI again */
			cams [0].RegionOfInterest = new Area (20, 20, 40, 40);
			player.ApplyROI (cams [0]);

			/* Check event was not impacted */
			Assert.AreEqual (new Area (10, 10, 20, 20), evt1.CamerasConfig [0].RegionOfInterest);

			/* And load event */
			player.LoadEvent (mfs, evt1, evt1.Start, true);
			Assert.AreEqual (new Area (10, 10, 20, 20), player.CamerasConfig [0].RegionOfInterest);

			/* Unload and check the original cams config is set back*/
			player.UnloadCurrentEvent ();
			Assert.AreEqual (new Area (20, 20, 40, 40), player.CamerasConfig [0].RegionOfInterest);
			/* check the event was not impacted */
			Assert.AreEqual (new Area (10, 10, 20, 20), evt1.CamerasConfig [0].RegionOfInterest);
		}
	}
}

