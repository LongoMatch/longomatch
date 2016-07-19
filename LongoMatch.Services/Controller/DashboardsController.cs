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
using System;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Services.ViewModel;
using VAS.Core.MVVMC;
using VAS.Services.Controller;

namespace LongoMatch.Services.Controller
{
	[ControllerAttribute ("DashboardsManager")]
	public class DashboardsController:  TemplatesController<DashboardLongoMatch, DashboardVM>
	{
		public DashboardsController ()
		{
			TemplateName = "Dashboard";
			Extension = Constants.CAT_TEMPLATE_EXT;
			Provider = App.Current.CategoriesTemplatesProvider;

			FilterText = Catalog.GetString ("Dashboard files");
			AlreadyExistsText = Catalog.GetString ("A dashboard with the same name already exists.");
			ExportedCorrectlyText = Catalog.GetString ("Dashboard exported correctly");
			CountText = Catalog.GetString ("Event types:");
			NewText = Catalog.GetString ("New dashboard");
			OverwriteText = Catalog.GetString ("Do you want to overwrite it?");
			ErrorSavingText = Catalog.GetString ("Error saving dashboard");
			ConfirmDeleteText = Catalog.GetString ("Do you really want to delete the dashboard: ");
			CouldNotLoadText = Catalog.GetString ("Could not load dashboard");
			AlreadyExistsText = Catalog.GetString ("A dashboard with the same name already exists");
			NotEditableText = Catalog.GetString ("System dashboards can't be edited, do you want to create a copy?");
			ConfirmSaveText = Catalog.GetString ("Do you want to save the current dashboard");
			ImportText = Catalog.GetString ("Import dashboard");
			NameText = Catalog.GetString ("Dashboard name:");
		}
	}
}

