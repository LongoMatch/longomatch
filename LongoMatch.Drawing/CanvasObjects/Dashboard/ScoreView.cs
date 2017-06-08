//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.MVVMC;
using VAS.Drawing.CanvasObjects.Dashboard;

namespace LongoMatch.Drawing.CanvasObjects.Dashboard
{
	[View ("ScoreButtonView")]
	public class ScoreView : TimedTaggerButtonView, IView<ScoreButtonVM>
	{
		static Image iconImage;

		static ScoreView ()
		{
			iconImage = App.Current.ResourcesLocator.LoadImage (StyleConf.ButtonScoreIcon);
		}

		public ScoreButtonVM ViewModel {
			get {
				return TimedButtonVM as ScoreButtonVM;
			}
			set {
				TimedButtonVM = value;
			}
		}

		public override Image Icon {
			get {
				return iconImage;
			}
		}

		public override string Text {
			get {
				if (Recording) {
					return (ViewModel.CurrentTime - Start).ToSecondsString ();
				} else {
					return ViewModel.Name;
				}
			}
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (ScoreButtonVM)viewModel;
		}
	}
}

