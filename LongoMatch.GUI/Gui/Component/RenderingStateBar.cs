// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using VAS.Core.Interfaces.GUI;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class RenderingStateBar : Gtk.Bin, IRenderingStateBar
	{
		public event EventHandler Cancel;
		public event EventHandler ManageJobs;

		public RenderingStateBar ()
		{
			this.Build ();
			progressbar.CanFocus = false;
			cancellbutton.CanFocus = false;
			statebutton.CanFocus = false;
			
			statebutton.Clicked += delegate(object sender, EventArgs e) {
				if (ManageJobs != null)
					ManageJobs (this, null);
			};
			
			cancellbutton.Clicked += delegate(object sender, EventArgs e) {
				if (Cancel != null)
					Cancel (this, null);
			};
		}

		public bool JobRunning {
			set {
				Visible = value;
			}
		}

		public string Text {
			set {
				statebutton.Label = value;
			}
		}

		public string ProgressText {
			set {
				progressbar.Text = value;
			}
		}

		public double Fraction {
			set {
				progressbar.Fraction = value;
			}
			get {
				return progressbar.Fraction;
			}
			
		}
	}
}

