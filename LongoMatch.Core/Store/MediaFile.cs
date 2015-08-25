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
using System.IO;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class MediaFile: IChanged
	{
		public MediaFile ()
		{
			Offset = new Time (0);
		}

		public MediaFile (string filePath,
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
		                  Image preview,
		                  String name)
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
			Offset = new Time (0);
			Name = name;
		}

		[JsonIgnore]
		public bool IsChanged {
			get;
			set;
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

		public Time Offset {
			get;
			set;
		}

		public String Name {
			get;
			set;
		}

		[JsonIgnore]
		public bool IsFakeCapture {
			get {
				return FilePath == Constants.FAKE_PROJECT;
			}
		}

		[JsonIgnore]
		public string ShortDescription {
			get {
				return String.Format ("{0}x{1}@{2}fps", VideoWidth, VideoHeight, Fps);
			}
		}

		public override string ToString ()
		{
			return string.Format ("{0} {1}", Path.GetFileName (FilePath), ShortDescription);
		}

		public bool Exists ()
		{
			return File.Exists (FilePath);
		}
	}
}
