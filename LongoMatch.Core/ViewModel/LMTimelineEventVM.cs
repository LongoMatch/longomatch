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
