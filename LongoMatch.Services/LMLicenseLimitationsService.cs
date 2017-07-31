//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.License;
using VAS.Core.License;
using VAS.Core.ViewModel;
using VAS.Services;

namespace LongoMatch.Services
{
	/// <summary>
	/// LongoMatch license limitation service, used to Initialize the limitations
	/// and update the limitations when a license is changed.
	/// </summary>
	public class LMLicenseLimitationsService : LicenseLimitationsService
	{
		public LMLicenseLimitationsService ()
		{
			CreateLimitations ();
		}

		protected override void UpdateFeatureLimitations ()
		{
			LMLicenseStatus status = (LMLicenseStatus)App.Current.LicenseManager.LicenseStatus;
			var databaseManagerFeature = Get<FeatureLimitationVM> (LongoMatchFeature.DatabaseManager.ToString ());
			databaseManagerFeature.Model.Enabled = status.Limitations.Contains (LongoMatchFeature.DatabaseManager.ToString ());
		}

		void CreateLimitations ()
		{
			LMLicenseStatus status = (LMLicenseStatus)App.Current.LicenseManager.LicenseStatus;
			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.DatabaseManager.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.DatabaseManager.ToString ()),
				FeatureName = "Database Manager"
			});
		}
	}
}
