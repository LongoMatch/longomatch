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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Video.Common;
using LongoMatch.Video.Utils;
using Timer = System.Threading.Timer;

namespace LongoMatch.Services.Services
{
	public class PlayerController: IPlayerController
	{
		public event TimeChangedHandler TimeChangedEvent;
		public event StateChangeHandler PlaybackStateChangedEvent;
		public event LoadImageHander LoadImageEvent;
		public event PlaybackRateChangedHandler PlaybackRateChangedEvent;
		public event VolumeChangedHandler VolumeChangedEvent;
		public event ElementLoadedHandler ElementLoadedEvent;
		public event ElementUnloadedHandler ElementUnloadedEvent;
		public event PARChangedHandler PARChangedEvent;

		const int TIMEOUT_MS = 20;

		IPlayer player;
		TimelineEvent loadedEvent;
		IPlaylistElement loadedPlaylistElement;
		Playlist loadedPlaylist;

		Time streamLenght, videoTS, imageLoadedTS;
		bool readyToSeek, stillimageLoaded;
		MediaFile activeFile;
		Seeker seeker;
		Segment loadedSegment;
		object[] pendingSeek;
		Timer timer;

		struct Segment
		{
			public Time Start;
			public Time Stop;
		}

		#region Constructors

		public PlayerController ()
		{
			seeker = new Seeker ();
			seeker.SeekEvent += HandleSeekEvent;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
			videoTS = new Time (0);
			imageLoadedTS = new Time (0);
			streamLenght = new Time (0);
			Step = new Time (5000);
			timer = new Timer (HandleTimeout);
			CreatePlayer ();
		}

		#endregion

		#region Properties

		public bool IgnoreTicks {
			get;
			set;
		}

		public List<int> CamerasVisible {
			get;
			set;
		}

		public object CamerasLayout {
			get;
			set;
		}

		public List<IntPtr> WindowHandles {
			set {
				throw new NotImplementedException ();
			}
		}

		public IntPtr WindowHandle {
			set {
				throw new NotImplementedException ();
			}
		}

		public double Volume {
			get {
				return player.Volume;
			}
			set {
				player.Volume = value;
			}
		}

		public double Rate {
			set {
				player.Rate = value;
			}
		}

		public Time CurrentTime {
			get {
				return player.CurrentTime;
			}
		}

		public Time StreamLength {
			get {
				return player.StreamLength;
			}
		}

		public Image CurrentMiniatureFrame {
			get {
				return player.GetCurrentFrame (Constants.MAX_THUMBNAIL_SIZE, Constants.MAX_THUMBNAIL_SIZE);
			}
		}

		public Image CurrentFrame {
			get {
				return player.GetCurrentFrame ();
			}
		}

		public bool Playing {
			get;
			set;
		}

		public MediaFileSet MediaFileSet {
			get;
			protected set;
		}

		public bool Opened {
			get {
				return MediaFileSet != null;
			}
		}

		public Time Step {
			get;
			set;
		}

		#endregion

		#region Private Properties

		IPlayer Player {
			get;
			set;
		}

		#endregion


		#region Public methods

		public void Stop ()
		{
			Pause ();
		}


		public void Dispose ()
		{
			throw new NotImplementedException ();
		}

		public void Open (MediaFileSet fileSet)
		{
			player.Open (fileSet [0].FilePath);
		}

		public void Play ()
		{
			if (StillImageLoaded) {
				ReconfigureTimeout (TIMEOUT_MS);
			} else {
				EmitLoadImage (null);
				player.Play ();
			}
			Playing = true;
		}

		public void Pause ()
		{
			if (StillImageLoaded) {
				ReconfigureTimeout (0);
			} else {
				player.Pause ();
			}
			Playing = false;
		}

		public void Close ()
		{
			player.Error -= OnError;
			player.StateChange -= OnStateChanged;
			player.Eos -= OnEndOfStream;
			player.ReadyToSeek -= OnReadyToSeek;
			ReconfigureTimeout (0);
			player.Dispose ();
			MediaFileSet = null;
		}

		public void TogglePlay ()
		{
			if (Playing)
				Pause ();
			else
				Play ();
		}

		public bool Seek (Time time, bool accurate, bool synchronous = false)
		{
			if (!StillImageLoaded) {
				EmitLoadImage (null);
				if (readyToSeek) {
					player.Seek (time + activeFile.Offset, accurate, synchronous);
					OnTick ();
				} else {
					Log.Debug ("Delaying seek until player is ready");
					pendingSeek = new object[3] { time, 1.0f, false };
				}
			}
			return true;
		}

