// PlayerMaker.cs
//
//  Copyright(C) 2007-2009 Andoni Morales Alastruey
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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System.IO;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Multimedia;
using VAS.Core.Store;
using VAS.Multimedia;
using VAS.Multimedia.Utils;

namespace LongoMatch.Video
{
	public class MultimediaToolkit : MultimediaFactory, IMultimediaToolkit
	{
		public string RemuxFile (MediaFile file, object window)
		{
			VideoMuxerType muxType = VideoMuxerType.Mp4;
			string ext = ".mp4";
			string ext_desc = "MP4 (.mp4)";

			if (file.VideoCodec == GStreamer.MPEG1_VIDEO || file.VideoCodec == GStreamer.MPEG2_VIDEO) {
				ext = ".mkv";
				ext_desc = "MKV (.mkv)";
				muxType = VideoMuxerType.Matroska;
			}

			string outputFile = Config.GUIToolkit.SaveFile (Catalog.GetString ("Output file"),
				                    Path.ChangeExtension (file.FilePath, ext),
				                    Path.GetDirectoryName (file.FilePath),
				                    ext_desc, new string[] { ext });
			outputFile = Path.ChangeExtension (outputFile, ext);
			Utils.Remuxer remuxer = new Utils.Remuxer (file, outputFile, muxType);
			return remuxer.Remux (window as Gtk.Window);
		}
	}
}
