//
//  Copyright (C) 2016 Fluendo S.A.
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
using LongoMatch.Core;
using LongoMatch.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace LongoMatch.Services.Controller
{
	/// <summary>
	/// Controller for teams.
	/// </summary>
	[ControllerAttribute ("TeamsManager")]
	public class TeamsController : TemplatesController<Team, TeamVM, Player, PlayerVM>
	{
		public TeamsController ()
		{
			TemplateName = "Team";
			Extension = Constants.TEAMS_TEMPLATE_EXT;
			Provider = App.Current.TeamTemplatesProvider;

			FilterText = Catalog.GetString ("Team files");
			AlreadyExistsText = Catalog.GetString ("A team with the same name already exists.");
			ExportedCorrectlyText = Catalog.GetString ("Team exported correctly");
			CountText = Catalog.GetString ("Players:");
			NewText = Catalog.GetString ("New team");
			OverwriteText = Catalog.GetString ("Do you want to overwrite it?");
			ErrorSavingText = Catalog.GetString ("Error saving team");
			ConfirmDeleteText = Catalog.GetString ("Do you really want to delete the team: ");
			CouldNotLoadText = Catalog.GetString ("Could not load team");
			AlreadyExistsText = Catalog.GetString ("A team with the same name already exists");
			ConfirmSaveText = Catalog.GetString ("Do you want to save the current team");
			ImportText = Catalog.GetString ("Import team");
			NameText = Catalog.GetString ("Team name:");
		}

		protected override bool SaveValidations (Team model)
		{
			return true;
		}
	}
}

