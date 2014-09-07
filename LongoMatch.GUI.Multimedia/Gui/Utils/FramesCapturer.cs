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
using System.Threading;

using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Video;
using LongoMatch.Video.Common;
using LongoMatch.Core.Store;
using LongoMatch.Multimedia.Utils;
using Gtk;

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
		private const int THUMBNAIL_MAX_HEIGHT=250;
		private const int THUMBNAIL_MAX_WIDTH=300;

		public event LongoMatch.Core.Handlers.FramesProgressHandler Progress;

		public FramesSeriesCapturer(string videoFile, Time start, Time stop, uint interval, string outputDir)
		{
			MultimediaFactory mf= new MultimediaFactory();
			this.capturer=mf.GetFramesCapturer();
			this.capturer.Open(videoFile);
			this.start= start;
			this.stop = stop;
			this.interval = interval;
			this.outputDir = outputDir;
			this.seriesName = System.IO.Path.GetFileName(outputDir);
			this.totalFrames = (int)Math.Floor((double)((stop - start).MSeconds / interval))+1;
		}

		public void Cancel() {
			cancel = true;
		}

		public void Start() {
			Thread thread = new Thread(new ThreadStart(CaptureFrames));
			thread.Start();
		}

		public void CaptureFrames() {
			Time pos;
			LongoMatch.Core.Common.Image frame;
			int i = 0;

			System.IO.Directory.CreateDirectory(outputDir);

			pos = new Time {MSeconds = start.MSeconds};
			if(Progress != null) {
				Application.Invoke (delegate {
					Progress(0,totalFrames,null);
				});
			}

			while(pos <= stop) {
				if(!cancel) {
					frame = capturer.GetFrame(pos, true);
					if(frame != null) {
						frame.Save(System.IO.Path.Combine(outputDir,seriesName+"_" + i +".png"));
						frame.ScaleInplace(THUMBNAIL_MAX_WIDTH, THUMBNAIL_MAX_HEIGHT);
					}

					if(Progress != null) {
						Application.Invoke (delegate {
							Progress(i+1, totalFrames, frame);
						});
					}
					pos.MSeconds += (int) interval;
					i++;
				}
				else {
					System.IO.Directory.Delete(outputDir,true);
					cancel=false;
					break;
				}
			}
			capturer.Dispose ();
		}
	}
}