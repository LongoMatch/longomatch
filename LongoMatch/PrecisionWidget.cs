// PrecisionWidget.cs
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
	
	// Evento porducido al cambiar la position del video mediante los 
	// botones de ajuste
	
	public delegate void PositionAdjustedHandler (long gap);
	
	public partial  class PrecisionWidget : Gtk.Bin
	{

		public event         PositionAdjustedHandler PositionAdjusted;
		public PrecisionWidget()
		{
			this.Build();
		}



		protected virtual void OnMbuttonClicked (object sender, System.EventArgs e)
		{
			PositionAdjusted(+40);
		}

		protected virtual void OnMmbuttonClicked (object sender, System.EventArgs e)
		{
			PositionAdjusted(+500);
		}

		protected virtual void OnMmmbuttonClicked (object sender, System.EventArgs e)
		{
			PositionAdjusted(+1000);
		}

		protected virtual void OnLbuttonClicked (object sender, System.EventArgs e)
		{
			PositionAdjusted(-40);
		}

		protected virtual void OnLlbuttonClicked (object sender, System.EventArgs e)
		{
			PositionAdjusted(-500);
		}

		protected virtual void OnLllbuttonClicked (object sender, System.EventArgs e)
		{
			PositionAdjusted(-1000);
		}

		
		
	}
}
