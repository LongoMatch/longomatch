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
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	/// <summary>
	/// Represents a time instant. Other objects uses it to keep consistency in the time units consitency.
	/// It's expressed in miliseconds and provide some helper methods for time conversion and representation
	/// </summary>
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class Time :  IComparable, IChanged
	{
		private const int MS = 1000000;
		public const int SECONDS_TO_TIME = 1000;
		public const int TIME_TO_NSECONDS = 1000000;

		#region Constructors

		public Time ()
		{
		}

		public Time (int mSeconds)
		{
			MSeconds = mSeconds;
		}

		#endregion

		#region Properties

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		//// <summary>
		/// Time in miliseconds
		/// </summary>
		public int MSeconds {
			get;
			set;
		}

		/// <summary>
		/// Time in seconds
		/// </summary>		
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public int TotalSeconds {
			get {
				return MSeconds / SECONDS_TO_TIME;
			}
			set {
				MSeconds = value * SECONDS_TO_TIME;
			}
		}

		/// <summary>
		/// Time in nano seconds
		/// </summary>		
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public long NSeconds {
			get {
				return  (long)MSeconds * TIME_TO_NSECONDS;
			}
			set {
				MSeconds = (int)(value / TIME_TO_NSECONDS);
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public int Seconds {
			get {
				return (TotalSeconds % 3600) % 60;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public int Minutes {
			get {
				return (TotalSeconds % 3600) / 60;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public int Hours {
			get {
				return (TotalSeconds / 3600);
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
		public  string ToSecondsString (bool includeHour = false)
		{
			if (Hours > 0 || includeHour)
				return String.Format ("{0}:{1}:{2}", Hours, Minutes.ToString ("d2"),
					Seconds.ToString ("d2"));

			return String.Format ("{0}:{1}", Minutes, Seconds.ToString ("d2"));
		}

		/// <summary>
		/// String representation including the milisenconds information
		/// </summary>
		/// <returns>
		/// A <see cref="System.String"/>
		/// </returns>
		public  string ToMSecondsString (bool includeHour = false)
		{
			int _ms;
			_ms = ((MSeconds % 3600000) % 60000) % 1000;

			return String.Format ("{0},{1}", ToSecondsString (includeHour), _ms.ToString ("d3"));
		}

		public override string ToString ()
		{
			return ToMSecondsString (true);
		}

		public override bool Equals (object o)
		{
			if (o is Time) {
				return ((Time)o).MSeconds == MSeconds;
			} else
				return false;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public int CompareTo (object obj)
		{
			if (obj is Time) {
				Time otherTime = (Time)obj;
				return MSeconds.CompareTo (otherTime.MSeconds);
			} else
				throw new ArgumentException ("Object is not a Temperature");
		}

		#endregion

		#region Operators

		public static bool operator == (Time t1, Time t2)
		{
			if (Object.ReferenceEquals (t1, t2)) {
				return true;
			}
			
			if ((object)t1 == null || (object)t2 == null) {
				return false;
			}
			
			return t1.Equals (t2);
		}

		public static bool operator != (Time t1, Time t2)
		{
			return !(t1 == t2);
		}

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

		public static Time operator + (Time t1, int t2)
		{
			return new Time { MSeconds = t1.MSeconds + t2 };
		}

		public static Time operator + (Time t1, Time t2)
		{
			return new Time { MSeconds = t1.MSeconds + t2.MSeconds };
		}

		public  static Time operator - (Time t1, Time t2)
		{
			return new Time { MSeconds = t1.MSeconds - t2.MSeconds };
		}

		public  static Time operator - (Time t1, int t2)
		{
			return new Time { MSeconds = t1.MSeconds - t2 };
		}

		public static Time operator * (Time t1, double t2)
		{
			return new Time { MSeconds = (int)(t1.MSeconds * t2) };
		}

		public static Time operator / (Time t1, int t2)
		{
			return new Time { MSeconds = (int)(t1.MSeconds / t2) };
		}

		#endregion
	}
}
