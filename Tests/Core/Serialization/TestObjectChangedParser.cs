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
using NUnit.Framework;
using System.Collections.Generic;
using Newtonsoft.Json;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;

namespace Tests.Core.Serialization
{
	public class TestObjectBase : IStorable
	{
		public Guid ID { get; set; }

		public List<IStorable> storables { get; set; }

		public IStorage Storage { get; set; }

		public bool IsLoaded { get; set; }

		public bool IsChanged { get; set; }

		public bool DeleteChildren { get { return true; } }

		public List<IStorable> SavedChildren { get; set; }

		public string DocumentID { get; set; }

		public Guid ParentID { get; set; }

		public int Version { get; set; }
	}

	public class TestObject1 : TestObjectBase
	{
		public string Name { get; set; }

		public int Idx { get; set; }

		public TestObject2 Storable { get; set; }

		[JsonIgnore]
		public TestObject2 Ignored { get; set; }

		public List<TestObject2> StorableList { get; set; }

		public Dictionary<string, TestObject2> StorableDict { get; set; }

		public List<TestObject3> NotStorableList { get; set; }
	}

	public class TestObject2 : TestObjectBase
	{
		public TestObject3 NotStorable { get; set; }

		public TestObject4 Storable { get; set; }

		public List<TestObject4> StorableList { get; set; }
	}

	public class TestObject3
	{
		public DateTime Date { get; set; }

		public Color Color { get; set; }

		public bool IsChanged { get; set; }
	}

	public class TestObject4 : TestObjectBase
	{
		public long Idx;

		public TestObject2 DepCycle { get; set; }
	}

	[TestFixture ()]
	public class TestObjectChangedParser
	{

		[Test ()]
		public void TestParsed ()
		{
			StorableNode parent;
			List<IStorable> storables = null, changed = null;
			TestObject1 obj1 = new TestObject1 ();
			obj1.Storable = new TestObject2 ();
			obj1.Ignored = new TestObject2 ();
			ObjectChangedParser parser = new ObjectChangedParser ();
			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (2, parser.parsedCount);
			Assert.AreEqual (0, changed.Count);
			Assert.AreEqual (2, storables.Count);
		}

		[Test ()]
		public void TestParsedWithDependencyCycles ()
		{
			StorableNode parent;
			List<IStorable> storables = null, changed = null;
			TestObject2 obj2 = new TestObject2 ();
			obj2.Storable = new TestObject4 ();
			obj2.Storable.DepCycle = obj2;
			ObjectChangedParser parser = new ObjectChangedParser ();
			Assert.IsTrue (parser.ParseInternal (out parent, obj2, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (2, parser.parsedCount);
			Assert.AreEqual (0, changed.Count);
			Assert.AreEqual (2, storables.Count);
		}

		[Test ()]
		public void TestParsedAndReset ()
		{
			StorableNode parent;
			List<IStorable> storables = null, changed = null;
			ObjectChangedParser parser = new ObjectChangedParser ();
			TestObject1 obj1 = new TestObject1 ();
			obj1.IsChanged = true;

			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (1, parser.parsedCount);
			Assert.AreEqual (1, changed.Count);
			Assert.AreEqual (1, storables.Count);
			Assert.IsFalse (obj1.IsChanged);

			obj1.IsChanged = true;
			storables = null;
			changed = null;
			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings, false));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (1, parser.parsedCount);
			Assert.AreEqual (1, changed.Count);
			Assert.AreEqual (1, storables.Count);
			Assert.IsTrue (obj1.IsChanged);
		}

		[Test ()]
		public void TestParsedAndResetstorables ()
		{
			StorableNode parent;
			List<IStorable> storables = null, changed = null;
			ObjectChangedParser parser = new ObjectChangedParser ();
			TestObject1 obj1 = new TestObject1 ();
			obj1.Storable = new TestObject2 ();
			obj1.Ignored = new TestObject2 ();
			obj1.IsChanged = true;
			obj1.Storable.IsChanged = true;
			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (2, parser.parsedCount);
			Assert.AreEqual (2, changed.Count);
			Assert.AreEqual (2, storables.Count);
			storables = null;
			changed = null;
			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (2, parser.parsedCount);
			Assert.AreEqual (0, changed.Count);
			Assert.AreEqual (2, storables.Count);
		}

		[Test ()]
		public void TestAllObjectsParsed ()
		{
			StorableNode parent;
			List<IStorable> storables = null, changed = null;
			List<object> objects = new List<object> ();
			TestObject1 obj1 = CreateObject1 (objects);
			ObjectChangedParser parser = new ObjectChangedParser ();
			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (objects.Count, parser.parsedCount);
			Assert.AreEqual (0, changed.Count);
			Assert.AreEqual (57, storables.Count);
			obj1.Storable.IsChanged = true;
			obj1.StorableList [2].IsChanged = true;
			obj1.StorableDict ["1"].IsChanged = true;
			storables = null;
			changed = null;
			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings, false));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (3, changed.Count);
			Assert.AreEqual (57, storables.Count);
			obj1.NotStorableList [1].IsChanged = true;
			storables = null;
			changed = null;
			Assert.IsTrue (parser.ParseInternal (out parent, obj1, Serializer.JsonSettings));
			Assert.IsTrue (parent.ParseTree (ref storables, ref changed));
			Assert.AreEqual (4, changed.Count);
			Assert.AreEqual (57, storables.Count);
		}

		TestObject1 CreateObject1 (List<object> objects)
		{
			TestObject1 obj1 = new TestObject1 ();
			objects.Add (obj1);
			obj1.Name = "test";
			obj1.Idx = 2;
			obj1.Storable = CreateObject2 (objects);
			obj1.StorableList = new List<TestObject2> ();
			for (int i = 0; i < 5; i++) {
				obj1.StorableList.Add (CreateObject2 (objects));
			}
			obj1.StorableDict = new Dictionary<string, TestObject2> ();
			obj1.StorableDict ["1"] = CreateObject2 (objects);
			obj1.StorableDict ["2"] = CreateObject2 (objects);
			obj1.NotStorableList = new List<TestObject3> ();
			for (int i = 0; i < 5; i++) {
				obj1.NotStorableList.Add (CreateObject3 (objects));
			}
			return obj1;
		}

		TestObject2 CreateObject2 (List<object> objects)
		{
			TestObject2 obj2 = new TestObject2 ();
			objects.Add (obj2);
			obj2.NotStorable = CreateObject3 (objects);
			obj2.Storable = CreateObject4 (objects);
			obj2.StorableList = new List<TestObject4> ();
			for (int i = 0; i < 5; i++) {
				obj2.StorableList.Add (CreateObject4 (objects));
			}
			return obj2;
		}

		TestObject3 CreateObject3 (List<object> objects)
		{
			TestObject3 obj3 = new TestObject3 ();
			objects.Add (obj3);
			obj3.Date = DateTime.Now;
			obj3.Color = new Color (2, 2, 2);
			obj3.Color.IsChanged = false;
			objects.Add (obj3.Color);
			return obj3;
		}

		TestObject4 CreateObject4 (List<object> objects)
		{
			TestObject4 obj4 = new TestObject4 ();
			objects.Add (obj4);
			obj4.Idx = 4000;
			return obj4;
		}
	}
}

