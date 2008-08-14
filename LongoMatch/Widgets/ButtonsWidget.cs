// ButtonsWidget.cs
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
using Gtk;
using LongoMatch.DB;
using LongoMatch.Handlers;

namespace LongoMatch.Widgets.Component
{
	

	
	//FIXME Make non-sensitive non visible buttons
	
	public partial class ButtonsWidget : Gtk.Bin
	{
				
		private Sections sections;
		private const int MS = 1000;
		
	
		public event NewMarkEventHandler NewMarkEvent;

		
		public ButtonsWidget()
		{
			this.Build();

		}
		
		public Sections Sections{
			set{
				this.sections = value;
				this.Names = value.GetSectionsNames();	
			}
			
		}
			
		public String[] Names {
			
			set{
				button1.Label = value[0];
				button2.Label = value[1];
				button3.Label = value[2];
				button4.Label = value[3];
				button5.Label = value[4];
				button6.Label = value[5];
				button7.Label = value[6];
				button8.Label = value[7];
				button9.Label = value[8];
				button10.Label = value[9];
				button11.Label = value[10];
				button12.Label = value[11];
				button13.Label = value[12];
				button14.Label = value[13];
				button15.Label = value[14];
				button16.Label = value[15];
				button17.Label = value[16];
				button18.Label = value[17];
				button19.Label = value[18];
				button20.Label = value[19];	
			}
		}
		

		
		protected virtual void OnButton1Clicked(object sender, System.EventArgs e)
		{
			
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(0,sections.GetStartTime(0),sections.GetStopTime(0));	

		}

		protected virtual void OnButton2Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(1,sections.GetStartTime(1),sections.GetStopTime(1));			
		}

		protected virtual void OnButton3Clicked(object sender, System.EventArgs e)
		{
			
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(2,sections.GetStartTime(2),sections.GetStopTime(2));
		}

		protected virtual void OnButton4Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(3,sections.GetStartTime(3),sections.GetStopTime(3));
		}

		protected virtual void OnButton5Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(4,sections.GetStartTime(4),sections.GetStopTime(4));
		}

		protected virtual void OnButton6Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(5,sections.GetStartTime(5),sections.GetStopTime(5));
		}

		protected virtual void OnButton7Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(6,sections.GetStartTime(6),sections.GetStopTime(6));
		}

		protected virtual void OnButton8Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(7,sections.GetStartTime(7),sections.GetStopTime(7));
		}

		protected virtual void OnButton9Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(8,sections.GetStartTime(8),sections.GetStopTime(8));
		}

		protected virtual void OnButton10Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(9,sections.GetStartTime(9),sections.GetStopTime(9));
		}

		protected virtual void OnButton11Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(10,sections.GetStartTime(10),sections.GetStopTime(10));
		}

		protected virtual void OnButton12Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(11,sections.GetStartTime(11),sections.GetStopTime(11));

		}

		protected virtual void OnButton13Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(12,sections.GetStartTime(12),sections.GetStopTime(12));
		}

		protected virtual void OnButton14Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(13,sections.GetStartTime(13),sections.GetStopTime(13));
		}

		protected virtual void OnButton15Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(14,sections.GetStartTime(14),sections.GetStopTime(14));
		}

		protected virtual void OnButton16Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(15,sections.GetStartTime(15),sections.GetStopTime(15));
		}

		protected virtual void OnButton17Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(16,sections.GetStartTime(16),sections.GetStopTime(16));
		}

		protected virtual void OnButton18Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(17,sections.GetStartTime(17),sections.GetStopTime(17));
		}

		protected virtual void OnButton19Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(18,sections.GetStartTime(18),sections.GetStopTime(18));
		}

		protected virtual void OnButton20Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(19,sections.GetStartTime(19),sections.GetStopTime(19));
		}
	}
}
