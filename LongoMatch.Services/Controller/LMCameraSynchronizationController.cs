//
//  Copyright (C) 2017 Fluendo S.A.
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
using LongoMatch.Services.State;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.Controller;
using VAS.Services.State;
using LongoMatch.Core.Store;

namespace LongoMatch.Services.Controller
{
	/// <summary>
	/// A controller to edit periods and synchronize different cameras in a newly created project.
	/// </summary>
	[Controller (CameraSynchronizationState.NAME)]
	public class LMCameraSynchronizationController : CameraSynchronizationController
	{
		protected override void HandleSave (SaveEvent<ProjectVM> saveEvent)
		{
			if (projectVM != saveEvent.Object) {
				return;
			}

			SaveChanges (saveEvent);

			if (projectVM.ProjectType == ProjectType.EditProject) {
				projectVM.ProjectType = ProjectType.FileProject;
			} else {
				(projectVM.Model as LMProject).CreateLineupEvent ();
			}
			LMStateHelper.OpenProject (projectVM);
		}
	}
}
