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
using LongoMatch.Core.Filters;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Drawing.CanvasObjects.Location;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store;
using VAS.Drawing;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.Widgets
{
	/// <summary>
	/// Show coordinates and points positions in a view. It can be used in the stats to display all points in a field.
	/// This View is used in places not migrated yet to MVVM.
	/// </summary>
	public class PositionsViewerView : BackgroundCanvas
	{
		EventsFilter filter;

		public PositionsViewerView (IWidget widget) : base (widget)
		{
			Accuracy = VASDrawing.Constants.TAGGER_POINT_SIZE + 3;
			ObjectsCanMove = false;
			SelectionMode = MultiSelectionMode.Single;
			BackgroundColor = App.Current.Style.PaletteBackground;
		}

		public PositionsViewerView () : this (null)
		{
		}

		public FieldPositionType FieldPosition {
			get;
			set;
		}

		public EventsFilter Filter {
			get {
				return filter;
			}
			set {
				if (filter != null) {
					filter.FilterUpdated -= HandleFilterUpdated;
				}
				filter = value;
				filter.FilterUpdated += HandleFilterUpdated;
			}
		}

		public List<Coordinates> Coordinates {
			set {
				ClearObjects ();
				foreach (Coordinates coord in value) {
					AddPosition (coord.Points);
				}
			}
		}

		public Project Project {
			set {
				foreach (LMTimelineEvent evt in value.Timeline) {
					AddPlay (evt);
				}
			}
		}

		void AddPlay (TimelineEvent play)
		{
			Coordinates coords;

			coords = play.CoordinatesInFieldPosition (FieldPosition);
			if (coords == null)
				return;

			PointLocationView view = AddPosition (coords.Points);
			view.TimelineEvent = play;
			if (Filter != null) {
				view.Visible = Filter.IsVisible (play);
			}
		}

		PointLocationView AddPosition (IList<Point> position)
		{
			var positionView = new PointLocationView {
				BackgroundWidth = Background.Width,
				BackgroundHeight = Background.Height,
				FieldPosition = FieldPosition,
				Points = position,
			};
			AddObject (positionView);
			return positionView;
		}

		void HandleFilterUpdated ()
		{
			foreach (PointLocationView po in Objects) {
				po.Visible = Filter.IsVisible (po.TimelineEvent);
			}
			widget?.ReDraw ();
		}
	}
}