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
using LongoMatch.Store;
using LongoMatch.Common;
using LongoMatch.Interfaces;

namespace LongoMatch.Common
{


	[Serializable]
	public class PlayList: List<PlayListPlay>, IPlayList
	{

		private int indexSelection = 0;
		private Version version;

		#region Constructors
		public PlayList() {
			version = new Version(2,0);
		}

		#endregion

		#region Properties

		public string Filename {
			get;
			set;
		}

		public Version Version {
			get {
				return version;
			}
		}
		#endregion

		#region Public methods
		public void Save() {
			Save(Filename);
		}

		public void Save(string filePath) {
			SerializableObject.Save(this, filePath);
		}

		public static PlayList Load(string filePath) {
			PlayList pl;
			string filename = System.IO.Path.ChangeExtension(filePath, Constants.PLAYLIST_EXT);
			
			//For new Play List
			if(!System.IO.File.Exists(filePath))
				pl = new PlayList();
			else
				pl = SerializableObject.Load<PlayList>(filePath);
			pl.Filename = filename;
			return pl; 
		}

		public int GetCurrentIndex() {
			return indexSelection;
		}

		public PlayListPlay Next() {
			if(HasNext())
				indexSelection++;
			return this[indexSelection];
		}

		public PlayListPlay Prev() {
			if(HasPrev())
				indexSelection--;
			return this[indexSelection];
		}
		
		public void Reorder (int indexIn, int indexOut) {
			var play = this[indexIn];
			RemoveAt(indexIn);
			Insert(indexOut, play);
			
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

		public new bool Remove(PlayListPlay plNode) {
			bool ret = base.Remove(plNode);
			if(GetCurrentIndex() >= Count)
				indexSelection --;
			return ret;
		}

		public PlayListPlay Select(int index) {
			indexSelection = index;
			return this[index];
		}

		public bool HasNext() {
			return indexSelection < Count-1;
		}

		public bool HasPrev() {
			return !indexSelection.Equals(0);
		}

/*		public ListStore GetModel() {
			Gtk.ListStore listStore = new ListStore(typeof(PlayListPlay));
			foreach(PlayListPlay plNode in this) {
				listStore.AppendValues(plNode);
			}
			return listStore;
		}

		public void SetModel(ListStore listStore) {
			TreeIter iter ;

			listStore.GetIterFirst(out iter);
			Clear();
			while(listStore.IterIsValid(iter)) {
				Add(listStore.GetValue(iter, 0) as PlayListPlay);
				listStore.IterNext(ref iter);
			}
		}
*/

		public IPlayList Copy() {
			return (IPlayList)(MemberwiseClone());
		}
		#endregion
	}
}