		public bool SeekToNextFrame ()
		{
			if (!StillImageLoaded) {
				EmitLoadImage (null);
				if (CurrentTime < loadedSegment.Stop) {
					player.SeekToNextFrame ();
					OnTick ();
				}
			}
			return true;
		}

		public bool SeekToPreviousFrame ()
		{
			if (!StillImageLoaded) {
				EmitLoadImage (null);
				if (CurrentTime > loadedSegment.Start) {
					seeker.Seek (SeekType.StepDown);
				}
			}
			return true;
		}

		public void StepForward ()
		{
			if (StillImageLoaded) {
				return;
			}
			EmitLoadImage (null);
			DoStep (Step);
		}

		public void StepBackward ()
		{
			if (StillImageLoaded) {
				return;
			}
			EmitLoadImage (null);
			DoStep (new Time (-Step.MSeconds));
		}

		public void FramerateUp ()
		{
			if (!StillImageLoaded) {
				EmitLoadImage (null);
			}

			/* FIXME */
			//vscale1.Adjustment.Value += vscale1.Adjustment.StepIncrement;
		}

		public void FramerateDown ()
		{
			if (!StillImageLoaded) {
				EmitLoadImage (null);
			}
			/* FIXME */
			//vscale1.Adjustment.Value -= vscale1.Adjustment.StepIncrement;
		}

		public void Expose ()
		{
			player.Expose ();
		}

		public void LoadPlayListEvent (Playlist playlist, IPlaylistElement element)
		{
			loadedEvent = null;
			loadedPlaylist = playlist;
			loadedPlaylistElement = element;
			EmitElementLoaded (playlist.HasNext ());

			if (element is PlaylistPlayElement) {
				PlaylistPlayElement ple = element as PlaylistPlayElement;
				TimelineEvent play = ple.Play;
				LoadSegment (ple.FileSet, play.Start, play.Stop, play.Start, true, ple.Rate);
			} else if (element is PlaylistVideo) {
				LoadVideo (element as PlaylistVideo);
			} else if (element is PlaylistImage) {
				LoadStillImage (element as PlaylistImage);
			} else if (element is PlaylistDrawing) {
				LoadFrameDrawing (element as PlaylistDrawing);
			}
		}

		public void LoadEvent (MediaFileSet fileSet, TimelineEvent evt, Time seekTime, bool playing)
		{
			loadedPlaylist = null;
			loadedPlaylistElement = null;
			loadedEvent = evt;
			if (evt.Start != null && evt.Start != null) {
				LoadSegment (fileSet, evt.Start, evt.Stop, seekTime, playing, evt.Rate);
			} else if (evt.EventTime != null) {
				Seek (evt.EventTime, true);
			} else {
				Log.Error ("Event does not have timing info: " + evt);
			}
		}

		public void UnloadCurrentEvent ()
		{
			EmitEventUnloaded ();
			SetRate (1);
			StillImageLoaded = false;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
			loadedEvent = null;
		}

		#endregion

		#region Signals

		void EmitLoadImage (Image image, FrameDrawing drawing = null, bool isStill = false)
		{
			if (LoadImageEvent != null) {
				LoadImageEvent (image, drawing);
			}
		}

		void EmitElementLoaded (bool hasNext)
		{
			if (ElementLoadedEvent != null) {
				ElementLoadedEvent (hasNext);
			}
		}

		void EmitEventUnloaded ()
		{
			if (ElementUnloadedEvent != null) {
				ElementUnloadedEvent ();
			}
		}

		void EmitRateChanged (float rate)
		{
			if (PlaybackRateChangedEvent != null) {
				PlaybackRateChangedEvent (rate);
			}
		}

		void EmitVolumeChanged (float volume)
		{
			if (VolumeChangedEvent != null) {
				VolumeChangedEvent (volume);
			}
		}

		void EmitTimeChanged (Time currentTime, Time duration)
		{
			if (TimeChangedEvent != null) {
				TimeChangedEvent (currentTime, duration, StillImageLoaded);
			}
		}

		void EmitPARChanged (float par)
		{
			if (PARChangedEvent != null) {
				PARChangedEvent (par);
			}
		}

		void EmitPlaybackStateChanged (bool playing)
		{
			if (PlaybackStateChangedEvent != null) {
				PlaybackStateChangedEvent (playing);
			}
		}

		#endregion

		#region Private methods

