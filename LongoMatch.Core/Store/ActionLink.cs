//
//  Copyright (C) 2015 Fluendo S.A.
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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// Defines an action link between 2 buttons in a <see cref="LongoMatch.Core.Store.Templates.Dashboard"/>.
	/// </summary>
	[Serializable]
	public class ActionLink
	{
		public ActionLink ()
		{
			KeepCommonTags = true;
			KeepPlayerTags = true;
			TeamAction = TeamLinkAction.Keep;
			SourceTags = new List<Tag> ();
			DestinationTags = new List<Tag> ();
		}

		/// <summary>
		/// The source button of the link
		/// </summary>
		public DashboardButton SourceButton {
			get;
			set;
		}

		/// <summary>
		/// A list of tags that needs to match in the source
		/// </summary>
		public List<Tag> SourceTags {
			get;
			set;
		}

		/// <summary>
		/// The destination button of the link
		/// </summary>
		public DashboardButton DestinationButton {
			get;
			set;
		}

		/// <summary>
		/// A list of tags that needs to be set in the destination
		/// </summary>
		public List<Tag> DestinationTags {
			get;
			set;
		}

		/// <summary>
		/// The type of action that will be performed in the destination.
		/// </summary>
		public LinkAction Action {
			get;
			set;
		}

		/// <summary>
		/// The type of action that will be performed in the destination
		/// for team tagged in the source event.
		/// </summary>
		public TeamLinkAction TeamAction {
			get;
			set;
		}

		/// <summary>
		/// If <c>true</c>, players tagged in the source event will be copied
		/// </summary>
		public bool KeepPlayerTags {
			get;
			set;
		}

		/// <summary>
		/// If <c>true</c>, common tags will be copied
		/// </summary>
		public bool KeepCommonTags {
			get;
			set;
		}

		public override bool Equals (object obj)
		{
			ActionLink link = obj as ActionLink;
			if (link == null)
				return false;
			if (link.SourceButton != SourceButton ||
			    link.DestinationButton != DestinationButton) {
				return false;
			}
			if (!link.SourceTags.SequenceEqualSafe (SourceTags) ||
			    !link.DestinationTags.SequenceEqualSafe (DestinationTags)) {
				return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public static bool operator == (ActionLink l1, ActionLink l2)
		{
			if (Object.ReferenceEquals (l1, l2)) {
				return true;
			}
			
			if ((object)l1 == null || (object)l2 == null) {
				return false;
			}
			
			return l1.Equals (l2);
		}

		public static bool operator != (ActionLink l1, ActionLink l2)
		{
			return !(l1 == l2);
		}

		public override string ToString ()
		{
			return string.Format ("{0}->{1}", SourceButton != null ? SourceButton.Name : "empty",
				DestinationButton != null ? DestinationButton.Name : "empty");
		}
	}
}

