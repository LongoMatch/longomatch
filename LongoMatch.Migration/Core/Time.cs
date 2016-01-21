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
using Newtonsoft.Json;
using LongoMatch.Common;

namespace LongoMatch.Store
{

	/// <summary>
	/// Represents a time instant. Other objects uses it to keep consistency in the time units consitency.
	/// It's expressed in miliseconds and provide some helper methods for time conversion and representation
	/// </summary>
	[Serializable]
	[JsonConverter (typeof (LongoMatchConverter))]
	public class Time :  IComparable
	{
		private const int MS = 1000000 ;
		public const int SECONDS_TO_TIME = 1000;
		public const int TIME_TO_NSECONDS = 1000000;

		#region Constructors
		public Time() {
		}
		
		public Time(int mSeconds) {
			MSeconds = mSeconds;
		}
		#endregion

		//// <summary>
		/// Time in miliseconds
		/// </summary>
		#region Properties
		public int MSeconds {
			get;
			set;
		}

		/// <summary>
		/// Time in seconds
		/// </summary>		
		[JsonIgnore]
		public int Seconds {
			get {
				return MSeconds/SECONDS_TO_TIME;
			}
			set {
				MSeconds = value * SECONDS_TO_TIME;
			}
		}
		
		/// <summary>
		/// Time in nano seconds
		/// </summary>		
		[JsonIgnore]
		public long NSeconds {
			get {
				return  (long)MSeconds * TIME_TO_NSECONDS;
			}
			set {
				MSeconds = (int) (value / TIME_TO_NSECONDS);
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
			int _h, _m, _s, _time;

			_time = MSeconds / 1000;
			_h = (_time / 3600);
			_m = ((_time % 3600) / 60);
			_s = ((_time % 3600) % 60);

			if(_h > 0)
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
			int _ms ;
			_ms = ((MSeconds % 3600000)%60000)%1000;

			return String.Format("{0},{1}", ToSecondsString(), _ms.ToString("d3"));
		}

		public override bool Equals(object o)
		{
			if(o is Time) {
				return ((Time)o).MSeconds == MSeconds;
			}
			else return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public int CompareTo(object obj) {
			if(obj is Time)
			{
				Time  otherTime = (Time) obj;
				return MSeconds.CompareTo(otherTime.MSeconds);
			}
			else throw new ArgumentException("Object is not a Temperature");
		}
		#endregion

		#region Operators

		public static bool operator < (Time t1, Time t2)
		{
			return t1?.MSeconds < t2?.MSeconds;
		}

		public static bool operator > (Time t1, Time t2)
		{
			return t1?.MSeconds > t2?.MSeconds;
		}

		public static bool operator <= (Time t1, Time t2)
		{
			return t1?.MSeconds <= t2?.MSeconds;
		}

		public static bool operator >= (Time t1, Time t2)
		{
			return t1?.MSeconds >= t2?.MSeconds;
		}

		public static Time operator +(Time t1,int t2) {
			return new Time {MSeconds = t1.MSeconds+t2};
		}

		public static Time operator +(Time t1,Time t2) {
			return new Time {MSeconds = t1.MSeconds+t2.MSeconds};
		}

		public  static Time operator -(Time t1,Time t2) {
			return new Time {MSeconds = t1.MSeconds-t2.MSeconds};
		}

		public  static Time operator -(Time t1,int t2) {
			return new Time {MSeconds = t1.MSeconds-t2};
		}
		
		public static Time operator *(Time t1, double t2) {
			return new Time {MSeconds = (int) (t1.MSeconds * t2)};
		}
		
		public static Time operator /(Time t1, int t2) {
			return new Time {MSeconds = (int) (t1.MSeconds / t2)};
		}
		#endregion
	}
}