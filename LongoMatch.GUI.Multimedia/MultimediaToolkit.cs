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

using Remuxer = LongoMatch.Video.Utils;

namespace LongoMatch.Video
{

	public class MultimediaToolkit:MultimediaFactory, IMultimediaToolkit
	{
		public string RemuxFile (MediaFile file, object window) {
			LongoMatch.Video.Utils.Remuxer remuxer = new LongoMatch.Video.Utils.Remuxer (file);
			return remuxer.Remux (window as Gtk.Window);
		}
	}
}
