//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using System.Collections.Generic;
using LongoMatch.Core.Handlers;
using LongoMatch.Drawing.Widgets;
using VAS.Core.Common;
using VAS.Core.Interfaces.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing.Cairo;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class CoordinatesTagger : Gtk.Bin, IView<ProjectVM>
	{
		public event ShowTaggerMenuHandler ShowMenuEvent;

		ProjectVM viewModel;

		public CoordinatesTagger ()
		{
			this.Build ();
			Tagger = new ProjectLocationsTaggerView (new WidgetWrapper (drawingarea));
			Tagger.ShowMenuEvent += HandleShowMenuEvent;
		}

		protected override void OnDestroyed ()
		{
			Tagger.Dispose ();
			base.OnDestroyed ();
		}

		public FieldPositionType FieldPosition {
			get {
				return Tagger.FieldPosition;
			}
			set {
				Tagger.FieldPosition = value;
			}
		}

		public ProjectVM ViewModel {
			get {
				return viewModel;
			}
			set {
				viewModel = value;
				if (viewModel != null) {
					Tagger.Background = ViewModel.Model.GetBackground (FieldPosition);
				}
				Tagger.SetViewModel (ViewModel);
			}
		}

		public ProjectLocationsTaggerView Tagger {
			get;
			set;
		}

		public void SetViewModel (object viewModel)
		{
			ViewModel = (ProjectVM)viewModel;
		}

		void HandleShowMenuEvent (IEnumerable<TimelineEvent> plays)
		{
			if (ShowMenuEvent != null) {
				ShowMenuEvent (plays);
			}
		}
	}
}

