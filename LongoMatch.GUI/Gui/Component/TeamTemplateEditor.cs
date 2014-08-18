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
using System.Linq;
using System.Collections.Generic;
using Gtk;
using Gdk;
using LongoMatch.Store.Templates;
using LongoMatch.Store;
using Mono.Unix;

using Image = LongoMatch.Common.Image;
using Color = LongoMatch.Common.Color;
using LongoMatch.Common;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Dialog;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Drawing.Cairo;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TeamTemplateEditor : Gtk.Bin
	{
		public event EventHandler TemplateSaved;
	
		Player loadedPlayer;
		TeamTemplate template;
		bool edited, ignoreChanges;
		List<Player> selectedPlayers;
		TeamTagger teamtagger;
		
		public TeamTemplateEditor ()
		{
			this.Build ();
			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea));
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;
			ConnectSignals ();
		}

		public bool Edited {
			get {
				return edited;
			}
			protected set {
				edited = value;
				savebutton.Sensitive = edited;
			}
		}
		
		public TeamTemplate  Team {
			set {
				template = value;
				ignoreChanges = true;
				if (template.Shield != null) {
					shieldimage.Pixbuf = template.Shield.Value;
				} else {
					shieldimage.Pixbuf = IconTheme.Default.LoadIcon (Constants.LOGO_ICON,
					                                                 Constants.MAX_SHIELD_ICON_SIZE,
					                                                 IconLookupFlags.ForceSvg);
				}
				teamnameentry.Text = template.TeamName;
				FillFormation ();
				teamtagger.LoadTeams (template, null, null);
				ignoreChanges = false;
				Edited = false;
			}
		}
		
		void ConnectSignals () {
			newplayerbutton.Clicked += HandleNewPlayerClicked;
			savebutton.Clicked += HandleSaveTemplateClicked;
			deletebutton.Clicked += HandleDeletePlayerClicked;
			
			shieldeventbox.ButtonPressEvent += HandleShieldButtonPressEvent;
			playereventbox.ButtonPressEvent += HandlePlayerButtonPressEvent;
			
			teamnameentry.Changed += HandleEntryChanged;
			nameentry.Changed += HandleEntryChanged;
			positionentry.Changed += HandleEntryChanged;
			numberspinbutton.Changed += HandleEntryChanged;
			heightspinbutton.Changed += HandleEntryChanged;
			weightspinbutton.Changed += HandleEntryChanged;
			nationalityentry.Changed += HandleEntryChanged;
			mailentry.Changed += HandleEntryChanged;
			
			applybutton.Clicked += (s,e) => {ParseTactics();}; 
			tacticsentry.Activated += (s, e) => {ParseTactics();};
			
			datebutton.Clicked += HandleCalendarbuttonClicked; 
			
			Edited = false;
		}

		void HandleEntryChanged (object sender, EventArgs e)
		{
			if (ignoreChanges == true)
				return;

			if (sender == teamnameentry) {
				template.TeamName = (sender as Entry).Text;
			} else if (sender == nameentry) {
				loadedPlayer.Name = (sender as Entry).Text;
			} else if (sender == positionentry) {
				loadedPlayer.Position = (sender as Entry).Text;
			} else if (sender == numberspinbutton) {
				loadedPlayer.Number = (sender as SpinButton).ValueAsInt;
			} else if (sender == heightspinbutton) {
				loadedPlayer.Height = (sender as SpinButton).ValueAsInt;
			} else if (sender == weightspinbutton) {
				loadedPlayer.Weight = (sender as SpinButton).ValueAsInt;
			} else if (sender == nationalityentry) {
				loadedPlayer.Nationality = (sender as Entry).Text;
			} else if (sender == mailentry) {
				loadedPlayer.Mail = (sender as Entry).Text;
			}
			Edited = true;
			drawingarea.QueueDraw ();
		}

		void FillFormation () {
			tacticsentry.Text = template.FormationStr;
			nplayerslabel.Text = template.PlayingPlayers.ToString();
		}
		
		void LoadPlayer (Player p) {
			loadedPlayer = p;
			nameentry.Text = p.Name;
			positionentry.Text = p.Position;
			numberspinbutton.Value = p.Number;
			heightspinbutton.Value = p.Height;
			weightspinbutton.Value = p.Weight;
			nationalityentry.Text = p.Number.ToString();
			bdaylabel.Text = p.Birthday.ToShortDateString();
			playerimage.Pixbuf = PlayerPhoto (p);
		}
		
		void ParseTactics () {
			try {
				template.FormationStr = tacticsentry.Text;
				teamtagger.Reload ();
				Edited = true;
			} catch {
				Config.GUIToolkit.ErrorMessage (
					Catalog.GetString ("Could not parse tactics string"));
			}
			FillFormation ();
		}
		
		Pixbuf PlayerPhoto (Player p) {
			Pixbuf playerImage;
				
			if (p.Photo != null) {
				playerImage = p.Photo.Value;
			} else {
				playerImage = Stetic.IconLoader.LoadIcon (this, "stock_person", IconSize.Dialog);
			}
			return playerImage;
		}
		
		void PlayersSelected (List<Player> players) {
			ignoreChanges = true;
			playerframe.Sensitive = players.Count == 1;
			selectedPlayers = players;
			deletebutton.Sensitive = players.Count != 0;
			playerframe.Sensitive = players.Count != 0;
			if (players.Count == 1) {
				LoadPlayer (players[0]);
			} else {
				loadedPlayer = null;
			}
			ignoreChanges = false;
		}
		
		void DeleteSelectedPlayers () {
			if (selectedPlayers.Count == 0) {
				return;
			}
			
			foreach (Player p in selectedPlayers) {
				string msg = Catalog.GetString ("Do you want to delete player: ") + p.Name;
				if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
					template.List.Remove (p);
					Edited = true;
				}
			}
			Team = template;
		}
		
		void HandlePlayersSelectionChangedEvent (List<Player> players)
		{
			PlayersSelected (players);
		}
		
		void HandleSaveTemplateClicked (object sender, EventArgs e)
		{
			if (template != null) {
				Config.TeamTemplatesProvider.Update (template);			
				Edited = false;
			}
			if (TemplateSaved != null)
				TemplateSaved (this, null);
		}

		void HandleNewPlayerClicked (object sender, EventArgs e)
		{
			Player p = template.AddDefaultItem (template.List.Count);
			teamtagger.Reload ();
			teamtagger.Select (p);
			Edited = true;
		}

		void HandleDeletePlayerClicked (object sender, EventArgs e)
		{
			DeleteSelectedPlayers ();
		}
		
		void HandleKeyPressEvent (object o, KeyPressEventArgs args)
		{
			if (args.Event.Key == Gdk.Key.Delete) {
				DeleteSelectedPlayers ();
			}
		}

		void HandlePlayerButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			Image player;
			Pixbuf pix = Helpers.Misc.OpenImage (this);
			
			if (pix == null) {
				return;
			}
			
			player = new Image (pix);
			player.ScaleInplace (Constants.MAX_PLAYER_ICON_SIZE,
			                     Constants.MAX_PLAYER_ICON_SIZE); 
			if (player != null && loadedPlayer != null) {
				playerimage.Pixbuf = player.Value;
				loadedPlayer.Photo = player;
				teamtagger.Reload ();
				Edited = true;
			}
		}

		void HandleShieldButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			Image shield;
			Pixbuf pix = Helpers.Misc.OpenImage (this);
			
			if (pix == null) {
				return;
			}
			
			shield = new Image (pix);
			shield.ScaleInplace (Constants.MAX_SHIELD_ICON_SIZE,
			                     Constants.MAX_SHIELD_ICON_SIZE); 
			if (shield != null)
			{
				shieldimage.Pixbuf = shield.Value;
				template.Shield = shield;
				Edited = true;
			}
		}

		void HandleCalendarbuttonClicked(object sender, System.EventArgs e)
		{
			loadedPlayer.Birthday = Config.GUIToolkit.SelectDate (loadedPlayer.Birthday, this);
			bdaylabel.Text = loadedPlayer.Birthday.ToShortDateString ();
		}
	}
}

