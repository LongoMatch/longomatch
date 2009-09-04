// 
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;
using System.Collections.Generic;
using Gtk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;


namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TeamTemplateWidget : Gtk.Bin
	{
		private List<PlayerProperties> pplist;
		
		public TeamTemplateWidget()
		{
			this.Build();
			pplist = new List<PlayerProperties>();
			table1.NColumns =(uint) 5;
		}
		
		public void SetTeamTemplate(TeamTemplate template){
			int playersCount = template.PlayersCount;
			
			pplist.Clear();
			
			foreach (Widget w in table1.AllChildren){
					w.Unrealize();
					table1.Remove(w);
			}
			
			for( int i=0;i<playersCount;i++){
				PlayerProperties pp = new PlayerProperties();
				
				pp.Name = i.ToString();
				pp.Title =  "Player "+(i+1);			
				pp.Player = template.GetPlayer(i);
			
				AddPlayerToTable(i,template.PlayersCount,pp);			
			}		
			
		}
		
		public TeamTemplate GetTeamTemplate(){
			TeamTemplate template = new TeamTemplate();
			foreach (PlayerProperties pp in pplist){
				template.AddPlayer(pp.Player);					
			}
			return template;
		}
		
		private void AddPlayerToTable(int index, int count, PlayerProperties pp){
			uint row_top,row_bottom,col_left,col_right;
			
			pplist.Insert(index,pp);
			table1.NRows =(uint) (count/5);			
			row_top =(uint) (index/table1.NColumns);
			row_bottom = (uint) row_top+1 ;
			col_left = (uint) index%table1.NColumns;
			col_right = (uint) col_left+1 ;
			
			table1.Attach(pp,col_left,col_right,row_top,row_bottom);	
			pp.Show();
		}
	}
}
