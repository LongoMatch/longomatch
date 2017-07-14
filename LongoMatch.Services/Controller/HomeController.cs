//
//  Copyright (C) 2017 FLUENDO S.A.
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
using System.Threading.Tasks;
using LongoMatch.Core.Common;
using LongoMatch.License;
using LongoMatch.Services.States;
using LongoMatch.Services.ViewModel;
using VAS.Core.Events;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
namespace LongoMatch.Services.Controller
{
	[Controller (HomeState.NAME)]
	public class HomeController : ControllerBase
	{
		HomeViewModel vm;

		public override void SetViewModel (IViewModel viewModel)
		{
			vm = (HomeViewModel)viewModel;
		}

		public override async Task Start ()
		{
			await base.Start ();
			HandleLicenseChange (null);
			App.Current.EventsBroker.Subscribe<LicenseChangeEvent> (HandleLicenseChange);

		}

		public override async Task Stop ()
		{
			await base.Stop ();
			App.Current.EventsBroker.Unsubscribe<LicenseChangeEvent> (HandleLicenseChange);
		}

		void HandleLicenseChange (LicenseChangeEvent e)
		{
			string iconName = string.Empty;
			switch (((LMLicenseStatus)App.Current.LicenseManager.LicenseStatus).LicenseType) {
			case LMLicenseType.BASIC:
				iconName = Constants.LOGO_BASIC_ICON;
				break;
			case LMLicenseType.STARTER:
				iconName = Constants.LOGO_STARTER_ICON;
				break;

			case LMLicenseType.PRO:
				iconName = Constants.LOGO_PRO_ICON;
				break;

			default:
				iconName = Constants.LOGO_ICON;
				break;
			}

			vm.LogoIcon = App.Current.ResourcesLocator.LoadIcon (iconName);
			App.Current.SoftwareIconName = iconName;
		}
	}
}
