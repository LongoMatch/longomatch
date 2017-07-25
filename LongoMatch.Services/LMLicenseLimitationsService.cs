//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using LongoMatch.Core.Common;
using LongoMatch.License;
using VAS.Core.License;
using VAS.Core.ViewModel;
using VAS.Services;

namespace LongoMatch.Services
{
	public class LMLicenseLimitationsService : LicenseLimitationsService
	{
		public LMLicenseLimitationsService ()
		{
			CreateLimitations ();
		}

		protected override void UpdateFeatureLimitations ()
		{
			LMLicenseStatus status = (LMLicenseStatus)App.Current.LicenseManager.LicenseStatus;
			var databaseManagerFeature = Get<FeatureLimitationVM> (Constants.DATABASE_MANAGER_FEATURE);
			databaseManagerFeature.Model.Enabled = status.DatabaseManagerLimited;
		}

		void CreateLimitations ()
		{
			LMLicenseStatus status = (LMLicenseStatus)App.Current.LicenseManager.LicenseStatus;
			Add (new LicenseLimitation {
				Name = Constants.DATABASE_MANAGER_FEATURE,
				Enabled = status.DatabaseManagerLimited
			});
		}
	}
}
