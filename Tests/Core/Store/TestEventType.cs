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
using System.Collections.Generic;
using System.IO;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using NUnit.Framework;
using Newtonsoft.Json;
using LongoMatch.Core.Interfaces;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestEventType
	{
		[Test ()]
		public void TestEvntType ()
		{
			string jsonString;
			EventType evType;
			MemoryStream stream;
			StreamReader reader;
			
			evType = new EventType();
			Utils.CheckSerialization (evType);

			evType.Color = new Color (255, 0, 0);
			evType.Name = "test";
			evType.SortMethod = SortMethodType.SortByDuration;
			evType.TagFieldPosition = true;
			evType.TagGoalPosition = true;
			evType.TagHalfFieldPosition = true;
			evType.FieldPositionIsDistance = true;
			evType.HalfFieldPositionIsDistance = false;
			
			Utils.CheckSerialization (evType);
			
			stream = new MemoryStream ();
			Serializer.Instance.Save (evType, stream, SerializationType.Json);
			stream.Seek (0, SeekOrigin.Begin);
			reader = new StreamReader (stream);
			jsonString = reader.ReadToEnd ();
			Assert.IsFalse (jsonString.Contains ("SortMethodString"));
			stream.Seek (0, SeekOrigin.Begin);
			EventType newEventType = Serializer.Instance.Load<EventType> (stream, SerializationType.Json);
			
			Assert.AreEqual (evType.ID, newEventType.ID);
			Assert.AreEqual (evType.Name, newEventType.Name);
			Assert.AreEqual (evType.SortMethod, newEventType.SortMethod);
			Assert.AreEqual (evType.TagFieldPosition, newEventType.TagFieldPosition);
			Assert.AreEqual (evType.TagGoalPosition, newEventType.TagGoalPosition);
			Assert.AreEqual (evType.TagHalfFieldPosition, newEventType.TagHalfFieldPosition);
			Assert.AreEqual (evType.FieldPositionIsDistance, newEventType.FieldPositionIsDistance);
			Assert.AreEqual (evType.HalfFieldPositionIsDistance, newEventType.HalfFieldPositionIsDistance);
			Assert.AreEqual (255, newEventType.Color.R);
			Assert.AreEqual (0, newEventType.Color.G);
			Assert.AreEqual (0, newEventType.Color.B);
		}

		[Test ()]
		public void TestAnalysisEventType ()
		{
			AnalysisEventType at = new AnalysisEventType ();
			Utils.CheckSerialization (at);
			
			Assert.IsNotNull (at.Tags);
			Assert.AreEqual (at.TagsByGroup.Count, 0); 
			at.Tags.Add (new Tag ("test1", "grp1"));
			at.Tags.Add (new Tag ("test2", "grp1"));
			at.Tags.Add (new Tag ("test3", "grp2"));
			
			var tbg = at.TagsByGroup;
			Assert.AreEqual (tbg.Count, 2);
			Assert.AreEqual (tbg ["grp1"].Count, 2);
			Assert.AreEqual (tbg ["grp2"].Count, 1);
		}

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

		[Test()]
		public void TestIsChanged (){
			EventType et = new EventType ();
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.Color = Color.Green;
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.FieldPositionIsDistance = true;
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.HalfFieldPositionIsDistance = true;
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.Name = "name";
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.SortMethod = SortMethodType.SortByStartTime;
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.TagFieldPosition = true;
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.TagGoalPosition = true;
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;
			et.TagHalfFieldPosition = true;
			Assert.IsTrue (et.IsChanged);
			et.IsChanged = false;

			AnalysisEventType at = new AnalysisEventType ();
			Assert.IsTrue (at.IsChanged);
			at.IsChanged = false;
			at.Tags.Add (new Tag (""));
			Assert.IsTrue (at.IsChanged);
			at.IsChanged = false;
			at.Tags = null;
			Assert.IsTrue (at.IsChanged);
			at.IsChanged = false;
		}
	}
}
