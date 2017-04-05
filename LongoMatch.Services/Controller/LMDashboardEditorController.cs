//
//  Copyright (C) 2017 Fluendo S.A.
using LongoMatch.Core.Store;
using LongoMatch.Services.State;
using LongoMatch.Services.States;
using VAS.Core.Common;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Services.Controller;

namespace LongoMatch.Services.Controller
{
	[Controller (DashboardsManagerState.NAME)]
	[Controller (ProjectAnalysisState.NAME)]
	public class LMDashboardEditorController : DashboardEditorController
	{
		protected override DashboardButton CreateButton (string buttonType)
		{
			DashboardButton button = null;
			if (buttonType == "Card") {
				button = new PenaltyCardButton {
					PenaltyCard = new PenaltyCard ("Red", Color.Red, CardShape.Rectangle)
				};
			} else if (buttonType == "Score") {
				button = new ScoreButton {
					Score = new Score ("Score", 1)
				};
			} else if (buttonType == "Timer") {
				button = new TimerButton { Timer = new LMTimer { Name = "Timer" } };
			} else {
				button = base.CreateButton (buttonType);
			}
			return button;
		}
	}
}
