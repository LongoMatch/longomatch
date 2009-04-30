// TimeAdjustWidget.cs
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
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TimeAdjustWidget : Gtk.Bin
	{

		
		public TimeAdjustWidget()
		{
			this.Build();
		}
		
		public void SetTimeNode(SectionsTimeNode tNode){
			spinbutton1.Value=tNode.Start.Seconds;
			spinbutton2.Value=tNode.Stop.Seconds;			
		}
		
		public Time GetStartTime(){
			return new Time((int)spinbutton1.Value*Time.SECONDS_TO_TIME);
		}
		
		public Time GetStopTime(){
			return new Time ((int)spinbutton2.Value*Time.SECONDS_TO_TIME);
		}
	}
}
