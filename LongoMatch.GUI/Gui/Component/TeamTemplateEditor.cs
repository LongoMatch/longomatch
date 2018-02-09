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
using System.ComponentModel;
using System.Linq;
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Drawing.Cairo;
using VAS.UI.Component;
using Color = VAS.Core.Common.Color;
using Constants = LongoMatch.Core.Common.Constants;
using Helpers = VAS.UI.Helpers;
using Image = VAS.Core.Common.Image;
using Misc = VAS.UI.Helpers.Misc;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class TeamTemplateEditor : Gtk.Bin, IView<LMTeamEditorVM>
	{
		public event EventHandler TemplateSaved;

		const int SHIELD_SIZE = 70;
		const int PLAYER_SIZE = 70;

		LMTeamEditorVM viewModel;
		LMPlayer loadedPlayer;
		LMTeam template;
		bool edited, ignoreChanges;
		LMTeamTaggerView teamtagger;

		public TeamTemplateEditor ()
		{
			this.Build ();

			applyimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-apply", Sizes.ButtonDialogIconSize);

			teamtagger = new LMTeamTaggerView (new WidgetWrapper (drawingarea));
			shieldvbox.WidthRequest = SHIELD_SIZE;
			shieldimage.SetSize (SHIELD_SIZE);
			playerimage.SetSize (PLAYER_SIZE);
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

		public LMTeam Team {
			set {
				template = value;
				ignoreChanges = true;
				if (template.Shield != null) {
					shieldimage.Image = template.Shield;
				} else {
					shieldimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-default-shield", SHIELD_SIZE);
				}
				teamnameentry.Text = template.TeamName;
				FillFormation ();
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

		public LMTeamTaggerVM TeamTagger {
			set {
				teamtagger.ViewModel = value;
			}
		}

		public LMTeamEditorVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.Team.PropertyChanged -= HandleTeamPropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.Team.PropertyChanged += HandleTeamPropertyChanged;
					Team = ViewModel.Team.TypedModel;
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTeamEditorVM)viewModel;
		}

		public void AddPlayer ()
		{
			App.Current.EventsBroker.Publish (new CreateEvent<LMPlayer> ());
		}

		public void DeleteSelectedPlayers ()
		{
			App.Current.EventsBroker.Publish (new DeleteEvent<LMPlayer> ());
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

		void LoadPlayer (LMPlayer p)
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
			playerimage.Image = PlayerPhoto (p);

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
			playerimage.Image = App.Current.ResourcesLocator.LoadIcon ("lm-player-pic", PLAYER_SIZE);

			ignoreChanges = false;
		}

		void ParseTactics ()
		{
			try {
				template.FormationStr = tacticsentry.Text;
				Edited = true;
			} catch {
				App.Current.Dialogs.ErrorMessage (
					Catalog.GetString ("Could not parse tactics string"));
			}
			FillFormation ();
		}

		Image PlayerPhoto (LMPlayer p)
		{
			Image playerImage;

			if (p.Photo != null) {
				playerImage = p.Photo;
			} else {
				playerImage = App.Current.ResourcesLocator.LoadIcon ("lm-player-pic", PLAYER_SIZE);
			}
			return playerImage;
		}

		void HandleSaveTemplateClicked (object sender, EventArgs e)
		{
			if (template != null) {
				try {
					App.Current.TeamTemplatesProvider.Save (template);
					Edited = false;
				} catch (InvalidTemplateFilenameException ex) {
					App.Current.Dialogs.ErrorMessage (ex.ToString (), this);
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
			if (args.Event.Key == Gdk.Key.Delete &&
				ViewModel.DeletePlayersCommand.CanExecute ()) {
				ViewModel.DeletePlayersCommand.Execute ();
			}
		}

		void HandlePlayerButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			Image newPlayerImage = Helpers.Misc.OpenImage (this);

			if (newPlayerImage == null) {
				return;
			}

			newPlayerImage.ScaleInplace (Constants.MAX_PLAYER_ICON_SIZE, Constants.MAX_PLAYER_ICON_SIZE);
			if (newPlayerImage != null && loadedPlayer != null) {
				playerimage.Image = newPlayerImage;
				loadedPlayer.Photo = newPlayerImage;
				Edited = true;
			}
		}

		void HandleShieldButtonPressEvent (object o, ButtonPressEventArgs args)
		{
			Image shield = Helpers.Misc.OpenImage (this);

			if (shield == null) {
				return;
			}

			if (shield != null) {
				shield.ScaleInplace (Constants.MAX_SHIELD_ICON_SIZE, Constants.MAX_SHIELD_ICON_SIZE);
				template.Shield = shield;
				shieldimage.Image = shield;
				Edited = true;
			}
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

		void HandleTeamPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.Team.NeedsSync (e.PropertyName, nameof (ViewModel.Team.Model),
											  sender, ViewModel.Team)) {
				Team = ViewModel.Team.TypedModel;
			}
			if (ViewModel.Team.NeedsSync (e.PropertyName, $"Collection_{nameof (ViewModel.Team.Selection)}",
											  sender, ViewModel.Team)) {
				if (ViewModel.Team.Selection.Count == 1) {
					LoadPlayer ((LMPlayer)ViewModel.Team.Selection.First ().Model);
				} else {
					ClearPlayer ();
				}
				ViewModel.DeletePlayersCommand.EmitCanExecuteChanged ();
			}
		}
	}
}
