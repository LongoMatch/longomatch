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
using System;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Filters;
using VAS.Core.Interfaces;
using VAS.Core.Interfaces.GUI;
using VAS.Core.Store;
using VAS.Core.Store.Playlists;
using VAS.Core.Store.Templates;
using LMCommon = LongoMatch.Core.Common;

namespace LongoMatch.Services
{
	public class HotKeysManager: IService
	{
		Dictionary<HotKey, DashboardButton> dashboardHotkeys;
		IAnalysisWindow analysisWindow;
		ProjectType projectType;
		ICapturerBin capturer;
		IPlayerController player;
		Dashboard dashboard;
		ProjectLongoMatch openedProject;
		AnalysisEventButton pendingButton;
		bool inPlayerTagging;
		string playerNumber;
		TeamType taggedTeam;
		System.Threading.Timer timer;
		const int TIMEOUT_MS = 1000;

		public HotKeysManager ()
		{
			dashboardHotkeys = new Dictionary<HotKey,DashboardButton> ();
		}

		void TagPlayer ()
		{
			int playerNumber;
			
			if (int.TryParse (this.playerNumber, out playerNumber)) {
				SportsTeam team = taggedTeam == TeamType.LOCAL ? openedProject.LocalTeamTemplate :
					openedProject.VisitorTeamTemplate;
				PlayerLongoMatch player = team.Players.FirstOrDefault (p => p.Number == playerNumber);
				if (player != null) {
					analysisWindow.TagPlayer (player);
				}
			}
			inPlayerTagging = false;
			this.playerNumber = "";
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
			App.Current.DrawingToolkit.Invoke (delegate {
				if (pendingButton != null) {
					analysisWindow.ClickButton (pendingButton);
					pendingButton = null;
				} else if (inPlayerTagging) {
					TagPlayer ();
				}
			});
		}

		void HandleDashboardEditedEvent (DashboardEditedEvent e)
		{
			ReloadHotkeys ();
		}

		void HandleOpenedProjectChanged (OpenedProjectEvent e)
		{
			this.analysisWindow = e.AnalysisWindow as IAnalysisWindow;
			capturer = e.AnalysisWindow.Capturer;
			player = e.AnalysisWindow.Player;
			openedProject = e.Project as ProjectLongoMatch;
			this.projectType = e.ProjectType;
			if (e.Project == null) {
				dashboard = null;
			} else {
				dashboard = e.Project.Dashboard;
			}
			ReloadHotkeys ();
		}

		void HandleOpenedPresentationChanged (OpenedPresentationChangedEvent e)
		{
			if (e.Player == null)
				return;
			this.player = e.Player;

			projectType = ProjectType.None;
			analysisWindow = null;
		}

		public void UIKeyListener (KeyPressedEvent e)
		{
			KeyAction action;

			try {
				action = App.Current.Config.Hotkeys.ActionsHotkeys.GetKeyByValue (e.Key);
			} catch (Exception ex) {
				/* The dictionary contains 2 equal values for different keys */
				Log.Exception (ex);
				return;
			}
			
			if (action != KeyAction.None) {
				
				if (analysisWindow != null) {
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
				} else {
					switch (action) {
					case KeyAction.FrameUp:
						player.SeekToNextFrame ();
						return;
					case KeyAction.FrameDown:
						player.SeekToPreviousFrame ();
						return;
					case KeyAction.JumpUp:
						player.StepForward ();
						return;
					case KeyAction.JumpDown:
						player.StepBackward ();
						return;
					case KeyAction.DrawFrame:
						player.DrawFrame ();
						return;
					case KeyAction.TogglePlay:
						player.TogglePlay ();
						return;
					case KeyAction.SpeedUp:
						player.FramerateUp ();
						App.Current.EventsBroker.Publish<PlaybackRateChangedEvent> (
							new PlaybackRateChangedEvent {
								Value = (float)player.Rate
							}
						);
						return;
					case KeyAction.SpeedDown:
						player.FramerateDown ();
						App.Current.EventsBroker.Publish<PlaybackRateChangedEvent> (
							new PlaybackRateChangedEvent {
								Value = (float)player.Rate
							}
						);
						return;
					case KeyAction.CloseEvent:
						App.Current.EventsBroker.Publish<LoadEventEvent> (new LoadEventEvent ());
						return;
					case KeyAction.Prev:
						player.Previous ();
						return;
					case KeyAction.Next:
						player.Next ();
						return;
					case KeyAction.SpeedUpper:
						player.FramerateUpper ();
						return;
					case KeyAction.SpeedLower:
						player.FramerateLower ();
						return;
					}
				}
			}
		}

