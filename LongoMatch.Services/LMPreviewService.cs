//
//  Copyright (C) 2017 FLuendo
//
//
using System;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.Widgets;
using VAS.Core.Common;
using VAS.Core.Interfaces;
using VAS.Core.Store.Templates;
using VAS.Services;
using Sizes = LongoMatch.Core.Resources.Styles.Sizes;

namespace LongoMatch.Services
{
	/// <summary>
	/// Service 
	/// </summary>
	public sealed class LMPreviewService : PreviewService
	{
		/// <summary>
		/// Generates the team preview.
		/// </summary>
		/// <returns>The team preview.</returns>
		/// <param name="team">Team.</param>
		protected override Image CreatePreview (Team team)
		{
			// load viewmodel and create the view to extract the image 
			LMTeamTaggerVM taggerVM = new LMTeamTaggerVM { HomeTeam = new LMTeamVM { Model = team as LMTeam } };
			taggerVM.AwayTeam = null;
			taggerVM.Background = App.Current.HHalfFieldBackground;

			LMTeamTaggerView taggerView = new LMTeamTaggerView { BackgroundColor = App.Current.Style.ColorBackgroundPreview };
			double width = App.Current.HHalfFieldBackground.Width + Sizes.BenchSize;
			double height = App.Current.HHalfFieldBackground.Height;
			CreateInternalPreview (taggerView, taggerVM, width, height);

			return App.Current.DrawingToolkit.Copy (taggerView, new Area (new Point (0, 0), width, height));
		}
	}
}
