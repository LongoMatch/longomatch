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
using Newtonsoft.Json;

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

	[Serializable]
	[JsonConverter (typeof(VASConverter))]
	public class Image: ISerializable, IDisposable
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
			Scale (Constants.MAX_THUMBNAIL_SIZE, Constants.MAX_THUMBNAIL_SIZE);
		}
		
		public void ScaleFactor (int destWidth, int destHeight,
		                         out double scaleX, out double scaleY,
		                         out Point offset) {
			int oWidth = 0;
			int oHeight = 0;
			
			ComputeScale (Width, Height, destWidth, destHeight, out oWidth, out oHeight);
			scaleX = (double) oWidth / Width;
			scaleY = (double) oHeight / Height;
			offset = new Point ((destWidth - oWidth) / 2, (destHeight - oHeight) / 2);
		}
		
		// this constructor is automatically called during deserialization
		public Image (SerializationInfo info, StreamingContext context) {
			try {
				image = Deserialize ((byte[]) info.GetValue ("pngbuf", typeof (byte[]))).Value;
			} catch {
				image = null;
			}
		}

		// this method is automatically called during serialization
		public void GetObjectData(SerializationInfo info, StreamingContext context) {
			try {
				info.AddValue("pngbuf", Serialize());
			} catch  {
				info.AddValue("pngbuf", null);
			}
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
		
		public static Image Composite(Image image1, Image image2) {
			SImage dest = new SImage(image1.Value.Colorspace, true, image1.Value.BitsPerSample,
			                         image1.Width, image1.Height);
			image1.Value.Composite(dest, 0, 0, image2.Width, image2.Height, 0, 0, 1, 1,
			                       Gdk.InterpType.Bilinear, 255);
			image2.Value.Composite(dest, 0, 0, image2.Width, image2.Height, 0, 0, 1, 1,
			                       Gdk.InterpType.Bilinear, 255);
			return new Image(dest);
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

			double par = (double)inWidth /(double)inHeight;
			double outPar = (double)maxOutWidth /(double)maxOutHeight;
				
			if (outPar > par) {
				outWidth = Math.Min (maxOutWidth, (int)(outHeight * par));
			} else {
				outHeight = Math.Min (maxOutHeight, (int)(outWidth / par));
			}
		} 
	}
}

