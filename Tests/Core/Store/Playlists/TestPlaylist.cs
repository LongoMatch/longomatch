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
using System.Collections.ObjectModel;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using NUnit.Framework;

namespace Tests.Core.Store.Playlists
{
	[TestFixture ()]
	public class TestPlaylist
	{
		[Test ()]
		public void TestSerialization ()
		{
			Playlist pl = new Playlist ();
			Utils.CheckSerialization (pl);
			pl.Name = "playlist";
			pl.Elements.Add (new PlaylistDrawing (null));
			pl.Elements.Add (new PlaylistDrawing (null));
			Playlist pl2 = Utils.SerializeDeserialize (pl);
			Assert.AreEqual (pl.Name, pl2.Name);
			Assert.AreEqual (2, pl.Elements.Count);
		}

		[Test ()]
		public void TestIsChanged ()
		{
			Playlist pl = new Playlist ();
			Assert.IsTrue (pl.IsChanged);
			pl.IsChanged = false;
			pl.Name = "playlist";
			Assert.IsTrue (pl.IsChanged);
			pl.IsChanged = false;
			pl.Elements.Add (new PlaylistDrawing (null));
			Assert.IsTrue (pl.IsChanged);
			pl.IsChanged = false;
			pl.Elements = null;
			Assert.IsTrue (pl.IsChanged);
			pl.IsChanged = false;
		}

		[Test ()]
		public void TestDurationEmpty ()
		{
			Playlist pl = new Playlist ();
			Assert.AreEqual (new Time (0), pl.Duration);
		}

		[Test ()]
		public void TestDuration ()
		{
			Playlist pl = new Playlist ();
			pl.IsLoaded = false;
			Assert.AreEqual (new Time (0), pl.Duration);
			Assert.IsFalse (pl.IsLoaded);
			pl.IsLoaded = true;
			var event1 = new TimelineEvent ();
			event1.Start = new Time (10);
			event1.Stop = new Time (20);
			var event2 = new TimelineEvent ();
			event2.Start = new Time (20);
			event2.Stop = new Time (40);
			var event3 = new TimelineEvent ();
			event3.Start = new Time (0);
			event3.Stop = new Time (40);
			var playlistPlayElement = new PlaylistPlayElement (event1);
			var playlistPlayElement2 = new PlaylistPlayElement (event2);
			var playlistPlayElement3 = new PlaylistPlayElement (event3);

			pl.Elements.Add (playlistPlayElement);
			pl.Elements.Add (playlistPlayElement2);

			Assert.AreEqual (new Time (10 + 20), pl.Duration);

			pl.Elements.Add (playlistPlayElement3);

			Assert.AreEqual (new Time (10 + 20 + 40), pl.Duration);

			pl.Elements = new ObservableCollection<IPlaylistElement> ();
			Assert.AreEqual (new Time (0), pl.Duration);
		}

		[Test ()]
		public void TestGetStartTime ()
		{
			Playlist pl = new Playlist ();
			var event1 = new TimelineEvent ();
			event1.Start = new Time (10);
			event1.Stop = new Time (20);
			var event2 = new TimelineEvent ();
			event2.Start = new Time (20);
			event2.Stop = new Time (40);
			var event3 = new TimelineEvent ();
			event3.Start = new Time (0);
			event3.Stop = new Time (40);
			var playlistPlayElement = new PlaylistPlayElement (event1);
			var playlistPlayElement2 = new PlaylistPlayElement (event2);
			var playlistPlayElement3 = new PlaylistPlayElement (event3);

			pl.Elements.Add (playlistPlayElement);
			pl.Elements.Add (playlistPlayElement2);
			pl.Elements.Add (playlistPlayElement3);

			var time = pl.GetStartTime (playlistPlayElement2);
			Assert.AreEqual (new Time (10), time);
			var time3 = pl.GetStartTime (playlistPlayElement3);
			Assert.AreEqual (new Time (30), time3);
		}