		public void DashboardKeyListener (KeyPressedEvent e)
		{
			KeyAction action;
			DashboardButton button;

			if (openedProject == null) {
				return;
			}

			try {
				action = App.Current.Config.Hotkeys.ActionsHotkeys.GetKeyByValue (e.Key);
			} catch {
				return;
			}
			
			if (action == KeyAction.LocalPlayer || action == KeyAction.VisitorPlayer) {
				if (inPlayerTagging) {
					TagPlayer ();
				}
				if (pendingButton != null) {
					analysisWindow.ClickButton (pendingButton);
				}
				inPlayerTagging = true;
				taggedTeam = action == KeyAction.LocalPlayer ? TeamType.LOCAL : TeamType.VISITOR;
				playerNumber = "";
				analysisWindow.TagTeam (taggedTeam);
				timer.Change (TIMEOUT_MS, 0);
			} else if (action == KeyAction.None) {
				if (pendingButton != null) {
					Tag tag = pendingButton.AnalysisEventType.Tags.FirstOrDefault (t => t.HotKey == e.Key);
					if (tag != null) {
						analysisWindow.ClickButton (pendingButton, tag);
						timer.Change (TIMEOUT_MS, 0);
					}
				} else if (dashboardHotkeys.TryGetValue (e.Key, out button)) {
					if (inPlayerTagging) {
						TagPlayer ();
					}
					if (button is AnalysisEventButton) {
						AnalysisEventButton evButton = button as AnalysisEventButton;
						/* Finish tagging for the pending button */
						if (pendingButton != null) {
							analysisWindow.ClickButton (pendingButton);
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
				} else if (inPlayerTagging) {
					int number;
					string name = App.Current.Keyboard.NameFromKeyval ((uint)e.Key.Key);
					if (name.StartsWith ("KP_")) {
						name = name.Replace ("KP_", "");
					}
					if (int.TryParse (name, out number)) {
						playerNumber += number.ToString ();
						timer.Change (TIMEOUT_MS, 0);
					}
					return;
				}
			}
		}

		#region IService

		public int Level {
			get {
				return 70;
			}
		}

		public string Name {
			get {
				return "HotKeys";
			}
		}

		public bool Start ()
		{
			App.Current.EventsBroker.Subscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			App.Current.EventsBroker.Subscribe<OpenedPresentationChangedEvent> (HandleOpenedPresentationChanged);
			App.Current.EventsBroker.Subscribe<KeyPressedEvent> (DashboardKeyListener);
			App.Current.EventsBroker.Subscribe<KeyPressedEvent> (UIKeyListener);
			App.Current.EventsBroker.Subscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			timer = new System.Threading.Timer (HandleTimeout);
			return true;
		}

		public bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<OpenedProjectEvent> (HandleOpenedProjectChanged);
			App.Current.EventsBroker.Unsubscribe<OpenedPresentationChangedEvent> (HandleOpenedPresentationChanged);
			App.Current.EventsBroker.Unsubscribe<KeyPressedEvent> (DashboardKeyListener);
			App.Current.EventsBroker.Unsubscribe<KeyPressedEvent> (UIKeyListener);
			App.Current.EventsBroker.Unsubscribe<DashboardEditedEvent> (HandleDashboardEditedEvent);
			return true;
		}

		#endregion
	}
}
