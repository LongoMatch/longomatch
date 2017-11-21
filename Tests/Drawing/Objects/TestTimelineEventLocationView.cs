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
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.CanvasObjects.Location;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;

namespace Tests.Drawing.Objects
{
	[TestFixture]
	public class TestTimelineEventLocationView
	{
		[Test]
		public void TestSetEventVMWithCoordinates ()
		{
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new LMTimelineEvent { EventType = new EventType () };
			timelineEvent.EventType.TagFieldPosition = true;
			timelineEvent.AddDefaultPositions ();
			var timelineEventVM = new LMTimelineEventVM { Model = timelineEvent };

			TimelineEventLocationView view = new TimelineEventLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.SetViewModel (timelineEventVM);

			view.Draw (tkMock.Object, null);

			tkMock.Verify (tk => tk.DrawCircle (It.IsAny<Point> (), It.IsAny<double> ()), Times.Once ());
		}

		[Test]
		public void TestSetEventWithoutCoordinates ()
		{
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new LMTimelineEvent { EventType = new EventType () };
			var timelineEventVM = new LMTimelineEventVM { Model = timelineEvent };

			TimelineEventLocationView view = new TimelineEventLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.SetViewModel (timelineEventVM);

			view.Draw (tkMock.Object, null);

			tkMock.Verify (tk => tk.DrawCircle (It.IsAny<Point> (), It.IsAny<double> ()), Times.Never ());
		}

		[Test]
		public void TestEventDrawWhenPointChanges ()
		{
			int redrawCount = 0;
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new LMTimelineEvent { EventType = new EventType () };
			var timelineEventVM = new LMTimelineEventVM { Model = timelineEvent };
			TimelineEventLocationView view = new TimelineEventLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.SetViewModel (timelineEventVM);
			view.RedrawEvent += (co, area) => redrawCount++;

			timelineEvent.EventType.TagFieldPosition = true;
			timelineEvent.AddDefaultPositions ();

			view.Points = timelineEvent.FieldPosition.Points;
			view.Draw (tkMock.Object, null);

			Assert.Greater (1, redrawCount);
			tkMock.Verify (tk => tk.DrawCircle (It.IsAny<Point> (), It.IsAny<double> ()), Times.Once ());
		}

		[Test]
		public void HandleViewModelPropertyChanged_VisibilityChanged_RedrawTriggered ()
		{
			int redrawCount = 0;
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new LMTimelineEvent { EventType = new EventType () };
			var timelineEventVM = new LMTimelineEventVM { Model = timelineEvent };
			TimelineEventLocationView view = new TimelineEventLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.SetViewModel (timelineEventVM);
			view.RedrawEvent += (co, area) => redrawCount++;
			timelineEvent.EventType.TagFieldPosition = true;
			timelineEvent.AddDefaultPositions ();
			view.RedrawEvent += (co, area) => redrawCount++;

			timelineEventVM.Visible = false;

			Assert.GreaterOrEqual (redrawCount, 1);
		}

		[Test]
		public void HandleViewModelPropertyChanged_ColorChanged_RedrawTriggered ()
		{
			int redrawCount = 0;
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new LMTimelineEvent { EventType = new EventType () };
			var timelineEventVM = new LMTimelineEventVM { Model = timelineEvent };
			TimelineEventLocationView view = new TimelineEventLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.SetViewModel (timelineEventVM);
			view.RedrawEvent += (co, area) => redrawCount++;
			timelineEvent.EventType.TagFieldPosition = true;
			timelineEvent.AddDefaultPositions ();
			view.RedrawEvent += (co, area) => redrawCount++;

			timelineEvent.EventType.Color = Color.Black.Copy ();

			Assert.GreaterOrEqual (redrawCount, 1);
		}

		[Test]
		public void HandleViewModelPropertyChanged_TimeChanged_RedrawNotTriggered ()
		{
			int redrawCount = 0;
			Mock<IDrawingToolkit> tkMock = new Mock<IDrawingToolkit> ();
			var timelineEvent = new LMTimelineEvent { EventType = new EventType () };
			var timelineEventVM = new LMTimelineEventVM { Model = timelineEvent };
			TimelineEventLocationView view = new TimelineEventLocationView {
				FieldPosition = FieldPositionType.Field,
				BackgroundWidth = 100,
				BackgroundHeight = 100,
			};
			view.SetViewModel (timelineEventVM);
			timelineEvent.EventType.TagFieldPosition = true;
			timelineEvent.AddDefaultPositions ();
			view.RedrawEvent += (co, area) => redrawCount++;

			timelineEventVM.Start = new Time ();

			Assert.AreEqual (0, redrawCount);
		}

	}
}
