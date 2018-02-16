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
using System.Linq;
using LongoMatch.Core.ViewModel;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Core.MVVMC;
using VAS.Core.Resources.Styles;
using VAS.Drawing.CanvasObjects.Timeline;
using VAS.Drawing.Widgets;
using VUtils = VAS.Drawing.Utils;


namespace LongoMatch.Drawing.Widgets
{
	[View ("TimelineLabelsView")]
	public class LMTimelineLabels : TimelineLabels, ICanvasView<LMProjectAnalysisVM>
	{
		public LMTimelineLabels (IWidget widget) : base (widget)
		{
			labelWidth = Sizes.TimelineLabelsWidth;
			labelHeight = Sizes.TimelineSelectionLeftHeight;
		}

		public LMTimelineLabels () : this (null)
		{
		}

		public new LMProjectAnalysisVM ViewModel {
			get {
				return base.ViewModel as LMProjectAnalysisVM;
			}
			set {
				int i = 0;
				base.ViewModel = value;
				ClearObjects ();
				if (base.ViewModel != null) {
					FillCanvas (ref i);
				}
			}
		}

		public override void SetViewModel (object viewModel)
		{
			ViewModel = (LMProjectAnalysisVM)viewModel;
		}

		protected override void FillCanvas (ref int i)
		{
			LabelView label;

			label = new LabelView {
				Height = labelHeight,
				OffsetY = i * labelHeight,
				BackgroundColor = VUtils.ColorForRow (i),
				Name = Catalog.GetString ("Periods"),
			};
			AddLabel (label, null);
			i++;

			base.FillCanvas (ref i);
		}
	}
}
