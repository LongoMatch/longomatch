﻿//
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
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using Mono.Unix;
using Timer = System.Threading.Timer;

namespace LongoMatch.Services
{
	public class PlayerController: IPlayerController
	{
		public event TimeChangedHandler TimeChangedEvent;
		public event StateChangeHandler PlaybackStateChangedEvent;
		public event LoadDrawingsHandler LoadDrawingsEvent;
		public event PlaybackRateChangedHandler PlaybackRateChangedEvent;
		public event VolumeChangedHandler VolumeChangedEvent;
		public event ElementLoadedHandler ElementLoadedEvent;
		public event MediaFileSetLoadedHandler MediaFileSetLoadedEvent;
		public event PrepareViewHandler PrepareViewEvent;

		const int TIMEOUT_MS = 20;
		const int SCALE_FPS = 25;

		IPlayer player;
		IMultiPlayer multiPlayer;
		TimelineEvent loadedEvent;
		IPlaylistElement loadedPlaylistElement;
		Playlist loadedPlaylist;
		List<IViewPort> viewPorts;
		List<CameraConfig> camerasConfig;
		List<CameraConfig> defaultCamerasConfig;
		object defaultCamerasLayout;
		MediaFileSet defaultFileSet;

		Time streamLength, videoTS, imageLoadedTS;
		bool readyToSeek, stillimageLoaded, ready;
		bool disposed, skipApplyCamerasConfig;
		Action delayedOpen;
		Seeker seeker;
		Segment loadedSegment;
		PendingSeek pendingSeek;
		readonly Timer timer;
		readonly ManualResetEvent TimerDisposed;

		struct Segment
		{
			public Time Start;
			public Time Stop;
		}

		class PendingSeek
		{
			public Time time;
			public float rate;
			public bool playing;
			public bool accurate;
			public bool syncrhonous;
			public bool throttled;
		}

		#region Constructors

		public PlayerController (bool supportMultipleCameras = false)
		{
			seeker = new Seeker ();
			seeker.SeekEvent += HandleSeekEvent;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
			videoTS = new Time (0);
			imageLoadedTS = new Time (0);
			streamLength = new Time (0);
			Step = new Time (5000);
			timer = new Timer (HandleTimeout);
			TimerDisposed = new ManualResetEvent (false);
			ready = false;
			CreatePlayer (supportMultipleCameras);
		}

		#endregion

		#region IPlayerController implementation

		public bool IgnoreTicks {
			get;
			set;
		}

		public List<CameraConfig> CamerasConfig {
			set {
				Log.Debug ("Updating cameras configuration: ", string.Join ("-", value));
				camerasConfig = value;
				if (defaultCamerasConfig == null) {
					defaultCamerasConfig = value;
				}
				if (loadedEvent != null) {
					loadedEvent.CamerasConfig = value.ToList ();
				} else if (loadedPlaylistElement is PlaylistPlayElement) {
					(loadedPlaylistElement as PlaylistPlayElement).CamerasConfig = value.ToList ();
				}
				if (multiPlayer != null) {
					multiPlayer.CamerasConfig = camerasConfig;
					if (!skipApplyCamerasConfig && Opened) {
						ApplyCamerasConfig ();
					}
				}
			}
			get {
				if (camerasConfig != null) {
					return camerasConfig.Clone ();
				} else {
					return null;
				}
			}
		}

		public object CamerasLayout {
			get;
			set;
		}

		public List<IViewPort> ViewPorts {
			set {
				if (value != null) {
					if (multiPlayer == null) {
						player.WindowHandle = value [0].WindowHandle;
					} else {
						multiPlayer.WindowHandles = value.Select (v => v.WindowHandle).ToList ();
					}
				}
				viewPorts = value;
			}
			protected get {
				return viewPorts;
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
				Log.Debug ("Rate set to " + value);
			}
			get {
				return player.Rate;
			}
		}

