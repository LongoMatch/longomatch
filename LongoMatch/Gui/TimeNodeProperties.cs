// TimeNodeProperties.cs
//
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
using Gdk;
using Gtk;
using Mono.Unix;
using LongoMatch.TimeNodes;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Component
{
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial  class TimeNodeProperties : Gtk.Bin
	{

		private SectionsTimeNode stn = null;
		
		public TimeNodeProperties()
		{
			this.Build();
		}
		
		public string Title {
			set{
				titlelabel.Text=value;	
			}
		}
		
		public SectionsTimeNode Section
		{
			set{
				this.stn = value;
				UpdateGui();				
			}
			
			get{
				UpdateSectionTimeNode();
				return stn;
			}
		}

		private void  UpdateGui(){
			if ( stn != null){
				nameentry.Text = stn.Name;
				timeadjustwidget1.SetTimeNode(stn);
				colorbutton1.Color = stn.Color;
				
				//FIXME 1.0 Every TimeNode object must have a HotKey != null
				if (stn.HotKey.Defined){
					hotKeyLabel.Text = stn.HotKey.ToString();
				}
				else hotKeyLabel.Text = Catalog.GetString("none"); 
			}
		}

		private void UpdateSectionTimeNode(){
			stn.Name = nameentry.Text;
			stn.Start=timeadjustwidget1.GetStartTime();
			stn.Stop=timeadjustwidget1.GetStopTime();
			stn.Color=colorbutton1.Color;
		}
		
		protected virtual void OnChangebutonClicked (object sender, System.EventArgs e)
		{
			HotKeySelectorDialog dialog = new HotKeySelectorDialog();
			if (dialog.Run() == (int)ResponseType.Ok){
				stn.HotKey=dialog.HotKey;
				UpdateGui();
			}
			dialog.Destroy();		
		}
		
	

	}
}
