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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;

namespace Tests.Core
{
	[TestFixture()]
	public class TestTimeNode
	{
		[Test()]
		public void TestSerialization ()
		{
			TimeNode tn = new TimeNode();
			
			Utils.CheckSerialization (tn);
			
			tn.Start = new Time (1000);
			tn.Stop = new Time (2000);
			tn.Name = "Test";
			tn.Rate = 2.0f;
			
			Utils.CheckSerialization (tn);
			
			TimeNode newtn = Utils.SerializeDeserialize (tn);
			Assert.AreEqual (tn.Start, newtn.Start);
			Assert.AreEqual (tn.Stop, newtn.Stop);
			Assert.AreEqual (tn.Name, newtn.Name);
			Assert.AreEqual (tn.Rate, newtn.Rate);
		}
		
		[Test()]
		public void TestDuration ()
		{
			TimeNode tn = new TimeNode();
			tn.Start = new Time (1000);
			tn.Stop = new Time (2000);
			Assert.AreEqual (tn.Duration, tn.Stop - tn.Start);
		}
	}
}

