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
		ProjectType projectType;
		ICapturerBin capturer;
		Dashboard dashboard;
		AnalysisEventButton pendingButton;
		System.Threading.Timer timer;
		const int TIMEOUT_MS = 1000;

		public HotKeysManager ()
		{
			dashboardHotkeys = new Dictionary<HotKey,DashboardButton> ();
			Config.EventsBroker.OpenedProjectChanged += HandleOpenedProjectChanged;
			Config.EventsBroker.KeyPressed += DashboardKeyListener;
			Config.EventsBroker.KeyPressed += UIKeyListener;
			Config.EventsBroker.DashboardEditedEvent += HandleDashboardEditedEvent;
			timer = new System.Threading.Timer (HandleTimeout);
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

		void HandleTimeout (object state)
		{
			Config.DrawingToolkit.Invoke (delegate {
				if (pendingButton != null) {
					analysisWindow.ClickButton (pendingButton);
					pendingButton = null;
				}
			});
		}

		void HandleDashboardEditedEvent ()
		{
			ReloadHotkeys ();
		}

		void HandleOpenedProjectChanged (Project project, ProjectType projectType,
		                                 EventsFilter filter, IAnalysisWindow analysisWindow)
		{
			this.analysisWindow = analysisWindow;
			this.capturer = analysisWindow.Capturer;
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
				
				if (projectType == ProjectType.CaptureProject ||
					projectType == ProjectType.FakeCaptureProject ||
					projectType == ProjectType.URICaptureProject) {
					switch (action) {
					case KeyAction.PauseClock:
						if (capturer.Capturing) {
							capturer.PausePeriod ();
						} else {
							capturer.ResumePeriod ();
						}
						break;
					case KeyAction.StartPeriod:
						capturer.StartPeriod ();
						break;
					case KeyAction.StopPeriod:
						capturer.StopPeriod ();
						break;
					}
				}
			}
		}

		public void DashboardKeyListener (object sender, HotKey key)
		{
			KeyAction action;
			DashboardButton button;

			try {
				action = Config.Hotkeys.ActionsHotkeys.GetKeyByValue (key);
			} catch {
				return;
			}
			if (action != KeyAction.None) {
				/* Keep prevalence of general hotkeys over the dashboard ones */
				return;
			}
			
			if (dashboardHotkeys.TryGetValue (key, out button)) {
				if (button is AnalysisEventButton) {
					AnalysisEventButton evButton = button as AnalysisEventButton;
					/* Finish tagging for the pending button */
					if (pendingButton != null) {
						analysisWindow.ClickButton (button);
					}
					if (evButton.AnalysisEventType.Tags.Count == 0) {
						analysisWindow.ClickButton (button);
					} else {
						pendingButton = evButton;
						timer.Change (TIMEOUT_MS, 0);
					}
				} else {
					analysisWindow.ClickButton (button);
				}
			}
		}
	}
}
