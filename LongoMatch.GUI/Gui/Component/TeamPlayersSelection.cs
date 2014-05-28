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
using Gtk;
using LongoMatch.Interfaces;
using LongoMatch.Store.Templates;
using LongoMatch.Store;
using Gdk;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TeamPlayersSelection : Gtk.Bin
	{
	
		ITeamTemplatesProvider teamsTemplates;
		TeamTemplate template;
		ListStore teams, players;
		
		public TeamPlayersSelection ()
		{
			this.Build ();
			teams = new ListStore (typeof(string));
			teamscombobox.Model = teams;
			teamscombobox.Changed += HandleChanged;
		}

		public ITeamTemplatesProvider TemplatesProvider {
			set {
				teamsTemplates = value;
				teams.Clear ();
				foreach (string name in teamsTemplates.TemplatesNames) {
					teams.AppendValues (name);
				}
				teamscombobox.Active = 0;
			}
		}
		
		public TeamTemplate Template {
			get {
				return template;
			}
		}
		
		public void Load (string name) {
			if (name != null) {
				template = teamsTemplates.Load (name);
				if (template.Shield != null) {
					shieldimage.Pixbuf = template.Shield.Value;
				}
				namelabel.Text = template.TeamName;
				//teamtaggerwidget.Team = template;	
			}
		}
		
		void HandleChanged (object sender, EventArgs e)
		{
			Load (teamscombobox.ActiveText);
		}
	}
}

