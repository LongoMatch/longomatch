//
//  Copyright (C) 2016 Andoni Morales Alastruey
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
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.Widgets;
using NUnit.Framework;
using VAS.Core.Store;
using Moq;
using VAS.Core.Interfaces.Drawing;

namespace Tests.Drawing.Widgets
{
	[TestFixture]
	public class TestProjectLocationsTaggerView
	{
		[Test]
		public void TestSetProjectVM ()
		{
			var project = Utils.CreateProject (true);
			var projectVM = new LMProjectVM { Model = project };
			var view = new ProjectLocationsTaggerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (VAS.Core.Common.FieldPositionType.Field)
			};
			view.ViewModel = projectVM;

			Assert.AreEqual (project.Timeline.Count, view.Objects.Count);
		}

		[Test]
		public void TestAddEvent ()
		{
			var project = Utils.CreateProject (false);
			var projectVM = new LMProjectVM { Model = project };
			var view = new ProjectLocationsTaggerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (VAS.Core.Common.FieldPositionType.Field)
			};
			view.ViewModel = projectVM;

			project.AddEvent (project.EventTypes [0], new Time (0), new Time (0), new Time (0), null);

			Assert.AreEqual (1, view.Objects.Count);
		}

		[Test]
		public void TestRemoveEvent ()
		{
			var project = Utils.CreateProject (true);
			var projectVM = new LMProjectVM { Model = project };
			var view = new ProjectLocationsTaggerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (VAS.Core.Common.FieldPositionType.Field)
			};
			view.ViewModel = projectVM;
			int count = project.Timeline.Count;
			project.Timeline.RemoveAt (0);

			Assert.AreEqual (count - 1, view.Objects.Count);
		}

		[Test]
		public void TestDispose ()
		{
			var project = Utils.CreateProject (true);
			var projectVM = new LMProjectVM { Model = project };
			var view = new ProjectLocationsTaggerView (Mock.Of<IWidget> ()) {
				Background = project.GetBackground (VAS.Core.Common.FieldPositionType.Field)
			};
			view.ViewModel = projectVM;

			view.Dispose ();

			Assert.IsNull (view.Objects);
		}
	}
}
