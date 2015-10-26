// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using LongoMatch.Core.Store;

namespace LongoMatch.Core.Interfaces.GUI
{
	public interface IAnalysisWindow
	{
		void SetProject (Project project, ProjectType projectType, CaptureSettings props, EventsFilter filter);

		void ReloadProject ();

		void CloseOpenedProject ();

		void AddPlay (TimelineEvent play);

		void UpdateCategories ();

		void DeletePlays (List<TimelineEvent> plays);

		void DetachPlayer ();

		void ZoomIn ();

		void ZoomOut ();

		void FitTimeline ();

		void ShowDashboard ();

		void ShowTimeline ();

		void ShowZonalTags ();

		void ClickButton (DashboardButton button, Tag tag = null);

		void TagPlayer (Player player);

		void TagTeam (TeamType team);

		IPlayerController Player{ get; }

		ICapturerBin Capturer{ get; }
	}
}