		public Time CurrentTime {
			get {
				if (StillImageLoaded) {
					return imageLoadedTS;
				} else {
					return player.CurrentTime;
				}
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

		public MediaFileSet FileSet {
			get;
			protected set;
		}

		public bool Opened {
			get {
				return FileSet != null;
			}
		}

		public Time Step {
			get;
			set;
		}

		public void Dispose ()
		{
			if (!disposed) {
				Log.Debug ("Disposing PlayerController");
				ReconfigureTimeout (0);
				IgnoreTicks = true;
				seeker.Dispose ();
				timer.Dispose (TimerDisposed);
				TimerDisposed.WaitOne (200);
				TimerDisposed.Dispose ();
				player.Error -= HandleError;
				player.StateChange -= HandleStateChange;
				player.Eos -= HandleEndOfStream;
				player.ReadyToSeek -= HandleReadyToSeek;
				player.Dispose ();
				FileSet = null;
			}
			disposed = true;
		}

		public void Ready ()
		{
			Log.Debug ("Player ready");
			ready = true;
			if (delayedOpen != null) {
				Log.Debug ("Calling delayed open");
				delayedOpen ();
				delayedOpen = null;
			}
		}

		public void Open (MediaFileSet fileSet)
		{
			Log.Debug ("Openning file set");
			if (ready) {
				InternalOpen (fileSet, true, true, false, true);
			} else {
				Log.Debug ("Player is not ready, delaying ...");
				delayedOpen = () => InternalOpen (FileSet, true, true, false, true);
				FileSet = fileSet;
			}
		}

		public void Stop ()
		{
			Log.Debug ("Stop");
			Pause ();
		}

		public void Play ()
		{
			Log.Debug ("Play");
			if (StillImageLoaded) {
				ReconfigureTimeout (TIMEOUT_MS);
				EmitPlaybackStateChanged (this, true);
			} else {
				EmitLoadDrawings (null);
				player.Play ();
			}
			Playing = true;
		}

		public void Pause ()
		{
			Log.Debug ("Pause");
			if (StillImageLoaded) {
				ReconfigureTimeout (0);
				EmitPlaybackStateChanged (this, false);
			} else {
				player.Pause ();
			}
			Playing = false;
		}

		public void TogglePlay ()
		{
			Log.Debug ("Toggle playback");
			if (Playing)
				Pause ();
			else
				Play ();
		}

		public bool Seek (Time time, bool accurate, bool synchronous = false, bool throtlled = false)
		{
			if (StillImageLoaded) {
				imageLoadedTS = time;
			} else {
				EmitLoadDrawings (null);
				if (readyToSeek) {
					if (throtlled) {
						Log.Debug ("Throttled seek");
						seeker.Seek (accurate ? SeekType.Accurate : SeekType.Keyframe, time);
					} else {
						Log.Debug (string.Format ("Seeking to {0} accurate:{1} synchronous:{2} throttled:{3}",
							time, accurate, synchronous, throtlled));
						player.Seek (time, accurate, synchronous);
						Tick ();
					}
				} else {
					Log.Debug ("Delaying seek until player is ready");
					pendingSeek = new PendingSeek {
						time = time,
						rate = 1.0f,
						accurate = accurate,
						syncrhonous = synchronous,
						throttled = throtlled
					};
				}
			}
			return true;
		}

		public bool Seek (Time time, bool accurate, bool synchronous)
		{
			return Seek (time, accurate, synchronous, false);
		}

		public void Seek (double pos)
		{
			Time seekPos;
			bool accurate;
			bool throthled;

			Log.Debug (string.Format ("Seek relative to {0}", pos));
			if (SegmentLoaded) {
				Time duration = loadedSegment.Stop - loadedSegment.Start;
				seekPos = loadedSegment.Start + duration * pos;
				accurate = true;
				throthled = true;
			} else {
				seekPos = streamLength * pos;
				accurate = false;
				throthled = false;
			}
			Seek (seekPos, accurate, false, throthled);
		}

		public bool SeekToNextFrame ()
		{
			Log.Debug ("Seek to next frame");
			if (!StillImageLoaded) {
				EmitLoadDrawings (null);
				if (CurrentTime < loadedSegment.Stop) {
					player.SeekToNextFrame ();
					Tick ();
				}
			}
			return true;
		}

		public bool SeekToPreviousFrame ()
		{
			Log.Debug ("Seek to previous frame");
			if (!StillImageLoaded) {
				if (CurrentTime > loadedSegment.Start) {
					seeker.Seek (SeekType.StepDown);
				}
			}
			return true;
		}

		public void StepForward ()
		{
			Log.Debug ("Step forward");
			if (StillImageLoaded) {
				return;
			}
			PerformStep (Step);
		}

		public void StepBackward ()
		{
			Log.Debug ("Step backward");
			if (StillImageLoaded) {
				return;
			}
			PerformStep (new Time (-Step.MSeconds));
		}

		public void FramerateUp ()
		{
			if (!StillImageLoaded) {
				float rate;

				EmitLoadDrawings (null);
				rate = (float)Rate;
				if (rate >= 5) {
					return;
				}
				Log.Debug ("Framerate up");
				if (rate < 1) {
					SetRate (rate + (float)1 / SCALE_FPS);
				} else {
					SetRate (rate + 1);
				}
			}
		}

		public void FramerateDown ()
		{

			if (!StillImageLoaded) {
				float rate;

				EmitLoadDrawings (null);
				rate = (float)Rate;
				if (rate <= (float)1 / SCALE_FPS) {
					return;
				}
				Log.Debug ("Framerate down");
				if (rate > 1) {
					SetRate (rate - 1);
				} else {
					SetRate (rate - (float)1 / SCALE_FPS);
				}
			}
		}

		public void Expose ()
		{
			player.Expose ();
		}

		public void LoadPlaylistEvent (Playlist playlist, IPlaylistElement element)
		{
			Log.Debug (string.Format ("Loading playlist element \"{0}\"", element.Description));

			loadedEvent = null;
			loadedPlaylist = playlist;
			loadedPlaylistElement = element;

			if (element is PlaylistPlayElement) {
				PlaylistPlayElement ple = element as PlaylistPlayElement;
				LoadSegment (ple.FileSet, ple.Play.Start, ple.Play.Stop,
					ple.Play.Start, ple.Rate, ple.CamerasConfig,
					ple.CamerasLayout, true);
			} else if (element is PlaylistVideo) {
				LoadVideo (element as PlaylistVideo);
			} else if (element is PlaylistImage) {
				LoadStillImage (element as PlaylistImage);
			} else if (element is PlaylistDrawing) {
				LoadFrameDrawing (element as PlaylistDrawing);
			}
			EmitElementLoaded (element, playlist.HasNext ());
		}

		public void LoadEvent (MediaFileSet fileSet, TimelineEvent evt, Time seekTime, bool playing)
		{
			Log.Debug (string.Format ("Loading event \"{0}\" seek:{1} playing:{2}", evt.Name, seekTime, playing));

			if (!ready) {
				EmitPrepareViewEvent ();
				delayedOpen = () => LoadEvent (fileSet, evt, seekTime, playing);
				return;
			}

			loadedPlaylist = null;
			loadedPlaylistElement = null;
			loadedEvent = evt;
			if (evt.Start != null && evt.Start != null) {
				LoadSegment (fileSet, evt.Start, evt.Stop, seekTime, evt.Rate,
					evt.CamerasConfig, evt.CamerasLayout, playing);
			} else if (evt.EventTime != null) {
				Seek (evt.EventTime, true);
			} else {
				Log.Error ("Event does not have timing info: " + evt);
			}
			EmitElementLoaded (evt, false);
		}

		public void UnloadCurrentEvent ()
		{
			Log.Debug ("Unload current event");
			Reset ();
			if (defaultFileSet != null && FileSet != defaultFileSet) {
				UpdateCamerasConfig (defaultCamerasConfig, defaultCamerasLayout);
				EmitEventUnloaded ();
				Open (defaultFileSet);
			} else {
				CamerasConfig = defaultCamerasConfig;
				EmitEventUnloaded ();
			}
		}

		public void Next ()
		{
			Log.Debug ("Next");
			if (loadedPlaylistElement != null && loadedPlaylist.HasNext ()) {
				Config.EventsBroker.EmitNextPlaylistElement (loadedPlaylist);
			}
		}

		public void Previous ()
		{
			Log.Debug ("Previous");
			if (loadedPlaylistElement != null) {
				if (loadedPlaylist.HasPrev ()) {
					Config.EventsBroker.EmitPreviousPlaylistElement (loadedPlaylist);
				}
			} else if (loadedEvent != null) {
				Seek (loadedEvent.Start, true);
			} else {
				Seek (new Time (0), true);
			}
		}

		#endregion

		#region Signals

		void EmitLoadDrawings (FrameDrawing drawing = null)
		{
			if (LoadDrawingsEvent != null && !disposed) {
				LoadDrawingsEvent (drawing);
			}
		}

		void EmitPrepareViewEvent ()
		{
			if (PrepareViewEvent != null && !disposed) {
				PrepareViewEvent ();
			}
		}

		void EmitElementLoaded (object element, bool hasNext)
		{
			if (ElementLoadedEvent != null && !disposed) {
				ElementLoadedEvent (element, hasNext);
			}
		}

		void EmitEventUnloaded ()
		{
			EmitElementLoaded (null, false);
		}

		void EmitRateChanged (float rate)
		{
			if (PlaybackRateChangedEvent != null && !disposed) {
				PlaybackRateChangedEvent (rate);
			}
		}

		void EmitVolumeChanged (float volume)
		{
			if (VolumeChangedEvent != null && !disposed) {
				VolumeChangedEvent (volume);
			}
		}

		void EmitTimeChanged (Time currentTime, Time duration)
		{
			if (TimeChangedEvent != null && !disposed) {
				TimeChangedEvent (currentTime, duration, !StillImageLoaded);
			}
		}

		void EmitPlaybackStateChanged (object sender, bool playing)
		{
			if (PlaybackStateChangedEvent != null && !disposed) {
				PlaybackStateChangedEvent (sender, playing);
			}
		}

		void EmitMediaFileSetLoaded (MediaFileSet fileSet, List<CameraConfig> camerasVisible)
		{
			if (MediaFileSetLoadedEvent != null && !disposed) {
				MediaFileSetLoadedEvent (fileSet, camerasConfig);
			}
		}

		#endregion

		#region Private Properties

		/// <summary>
		/// Indicates if a still image is loaded instead of a video segment.
		/// </summary>
		bool StillImageLoaded {
			set {
				stillimageLoaded = value;
				if (stillimageLoaded) {
					EmitPlaybackStateChanged (this, true);
					player.Pause ();
					imageLoadedTS = new Time (0);
					ReconfigureTimeout (TIMEOUT_MS);
				}
			}
			get {
				return stillimageLoaded;
			}
		}

		/// <summary>
		/// Inidicates if a video segment is loaded.
		/// </summary>
		bool SegmentLoaded {
			get {
				return loadedSegment.Start.MSeconds != -1;
			}
		}

		/// <summary>
		/// Gets the list of drawing for the loaded event.
		/// </summary>
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


		#endregion

		#region Private methods

		/// <summary>
		/// Updates the cameras configuration internally without applying the new
		/// configuration in the <see cref="IMultiPlayer"/>.
		/// </summary>
		/// <param name="camerasConfig">The cameras configuration.</param>
		/// <param name="layout">The cameras layout.</param>
		void UpdateCamerasConfig (List<CameraConfig> camerasConfig, object layout)
		{
			skipApplyCamerasConfig = true;
			CamerasConfig = camerasConfig;
			CamerasLayout = layout;
			skipApplyCamerasConfig = false;
		}

		/// <summary>
		/// Applies the current cameras configuration.
		/// </summary>
		void ApplyCamerasConfig ()
		{
			ValidateVisibleCameras ();
			if (multiPlayer != null) {
				multiPlayer.ApplyCamerasConfig ();
				UpdatePar ();
			}
		}

		/// <summary>
		/// Validates that the list of visible cameras indexes are consistent with fileset
		/// </summary>
		void ValidateVisibleCameras ()
		{
			if (FileSet != null && camerasConfig != null && camerasConfig.Max (c => c.Index) >= FileSet.Count) {
				Log.Error ("Invalid cameras configuration, fixing list of cameras");
				UpdateCamerasConfig (camerasConfig.Where (i => i.Index < FileSet.Count).ToList<CameraConfig> (),
					CamerasLayout);
			}
		}

		/// <summary>
		/// Updates the pixel aspect ration in all the view ports.
		/// </summary>
		void UpdatePar ()
		{
			for (int i = 0; i < Math.Min (CamerasConfig.Count, ViewPorts.Count); i++) {
				int index = CamerasConfig [i].Index;
				MediaFile file = FileSet [index];
				float par = 1;
				if (file.VideoHeight != 0) {
					par = (float)(file.VideoWidth * file.Par / file.VideoHeight);
				}
				ViewPorts [i].Ratio = par;
			}
		}

		/// <summary>
		/// Open the specified file set.
		/// </summary>
		/// <param name="fileSet">the files to open.</param>
		/// <param name="seek">If set to <c>true</c>, seeks to the beginning of the stream.</param>
		/// <param name="force">If set to <c>true</c>, opens the fileset even if it was already set.</param>
		/// <param name="play">If set to <c>true</c>, sets the player to play.</param>
		/// <param name="defaultFile">If set to <c>true</c>, store this as the default file set to use.</param>
		void InternalOpen (MediaFileSet fileSet, bool seek, bool force = false, bool play = false, bool defaultFile = false)
		{
			Reset ();
			// This event gives a chance to the view to define camera visibility.
			// As there might already be a configuration defined (loading an event for example), the view
			// should adapt if needed.
			skipApplyCamerasConfig = true;
			EmitMediaFileSetLoaded (fileSet, camerasConfig);
			skipApplyCamerasConfig = false;

			if (defaultFile) {
				defaultFileSet = fileSet;
			}

			if (fileSet != FileSet || force) {
				readyToSeek = false;
				FileSet = fileSet;
				// Check if the view failed to configure a proper cam config
				if (CamerasConfig == null) {
					Config.EventsBroker.EmitMultimediaError (this, 
						Catalog.GetString ("Invalid camera configuration"));
					FileSet = null;
					return;
				}
				// Validate Cam config against fileset
				ValidateVisibleCameras ();
				UpdatePar ();
				try {
					Log.Debug ("Opening new file set " + fileSet);
					if (multiPlayer != null) {
						multiPlayer.Open (fileSet);
					} else {
						player.Open (fileSet [0]);
					}
					EmitTimeChanged (new Time (0), player.StreamLength);
				} catch (Exception ex) {
					Log.Exception (ex);
					//We handle this error async
				}
			}
			if (seek) {
				Seek (new Time (0), true);
			}
			if (play) {
				player.Play ();
			}
		}

		/// <summary>
		/// Reset the player segment information.
		/// </summary>
		void Reset ()
		{
			SetRate (1);
			StillImageLoaded = false;
			loadedSegment.Start = new Time (-1);
			loadedSegment.Stop = new Time (int.MaxValue);
			loadedEvent = null;
		}

		/// <summary>
		/// Sets the rate and notifies the change.
		/// </summary>
		void SetRate (float rate)
		{
			if (rate == 0)
				rate = 1;
			Rate = rate;
			EmitRateChanged (rate);
		}

		/// <summary>
		/// Loads a video segment defined by a <see cref="TimelineEvent"/> in the player.
		/// </summary>
		/// <param name="fileSet">File set.</param>
		/// <param name="start">Start time.</param>
		/// <param name="stop">Stop time.</param>
		/// <param name="seekTime">Position to seek after loading the segment.</param>
		/// <param name="rate">Playback rate.</param>
		/// <param name="camerasConfig">Cameras configuration.</param>
		/// <param name="camerasLayout">Cameras layout.</param>
		/// <param name="playing">If set to <c>true</c> starts playing.</param>
		void LoadSegment (MediaFileSet fileSet, Time start, Time stop, Time seekTime,
		                  float rate, List<CameraConfig> camerasConfig, object camerasLayout,
		                  bool playing)
		{
			Log.Debug (String.Format ("Update player segment {0} {1} {2}",
				start, stop, rate));

			if (!SegmentLoaded) {
				defaultCamerasConfig = CamerasConfig;
				defaultCamerasLayout = CamerasLayout;
			}

			UpdateCamerasConfig (camerasConfig, camerasLayout);

			if (fileSet != this.FileSet) {
				InternalOpen (fileSet, false);
			} else {
				ApplyCamerasConfig ();
			}

			Pause ();
			loadedSegment.Start = start;
			loadedSegment.Stop = stop;
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
				pendingSeek = new PendingSeek {
					time = seekTime,
					rate = 1.0f,
					playing = playing,
					accurate = true,
				};
			}
		}

		void LoadStillImage (PlaylistImage image)
		{
			loadedPlaylistElement = image;
			StillImageLoaded = true;
		}

		void LoadFrameDrawing (PlaylistDrawing drawing)
		{
			loadedPlaylistElement = drawing;
			StillImageLoaded = true;
		}

		void LoadVideo (PlaylistVideo video)
		{
			loadedPlaylistElement = video;
			MediaFileSet fileSet = new MediaFileSet ();
			fileSet.Add (video.File);
			EmitLoadDrawings (null);
			UpdateCamerasConfig (new List<CameraConfig> { new CameraConfig (0) }, null);
			InternalOpen (fileSet, false, true, true);
		}

		void LoadPlayDrawing (FrameDrawing drawing)
		{
			Pause ();
			IgnoreTicks = true;
			player.Seek (drawing.Render, true, true);
			IgnoreTicks = false;
			EmitLoadDrawings (drawing);
		}

		/// <summary>
		/// Performs a step using the configured <see cref="Step"/> time.
		/// </summary>
		void PerformStep (Time step)
		{
			Time pos = CurrentTime + step;
			if (pos.MSeconds < 0) {
				pos.MSeconds = 0;
			} else if (pos >= StreamLength) {
				pos = StreamLength;
			}
			Log.Debug (String.Format ("Stepping {0} seconds from {1} to {2}",
				step, CurrentTime, pos));
			Seek (pos, true);
		}

		/// <summary>
		/// Creates the backend video player.
		/// </summary>
		void CreatePlayer (bool supportMultipleCameras)
		{
			if (supportMultipleCameras) {
				try {
					player = multiPlayer = Config.MultimediaToolkit.GetMultiPlayer ();
					multiPlayer.ScopeChangedEvent += HandleScopeChangedEvent;
				} catch {
					Log.Error ("Player with support for multiple cameras not found");
				}
			}
			if (player == null) {
				player = Config.MultimediaToolkit.GetPlayer ();
			}


			player.Error += HandleError;
			player.StateChange += HandleStateChange;
			player.Eos += HandleEndOfStream;
			player.ReadyToSeek += HandleReadyToSeek;
		}

		/// <summary>
		/// Reconfigures the timeout for the timer emitting the timming events.
		/// If set to <code>0</code>, the timer is topped
		/// </summary>
		/// <param name="mseconds">Mseconds.</param>
		void ReconfigureTimeout (uint mseconds)
		{
			if (mseconds == 0) {
				timer.Change (Timeout.Infinite, Timeout.Infinite);
			} else {
				timer.Change (mseconds, mseconds);
			}
		}

		/// <summary>
		/// Called periodically to update the current time and check if and has reached
		/// its stop time, or drawings must been shonw.
		/// </summary>
		bool Tick ()
		{
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
							FrameDrawing fd = drawings.FirstOrDefault (f => f.Render > videoTS &&
							                  f.Render <= currentTime &&
							                  f.CameraConfig.Index == CamerasConfig [0].Index);
							if (fd != null) {
								LoadPlayDrawing (fd);
							}
						}
					}
				} else {
					EmitTimeChanged (currentTime, streamLength);
				}
				videoTS = currentTime;

