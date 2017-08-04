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
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;

namespace LongoMatch.Services.ViewModel
{
	public class DashboardsManagerVM : TemplatesManagerViewModel<Dashboard, LMDashboardVM, DashboardButton, DashboardButtonVM>, IDashboardDealer
	{
		public DashboardsManagerVM ()
		{
			AddButton = LoadedTemplate.AddButton;
			NewCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-add", StyleConf.TemplatesIconSize);
			SaveCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-save", StyleConf.TemplatesIconSize);
			DeleteCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-delete", StyleConf.TemplatesIconSize);
			ExportCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("lm-export", StyleConf.TemplatesIconSize);
			ImportCommand.Icon = App.Current.ResourcesLocator.LoadIcon ("vas-import", StyleConf.TemplatesIconSize);
			if (LimitationChart != null) {
				LimitationChart.Dispose ();
				LimitationChart = null;
			}
		}

		public Command<string> AddButton {
			get;
			private set;
		}

		public DashboardVM Dashboard {
			get {
				return LoadedTemplate;
			}
		}

		/// <summary>
		/// ViewModel for the Bar chart used to display count limitations in the Limitation Widget
		/// </summary>
		public CountLimitationBarChartVM LimitationChart {
			get;
			set;
		}
	}
}