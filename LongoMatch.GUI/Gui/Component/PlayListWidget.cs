// PlayListWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using Gtk;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.ViewModel;
using VAS.UI.Helpers;
using Misc = VAS.UI.Helpers.Misc;
using VAS.Core.MVVMC;
using VAS.UI.Helpers.Bindings;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.Category ("LongoMatch")]
	[System.ComponentModel.ToolboxItem (true)]
	public partial class PlayListWidget : Gtk.Bin, IView<PlaylistCollectionVM>
	{
		PlaylistCollectionVM viewModel;
		LMPlaylistTreeView playlistTreeView;
		BindingContext ctx;

		public PlayListWidget ()
		{
			this.Build ();

			playlistTreeView = new LMPlaylistTreeView ();
			playlistTreeView.Show ();
			scrolledwindow1.Add (playlistTreeView);

			// Force tooltips to be translatable as there seems to be a bug in stetic 
			// code generation for translatable tooltips.
			newbutton.TooltipMarkup = Catalog.GetString ("Create a new playlist");
			newvideobutton.TooltipMarkup = Catalog.GetString ("Export the playlist to new video file");

			newbutton.CanFocus = false;
			newvideobutton.CanFocus = false;

			hbox2.HeightRequest = StyleConf.PlayerCapturerControlsHeight;
			recimage.Image = App.Current.ResourcesLocator.LoadIcon ("vas-control-record", StyleConf.PlayerCapturerIconSize);
			newvideobutton.Clicked += HandleRenderPlaylistClicked;

			Bind ();
		}

		public PlaylistCollectionVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				playlistTreeView.ViewModel = value;
				ctx.UpdateViewModel (viewModel);
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (PlaylistCollectionVM)viewModel;
		}

		void Bind ()
		{
			ctx = this.GetBindingContext ();
			ctx.Add (newbutton.BindWithIcon (
				App.Current.ResourcesLocator.LoadIcon ("lm-playlist-new", StyleConf.PlayerCapturerIconSize),
				vm => ((PlaylistCollectionVM)vm).NewCommand));
		}

		void HandleRenderPlaylistClicked (object sender, System.EventArgs ea)
		{
			Menu menu;

			menu = new Menu ();
			foreach (PlaylistVM playlist in ViewModel.ViewModels) {
				MenuItem plmenu = new MenuItem (playlist.Name);
				plmenu.Activated += (s, e) => App.Current.EventsBroker.Publish (
					new RenderPlaylistEvent { Playlist = playlist.Model });
				menu.Append (plmenu);
			}
			menu.ShowAll ();
			menu.Popup ();
		}
	}
}
