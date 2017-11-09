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
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Core.Store;
using VAS.Services.Controller;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	public class ProjectsManagerState : ScreenState<SportsProjectsManagerVM>
	{
		public const string NAME = "ProjectsManager";

		public override string Name
		{
			get
			{
				return NAME;
			}
		}

		protected override void CreateViewModel(dynamic data)
		{
			ViewModel = new SportsProjectsManagerVM();
			if (App.Current.LicenseLimitationsService != null)
			{
				ViewModel.LimitationChart = App.Current.LicenseLimitationsService.CreateBarChartVM(
					LongoMatchCountLimitedObjects.Projects.ToString());
			}
		}

		protected override void CreateControllers(dynamic data)
		{
			Controllers.Add(new MediaFileSetController());
		}

		public override async Task<bool> ShowState()
		{
			if (!await base.ShowState())
			{
				return false;
			}

			ViewModel.Model.Reset(App.Current.DatabaseManager.ActiveDB.RetrieveAll<LMProject>().
								   SortByCreationDate(true));

			var projects = new RangeObservableCollection<LMProject>();
			var teams = App.Current.TeamTemplatesProvider.Templates;

			for (int i = 0; i < 50; i++)
			{
				var project = new LMProject();
				project.LocalTeamTemplate = (LMTeam)teams[0];
				project.VisitorTeamTemplate = (LMTeam)teams[1];
				project.ProjectType = ProjectType.FileProject;
				project.Description.Competition = $"Test Comp {i}";
				project.Description.DashboardName = $"Test Dash {i}";
				project.Description.Description = $"Test Des {i}";
				project.Description.LocalShield = project.LocalTeamTemplate.Shield;
				project.Description.VisitorShield = project.VisitorTeamTemplate.Shield;
				project.Description.LocalGoals = 1;
				project.Description.VisitorGoals = 6;
				project.Description.LocalName = "Manchester U";
				project.Description.VisitorName = "FC Barcelona";
				project.Description.FileSet = new MediaFileSet();
				project.IsLoaded = false;
				projects.Add(project);
			}
			ViewModel.Model.AddRange(projects);

			if (ViewModel.Selection.Count == 0)
			{
				ViewModel.Select(ViewModel.ViewModels.FirstOrDefault());
			}
			return true;
		}
	}
}