				Config.EventsBroker.EmitPlayerTick (currentTime);
				return true;
			}
		}

		#endregion

		#region Backend Callbacks

		/* These callbacks are triggered by the multimedia backend and need to
		 * be deferred to the UI main thread */
		void HandleStateChange (object sender, bool playing)
		{
			Config.GUIToolkit.Invoke (delegate {
				if (playing) {
					ReconfigureTimeout (TIMEOUT_MS);
				} else {
					if (!StillImageLoaded) {
						ReconfigureTimeout (0);
					}
				}
				EmitPlaybackStateChanged (this, playing);
			});
		}

		void HandleReadyToSeek (object sender)
		{
			Config.GUIToolkit.Invoke (delegate {
				readyToSeek = true;
				streamLength = player.StreamLength;
				if (pendingSeek != null) {
					SetRate (pendingSeek.rate);
					player.Seek (pendingSeek.time, pendingSeek.accurate, pendingSeek.syncrhonous);
					if (pendingSeek.playing) {
						Play ();
					}
					pendingSeek = null;
				}
				Tick ();
				player.Expose ();
			});
		}

		void HandleEndOfStream (object sender)
		{
			Config.GUIToolkit.Invoke (delegate {
				if (loadedPlaylistElement is PlaylistVideo) {
					Config.EventsBroker.EmitNextPlaylistElement (loadedPlaylist);
				} else {
					Seek (new Time (0), true);
					Pause ();
				}
			});
		}

		void HandleError (object sender, string message)
		{
			Config.GUIToolkit.Invoke (delegate {
				Config.EventsBroker.EmitMultimediaError (sender, message);
			});
		}

		void HandleScopeChangedEvent (int index, bool visible)
		{
			ViewPorts [index].MessageVisible = !visible;
		}

		#endregion

		#region Callbacks

		void HandleTimeout (Object state)
		{
			Config.GUIToolkit.Invoke (delegate {
				if (!IgnoreTicks) {
					Tick ();
				}
			});
		}

		void HandleSeekEvent (SeekType type, Time start, float rate)
		{
			Config.GUIToolkit.Invoke (delegate {
				EmitLoadDrawings (null);
				/* We only use it for backwards framestepping for now */
				if (type == SeekType.StepDown || type == SeekType.StepUp) {
					if (player.Playing)
						Pause ();
					if (type == SeekType.StepDown)
						player.SeekToPreviousFrame ();
					else
						player.SeekToNextFrame ();
					Tick ();
				}
				if (type == SeekType.Accurate || type == SeekType.Keyframe) {
					SetRate (rate);
					Seek (start, type == SeekType.Accurate, false, false);
				}
			});
		}

		#endregion
	}
}
