//
//  Copyright (C) 2016 Fluendo S.A.
//
//
using System.Threading.Tasks;

namespace LongoMatch.Core.Interfaces.Services
{
	/// <summary>
	/// Interface for exposing native device services
	/// </summary>
	public interface IDeviceServices
	{
		void ShareFiles (string [] filePaths, bool emailEnabled);

		/// <summary>
		/// Return if camera and micro recording is allowed. 
		/// If permissions are denied by default, tries to request for permissions.
		/// </summary>
		/// <returns>The capture permission allowed.</returns>
		Task<bool> CheckCapturePermissions ();

		/// <summary>
		/// Return if external storage access is allowed. 
		/// If permissions are denied by default, tries to request for permissions.
		/// If the device doesn't support external storage, alwais returns false.
		/// </summary>
		/// <returns>The external storage permission allowed.</returns>
		Task<bool> CheckExternalStoragePermission ();
	}
}

