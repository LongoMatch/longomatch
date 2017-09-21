//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using System.Linq;
using LongoMatch.Core.Hotkeys;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.UI.Helpers;
using Constants = LongoMatch.Core.Common.Constants;
using VKeyAction = VAS.Core.Hotkeys.KeyAction;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	[View (ProjectAnalysisState.NAME)]
	[View (LiveProjectAnalysisState.NAME)]
	public partial class AnalysisComponent : Gtk.Bin, IPanel<LMProjectAnalysisVM>
	{
		bool detachedPlayer;
		LMProjectAnalysisVM viewModel;
		Gtk.Window playerWindow;
		ProjectFileMenuLoader fileMenuLoader;
		ProjectToolsMenuLoader toolsMenuLoader;

		public AnalysisComponent ()
		{
			this.Build ();
			detachedPlayer = false;
			fileMenuLoader = new ProjectFileMenuLoader ();
			toolsMenuLoader = new ProjectToolsMenuLoader ();
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

		protected override void OnDestroyed ()
		{
			if (detachedPlayer) {
				playerWindow.Destroy ();
				detachedPlayer = false;
			}
			playercapturer.Dispose ();
			playsSelection.Dispose ();
			base.OnDestroyed ();
		}

		protected override void OnUnmapped ()
		{
			base.OnUnmapped ();
			// When a container widget is unmapped there are 2 options, either it has a window and it hides it,
			// or it just unmaps all children. In our case we are use an event box with its own window for theming.
			// Unmapping this child will just hide its window and won't unmap the children below such as as the 
			// player view and the video window. This can be a problem as this widget will never detect that it has been 
			// hidden and on windows the actual gdkwindow will be hidden and won't be shown again. So we make sure to
			// proxy the unmap to the children below the eventbox.
			centralpane.Unmap ();
		}

		public IVideoPlayerController Player {
			get {
				return playercapturer.ViewModel.Player;
			}
		}

		public ICapturerBin Capturer {
			get {
				return playercapturer.Capturer;
			}
		}

		public LMProjectAnalysisVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				playercapturer.ViewModel = viewModel.VideoPlayer;
				codingwidget.ViewModel = viewModel;
				playsSelection.ViewModel = viewModel.Project;
				if (viewModel.Project.Model.ProjectType == ProjectType.FileProject) {
					playercapturer.Mode = PlayerViewOperationMode.Analysis;
				} else {
					playercapturer.Mode = playercapturer.Mode = PlayerViewOperationMode.LiveAnalysisReview;
					Capturer.PeriodsNames = viewModel.Project.Model.Dashboard.GamePeriods.ToList ();
					Capturer.Periods = viewModel.Project.Model.Periods.ToList ();
				}
			}
		}

		public string Title {
			get {
				return ViewModel?.Project.ShortDescription;
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMProjectAnalysisVM)viewModel;
		}

		public KeyContext GetKeyContext ()
		{
			var keyContext = new KeyContext ();
			keyContext.AddAction (
				new VKeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.ZOOM_IN),
							   () => codingwidget.ZoomIn ()));
			keyContext.AddAction (
				new VKeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.ZOOM_OUT),
							   () => codingwidget.ZoomOut ()));
			keyContext.AddAction (
				new VKeyAction (App.Current.HotkeysService.GetByName (LMGeneralUIHotkeys.SHOW_ZONAL_TAGS),
							   () => codingwidget.ShowZonalTags ()));
			keyContext.AddAction (
				new VKeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.FIT_TIMELINE),
							   () => codingwidget.FitTimeline ()));
			keyContext.AddAction (
				new VKeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.SHOW_DASHBOARD),
							   () => codingwidget.ShowDashboard ()));
			keyContext.AddAction (
				new VKeyAction (App.Current.HotkeysService.GetByName (GeneralUIHotkeys.SHOW_TIMELINE),
							   () => codingwidget.ShowTimeline ()));

			return keyContext;
		}

		bool Disposed { get; set; }

		public void OnLoad ()
		{
			App.Current.EventsBroker.Subscribe<DetachEvent> (DetachPlayer);
			fileMenuLoader.LoadMenu (viewModel);
			toolsMenuLoader.LoadMenu (viewModel);
		}

		public void OnUnload ()
		{
			App.Current.EventsBroker.Unsubscribe<DetachEvent> (DetachPlayer);
			fileMenuLoader.UnloadMenu ();
			toolsMenuLoader.UnloadMenu ();
		}

		void DetachPlayer (DetachEvent evt)
		{
			bool isPlaying = ViewModel.VideoPlayer.Playing;

			/* Pause the player here to prevent the sink drawing while the windows
			 * are beeing changed */
			ViewModel.VideoPlayer.Pause ();
			if (!detachedPlayer) {
				Log.Debug ("Detaching player");

				ExternalWindow playerWindow = new ExternalWindow ();
				this.playerWindow = playerWindow;
				playerWindow.Title = Constants.SOFTWARE_NAME;
				int player_width = playercapturer.Allocation.Width;
				int player_height = playercapturer.Allocation.Height;
				playerWindow.SetDefaultSize (player_width, player_height);
				playerWindow.DeleteEvent += (o, args) => DetachPlayer (new DetachEvent ());
				playerWindow.Show ();
				playercapturer.Reparent (playerWindow.Box);
				// Hack to reposition video window in widget for OSX
				playerWindow.Resize (player_width + 10, player_height);
				videowidgetsbox.Visible = false;
				playsSelection.ExpandTabs = true;
			} else {
				Log.Debug ("Attaching player again");
				videowidgetsbox.Visible = true;
				playercapturer.Reparent (this.videowidgetsbox);
				playerWindow.Destroy ();
				playsSelection.ExpandTabs = false;
			}
			if (isPlaying) {
				ViewModel.VideoPlayer.Play ();
			}
			detachedPlayer = !detachedPlayer;
			playercapturer.AttachPlayer (detachedPlayer);
		}

		public void CloseOpenedProject ()
		{
			if (detachedPlayer)
				DetachPlayer (new DetachEvent ());
		}
	}
}
