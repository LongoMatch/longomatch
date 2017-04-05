//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Drawing.CanvasObjects.Location;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;

namespace Tests.Drawing.Objects
{
	[TestFixture]
	public class TestPointLocationView
	{
		[Test]
		public void TestSetEventWithCoordinates ()
		{
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new TimelineEvent { EventType = new EventType () };
			timelineEvent.EventType.TagFieldPosition = true;
			timelineEvent.AddDefaultPositions ();
			PointLocationView view = new PointLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.TimelineEvent = timelineEvent;

			view.Draw (tkMock.Object, null);

			tkMock.Verify (tk => tk.DrawCircle (It.IsAny<Point> (), It.IsAny<double> ()), Times.Once ());
		}

		[Test]
		public void TestSetEventWithoutCoordinates ()
		{
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new TimelineEvent { EventType = new EventType () };
			PointLocationView view = new PointLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.TimelineEvent = timelineEvent;

			view.Draw (tkMock.Object, null);

			tkMock.Verify (tk => tk.DrawCircle (It.IsAny<Point> (), It.IsAny<double> ()), Times.Never ());
		}
	}
}
