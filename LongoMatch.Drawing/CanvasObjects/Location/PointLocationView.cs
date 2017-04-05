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
using System;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Drawing.CanvasObjects.Location
{
	/// <summary>
	/// A view to display a single location in a map.
	/// </summary>
	public class PointLocationView : LocationView
	{
		TimelineEvent timelineEvent;

		protected override Color Color {
			get;
			set;
		}

		protected override Color OutlineColor {
			get;
			set;
		}

		/// <summary>
		/// Get or sets the event we want to draw.
		/// </summary>
		/// <value>The play.</value>
		public TimelineEvent TimelineEvent {
			get {
				return timelineEvent;
			}

			set {
				timelineEvent = value;
				Points = timelineEvent.CoordinatesInFieldPosition (FieldPosition)?.Points;
			}
		}
	}
}
