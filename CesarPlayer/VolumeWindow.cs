// VolumeWindow.cs 
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

namespace CesarPlayer
{
	
	
	public partial class VolumeWindow : Gtk.Window
	{

	
		
		public event         VolumeChangedHandler VolumeChanged;
		
		
		public VolumeWindow() : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build();
		}
		
		public void SetLevel(int level){
			volumescale.Value = level ;
		}

		protected virtual void OnLessbuttonClicked(object sender, System.EventArgs e)
		{
			volumescale.Value = volumescale.Value - 5;
		}

		protected virtual void OnMorebuttonClicked(object sender, System.EventArgs e)
		{
			volumescale.Value = volumescale.Value + 5;
		}

		protected virtual void OnVolumescaleValueChanged(object sender, System.EventArgs e)
		{
			VolumeChanged((int)volumescale.Value);
		}

		protected virtual void OnFocusOutEvent (object o, Gtk.FocusOutEventArgs args)
		{
			this.Hide();
		}


		
	}
}
