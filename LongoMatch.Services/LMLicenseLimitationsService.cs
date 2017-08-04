//
//  Copyright (C) 2017 Fluendo S.A.
using System.Diagnostics;
using System.Linq;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.License;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.License;
using VAS.Core.MVVMC;
using VAS.Core.ViewModel;
using VAS.Services;
using Utils = VAS.Core.Common.Utils;

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

		public override bool Start ()
		{
			App.Current.EventsBroker.Subscribe<StorageAddedEvent<LMProject>> (HandleProjectCreated);
			App.Current.EventsBroker.Subscribe<StorageDeletedEvent<LMProject>> (HandleProjectDeleted);
			UpdateLicenseLimitationsCounters ();
			return base.Start ();
		}

		public override bool Stop ()
		{
			App.Current.EventsBroker.Unsubscribe<StorageAddedEvent<LMProject>> (HandleProjectCreated);
			App.Current.EventsBroker.Unsubscribe<StorageDeletedEvent<LMProject>> (HandleProjectDeleted);
			return base.Stop ();
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
			
            string zoomLimitation = VASFeature.Zoom.ToString ();
			var zoomFeature = Get<FeatureLimitationVM> (zoomLimitation);
			zoomFeature.Model.Enabled = status.Limitations.Contains (zoomLimitation);
			
			string openMultiCameraLimitation = VASFeature.OpenMultiCamera.ToString ();
			var openMultiCameraFeature = Get<FeatureLimitationVM> (openMultiCameraLimitation);
			openMultiCameraFeature.Model.Enabled = status.Limitations.Contains (openMultiCameraLimitation);

			string createMultiCameraLimitation = VASFeature.CreateMultiCamera.ToString ();
			var createMultiCameraFeature = Get<FeatureLimitationVM> (createMultiCameraLimitation);
			createMultiCameraFeature.Model.Enabled = status.Limitations.Contains (createMultiCameraLimitation);
		}

		void UpdateLicenseLimitationsCounters ()
		{
			int count = App.Current.DatabaseManager.ActiveDB.Count<LMProject> ();
			Get<CountLimitationVM> ("Projects").Count = count;
		}

		void CreateLimitations ()
		{
			LMLicenseStatus status = (LMLicenseStatus)App.Current.LicenseManager.LicenseStatus;

			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.DatabaseManager.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.DatabaseManager.ToString ()),
				DisplayName = Catalog.GetString ("Database Manager")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.VideoConverter.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.VideoConverter.ToString ()),
				DisplayName = Catalog.GetString ("Video Converter")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.ExcelExport.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.ExcelExport.ToString ()),
				DisplayName = Catalog.GetString ("Excel Export")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.XMlImportExport.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.XMlImportExport.ToString ()),
				DisplayName = Catalog.GetString ("XML Import/Export")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = VASFeature.Zoom.ToString (),
				Enabled = status.Limitations.Contains (VASFeature.Zoom.ToString ()),
				DisplayName = Catalog.GetString ("Zoom")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = VASFeature.OpenMultiCamera.ToString (),
				Enabled = status.Limitations.Contains (VASFeature.OpenMultiCamera.ToString ()),
				DisplayName = Catalog.GetString ("Multi-Camera"),
				DetailInfo = Catalog.GetString ("The project you are trying to use uses the Multi-Camera feature, which is not available in the " +
											   status.PlanName + " plan")
			});
			Add (new FeatureLicenseLimitation {
				RegisterName = VASFeature.CreateMultiCamera.ToString (),
				Enabled = status.Limitations.Contains (VASFeature.CreateMultiCamera.ToString ()),
				DisplayName = Catalog.GetString ("Multi-Camera")
			});
			Add (new CountLicenseLimitation {
				RegisterName = LongoMatchCountLimitedObjects.Projects.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchCountLimitedObjects.Projects.ToString ()),
				DisplayName = Catalog.GetString ("Projects"),
				Maximum = 5,
			}, new Command (() => Utils.OpenURL (Constants.WEBSITE, "Limitation_Projects")));
		}

		void HandleProjectCreated (StorageAddedEvent<LMProject> obj)
		{
			var limit = Get<CountLimitationVM> ("Projects");
			limit.Count++;
		}

		void HandleProjectDeleted (StorageDeletedEvent<LMProject> obj)
		{
			var limit = Get<CountLimitationVM> ("Projects");
			limit.Count--;
		}
	}
}