		void SetRate (float rate)
		{
			Rate = rate;
			EmitRateChanged (rate);
		}

		bool StillImageLoaded {
			set {
				stillimageLoaded = value;
				if (stillimageLoaded) {
					player.Pause ();
					imageLoadedTS = new Time (0);
					ReconfigureTimeout (TIMEOUT_MS);
				}
			}
			get {
				return stillimageLoaded;
			}
		}

		void Open (MediaFileSet fileSet, bool seek, bool force = false, bool play = false)
		{
			UnloadCurrentEvent ();
			if (fileSet != this.MediaFileSet || force) {
				readyToSeek = false;
				MediaFileSet = fileSet;
				activeFile = fileSet.First ();
				if (activeFile.VideoHeight != 0) {
					EmitPARChanged ((float)(activeFile.VideoWidth * activeFile.Par / activeFile.VideoHeight));
				} else {
					EmitPARChanged (1);
				}
				try {
					Log.Debug ("Opening new file " + activeFile.FilePath);
					player.Open (fileSet);
				} catch (Exception ex) {
					Log.Exception (ex);
					//We handle this error async
				}
			} else {
				if (seek) {
					Seek (new Time (0), true);
				}
			}
			if (play) {
				player.Play ();
			}
		}

		bool SegmentLoaded {
			get {
				return loadedSegment.Start.MSeconds != -1;
			}
		}

		List<FrameDrawing> EventDrawings {
			get {
				if (loadedEvent != null) {
					return loadedEvent.Drawings;
				} else if (loadedPlaylistElement is PlaylistPlayElement) {
					return (loadedPlaylistElement as PlaylistPlayElement).Play.Drawings;
				}
				return null;
			}
		}

		void LoadSegment (MediaFileSet fileSet, Time start, Time stop, Time seekTime,
		                  bool playing, float rate = 1)
		{
			Log.Debug (String.Format ("Update player segment {0} {1} {2}",
				start.ToMSecondsString (),
				stop.ToMSecondsString (), rate));
			if (fileSet != this.MediaFileSet) {
				Open (fileSet, false);
			}
			Pause ();
			loadedSegment.Start = start;
			loadedSegment.Stop = stop;
			rate = rate == 0 ? 1 : rate;
			EmitElementLoaded (false);
			StillImageLoaded = false;
			if (readyToSeek) {
				Log.Debug ("Player is ready to seek, seeking to " +
				seekTime.ToMSecondsString ());
				SetRate (rate);
				Seek (seekTime, true);
				if (playing) {
					Play ();
				}
			} else {
				Log.Debug ("Delaying seek until player is ready");
				pendingSeek = new object[3] { seekTime, rate, playing };
			}
		}

		void LoadImage (Image image, FrameDrawing drawing)
		{
			EmitLoadImage (image, drawing);
		}

		void LoadStillImage (PlaylistImage image)
		{
			loadedPlaylistElement = image;
			UnloadCurrentEvent ();
			StillImageLoaded = true;
			LoadImage (image.Image, null);
		}

		void LoadFrameDrawing (PlaylistDrawing drawing)
		{
			loadedPlaylistElement = drawing;
			UnloadCurrentEvent ();
			StillImageLoaded = true;
			LoadImage (null, drawing.Drawing);
		}

		void LoadVideo (PlaylistVideo video)
		{
			loadedPlaylistElement = video;
			MediaFileSet fileSet = new MediaFileSet ();
			fileSet.Add (video.File);
			Open (fileSet, false, true, true);
		}

		void LoadPlayDrawing (FrameDrawing drawing)
		{
			Pause ();
			IgnoreTicks = true;
			player.Seek (drawing.Render + activeFile.Offset, true, true);
			IgnoreTicks = false;
			LoadImage (CurrentFrame, drawing);
		}

		void DoStep (Time step)
		{
			Time pos = CurrentTime + step;
			if (pos.MSeconds < 0)
				pos.MSeconds = 0;
			Log.Debug (String.Format ("Stepping {0} seconds from {1} to {2}",
				step, CurrentTime, pos));
			EmitLoadImage (null);
			Seek (pos, true);
		}

		void SeekFromTimescale (double pos)
		{
			Time seekPos, duration;
			SeekType seekType;

			if (SegmentLoaded) {
				duration = loadedSegment.Stop - loadedSegment.Start;
				seekPos = loadedSegment.Start + duration * pos;
				seekType = SeekType.Accurate;
			} else {
				duration = streamLenght;
				seekPos = streamLenght * pos;
				seekType = SeekType.Keyframe;
			}
			seeker.Seek (seekType, seekPos);
			EmitTimeChanged (seekPos, duration);
		}

