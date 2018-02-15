//
//  Copyright (C) 2018 
using LongoMatch;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.ViewModel;
using LongoMatch.Drawing.Widgets;
using LongoMatch.Services;
using Moq;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Interfaces.Drawing;

namespace Tests.Services
{
	[TestFixture]
	public class TestLMPreviewService
	{
		Mock<IDrawingToolkit> mockDrawingToolkit;
		LMPreviewService previewService;
		
		[SetUp]
		public void Setup () 
		{
			mockDrawingToolkit = new Mock<IDrawingToolkit> ();
			App.Current.DrawingToolkit = mockDrawingToolkit.Object;
			previewService = new LMPreviewService ();
			App.Current.PreviewService = previewService;
		}

		[Test]
		public void CreateTeamPreview_ModelAvailable_ViewWellFilledForPreview ()
		{
			// Arrange
			LMTeam team = LMTeam.DefaultTemplate (6);
			team.FormationStr = "1-2-1";

			mockDrawingToolkit.Setup (d => d.Copy (It.IsAny<ICanvas> (), It.IsAny<Area> ())).Returns (new Image (1, 1));

			// Act
			Image result = previewService.CreatePreview (team);

			// Assert
			Assert.IsNotNull (result);
			mockDrawingToolkit.Verify (d => d.Copy (It.Is<LMTeamTaggerView> (view => ValidateTaggerView (view)), It.IsAny<Area> ()), Times.Exactly (2));
		}

		bool ValidateTaggerView (LMTeamTaggerView view) {
			LMTeamTaggerVM vm = view.ViewModel;

			if (vm.AwayTeam != null) return false;
			if (vm.Background.Width != App.Current.HHalfFieldBackground.Width) return false;
			if (vm.Background.Height != App.Current.HHalfFieldBackground.Height) return false;

			return true;
		}
	}
}
