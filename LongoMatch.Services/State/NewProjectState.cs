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
using System.Threading.Tasks;
using LongoMatch.Core.Store;
using LongoMatch.Services.ViewModel;
using VAS.Core;
using VAS.Core.Interfaces.MVVMC;
using VAS.Services.State;

namespace LongoMatch.Services.State
{
	public class NewProjectState : ScreenState<NewProjectPanelVM>
	{
		public const string NAME = "NewProject";

		public override string Name {
			get {
				return NAME;
			}
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new NewProjectPanelVM ();
			if (data != null) {
				ViewModel.Model = data;
			}
		}

		public override async Task<bool> LoadState (dynamic data)
		{
			Initialize (data);
			if (!await App.Current.Device.CheckExternalStoragePermission ()) {
				var warningMessage = string.Format (Catalog.GetString ("{0} can't create new projects without permissions"),
												   App.Current.SoftwareName);
				App.Current.Dialogs.WarningMessage (warningMessage);
				return false;
			}
			await App.Current.Device.CheckCapturePermissions ();
			return true;
		}
	}
}

