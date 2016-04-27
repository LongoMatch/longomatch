//
//  Copyright (C) 2015 Andoni Morales Alastruey
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
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using NUnit.Framework;
using VAS.Core.Common;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestPenaltyCard
	{
		[Test ()]
		public void TestSerialization ()
		{
			PenaltyCard pc = new PenaltyCard ();
			pc.Color = Color.Red;
			pc.Name = "test";
			pc.Shape = CardShape.Circle;
			
			Utils.CheckSerialization (pc);
			
			PenaltyCard pc2 = Utils.SerializeDeserialize (pc);
			Assert.AreEqual (pc.Name, pc2.Name);
			Assert.AreEqual (pc.Color, pc2.Color);
			Assert.AreEqual (pc.Shape, pc2.Shape);
		}
	}
}

