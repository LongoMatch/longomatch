//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Specialized;
using System.Linq;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.CanvasObjects.Location;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.Widgets
{
	/// <summary>
	/// A view to display and interact with the location of all the timeline events in a <see cref="LMProject"/>.
	/// </summary>
	public class ProjectLocationsTaggerView : BackgroundCanvas, ICanvasView<LMProjectVM>
	{
		public event ShowTaggerMenuHandler ShowMenuEvent;

		Dictionary<LMTimelineEventVM, TimelineEventLocationView> eventToView;
		LMProjectVM viewModel;

		public ProjectLocationsTaggerView (IWidget widget) : base (widget)
		{
			Accuracy = VASDrawing.Constants.TAGGER_POINT_SIZE + 3;
			EmitSignals = true;
			SelectionMode = MultiSelectionMode.MultipleWithModifier;
			BackgroundColor = App.Current.Style.PaletteBackground;
			eventToView = new Dictionary<LMTimelineEventVM, TimelineEventLocationView> ();
		}

		public ProjectLocationsTaggerView () : this (null)
		{
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
			if (disposing) {
				ViewModel = null;
			}
		}

		public LMProjectVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.Timeline.FullTimeline.GetNotifyCollection ().CollectionChanged -= HandleCollectionChanged;
				}
				viewModel = value;
				ClearObjects ();
				if (viewModel != null) {
					viewModel.Timeline.FullTimeline.GetNotifyCollection ().CollectionChanged += HandleCollectionChanged;
					foreach (LMTimelineEventVM eventVM in viewModel.Timeline.FullTimeline) {
						AddTimelineEvent (eventVM);
					}
				}
			}
		}

		/// <summary>
		/// The field position used by this view.
		/// </summary>
		/// <value>The field position.</value>
		public FieldPositionType FieldPosition {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="T:LongoMatch.Drawing.Widgets.ProjectLocationsTaggerView"/>
		/// emit signals to load an event when the location is clicked.
		/// </summary>
		/// <value><c>true</c> to emit signals; otherwise, <c>false</c>.</value>
		public bool EmitSignals {
			get;
			set;
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMProjectVM)viewModel;
		}

		public void AddTimelineEvent (LMTimelineEventVM timelineEventVM)
		{
			var po = new TimelineEventLocationView {
				BackgroundWidth = Background.Width,
				BackgroundHeight = Background.Height,
				FieldPosition = FieldPosition
			};
			po.SetViewModel (timelineEventVM);
			eventToView [timelineEventVM] = po;
			AddObject (po);
		}

		public void RemoveTimelineEvent (LMTimelineEventVM timelineEventVM)
		{
			Objects.Remove (eventToView [timelineEventVM]);
			eventToView.Remove (timelineEventVM);
		}

		protected override void SelectionChanged (List<Selection> selections)
		{
			if (selections.Count > 0) {
				LMTimelineEventVM p = (selections.Last ().Drawable as TimelineEventLocationView).ViewModel;
				if (EmitSignals) {
					// FIXME: Use a ViewModel command
					App.Current.EventsBroker.Publish (new LoadEventEvent { TimelineEvent = p.Model });
				}
			}
		}

		protected override void ShowMenu (Point coords)
		{
			if (ShowMenuEvent != null) {
				List<LMTimelineEvent> plays = Selections.Select (p => (p.Drawable as TimelineEventLocationView).ViewModel.Model).ToList ();
				ShowMenuEvent (plays);
			}
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			switch (e.Action) {
			case NotifyCollectionChangedAction.Add: {
					foreach (LMTimelineEventVM timelineEvent in e.NewItems) {
						AddTimelineEvent (timelineEvent);
					}
					break;
				}
			case NotifyCollectionChangedAction.Remove: {
					foreach (LMTimelineEventVM timelineEvent in e.OldItems) {
						RemoveTimelineEvent (timelineEvent);
					}
					break;
				}
			case NotifyCollectionChangedAction.Reset: {
					eventToView.Clear ();
					break;
				}
			}
		}
	}
}