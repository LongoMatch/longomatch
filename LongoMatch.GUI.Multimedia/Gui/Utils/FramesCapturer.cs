// FramesCapturer.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using System.Threading;
using Gtk;
using VAS.Core.Common;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Core.Handlers;

namespace LongoMatch.Video.Utils
{


	public class FramesSeriesCapturer
	{
		IFramesCapturer capturer;
		Time start;
		Time stop;
		uint interval;
		int totalFrames;
		string seriesName;
		string outputDir;
		bool cancel;
		TimelineEvent evt;
		MediaFileSet fileSet;
		private const int THUMBNAIL_MAX_HEIGHT = 250;
		private const int THUMBNAIL_MAX_WIDTH = 300;

		public event FramesProgressHandler Progress;

		public FramesSeriesCapturer (MediaFileSet fileSet, TimelineEvent evt, uint interval, string outputDir)
		{
			this.capturer = Config.MultimediaToolkit.GetFramesCapturer ();
			this.fileSet = fileSet;
			this.evt = evt;
			this.start = evt.Start;
			this.stop = evt.Stop;
			this.interval = interval;
			this.outputDir = outputDir;
			this.seriesName = System.IO.Path.GetFileName (outputDir);
			this.totalFrames = ((int)Math.Floor ((double)((stop - start).MSeconds / interval)) + 1) * evt.CamerasConfig.Count;
			this.cancel = false;
		}

		public void Cancel ()
		{
			cancel = true;
		}

		public void Start ()
		{
			Thread thread = new Thread (new ThreadStart (CaptureFrames));
			thread.Start ();
		}

		public void CaptureFrames ()
		{
			Time pos;
			VAS.Core.Common.Image frame;
			IList<CameraConfig> cameras;
			bool quit = false;
			int i = 0;
			int j = 0;

			System.IO.Directory.CreateDirectory (outputDir);

			Log.Debug ("Start frames series capture with interval: " + interval);
			Log.Debug ("Total frames to be captured: " + totalFrames);
			if (evt.CamerasConfig.Count == 0 || fileSet.Count == 1) {
				cameras = new List<CameraConfig> { new CameraConfig (0) };
			} else {
				cameras = evt.CamerasConfig;
			}

			foreach (CameraConfig cameraConfig in cameras) {
				MediaFile file;

				if (quit) {
					break;
				}
				Log.Debug ("Start frames series capture for angle " + cameraConfig.Index);
				try {
					file = fileSet [cameraConfig.Index];
				} catch (Exception ex) {
					Log.Exception (ex);
					Log.Error (string.Format ("Camera index {0} not found in fileset", cameraConfig.Index));
					continue;
				}
				capturer.Open (file.FilePath);
				pos = new Time { MSeconds = start.MSeconds };
				if (Progress != null) {
					Application.Invoke (delegate {
						Progress (i, totalFrames, null);
					});
				}
				
				j = 0;
				while (pos <= stop) {
					Log.Debug ("Capturing fame " + j);
					if (!cancel) {
						frame = capturer.GetFrame (pos + file.Offset, true);
						if (frame != null) {
							string path = String.Format ("{0}_angle{1}_{2}.png", seriesName, cameraConfig.Index, j);
							frame.Save (System.IO.Path.Combine (outputDir, path));
							frame.ScaleInplace (THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_HEIGHT);
						}
						
						if (Progress != null) {
							Application.Invoke (delegate {
								Progress (i + 1, totalFrames, frame);
							});
						}
						pos.MSeconds += (int)interval;
						i++;
						j++;
					} else {
						Log.Debug ("Capture cancelled, deleting output directory");
						System.IO.Directory.Delete (outputDir, true);
						cancel = false;
						quit = true;
						break;
					}
				}
			}
			capturer.Dispose ();
		}
	}
}