// SectionsPropertiesWidget.cs
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//
using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using Gdk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;


namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SectionsPropertiesWidget : Gtk.Bin
	{
		private List<HotKey> hkList;
		private List<TimeNodeProperties> tndlist;
		private Project project;
		
		
		public SectionsPropertiesWidget()
		{
			this.Build();
			tndlist = new List<TimeNodeProperties>();
			hkList = new List<HotKey>();
			table1.NColumns =(uint) 5;						
		}
		
		public void SetProject(Project project){
			this.project = project;
			SetSections(project.Sections);
		}
		
		public void SetSections(Sections sections){
			int sectionsCount = sections.Count;
			
			tndlist.Clear();
			hkList.Clear();
			
			foreach (Widget w in table1.AllChildren){
					w.Unrealize();
					table1.Remove(w);
			}
			
			for( int i=0;i<sectionsCount;i++){
				TimeNodeProperties tnp = new TimeNodeProperties();
				HotKey hk = sections.GetHotKey(i);
				
				tnp.Name = i.ToString();
				tnp.Title =  "Section "+(i+1);			
				tnp.Section = sections.GetSection(i);
				ConnectTimeNodePropertiesEvents(tnp);
				
				
				if (hk.Defined)
					hkList.Add(sections.GetHotKey(i));
				
				AddTimeNodeToTable(i,sections.Count,tnp);			
			}			
		}
		
		
		
		public Sections GetSections (){
			Sections sections = new Sections();
			foreach (TimeNodeProperties tnp in tndlist){
				sections.AddSection(tnp.Section);					
			}
			return sections;
		}
		
		private void AddTimeNodeToTable(int index, int count, TimeNodeProperties tnp){
			uint row_top,row_bottom,col_left,col_right;
			
			tndlist.Insert(index,tnp);
			table1.NRows =(uint) (count/5);			
			row_top =(uint) (index/table1.NColumns);
			row_bottom = (uint) row_top+1 ;
			col_left = (uint) index%table1.NColumns;
			col_right = (uint) col_left+1 ;
			
			table1.Attach(tnp,col_left,col_right,row_top,row_bottom);	
			tnp.Show();
		}
		
		private void ConnectTimeNodePropertiesEvents(TimeNodeProperties tnp){
			tnp.DeleteSection += new EventHandler(OnDelete);
			tnp.InsertAfter += new EventHandler(OnInsertAfter);
			tnp.InsertBefore += new EventHandler(OnInsertBefore);
			tnp.HotKeyChanged += new HotKeyChangeHandler(OnHotKeyChanged);
		}
		
		private void AddSection (int index){
			Sections sections;
			SectionsTimeNode tn;
			TimeNodeProperties tnp;
			HotKey hkey = new HotKey();
			
			Time start = new Time(10*Time.SECONDS_TO_TIME);
			Time stop = new Time(10*Time.SECONDS_TO_TIME);
			
			
			tn  = new SectionsTimeNode("New Section",start,stop,hkey,new Color(Byte.MaxValue,Byte.MinValue,Byte.MinValue));
			tnp = new TimeNodeProperties();
			ConnectTimeNodePropertiesEvents(tnp);
			
			if (project != null){
				project.AddSectionAtPos(tn,index);
				AddTimeNodeToTable(project.Sections.Count-1,project.Sections.Count,tnp);
				UpdateGui(project.Sections);
			}
			else{				
				sections = GetSections();
				sections.AddSectionAtPos(tn,index);
				AddTimeNodeToTable(sections.Count-1,sections.Count,tnp);
				UpdateGui(sections);
			}			
		}
		
		private void DeleteSection(TimeNodeProperties tnp){
			Sections sections;
			int index = int.Parse(tnp.Name);
			int count;
			
			//Remove the last TimeNodeProperties Widget and clean-up
			

			if(project!= null){
				try{
					project.DeleteSection(index);
				}
				catch (Exception e){
					MessagePopup.PopupMessage(this,MessageType.Warning,
					                          Catalog.GetString("You can't delete the last section"));
					return;
				}
				sections=project.Sections;
			}
			else{
				sections = GetSections();
				sections.RemoveSection(index);
			}
			count = tndlist.Count;
			table1.Remove(tndlist[count-1]);
			tndlist.Remove(tndlist[count-1]);
			
			UpdateGui(sections);
			
		}
		
		private void UpdateGui(Sections sections){
			//After delting/adding a TimeNodeProperties we need to update
			//both the widget names and their position in the table
			TimeNodeProperties tnp;			
			
			for( int i=0;i< sections.Count;i++){
				tnp=tndlist[i];
				tnp.Name = i.ToString();
				tnp.Title =  "Section "+(i+1);
				tnp.Section = sections.GetSection(i);				
			}
			
		}
		
		protected virtual void OnDelete(object sender, EventArgs args){			
			DeleteSection((TimeNodeProperties)sender);
		}
		
		protected virtual void OnInsertAfter(object sender, EventArgs args){
			AddSection(int.Parse(((Widget)sender).Name)+1);
		}
		
		protected virtual void OnInsertBefore(object sender, EventArgs args){
			AddSection(int.Parse(((Widget)sender).Name));
		}
		
		protected virtual void OnHotKeyChanged(TimeNodeProperties sender, HotKey prevHotKey, SectionsTimeNode section){
			if (hkList.Contains(section.HotKey)){
			    MessagePopup.PopupMessage(this,MessageType.Warning,
				                        Catalog.GetString("This hotkey is already in use."));
				section.HotKey = prevHotKey;
				sender.Section = section;			
			}
			else if (section.HotKey.Defined)
				hkList.Add(section.HotKey);
		}
	}
}
