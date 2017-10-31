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

using System.Collections.Specialized;
using System.Linq;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing.CanvasObjects.Timeline;

namespace LongoMatch.Drawing.CanvasObjects.Timeline
{
	[View ("PeriodsTimelineView")]
	public class PeriodsTimelineView : TimelineView, ICanvasObjectView<CollectionViewModel<Period, PeriodVM>>
	{
		CollectionViewModel<Period, PeriodVM> viewModel;

		/// <summary>
		///  Color used to drag the nodes in the timeline
		/// </summary>
		/// <value>The color of the line.</value>
		public Color LineColor {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating if a background line is rendered in the timeline.
		/// </summary>
		/// <value><c>true</c> if show line; otherwise, <c>false</c>.</value>
		public bool ShowLine {
			get;
			set;
		}


		/// <summary>
		/// Gets or sets the dragging mode of the nodes in this timeline.
		/// </summary>
		/// <value>The dragging mode.</value>
		public NodeDraggingMode DraggingMode {
			get;
			set;
		}

		public CollectionViewModel<Period, PeriodVM> ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.GetNotifyCollection ().CollectionChanged -= HandleTimersCollectionChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.GetNotifyCollection ().CollectionChanged += HandleTimersCollectionChanged;
				}
				ReloadPeriods ();
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (CollectionViewModel<Period, PeriodVM>)viewModel;
		}

		protected override void DrawBackground (IDrawingToolkit tk, Area area)
		{
			base.DrawBackground (tk, area);

			if (ShowLine) {
				// We want the background line and overlay to use the same starting point although they have different sizes.
				double linepos = OffsetY + Height / 2 + StyleConf.TimelineLineSize / 2;
				tk.FillColor = App.Current.Style.ThemeBase;
				tk.StrokeColor = App.Current.Style.ThemeBase;
				tk.LineWidth = StyleConf.TimelineBackgroundLineSize;
				tk.DrawLine (new Point (0, linepos),
							 new Point (Width, linepos));
			}
		}

		void ReloadPeriods ()
		{
			ClearObjects ();
			foreach (PeriodVM t in viewModel) {
				AddPeriod (t);
			}
		}

		void AddTimeNode (TimeNodeVM timeNodeVM, TimerVM timerVM)
		{
			TimerTimeNodeView to = new TimerTimeNodeView ();
			to.ViewModel = timeNodeVM;
			to.Timer = timerVM;
			to.OffsetY = OffsetY;
			to.Height = Height;
			to.SecondsPerPixel = SecondsPerPixel;
			to.MaxTime = Duration;
			to.DraggingMode = DraggingMode;
			to.ShowName = true;
			to.LineColor = LineColor;
			AddNode (to);
		}

		void AddPeriod (PeriodVM period)
		{
			foreach (TimeNodeVM timeNodeVM in period.ViewModels) {
				AddTimeNode (timeNodeVM, period);
			}
			ReDraw ();
		}

		void RemovePeriod (PeriodVM timer)
		{
			foreach (TimerTimeNodeView view in nodes.OfType<TimerTimeNodeView> ().ToList()) {
				if (view.Timer == timer) {
					RemoveObject (view, true);
				}
			}
			ReDraw ();
		}

		void HandleTimersCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					foreach (PeriodVM period in e.NewItems) {
						AddPeriod (period);
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (PeriodVM period in e.OldItems) {
						RemovePeriod (period);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					ReloadPeriods ();
					break;
				}
			}
			ReDraw ();
		}
	}
}
