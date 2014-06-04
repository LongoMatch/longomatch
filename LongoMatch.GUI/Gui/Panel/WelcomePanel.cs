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
using LongoMatch.Handlers;
using LongoMatch.Common;

namespace LongoMatch.Gui.Panel
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class WelcomePanel : Gtk.Bin
	{
		public WelcomePanel ()
		{
			this.Build ();
			backgroundwidget.Background = Gdk.Pixbuf.LoadFromResource (Constants.BACKGROUND);
			Bind ();
		}
		
		void Bind ()
		{
			openbutton.Clicked += (sender, e) => {
				Config.EventsBroker.EmitOpenProject ();};
			newbutton.Clicked += (sender, e) => {
				Config.EventsBroker.EmitNewProject ();};
			teamsbutton.Clicked += (sender, e) => {
				Config.EventsBroker.EmitManageTeams ();};
			sportsbutton.Clicked += (sender, e) => {
				Config.EventsBroker.EmitManageCategories ();};
		    preferencesbutton.Clicked += (sender, e) => {
				Config.EventsBroker.EmitEditPreferences ();};
		    projectsbutton.Clicked += (sender, e) =>  {
				Config.EventsBroker.EmitManageProjects ();};
		}
	}
}

