// GstVideoSplitter.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//
namespace LongoMatch.Video.Editor
{
	using System;
	using System.Collections;
	using System.Runtime.InteropServices;
	using LongoMatch.Core.Interfaces;
	using LongoMatch.Core.Common;
	using LongoMatch.Core.Interfaces.Multimedia;
	using LongoMatch.Video.Common;

	public class GstVideoSplitter : GLib.Object, IVideoEditor
	{

		[DllImport ("libcesarplayer.dll")]
		static extern unsafe IntPtr gst_video_editor_new (out IntPtr err);

		public event LongoMatch.Core.Handlers.ProgressHandler Progress;
		public event LongoMatch.Core.Handlers.ErrorHandler Error;

		public unsafe GstVideoSplitter () : base (IntPtr.Zero)
		{
			if (GetType () != typeof(GstVideoSplitter)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			IntPtr error = IntPtr.Zero;
			Raw = gst_video_editor_new (out error);
			if (error != IntPtr.Zero)
				throw new GLib.GException (error);
			PercentCompleted += delegate(object o, PercentCompletedArgs args) {
				if (Progress != null)
					Progress (args.Percent);
			};
			InternalError += delegate(object o, ErrorArgs args) {
				if (Error != null)
					Error (this, args.Message);
			};
		}

		#region GSignals

		#pragma warning disable 0169
		[GLib.CDeclCallback]
		delegate void ErrorVMDelegate (IntPtr gvc,IntPtr message);

		static ErrorVMDelegate ErrorVMCallback;

		static void error_cb (IntPtr gvc, IntPtr message)
		{
			try {
				GstVideoSplitter gvc_managed = GLib.Object.GetObject (gvc, false) as GstVideoSplitter;
				gvc_managed.OnError (GLib.Marshaller.Utf8PtrToString (message));
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideError (GLib.GType gtype)
		{
			if (ErrorVMCallback == null)
				ErrorVMCallback = new ErrorVMDelegate (error_cb);
			OverrideVirtualMethod (gtype, "error", ErrorVMCallback);
		}

		[GLib.DefaultSignalHandler (Type = typeof(LongoMatch.Video.Editor.GstVideoSplitter), ConnectionMethod = "OverrideError")]
		protected virtual void OnError (string message)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (message);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal ("error")]
		public event GlibErrorHandler InternalError {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "error", typeof(ErrorArgs));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "error", typeof(ErrorArgs));
				sig.RemoveDelegate (value);
			}
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void PercentCompletedVMDelegate (IntPtr gvc,float percent);

		static PercentCompletedVMDelegate PercentCompletedVMCallback;

		static void percentcompleted_cb (IntPtr gvc, float percent)
		{
			try {
				GstVideoSplitter gvc_managed = GLib.Object.GetObject (gvc, false) as GstVideoSplitter;
				gvc_managed.OnPercentCompleted (percent);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverridePercentCompleted (GLib.GType gtype)
		{
			if (PercentCompletedVMCallback == null)
				PercentCompletedVMCallback = new PercentCompletedVMDelegate (percentcompleted_cb);
			OverrideVirtualMethod (gtype, "percent_completed", PercentCompletedVMCallback);
		}

		[GLib.DefaultSignalHandler (Type = typeof(LongoMatch.Video.Editor.GstVideoSplitter), ConnectionMethod = "OverridePercentCompleted")]
		protected virtual void OnPercentCompleted (float percent)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (percent);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal ("percent_completed")]
		public event GlibPercentCompletedHandler PercentCompleted {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "percent_completed", typeof(PercentCompletedArgs));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "percent_completed", typeof(PercentCompletedArgs));
				sig.RemoveDelegate (value);
			}
		}
		#pragma warning disable 0169
		#endregion

		#region Public Methods

		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr gst_video_editor_get_type ();

		public static new GLib.GType GType {
			get {
				IntPtr raw_ret = gst_video_editor_get_type ();
				GLib.GType ret = new GLib.GType (raw_ret);
				return ret;
			}
		}

		[DllImport ("libcesarplayer.dll")]
		static extern void gst_video_editor_clear_segments_list (IntPtr raw);

		public void ClearList ()
		{
			gst_video_editor_clear_segments_list (Handle);
		}

		[DllImport ("libcesarplayer.dll")]
		static extern void gst_video_editor_add_segment (IntPtr raw, string file_path, long start, long duration, double rate, IntPtr title, bool hasAudio,
		                                                 uint roi_x, uint roi_y, uint roi_w, uint roi_h);

		public void AddSegment (string filePath, long start, long duration, double rate, string title, bool hasAudio, Area roi)
		{
			gst_video_editor_add_segment (Handle, filePath, start, duration, rate, GLib.Marshaller.StringToPtrGStrdup (title), true, (uint)roi.Start.X, (uint)roi.Start.Y, (uint)roi.Width, (uint)roi.Height);
		}

		[DllImport ("libcesarplayer.dll")]
		static extern void gst_video_editor_add_image_segment (IntPtr raw, string file_path, long start, long duration, IntPtr title,
		                                                       uint roi_x, uint roi_y, uint roi_w, uint roi_h);

		public void AddImageSegment (string filePath, long start, long duration, string title, Area roi)
		{
			gst_video_editor_add_image_segment (Handle, filePath, start, duration, GLib.Marshaller.StringToPtrGStrdup (title), (uint)roi.Start.X, (uint)roi.Start.Y, (uint)roi.Width, (uint)roi.Height);
		}

		[DllImport ("libcesarplayer.dll")]
		static extern void gst_video_editor_start (IntPtr raw, bool hardwareAcceleration);

		public void Start ()
		{
			gst_video_editor_start (Handle, LongoMatch.Config.HardwareAcceleration);
		}

		[DllImport ("libcesarplayer.dll")]
		static extern void gst_video_editor_cancel (IntPtr raw);

		public void Cancel ()
		{
			// The handle might have already been dealocated
			try {
				gst_video_editor_cancel (Handle);
			} catch {
			}
		}

		[DllImport ("libcesarplayer.dll")]
		static extern void gst_video_editor_init_backend (out int argc, IntPtr argv);

		public static int InitBackend (string argv)
		{
			int argc;
			gst_video_editor_init_backend (out argc, GLib.Marshaller.StringToPtrGStrdup (argv));
			return argc;
		}

		[DllImport ("libcesarplayer.dll")]
		static extern bool gst_video_editor_set_encoding_format (IntPtr raw,
		                                                         string output_file,
		                                                         VideoEncoderType video_codec,
		                                                         AudioEncoderType audio_codec,
		                                                         VideoMuxerType muxer,
		                                                         uint video_quality,
		                                                         uint audio_quality,
		                                                         uint width,
		                                                         uint height,
		                                                         uint fps_n,
		                                                         uint fps_d,
		                                                         bool enable_audio,
		                                                         bool enable_video);

		public EncodingSettings EncodingSettings {
			set {
				gst_video_editor_set_encoding_format (Handle,
					value.OutputFile,
					value.EncodingProfile.VideoEncoder,
					value.EncodingProfile.AudioEncoder,
					value.EncodingProfile.Muxer,
					value.EncodingQuality.VideoQuality,
					value.EncodingQuality.AudioQuality,
					value.VideoStandard.Width,
					value.VideoStandard.Height,
					value.Framerate_n,
					value.Framerate_d,
					value.EnableAudio,
					value.EnableTitle);
			}
		}

		public string TempDir {
			set {
				;
			}
		}

		#endregion
	}
}
