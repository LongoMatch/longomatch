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
using System.Threading.Tasks;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Services.ViewModel;
using VAS.Core.Common;
using VAS.Services.State;

namespace LongoMatch.Services.States
{
	public class TeamsManagerState : ScreenState<TeamsManagerVM>
	{
		public const string NAME = "TeamsManager";

		public override string Name {
			get {
				return NAME;
			}
		}

		public override Task<bool> ShowState ()
		{
			ViewModel.Save (false);
			return base.ShowState ();
		}

		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new TeamsManagerVM ();
			ViewModel.Model = new RangeObservableCollection<LMTeam> (App.Current.TeamTemplatesProvider.Templates);
		}
	}
}

