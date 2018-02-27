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
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Core.Resources.Styles;
using System.Collections.Generic;
using VAS.Core;
using System;
using VAS.Core.Resources;
using LMIcons = LongoMatch.Core.Resources.Icons;
using System.Threading.Tasks;
using System.Dynamic;
using LongoMatch.Services.State;

namespace LongoMatch.Services.ViewModel
{
	public class DashboardsManagerVM : TemplatesManagerViewModel<Dashboard, DashboardVM, DashboardButton, DashboardButtonVM>, IDashboardDealer
	{
		CountLimitationBarChartVM chartVM;

		public DashboardsManagerVM ()
		{
			AddButton = LoadedItem.AddButton;
			NewCommand.Icon = App.Current.ResourcesLocator.LoadIcon (Icons.Add, Sizes.TemplatesIconSize);
			NewCommand.IconName = Icons.Add;
			SaveCommand.Icon = App.Current.ResourcesLocator.LoadIcon (Icons.Save, Sizes.TemplatesIconSize);
			DeleteCommand.Icon = App.Current.ResourcesLocator.LoadIcon (Icons.Delete, Sizes.TemplatesIconSize);
			DeleteCommand.IconName = Icons.Delete;
			ExportCommand.Icon = App.Current.ResourcesLocator.LoadIcon (LMIcons.Export, Sizes.TemplatesIconSize);
			ImportCommand.Icon = App.Current.ResourcesLocator.LoadIcon (Icons.Import, Sizes.TemplatesIconSize);
			TransferCommand = new Command (() => throw new NotImplementedException ()) {
				Icon = App.Current.ResourcesLocator.LoadIcon (LMIcons.Transfer, Sizes.TemplatesIconSize),
				IconName = LMIcons.Transfer
			};
			MakeDefaultCommand = new Command (() => throw new NotImplementedException ()) {
				Icon = App.Current.ResourcesLocator.LoadIcon (LMIcons.Select, Sizes.TemplatesIconSize),
				IconName = LMIcons.Select
			};
			ShowDetailsCommand = new AsyncCommand<DashboardVM> (ShowDetails, (vm) => vm != null);

			if (LimitationChart != null) {
				LimitationChart.Dispose ();
				LimitationChart = null;
			}
		}

		public Command<string> AddButton { get; private set; }

		/// <summary>
		/// Calls the transfer service holding the current Dashboard.
		/// </summary>
		/// <value>The transfer command.</value>
		public Command TransferCommand { get; private set; }

		/// <summary>
		/// Marks dashboard as default
		/// </summary>
		/// <value>The make default command.</value>
		public Command MakeDefaultCommand { get; private set; }

		public DashboardVM Dashboard => LoadedItem;

		/// <summary>
		/// ViewModel for the Bar chart used to display count limitations in the Limitation Widget
		/// </summary>
		public CountLimitationBarChartVM LimitationChart {
			get { return chartVM; }
			set {
				chartVM = value;
				Limitation = chartVM?.Limitation;
			}
		}

		protected override MenuVM CreateMenu (IViewModel vm)
		{
			DeleteCommand.IconName = Icons.Delete;
			MenuVM menu = new MenuVM ();
			menu.ViewModels.AddRange (new List<MenuNodeVM> {
				new MenuNodeVM (DeleteCommand, vm, Strings.Delete) { ActiveColor = App.Current.Style.ColorAccentError },
			});

			return menu;
		}

		protected override DashboardVM CreateInstance (Dashboard model)
		{
			var vm = base.CreateInstance (model);
			if (model.Static) {
				StaticViewModels.Add (vm);
			}

			return vm;
		}

		public async Task ShowDetails (DashboardVM viewModel)
		{
			this.LoadedItem = viewModel;
			dynamic properties = new ExpandoObject ();
			properties.viewModel = this;

			await App.Current.StateController.MoveTo (DashboardDetailsState.NAME, properties, false);
		}
	}
}