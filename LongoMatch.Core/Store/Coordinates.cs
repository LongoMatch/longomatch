// 
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Core.Common
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class Coordinates: IChanged
	{
		ObservableCollection<Point> points;
		
		public Coordinates ()
		{
			Points = new ObservableCollection<Point> ();
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		public ObservableCollection<Point> Points {
			get {
				return points;
			}
			set {
				if (points != null) {
					points.CollectionChanged -= ListChanged;
				}
				points = value;
				if (points != null) {
					points.CollectionChanged += ListChanged;
				}
			}
		}

		public override bool Equals (object obj)
		{
			Coordinates c = obj as Coordinates;
			if (c == null)
				return false;
				
			if (c.Points.Count != Points.Count)
				return false;
			
			for (int i = 0; i < Points.Count; i++) {
				if (!c.Points [i].Equals (Points [i]))
					return false;
			}
			return true;
		}

		public override int GetHashCode ()
		{
			string s = "";
			
			if (Points.Count == 0) {
				return base.GetHashCode ();
			}
			
			for (int i = 0; i < Points.Count; i++) {
				s += this.Points [i].X.ToString () + this.Points [i].Y.ToString ();
			}
			
			return int.Parse (s);
		}

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}


}

