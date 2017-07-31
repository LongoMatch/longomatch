//
//  Copyright (C) 2016 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
using System;
using System.Collections.Generic;
using LongoMatch.Services.State;
using VAS.Core.Interfaces.GUI;
using VAS.Services.State;
using PreferencesState = LongoMatch.Services.State.PreferencesState;

namespace LongoMatch.Services.States
{
	public class CoreTool : ITool
	{
		List<IWelcomeButton> listWelcome;
		Dictionary<string, Func<IScreenState>> uiFlow;

		public CoreTool ()
		{
			listWelcome = new List<IWelcomeButton> ();
			uiFlow = new Dictionary<string, Func<IScreenState>> ();
			uiFlow.Add (HomeState.NAME, () => new HomeState ());
			uiFlow.Add (TeamsManagerState.NAME, () => new TeamsManagerState ());
			uiFlow.Add (DashboardsManagerState.NAME, () => new DashboardsManagerState ());
			uiFlow.Add (PreferencesState.NAME, () => new PreferencesState ());
			uiFlow.Add (NewProjectState.NAME, () => new NewProjectState ());
			uiFlow.Add (ProjectsManagerState.NAME, () => new ProjectsManagerState ());
			uiFlow.Add (OpenProjectState.NAME, () => new OpenProjectState ());
			uiFlow.Add (DrawingToolState.NAME, () => new LMDrawingToolState ());
			uiFlow.Add (JobsManagerState.NAME, () => new JobsManagerState ());
			uiFlow.Add (ProjectAnalysisState.NAME, () => new ProjectAnalysisState ());
			uiFlow.Add (LiveProjectAnalysisState.NAME, () => new LiveProjectAnalysisState ());
			uiFlow.Add (FakeLiveProjectAnalysisState.NAME, () => new FakeLiveProjectAnalysisState ());
			uiFlow.Add (DatabasesManagerState.NAME, () => new DatabasesManagerState ());
			uiFlow.Add (PlayEditorState.NAME, () => new PlayEditorState ());
			uiFlow.Add (SubstitutionsEditorState.NAME, () => new SubstitutionsEditorState ());
			uiFlow.Add (CameraSynchronizationState.NAME, () => new CameraSynchronizationState ());
			uiFlow.Add (CameraSynchronizationEditorState.NAME, () => new CameraSynchronizationEditorState ());
			uiFlow.Add (EditPlaylistElementState.NAME, () => new EditPlaylistElementState ());
			uiFlow.Add (UpgradeLimitationState.NAME, () => new LMUpgradeLimitationState ());
		}

		#region ITool implementation

		public void Enable ()
		{
			LoadStates ();
		}

		public void Disable ()
		{
			UnLoadStates ();
		}

		public string Name {
			get {
				return "CoreTool";
			}
		}

		public IEnumerable<IWelcomeButton> WelcomePanelIconList {
			get {
				return listWelcome;
			}
		}

		public Dictionary<string, Func<IScreenState>> UIFlow {
			get {
				return uiFlow;
			}
		}

		public string MenubarLabel {
			get {
				throw new NotImplementedException ();
			}
		}

		public string MenubarAccelerator {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

		void LoadStates ()
		{
			foreach (var ui in uiFlow) {
				App.Current.StateController.Register (ui.Key, ui.Value);
			}
		}

		void UnLoadStates ()
		{
			foreach (var ui in uiFlow) {
				App.Current.StateController.UnRegister (ui.Key);
			}
		}
	}
}

