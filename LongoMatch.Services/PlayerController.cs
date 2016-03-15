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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
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
		List<IViewPort> viewPorts;
		ObservableCollection<CameraConfig> camerasConfig;
		ObservableCollection<CameraConfig> defaultCamerasConfig;
		object defaultCamerasLayout;
		MediaFileSet defaultFileSet;
		MediaFileSet mediafileSet;
		MediaFileSet mediaFileSetCopy;

		Time streamLength, videoTS, imageLoadedTS;
		bool readyToSeek, stillimageLoaded, ready;
		bool disposed, skipApplyCamerasConfig;
		Action delayedOpen;
		Seeker seeker;
		Segment loadedSegment;
		PendingSeek pendingSeek;
		readonly Timer timer;
		readonly ManualResetEvent TimerDisposed;

		bool active;

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
			Active = true;
			PresentationMode = false;
		}

		#endregion

		#region IPlayerController implementation

		public bool IgnoreTicks {
			get;
			set;
		}

		public ObservableCollection<CameraConfig> CamerasConfig {
			set {
				Log.Debug ("Updating cameras configuration: ", string.Join ("-", value));
				camerasConfig = value;
				if (defaultCamerasConfig == null) {
					defaultCamerasConfig = value;
				}
				if (loadedEvent != null) {
					loadedEvent.CamerasConfig = new ObservableCollection<CameraConfig> (value);
				} else if (loadedPlaylistElement is PlaylistPlayElement) {
					(loadedPlaylistElement as PlaylistPlayElement).CamerasConfig =
						new ObservableCollection<CameraConfig> (value);
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
				SetRate ((float)value);
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
			get {
				return mediafileSet;
			}
			protected set {
				mediafileSet = value;
				mediaFileSetCopy = value.Clone ();
			}
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

		public bool Active {
			get { return active; }
			set {
				active = value;
				if (!value && Playing) {
					Pause ();
				}
			}
		}

		public Playlist LoadedPlaylist {
			get;
			set;
		}

		public bool PresentationMode {
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

		public void Stop (bool synchronous = false)
		{
			Log.Debug ("Stop");
			Pause (synchronous);
		}

		public void Play (bool synchronous = false)
		{
			Log.Debug ("Play");
			if (StillImageLoaded) {
				ReconfigureTimeout (TIMEOUT_MS);
				EmitPlaybackStateChanged (this, true);
			} else {
				EmitLoadDrawings (null);
				if (pendingSeek != null) {
					pendingSeek.playing = true;
				} else {
					player.Play (synchronous);
				}
			}
			Playing = true;
		}

		public void Pause (bool synchronous = false)
		{
			Log.Debug ("Pause");
			if (StillImageLoaded) {
				ReconfigureTimeout (0);
				EmitPlaybackStateChanged (this, false);
			} else {
				if (pendingSeek != null) {
					pendingSeek.playing = false;
				} else {
					player.Pause (synchronous);
				}
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

		public bool Seek (Time time, bool accurate, bool synchronous = false, bool throttled = false)
		{
			Log.Debug (string.Format ("PlayerController::Seek (time: {0}, accurate: {1}, synchronous: {2}, throttled: {3}", time, accurate, synchronous, throttled));

			if (PresentationMode) {
				return PlaylistSeek (time, accurate, synchronous, throttled);
			} else {
				if (SegmentLoaded) {
					time += loadedSegment.Start;
					accurate = true;
					Log.Debug ("Segment loaded - seek accurate");
				}
				return AbsoluteSeek (time, accurate, synchronous, throttled);
			}
		}

		/// <summary>
		/// Seeks absolutely. This seek is the one that will go to the real player, made over the video file.
		/// </summary>
		/// <returns><c>true</c>, if seek was made correctly, <c>false</c> otherwise.</returns>
		/// <param name="time">Time in the video to seek.</param>
		/// <param name="accurate">If set to <c>true</c>, accurate seek.</param>
		/// <param name="synchronous">If set to <c>true</c>, synchronous seek.</param>
		/// <param name="throttled">If set to <c>true</c>, throttled seek.</param>
		bool AbsoluteSeek (Time time, bool accurate, bool synchronous = false, bool throttled = false)
		{
			if (StillImageLoaded) {
				imageLoadedTS = time;
				Tick ();
			} else {
				EmitLoadDrawings (null);
				if (readyToSeek) {
					if (throttled) {
						Log.Debug ("Throttled seek");
						seeker.Seek (accurate ? SeekType.Accurate : SeekType.Keyframe, time, (float)Rate);
					} else {
						Log.Debug (string.Format ("Seeking to {0} accurate:{1} synchronous:{2} throttled:{3}",
							time, accurate, synchronous, throttled));
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
						throttled = throttled
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
				seekPos = duration * pos;
				accurate = true;
				throthled = true;
			} else {
				seekPos = streamLength * pos;
				accurate = false;
				throthled = false;
			}
			Seek (seekPos, accurate, false, throthled);
		}

		bool PlaylistSeek (Time time, bool accurate = false, bool synchronous = false, bool throttled = false)
		{
			if (loadedPlaylistElement == null) {
				return AbsoluteSeek (time, accurate, synchronous, throttled);
			}

			// if time is outside the currently loaded event
			var elementTuple = LoadedPlaylist.GetElementAtTime (time);
			var elementAtTime = elementTuple.Item1;
			var elementStart = elementTuple.Item2;
			if (elementAtTime != loadedPlaylistElement || (elementStart > time || elementStart + elementAtTime.Duration < time)) {
				if (elementAtTime == null) {
					Log.Debug (String.Format ("There is no playlist element at {0}.", time));
					return false;
				}
				Config.EventsBroker.EmitPlaylistElementSelected (LoadedPlaylist, elementAtTime, false);
			}

			time -= elementStart;

			var play = loadedPlaylistElement as PlaylistPlayElement;
			if (play != null) {
				time += play.Play.Start;
				if (time > play.Play.FileSet.Duration) {
					Log.Warning (String.Format ("Attempted seek to {0}, which is longer than the fileSet", time));
					return false;
				}
			}
			Log.Debug (string.Format ("New time: {0}", time));

			return AbsoluteSeek (time, accurate, synchronous, throttled);
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
				EmitLoadDrawings (null);
				if (CurrentTime > loadedSegment.Start) {
					player.SeekToPreviousFrame ();
					Tick ();
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

		public void Switch (TimelineEvent play, Playlist playlist, IPlaylistElement element)
		{
			if (loadedPlaylistElement != null) {
				loadedPlaylistElement.Selected = false;
				var playElement = (loadedPlaylistElement as PlaylistPlayElement);
				if (playElement != null) {
					playElement.Play.Selected = false;
				}
			}
			if (loadedEvent != null) {
				loadedEvent.Selected = false;
			}

			loadedEvent = play;
			LoadedPlaylist = playlist;
			loadedPlaylistElement = element;

			if (element != null) {
				element.Selected = true;
				var playElement = (element as PlaylistPlayElement);
				if (playElement != null) {
					playElement.Play.Selected = true;
				}
			}
			if (play != null) {
				play.Selected = true;
			}
		}

		public void LoadPlaylistEvent (Playlist playlist, IPlaylistElement element, bool playing)
		{
			Log.Debug (string.Format ("Loading playlist element \"{0}\"", element?.Description));

			if (!ready) {
				EmitPrepareViewEvent ();
				delayedOpen = () => LoadPlaylistEvent (playlist, element, playing);
				return;
			}

			if (playlist == null) {
				return;
			}

			Switch (null, playlist, element);

			if (element is PlaylistPlayElement) {
				PlaylistPlayElement ple = element as PlaylistPlayElement;
				LoadSegment (ple.Play.FileSet, ple.Play.Start, ple.Play.Stop,
					ple.Play.Start, ple.Rate, ple.CamerasConfig,
					ple.CamerasLayout, playing);
			} else if (element is PlaylistVideo) {
				LoadVideo (element as PlaylistVideo, playing);
			} else if (element is PlaylistImage) {
				LoadStillImage (element as PlaylistImage, playing);
			} else if (element is PlaylistDrawing) {
				LoadFrameDrawing (element as PlaylistDrawing, playing);
			}
			EmitElementLoaded (element, playlist.HasNext ());
		}

		public void LoadEvent (TimelineEvent evt, Time seekTime, bool playing)
		{
			MediaFileSet fileSet = evt.FileSet;
			Log.Debug (string.Format ("Loading event \"{0}\" seek:{1} playing:{2}", evt.Name, seekTime, playing));

			if (!ready) {
				EmitPrepareViewEvent ();
				delayedOpen = () => LoadEvent (evt, seekTime, playing);
				return;
			}

			Switch (evt, null, null);

			if (evt.Start != null && evt.Stop != null) {
				LoadSegment (fileSet, evt.Start, evt.Stop, evt.Start + seekTime, evt.Rate,
					evt.CamerasConfig, evt.CamerasLayout, playing);
				if (loadedEvent == null) { // LoadSegment sometimes removes the loadedEvent
					loadedEvent = evt;
				}
			} else if (evt.EventTime != null) {
				AbsoluteSeek (evt.EventTime, true);
			} else {
				Log.Error ("Event does not have timing info: " + evt);
			}
			EmitElementLoaded (evt, false);
		}

		public void UnloadCurrentEvent ()
		{
			Log.Debug ("Unload current event");
			Reset ();
			if (defaultFileSet != null && !defaultFileSet.Equals (FileSet)) {
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
			if (loadedPlaylistElement != null) {
				if (LoadedPlaylist.HasNext ()) {
					Config.EventsBroker.EmitPlaylistElementSelected (LoadedPlaylist, LoadedPlaylist.Next (), Playing);
				} else {
					Pause ();
					Seek (new Time (0), true);
				}
			} else {
				Pause ();
			}
		}

		public void Previous (bool force = false)
		{
			Log.Debug ("Previous");

			/* Select the start of the element if it's a regular play */
			if (loadedEvent != null) {
				Seek (new Time (0), true);
			} else if (loadedPlaylistElement != null) {
				/* Select the start of the element if we haven't played 500ms, unless forced */
				Time start = new Time (0);
				if (loadedPlaylistElement is PlaylistPlayElement) {
					start = (loadedPlaylistElement as PlaylistPlayElement).Play.Start;
				}
				if (!force && (CurrentTime - start).MSeconds > 500) {
					Config.EventsBroker.EmitPlaylistElementSelected (LoadedPlaylist, loadedPlaylistElement, Playing);
					return;
				}
				if (LoadedPlaylist.HasPrev ()) {
					Config.EventsBroker.EmitPlaylistElementSelected (LoadedPlaylist, LoadedPlaylist.Prev (), Playing);
				}
			} else {
				Seek (new Time (0), true);
			}
		}

		public void ApplyROI (CameraConfig camConfig)
		{
			camerasConfig [camConfig.Index] = camConfig;
			if (multiPlayer != null) {
				multiPlayer.ApplyROI (camConfig);
			}
		}

		public void DrawFrame ()
		{
			TimelineEvent evt = loadedEvent;
			if (evt == null && loadedPlaylistElement is PlaylistPlayElement) {
				evt = (loadedPlaylistElement as PlaylistPlayElement).Play;
			}
			if (evt != null) {
				Config.EventsBroker.EmitDrawFrame (evt, -1, CamerasConfig [0], true);
			} else {
				Config.EventsBroker.EmitDrawFrame (null, -1, null, true);
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
				TimeChangedEvent (currentTime, duration ?? currentTime, !StillImageLoaded);
			}
		}

		void EmitPlaybackStateChanged (object sender, bool playing)
		{
			if (PlaybackStateChangedEvent != null && !disposed) {
				PlaybackStateChangedEvent (sender, playing);
				Config.EventsBroker.EmitPlaybackStateChanged (sender, playing);
			}
		}

		void EmitMediaFileSetLoaded (MediaFileSet fileSet, ObservableCollection<CameraConfig> camerasVisible)
		{
			if (MediaFileSetLoadedEvent != null && !disposed) {
				MediaFileSetLoadedEvent (fileSet, camerasVisible);
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
					EmitPlaybackStateChanged (this, Playing);
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
		ObservableCollection<FrameDrawing> EventDrawings {
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
		void UpdateCamerasConfig (ObservableCollection<CameraConfig> camerasConfig, object layout)
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
				UpdateCamerasConfig (
					new ObservableCollection<CameraConfig> (camerasConfig.Where (i => i.Index < FileSet.Count)), 
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

			if ((fileSet != null && (!fileSet.Equals (FileSet) || fileSet.CheckMediaFilesModified (mediaFileSetCopy))) || force) {
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
				} catch (Exception ex) {
					Log.Exception (ex);
					//We handle this error async
				}
			}
			if (seek) {
				AbsoluteSeek (new Time (0), true);
			}
			if (play) {
				Play ();
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
			player.Rate = rate;

			SetEventRate (rate);
			EmitRateChanged (rate);
		}

		/// <summary>
		/// Sets the event rate.
		/// </summary>
		/// <param name="rate">Rate.</param>
		void SetEventRate (float rate)
		{
			if (loadedPlaylistElement is PlaylistPlayElement) {
				(loadedPlaylistElement as PlaylistPlayElement).Rate = rate;
			} else if (loadedEvent != null) {
				loadedEvent.Rate = rate;
			}
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
		                  float rate, ObservableCollection<CameraConfig> camerasConfig, object camerasLayout,
		                  bool playing)
		{
			Log.Debug (String.Format ("Update player segment {0} {1} {2}",
				start, stop, rate));
			if (!SegmentLoaded) {
				defaultCamerasConfig = CamerasConfig;
				defaultCamerasLayout = CamerasLayout;
			}

			UpdateCamerasConfig (camerasConfig, camerasLayout);

			if (fileSet != null && (!fileSet.Equals (mediafileSet) || fileSet.CheckMediaFilesModified (mediaFileSetCopy))) {
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
				AbsoluteSeek (seekTime, true);
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

		void LoadStillImage (PlaylistImage image, bool playing)
		{
			Reset ();
			loadedPlaylistElement = image;
			StillImageLoaded = true;
			if (playing) {
				Play ();
			}
		}

		void LoadFrameDrawing (PlaylistDrawing drawing, bool playing)
		{
			loadedPlaylistElement = drawing;
			StillImageLoaded = true;
			if (playing) {
				Play ();
			}
		}

		void LoadVideo (PlaylistVideo video, bool playing)
		{
			loadedPlaylistElement = video;
			MediaFileSet fileSet = new MediaFileSet ();
			fileSet.Add (video.File);
			EmitLoadDrawings (null);
			UpdateCamerasConfig (new ObservableCollection<CameraConfig> { new CameraConfig (0) }, null);
			InternalOpen (fileSet, true, true, playing);
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
			AbsoluteSeek (pos, true);
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
				Time relativeTime = imageLoadedTS;
				Time duration = loadedPlaylistElement.Duration;

				if (PresentationMode) {
					relativeTime += LoadedPlaylist.GetCurrentStartTime ();
					duration = LoadedPlaylist.Duration;
				}

				EmitTimeChanged (relativeTime, duration);

				if (imageLoadedTS >= loadedPlaylistElement.Duration) {
					Next ();
				} else {
					if (Playing) {
						imageLoadedTS.MSeconds += TIMEOUT_MS;
					}
				}
				return true;
			} else {
				Time currentTime = CurrentTime;
				Time relativeTime = currentTime;
				Time duration = null;
				if (PresentationMode) {
					relativeTime += LoadedPlaylist.GetCurrentStartTime ();
					duration = LoadedPlaylist.Duration;
				}

				if (SegmentLoaded) {
					relativeTime -= loadedSegment.Start;
					if (duration == null) {
						duration = loadedSegment.Stop - loadedSegment.Start;
					}

					EmitTimeChanged (relativeTime, duration);

					if (currentTime > loadedSegment.Stop) {
						/* Check if the segment is now finished and jump to next one */
						Next ();
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
					if (duration == null) {
						duration = streamLength;
					}
					EmitTimeChanged (relativeTime, duration);
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
				if (!StillImageLoaded) {
					EmitPlaybackStateChanged (this, playing);
				}
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
					var playing = pendingSeek.playing;
					pendingSeek = null;
					if (playing) {
						Play ();
					}
				}
				Tick ();
				player.Expose ();
			});
		}

		void HandleEndOfStream (object sender)
		{
			Config.GUIToolkit.Invoke (delegate {
				if (loadedPlaylistElement is PlaylistVideo) {
					Next ();
				} else {
					Time position = null;
					if (loadedEvent != null) {
						Log.Debug ("Seeking back to event start");
						position = loadedEvent.Start;
					} else {
						Log.Debug ("Seeking back to 0");
						position = new Time (0);
					}
					AbsoluteSeek (position, true);
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
			if (!visible) {
				ViewPorts [index].Message = Catalog.GetString ("Out of scope");
			}
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
					AbsoluteSeek (start, type == SeekType.Accurate, false, false);
				}
			});
		}

		#endregion
	}
}
