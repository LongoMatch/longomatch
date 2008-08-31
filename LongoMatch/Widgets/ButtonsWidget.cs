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
using System.Collections.Generic;

namespace LongoMatch.Gui.Component
{
	

	
	//FIXME Make non-sensitive non visible buttons
	
	public partial class ButtonsWidget : Gtk.Bin
	{
				
		private Sections sections;
		private const int MS = 1000;
		private Button[] bList;
	
		public event NewMarkEventHandler NewMarkEvent;

		
		public ButtonsWidget()
		{
			int i=19;
			
			this.Build();
			
			bList = new Button[20];
			foreach (Button b in this.table1){
				bList[i]=b;
				i--;
			}
		}
		
		public Sections Sections{
			set{
				this.sections = value;
				this.Names = value.GetSectionsNames();	
				for( int i=0;i<20;i++){
					bList[i].Sensitive=sections.GetVisibility(i);
				}
			}
			
		}
			
		public String[] Names {
			
			set{
				for (int i=0;i<20;i++)
					bList[i].Label = value[i];
			}
		}
		

		
		protected virtual void OnButton1Clicked(object sender, System.EventArgs e)
		{
			
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(0);	

		}

		protected virtual void OnButton2Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(1);			
		}

		protected virtual void OnButton3Clicked(object sender, System.EventArgs e)
		{
			
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(2);
		}

		protected virtual void OnButton4Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(3);
		}

		protected virtual void OnButton5Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(4);
		}

		protected virtual void OnButton6Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(5);
		}

		protected virtual void OnButton7Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(6);
		}

		protected virtual void OnButton8Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(7);
		}

		protected virtual void OnButton9Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(8);
		}

		protected virtual void OnButton10Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(9);
		}

		protected virtual void OnButton11Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(10);
		}

		protected virtual void OnButton12Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(11);

		}

		protected virtual void OnButton13Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(12);
		}

		protected virtual void OnButton14Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(13);
		}

		protected virtual void OnButton15Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(14);
		}

		protected virtual void OnButton16Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(15);
		}

		protected virtual void OnButton17Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(16);
		}

		protected virtual void OnButton18Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(17);
		}

		protected virtual void OnButton19Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(18);
		}

		protected virtual void OnButton20Clicked(object sender, System.EventArgs e)
		{
			if (NewMarkEvent != null && this.sections != null)
				this.NewMarkEvent(19);
		}
	}
}
