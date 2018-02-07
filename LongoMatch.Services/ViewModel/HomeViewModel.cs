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
using LongoMatch.Core.Resources;
using LongoMatch.Core.Resources.Styles;
using LongoMatch.Services.State;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VASStrings = VAS.Core.Resources.Strings;

namespace LongoMatch.Services.ViewModel
{
	public class HomeViewModel : ViewModelBase
	{
		public HomeViewModel ()
		{
			LicenseBanner = new LicenseBannerVM ();
			PreferencesCommand = new Command (HandlePreferencesCommand) {
				ToolTipText = VASStrings.Preferences,
				Icon = App.Current.ResourcesLocator.LoadIcon ("lm-preferences", Sizes.WelcomeBarIconSize)
			};
		}

		public Image LogoIcon {
			get;
			set;
		}

		public LicenseBannerVM LicenseBanner {
			get;
			set;
		}

		public Command PreferencesCommand {
			get;
			set;
		}

		void HandlePreferencesCommand ()
		{
			App.Current.StateController.MoveTo (PreferencesState.NAME, null);
		}
	}
}
