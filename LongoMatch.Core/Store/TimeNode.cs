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
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using LongoMatch.Core.Store;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{

	/// <summary>
	/// Base class for all the time span related objects in the database.
	/// It has a name that describe it and a start and stop <see cref="LongoMatch.Store.Time"/>
	/// </summary>
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class TimeNode: IChanged
	{
		Time start, stop, eventTime;

		#region Constructors

		public TimeNode ()
		{
			Rate = 1;
		}

		#endregion

		#region Properties

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		/// <summary>
		/// A short description of the time node
		/// </summary>
		[LongoMatchPropertyPreload]
		[LongoMatchPropertyIndex (0)]
		public virtual string Name {
			get;
			set;
		}

		/// <summary>
		/// Start Time
		/// </summary>
		public virtual Time Start {
			get {
				return start;
			}
			set {
				start = value;
				if (eventTime != null && start > eventTime) {
					eventTime = start;
				}
			}
		}

		/// <summary>
		/// Stop time
		/// </summary>
		public virtual Time Stop {
			get {
				return stop;
			}
			set {
				stop = value;
				if (eventTime != null && stop < eventTime) {
					eventTime = stop;
				}
			}
		}

		/// <summary>
		/// The time at which the event takes place
		/// </summary>
		public virtual Time EventTime {
			get {
				return eventTime ?? start;
			}
			set {
				eventTime = value;
			}
		}

		/// <summary>
		/// Duration (stop_time - start_time)
		/// </summary>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Time Duration {
			get {
				if (Stop != null && Start != null) {
					return Stop - Start;
				} else {
					return new Time (0);
				}
			}
		}

		/// <summary>
		/// Play rate
		/// </summary>
		public virtual float Rate {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public string RateString {
			get {
				return String.Format ("{0}X", Rate);
			}
		}

		public void Move (Time diff)
		{
			if (start != null) {
				start += diff;
			}
			if (stop != null) {
				stop += diff;
			}
			if (eventTime != null) {
				eventTime += diff;
			}
		}

		public TimeNode Join (TimeNode tn)
		{
			if (tn.Stop < Start || tn.Start > Stop)
				return null;
			else
				return new TimeNode {
					Start = new Time (Math.Min (Start.MSeconds, tn.Start.MSeconds)),
					Stop = new Time (Math.Max (Stop.MSeconds, tn.Stop.MSeconds))
				};
		}

		public TimeNode Intersect (TimeNode tn)
		{
			if (tn.Stop == null || tn.Start == null || Start == null || Stop == null)
				return null;
			if (tn.Stop <= Start || tn.Start >= Stop)
				return null;
			else
				return new TimeNode {
					Start = new Time (Math.Max (Start.MSeconds, tn.Start.MSeconds)),
					Stop = new Time (Math.Min (Stop.MSeconds, tn.Stop.MSeconds))
				};
		}

		#endregion

	}
}
