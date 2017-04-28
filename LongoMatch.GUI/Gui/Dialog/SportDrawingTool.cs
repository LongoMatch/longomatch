﻿//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Core;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Drawables;
using VAS.Drawing.Cairo;
using VAS.UI.Dialog;

namespace LongoMatch.Gui.Dialog
{
	[ViewAttribute (LMDrawingToolState.NAME)]
	public class SportDrawingTool : DrawingTool, IPanel<LMDrawingToolVM>
	{
		public new LMDrawingToolVM ViewModel {
			get;
			set;
		}

		public new void SetViewModel (object viewModel)
		{
			ViewModel = (LMDrawingToolVM)viewModel;
			base.SetViewModel (viewModel);
		}

		public override void EditPlayer (Text text)
		{
			playerText = text;
			if (playerDialog == null) {
				Gtk.Dialog d = new Gtk.Dialog (Catalog.GetString ("Select player"),
								   this, DialogFlags.Modal | DialogFlags.DestroyWithParent,
								   Stock.Cancel, ResponseType.Cancel);
				d.WidthRequest = 600;
				d.HeightRequest = 400;

				DrawingArea da = new DrawingArea ();
				LMTeamTaggerView tagger = new LMTeamTaggerView (new WidgetWrapper (da));
				tagger.ViewModel = ViewModel.TeamTagger;
				tagger.ViewModel.PropertyChanged += (sender, e) => {
					if (e.PropertyName == "Tagged") {
						Player p = (sender as LMPlayerVM).Model;
						playerText.Value = p.ToString ();
						d.Respond (ResponseType.Ok);
					}
				};
				d.VBox.PackStart (da, true, true, 0);
				d.ShowAll ();
				playerDialog = d;
			}
			ViewModel.TeamTagger.SubstitutionMode = false;
			if (playerDialog.Run () != (int)ResponseType.Ok) {
				text.Value = null;
			}
			playerDialog.Hide ();
			ViewModel.TeamTagger.SubstitutionMode = true;
		}
	}
}
