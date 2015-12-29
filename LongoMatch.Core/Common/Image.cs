// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Runtime.Serialization;
using System;
using Gdk;

namespace LongoMatch.Core.Common
{

	[Serializable]
	public class Image: BaseImage<Pixbuf>
	{

		public Image (int width, int height)
		{
			Value = new Pixbuf (Colorspace.Rgb, true, 32, width, height);
		}

		public Image (Pixbuf image) : base (image)
		{
		}

		public Image (string filepath) : base (filepath)
		{
		}

		// this constructor is automatically called during deserialization
		public Image (SerializationInfo info, StreamingContext context)
		{
			try {
				Value = Deserialize ((byte[])info.GetValue (BUF_PROPERTY, typeof(byte[]))).Value;
			} catch {
				Value = null;
			}
		}

		protected override Pixbuf LoadFromFile (string filepath)
		{
			return new Pixbuf (filepath);
		}

		public override byte[] Serialize ()
		{
			if (Value == null)
				return null;
			return Value.SaveToBuffer ("png");
		}

		public static Image Deserialize (byte[] ser)
		{
			return new Image (new Pixbuf (ser));
		}

		public override Image Scale (int maxWidth, int maxHeight)
		{
			return new Image (Scale (Value, maxWidth, maxHeight));
		}

		public override IntPtr LockPixels ()
		{
			return Value.Pixels;
		}

		public override void UnlockPixels (IntPtr pixels)
		{
		}

		protected override Pixbuf Scale (Pixbuf pix, int maxWidth, int maxHeight)
		{
			int width, height;
			
			ComputeScale (pix.Width, pix.Height, maxWidth, maxHeight, ScaleMode.AspectFit, out width, out height);
			return pix.ScaleSimple (width, height, Gdk.InterpType.Bilinear);	
		}

		public override void Save (string filename)
		{
			Value.Save (filename, "png");
		}

		protected override int GetWidth ()
		{
			return Value.Width;
		}

		protected override int GetHeight ()
		{
			return Value.Height;
		}

		public override Image Composite (Image image)
		{
			Pixbuf dest = new Pixbuf (Value.Colorspace, true, Value.BitsPerSample, Width, Height);
			Value.Composite (dest, 0, 0, Width, Height, 0, 0, 1, 1,
				Gdk.InterpType.Bilinear, 255);
			image.Value.Composite (dest, 0, 0, image.Width, image.Height, 0, 0, 1, 1,
				Gdk.InterpType.Bilinear, 255);
			return new Image (dest);
		}
		
	}
}
