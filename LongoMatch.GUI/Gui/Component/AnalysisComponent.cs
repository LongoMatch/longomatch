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
using System.Collections.Generic;
using Gtk;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AnalysisComponent : Gtk.Bin, IAnalysisWindow
	{
		static Project openedProject;
		ProjectType projectType;
		EventsFilter filter;
		bool detachedPlayer;
		Gtk.Window playerWindow;

		public AnalysisComponent ()
		{
			this.Build ();
			projectType = ProjectType.None;
			detachedPlayer = false;
		}

		protected override void OnDestroyed ()
		{
			playercapturer.Destroy ();
			base.OnDestroyed ();
		}

		public IPlayerBin Player {
			get {
				return playercapturer;
			}
		}

		public ICapturerBin Capturer {
			get {
				return playercapturer;
			}
		}

		public void AddPlay (TimelineEvent play)
		{
			playsSelection.AddPlay (play);
			codingwidget.AddPlay (play);
		}

		public void UpdateCategories ()
		{
			codingwidget.UpdateCategories ();
		}

		public void DeletePlays (List<TimelineEvent> plays)
		{
			playsSelection.RemovePlays (plays);
			codingwidget.DeletePlays (plays);
		}

		public void ZoomIn ()
		{
			codingwidget.ZoomIn ();
		}

		public void ZoomOut ()
		{
			codingwidget.ZoomOut ();
		}

		public void FitTimeline ()
		{
			codingwidget.FitTimeline ();
		}

		public void ShowDashboard ()
		{
			codingwidget.ShowDashboard ();
		}

		public void ShowTimeline ()
		{
			codingwidget.ShowTimeline ();
		}

		public void ShowZonalTags ()
		{
			codingwidget.ShowZonalTags ();
		}

		public void ClickButton (DashboardButton button)
		{
			codingwidget.ClickButton (button);
		}

		public void DetachPlayer ()
		{
			bool isPlaying = playercapturer.Playing;
			
			/* Pause the player here to prevent the sink drawing while the windows
			 * are beeing changed */
			playercapturer.Pause ();
			if (!detachedPlayer) {
				EventBox box;
				Log.Debug ("Detaching player");
				
				playerWindow = new Gtk.Window (Constants.SOFTWARE_NAME);
				playerWindow.SetDefaultSize (playercapturer.Allocation.Width, playercapturer.Allocation.Height);
				playerWindow.Icon = Stetic.IconLoader.LoadIcon (this, "longomatch", IconSize.Button);
				playerWindow.DeleteEvent += (o, args) => DetachPlayer ();
				box = new EventBox ();
				box.Name = "lightbackgroundeventbox";
				box.KeyPressEvent += (o, args) => {
					Config.EventsBroker.EmitKeyPressed (this, Keyboard.ParseEvent (args.Event));
				};
				playerWindow.Add (box);
				
				box.Show ();
				box.CanFocus = true;
				playerWindow.Show ();
				playercapturer.Reparent (box);
				playerWindow.Focus = box;
				videowidgetsbox.Visible = false;
			} else {
				Log.Debug ("Attaching player again");
				videowidgetsbox.Visible = true;
				playercapturer.Reparent (this.videowidgetsbox);
				playerWindow.Destroy ();
			}
			if (isPlaying) {
				playercapturer.Play ();
			}
			detachedPlayer = !detachedPlayer;
		}

		public void CloseOpenedProject ()
		{
			openedProject = null;
			projectType = ProjectType.None;
			if (detachedPlayer)
				DetachPlayer ();
		}

		public void SetProject (Project project, ProjectType projectType, CaptureSettings props, EventsFilter filter)
		{
			openedProject = project;
			this.projectType = projectType;
			this.filter = filter;
			
			codingwidget.SetProject (project, projectType, filter);
			playsSelection.SetProject (project, filter);
			if (projectType == ProjectType.FileProject) {
				playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.Player;
			} else {
				if (projectType == ProjectType.FakeCaptureProject) {
					playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.FakeCapturer;
				} else {
					playercapturer.Mode = PlayerCapturerBin.PlayerOperationMode.PreviewCapturer;
				}
				playercapturer.PeriodsNames = project.Dashboard.GamePeriods;
				playercapturer.Periods = project.Periods;
			}
		}

		public void ReloadProject ()
		{
			codingwidget.SetProject (openedProject, projectType, filter);
			playsSelection.SetProject (openedProject, filter);
		}
	}
}

