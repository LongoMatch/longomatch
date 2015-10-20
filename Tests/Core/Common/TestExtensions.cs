//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Generic;
using LongoMatch.Core.Common;
using NUnit.Framework;

namespace Tests.Core.Common
{
	[TestFixture ()]
	public class TestExtensions
	{
		[Test ()]
		public void TestSequenceEqualNoOrder ()
		{
			// Same lists
			List<string> l1 = new List<string> { "a", "b", "c" };
			List<string> l2 = new List<string> { "a", "b", "c" };
			Assert.IsTrue (l1.SequenceEqualNoOrder (l2));
			// Same list, different order
			l2 = new List<string> { "b", "c", "a" };
			Assert.IsTrue (l1.SequenceEqualNoOrder (l2));
			// Different lists with same count
			l2 = new List<string> { "b", "d", "a" };
			Assert.IsFalse (l1.SequenceEqualNoOrder (l2));
			// Different lists with different count
			l2 = new List<string> { "b", "a" };
			Assert.IsFalse (l1.SequenceEqualNoOrder (l2));

			// Different lists with repeating elements in second list
			l2 = new List<string> { "a", "a", "a" };
			Assert.IsFalse (l1.SequenceEqualNoOrder (l2));

			// Different lists with repeating elements in first list
			l1 = new List<string> { "a", "a", "c" };
			l2 = new List<string> { "a", "b", "c" };
			Assert.IsFalse (l1.SequenceEqualNoOrder (l2));
		}
	}
}

