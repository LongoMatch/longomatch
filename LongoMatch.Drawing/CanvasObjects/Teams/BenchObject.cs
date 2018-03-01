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
using System.Collections.Specialized;
using LongoMatch.Core.Common;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Drawing;
using VASDrawing = VAS.Drawing;

namespace LongoMatch.Drawing.CanvasObjects.Teams
{
	public class BenchObject : FixedSizeCanvasObject, ICanvasSelectableObject, ICanvasObjectView<LMTeamVM>
	{
		public event EventHandler SizeChanged;

		LMTeamVM teamVM;
		List<LMPlayerView> benchPlayers;

		public BenchObject ()
		{
			benchPlayers = new List<LMPlayerView> ();
		}

		protected override void DisposeManagedResources ()
		{
			ClearPlayers ();
		}

		public int PlayersPerRow { get; set; }

		public TeamType TeamType { get; set; }

		public bool SubstitutionMode { get; set; }

		public int PlayersSize { get; set; }

		public LMTeamVM ViewModel {
			get => teamVM;
			set {
				if (teamVM != null) {
					teamVM.BenchPlayersList.CollectionChanged -= HandleCollectionChanged;
				}
				teamVM = value;
				if (teamVM != null) {
					LoadBench ();
					teamVM.BenchPlayersList.CollectionChanged += HandleCollectionChanged;
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTeamVM)viewModel;
		}

		public void Update ()
		{
			if (benchPlayers == null || PlayersPerRow == 0 || ViewModel == null) {
				return;
			}

			for (int i = 0; i < benchPlayers.Count; i++) {
				LMPlayerView po;
				double x, y;
				double s = Width / PlayersPerRow;

				x = s * (i % PlayersPerRow) + s / 2;
				y = s * (i / PlayersPerRow) + s / 2;

				po = benchPlayers [i];
				po.Size = PlayersSize;
				po.Center = new Point (x, y);
			}

			if (PlayersPerRow == 1) {
				Height = benchPlayers [benchPlayers.Count - 1].Center.Y + PlayersSize;
				SizeChanged?.Invoke (this, new EventArgs ());
			}

			ReDraw ();
		}

		public override void Draw (IDrawingToolkit tk, Area area)
		{
			if (benchPlayers == null || Position == null) {
				return;
			}
			tk.Begin ();
			tk.TranslateAndScale (Position, new Point (1, 1));
			tk.FillColor = App.Current.Style.ThemeContrastBase;
			tk.LineWidth = 0;
			tk.DrawRectangle (new Point (0, 0), Width, Height);

			foreach (LMPlayerView po in benchPlayers) {
				po.SubstitutionMode = SubstitutionMode;
				po.Size = PlayersSize;
				po.Circular = false;
				po.Draw (tk, area);
			}

			tk.End ();
		}

		public Selection GetSelection (Point point, double precision, bool inMotion = false)
		{
			Selection selection = null;

			if (benchPlayers == null || Position == null) {
				return selection;
			}

			point = VASDrawing.Utils.ToUserCoords (point, Position, 1, 1);

			foreach (LMPlayerView po in benchPlayers) {
				selection = po.GetSelection (point, precision);
				if (selection != null)
					break;
			}
			return selection;
		}

		public void Move (Selection s, Point p, Point start)
		{
		}

		void LoadBench () {
			ViewModel.TypedModel.UpdateColors ();
			ClearPlayers ();
			AddPlayers ();
			Update ();
		}

		void HandleCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.Action == NotifyCollectionChangedAction.Reset) {
				LoadBench ();
			}
		}

		void HandlePlayerClickedEvent (ICanvasObject co)
		{
			LMPlayerView player = co as LMPlayerView;
			ViewModel.PlayerClick (player.ViewModel, ButtonModifier.None); //)modifier); Store the modifier in the canvas object
			ReDraw (); 
		}

		void AddPlayers () {
			foreach (LMPlayerVM player in teamVM.BenchPlayersList) {
				var playerView = new LMPlayerView { Team = TeamType, ViewModel = player };
				benchPlayers.Add (playerView);
				playerView.ClickedEvent += HandlePlayerClickedEvent;
			}
		}

		void ClearPlayers () {
			foreach (var player in benchPlayers) {
				player.ClickedEvent -= HandlePlayerClickedEvent;
				player.Dispose ();
			}

			benchPlayers.Clear ();
		}
	}
}

