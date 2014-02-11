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

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TeamPlayersSelection : Gtk.Bin
	{
	
		ITeamTemplatesProvider teamsTemplates;
		TeamTemplate template;
		ListStore teams, players;
		
		enum Columns {
			Desc,
			Number,
			Playing,
			Photo,
			Player,
			NumCols,
		}
		
		public TeamPlayersSelection ()
		{
			CellRendererToggle togglerenderer;
			CellRendererText textrenderer;
			
			this.Build ();
			teams = new ListStore (typeof(string));
			players = new ListStore (typeof(string), typeof(string), typeof(bool), typeof(Pixbuf), typeof(Player));
			teamscombobox.Model = teams;
			playersiconview.Model = players;
			teamscombobox.Changed += HandleChanged;
			
			togglerenderer = new CellRendererToggle ();
			togglerenderer.Radio = false;
			togglerenderer.Toggled += HandleToggled;
			textrenderer = new CellRendererText ();
			playersiconview.PixbufColumn = (int) Columns.Photo;
			playersiconview.TooltipColumn = (int) Columns.Desc;
			playersiconview.PackEnd (textrenderer, false);
			playersiconview.PackEnd (togglerenderer, false);
			playersiconview.SetAttributes (togglerenderer, "active", Columns.Playing);
			playersiconview.SetAttributes (textrenderer, "text", Columns.Number);
			playersiconview.Orientation = Orientation.Horizontal;
			playersiconview.SelectionMode = SelectionMode.None;
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
				players.Clear ();
				foreach (Player p in template) {
					Pixbuf playerImage;
					
					if (p.Photo != null) {
						playerImage = p.Photo.Value;
					} else {
						playerImage = Stetic.IconLoader.LoadIcon (this, "stock_person", IconSize.Dialog);
					}
					players.AppendValues (String.Format("{0} {1}", p.Name, p.Number),
					                      p.Number.ToString(), p.Playing, playerImage, p);
				}
			}
		}
		
		void HandleChanged (object sender, EventArgs e)
		{
			Load (teamscombobox.ActiveText);
		}
		
		void HandleToggled (object o, ToggledArgs args)
		{
			Player player;
			TreeIter iter;
			
			playersiconview.Model.GetIterFromString (out iter, args.Path);
			player = playersiconview.Model.GetValue (iter, (int) Columns.Player) as Player;
			player.Playing = !(o as CellRendererToggle).Active;
			playersiconview.Model.SetValue (iter, (int) Columns.Playing, player.Playing);
		}
	}
}

