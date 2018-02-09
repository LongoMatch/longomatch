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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Resources;
using VAS.Core.Resources.Styles;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using VAS.Drawing.CanvasObjects;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.CanvasObjects.Teams
{
	public class LMPlayersTaggerView : CanvasObject, ICanvasSelectableObject, ICanvasObjectView<LMTeamTaggerVM>
	{

		/* This object can be used like single object filling a canvas or embedded
		 * in a canvas with more objects, like in the analysis template.
		 * For this reason we can't use the canvas selection logic and we have
		 * to handle it internally
		 */

		LMTeamTaggerVM viewModel;

		const int BUTTONS_HEIGHT = 40;
		const int BUTTONS_WIDTH = 60;
		const int MIN_PLAYERS_ROWS = 5;
		ButtonObject subPlayers, subInjury, homeButton, awayButton;
		LMTeam homeTeam, awayTeam;
		Dictionary<LMPlayerVM, LMPlayerView> homePlayerToPlayerObject;
		Dictionary<LMPlayerVM, LMPlayerView> awayPlayerToPlayerObject;
		List<LMPlayerView> homePlayingPlayers, awayPlayingPlayers;
		List<LMPlayerView> homeBenchPlayers, awayBenchPlayers;
		List<LMPlayerView> homePlayers, awayPlayers;
		BenchObject homeBench, awayBench;
		LMPlayerView clickedPlayer;
		ButtonObject clickedButton;
		FieldObject field;
		int NTeams;
		Point offset;
		double scaleX, scaleY;
		ButtonModifier modifier;

		public LMPlayersTaggerView ()
		{
			Position = new Point (0, 0);
			homeBench = new BenchObject ();
			awayBench = new BenchObject ();
			offset = new Point (0, 0);
			scaleX = scaleY = 1;
			homePlayerToPlayerObject = new Dictionary<LMPlayerVM, LMPlayerView> ();
			awayPlayerToPlayerObject = new Dictionary<LMPlayerVM, LMPlayerView> ();
			field = new FieldObject ();
			SelectedPlayers = new List<LMPlayer> ();
			LoadSubsButtons ();
			LoadTeamsButtons ();
		}

		protected override void DisposeManagedResources ()
		{
			ClearPlayers ();
			homeBench.Dispose ();
			awayBench.Dispose ();
			field.Dispose ();
			subPlayers.Dispose ();
			subInjury.Dispose ();
			homeButton.Dispose ();
			awayButton.Dispose ();
			base.DisposeManagedResources ();
		}

		public Point Position {
			get;
			set;
		}

		public double Width {
			get;
			set;
		}

		public double Height {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		public Color BackgroundColor {
			get;
			set;
		}

		public List<LMPlayer> SelectedPlayers {
			get;
			set;
		}

		public ObservableCollection<LMTeam> SelectedTeams {
			get {
				ObservableCollection<LMTeam> teams = new ObservableCollection<LMTeam> ();
				if (homeButton.Active) {
					teams.Add (homeTeam);
				}
				if (awayButton.Active) {
					teams.Add (awayTeam);
				}
				return teams;
			}
		}

		public LMTeamTaggerVM ViewModel {
			get {
				return viewModel;
			}
			set {
				if (viewModel != null) {
					viewModel.PropertyChanged -= HandleViewModelPropertyChanged;
				}
				viewModel = value;
				if (viewModel != null) {
					viewModel.PropertyChanged += HandleViewModelPropertyChanged;
					viewModel.Sync ();
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTeamTaggerVM)viewModel;
		}

		public void LoadTeams ()
		{
			int [] homeF = null, awayF = null;
			int playerSize, colSize, border;

			NTeams = 0;

			SetFieldBackground ();
			ClearPlayers ();
			homePlayingPlayers = awayPlayingPlayers = null;

			homePlayers = new List<LMPlayerView> ();
			awayPlayers = new List<LMPlayerView> ();

			if (ViewModel.HomeTeam != null) {
				homeTeam = ViewModel.HomeTeam.TypedModel;
				homeTeam.UpdateColors ();
				homePlayingPlayers = GetPlayersViews (ViewModel.HomeTeam.FieldPlayersList, TeamType.LOCAL);
				homeBenchPlayers = GetPlayersViews (ViewModel.HomeTeam.BenchPlayersList, TeamType.LOCAL);
				homePlayers.AddRange (homePlayingPlayers);
				homePlayers.AddRange (homeBenchPlayers);
				homeF = homeTeam.Formation;
				if (ViewModel.HomeTeam.Icon == null) {
					homeButton.BackgroundImage = App.Current.ResourcesLocator.LoadIcon (Icons.DefaultShield);
				} else {
					homeButton.BackgroundImage = ViewModel.HomeTeam.Icon;
				}
				NTeams++;
			}
			if (ViewModel.AwayTeam != null) {
				awayTeam = ViewModel.AwayTeam.TypedModel;
				awayTeam.UpdateColors ();
				awayPlayingPlayers = GetPlayersViews (ViewModel.AwayTeam.FieldPlayersList, TeamType.VISITOR);
				awayBenchPlayers = GetPlayersViews (ViewModel.AwayTeam.BenchPlayersList, TeamType.VISITOR);
				awayPlayers.AddRange (awayPlayingPlayers);
				awayPlayers.AddRange (awayBenchPlayers);
				awayF = awayTeam.Formation;
				if (ViewModel.AwayTeam.Icon == null) {
					awayButton.BackgroundImage = App.Current.ResourcesLocator.LoadIcon (Icons.DefaultShield);
				} else {
					awayButton.BackgroundImage = ViewModel.AwayTeam.Icon;
				}
				NTeams++;
			}

			colSize = ColumnSize;
			playerSize = colSize * 90 / 100;

			BenchWidth (colSize, field.Height, playerSize);
			field.LoadTeams (ViewModel.Background, homeF, awayF, homePlayingPlayers,
				awayPlayingPlayers, playerSize, NTeams);
			homeBench.BenchPlayers = homeBenchPlayers;
			awayBench.BenchPlayers = awayBenchPlayers;
			homeBench.Height = awayBench.Height = field.Height;

			border = Sizes.TeamTaggerBenchBorder;
			if (homeTeam == null || awayTeam == null) {
				if (homeTeam != null) {
					homeBench.Position = new Point (border, 0);
					field.Position = new Point (border + homeBench.Width + border, 0);
				} else {
					field.Position = new Point (border, 0);
					awayBench.Position = new Point (border + field.Width + border, 0);
				}
			} else {
				homeBench.Position = new Point (border, 0);
				field.Position = new Point (homeBench.Width + 2 * border, 0);
				awayBench.Position = new Point (awayBench.Width + field.Width + 3 * border, 0);
			}
			Update ();
		}

		public override void ResetDrawArea ()
		{
			base.ResetDrawArea ();
			if (homePlayers != null) {
				foreach (CanvasObject co in homePlayers) {
					co.ResetDrawArea ();
				}
			}
			if (awayPlayers != null) {
				foreach (CanvasObject co in awayPlayers) {
					co.ResetDrawArea ();
				}
			}
			subPlayers.ResetDrawArea ();
			subInjury.ResetDrawArea ();
			homeButton.ResetDrawArea ();
			awayButton.ResetDrawArea ();
		}

		void BenchWidth (int colSize, int height, int playerSize)
		{
			int maxPlayers, playersPerColumn, playersPerRow;
			double ncolSize;

			ncolSize = colSize;

			maxPlayers = Math.Max (
				homeBenchPlayers != null ? homeBenchPlayers.Count : 0,
				awayBenchPlayers != null ? awayBenchPlayers.Count : 0);
			playersPerColumn = height / colSize;
			if (ViewModel.Compact) {
				/* Try with 4/4, 3/4 and 2/4 of the original column size
				 * to fit all players in a single column */
				for (int i = 4; i > 1; i--) {
					ncolSize = (double)colSize * i / 4;
					playersPerColumn = (int)(height / ncolSize);
					playersPerRow = (int)Math.Ceiling ((double)maxPlayers / playersPerColumn);
					if (playersPerRow == 1) {
						break;
					}
				}
			}

			homeBench.PlayersSize = awayBench.PlayersSize = (int)(ncolSize * 90 / 100);
			homeBench.PlayersPerRow = awayBench.PlayersPerRow =
				(int)Math.Ceiling ((double)maxPlayers / playersPerColumn);
			homeBench.Width = awayBench.Width = (int)ncolSize * homeBench.PlayersPerRow;
		}

		void ClearPlayers ()
		{
			if (homePlayers != null) {
				foreach (LMPlayerView po in homePlayers) {
					po.Dispose ();
				}
				homePlayers = null;
			}
			if (awayPlayers != null) {
				foreach (LMPlayerView po in awayPlayers) {
					po.Dispose ();
				}
				awayPlayers = null;
			}
			homePlayerToPlayerObject.Clear ();
			awayPlayerToPlayerObject.Clear ();
		}

		void LoadSubsButtons ()
		{
			subPlayers = new ButtonObject ();
			subPlayers.BackgroundImageActive = App.Current.ResourcesLocator.LoadIcon (Icons.SubsUnlock);
			subPlayers.BackgroundColorActive = App.Current.Style.ScreenBase;
			subPlayers.BackgroundImage = App.Current.ResourcesLocator.LoadIcon (Icons.SubsLock);
			subPlayers.Toggle = true;
			subPlayers.ClickedEvent += HandleSubsClicked;
			subPlayers.RedrawEvent += (co, area) => EmitRedrawEvent (subPlayers, area);
			subInjury = new ButtonObject ();
			subInjury.BackgroundColorActive = App.Current.Style.ScreenBase;
			subInjury.Toggle = true;
			subInjury.ClickedEvent += HandleSubsClicked;
			subInjury.Visible = false;
		}

		void LoadTeamsButtons ()
		{
			homeButton = new ButtonObject ();
			homeButton.Toggle = true;
			homeButton.ClickedEvent += HandleTeamClickedEvent;
			homeButton.Width = BUTTONS_WIDTH;
			homeButton.Height = BUTTONS_HEIGHT;
			homeButton.RedrawEvent += (co, area) => {
				EmitRedrawEvent (homeButton, area);
			};
			awayButton = new ButtonObject ();
			awayButton.Toggle = true;
			awayButton.Width = BUTTONS_WIDTH;
			awayButton.Height = BUTTONS_HEIGHT;
			awayButton.ClickedEvent += HandleTeamClickedEvent;
			awayButton.RedrawEvent += (co, area) => {
				EmitRedrawEvent (awayButton, area);
			};
		}

		int ColumnSize {
			get {
				int width, optWidth, optHeight, count = 0, max = 0;

				width = field.Width / NTeams;
				if (homeTeam != null && awayTeam != null) {
					count = Math.Max (homeTeam.Formation.Count (),
						awayTeam.Formation.Count ());
					max = Math.Max (homeTeam.Formation.Max (),
						awayTeam.Formation.Max ());
				} else if (homeTeam != null) {
					count = homeTeam.Formation.Count ();
					max = homeTeam.Formation.Max ();
				} else if (awayTeam != null) {
					count = awayTeam.Formation.Count ();
					max = awayTeam.Formation.Max ();
				}
				optWidth = width / count;
				optHeight = field.Height / Math.Max (MIN_PLAYERS_ROWS, max);
				return Math.Min (optWidth, optHeight);
			}
		}

		List<LMPlayerView> GetPlayersViews (IEnumerable<LMPlayerVM> players, TeamType team)
		{
			List<LMPlayerView> playerObjects;
			Color color = null;

			if (team == TeamType.LOCAL) {
				color = App.Current.Style.HomeTeamColor;
			} else {
				color = App.Current.Style.AwayTeamColor;
			}

			playerObjects = new List<LMPlayerView> ();
			foreach (var player in players) {
				LMPlayerView po = new LMPlayerView { Team = team };
				po.ViewModel = player;
				po.ClickedEvent += HandlePlayerClickedEvent;
				po.RedrawEvent += (co, area) => {
					EmitRedrawEvent (po, area);
				};
				playerObjects.Add (po);
				if ((team == TeamType.LOCAL) && !homePlayerToPlayerObject.ContainsKey (player)) {
					homePlayerToPlayerObject.Add (player, po);
				} else if ((team == TeamType.VISITOR) && !awayPlayerToPlayerObject.ContainsKey (player)) {
					awayPlayerToPlayerObject.Add (player, po);
				}
			}
			return playerObjects;
		}

		void HandlePlayerClickedEvent (ICanvasObject co)
		{
			LMPlayerView player = co as LMPlayerView;
			ViewModel.PlayerClick (player.ViewModel, modifier);
		}

		bool ButtonClickPressed (Point point, ButtonModifier modif, params ButtonObject [] buttons)
		{
			Selection sel;

			if (!ViewModel.ShowSubstitutionButtons && !ViewModel.ShowTeamsButtons) {
				return false;
			}

			foreach (ButtonObject button in buttons) {
				if (!button.Visible)
					continue;
				sel = button.GetSelection (point, 0);
				if (sel != null) {
					clickedButton = sel.Drawable as ButtonObject;
					(sel.Drawable as ICanvasObject).ClickPressed (point, modif, sel);
					return true;
				}
			}
			return false;
		}

		void HandleSubsClicked (ICanvasObject co)
		{
			ViewModel.SubstitutionMode = !ViewModel.SubstitutionMode;
		}

		void HandleTeamClickedEvent (ICanvasObject co)
		{
			var button = co as ButtonObject;
			if (button == homeButton) {
				ViewModel.HomeTeam.Tagged = button.Active;
			} else if (button == awayButton) {
				ViewModel.AwayTeam.Tagged = button.Active;
			}
		}

		public override void ClickPressed (Point point, ButtonModifier modif, Selection sel)
		{
			Selection selection = null;
			modifier = ButtonModifier.None;
			if (ButtonClickPressed (point, modif, subPlayers, subInjury,
					homeButton, awayButton)) {
				return;
			}

			// FIXME: this is very awkward, click events should be forwarded to the child views
			point = VASDrawing.Utils.ToUserCoords (point, offset, scaleX, scaleY);
			selection = homeBench.GetSelection (point, 0, false);
			if (selection == null) {
				selection = awayBench.GetSelection (point, 0, false);
				if (selection == null) {
					selection = field.GetSelection (point, 0, false);
					if (selection != null) {
						point = VASDrawing.Utils.ToUserCoords (point, field.Position, 1, 1);
					}
				} else {
					point = VASDrawing.Utils.ToUserCoords (point, awayBench.Position, 1, 1);
				}
			} else {
				point = VASDrawing.Utils.ToUserCoords (point, homeBench.Position, 1, 1);
			}
			if (selection != null) {
				clickedPlayer = selection.Drawable as LMPlayerView;
				modifier = modif;
				(selection.Drawable as ICanvasObject).ClickPressed (point, modif, selection);
			} else {
				clickedPlayer = null;
			}
		}

		public override void ClickReleased ()
		{
			if (clickedButton != null) {
				clickedButton.ClickReleased ();
				clickedButton = null;
			} else if (clickedPlayer != null) {
				clickedPlayer.ClickReleased ();
				modifier = ButtonModifier.None;
			}
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			double width, height;

			/* Compute how we should scale and translate to fit the widget
			 * in the designated area */
			width = homeBench.Width * NTeams + field.Width +
			2 * NTeams * Sizes.TeamTaggerBenchBorder;
			height = field.Height;
			Image.ScaleFactor ((int)width, (int)height, (int)Width,
				(int)Height - BUTTONS_HEIGHT, ScaleMode.AspectFit,
				out scaleX, out scaleY, out offset);
			offset.Y += BUTTONS_HEIGHT;
			tk.Begin ();
			tk.Clear (BackgroundColor);

			/* Draw substitution buttons */
			if (subPlayers.Visible) {
				subPlayers.Position = new Point (Width / 2 - BUTTONS_WIDTH / 2,
					offset.Y - BUTTONS_HEIGHT);
				subPlayers.Width = BUTTONS_WIDTH;
				subPlayers.Height = BUTTONS_HEIGHT;
				subPlayers.Draw (tk, area);
			}
			if (homeButton.Visible) {
				/* Draw local team button */
				double x = Position.X + Sizes.TeamTaggerBenchBorder * scaleX + offset.X;
				homeButton.Position = new Point (x, offset.Y - homeButton.Height);
				homeButton.Draw (tk, area);
			}
			if (awayButton.Visible) {
				double x = (Position.X + Width - offset.X - Sizes.TeamTaggerBenchBorder * scaleX) - awayButton.Width;
				awayButton.Position = new Point (x, offset.Y - awayButton.Height);
				awayButton.Draw (tk, area);
			}

			tk.TranslateAndScale (Position + offset, new Point (scaleX, scaleY));
			homeBench.Draw (tk, area);
			awayBench.Draw (tk, area);
			field.Draw (tk, area);
			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection sel = null;

			if (!inMotion) {
				if (point.X < Position.X || point.X > Position.X + Width ||
					point.Y < Position.Y || point.Y > Position.Y + Height) {
					sel = null;
				} else {
					sel = new Selection (this, SelectionPosition.All, 0);
				}
			} else {
				point = VASDrawing.Utils.ToUserCoords (point, offset, scaleX, scaleY);
				sel = homeBench.GetSelection (point, 0, false);
				if (sel == null) {
					sel = awayBench.GetSelection (point, 0, false);
					if (sel == null) {
						sel = field.GetSelection (point, 0, false);
					}
				}
			}
			return sel;
		}

		public void Move (Selection s, Point p, Point start)
		{
			throw new NotImplementedException ("Unsupported move for PlayersTaggerObject:  " + s.Position);
		}

		void SetFieldBackground ()
		{
			if (ViewModel.Background != null) {
				field.Height = ViewModel.Background.Height;
				field.Width = ViewModel.Background.Width;
			} else {
				field.Width = 300;
				field.Height = 250;
			}
		}

		void Update ()
		{
			homeBench.Update ();
			awayBench.Update ();
			field.Update ();
		}

		void HandleViewModelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if (ViewModel.HomeTeam != null) {
				if ((ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.HomeTeam), sender, ViewModel) ||
					 ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.HomeTeam.FieldPlayersList), sender, ViewModel.HomeTeam) ||
					 ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.HomeTeam.BenchPlayersList), sender, ViewModel.HomeTeam))) {
					LoadTeams ();
					ReDraw ();
				}

				if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.HomeTeam.Tagged), sender, ViewModel.HomeTeam) ||
					ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.HomeTeam.Tagged), sender, ViewModel)) {
					homeButton.Active = ViewModel.HomeTeam.Tagged;
				}
			}

			if (ViewModel.AwayTeam != null) {
				if ((ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.AwayTeam), sender, ViewModel) ||
					 ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.AwayTeam.FieldPlayersList), sender, ViewModel.AwayTeam) ||
				 ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.AwayTeam.BenchPlayersList), sender, ViewModel.AwayTeam))) {
					LoadTeams ();
					ReDraw ();
				}
				if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.AwayTeam.Tagged), sender, ViewModel.AwayTeam) ||
					ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.AwayTeam.Tagged), sender, ViewModel)) {
					awayButton.Active = ViewModel.AwayTeam.Tagged;
				}
			}

			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.Background), sender, ViewModel)) {
				LoadTeams ();
				ReDraw ();
			}

			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.ShowSubstitutionButtons), sender, ViewModel)) {
				subPlayers.Visible = ViewModel.ShowSubstitutionButtons;
				/* FIXME: Not displayed for now */
				subInjury.Visible = false;
			}

			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.SubstitutionMode), sender, ViewModel)) {
				homeBench.SubstitutionMode =
					awayBench.SubstitutionMode =
						field.SubstitutionMode = ViewModel.SubstitutionMode;
				subPlayers.Active = ViewModel.SubstitutionMode;
			}

			if (ViewModel.NeedsSync (e.PropertyName, nameof (ViewModel.ShowTeamsButtons), sender, ViewModel)) {
				homeButton.Visible = awayButton.Visible = ViewModel.ShowTeamsButtons;
			}
		}
	}
}

