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
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
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
		private TeamTemplate template;
		private Player selectedPlayer;
		private bool edited;
		
		public TeamTemplateWidget()
		{
			this.Build();			
		}
		
		public TeamTemplate TeamTemplate{
			get{return template;}
			set{
				this.template= value;
				edited = false;
				Gtk.TreeStore playersListStore = new Gtk.TreeStore (typeof (Player));
				for (int i=0;i<template.PlayersCount;i++)
					playersListStore.AppendValues (template.GetPlayer(i));				
				playerpropertiestreeview1.Model=playersListStore;
			}
		}
		
		public bool Edited{
			get{return edited;}
			set{edited=value;}
		}
								
		private void EditSelectedPlayer(){
			LongoMatch.Gui.Dialog.EditPlayerDialog dialog = new LongoMatch.Gui.Dialog.EditPlayerDialog();
			dialog.Player=selectedPlayer;
			dialog.TransientFor = (Gtk.Window) Toplevel;
			dialog.Run();
			dialog.Destroy();
			edited = true;
		}

		protected virtual void OnPlayerpropertiestreeview1PlayerClicked (LongoMatch.TimeNodes.Player player)
		{
			selectedPlayer = player;
			EditSelectedPlayer();
		}

		protected virtual void OnPlayerpropertiestreeview1PlayerSelected (LongoMatch.TimeNodes.Player player)
		{
			selectedPlayer = player;
		}	
	}
}
