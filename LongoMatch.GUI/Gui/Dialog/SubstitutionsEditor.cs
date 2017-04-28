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
using LongoMatch.Drawing.CanvasObjects.Teams;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Drawing;
using VAS.Drawing.Cairo;

namespace LongoMatch.Gui.Dialog
{
	[ViewAttribute (SubstitutionsEditorState.NAME)]
	public partial class SubstitutionsEditor : Gtk.Dialog, IPanel<SubstitutionsEditorVM>
	{
		LMTeamTaggerView tagger;
		SelectionCanvas incanvas, outcanvas;
		LMPlayerView inpo, outpo;
		const int PLAYER_SIZE = 100;
		SubstitutionsEditorVM editorVM;

		public SubstitutionsEditor ()
		{
			this.Build ();
			tagger = new LMTeamTaggerView (new WidgetWrapper (drawingarea));
			incanvas = new SelectionCanvas (new WidgetWrapper (drawingarea2));
			outcanvas = new SelectionCanvas (new WidgetWrapper (drawingarea3));
			inpo = new LMPlayerView ();
			outpo = new LMPlayerView ();
			inpo.ClickedEvent += HandleClickedEvent;
			outpo.ClickedEvent += HandleClickedEvent;
			inpo.Size = PLAYER_SIZE;
			outpo.Size = PLAYER_SIZE;
			inpo.Center = new Point (PLAYER_SIZE / 2, PLAYER_SIZE / 2);
			outpo.Center = new Point (PLAYER_SIZE / 2, PLAYER_SIZE / 2);
			incanvas.AddObject (inpo);
			outcanvas.AddObject (outpo);
			drawingarea2.WidthRequest = drawingarea2.HeightRequest = PLAYER_SIZE;
			drawingarea3.WidthRequest = drawingarea3.HeightRequest = PLAYER_SIZE;
		}

		public override void Dispose ()
		{
			Dispose (true);
			base.Dispose ();
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Disposed) {
				return;
			}
			if (disposing) {
				Destroy ();
			}
			Disposed = true;
		}

		protected bool Disposed { get; private set; } = false;

		public SubstitutionsEditorVM ViewModel {
			get {
				return editorVM;
			}
			set {
				editorVM = value;
				tagger.ViewModel = editorVM?.TeamTagger;
				inpo.ViewModel = editorVM?.InPlayer;
				outpo.ViewModel = editorVM?.OutPlayer;
				if (editorVM != null) {
					playershbox.Visible = !editorVM.LineupMode;
				}
			}
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = ((SubstitutionsEditorVM)viewModel as dynamic);
		}

		public KeyContext GetKeyContext ()
		{
			return new KeyContext ();
		}

		protected override void OnResponse (ResponseType response_id)
		{
			base.OnResponse (response_id);
			if (response_id == ResponseType.Ok) {
				ViewModel.SaveCommand.Execute ();
			}

			App.Current.StateController.MoveBack ();
		}

		protected override void OnDestroyed ()
		{
			base.OnDestroyed ();
			OnUnload ();
		}

		void HandleClickedEvent (ICanvasObject co)
		{
			LMPlayerView po = co as LMPlayerView;
			po.ViewModel.Tagged = !po.ViewModel.Tagged;
		}
	}
}