		void CreatePlayer ()
		{
			player = Config.MultimediaToolkit.GetPlayer ();

			player.Error += OnError;
			player.StateChange += OnStateChanged;
			player.Eos += OnEndOfStream;
			player.ReadyToSeek += OnReadyToSeek;
		}

		void ReconfigureTimeout (uint mseconds)
		{
			if (mseconds == 0) {
				timer.Change (Timeout.Infinite, Timeout.Infinite);
			} else {
				timer.Change (mseconds, mseconds);
			}
		}

		void DoStateChanged (bool playing)
		{
			if (playing) {
				ReconfigureTimeout (TIMEOUT_MS);
			} else {
				if (!StillImageLoaded) {
					ReconfigureTimeout (0);
				}
			}
			EmitPlaybackStateChanged (playing);
		}

		void DoReadyToSeek ()
		{
			readyToSeek = true;
			streamLenght = player.StreamLength;
			if (pendingSeek != null) {
				SetRate ((float)pendingSeek [1]);
				player.Seek ((Time)pendingSeek [0], true);
				if ((bool)pendingSeek [2]) {
					Play ();
				}
				pendingSeek = null;
			}
			OnTick ();
		}

		#endregion

		#region Backend Callbacks

		/* These callbacks are triggered by the multimedia backend and need to
		 * be deferred to the UI main thread */
		void OnStateChanged (bool playing)
		{
			Config.DrawingToolkit.Invoke (delegate {
				DoStateChanged (playing);
			});
		}

		void OnReadyToSeek ()
		{
			Config.DrawingToolkit.Invoke (delegate {
				DoReadyToSeek ();
			});
		}

		void OnEndOfStream ()
		{
			Config.DrawingToolkit.Invoke (delegate {
				if (loadedPlaylistElement is PlaylistVideo) {
					Config.EventsBroker.EmitNextPlaylistElement (loadedPlaylist);
				} else {
					Seek (new Time (0), true);
					Pause ();
				}
			});
		}

		void OnError (string message)
		{
			Config.DrawingToolkit.Invoke (delegate {
				Config.EventsBroker.EmitMultimediaError (message);
			});
		}

		#endregion

		#region Callbacks

		void HandleTimeout (Object state)
		{
			OnTick ();
		}

		bool OnTick ()
		{
			if (IgnoreTicks) {
				return true;
			}

			if (StillImageLoaded) {
				EmitTimeChanged (imageLoadedTS, loadedPlaylistElement.Duration);
				if (imageLoadedTS >= loadedPlaylistElement.Duration) {
					Config.EventsBroker.EmitNextPlaylistElement (loadedPlaylist);
				} else {
					imageLoadedTS.MSeconds += TIMEOUT_MS;
				}
				return true;
			} else {
				Time currentTime = CurrentTime;

				if (SegmentLoaded) {
					EmitTimeChanged (currentTime - loadedSegment.Start,
						loadedSegment.Stop - loadedSegment.Start);
					if (currentTime > loadedSegment.Stop) {
						/* Check if the segment is now finished and jump to next one */
						Pause ();
						Config.EventsBroker.EmitNextPlaylistElement (loadedPlaylist);
					} else {
						var drawings = EventDrawings;
						if (drawings != null) {
							/* Check if the event has drawings to display */
							FrameDrawing fd = drawings.FirstOrDefault (f => f.Render > videoTS && f.Render <= currentTime);
							if (fd != null) {
								LoadPlayDrawing (fd);
							}
						}
					}
				} else {
					EmitTimeChanged (currentTime, streamLenght);
				}
				videoTS = currentTime;

				Config.EventsBroker.EmitPlayerTick (currentTime);
				return true;
			}
		}

		void HandleSeekEvent (SeekType type, Time start, float rate)
		{
			EmitLoadImage (null, null);
			/* We only use it for backwards framestepping for now */
			if (type == SeekType.StepDown || type == SeekType.StepUp) {
				if (player.Playing)
					Pause ();
				if (type == SeekType.StepDown)
					player.SeekToPreviousFrame ();
				else
					player.SeekToNextFrame ();
				OnTick ();
			}
			if (type == SeekType.Accurate || type == SeekType.Keyframe) {
				SetRate (rate);
				Seek (start, type == SeekType.Accurate);
			}
		}

		#endregion
	}
}

