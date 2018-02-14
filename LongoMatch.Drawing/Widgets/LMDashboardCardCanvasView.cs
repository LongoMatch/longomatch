//
//  Copyright (C) 2018 Fluendo S.A.
//
//
using System;
using LongoMatch.Core.Resources;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.ViewModel;
using VAS.Drawing.Widgets;

namespace LongoMatch.Drawing.Widgets
{
	public class LMDashboardCardCanvasView : CardCanvasView<DashboardVM>
	{

		protected override string Title => ViewModel.Name;

		protected override string SubTitle => $"{ViewModel.ViewModels.Count} {Strings.EventButtons}";

		protected override DateTime CreationDate => ViewModel.Model.CreationDate;

		protected override void DrawBackgroundImage ()
		{
			if (ViewModel.Preview != null) {
				tk.DrawImage (cardDetailArea.Start, cardDetailArea.Width, cardDetailArea.Height,
				ViewModel.Preview, ScaleMode.AspectFit);
			} else {
				base.DrawBackgroundImage ();
			}
		}

		protected override void DrawContent ()
		{
		}
	}
}
