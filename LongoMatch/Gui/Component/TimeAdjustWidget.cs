// TimeAdjustWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using LongoMatch.Store;

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

		public void SetTimeNode(Category tNode) {
			spinbutton1.Value=tNode.Start.Seconds;
			spinbutton2.Value=tNode.Stop.Seconds;
		}

		public Time GetStartTime() {
			return new Time {Seconds = (int)(spinbutton1.Value)};
		}

		public Time GetStopTime() {
			return new Time {Seconds = (int)(spinbutton2.Value)};
		}

		protected virtual void OnSpinbutton1ValueChanged(object sender, System.EventArgs e)
		{
			if(LeadTimeChanged != null)
				LeadTimeChanged(this,new EventArgs());
		}

		protected virtual void OnSpinbutton2ValueChanged(object sender, System.EventArgs e)
		{
			if(LagTimeChanged != null)
				LagTimeChanged(this,new EventArgs());
		}
	}
}
