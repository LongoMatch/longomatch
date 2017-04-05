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
using VAS.Core.Store;
using VAS.Core.Store.Templates;
using VAS.Core.ViewModel;
using VAS.Services.ViewModel;
using VAS.Core.MVVMC;
using VAS.Core;
using VAS.Core.Common;

namespace LongoMatch.Services.ViewModel
{
	public class DashboardsManagerVM : TemplatesManagerViewModel<Dashboard, LMDashboardVM, DashboardButton, DashboardButtonVM>
	{

		public DashboardsManagerVM ()
		{
			AddButton = LoadedTemplate.AddButton;
			NewCommand.Icon = Resources.LoadIcon ("longomatch-add", StyleConf.TemplatesIconSize);
			SaveCommand.Icon = Resources.LoadIcon ("longomatch-save", StyleConf.TemplatesIconSize);
			DeleteCommand.Icon = Resources.LoadIcon ("longomatch-delete", StyleConf.TemplatesIconSize);
			ExportCommand.Icon = Resources.LoadIcon ("longomatch-export", StyleConf.TemplatesIconSize);
			ImportCommand.Icon = Resources.LoadIcon ("longomatch-import", StyleConf.TemplatesIconSize);
		}

		public static implicit operator DashboardVM (DashboardsManagerVM viewModel)
		{
			return viewModel?.LoadedTemplate;
		}

		public Command<string> AddButton {
			get;
			private set;
		}
	}
}