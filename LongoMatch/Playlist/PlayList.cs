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
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Gtk;
using LongoMatch.Store;
using LongoMatch.Common;
using Mono.Unix;
namespace LongoMatch.Playlist
{


	public class PlayList: SerializableObject,IPlayList
	{

		private  List<PlayListPlay> list;
		private string filename = null;
		private int indexSelection = 0;
		private Version version;

		#region Constructors
		public PlayList() {
			list = new List<PlayListPlay>();
			version = new Version(1,0);
		}

		public PlayList(string file)
		{
			//For new Play List
			if(!System.IO.File.Exists(file)) {
				list = new List<PlayListPlay>();
				filename = file;
			}
			else
				Load(file);

			version = new Version(1,0);
		}
		#endregion

		#region Properties

		public int Count {
			get {
				return list.Count;
			}
		}

		public string File {
			get {
				return filename;
			}
		}

		public Version Version {
			get {
				return version;
			}
		}
		#endregion

		#region Public methods
		public void Save() {
			Save(File);
		}

		public void Save(string filePath) {
			Save(this, filePath);
		}

		public static PlayList Load(string filePath) {
			return Load<PlayList>(filePath);
		}

		public bool isLoaded() {
			return filename != null;
		}

		public int GetCurrentIndex() {
			return indexSelection;
		}

		public PlayListPlay Next() {
			if(HasNext())
				indexSelection++;
			return list[indexSelection];
		}

		public PlayListPlay Prev() {
			if(HasPrev())
				indexSelection--;
			return list[indexSelection];
		}

		public void Add(PlayListPlay plNode) {
			list.Add(plNode);
		}

		public void Remove(PlayListPlay plNode) {

			list.Remove(plNode);
			if(GetCurrentIndex() >= list.Count)
				indexSelection --;
		}

		public PlayListPlay Select(int index) {
			indexSelection = index;
			return list[index];
		}

		public bool HasNext() {
			return indexSelection < list.Count-1;
		}

		public bool HasPrev() {
			return !indexSelection.Equals(0);
		}

		public ListStore GetModel() {
			Gtk.ListStore listStore = new ListStore(typeof(PlayListPlay));
			foreach(PlayListPlay plNode in list) {
				listStore.AppendValues(plNode);
			}
			return listStore;
		}

		public void SetModel(ListStore listStore) {
			TreeIter iter ;

			listStore.GetIterFirst(out iter);
			list.Clear();
			while(listStore.IterIsValid(iter)) {
				list.Add(listStore.GetValue(iter, 0) as PlayListPlay);
				listStore.IterNext(ref iter);
			}
		}

		public IEnumerator GetEnumerator() {
			return list.GetEnumerator();
		}

		public IPlayList Copy() {
			return (IPlayList)(MemberwiseClone());
		}
		#endregion
	}
}
