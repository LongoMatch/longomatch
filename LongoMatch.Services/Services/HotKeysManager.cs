// HotKeysManager.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
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
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using System;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.Services
{
	public class HotKeysManager
	{
		Dictionary<HotKey, DashboardButton> dashboardHotkeys;
		IAnalysisWindow analysisWindow;
		Dashboard dashboard;

		public HotKeysManager ()
		{
			dashboardHotkeys = new Dictionary<HotKey,DashboardButton> ();
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			Config.EventsBroker.KeyPressed += UIKeyListener;
			Config.EventsBroker.KeyPressed += DashboardKeyListener;
			Config.EventsBroker.DashboardEditedEvent += HandleDashboardEditedEvent;
		}

		void ReloadHotkeys ()
		{
			dashboardHotkeys.Clear ();
			if (dashboard == null) {
				return;
			}
			foreach (DashboardButton button in dashboard.List) {
				if (button.HotKey.Defined && !dashboardHotkeys.ContainsKey (button.HotKey))
					dashboardHotkeys.Add (button.HotKey, button);
			}
		}

		void HandleDashboardEditedEvent ()
		{
			ReloadHotkeys ();
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 EventsFilter filter, IAnalysisWindow analysisWindow)
		{
			this.analysisWindow = analysisWindow;
			if (project == null) {
				dashboard = null;
			} else {
				dashboard = project.Dashboard;
			}
			ReloadHotkeys ();
		}

		public void UIKeyListener (object sender, HotKey key)
		{
			KeyAction action;

			try {
				action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
			} catch (Exception ex) {
				/* The dictionary contains 2 equal values for different keys */
				Log.Exception (ex);
				return;
			}
			
			if (action != KeyAction.None && analysisWindow != null) {
				switch (action) {
				case KeyAction.ZoomIn:
					analysisWindow.ZoomIn ();
					return;
				case KeyAction.ZoomOut:
					analysisWindow.ZoomOut ();
					return;
				case KeyAction.ShowDashboard:
					analysisWindow.ShowDashboard ();
					return;
				case KeyAction.ShowTimeline:
					analysisWindow.ShowTimeline ();
					return;
				case KeyAction.ShowPositions:
					analysisWindow.ShowZonalTags ();
					return;
				case KeyAction.FitTimeline:
					analysisWindow.FitTimeline ();
					return;
				}
			}
		}
		
		public void DashboardKeyListener (object sender, HotKey key)
		{
			KeyAction action;
			DashboardButton button;

			try {
				action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
			} catch (Exception ex) {
				return;
			}
			if (action != KeyAction.None) {
				/* Keep prevalence of general hotkeys over the dashboard ones */
				return;
			}
			
			if (!dashboardHotkeys.TryGetValue (key, out button)) {
				return;
			}
			if (! (button is AnalysisEventButton)) {
				analysisWindow.ClickButton (button);
			}
		}
	}
}
