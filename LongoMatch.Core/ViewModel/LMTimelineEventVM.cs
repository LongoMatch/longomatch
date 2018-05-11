//
//  Copyright (C) 2016 Fluendo S.A.
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store.Playlists;
using VAS.Core.ViewModel;

namespace LongoMatch.Core.ViewModel
{
	public class LMTimelineEventVM : TimelineEventVM<LMTimelineEvent>
	{
		/// <summary>
		/// Explicit conversion from PlaylistElementVM to LMTimelineEventVM
		/// </summary>
		/// <returns>The converted LMTimelineEventVM</returns>
		/// <param name="vm">PlaylistElementVM</param>
		public static explicit operator LMTimelineEventVM (PlaylistElementVM vm)
		{
			if (vm.Model is PlaylistPlayElement) {
				var timelineEvent = vm.Model as PlaylistPlayElement;
				if (timelineEvent.Play is LMTimelineEvent) {
					return new LMTimelineEventVM { Model = timelineEvent.Play as LMTimelineEvent };
				}
			}
			return null;
		}

		/// <summary>
		/// Gets or sets the field position.
		/// </summary>
		/// <value>The field position.</value>
		public Coordinates FieldPosition {
			get {
				return Model.FieldPosition;
			}
			set {
				Model.FieldPosition = value;
			}
		}

		/// <summary>
		/// Position of this event in the half field.
		/// </summary>
		/// <value>The field position.</value>
		public Coordinates HalfFieldPosition {
			get {
				return Model?.HalfFieldPosition;
			}
			set {
				Model.HalfFieldPosition = value;
			}
		}

		/// <summary>
		/// Position of this event in the goal.
		/// </summary>
		/// <value>The field position.</value>
		public Coordinates GoalPosition {
			get {
				return Model?.GoalPosition;
			}
			set {
				Model.GoalPosition = value;
			}
		}

		public Color TeamColor {
			get {
				if (Model.Teams.Count == 1) {
					return Model.Teams [0].Color;
				}
				return Color;
			}
		}

		public override bool Equals (object obj)
		{
			LMTimelineEventVM evt = obj as LMTimelineEventVM;
			if (evt == null)
				return false;
			return Model.Equals (evt.Model);
		}

		public override int GetHashCode ()
		{
			return Model.GetHashCode ();
		}
	}
}
