// PixbufTimeNode.cs
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
using Gdk;

namespace LongoMatch.TimeNodes
{

	/// <summary>
	/// I am the base class for all the video segments.
	/// I have a <see cref="Pixbuf"/> with a thumbnail of the video segment I represent
	/// </summary>
	[Serializable]
	public class PixbufTimeNode : TimeNode
	{
		private byte[] thumbnailBuf;
		private const int MAX_WIDTH=100;
		private const int MAX_HEIGHT=75;
		#region Contructors
		public PixbufTimeNode() {
		}

		/// <summary>
		/// Creates a new PixbufTimeNode object
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/> with my name
		/// </param>
		/// <param name="start">
		/// A <see cref="Time"/> with my start time
		/// </param>
		/// <param name="stop">
		/// A <see cref="Time"/> with my stop time
		/// </param>
		/// <param name="thumbnail">
		/// A <see cref="Pixbuf"/> with my preview
		/// </param>
		public PixbufTimeNode(string name, Time start, Time stop, Pixbuf thumbnail): base(name,start,stop)
		{
			if (thumbnail != null) {
				this.thumbnailBuf = thumbnail.SaveToBuffer("png");
				thumbnail.Dispose();
			}
			else thumbnailBuf = null;
		}
		#endregion

		#region Properties
		/// <value>
		/// Segment thumbnail
		/// </value>
		public Pixbuf Miniature {
			get {
				if (thumbnailBuf != null)
					return new Pixbuf(thumbnailBuf);
				else return null;
			}set {
				if (value != null)
					ScaleAndSave(value);
				else thumbnailBuf = null;
			}
		}

		#endregion

		private void ScaleAndSave(Pixbuf pixbuf) {
			int ow,oh,h,w;

			h = ow = pixbuf.Height;
			w = oh = pixbuf.Width;
			ow = MAX_WIDTH;
			oh = MAX_HEIGHT;

			if (w>MAX_WIDTH || h>MAX_HEIGHT) {
				double rate = (double)w/(double)h;
				if (h>w)
					ow = (int)(oh * rate);
				else
					oh = (int)(ow / rate);
				thumbnailBuf = pixbuf.ScaleSimple(ow,oh,Gdk.InterpType.Bilinear).SaveToBuffer("png");
				pixbuf.Dispose();
			}
			else thumbnailBuf =  pixbuf.SaveToBuffer("png");
		}
	}
}
