// MediaFile.cs
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
using LongoMatch.Core;

using LongoMatch.Common;
using Newtonsoft.Json;

namespace LongoMatch.Store
{

	[Serializable]
	public class MediaFile
	{

		string filePath;
		long length; // In MSeconds
		ushort fps;
		bool hasAudio;
		bool hasVideo;
		string container;
		string videoCodec;
		string audioCodec;
		uint videoHeight;
		uint videoWidth;
		double par;
		byte[] thumbnailBuf;


		public MediaFile() {}

		public MediaFile(string filePath,
		                 long length,
		                 ushort fps,
		                 bool hasAudio,
		                 bool hasVideo,
		                 string container,
		                 string videoCodec,
		                 string audioCodec,
		                 uint videoWidth,
		                 uint videoHeight, 
		                 double par,
		                 Image preview)
		{
			this.filePath = filePath;
			this.length = length;
			this.hasAudio = hasAudio;
			this.hasVideo = hasVideo;
			this.container = container;
			this.videoCodec = videoCodec;
			this.audioCodec = audioCodec;
			this.videoHeight = videoHeight;
			this.videoWidth = videoWidth;
			if(fps == 0)
				//For audio Files
				this.fps=25;
			else
				this.fps = fps;
			this.Preview = preview;
			Par = par;
		}

		public string FilePath {
			get {
				return this.filePath;
			}
			set {
				this.filePath = value;
			}
		}

		public Time Duration {
			get {
				return new Time ((int) length);
			}
		}

		public long Length {
			get {
				return this.length;
			}
			set {
				this.length = value;
			}
		}

		public bool HasVideo {
			get {
				return this.hasVideo;
			}
			set {
				this.hasVideo = value;
			}
		}

		public bool HasAudio {
			get {
				return this.hasAudio;
			}
			set {
				this.hasAudio = value;
			}
		}
		
		public string Container {
			get {
				return this.container;
			}
			set {
				this.container = value;
			}
		}

		public string VideoCodec {
			get {
				return this.videoCodec;
			}
			set {
				this.videoCodec = value;
			}
		}

		public string AudioCodec {
			get {
				return this.audioCodec;
			}
			set {
				this.audioCodec = value;
			}
		}

		public uint VideoWidth {
			get {
				return this.videoWidth;
			}
			set {
				this.videoWidth= value;
			}
		}

		public uint VideoHeight {
			get {
				return this.videoHeight;
			}
			set {
				this.videoHeight= value;
			}
		}

		public ushort Fps {
			get {
				return this.fps;
			}
			set {
				if(value == 0)
					//For audio Files
					this.fps=25;
				else
					this.fps = value;
			}
		}
		
		public double Par {
			get {
				if (par == 0)
					return 1;
				return par;
			}
			set {
				par = value;
			}
		}
		
		public Image Preview {
			get {
				if(thumbnailBuf != null)
					return Image.Deserialize(thumbnailBuf);
				return null;
			}
			set {
				if(value != null) {
					thumbnailBuf = value.Serialize();
				} else
					thumbnailBuf = null;
			}
		}
		
		[JsonIgnore]
		public string Description {
			get {
				string desc = String.Format ("<b>File path</b>: {0}\n", FilePath);
				desc += String.Format ("<b>Format</b>: {0}x{1} @ {2}fps\n", VideoWidth,
				                       VideoHeight, Fps);
				desc += String.Format ("<b>Duration</b>: {0}\n",
				                       new Time {MSeconds=(int)Length}.ToSecondsString());
				desc += String.Format ("<b>Video Codec</b>: {0}\n", VideoCodec);
				desc += String.Format ("<b>Audio Codec</b>: {0}\n", AudioCodec);
				desc += String.Format ("<b>Container</b>: {0}\n", Container);
				return desc;
			}
		}
		
		public uint GetFrames() {
			return (uint)(Fps*Length/1000);
		}
		
	}
}
