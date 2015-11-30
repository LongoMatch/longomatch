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
using Gdk;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.Cairo;
using LongoMatch.Drawing.Widgets;
using Mono.Unix;
using Color = LongoMatch.Core.Common.Color;
using Image = LongoMatch.Core.Common.Image;
using Misc = LongoMatch.Gui.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TeamTemplateEditor : Gtk.Bin
	{
		public event EventHandler TemplateSaved;

		Player loadedPlayer;
		Team template;
		bool edited, ignoreChanges;
		List<Player> selectedPlayers;
		TeamTagger teamtagger;
		const int SHIELD_SIZE = 70;

		public TeamTemplateEditor ()
		{
			this.Build ();

			teamtagger = new TeamTagger (new WidgetWrapper (drawingarea));
			teamtagger.SelectionMode = MultiSelectionMode.MultipleWithModifier;
			teamtagger.PlayersSelectionChangedEvent += HandlePlayersSelectionChangedEvent;
			teamtagger.PlayersSubstitutionEvent += HandlePlayersSubstitutionEvent;
			shieldimage.HeightRequest = shieldvbox.WidthRequest = SHIELD_SIZE;
			colorbutton1.Color = Misc.ToGdkColor (Color.Red1);
			colorbutton1.ColorSet += HandleColorSet;
			colorbutton2.Color = Misc.ToGdkColor (Color.Green1);
			colorbutton2.ColorSet += HandleColorSet;

			ConnectSignals ();

			ClearPlayer ();
		}

		protected override void OnDestroyed ()
		{
			teamtagger.Dispose ();
			base.OnDestroyed ();
		}

		public bool Edited {
			get {
				return edited;
			}
			set {
				edited = value;
				savebutton.Sensitive = edited;
			}
		}

		public Team  Team {
			set {
				template = value;
				ignoreChanges = true;
				if (template.Shield != null) {
					shieldimage.Pixbuf = template.Shield.Scale (SHIELD_SIZE, SHIELD_SIZE).Value;
				} else {
					shieldimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-default-shield", SHIELD_SIZE);
				}
				teamnameentry.Text = template.TeamName;
				FillFormation ();
				teamtagger.LoadTeams (template, null, Config.HHalfFieldBackground);
				// Start with disabled widget until something get selected
				ClearPlayer ();
				colorbutton1.Color = Misc.ToGdkColor (value.Colors [0]);
				colorbutton2.Color = Misc.ToGdkColor (value.Colors [1]);
				ignoreChanges = false;
				Edited = false;
			}
		}

		public bool VisibleButtons {
			set {
				hbuttonbox2.Visible = value;
			}
		}

		public void AddPlayer ()
		{
			Player p = template.AddDefaultItem (template.List.Count);
			teamtagger.Reload ();
			teamtagger.Select (p);
			Edited = true;
		}

		public void DeleteSelectedPlayers ()
		{
			bool edited = false;

			if (selectedPlayers == null || selectedPlayers.Count == 0) {
				return;
			}

			foreach (Player p in selectedPlayers) {
				string msg = Catalog.GetString ("Do you want to delete player: ") + p.Name;
				if (Config.GUIToolkit.QuestionMessage (msg, null, this).Result) {
					template.List.Remove (p);
					edited = true;
				}
			}
			if (edited) {
				teamtagger.Reload ();
				Edited = true;
			}
		}

		void ConnectSignals ()
		{
			newplayerbutton.Clicked += HandleNewPlayerClicked;
			savebutton.Clicked += HandleSaveTemplateClicked;
			deletebutton.Clicked += HandleDeletePlayerClicked;
			
			shieldeventbox.ButtonPressEvent += HandleShieldButtonPressEvent;
			playereventbox.ButtonPressEvent += HandlePlayerButtonPressEvent;


			teamnameentry.Changed += HandleEntryChanged;
			nameentry.Changed += HandleEntryChanged;
			lastnameentry.Changed += HandleEntryChanged;
			nicknameentry.Changed += HandleEntryChanged;
			positionentry.Changed += HandleEntryChanged;
			numberspinbutton.ValueChanged += HandleEntryChanged;
			heightspinbutton.ValueChanged += HandleEntryChanged;
			weightspinbutton.ValueChanged += HandleEntryChanged;
			nationalityentry.Changed += HandleEntryChanged;
			mailentry.Changed += HandleEntryChanged;
			bdaydatepicker.ValueChanged += HandleEntryChanged;
			
			applybutton.Clicked += (s, e) => {
				ParseTactics ();
			}; 

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
			} else if (sender == lastnameentry) {
				loadedPlayer.LastName = (sender as Entry).Text;
			} else if (sender == nicknameentry) {
				loadedPlayer.NickName = (sender as Entry).Text;
			} else if (sender == positionentry) {
				loadedPlayer.Position = (sender as Entry).Text;
			} else if (sender == numberspinbutton) {
				loadedPlayer.Number = (sender as SpinButton).ValueAsInt;
			} else if (sender == heightspinbutton) {
				loadedPlayer.Height = (float)(sender as SpinButton).Value;
			} else if (sender == weightspinbutton) {
				loadedPlayer.Weight = (sender as SpinButton).ValueAsInt;
			} else if (sender == nationalityentry) {
				loadedPlayer.Nationality = (sender as Entry).Text;
			} else if (sender == mailentry) {
				loadedPlayer.Mail = (sender as Entry).Text;
			} else if (sender == bdaydatepicker) {
				loadedPlayer.Birthday = (sender as DatePicker).Date;
			}

			Edited = true;
			drawingarea.QueueDraw ();
		}

		void FillFormation ()
		{
			tacticsentry.Text = template.FormationStr;
		}

		void LoadPlayer (Player p)
		{
			ignoreChanges = true;

			loadedPlayer = p;

			nameentry.Text = p.Name ?? "";
			lastnameentry.Text = p.LastName ?? "";
			nicknameentry.Text = p.NickName ?? "";
			positionentry.Text = p.Position ?? "";
			numberspinbutton.Value = p.Number;
			heightspinbutton.Value = p.Height;
			weightspinbutton.Value = p.Weight;
			nationalityentry.Text = p.Nationality ?? "";
			bdaydatepicker.Date = p.Birthday;
			mailentry.Text = p.Mail ?? "";
			playerimage.Pixbuf = PlayerPhoto (p);

			playerframe.Sensitive = true;

			ignoreChanges = false;
		}

		void ClearPlayer ()
		{
			ignoreChanges = true;

			playerframe.Sensitive = false;

			loadedPlayer = null;

			nameentry.Text = "";
			lastnameentry.Text = "";
			nicknameentry.Text = "";
			positionentry.Text = "";
			numberspinbutton.Value = 0;
			heightspinbutton.Value = 0;
			weightspinbutton.Value = 0;
			nationalityentry.Text = "";
			bdaydatepicker.Date = new DateTime ();
			mailentry.Text = "";
			playerimage.Pixbuf = playerimage.Pixbuf = Helpers.Misc.LoadIcon ("longomatch-player-pic", 45, IconLookupFlags.ForceSvg);

			ignoreChanges = false;
		}

		void ParseTactics ()
		{
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

		Pixbuf PlayerPhoto (Player p)
		{
			Pixbuf playerImage;
				
			if (p.Photo != null) {
				playerImage = p.Photo.Value;
			} else {
				playerImage = Misc.LoadIcon ("longomatch-player-pic", 45, IconLookupFlags.ForceSvg);
			}
			return playerImage;
		}

		void PlayersSelected (List<Player> players)
		{
			ignoreChanges = true;

			selectedPlayers = players;
			deletebutton.Sensitive = players.Count != 0;
			if (players.Count == 1) {
				LoadPlayer (players [0]);
			} else {
				ClearPlayer ();
			}

			ignoreChanges = false;
		}

		void HandlePlayersSelectionChangedEvent (List<Player> players)
		{
			PlayersSelected (players);
		}

		void HandleSaveTemplateClicked (object sender, EventArgs e)
		{
			if (template != null) {
				try {
					Config.TeamTemplatesProvider.Save (template);
					Edited = false;
				} catch (InvalidTemplateFilenameException ex) {
					Config.GUIToolkit.ErrorMessage (ex.ToString (), this);
					return;
				}
			}
			if (TemplateSaved != null)
				TemplateSaved (this, null);
		}

		void HandleNewPlayerClicked (object sender, EventArgs e)
		{
			AddPlayer ();
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
			if (shield != null) {
				shield.ScaleInplace (Constants.MAX_SHIELD_ICON_SIZE,
					Constants.MAX_SHIELD_ICON_SIZE); 
				template.Shield = shield;
				shieldimage.Pixbuf = shield.Scale (SHIELD_SIZE, SHIELD_SIZE).Value;
				Edited = true;
			}
		}

		void HandlePlayersSubstitutionEvent (Team team, Player p1, Player p2,
		                                     SubstitutionReason reason, Time time)
		{
			team.List.Swap (p1, p2);
			teamtagger.Substitute (p1, p2, team);
			Edited = true;
		}

		void HandleColorSet (object sender, EventArgs e)
		{
			if (ignoreChanges)
				return;
			if (sender == colorbutton1) {
				template.Colors [0] = Misc.ToLgmColor (colorbutton1.Color);
				template.UpdateColors ();
				drawingarea.QueueDraw ();
			} else {
				template.Colors [1] = Misc.ToLgmColor (colorbutton2.Color);
			}
			Edited = true;
		}

	}
}

