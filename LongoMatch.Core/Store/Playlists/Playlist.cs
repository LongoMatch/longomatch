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
using LongoMatch.Core.Interfaces;
using System.Collections.ObjectModel;

namespace LongoMatch.Core.Store.Playlists
{
	[Serializable]
	public class Playlist: IStorable
	{
		int indexSelection = 0;

		#region Constructors

		public Playlist ()
		{
			ID = System.Guid.NewGuid ();
			Elements = new ObservableCollection<IPlaylistElement> ();
		}

		#endregion

		#region Properties

		/// <summary>
		/// Unique ID for the playlist
		/// </summary>
		public Guid ID {
			get;
			set;
		}

		public string Name {
			get;
			set;
		}

		public ObservableCollection<IPlaylistElement> Elements {
			get;
			set;
		}

		public int CurrentIndex {
			get {
				return indexSelection;
			}
		}

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
				indexSelection = Elements.IndexOf (play);
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

		#endregion
	}
}
