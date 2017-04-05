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
using System.Collections.Generic;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.Widgets;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;

namespace Tests.Drawing.Widgets
{
	[TestFixture]
	public class TestPositionsViewerView
	{
		[Test]
		public void TestAddProjectWithoutCoords ()
		{
			var project = Utils.CreateProject (true);
			var view = new PositionsViewerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (FieldPositionType.Field)
			};

			view.Project = project;

			Assert.AreEqual (0, view.Objects.Count);
		}

		[Test]
		public void TestAddProjectWithCoordsButNotInTheSameField ()
		{
			var project = Utils.CreateProject (true);
			project.Timeline [0].EventType.TagHalfFieldPosition = true;
			project.Timeline [0].AddDefaultPositions ();
			var view = new PositionsViewerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (FieldPositionType.Field)
			};

			view.Project = project;

			Assert.AreEqual (0, view.Objects.Count);
		}

		[Test]
		public void TestAddProjectWithCoords ()
		{
			var project = Utils.CreateProject (true);
			project.Timeline [0].EventType.TagFieldPosition = true;
			project.Timeline [0].AddDefaultPositions ();
			var view = new PositionsViewerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (FieldPositionType.Field)
			};

			view.Project = project;

			Assert.AreEqual (1, view.Objects.Count);
		}

		[Test]
		public void TestAddCoordinates ()
		{
			var coordinates = new List<Coordinates> ();
			var coord1 = new Coordinates ();
			coord1.Points.Add (new Point (0, 0));
			var coord2 = new Coordinates ();
			coord2.Points.Add (new Point (0, 0));
			coordinates.Add (coord1);
			coordinates.Add (coord2);

			var view = new PositionsViewerView (Mock.Of<IWidget> ()) {
				Background = Utils.LoadImageFromFile ()
			};
			view.Coordinates = coordinates;

			Assert.AreEqual (2, view.Objects.Count);
		}

		[Test]
		public void TestDispose ()
		{
			var project = Utils.CreateProject (true);
			var projectVM = new LMProjectVM { Model = project };
			var view = new ProjectLocationsTaggerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (FieldPositionType.Field)
			};
			view.ViewModel = projectVM;

			view.Dispose ();

			Assert.IsNull (view.Objects);
		}
	}
}
