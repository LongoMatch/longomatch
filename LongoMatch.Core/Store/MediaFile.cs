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
using Mono.Unix;

using LongoMatch.Core.Common;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{

	[Serializable]
	public class MediaFile
	{
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
			FilePath = filePath;
			Duration = new Time ((int)length);
			HasAudio = hasAudio;
			HasVideo = hasVideo;
			Container = container;
			VideoCodec = videoCodec;
			AudioCodec = audioCodec;
			VideoHeight = videoHeight;
			VideoWidth = videoWidth;
			Fps = fps;
			Preview = preview;
			Par = par;
		}

		public string FilePath {
			get;
			set;
		}

		public Time Duration {
			get;
			set;
		}

		public bool HasVideo {
			get;
			set;
		}

		public bool HasAudio {
			get;
			set;
		}
		
		public string Container {
			get;
			set;
		}

		public string VideoCodec {
			get;
			set;
		}

		public string AudioCodec {
			get;
			set;
		}

		public uint VideoWidth {
			get;
			set;
		}

		public uint VideoHeight {
			get;
			set;
		}

		public ushort Fps {
			get;
			set;
		}
		
		public double Par {
			get;
			set;
		}
		
		public Image Preview {
			get;
			set;
		}
		
		[JsonIgnore]
		public string Description {
			get {
				if (FilePath == Constants.FAKE_PROJECT) {
					return Catalog.GetString ("No video file associated");
				} else {
					string desc = String.Format ("<b>File path</b>: {0}\n", FilePath);
					desc += String.Format ("<b>Format</b>: {0}x{1} @ {2}fps\n", VideoWidth,
					                       VideoHeight, Fps);
					desc += String.Format ("<b>Duration</b>: {0}\n", Duration.ToSecondsString ());
					desc += String.Format ("<b>Video Codec</b>: {0}\n", VideoCodec);
					desc += String.Format ("<b>Audio Codec</b>: {0}\n", AudioCodec);
					desc += String.Format ("<b>Container</b>: {0}\n", Container);
					return desc;
				}
			}
		}
	}
}
