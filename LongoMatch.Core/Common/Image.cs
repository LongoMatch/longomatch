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

namespace LongoMatch.Common
{
	using System;
	using System.IO;
#if HAVE_GTK
	using SImage = Gdk.Pixbuf;
#else
	using System.Drawing.Imaging;
	using SImage = System.Drawing.Image;
#endif

	public class Image
	{
		SImage image;
		
		public Image (SImage image)
		{
			this.image = image;
		}
		
		public SImage Value {
			get {
				return image;
			}
		}
		
		public void Dispose() {
			image.Dispose();
		}
		
		public void Scale() {
			Scale (Constants.THUMBNAIL_MAX_WIDTH, Constants.THUMBNAIL_MAX_HEIGHT);
		}
		
		
#if HAVE_GTK
		public byte[] Serialize () {
			if (image == null)
				return null;
			return image.SaveToBuffer("png");
		}
		
		public static Image Deserialize (byte[] ser) {
			return new Image(new SImage(ser));
		}
		
		public void Scale(int maxWidth, int maxHeight) {
			SImage scalled;
			int width, height;
			
			ComputeScale(image.Width, image.Height, maxWidth, maxHeight, out width, out height);
			scalled= image.ScaleSimple(width, height, Gdk.InterpType.Bilinear);	
			image.Dispose();
			image = scalled;
		}
		
		public void Save (string filename) {
			image.Save(filename, "png");
		}
		
		public int Width {
			get {
				return image.Width;
			}
		}
		
		public int Height {
			get {
				return image.Height;
			}
		}
		
#else
		public byte[] Serialize () {
			if (image == null)
				return null;
			using (MemoryStream stream = new MemoryStream()) {
				image.Save(stream, ImageFormat.Png);
				byte[] buf = new byte[stream.Length - 1];
				stream.Position = 0;
				stream.Read(buf, 0, buf.Length);
				return buf;
			}
		}
		
		public void Scale(int maxWidth, int maxHeight) {
			SImage scalled;
			int width, height;
			
			ComputeScale(image.Width, image.Height, maxWidth, maxHeight, out width, out height);
			scalled = image.GetThumbnailImage(width, height, new SImage.GetThumbnailImageAbort(ThumbnailAbort), IntPtr.Zero);
			image.Dispose();
			image = scalled;
		}
		
		public static Image Deserialize (byte[] ser) {
			Image img = null;
			using (MemoryStream stream = new MemoryStream(ser)) {
				img = new Image(System.Drawing.Image.FromStream(stream));
			}
			return img;
		}
		
		public void Save (string filename) {
			image.Save(filename, ImageFormat.Png);
		}
		
		bool ThumbnailAbort () {
			return false;
		}
#endif

		private void ComputeScale (int inWidth, int inHeight, int maxOutWidth, int maxOutHeight, out int outWidth, out int outHeight)
		{
			outWidth = maxOutWidth;
			outHeight = maxOutHeight;

			if(inWidth > maxOutWidth || inHeight > maxOutHeight) {
				double par = (double)inWidth /(double)inHeight;
				
				if(inHeight>inWidth)
					outWidth = (int)(outHeight * par);
				else
					outHeight = (int)(outWidth / par);
			}
		} 
	}
}

