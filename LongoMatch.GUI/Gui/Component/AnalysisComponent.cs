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
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Gui.Helpers;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
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
			codingwidget.Player = playercapturer.Player;
		}

		protected override void OnDestroyed ()
		{
			if (detachedPlayer) {
				playerWindow.Destroy ();
				detachedPlayer = false;
			}
			playercapturer.Destroy ();
			base.OnDestroyed ();
		}

		public override void Destroy ()
		{			
			if (detachedPlayer) {
				DetachPlayer ();
			}
			base.Destroy ();
		}

		public IPlayerController Player {
			get {
				return playercapturer.Player;
			}
		}

		public ICapturerBin Capturer {
			get {
				return playercapturer.Capturer;
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

		public void ClickButton (DashboardButton button, Tag tag = null)
		{
			codingwidget.ClickButton (button, tag);
		}

		public void TagPlayer (Player player)
		{
			codingwidget.TagPlayer (player);
		}

		public void TagTeam (TeamType team)
		{
			codingwidget.TagTeam (team);
		}

		public void DetachPlayer ()
		{
			bool isPlaying = Player.Playing;
			
			/* Pause the player here to prevent the sink drawing while the windows
			 * are beeing changed */
			Player.Pause ();
			if (!detachedPlayer) {
				Log.Debug ("Detaching player");
				
				ExternalWindow playerWindow = new ExternalWindow ();
				this.playerWindow = playerWindow;
				playerWindow.Title = Constants.SOFTWARE_NAME;
				int player_width = playercapturer.Allocation.Width;
				int player_height = playercapturer.Allocation.Height;
				playerWindow.SetDefaultSize (player_width, player_height);
				playerWindow.DeleteEvent += (o, args) => DetachPlayer ();
				playerWindow.Show ();
				playercapturer.Reparent (playerWindow.Box);
				// Hack to reposition video window in widget for OSX
				playerWindow.Resize (player_width + 10, player_height);
				videowidgetsbox.Visible = false;
			} else {
				Log.Debug ("Attaching player again");
				videowidgetsbox.Visible = true;
				playercapturer.Reparent (this.videowidgetsbox);
				playerWindow.Destroy ();
			}
			if (isPlaying) {
				Player.Play ();
			}
			detachedPlayer = !detachedPlayer;
			playercapturer.AttachPlayer (detachedPlayer);
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
				playercapturer.Mode = PlayerViewOperationMode.Analysis;
			} else {
				playercapturer.Mode = playercapturer.Mode = PlayerViewOperationMode.LiveAnalysisReview;
				Capturer.PeriodsNames = project.Dashboard.GamePeriods.ToList ();
				Capturer.Periods = project.Periods.ToList ();
			}
		}

		public void ReloadProject ()
		{
			codingwidget.SetProject (openedProject, projectType, filter);
			playsSelection.SetProject (openedProject, filter);
		}
	}
}

