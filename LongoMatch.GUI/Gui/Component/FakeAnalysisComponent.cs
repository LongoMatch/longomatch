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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Hotkeys;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using LKeyAction = LongoMatch.Core.Common.KeyAction;
using VKeyAction = VAS.Core.Hotkeys.KeyAction;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	[View (FakeLiveProjectAnalysisState.NAME)]
	public partial class FakeAnalysisComponent : Gtk.Bin, IPanel<LMProjectAnalysisVM>
	{
		LMProjectAnalysisVM viewModel;

		public FakeAnalysisComponent ()
		{
			this.Build ();
			capturerbin.Mode = CapturerType.Fake;
		}

		public void Dispose ()
		{
			Destroy ();
		}

		protected override void OnDestroyed ()
		{
			OnUnload ();
			codingwidget1.Destroy ();
		}


		public IVideoPlayerController Player {
			get {
				return null;
			}
		}

		public ICapturerBin Capturer {
			get {
				return capturerbin;
			}
		}

		public LMProjectAnalysisVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				codingwidget1.ViewModel = value;
			}
		}

		public string Title {
			get {
				return ViewModel?.Project.ShortDescription;
			}
		}

		public void OnLoad ()
		{
		}

		public void OnUnload ()
		{
		}

		public KeyContext GetKeyContext ()
		{
			var keyContext = new KeyContext ();
			/*keyContext.AddAction (
				new VKeyAction ("ZOOM_IN", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ZoomIn],
								() => codingwidget1.ZoomIn ()));
			keyContext.AddAction (
				new VKeyAction ("ZOOM_OUT", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ZoomOut],
								() => codingwidget1.ZoomOut ()));
			keyContext.AddAction (
				new VKeyAction ("FIT_TIMELINE", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.FitTimeline],
								() => codingwidget1.FitTimeline ()));
			keyContext.AddAction (
				new VKeyAction ("SHOW_DASHBOARD", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ShowDashboard],
								() => codingwidget1.ShowDashboard ()));
			keyContext.AddAction (
				new VKeyAction ("SHOW_TIMELINE", App.Current.Config.Hotkeys.ActionsHotkeys [LKeyAction.ShowDashboard],
								() => codingwidget1.ShowTimeline ()));*/
			return keyContext;
		}


		public void SetViewModel (object viewModel)
		{
			ViewModel = (LMProjectAnalysisVM)viewModel;
		}

		public void ShowDashboard ()
		{
			codingwidget1.ShowDashboard ();
		}

		public void ShowTimeline ()
		{
			codingwidget1.ShowTimeline ();
		}

		public void ShowZonalTags ()
		{
			codingwidget1.ShowZonalTags ();
		}

		public void TagPlayer (Player player)
		{
			codingwidget1.TagPlayer ((LMPlayer)player);
		}

		public void TagTeam (TeamType team)
		{
			codingwidget1.TagTeam (team);
		}

	}
}
