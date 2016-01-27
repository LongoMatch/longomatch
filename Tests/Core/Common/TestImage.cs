//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System;
using System.IO;
using LongoMatch.Core.Common;
using NUnit.Framework;

namespace Tests.Core.Common
{
	[TestFixture()]
	public class TestImage
	{
		Image img;
		
		
		[SetUp()]
		public void LoadImageFromFile ()
		{
			img = Utils.LoadImageFromFile (false);
		}

		[Test()]
		public void TestSerialization ()
		{
			string dir = Environment.CurrentDirectory;
			Utils.CheckSerialization (img);
		}
		
		[Test()]
		public void TestLoadFromFile ()
		{
			Assert.AreEqual (img.Width, 16);
			Assert.AreEqual (img.Height, 16);
			img = Utils.LoadImageFromFile (true);
			Assert.AreEqual (img.Width, 20);
			Assert.AreEqual (img.Height, 20);
		}
		
		[Test()]
		public void TestSerialize ()
		{
			byte[] buf = img.Serialize ();
			Assert.AreEqual (buf.Length, 102);
			img = Image.Deserialize (buf);
			Assert.AreEqual (img.Width, 16);
			Assert.AreEqual (img.Height, 16);
		}
		
		[Test()]
		public void TestScale ()
		{
			Image img2 = img.Scale (20, 20);
			Assert.AreNotSame (img, img2); 
			Assert.AreEqual (img2.Width, 20);
			Assert.AreEqual (img2.Height, 20);
			
			img = img.Scale (20, 30);
			Assert.AreEqual (img.Width, 20);
			Assert.AreEqual (img.Height, 20);
			img = img.Scale (25, 20);
			Assert.AreEqual (img.Width, 20);
			Assert.AreEqual (img.Height, 20);
			
			img.ScaleInplace ();
			Assert.AreEqual (img.Width, Constants.MAX_THUMBNAIL_SIZE);
			Assert.AreEqual (img.Height, Constants.MAX_THUMBNAIL_SIZE);
		}
		
		[Test()]
		public void TestSave ()
		{
			string tmpFile = Path.GetTempFileName ();
			try {
				img.Save (tmpFile);
				Image img2 = new Image (tmpFile);
				Assert.AreEqual (img2.Width, 16);
				Assert.AreEqual (img2.Height, 16);
			} finally {
				File.Delete (tmpFile);
			}
		}
		
		[Test()]
		public void TestComposite ()
		{
			Image img2 = Utils.LoadImageFromFile (true);
			Image img3 = img2.Composite (img);
			Assert.AreEqual (img3.Width, 20);
			Assert.AreEqual (img3.Height, 20);
			
		}

		void AssertImageScale (int imgWidth, int imgHeight, int reqWidth, int reqHeight, ScaleMode mode)
		{
			int outWidth, outHeight;
			Image.ComputeScale (imgWidth, imgHeight, reqWidth, reqHeight, mode, out outWidth,
				out outHeight);

			Assert.IsTrue (((double)imgWidth / imgHeight) - ((double)outWidth / outHeight) < 0.01);
			if (mode == ScaleMode.AspectFit) {
				Assert.IsTrue (outWidth <= reqWidth);
				Assert.IsTrue (outHeight <= reqHeight);
			} else if (mode == ScaleMode.AspectFill) {
				Assert.IsTrue (outWidth >= reqWidth);
				Assert.IsTrue (outHeight >= reqHeight);
			}
		}

		[Test]
		public void TestComputeScale ()
		{
			AssertImageScale (320, 240, 200, 100, ScaleMode.AspectFit);
			AssertImageScale (320, 240, 500, 400, ScaleMode.AspectFit);
			AssertImageScale (320, 240, 100, 200, ScaleMode.AspectFit);
			AssertImageScale (320, 240, 500, 300, ScaleMode.AspectFit);

			AssertImageScale (320, 240, 200, 100, ScaleMode.AspectFill);
			AssertImageScale (320, 240, 500, 400, ScaleMode.AspectFill);
			AssertImageScale (320, 240, 100, 200, ScaleMode.AspectFill);
			AssertImageScale (320, 240, 500, 300, ScaleMode.AspectFill);
		}

		[Test]
		public void TestScaleFactor ()
		{
			Point offset;
			double scaleX, scaleY;

			// Output is 133x100
			Image.ScaleFactor (320, 240, 200, 100, ScaleMode.AspectFit, out scaleX, out scaleY, out offset);
			Assert.IsTrue ((scaleX - (double)320 / 133) < 0.1);
			Assert.IsTrue ((scaleY - (double)240 / 100) < 0.1);
			Assert.AreEqual (33, offset.X);
			Assert.AreEqual (0, offset.Y);

			// Output is 200x150
			Image.ScaleFactor (320, 240, 200, 100, ScaleMode.AspectFill, out scaleX, out scaleY, out offset);
			Assert.IsTrue ((scaleX - (double)320 / 200) < 0.1);
			Assert.IsTrue ((scaleY - (double)240 / 150) < 0.1);
			Assert.AreEqual (0, offset.X);
			Assert.AreEqual (-25, offset.Y);
		}
	}
}