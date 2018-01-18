//
//  Copyright (C) 2017 Fluendo S.A.
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
using System.Linq;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing;
using LongoMatch.Drawing.CanvasObjects.Timeline;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services.ViewModel;
using Moq;
using NUnit.Framework;
using VAS;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects.Timeline;

namespace Tests.Drawing.Widgets
{
	[TestFixture]
	public class TestLMPlaysTimeline
	{
		LMProject project;
		LMPlaysTimeline timeline;
		LMProjectVM projectVM;
		Mock<IWidget> widgetMock;

		[OneTimeSetUp]
		public void Init ()
		{
			App.Current.DrawingToolkit = new Mock<IDrawingToolkit> ().Object;
			App.Current.ViewLocator = new VAS.Core.MVVMC.ViewLocator ();
			DrawingInit.ScanViews ();
			LMDrawingInit.ScanViews ();
		}

		[SetUp]
		public void SetUp ()
		{
			project = Utils.CreateProject (true);
			project.Periods.Add (new Period ());
			projectVM = new LMProjectVM { Model = project };
			widgetMock = new Mock<IWidget> ();
			widgetMock.SetupAllProperties ();
			timeline = new LMPlaysTimeline (widgetMock.Object);
			LMProjectAnalysisVM viewModel = new LMProjectAnalysisVM { Project = projectVM };
			timeline.ViewModel = viewModel;
		}

		[Test]
		public void TestCreateTimeline ()
		{
			Assert.AreEqual (1, timeline.Objects.OfType<PeriodsTimelineView> ().Count ());
			Assert.AreEqual (1, timeline.Objects.OfType<TimerTimelineView> ().Count ());
			Assert.AreEqual (15, timeline.Objects.OfType<EventTypeTimelineView> ().Count ());
		}

		[Test]
		public void TestAddEventType ()
		{
			projectVM.Timeline.EventTypesTimeline.ViewModels.Add (
				new EventTypeTimelineVM { Model = new EventType { Name = "EV" } });

			Assert.AreEqual (1, timeline.Objects.OfType<PeriodsTimelineView> ().Count ());
			Assert.AreEqual (1, timeline.Objects.OfType<TimerTimelineView> ().Count ());
			Assert.AreEqual (16, timeline.Objects.OfType<EventTypeTimelineView> ().Count ());
		}

		[Test]
		public void TestRemoveEventType ()
		{
			projectVM.Timeline.EventTypesTimeline.ViewModels.Remove (
				projectVM.Timeline.EventTypesTimeline.ViewModels.First ());

			Assert.AreEqual (1, timeline.Objects.OfType<PeriodsTimelineView> ().Count ());
			Assert.AreEqual (1, timeline.Objects.OfType<TimerTimelineView> ().Count ());
			Assert.AreEqual (14, timeline.Objects.OfType<EventTypeTimelineView> ().Count ());
		}

		[Test]
		public void TestUpdateDuration ()
		{
			project.FileSet [0].Duration = new Time { TotalSeconds = 400 };

			double width = project.FileSet.Duration.TotalSeconds / timeline.SecondsPerPixel + 10;
			Assert.AreEqual (widgetMock.Object.Width, width);
			foreach (EventTypeTimelineView view in timeline.Objects.OfType<EventTypeTimelineView> ()) {
				Assert.AreEqual (width, view.Width);
				Assert.AreEqual (project.FileSet.Duration, view.Duration);
			}
		}

		[Test]
		public void RemovePeriodNode_PeriodNodeSelected_SelectionUpdated ()
		{
			PeriodsTimelineView view = timeline.Objects.OfType<PeriodsTimelineView> ().First ();
			TimerTimeNodeView periodNode = (TimerTimeNodeView)view.First ();

			Selection sel = new Selection (periodNode, SelectionPosition.All, 0);
			timeline.UpdateSelection (sel);
			project.Periods.Remove (periodNode.Timer.Model as Period);

			Assert.AreEqual (0, timeline.Selections.Count);
		}
	}
}
