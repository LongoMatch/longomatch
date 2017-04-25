//
//  Copyright (C) 2016 
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
using LongoMatch.Drawing.Widgets;
using LongoMatch.Gui.Menus;
using VAS.Core.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public class Timeline : VAS.UI.Component.Timeline
	{
		protected PeriodsMenu periodsmenu;

		public Timeline () : base ()
		{
			periodsmenu = new PeriodsMenu ();
			menu = new SportsPlaysMenu ();
			((LMPlaysTimeline)timeline).ShowPeriodsMenuEvent += HandleShowTimerMenuEvent;
		}

		protected void HandleShowTimerMenuEvent (Timer timer, Time time)
		{
			periodsmenu.ShowMenu (ViewModel.Project.Model, timer, time, timeline);
		}
	}
}
