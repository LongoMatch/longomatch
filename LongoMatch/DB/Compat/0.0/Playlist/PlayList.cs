// PlayList.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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

namespace LongoMatch.DB.Compat.v00
{
	
	
	public class PlayList: IPlayList
	{
		
		private  List<PlayListTimeNode> list;
		private static XmlSerializer ser; 
		private string filename = null;
		private int indexSelection = 0;
		
		
		public PlayList(){
			ser = new XmlSerializer(typeof(List<PlayListTimeNode>),new Type[] {typeof(PlayListTimeNode)});
			list = new List<PlayListTimeNode>();
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
				this.Load(file);			
		}
		
		public int Count {
			get{return this.list.Count;}
		}
		
		public void Load(string file){
			
			using(FileStream strm = new FileStream(file, FileMode.Open, FileAccess.Read)) 
			{
				list = ser.Deserialize(strm) as List<PlayListTimeNode>; 
			}		
			foreach (PlayListTimeNode plNode in list){
				plNode.Valid = System.IO.File.Exists(plNode.FileName);
			}
			this.filename = file;
		}
		
		public void Save(){
			this.Save(this.filename);
		}
		
		public void Save(string file){
			file = Path.ChangeExtension(file,"lgm");			
			using (FileStream strm = new FileStream(file, FileMode.Create, FileAccess.Write))
			{
				ser.Serialize(strm, list);
			}
		} 

		public bool isLoaded(){
			return this.filename != null;
		}
		
		public int GetCurrentIndex(){
			return this.indexSelection;
		}
		
		public PlayListTimeNode Next(){
			
			if (this.HasNext())
				this.indexSelection++;	
			return list[indexSelection];
		}
		
		public PlayListTimeNode Prev(){
			if (this.HasPrev())
				this.indexSelection--;
			return list[indexSelection];
		}
		
		public void Add (PlayListTimeNode plNode){
			this.list.Add(plNode);
		}
		
		public void Remove(PlayListTimeNode plNode){
			
			this.list.Remove(plNode);
			if (this.GetCurrentIndex() >= list.Count)
				this.indexSelection --;
		}
		
		public PlayListTimeNode Select (int index){
			this.indexSelection = index;
			return this.list[index];
		}
		
		public bool HasNext(){
			return this.indexSelection < list.Count-1;
		}
		
		public bool HasPrev(){
			return ! this.indexSelection.Equals(0);
		}
		
		
		public string File{
			get {return this.filename;}
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
			this.list.Clear();
			while (listStore.IterIsValid(iter)){
				this.list.Add(listStore.GetValue (iter, 0) as PlayListTimeNode);
				listStore.IterNext(ref iter);
			}
			
		}
		
		public IEnumerator GetEnumerator(){
			return this.list.GetEnumerator();
		}
		
		public IPlayList Copy(){
			return (IPlayList)(this.MemberwiseClone());
		}
	
	
		
		
	}
}
