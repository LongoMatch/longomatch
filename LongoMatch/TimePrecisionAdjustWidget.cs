// TimePrecisionAdjustWidget.cs
//
//  Copyright (C) 2007 [name of author]
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

namespace LongoMatch
{
	
	
	public partial class TimePrecisionAdjustWidget : Gtk.Bin
	{
		
		private TimeNode tNode;


		public event TimeNodeChangedHandler TimeNodeChanged;
		
		public TimePrecisionAdjustWidget()
		{
			this.Build();
		}
						
				
		public void Reset (){
			startlabel.Text = "";
			stoplabel.Text = "";			
		}
		
		public void SetTimeNode(TimeNode tNode){
			this.tNode=tNode;
			startlabel.Text = TimeString.MSecondsToMSecondsString(tNode.Start);
			stoplabel.Text = TimeString.MSecondsToMSecondsString(tNode.Stop);
			

		}
		
		protected virtual void OnStartTimeAdjusted(long gap)
		{
			if (tNode != null ){
				if (tNode.Start +gap <= 0 ){
					tNode.Start = 0;
				}
				else if ((tNode.Start+gap) < (tNode.Stop -500) ) {
					tNode.Start += gap;					
				}
				//Si el gap introducido hace que sean más de 500ms lo 
				//ajustamos a 500 ms para que se quede parejo
				else {
					tNode.Start = tNode.Stop - 500;
				}
				
				startlabel.Text = TimeString.MSecondsToMSecondsString(tNode.Start);
				if (TimeNodeChanged != null)
						TimeNodeChanged(tNode, tNode.Start);
			}			
		}
		
		protected virtual void OnStopTimeAdjusted(long gap)
		{
			if (tNode != null){
				if ((tNode.Stop + gap) > (tNode.Start+500)){
					tNode.Stop += gap;
					
				}
				else{
					tNode.Stop = tNode.Start + 500;
				}
				stoplabel.Text = TimeString.MSecondsToMSecondsString(tNode.Stop);
				if (TimeNodeChanged != null)
						TimeNodeChanged(tNode, tNode.Stop);
			}
		}

		protected virtual void OnArrow1KeyPressEvent (object o, Gtk.KeyPressEventArgs args)
		{
			Console.WriteLine("test");
		}

		protected virtual void OnArrow1ButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			Console.WriteLine("test");
		}



		
	}
}
	