		[Test ()]
		public void TestGetCurrentStartTime ()
		{
			Playlist pl = new Playlist ();
			var event1 = new TimelineEvent ();
			event1.Start = new Time (10);
			event1.Stop = new Time (20);
			var event2 = new TimelineEvent ();
			event2.Start = new Time (20);
			event2.Stop = new Time (40);
			var event3 = new TimelineEvent ();
			event3.Start = new Time (0);
			event3.Stop = new Time (40);
			var playlistPlayElement = new PlaylistPlayElement (event1);
			var playlistPlayElement2 = new PlaylistPlayElement (event2);
			var playlistPlayElement3 = new PlaylistPlayElement (event3);

			pl.Elements.Add (playlistPlayElement);
			pl.Elements.Add (playlistPlayElement2);
			pl.Elements.Add (playlistPlayElement3);

			var time = pl.GetCurrentStartTime ();
			Assert.AreEqual (new Time (0), time);

			pl.Select (1);
			var time2 = pl.GetCurrentStartTime ();
			Assert.AreEqual (new Time (10), time2);

			pl.Select (2);
			var time3 = pl.GetCurrentStartTime ();
			Assert.AreEqual (new Time (30), time3);
		}


		[Test ()]
		public void TestGetCurrentStartTimeEmpty ()
		{
			Playlist pl = new Playlist ();

			var time = pl.GetCurrentStartTime ();
			Assert.AreEqual (new Time (0), time);
		}

		[Test ()]
		public void TestGetElementAtTime ()
		{
			Playlist pl = new Playlist ();
			var event1 = new TimelineEvent ();
			event1.Start = new Time (10);
			event1.Stop = new Time (20);
			var event2 = new TimelineEvent ();
			event2.Start = new Time (20);
			event2.Stop = new Time (40);
			var playlistPlayElement = new PlaylistPlayElement (event1);
			var playlistPlayElement2 = new PlaylistPlayElement (event2);

			pl.Elements.Add (playlistPlayElement);
			pl.Elements.Add (playlistPlayElement2);

			var elementTuple = pl.GetElementAtTime (new Time (0));
			Assert.AreEqual (playlistPlayElement, elementTuple.Item1);
			Assert.AreEqual (new Time (0), elementTuple.Item2);

			elementTuple = pl.GetElementAtTime (new Time (10));
			Assert.AreEqual (playlistPlayElement2, elementTuple.Item1);
			Assert.AreEqual (new Time (10), elementTuple.Item2);

			elementTuple = pl.GetElementAtTime (new Time (15));
			Assert.AreEqual (playlistPlayElement2, elementTuple.Item1);
			Assert.AreEqual (new Time (10), elementTuple.Item2);
		}

		[Test ()]
		public void TestGetElementAtTimeOver ()
		{
			Playlist pl = new Playlist ();
			var event1 = new TimelineEvent ();
			event1.Start = new Time (10);
			event1.Stop = new Time (20);
			var playlistPlayElement = new PlaylistPlayElement (event1);

			pl.Elements.Add (playlistPlayElement);

			var elementTuple = pl.GetElementAtTime (new Time (40));
			Assert.IsNull (elementTuple.Item1);
			Assert.AreEqual (new Time (10), elementTuple.Item2);
		}

		[Test ()]
		public void TestGetElementAtTimeNegative ()
		{
			Playlist pl = new Playlist ();
			var event1 = new TimelineEvent ();
			event1.Start = new Time (10);
			event1.Stop = new Time (20);
			var playlistPlayElement = new PlaylistPlayElement (event1);

			pl.Elements.Add (playlistPlayElement);

			var elementTuple = pl.GetElementAtTime (new Time (-10));
			Assert.IsNull (elementTuple.Item1);
			Assert.AreEqual (new Time (0), elementTuple.Item2);
		}

		[Test ()]
		public void TestGetElementAtTimeEmpty ()
		{
			Playlist pl = new Playlist ();

			var elementTuple = pl.GetElementAtTime (new Time (10));
			Assert.IsNull (elementTuple.Item1);
			Assert.AreEqual (new Time (0), elementTuple.Item2);
		}
	}
}

