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
using Gtk;
using Gdk;
using LongoMatch.Store.Templates;
using LongoMatch.Store;
using Mono.Unix;

using Image = LongoMatch.Common.Image;
using LongoMatch.Common;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Dialog;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class TeamTemplateEditor : Gtk.Bin
	{
	
		enum Columns {
			Desc,
			Photo,
			Tooltip,
			Player,
			NumCols,
		}
	
		ListStore players;
		Player loadedPlayer;
		List<Player> selectedPlayers;
		TreeIter loadedPlayerIter;
		TeamTemplate template;
		bool edited;
		
		public TeamTemplateEditor ()
		{
			this.Build ();
			
			players = new ListStore (typeof(string), typeof(Pixbuf), typeof(string), typeof(Player));

			playersiconview.Model = players;
			playersiconview.Reorderable = true;
			playersiconview.TextColumn = (int) Columns.Desc;
			playersiconview.PixbufColumn = (int) Columns.Photo;
			playersiconview.SelectionMode = SelectionMode.Multiple;
			playersiconview.SelectionChanged += HandlePlayersSelectionChanged;;
			playersiconview.KeyPressEvent += HandleKeyPressEvent;
			
			ConnectSignals ();
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
			
			datebutton.Clicked += HandleCalendarbuttonClicked; 
			
			Edited = false;
		}

		void HandleEntryChanged (object sender, EventArgs e)
		{
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
		}

		public TeamTemplate  Team {
			set {
				template = value;
				
				players.Clear ();
				foreach (Player p in value) {
					AddPlayer (p);
				}
				if (template.Shield != null) {
					shieldimage.Pixbuf = template.Shield.Value;
				} else {
					shieldimage.Pixbuf = Gdk.Pixbuf.LoadFromResource ("logo.svg");
				}
				teamnameentry.Text = template.TeamName;
				Edited = false;
			}
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
		
		Pixbuf PlayerPhoto (Player p) {
			Pixbuf playerImage;
				
			if (p.Photo != null) {
				playerImage = p.Photo.Value;
			} else {
				playerImage = Stetic.IconLoader.LoadIcon (this, "stock_person", IconSize.Dialog);
			}
			return playerImage;
		}
		
		TreeIter AddPlayer (Player p) {
			return players.AppendValues (String.Format("{0} #{1}", p.Name, p.Number),
			                             PlayerPhoto (p), p.Number.ToString(), p);
		}
		
		void PlayersSelected (List<Player> players) {
			playerframe.Sensitive = players.Count == 1;
			
			selectedPlayers = players;
			deletebutton.Sensitive = players.Count != 0;
			playerframe.Sensitive = players.Count != 0;
			if (players.Count == 1) {
				LoadPlayer (players[0]);
			} else {
				loadedPlayer = null;
			}
		}
		
		void DeleteSelectedPlayers () {
			if (selectedPlayers.Count == 0) {
				return;
			}
			
			foreach (Player p in selectedPlayers) {
				string msg = Catalog.GetString ("Do you want to delete player: ") + p.Name;
				if (Config.GUIToolkit.QuestionMessage (msg, null, this)) {
					template.Remove (p);
					Edited = true;
				}
			}
			Team = template;
		}
		
		void HandlePlayersSelectionChanged (object sender, EventArgs e)
		{
			TreeIter iter;
			List<Player> list;
			TreePath[] pathArray;
			
			list = new List<Player>();
			pathArray = playersiconview.SelectedItems;
				
			for(int i=0; i< pathArray.Length; i++) {
				Player player;
				
				playersiconview.Model.GetIterFromString (out iter, pathArray[i].ToString());
				player = playersiconview.Model.GetValue (iter, (int)Columns.Player) as Player; 
				list.Add (player);
				if (i== 0) {
					loadedPlayerIter = iter;
				}
			}
			PlayersSelected (list);
		}
		
		void HandleSaveTemplateClicked (object sender, EventArgs e)
		{
			if (template != null) {
				Config.TeamTemplatesProvider.Update (template);			
				Edited = false;
			}
		}

		void HandleNewPlayerClicked (object sender, EventArgs e)
		{
			TreeIter iter;
			Player p;
			
			p = template.AddDefaultItem (template.Count);
			iter = AddPlayer (p);
			playersiconview.SelectPath (playersiconview.Model.GetPath (iter));
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
			Image player = new Image (Helpers.Misc.OpenImage (this));
			if (player == null) {
				return;
			}
			
			player.Scale (Constants.MAX_PLAYER_ICON_SIZE, Constants.MAX_PLAYER_ICON_SIZE); 
			if (player != null && loadedPlayer != null) {
				playerimage.Pixbuf = player.Value;
				loadedPlayer.Photo = player;
				playersiconview.Model.SetValue (loadedPlayerIter, (int) Columns.Photo,
				                                playerimage.Pixbuf);
				Edited = true;
			}
		}

		void HandleShieldButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			Image shield = new Image (Helpers.Misc.OpenImage (this));
			if (shield == null) {
				return;
			}
			
			shield.Scale (Constants.MAX_SHIELD_ICON_SIZE, Constants.MAX_SHIELD_ICON_SIZE); 
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

