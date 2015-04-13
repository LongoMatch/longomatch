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
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using System.Collections.Generic;

namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestActionLink
	{

		ActionLink CreateLink ()
		{
			ActionLink link = new ActionLink ();
			link.SourceButton = new DashboardButton ();
			link.SourceTags = new List<Tag> { new Tag ("tag1") };
			link.DestinationButton = new DashboardButton ();
			link.DestinationTags = new List<Tag> { new Tag ("tag2") };
			link.Action = LinkAction.Toggle;
			link.TeamAction = TeamLinkAction.Invert;
			link.KeepCommonTags = false;
			link.KeepPlayerTags = false;
			return link;
		}

		[Test ()]
		public void TestSerialization ()
		{
			ActionLink link = new ActionLink ();

			Utils.CheckSerialization (link);

			link = CreateLink ();

			ActionLink link2 = Utils.SerializeDeserialize (link);
			Assert.AreEqual (link.SourceTags, link2.SourceTags);
			Assert.AreEqual (link.DestinationTags, link2.DestinationTags);
			Assert.AreEqual (link.Action, link2.Action);
			Assert.AreEqual (link.TeamAction, link2.TeamAction);
			Assert.AreEqual (link.KeepCommonTags, link2.KeepCommonTags);
			Assert.AreEqual (link.KeepPlayerTags, link2.KeepPlayerTags);
		}

		[Test ()]
		public void TestEquality ()
		{
			ActionLink link = CreateLink ();
			ActionLink link2 = new ActionLink ();
			Assert.IsTrue (link != link2);
			Assert.AreNotEqual (link, link2);
			link2.SourceButton = link.SourceButton;
			Assert.AreNotEqual (link, link2);
			link2.DestinationButton = link.DestinationButton;
			Assert.AreNotEqual (link, link2);
			link2.SourceTags = new List<Tag> { new Tag ("tag1") }; 
			Assert.AreNotEqual (link, link2);
			link2.DestinationTags = new List<Tag> { new Tag ("tag2") }; 
			Assert.IsTrue (link == link2);
			Assert.IsTrue (link.Equals (link2));
		}

	}
}

