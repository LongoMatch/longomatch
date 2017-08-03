//
//  Copyright (C) 2017 Fluendo S.A.
using System;
using System.Linq;
using LongoMatch.Core;
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

			string databaseLimitation = LongoMatchFeature.DatabaseManager.ToString ();
			var databaseManagerFeature = Get<FeatureLimitationVM> (databaseLimitation);
			databaseManagerFeature.Model.Enabled = status.Limitations.Contains (databaseLimitation);

			string conversionLimitation = LongoMatchFeature.VideoConverter.ToString ();
			var converterFeature = Get<FeatureLimitationVM> (conversionLimitation);
			converterFeature.Model.Enabled = status.Limitations.Contains (conversionLimitation);

			string excelExportLimitation = LongoMatchFeature.ExcelExport.ToString ();
			var excelExportFeature = Get<FeatureLimitationVM> (excelExportLimitation);
			excelExportFeature.Model.Enabled = status.Limitations.Contains (excelExportLimitation);

			string xmlLimitation = LongoMatchFeature.XMlImportExport.ToString ();
			var xmlFeature = Get<FeatureLimitationVM> (xmlLimitation);
			xmlFeature.Model.Enabled = status.Limitations.Contains (xmlLimitation);
		}

		void CreateLimitations ()
		{
			LMLicenseStatus status = (LMLicenseStatus)App.Current.LicenseManager.LicenseStatus;

			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.DatabaseManager.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.DatabaseManager.ToString ()),
				FeatureName = Catalog.GetString ("Database Manager")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.VideoConverter.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.VideoConverter.ToString ()),
				FeatureName = Catalog.GetString ("Video Converter")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.ExcelExport.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.ExcelExport.ToString ()),
				FeatureName = "Excel Export"
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.XMlImportExport.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.XMlImportExport.ToString ()),
				FeatureName = "XML Import/Export"
			});
		}
	}
}
