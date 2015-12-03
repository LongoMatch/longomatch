// PlayList.cs
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Serialization;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store.Playlists
{
	[Serializable]
	public class Playlist: StorableBase
	{
		int indexSelection = 0;
		ObservableCollection<IPlaylistElement> elements;

		#region Constructors

		public Playlist ()
		{
			ID = System.Guid.NewGuid ();
			Elements = new ObservableCollection <IPlaylistElement> ();
		}

		#endregion

		#region Properties

		[LongoMatchPropertyPreload]
		[LongoMatchPropertyIndex (0)]
		public string Name {
			get;
			set;
		}

		public ObservableCollection<IPlaylistElement> Elements {
			get {
				return elements;
			}
			set {
				if (elements != null) {
					elements.CollectionChanged -= ListChanged;
				}
				elements = value;
				if (elements != null) {
					elements.CollectionChanged += ListChanged;
				}
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public int CurrentIndex {
			get {
				return indexSelection;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public IPlaylistElement Selected {
			get {
				if (Elements.Count == 0) {
					return null;
				}
				if (indexSelection >= Elements.Count) {
					indexSelection = 0;
				}
				return Elements [indexSelection];
			}
		}

		#endregion

		#region Public methods

		public IPlaylistElement Next ()
		{
			if (HasNext ())
				indexSelection++;
			return Elements [indexSelection];
		}

		public IPlaylistElement Prev ()
		{
			if (HasPrev ())
				indexSelection--;
			return Elements [indexSelection];
		}

		public void Reorder (int indexIn, int indexOut)
		{
			var play = Elements [indexIn];
			Elements.RemoveAt (indexIn);
			Elements.Insert (indexOut, play);
			
			/* adjust selection index */
			if (indexIn == indexSelection)
				indexSelection = indexOut;
			if (indexIn < indexOut) {
				if (indexSelection < indexIn || indexSelection > indexOut)
					return;
				indexSelection++;
			} else {
				if (indexSelection > indexIn || indexSelection < indexOut)
					return;
				indexSelection--;
			}
		}

		public bool Remove (IPlaylistElement plNode)
		{
			bool ret = Elements.Remove (plNode);
			if (CurrentIndex >= Elements.Count)
				indexSelection--;
			return ret;
		}

		public IPlaylistElement Select (int index)
		{
			indexSelection = index;
			return Elements [index];
		}

		public void SetActive (IPlaylistElement play)
		{
			int newIndex;
			
			newIndex = Elements.IndexOf (play);
			if (newIndex >= 0) {
				indexSelection = newIndex;
			}
		}

		public bool HasNext ()
		{
			return indexSelection < Elements.Count - 1;
		}

		public bool HasPrev ()
		{
			return !indexSelection.Equals (0);
		}

		public Playlist Copy ()
		{
			return (Playlist)(MemberwiseClone ());
		}

		/// <summary>
		/// Gets the element and its start at the passed time.
		/// </summary>
		/// <returns>A tuple with the element at the passed time and its start time in the playlist.</returns>
		/// <param name="pos">Time to query.</param>
		public Tuple<IPlaylistElement, Time> GetElementAtTime (Time pos)
		{
			Time elementStart = new Time (0);
			IPlaylistElement element = null;
			foreach (var elem in Elements) {
				if (elementStart <= pos && elementStart + elem.Duration >= pos) {
					element = elem;
					break;
				}

				// avoid adding duration if pos > total duration
				if (elementStart + elem.Duration < pos) {
					elementStart += elem.Duration;
				}
			}

			return new Tuple<IPlaylistElement, Time> (element, elementStart);
		}

		public Time GetStartTime (IPlaylistElement element)
		{
			return new Time (Elements.TakeWhile (elem => elem != element).Sum (elem => elem.Duration.MSeconds));
		}

		public Time GetCurrentStartTime ()
		{
			if (CurrentIndex >= 0 && CurrentIndex < Elements.Count) {
				return GetStartTime (Elements [CurrentIndex]);
			}
			return new Time (0);
		}

		#endregion

		void ListChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			IsChanged = true;
		}
	}
}
