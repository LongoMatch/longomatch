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
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.States;
using VAS.Core;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Services.Controller;

namespace LongoMatch.Services.Controller
{
	[ControllerAttribute (DashboardsManagerState.NAME)]
	public class DashboardsController : TemplatesController<Dashboard, DashboardVM, DashboardButton, DashboardButtonVM>
	{
		public DashboardsController ()
		{
			TemplateName = LongoMatchCountLimitedObjects.Dashboard.ToString ();
			newTemplateCommand = new LimitationAsyncCommand<CreateEvent<Dashboard>> (TemplateName, HandleNew);
			saveStaticTemplateCommand = new LimitationAsyncCommand<Dashboard> (TemplateName, HandleSaveStatic);
			importTemplateCommand = new LimitationAsyncCommand<ImportEvent<Dashboard>> (TemplateName, Import);
			Extension = Constants.CAT_TEMPLATE_EXT;
			Provider = App.Current.CategoriesTemplatesProvider;

			FilterText = Catalog.GetString ("Dashboard files");
			AlreadyExistsText = Catalog.GetString ("A dashboard with the same name already exists.");
			ExportedCorrectlyText = Catalog.GetString ("Dashboard exported correctly");
			CountText = Catalog.GetString ("Event types:");
			NewText = Catalog.GetString ("New dashboard");
			OverwriteText = Catalog.GetString ("Do you want to overwrite it?");
			ErrorSavingText = Catalog.GetString ("Error saving dashboard");
			ConfirmDeleteText = Catalog.GetString ("Do you really want to delete the dashboard: {0}?");
			CouldNotLoadText = Catalog.GetString ("Could not load dashboard");
			AlreadyExistsText = Catalog.GetString ("A dashboard with the same name already exists");
			NotEditableText = Catalog.GetString ("System dashboards can't be edited, do you want to create a copy?");
			ConfirmSaveText = Catalog.GetString ("Do you want to save the current dashboard");
			ImportText = Catalog.GetString ("Import dashboard");
			NameText = Catalog.GetString ("Dashboard name:");
		}

		protected override bool SaveValidations (Dashboard model)
		{
			return true;
		}
	}
}

