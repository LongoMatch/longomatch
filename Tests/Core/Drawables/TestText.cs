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
using NUnit.Framework;
using LongoMatch.Core.Store.Drawables;
using LongoMatch.Core.Common;
using Text = LongoMatch.Core.Store.Drawables.Text;


namespace Tests.Core.Drawables
{
	[TestFixture()]
	public class TestText
	{
		[Test()]
		public void TestSerialization ()
		{
			Point o = new Point (10, 10);
			Text t = new Text (o, 10, 10, "TEST");
			Utils.CheckSerialization (t);
			Text nt = Utils.SerializeDeserialize (t);
			Assert.AreEqual ("TEST", nt.Value);
			
			Rectangle r = new Rectangle (o, 10, 10);
			Assert.AreEqual (r.TopLeft, t.TopLeft);
			Assert.AreEqual (r.TopRight, t.TopRight);
			Assert.AreEqual (r.BottomLeft, t.BottomLeft);
			Assert.AreEqual (r.BottomRight, t.BottomRight);
		}
	}
}

