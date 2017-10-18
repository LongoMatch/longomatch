//
//  Copyright (C) 2017 Fluendo S.A.
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services.State;
using VAS.Services.ViewModel;
using LMConstants = LongoMatch.Core.Common.Constants;

namespace LongoMatch.Services.State
{
	/// <summary>
	/// LongoMatch ugrade limitation state. Used to configure the
	/// UpgradeLimitationVM with the corresponding LongoMatch requirements.
	/// </summary>
	public class LMUpgradeLimitationState : UpgradeLimitationState
	{
		protected override void CreateViewModel (dynamic data)
		{
			ViewModel = new UpgradeLimitationVM ();
			LimitationVM limitation = (LimitationVM)data.limitationVM;
			if (data.limitationVM is FeatureLimitationVM) {
				FeatureLimitationVM featureLimitation = (FeatureLimitationVM)limitation;
				string featureMessage = (string.IsNullOrEmpty (featureLimitation.DetailInfo)) ?
					featureLimitation.DisplayName : featureLimitation.DetailInfo;
				if (!string.IsNullOrEmpty (featureLimitation.DetailInfo)) {
					ViewModel.Header = featureLimitation.DetailInfo;
				} else {
					ViewModel.Header = Catalog.GetString (
						$"The {featureLimitation.DisplayName} is not available in the {App.Current.LicenseManager.LicenseStatus.PlanName} plan");
				}
			} else if (limitation is CountLimitationVM) {
				ViewModel.Header = Catalog.GetString (
					$"You have reached the limit of {limitation.DisplayName} available for your plan");
			} else {
				ViewModel.Header = Catalog.GetString ("Unlock your team's potential with LongoMatch PRO");
			}
			if (limitation != null) {
				ViewModel.FeaturesHeader = Catalog.GetString ("Upgrade to LongoMatch PRO and unlock your team's potential");
			} else {
				ViewModel.FeaturesHeader = Catalog.GetString ("Upgrade to get access to the following features");
			}
			ViewModel.Features = new RangeObservableCollection<string> {
				Catalog.GetString("Unlimited projects, dashboards and teams"),
				Catalog.GetString("4x Zoom-in factor"),
				Catalog.GetString("Multicamera Analysis"),
				Catalog.GetString("SportsCode & XML Import and Export")
			};
			ViewModel.FeaturesCaption = Catalog.GetString ("... and much more");
			ViewModel.UpgradeCommand = new Command (() => {
				Utils.OpenURL (LMConstants.UPGRADE_URL, $"Limitation_{limitation.RegisterName.Replace (" ", string.Empty)}");
				App.Current.EventsBroker.Publish (new UpgradeLinkClickedEvent {
					LimitationName = limitation.RegisterName,
					Source = "UpgradeDialog"
				});
			});
			ViewModel.UpgradeCommand.Text = Catalog.GetString ("UPGRADE TO PRO");
			ViewModel.Undecided = Catalog.GetString ("Still undecided?");
			ViewModel.OtherPlansURL = LMConstants.OTHER_PLANS_URL;
		}
	}
}
