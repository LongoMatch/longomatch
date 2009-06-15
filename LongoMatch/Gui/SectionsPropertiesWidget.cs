// SectionsPropertiesWidget.cs
//  Copyright (C) 2007 Andoni Morales Alastruey
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
		List<TimeNodeProperties> tndlist;
		Project project;
		
		public SectionsPropertiesWidget()
		{
			this.Build();
			tndlist = new List<TimeNodeProperties>();
			
		}
		
		public void SetProject(Project project){
			this.project = project;
			SetSections(project.Sections);
		}
		
		public void SetSections(Sections sections){
			int sectionsCount = sections.Count;
			table1.NColumns =(uint) 5;
			table1.NRows =(uint) (sectionsCount/5);
			
			tndlist.Clear();
			
			foreach (Widget w in table1.AllChildren){
					w.Unrealize();
					table1.Remove(w);
			}
			
			for( int i=0;i<sectionsCount;i++){
				TimeNodeProperties tnp = new TimeNodeProperties();
				tnp.Name = i.ToString();
				tnp.Title =  "Section "+(i+1);			
				tnp.Section = sections.GetSection(i);	
				tnp.DeleteSection += new EventHandler(OnDelete);
				tnp.InsertAfter += new EventHandler(OnInsertAfter);
				tnp.InsertBefore += new EventHandler(OnInsertBefore);
				tndlist.Add(tnp);	
				
				uint row_top =(uint) (i/table1.NColumns);
				uint row_bottom = (uint) row_top+1 ;
				uint col_left = (uint) i%table1.NColumns;
				uint col_right = (uint) col_left+1 ;
				table1.Attach(tnp,col_left,col_right,row_top,row_bottom);	
				tnp.Show();
			}
			
		}
		
		
		
		public Sections GetSections (){
			Sections sections = new Sections();
			foreach (TimeNodeProperties tnp in tndlist){
				sections.AddSection(tnp.Section);					
			}
			return sections;
		}
		
		private void AddSection (int index){
			SectionsTimeNode tn;
			HotKey hkey = new HotKey();
			
			Time start = new Time(10*Time.SECONDS_TO_TIME);
			Time stop = new Time(10*Time.SECONDS_TO_TIME);
			
			
			tn  = new SectionsTimeNode("New Section",start,stop,hkey,new Color(Byte.MaxValue,Byte.MinValue,Byte.MinValue));
			
			if (project != null){
				project.AddSectionAtPos(tn,index);
				SetSections(project.Sections);
			}
			else{				
				Sections sections = GetSections();
				sections.AddSectionAtPos(tn,index);
				SetSections(sections);
			}
			
				
			
		}
		
		protected virtual void OnDelete(object sender, EventArgs args){
			int index = int.Parse(((Widget)sender).Name);
			if(project!= null){
				project.DeleteSection(index);
				SetSections(project.Sections);	
			}
			else{
				Sections sections = GetSections();
				sections.RemoveSection(index);
				SetSections(sections);
			}
		}
		
		protected virtual void OnInsertAfter(object sender, EventArgs args){
			AddSection(int.Parse(((Widget)sender).Name)+1);
		}
		
		protected virtual void OnInsertBefore(object sender, EventArgs args){
			AddSection(int.Parse(((Widget)sender).Name));
		}
	}
}
