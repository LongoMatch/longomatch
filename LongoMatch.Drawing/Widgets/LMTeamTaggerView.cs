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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Drawing.CanvasObjects.Teams;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Store.Drawables;
using VAS.Drawing;

namespace LongoMatch.Drawing.Widgets
{
	public class LMTeamTaggerView : SelectionCanvas, ICanvasView<LMTeamTaggerVM>
	{
		public event PlayersPropertiesHandler ShowMenuEvent;

		LMTeamTaggerVM viewModel;
		LMPlayersTaggerView tagger;

		public LMTeamTaggerView (IWidget widget) : base (widget)
		{
			Accuracy = 0;
			tagger = new LMPlayersTaggerView ();
			BackgroundColor = App.Current.Style.ThemeBase;
			ObjectsCanMove = false;
			AddObject (tagger);
		}

		public LMTeamTaggerView () : this (null)
		{
		}

		protected override void DisposeManagedResources ()
		{
			tagger.Dispose ();
			base.DisposeManagedResources ();
		}

		/* public ObservableCollection<LMTeam> SelectedTeams {
			get {
				return tagger.SelectedTeams;
			}
		}*/

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		public new Color BackgroundColor {
			set {
				tagger.BackgroundColor = value;
			}
		}

		public LMTeamTaggerVM ViewModel {
			get {
				return viewModel;
			}

			set {
				viewModel = value;
				tagger.ViewModel = value;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMTeamTaggerVM)viewModel;
		}

		protected override void ShowMenu (Point coords)
		{
			List<LMPlayer> players = new List<LMPlayer> (); // tagger.SelectedPlayers;

			// if (players.Count == 0) {
				Selection sel = tagger.GetSelection (coords, 0, true);
				if (sel != null) {
					players = new List<LMPlayer> { (sel.Drawable as LMPlayerView).ViewModel.TypedModel };
				}
			/* } else {
				players = tagger.SelectedPlayers;
			}*/

			if (ShowMenuEvent != null) {
				ShowMenuEvent (players);
			}
		}

		protected override void HandleSizeChangedEvent ()
		{
			if (tagger != null) {
				tagger.Width = widget.Width;
				tagger.Height = widget.Height;
			}
			base.HandleSizeChangedEvent ();
		}
	}
}

