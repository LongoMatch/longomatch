// PlayList.cs
//
//  Copyright (C) 2008-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Gtk;
using LongoMatch.TimeNodes;
using Mono.Unix;
namespace LongoMatch.Playlist
{
	
	
	public class PlayList: IPlayList
	{
		
		private  List<PlayListTimeNode> list;
		private static XmlSerializer ser; 
		private string filename = null;
		private int indexSelection = 0;
		private Version version;
		
		#region Constructors
		public PlayList(){
			ser = new XmlSerializer(typeof(List<PlayListTimeNode>),new Type[] {typeof(PlayListTimeNode)});
			list = new List<PlayListTimeNode>();
			version = new Version(1,0);
		}
		
		public PlayList(string file)
		{				
			ser = new XmlSerializer(typeof(List<PlayListTimeNode>),new Type[] {typeof(PlayListTimeNode)});
		
			//For new Play List
			if (!System.IO.File.Exists(file)){
			    list = new List<PlayListTimeNode>();
				filename = file;
			}
			else
				Load(file);	
			
			version = new Version(1,0);
		}
		#endregion
		
		#region Properties
		
		public int Count {
			get{return list.Count;}
		}
		
		public string File{
			get {return filename;}
		}
		
		public Version Version{
			get{return version;}
		}
		#endregion
		
		#region Public methods
		
		public void Load(string file){			
			using(FileStream strm = new FileStream(file, FileMode.Open, FileAccess.Read)) 
			{
				try {
					list = ser.Deserialize(strm) as List<PlayListTimeNode>; 
				}
				catch {
					throw new Exception(Catalog.GetString("The file you are trying to load is not a valid playlist"));
				}
			}		
			foreach (PlayListTimeNode plNode in list){
				plNode.Valid = System.IO.File.Exists(plNode.MediaFile.FilePath);
			}
			filename = file;
		}
		
		public void Save(){
			Save(filename);
		}
		
		public void Save(string file){
			file = Path.ChangeExtension(file,"lgm");			
			using (FileStream strm = new FileStream(file, FileMode.Create, FileAccess.Write))
			{
				ser.Serialize(strm, list);
			}
		} 

		public bool isLoaded(){
			return filename != null;
		}
		
		public int GetCurrentIndex(){
			return indexSelection;
		}
		
		public PlayListTimeNode Next(){			
			if (HasNext())
				indexSelection++;	
			return list[indexSelection];
		}
		
		public PlayListTimeNode Prev(){
			if (HasPrev())
				indexSelection--;
			return list[indexSelection];
		}
		
		public void Add (PlayListTimeNode plNode){
			list.Add(plNode);
		}
		
		public void Remove(PlayListTimeNode plNode){
			
			list.Remove(plNode);
			if (GetCurrentIndex() >= list.Count)
				indexSelection --;
		}
		
		public PlayListTimeNode Select (int index){
			indexSelection = index;
			return list[index];
		}
		
		public bool HasNext(){
			return indexSelection < list.Count-1;
		}
		
		public bool HasPrev(){
			return !indexSelection.Equals(0);
		}
				
		public ListStore GetModel (){
			Gtk.ListStore listStore = new ListStore (typeof (PlayListTimeNode));
			foreach (PlayListTimeNode plNode in list){
				listStore.AppendValues (plNode);							
			}
			return listStore;
		}		
		
		public void SetModel(ListStore listStore){
			TreeIter iter ;
			
			listStore.GetIterFirst(out iter);
			list.Clear();
			while (listStore.IterIsValid(iter)){
				list.Add(listStore.GetValue (iter, 0) as PlayListTimeNode);
				listStore.IterNext(ref iter);
			}			
		}
		
		public IEnumerator GetEnumerator(){
			return list.GetEnumerator();
		}
		
		public IPlayList Copy(){
			return (IPlayList)(MemberwiseClone());
		}				
		#endregion
	}
}
