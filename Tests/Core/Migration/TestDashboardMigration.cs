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
using System.IO;
using System.Linq;
using System.Reflection;
using LongoMatch;
using LongoMatch.Core.Common;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Serialization;
using VAS.Core.Store.Templates;
using Constants = LongoMatch.Core.Common.Constants;

namespace Tests.Core.Migration
{
	#pragma warning disable 0618

	[TestFixture]
	public class TestDashboardMigration
	{
		Mock<IPreviewService> mockPreview;

		[OneTimeSetUp]
		public void FixtureSetUp ()
		{
			mockPreview = new Mock<IPreviewService> ();
			App.Current.PreviewService = mockPreview.Object;
		}

		[Test]
		public void TestMigrateDashboardFromV0 ()
		{
			Dashboard dashboard, origDashboard;
			using (Stream resource = Assembly.GetExecutingAssembly ().GetManifestResourceStream ("basket.lct")) {
				origDashboard = Serializer.Instance.Load <Dashboard> (resource);
			}
			dashboard = origDashboard.Clone ();
			dashboard.ID = Guid.Empty;

			mockPreview.Setup (p => p.CreatePreview (dashboard)).Returns (new Image (1, 1));

			Assert.AreEqual (0, dashboard.Version);
			DashboardMigration.Migrate (dashboard);
			Assert.AreNotEqual (Guid.Empty, dashboard.ID);
			Assert.IsNotNull (dashboard.Preview);
			Assert.AreEqual (2, dashboard.Version);

			// Check that every Score and PenaltyCard buttons have now different event types
			Assert.AreNotEqual (1, dashboard.List.OfType<ScoreButton> ().Select (b => b.EventType).Distinct ());
			Assert.AreEqual (6, dashboard.List.OfType<ScoreButton> ().Count ());
			Assert.AreEqual (0, dashboard.List.OfType<ScoreButton> ().Count (b => b.ScoreEventType.ID == Constants.ScoreID));
			Assert.AreEqual (6, dashboard.List.OfType<ScoreButton> ().GroupBy (b => b.ScoreEventType.ID).Count ());

			Assert.AreNotEqual (1, dashboard.List.OfType<PenaltyCardButton> ().Select (b => b.EventType).Distinct ());
			Assert.AreEqual (2, dashboard.List.OfType<PenaltyCardButton> ().Count ());
			Assert.AreEqual (0, dashboard.List.OfType<PenaltyCardButton> ().
				Count (b => b.PenaltyCardEventType.ID == Constants.PenaltyCardID));
			Assert.AreEqual (2, dashboard.List.OfType<PenaltyCardButton> ().GroupBy (b => b.PenaltyCardEventType.ID).Count ());
			Assert.AreEqual (19, dashboard.List.Count);

			// Change that each ScoreEventType has now a Score
			foreach (ScoreButton b in  dashboard.List.OfType<ScoreButton> ()) {
				Assert.IsNotNull (b.ScoreEventType.Score);
			}

			// Change that each PenaltyCardEventType has now a PenaltyCard
			foreach (PenaltyCardButton b in  dashboard.List.OfType<PenaltyCardButton> ()) {
				Assert.IsNotNull (b.PenaltyCardEventType.PenaltyCard);
			}
		}
	}
}
