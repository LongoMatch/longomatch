//
//  Copyright (C) 2017 Fluendo S.A.
//
//
using System;
using LongoMatch.Core.ViewModel;
using LongoMatch.Core.Resources;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing.Widgets;

namespace LongoMatch.Drawing.Widgets
{
	/// <summary>
	/// LongoMatch Project Card Canvas, renders specific content related to LongoMatch projects, like video preview background,
	/// score, team shields, team Names, etc.
	/// </summary>
	public class LMTeamCardCanvasView : CardCanvasView<LMTeamVM>
    {
		static ISurface formation;
		const int CREST_SIZE = 64, COLOR_SIZE = 24;

		Area durationArea = new Area (145, 171, 70, 11);
		Area crestArea = new Area (128, 28, CREST_SIZE, CREST_SIZE);
		Area teamNameArea = new Area (5, 100, 310, 20);
		Area formationArea = new Area (125, 168, EXTRA_INFO_ICONS_SIZE, EXTRA_INFO_ICONS_SIZE);
		Area colorArea1 = new Area (132, 129, COLOR_SIZE, COLOR_SIZE);
		Area colorArea2 = new Area (164, 129, COLOR_SIZE, COLOR_SIZE);

		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			ViewModel = null;
		}

		static LMTeamCardCanvasView ()
		{
			formation = App.Current.DrawingToolkit.CreateSurfaceFromIcon (Icons.Formation);
		}

		protected override string Title => String.Empty;

		protected override string SubTitle => String.Empty;

		protected override DateTime CreationDate => ViewModel.Model.CreationDate;

		protected override void DrawBackgroundImage()
		{
			if (ViewModel.Preview != null) {
				tk.DrawImage(cardDetailArea.Start, cardDetailArea.Width, cardDetailArea.Height,
				             ViewModel.Preview, ScaleMode.AspectFill);
			} else {
				base.DrawBackgroundImage();
			}
		}

		protected override void DrawContent()
		{
		    tk.DrawImage (crestArea.Start, crestArea.Width, crestArea.Height,
						 ViewModel.Icon, ScaleMode.AspectFit);
			
			tk.FillColor = textColor;
			tk.StrokeColor = textColor;
			tk.FontSize = 18;
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Normal;
			tk.DrawText (teamNameArea.Start, teamNameArea.Width, teamNameArea.Height, ViewModel.Name);


			tk.LineWidth = 0;
			tk.FillColor = ViewModel.TypedModel.Colors[0];
			tk.DrawRoundedRectangle (colorArea1.Start, colorArea1.Width, colorArea1.Height, CARD_ROUND_RADIUS);
			tk.FillColor = ViewModel.TypedModel.Colors [1];
			tk.DrawRoundedRectangle (colorArea2.Start, colorArea2.Width, colorArea2.Height, CARD_ROUND_RADIUS);
		}

		protected override void DrawExtraInformation ()
		{
			base.DrawExtraInformation ();
			if (ViewModel.Formation != null) {
				tk.DrawSurface (formationArea.Start, formationArea.Width, formationArea.Height,
				                formation, ScaleMode.AspectFit, true);
				tk.DrawText (durationArea.Start, durationArea.Width, durationArea.Height, ViewModel.TypedModel.FormationStr);
			}
		}
	}
}
