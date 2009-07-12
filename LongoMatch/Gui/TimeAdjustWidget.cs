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

		public event EventHandler LeadTimeChanged;
		public event EventHandler LagTimeChanged;
		
		public TimeAdjustWidget()
		{
			this.Build();
		}
		
		public void SetTimeNode(SectionsTimeNode tNode){
			Console.WriteLine("SetTimeNode, Start{0}, Stop{1}",tNode.Start.Seconds, tNode.Stop.Seconds);
			spinbutton1.Value=tNode.Start.Seconds;
			spinbutton2.Value=tNode.Stop.Seconds;			
		}
		
		public Time GetStartTime(){
			Time t = new  Time ((int)(spinbutton1.Value)*Time.SECONDS_TO_TIME);
			Console.WriteLine((int)(spinbutton1.Value)*Time.SECONDS_TO_TIME);
			Console.WriteLine(t.Seconds);
			return new Time((int)(spinbutton1.Value)*Time.SECONDS_TO_TIME);
		}
		
		public Time GetStopTime(){
			return new Time ((int)(spinbutton2.Value)*Time.SECONDS_TO_TIME);
		}

		protected virtual void OnSpinbutton1ValueChanged (object sender, System.EventArgs e)
		{
			if (LeadTimeChanged != null)
				LeadTimeChanged(this,new EventArgs());
		}

		protected virtual void OnSpinbutton2ValueChanged (object sender, System.EventArgs e)
		{
			if (LagTimeChanged != null)
				LagTimeChanged(this,new EventArgs());
		}
	}
}
