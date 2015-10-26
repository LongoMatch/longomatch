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
using System.Collections.Generic;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class FakeAnalysisComponent : Gtk.Bin,  IAnalysisWindow
	{

		public FakeAnalysisComponent ()
		{
			this.Build ();
			capturerbin.Mode = CapturerType.Fake;
		}

		#region IAnalysisWindow implementation

		public void SetProject (Project project, ProjectType projectType, CaptureSettings props, EventsFilter filter)
		{
			codingwidget1.SetProject (project, projectType, filter);
		}

		public void ReloadProject ()
		{
		}

		public void CloseOpenedProject ()
		{
		}

		public void AddPlay (TimelineEvent play)
		{
			codingwidget1.AddPlay (play);
		}

		public void DeletePlays (List<TimelineEvent> plays)
		{
			codingwidget1.DeletePlays (plays);
		}

		public void UpdateCategories ()
		{
			codingwidget1.UpdateCategories ();
		}

		public void DetachPlayer ()
		{
		}

		public void ZoomIn ()
		{
		}

		public void ZoomOut ()
		{
		}

		public void FitTimeline ()
		{
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

		public void ClickButton (DashboardButton button, Tag tag = null)
		{
			codingwidget1.ClickButton (button, tag);
		}

		public void TagPlayer (Player player)
		{
			codingwidget1.TagPlayer (player);
		}

		public void TagTeam (TeamType team)
		{
			codingwidget1.TagTeam (team);
		}

		public IPlayerController Player {
			get {
				return null;
			}
		}

		public ICapturerBin Capturer {
			get {
				return capturerbin;
			}
		}

		#endregion
	}
}

