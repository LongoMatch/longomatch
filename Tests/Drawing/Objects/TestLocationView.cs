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
using System;
using NUnit.Framework;
using LongoMatch.Drawing.CanvasObjects.Location;
using VAS.Core.Common;
using System.Collections.Generic;
using Moq;
using VAS.Core.Interfaces.Drawing;
using LongoMatch;

namespace Tests.Drawing.Objects
{
	class DummyLocationView : LocationView
	{
		protected override Color Color {
			get;
			set;
		}

		protected override Color OutlineColor {
			get;
			set;
		}
	}

	[TestFixture]
	public class TestLocationView
	{
		Mock<IDrawingToolkit> tkMock;

		[TestFixtureSetUp]
		public void SetUpOnce ()
		{
			tkMock = new Mock<IDrawingToolkit> ();
			App.Current.DrawingToolkit = tkMock.Object;
		}

		[Test]
		public void PointsSetter_OnePoint_AreaUpdated ()
		{
			LocationView view = new DummyLocationView ();
			var points = new List<Point> ();
			points.Add (new Point (1, 1));

			// The area is updated correctly and the draw function will draw it
			view.Points = points;
			view.Draw (tkMock.Object, new Area (new Point (0.5, 0.5), 1, 1));

			tkMock.Verify (tk => tk.Begin (), Times.Once ());
			tkMock.Verify (tk => tk.End (), Times.Once ());
		}

		[Test]
		public void PointsSetter_TwoPoint_AreaUpdated ()
		{
			LocationView view = new DummyLocationView ();
			var points = new List<Point> ();
			points.Add (new Point (1, 1));
			points.Add (new Point (2, 2));

			// The area is updated correctly and the draw function will draw it
			view.Points = points;
			view.Draw (tkMock.Object, new Area (new Point (0.5, 0.5), 3, 3));

			tkMock.Verify (tk => tk.Begin (), Times.Once ());
			tkMock.Verify (tk => tk.End (), Times.Once ());
		}

		[Test]
		public void PointsSetter_NullPoints_AreaUpdated ()
		{
			LocationView view = new DummyLocationView ();

			view.Points = null;
			view.Draw (tkMock.Object, new Area (new Point (0.5, 0.5), 1, 1));

			tkMock.Verify (tk => tk.Begin (), Times.Never ());
			tkMock.Verify (tk => tk.End (), Times.Never ());
		}
	}
}
