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
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Gtk;

namespace LongoMatch
{
	
	
	public class PlayList
	{
		private ArrayList list;
		private static XmlSerializer ser; 
		private string filename = null;
		
		public PlayList()
		{
			list = new ArrayList();	
			ser = new XmlSerializer(typeof(ArrayList),new Type[] {typeof(PlayListTimeNode)});
		}
		
		public void Load(string file){
			
			using(FileStream strm = new FileStream(file, FileMode.Open, FileAccess.Read)) 
			{
				list = ser.Deserialize(strm) as ArrayList; 
			}			
			this.filename = file;
		}
		
		public void Save(string file){
			file = Path.ChangeExtension(file,"lgm");			
			using (FileStream strm = new FileStream(file, FileMode.Create, FileAccess.Write))
			{
				ser.Serialize(strm, list);
			}
		} 
		public void New(string filename){
			this.filename = filename;
			this.list.Clear();
		}
		public bool isLoaded(){
			return this.filename != null;
		}
		
		public FileFilter FileFilter{
			get{
				FileFilter filter = new FileFilter();
				filter.Name = "LGM playlist";
				filter.AddPattern("*.lgm");
				return filter;
			}
				
				
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
			while (listStore.IterIsValid(iter)){
				this.list.Add(listStore.GetValue (iter, 0) as PlayListTimeNode);
				Console.WriteLine((listStore.GetValue (iter, 0) as PlayListTimeNode).MiniaturePath);
				listStore.IterNext(ref iter);
			}
			
		}
		
		
	}
}
