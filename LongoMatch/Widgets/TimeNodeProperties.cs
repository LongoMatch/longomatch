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
using LongoMatch.TimeNodes;

namespace LongoMatch.Widgets.Component
{
	
	
	public partial  class TimeNodeProperties : Gtk.Bin
	{


		
		public TimeNodeProperties()
		{
			this.Build();
		}
		
		public string Title {
			set{
				GtkLabel1.Text=value;	
			}
		}
		
		public SectionsTimeNode TimeNode
		{
			set{
				entry1.Text = value.Name;
				this.checkbutton2.Active = value.Visible;
				timeadjustwidget1.SetTimeNode(value);	
			}
			
			get{
				return new SectionsTimeNode (entry1.Text,timeadjustwidget1.GetStartTime(),timeadjustwidget1.GetStopTime(),this.checkbutton2.Active);
			}
		}
		
		public Color Color{
			set{
				this.colorbutton1.Color = value;
			}
			get{
				return this.colorbutton1.Color;
			}
		}
		
	

	}
}
