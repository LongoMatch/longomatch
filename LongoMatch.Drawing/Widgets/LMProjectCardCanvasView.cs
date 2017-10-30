//
//  Copyright (C) 2017 Fluendo S.A.
//
//
using System;
using LongoMatch.Core.ViewModel;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;
using VAS.Drawing.Widgets;

namespace LongoMatch.Drawing.Widgets
{
	/// <summary>
	/// LongoMatch Project Card Canvas, renders specific content related to LongoMatch projects, like video preview background,
	/// score, team shields, team Names, etc.
	/// </summary>
	public class LMProjectCardCanvasView : CardCanvasView<LMProjectVM>
    {
		static ISurface stopwatch;
		const float ALPHA_SCORE_BACKGROUND = 0.9f;
		const int CREST_SIZE = 32;
		const int SCOREBOX_SIZE = 40;
		const int TEAM_SCORE_WIDTH = 36;
		const int TEAM_SCORE_HEIGHT = 20;
		const int TEAM_NAME_Y = 72;
		Area homeCrestArea = new Area (68, 32, CREST_SIZE, CREST_SIZE);
		Area awayCrestArea = new Area (220, 32, CREST_SIZE, CREST_SIZE);
		Area homeScoreBoxArea = new Area (116, 28, SCOREBOX_SIZE, SCOREBOX_SIZE);
		Area awayScoreBoxArea = new Area (164, 28, SCOREBOX_SIZE, SCOREBOX_SIZE);
		Area homeScoreTextArea = new Area (118, 38, TEAM_SCORE_WIDTH, TEAM_SCORE_HEIGHT);
		Area awayScoreTextArea = new Area (166, 38, TEAM_SCORE_WIDTH, TEAM_SCORE_HEIGHT);
		Area stopwatchArea = new Area (125, 168, EXTRA_INFO_ICONS_SIZE, EXTRA_INFO_ICONS_SIZE);
		Area duratonArea = new Area (145, 171, 70, 11);

		Color scoreBoxColor;

		protected override void DisposeManagedResources()
		{
			base.DisposeManagedResources();
			ViewModel = null;
		}

		static LMProjectCardCanvasView ()
		{
			stopwatch = App.Current.DrawingToolkit.CreateSurfaceFromIcon (StyleConf.StopwatchIcon);
		}

		public LMProjectCardCanvasView ()
		{
			scoreBoxColor = App.Current.Style.ThemeContrastBase;
			scoreBoxColor.SetAlpha (ALPHA_SCORE_BACKGROUND);
		}

		protected override string Title => ViewModel.Description;

		protected override string SubTitle => ViewModel.Competition;

		protected override DateTime CreationDate => ViewModel.MatchDate;

		protected override void DrawBackgroundImage()
		{
			if (ViewModel.Preview != null) {
				tk.DrawImage(cardDetailArea.Start, cardDetailArea.Width, cardDetailArea.Height,
				             ViewModel.Preview, ScaleMode.AspectFit);
			} else {
				base.DrawBackgroundImage();
			}
		}

		protected override void DrawContent()
		{
			tk.DrawImage(homeCrestArea.Start, homeCrestArea.Width, homeCrestArea.Height,
			             ViewModel.HomeTeamShield, ScaleMode.AspectFit);
			tk.DrawImage(awayCrestArea.Start, awayCrestArea.Width, awayCrestArea.Height,
			             ViewModel.AwayTeamShield, ScaleMode.AspectFit);
			tk.FillColor = scoreBoxColor;
			tk.StrokeColor = scoreBoxColor;
			tk.DrawRectangle(homeScoreBoxArea.Start, homeScoreBoxArea.Width, homeScoreBoxArea.Height);
			tk.DrawRectangle(awayScoreBoxArea.Start, awayScoreBoxArea.Width, awayScoreBoxArea.Height);
			tk.FillColor = textColor;
			tk.StrokeColor = textColor;
			tk.FontSize = 18;
			tk.FontAlignment = FontAlignment.Center;
			tk.FontWeight = FontWeight.Normal;
			tk.DrawText(homeScoreTextArea.Start, homeScoreTextArea.Width, homeScoreTextArea.Height, ViewModel.LocalScore);
			tk.DrawText(awayScoreTextArea.Start, awayScoreTextArea.Width, awayScoreTextArea.Height, ViewModel.AwayScore);
			int teamTextWidth = 0;
			int teamTextHeight = 0;
			tk.FontFamily = "Roboto";
			tk.FontSize = 10;
			tk.MeasureText (ViewModel.HomeTeamText, out teamTextWidth, out teamTextHeight, "Roboto", 10, FontWeight.Light);
			var position = new Point (0, 0);
			position.X = homeCrestArea.Start.X + (homeCrestArea.Width / 2) - (teamTextWidth / 2);
			position.Y = TEAM_NAME_Y;
			tk.DrawText (position, teamTextWidth, teamTextHeight, ViewModel.HomeTeamText);
			tk.MeasureText (ViewModel.AwayTeamText, out teamTextWidth, out teamTextHeight, "Roboto", 10, FontWeight.Light);
			position.X = awayCrestArea.Start.X + (awayCrestArea.Width / 2) - (teamTextWidth / 2);
			tk.DrawText (position, teamTextWidth, teamTextHeight, ViewModel.AwayTeamText);
		}

		protected override void DrawExtraInformation ()
		{
			base.DrawExtraInformation ();
			if (ViewModel.Duration != null) {
				tk.DrawSurface (stopwatchArea.Start, stopwatchArea.Width, stopwatchArea.Height,
								stopwatch, ScaleMode.AspectFit, true);
				tk.DrawText (duratonArea.Start, duratonArea.Width, duratonArea.Height, ViewModel.Duration.ToMSecondsString (true));
			}
		}
	}
}
