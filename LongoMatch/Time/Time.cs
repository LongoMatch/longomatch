// Time.cs
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

namespace LongoMatch.TimeNodes
{

	/// <summary>
	/// I represent a time instant. Other objects uses me to maintain time units consitency.
	/// I am expressed in miliseconds and I provide some helper methods for time conversion and representation
	/// </summary>
	public class Time :  IComparable
	{
		private int time;
		private const int MS = 1000000 ;
		public const int SECONDS_TO_TIME = 1000;

		#region Constructors
		public Time() {
			this.time = 0;
		}

		/// <summary>
		/// Creates a new time instant
		/// </summary>
		/// <param name="time">
		/// A <see cref="System.Int32"/> with the time expressed in miliseconds
		/// </param>
		public Time(int time)
		{
			this.time = time;
		}
		#endregion

		//// <value>
		/// Time in miliseconds
		/// </value>
		#region Properties
		public int MSeconds {
			get {
				return time;
			}
			set {
				time = value;
			}
		}

		/// <value>
		/// Time in seconds
		/// </value>
		public int Seconds {
			get {
				return time/SECONDS_TO_TIME;
			}
		}
		#endregion

		#region Public methods
		/// <summary>
		/// String representation in seconds
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public  string ToSecondsString()
		{
			int _h, _m, _s;

			_h = (time / 3600);
			_m = ((time % 3600) / 60);
			_s = ((time % 3600) % 60);

			if (_h > 0)
				return String.Format("{0}:{1}:{2}", _h, _m.ToString("d2"),
				                     _s.ToString("d2"));

			return String.Format("{0}:{1}", _m, _s.ToString("d2"));
		}

		/// <summary>
		/// String representation including the milisenconds information
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public  string ToMSecondsString()
		{
			int _h, _m, _s,_ms,_time;
			_time = time / 1000;
			_h = (_time / 3600);
			_m = ((_time % 3600) / 60);
			_s = ((_time % 3600) % 60);
			_ms = ((time % 3600000)%60000)%1000;

			//if (_h > 0)
			return String.Format("{0}:{1}:{2},{3}", _h, _m.ToString("d2"),
			                     _s.ToString("d2"),_ms.ToString("d3"));

			//return String.Format ("{0}:{1},{2}", _m, _s.ToString ("d2"),_ms.ToString("d3"));
		}

		public override bool Equals(object o)
		{
			if (o is Time) {
				return ((Time)o).MSeconds == MSeconds;
			}
			else return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public int CompareTo(object obj) {
			if (obj is Time)
			{
				Time  otherTime = (Time) obj;
				return MSeconds.CompareTo(otherTime.MSeconds);
			}
			else throw new ArgumentException("Object is not a Temperature");
		}
		#endregion

		#region Operators
		public static bool operator < (Time t1,Time t2) {
			return t1.MSeconds < t2.MSeconds;
		}

		public static bool operator > (Time t1,Time t2) {
			return t1.MSeconds > t2.MSeconds;
		}

		public static bool operator <= (Time t1,Time t2) {
			return t1.MSeconds <= t2.MSeconds;
		}

		public static bool operator >= (Time t1,Time t2) {
			return t1.MSeconds >= t2.MSeconds;
		}

		public static Time operator +(Time t1,int t2) {
			return new Time(t1.MSeconds+t2);
		}

		public static Time operator +(Time t1,Time t2) {
			return new Time(t1.MSeconds+t2.MSeconds);
		}

		public  static Time operator -(Time t1,Time t2) {
			return new Time(t1.MSeconds-t2.MSeconds);
		}

		public  static Time operator -(Time t1,int t2) {
			return new Time(t1.MSeconds-t2);
		}
		#endregion
	}
}