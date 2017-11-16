//
//  Copyright (C) 2015 FLUENDO S.A.
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
using LongoMatch;
using Moq;
using NUnit.Framework;
using VAS.Core.Interfaces;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestServices
	{
		[Test ()]
		public void TestServicesStartStop ()
		{
			List<int> levels = new List<int> ();

			var msvc1 = new Mock<IService> ();
			var msvc2 = new Mock<IService> ();

			msvc1.SetupGet (s => s.Level).Returns (10);
			msvc1.Setup (s => s.Start ()).Returns (true).Callback (() => levels.Add (10));
			msvc1.Setup (s => s.Stop ()).Returns (true).Callback (() => levels.Add (10));
			msvc2.SetupGet (s => s.Level).Returns (20);
			msvc2.Setup (s => s.Start ()).Returns (true).Callback (() => levels.Add (20));
			msvc2.Setup (s => s.Stop ()).Returns (true).Callback (() => levels.Add (20));

			App.Current.RegisterService (msvc1.Object);
			App.Current.RegisterService (msvc2.Object);

			msvc1.Verify (s => s.Start (), Times.Never);
			msvc2.Verify (s => s.Start (), Times.Never);
			msvc1.Verify (s => s.Stop (), Times.Never);
			msvc2.Verify (s => s.Stop (), Times.Never);

			App.Current.StartServices ();

			msvc1.Verify (s => s.Start (), Times.Once);
			msvc2.Verify (s => s.Start (), Times.Once);
			msvc1.Verify (s => s.Stop (), Times.Never);
			msvc2.Verify (s => s.Stop (), Times.Never);
			Assert.AreEqual (new List<int> { 10, 20 }, levels);

			levels.Clear ();

			App.Current.StopServices ();

			msvc1.Verify (s => s.Start (), Times.Once);
			msvc2.Verify (s => s.Start (), Times.Once);
			msvc1.Verify (s => s.Stop (), Times.Once);
			msvc2.Verify (s => s.Stop (), Times.Once);
			Assert.AreEqual (new List<int> { 20, 10 }, levels);
		}
	}
}

