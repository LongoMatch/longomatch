//
//  Copyright (C) 2017 ${CopyrightHolder}
using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch;
using LongoMatch.Drawing.CanvasObjects.Timeline;
using LongoMatch.Drawing.Widgets;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;
using VAS.Tests;

namespace Tests.Drawing.Widgets
{
	class DummyCamerasTimelineView : CamerasTimelineView
	{
		public DummyCamerasTimelineView (IWidget widget) : base (widget)
		{
			
		}
		/// <summary>
		/// A list with all the selected objects
		/// </summary>
		public new List<Selection> Selections {
			get {
				return base.Selections;
			}
		}

		public new void HandleLeftButton (Point coords, ButtonModifier modif)
		{
			base.HandleLeftButton (coords,modif);
		}
	}

	[TestFixture]
	public class TestCamerasTimelineView
	{
		CameraSynchronizationVM viewModel;

		[OneTimeSetUp]
		public void Initialize ()
		{
			var mockGuiToolkit = new Mock<IGUIToolkit> ();
			mockGuiToolkit.SetupGet (gt => gt.DeviceScaleFactor).Returns (1.0f);
			App.Current.GUIToolkit = mockGuiToolkit.Object;
			var drawingToolkitMock = new Mock<IDrawingToolkit> ();
			drawingToolkitMock.Setup (d => d.CreateSurfaceFromResource (It.IsAny<string> (), It.IsAny<bool> (), It.IsAny<bool> ())).
				  Returns (Mock.Of<ISurface> ());
			App.Current.DrawingToolkit = drawingToolkitMock.Object;
		}

		[SetUp]
		public void SetUp ()
		{
			var videoPlayerVM = new VideoPlayerVM ();
			Project project = Utils.CreateProject (false);
			project.Periods.Clear ();
			var period = new Period ();
			period.Nodes.Add (new TimeNode {
				Start = new Time (0),
				Stop = new Time (3000)
			});
			project.Periods.Add (period);
			var projectVM = new DummyProjectVM { Model = project };
			viewModel = new CameraSynchronizationVM { VideoPlayer = videoPlayerVM, Project = projectVM };
		}

		[Test]
		public void CamerasTimelineView_RemovePeriod_SelectionsUpdated ()
		{
			DummyCamerasTimelineView camerasTimelineView = new DummyCamerasTimelineView (Mock.Of<IWidget> ());
			camerasTimelineView.SetViewModel (viewModel);
			//Force a Selection by clicking on a Period
			var periodsView = camerasTimelineView.Objects.OfType<PeriodsTimelineView>().First ();
			var periodView = periodsView.nodes.First ();
			camerasTimelineView.HandleLeftButton (new Point (periodView.StartX + 1, periodView.OffsetY + 1), ButtonModifier.None);

			Assert.IsTrue (camerasTimelineView.Selections.Any ());

			viewModel.Project.Periods.ViewModels.RemoveAt (0);

			Assert.IsFalse (camerasTimelineView.Selections.Any ());
		}
	}
}
