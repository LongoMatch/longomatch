// TimePrecisionAdjustWidget.cs
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

namespace LongoMatch
{
	
	
	public partial class TimePrecisionAdjustWidget : Gtk.Bin
	{
		
		private MediaTimeNode tNode;


		public event TimeNodeChangedHandler TimeNodeChanged;
		
		public TimePrecisionAdjustWidget()
		{
			this.Build();
		}
						
				
		public void Reset (){
			startlabel.Text = "";
			stoplabel.Text = "";			
		}
		
		public void SetTimeNode(MediaTimeNode tNode){
			this.tNode=tNode;
			startlabel.Text = tNode.Start.ToMSecondsString();
			stoplabel.Text = tNode.Stop.ToMSecondsString();
			

		}
		
		protected virtual void OnStartTimeAdjusted(int gap)
		{
			
			if (tNode != null ){
				if (tNode.Start.MSeconds +gap <= 0 ){
					tNode.Start.MSeconds = 0;
				}
				else if ((tNode.Start.MSeconds+gap) < (tNode.Stop.MSeconds -500) ) {
					tNode.Start.MSeconds += gap;					
				}
				//Si el gap introducido hace que sean más de 500ms lo 
				//ajustamos a 500 ms para que se quede parejo
				else {
					tNode.Start.MSeconds = tNode.Stop.MSeconds - 500;
				}
				
				startlabel.Text = tNode.Start.ToMSecondsString();
				if (TimeNodeChanged != null)
						TimeNodeChanged(tNode, tNode.Start);
			}			
		}
		
		protected virtual void OnStopTimeAdjusted(int gap)
		{

			if (tNode != null){
				if ((tNode.Stop.MSeconds + gap) > (tNode.Start.MSeconds+500)){
					tNode.Stop.MSeconds += gap;
					
				}
				else{
					tNode.Stop.MSeconds = tNode.Start.MSeconds + 500;
				}
				stoplabel.Text = tNode.Stop.ToMSecondsString();
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
	