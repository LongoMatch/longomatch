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
using LongoMatch.Video.Utils;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using Mono.Unix;
using System.IO;

namespace LongoMatch.Video
{

	public class MultimediaToolkit:MultimediaFactory, IMultimediaToolkit
	{
		public string RemuxFile (MediaFile file, object window) {
			string outputFile = Config.GUIToolkit.SaveFile (Catalog.GetString ("Output file"),
			                                                Path.ChangeExtension (file.FilePath, ".mp4"),
			                                                Path.GetDirectoryName (file.FilePath),
			                                                "MP4 (.mp4)", new string[] { ".mp4"});
			outputFile = Path.ChangeExtension (outputFile, ".mp4");
			Utils.Remuxer remuxer = new Utils.Remuxer (file, outputFile, VideoMuxerType.Mp4);
			return remuxer.Remux (window as Gtk.Window);
		}
	}
}
