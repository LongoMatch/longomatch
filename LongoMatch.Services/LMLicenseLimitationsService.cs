//
//  Copyright (C) 2017 Fluendo S.A.
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
			var databaseManagerFeature = Get<LimitationVM> (databaseLimitation);
			databaseManagerFeature.Model.Enabled = status.Limitations.Contains (databaseLimitation);

			string conversionLimitation = LongoMatchFeature.VideoConverter.ToString ();
			var converterFeature = Get<LimitationVM> (conversionLimitation);
			converterFeature.Model.Enabled = status.Limitations.Contains (conversionLimitation);

			string excelExportLimitation = LongoMatchFeature.ExcelExport.ToString ();
			var excelExportFeature = Get<LimitationVM> (excelExportLimitation);
			excelExportFeature.Model.Enabled = status.Limitations.Contains (excelExportLimitation);

			string xmlLimitation = LongoMatchFeature.XMlImportExport.ToString ();
			var xmlFeature = Get<LimitationVM> (xmlLimitation);
			xmlFeature.Model.Enabled = status.Limitations.Contains (xmlLimitation);

			string zoomLimitation = VASFeature.Zoom.ToString ();
			var zoomFeature = Get<LimitationVM> (zoomLimitation);
			zoomFeature.Model.Enabled = status.Limitations.Contains (zoomLimitation);

			string openMultiCameraLimitation = VASFeature.OpenMultiCamera.ToString ();
			var openMultiCameraFeature = Get<LimitationVM> (openMultiCameraLimitation);
			openMultiCameraFeature.Model.Enabled = status.Limitations.Contains (openMultiCameraLimitation);

			string createMultiCameraLimitation = VASFeature.CreateMultiCamera.ToString ();
			var createMultiCameraFeature = Get<LimitationVM> (createMultiCameraLimitation);
			createMultiCameraFeature.Model.Enabled = status.Limitations.Contains (createMultiCameraLimitation);

			string projectLimitation = LongoMatchCountLimitedObjects.Projects.ToString ();
			var projectFeature = Get<LimitationVM> (projectLimitation);
			projectFeature.Model.Enabled = status.Limitations.Contains (projectLimitation);

			string linkingLimitation = VASFeature.LinkingButtons.ToString ();
			var linkingFeature = Get<FeatureLimitationVM> (linkingLimitation);
			linkingFeature.Model.Enabled = status.Limitations.Contains (linkingLimitation);

			string ipCameraLimitation = LongoMatchFeature.IpCameras.ToString ();
			var ipCameraFeature = Get<FeatureLimitationVM> (ipCameraLimitation);
			ipCameraFeature.Model.Enabled = status.Limitations.Contains (ipCameraLimitation);
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

			Add (new FeatureLicenseLimitation {
				RegisterName = VASFeature.LinkingButtons.ToString (),
				Enabled = status.Limitations.Contains (VASFeature.LinkingButtons.ToString ()),
				DisplayName = Catalog.GetString ("Linking Buttons")
			});

			Add (new CountLicenseLimitation {
				RegisterName = LongoMatchCountLimitedObjects.Projects.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchCountLimitedObjects.Projects.ToString ()),
				DisplayName = Catalog.GetString ("Projects"),
				Maximum = 5,
			}, new Command (() => Utils.OpenURL (Core.Common.Constants.WEBSITE, "Limitation_Projects")));

			Add (new FeatureLicenseLimitation {
				RegisterName = LongoMatchFeature.IpCameras.ToString (),
				Enabled = status.Limitations.Contains (LongoMatchFeature.IpCameras.ToString ()),
				DisplayName = Catalog.GetString ("Ip camera")
			});
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
