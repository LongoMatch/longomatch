//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Collections.Generic;
using LongoMatch.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class PeriodsRecoder : Gtk.Bin
	{
		int currentPeriod;
		uint timeoutID;
		DateTime currentPeriodStart;

		public PeriodsRecoder ()
		{
			this.Build ();
			startbutton.Visible = true;
			stopbutton.Visible = false;
			startbutton.Clicked += HandleStartClicked;;
			stopbutton.Clicked += HandleStopClicked;
			closebutton.Clicked += HandleCloseClicked;
			currentPeriod = 0;
		}
		
		public List<string> GamePeriods {
			set;
			get;
		}
		
		public Period Period {
			set;
			get;
		}
		
		Time CurrentTime {
			get {
				return (new Time ((int)(DateTime.UtcNow - currentPeriodStart).TotalMilliseconds));
			}
		}
		
		void HandleStopClicked (object sender, EventArgs e)
		{
			GLib.Source.Remove (timeoutID);
			Period.Stop (CurrentTime);
			
			startbutton.Visible = false;
			stopbutton.Visible = true;
		}

		void HandleStartClicked (object sender, EventArgs e)
		{
			string periodName;
			
			startbutton.Visible = false;
			stopbutton.Visible = true;
			
			if (GamePeriods != null && GamePeriods.Count > currentPeriod) {
				periodName = GamePeriods[currentPeriod];
			} else {
				periodName = (currentPeriod + 1).ToString ();
			}
			Period.Start (new Time (0), periodName);
			currentPeriodStart = DateTime.UtcNow;
			timeoutID = GLib.Timeout.Add (200, UpdateTime);
		}
		
		bool UpdateTime () {
			timelabel.Markup = CurrentTime.ToMSecondsString ();
			return true;
		}
		
		void HandleCloseClicked (object sender, EventArgs e)
		{
			Config.EventsBroker.EmitCloseOpenedProject ();
		}
	}
}