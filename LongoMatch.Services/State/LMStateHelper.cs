//
//  Copyright (C) 2017 Fluendo S.A.
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

using System.Dynamic;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.ViewModel;

namespace LongoMatch.Services.State
{
	public static class LMStateHelper
	{
		public static void OpenProject (ProjectVM project, CaptureSettings props = null)
		{
			Log.Information ($"Open project {project.ProjectType}");
			dynamic settings = new ExpandoObject ();
			settings.Project = project;
			settings.CaptureSettings = props;
			if (project.Model.IsFakeCapture) {
				App.Current.StateController.MoveTo (NewProjectState.NAME, project);
			} else if (project.ProjectType == ProjectType.FileProject || project.ProjectType == ProjectType.EditProject) {
				App.Current.StateController.MoveTo (ProjectAnalysisState.NAME, settings, true);
			} else if (project.ProjectType == ProjectType.FakeCaptureProject) {
				App.Current.StateController.MoveTo (FakeLiveProjectAnalysisState.NAME, settings, true);
			} else {
				App.Current.StateController.MoveTo (LiveProjectAnalysisState.NAME, settings, true);
			}

			App.Current.EventsBroker.Publish (new OpenedProjectEvent { Project = project.Model });
		}
	}
}
