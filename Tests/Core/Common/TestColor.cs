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
using LongoMatch.Core.Common;
using NUnit.Framework;
using VAS.Core.Common;

namespace Tests.Core.Common
{
	[TestFixture ()]
	public class TestColor
	{
		[Test ()]
		public void TestSerialization ()
		{
			Color c = new Color (255, 10, 255, 10);
			
			Utils.CheckSerialization (c);
			Color c1 = Utils.SerializeDeserialize (c);
			Assert.AreEqual (c, c1);
			
			YCbCrColor yc = new YCbCrColor (2, 3, 4);
			Utils.CheckSerialization (yc, true);
		}

		[Test ()]
		public void TestParse ()
		{
			Color c;
			
			Assert.IsNull (Color.Parse ("232424"));
			Assert.IsNull (Color.Parse ("#abc"));
			Assert.IsNull (Color.Parse ("#2324"));
			Assert.IsNull (Color.Parse ("#2324242434"));
			Assert.IsNull (Color.Parse ("#1020AZ"));
			
			c = Color.Parse ("#af1023");
			Assert.AreEqual (c.R, 175);
			Assert.AreEqual (c.G, 16);
			Assert.AreEqual (c.B, 35);
			Assert.AreEqual (c.A, Byte.MaxValue);
			
			c = Color.Parse ("#af1023aa");
			Assert.AreEqual (c.R, 175);
			Assert.AreEqual (c.G, 16);
			Assert.AreEqual (c.B, 35);
			Assert.AreEqual (c.A, 170);
		}

		[Test ()]
		public void TestToString ()
		{
			Color c = Color.Parse ("#af1023aa");

			String s = c.ToString ();
			Assert.AreEqual (s, "#AF1023AA");

			s = c.ToRGBString (false);
			Assert.AreEqual (s, "#AF1023");
		}

		[Test ()]
		public void TestCopy ()
		{
			Color c1 = new Color (100, 200, 240);
			Color c2 = c1.Copy ();
			Assert.IsFalse (Object.ReferenceEquals (c1, c2));
			Assert.AreEqual (c1, c2);
		}

		[Test ()]
		public void TestEquals ()
		{
			Color c1 = new Color (100, 200, 240);
			Color c2 = new Color (100, 200, 240);
			Assert.AreEqual (c1, c2);
		}

		[Test ()]
		public void TestYCbCr ()
		{
			Color c1 = new Color (100, 100, 100);
			YCbCrColor yc = YCbCrColor.YCbCrFromColor (c1);
			Assert.AreEqual (yc.Y, 101);
			Assert.AreEqual (yc.Cb, 128);
			Assert.AreEqual (yc.Cr, 125);
			Color c2 = yc.RGBColor ();
			/* Conversions is not 1-1 */
			Assert.AreNotEqual (c1, c2);
			Assert.AreEqual (c2.R, 94);
			Assert.AreEqual (c2.G, 101);
			Assert.AreEqual (c2.B, 98);
			
		}
	}
}

