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
using LongoMatch.Core.Store;
using NUnit.Framework;
using VAS.Core.Common;


namespace Tests.Core.Store
{
	[TestFixture ()]
	public class TestPlayer
	{
		[Test ()]
		public void TestSerialization ()
		{
			PlayerLongoMatch player = new PlayerLongoMatch {Name = "andoni", Position = "runner",
				Number = 5, Birthday = new DateTime (1984, 6, 11),
				Nationality = "spanish", Height = 1.73f, Weight = 70,
				Playing = true, Mail = "test@test", Color = Color.Red
			};
				
			Utils.CheckSerialization (player);
			
			PlayerLongoMatch newPlayer = Utils.SerializeDeserialize (player);
			Assert.AreEqual (player.Name, newPlayer.Name);
			Assert.AreEqual (player.Position, newPlayer.Position);
			Assert.AreEqual (player.Number, newPlayer.Number);
			Assert.AreEqual (player.Birthday, newPlayer.Birthday);
			Assert.AreEqual (player.Nationality, newPlayer.Nationality);
			Assert.AreEqual (player.Height, newPlayer.Height);
			Assert.AreEqual (player.Weight, newPlayer.Weight);
			Assert.AreEqual (player.Playing, newPlayer.Playing);
			Assert.AreEqual (player.Mail, newPlayer.Mail);
			Assert.IsNull (newPlayer.Color);
		}

		[Test ()]
		public void TestToString ()
		{
			PlayerLongoMatch player = new PlayerLongoMatch { Name = "andoni", LastName = "morales", Number = 1 };
			Assert.AreEqual ("1-andoni morales", player.ToString ());
			player.NickName = "ylatuya";
			Assert.AreEqual ("1-ylatuya", player.ToString ());
		}

		[Test ()]
		public void TestPhoto ()
		{
			PlayerLongoMatch player = new PlayerLongoMatch {Name = "andoni", Position = "runner",
				Number = 5, Birthday = new DateTime (1984, 6, 11),
				Nationality = "spanish", Height = 1.73f, Weight = 70,
				Playing = true
			};
			player.Photo = Utils.LoadImageFromFile ();
			Utils.CheckSerialization (player);
			Assert.AreEqual (player.Photo.Width, 16);
			Assert.AreEqual (player.Photo.Height, 16);
		}
	}
}

