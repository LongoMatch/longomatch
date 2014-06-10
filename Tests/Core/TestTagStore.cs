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

using LongoMatch.Common;
using LongoMatch.Store;
using System.IO;


namespace Tests.Core
{
	[TestFixture()]
	public class TagStore
	{
		Tag tag1, tag2, tag3, tag4;
		SubCategory subcat1, subcat2, subcat3;
		TagsStore store;
		
		void FillStore () {
			store = new TagsStore ();
			subcat1 = new SubCategory {Name = "subcat1"};
			subcat2 = new SubCategory {Name = "subcat2"};
			subcat3 = new SubCategory {Name = "subcat3"};
			tag1 = new Tag {SubCategory=subcat1, Value="tag1"};
			tag2 = new Tag {SubCategory=subcat1, Value="tag2"};
			tag3 = new Tag {SubCategory=subcat2, Value="tag3"};
			tag4 = new Tag {SubCategory=subcat3, Value="tag4"};
			store.Add (tag1);
			store.Add (tag2);
			store.Add (tag3);
			store.Add (tag4);
		}
		
		[Test()]
		public void TestAddRemove ()
		{
			FillStore ();
			Assert.AreEqual (store.Tags.Count, 4);
			Assert.True (store.Contains (tag4));
			store.Remove (tag4);
			Assert.False (store.Contains (tag4));
			Assert.AreEqual (store.Tags.Count, 3);
			store.Add (tag4);
			Assert.AreEqual (store.Tags.Count, 4);
			Assert.True (store.Contains (tag4));
		}
		
		[Test()]
		public void TestRemoveByCategory ()
		{
			FillStore ();
			store.RemoveBySubcategory (subcat1);
			Assert.AreEqual (store.Tags.Count, 2);
			store.RemoveBySubcategory (subcat2);
			Assert.AreEqual (store.Tags.Count, 1);
			store.RemoveBySubcategory (subcat3);
			Assert.AreEqual (store.Tags.Count, 0);
		}
		
		[Test()]
		public void TestUniqueElements ()
		{
			FillStore ();
			
			Assert.AreEqual (store.AllUniqueElements.Count, 4);
			var tag = new Tag {SubCategory=subcat1, Value="tag1"};
			store.Add (tag);
			Assert.AreEqual (store.AllUniqueElements.Count, 4);
		}
		
		[Test()]
		public void TestGetTags ()
		{
			FillStore ();
			
			Assert.AreEqual (store.GetTags (subcat1).Count, 2);
			Assert.AreEqual (store.GetTags (subcat2).Count, 1);
			Assert.AreEqual (store.GetTags (subcat3).Count, 1);
		}
		
		[Test()]
		public void TestTagValues ()
		{
			FillStore ();
			
			var values = store.GetTagsValues();
			Assert.AreEqual (values[0], "tag1");
			Assert.AreEqual (values[1], "tag2");
			Assert.AreEqual (values[2], "tag3");
			Assert.AreEqual (values[3], "tag4");
		}
		
		[Test()]
		public void TestSerialization ()
		{
			FillStore ();
			
			Utils.CheckSerialization (store);
			
			var newstore = Utils.SerializeDeserialize (store);
			Assert.AreEqual (store.Tags.Count, newstore.Tags.Count);
		}
	}
}

