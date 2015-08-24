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
using NUnit.Framework;
using System;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Store;
using System.Collections.Generic;

namespace Tests.Core.Store.Playlists
{
	[TestFixture ()]
	public class TestPlaylistPlayElement
	{
		[Test ()]
		public void TestSerialization ()
		{
			TimelineEvent evt = new TimelineEvent ();
			evt.Start = new Time (1000);
			evt.Stop = new Time (2000);
			evt.CamerasLayout = 1;

			PlaylistPlayElement element = new PlaylistPlayElement (evt);
			Utils.CheckSerialization (element);

			PlaylistPlayElement element2 = Utils.SerializeDeserialize (element);
			Assert.AreEqual (element.Description, element2.Description);
			Assert.AreEqual (element.Duration, element2.Duration);
			Assert.AreEqual (element.Rate, element2.Rate);
			Assert.AreEqual (element.RateString, element2.RateString);
			Assert.AreEqual (element.Title, element2.Title);
			Assert.AreEqual (new List<CameraConfig> { new CameraConfig (0) }, element2.CamerasConfig);
			Assert.AreEqual (element.CamerasLayout, element2.CamerasLayout);
		}

		[Test ()]
		public void TestPropertiesProxy ()
		{
			TimelineEvent evt = new TimelineEvent ();
			evt.Start = new Time (1000);
			evt.Stop = new Time (2000);
			evt.CamerasLayout = 1;
			evt.CamerasConfig = new List<CameraConfig> { new CameraConfig (2), new CameraConfig (4) };

			PlaylistPlayElement element = new PlaylistPlayElement (evt);

			Assert.AreEqual (evt.Duration, element.Duration);
			Assert.AreEqual (evt.CamerasLayout, element.CamerasLayout);
			Assert.AreEqual (evt.CamerasConfig, element.CamerasConfig);
			Assert.AreEqual (evt.Rate, element.Rate);
			Assert.AreEqual (evt.Name, element.Title);
		}
	}
}

