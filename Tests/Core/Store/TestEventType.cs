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
using System.Collections.Generic;
using System.IO;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Serialization;
using VAS.Core.Store;
using Constants = LongoMatch.Core.Common.Constants;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestEventType
	{
		[Test ()]
		public void TestPenaltyCardEventType ()
		{
			PenaltyCardEventType pc = new PenaltyCardEventType ();
			Utils.CheckSerialization (pc);
			
			Assert.AreNotEqual (pc.ID, Constants.PenaltyCardID);
			Assert.AreNotEqual (pc, new PenaltyCardEventType ());
		}

		[Test ()]
		public void TestScoreEventType ()
		{
			ScoreEventType score = new ScoreEventType ();
			Utils.CheckSerialization (score);
			
			Assert.AreNotEqual (score.ID, Constants.ScoreID);
			Assert.AreNotEqual (score, new ScoreEventType ());
		}

		[Test ()]
		public void TestSubstitutionEventType ()
		{
			SubstitutionEventType sub = new SubstitutionEventType ();
			Utils.CheckSerialization (sub);
			
			Assert.AreEqual (sub.ID, Constants.SubsID);
			Assert.AreEqual (sub, new SubstitutionEventType ());
		}
	}
}
