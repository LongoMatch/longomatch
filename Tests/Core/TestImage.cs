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

namespace Tests.Core
{
	[TestFixture()]
	public class TestImage
	{
		Image img;
		
		Image LoadFromFile (bool scaled = false)
		{
			Image img = null;
			string tmpFile = Path.GetTempFileName ();

			using (Stream resource = GetType().Assembly.GetManifestResourceStream("dibujo.svg")) {
				using (Stream output = File.OpenWrite(tmpFile)) {
					resource.CopyTo (output);
				}
			}
			try {
				if (!scaled) {
					img = Image.LoadFromFile (tmpFile);
				} else {
					img = Image.LoadFromFile (tmpFile, 20, 20);
				}
			} catch (Exception ex) {
				Assert.Fail (ex.Message);
			} finally {
				File.Delete (tmpFile);
			}
			return img;
		}

		[SetUp()]
		public void LoadFromFile ()
		{
			img = LoadFromFile (false);
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
			img = LoadFromFile (true);
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
				Image img2 = Image.LoadFromFile (tmpFile);
				Assert.AreEqual (img2.Width, 16);
				Assert.AreEqual (img2.Height, 16);
			} finally {
				File.Delete (tmpFile);
			}
		}
		
		[Test()]
		public void TestComposite ()
		{
			Image img2 = LoadFromFile (true);
			Image img3 = Image.Composite (img2, img);
			Assert.AreEqual (img3.Width, 20);
			Assert.AreEqual (img3.Height, 20);
			
		}
	}
}