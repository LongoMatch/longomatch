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
using LongoMatch.Core.Store.Playlists;

namespace Tests.Core.Store.Playlists
{
	[TestFixture ()]
	public class TestPlaylist
	{
		[Test()]
		public void TestSerialization () {
			Playlist pl = new Playlist ();
			Utils.CheckSerialization (pl);
			pl.Name = "playlist";
			pl.Elements.Add (new PlaylistDrawing (null));
			pl.Elements.Add (new PlaylistDrawing (null));
			Playlist pl2 = Utils.SerializeDeserialize (pl);
			Assert.AreEqual (pl.Name, pl2.Name);
			Assert.AreEqual (2, pl.Elements.Count);
		}

		[Test()]
		public void TestIsChanged () {
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

	}
}

