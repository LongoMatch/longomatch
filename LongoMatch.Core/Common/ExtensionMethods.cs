//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LongoMatch.Core.Common
{
	public static class ExtensionMethods
	{
		public static void Swap<T> (this IList<T> list, T e1, T e2)
		{
			int index1, index2;
			
			index1 = list.IndexOf (e1);
			index2 = list.IndexOf (e2);
			T temp = list [index1];
			list [index1] = list [index2];
			list [index2] = temp;
		}

		public static T[] Merge<T> (this List<T[]> list)
		{
			var res = new List<T> ();
			
			foreach (T[] t in list) {
				res.AddRange (t);
			}
			return res.ToArray ();
		}

		public static TKey GetKeyByValue<TKey, TValue> (this Dictionary<TKey, TValue> dict, TValue value)
		{
			return dict.SingleOrDefault (x => x.Value.Equals (value)).Key;
		}

		public static void  RemoveKeysByValue<TKey, TValue> (this Dictionary<TKey, TValue> dict, TValue value)
		{
			foreach (var item in dict.Where(k => k.Value.Equals(value)).ToList()) {
				try {
					dict.Remove (item.Key);
				} catch {
				}
			}
		}

		public static bool SequenceEqualSafe<T> (this IList<T> first, IList<T> second)
		{
			if (first == null && second == null) {
				return true;
			} else if (first == null || second == null) {
				return false;
			} else {
				return first.SequenceEqual (second);
			}
		}

		public static bool SequenceEqualNoOrder<T> (this IEnumerable<T> first, IEnumerable<T> second)
		{
			if (first == null && second == null) {
				return true;
			} else if (first == null || second == null) {
				return false;
			} else {
				return (!first.Except (second).Any () && !second.Except (first).Any ());
			}
		}

		public static int RemoveAll<T> (this ObservableCollection<T> coll, Func<T, bool> condition)
		{
			var itemsToRemove = coll.Where (condition).ToList ();
			foreach (var itemToRemove in itemsToRemove) {
				coll.Remove (itemToRemove);
			}
			return itemsToRemove.Count;
		}

		public static void AddRange<T> (this ObservableCollection<T> coll, IEnumerable<T> range)
		{
			foreach (var item in range) {
				coll.Add (item);
			}
		}

		public static T Clamp<T> (this T val, T min, T max) where T : IComparable<T>
		{
			if (val.CompareTo (min) < 0) {
				return min;
			} else if (val.CompareTo (max) > 0) {
				return max;
			} else {
				return val;
			}
		}
	}
}
