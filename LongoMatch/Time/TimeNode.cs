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
using LongoMatch.TimeNodes;

namespace LongoMatch.TimeNodes
{

	/// <summary>
	/// I am the base class for the time span related objects in the database.
	/// I have a name that describe me and a start and stop <see cref="LongoMatch.TimeNodes.Time"/>
	/// </summary>
	[Serializable]
	public class TimeNode
	{
		private string name;

		private Time start;

		private Time stop;

		#region Constructors
		public TimeNode() {
		}

		/// <summary>
		/// Creates a TimeNode object
		/// </summary>
		/// <param name="name">
		/// A <see cref="System.String"/> with my name
		/// </param>
		/// <param name="start">
		/// A <see cref="Time"/> with my start time
		/// </param>
		/// <param name="stop">
		/// A <see cref="Time"/> with my stop time
		/// </param>
		public TimeNode(String name,Time start, Time stop)
		{
			this.name = name;
			this.start = start;
			this.stop = stop;
		}
		#endregion

		#region Properties
		/// <value>
		/// A short description of myself
		/// </value>
		public string Name {
			get {
				return this.name;
			}
			set {
				this.name=value;
			}
		}

		//// <value>
		/// My start time
		/// </value>
		public Time Start {
			get {
				return this.start;
			}
			set {
				this.start=value;
			}
		}

		/// <value>
		/// My stop time
		/// </value>
		public Time Stop {
			get {
				return stop;
			}
			set {
				this.stop = value;
			}
		}

		/// <value>
		/// My duration
		/// </value>
		public Time Duration {
			get {
				return Stop-Start;
			}
		}
		#endregion

		#region Public methods

		/// <summary>
		/// Change my boundaries
		/// </summary>
		/// <param name="start">
		/// My new start <see cref="Time"/>
		/// </param>
		/// <param name="stop">
		/// My new stop <see cref="Time"/>
		/// </param>
		public void ChangeStartStop(Time start, Time stop) {
			this.start = start;
			this.stop = stop;
		}
		#endregion
	}
}
