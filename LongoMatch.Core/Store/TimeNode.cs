// TimeNode.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using LongoMatch.Core.Store;

namespace LongoMatch.Core.Store
{

	/// <summary>
	/// Base class for all the time span related objects in the database.
	/// It has a name that describe it and a start and stop <see cref="LongoMatch.Store.Time"/>
	/// </summary>
	[Serializable]
	public class TimeNode
	{
		Time start, stop, eventTime;

		#region Constructors
		public TimeNode() {
			Rate = 1;
		}
		#endregion

		#region Properties
		/// <summary>
		/// A short description of the time node
		/// </summary>
		public string Name {
			get;
			set;
		}

		/// <summary>
		/// Start Time
		/// </summary>
		public Time Start {
			get {
				return start;
			}
			set {
				start = value;
				if (start > eventTime) {
					eventTime = start;
				}
			}
		}

		/// <summary>
		/// Stop time
		/// </summary>
		public Time Stop {
			get {
				return stop;
			}
			set {
				stop = value;
				if (stop < eventTime) {
					eventTime = stop;
				}
			}
		}

		/// <summary>
		/// The time at which the event takes place
		/// </summary>
		public Time EventTime {
			get {
				return eventTime;
			}
			set {
				eventTime = value;
			}
		}

		/// <summary>
		/// Duration (stop_time - start_time)
		/// </summary>
		[JsonIgnore]
		public Time Duration {
			get {
				return Stop-Start;
			}
		}
		
		/// <summary>
		/// Play rate
		/// </summary>
		public float Rate {
			get;
			set;
		}

		#endregion

	}
}
