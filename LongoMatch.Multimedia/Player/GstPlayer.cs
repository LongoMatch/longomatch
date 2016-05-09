//  Copyright(C) 2007-2009 Andoni Morales Alastruey
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
using System;
using System.Runtime.InteropServices;
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Video.Common;
using LongoMatch.Core.Handlers;
using System.Collections.Generic;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Video.Player
{
	public class GstPlayer : GLib.Object, IPlayer
	{

		public event ErrorHandler Error;
		public event StateChangeHandler StateChange;
		public event ReadyToSeekHandler ReadyToSeek;
		public event EosHandler Eos;

		MediaFile file;
		double rate;

		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr lgm_video_player_get_type ();

		[DllImport ("libcesarplayer.dll")]
		static extern unsafe IntPtr lgm_video_player_new (int use_type, out IntPtr error);

		[DllImport ("libcesarplayer.dll")]
		static extern unsafe IntPtr lgm_video_player_set_window_handle (IntPtr raw, IntPtr window_handle);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_open (IntPtr raw, IntPtr uri, out IntPtr error);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_play (IntPtr raw, bool synchronous);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_is_playing (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_video_player_pause (IntPtr raw, bool synchronous);

		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_video_player_stop (IntPtr raw, bool synchronous);

		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_video_player_close (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_seek (IntPtr raw, double position);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_seek_time (IntPtr raw, long time, bool accurate, bool synchronous);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_seek_to_next_frame (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_seek_to_previous_frame (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern double lgm_video_player_get_position (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern long lgm_video_player_get_current_time (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern long lgm_video_player_get_stream_length (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern bool lgm_video_player_set_rate (IntPtr raw, double rate);

		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_video_player_set_volume (IntPtr raw, double volume);

		[DllImport ("libcesarplayer.dll")]
		static extern double lgm_video_player_get_volume (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern IntPtr lgm_video_player_get_current_frame (IntPtr raw);

		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_video_player_unref_pixbuf (IntPtr pixbuf);

		[DllImport ("libcesarplayer.dll")]
		static extern void lgm_video_player_expose (IntPtr pixbuf);

		public unsafe GstPlayer () : base (IntPtr.Zero)
		{
			Init (PlayerUseType.Video);
		}

		public unsafe GstPlayer (PlayerUseType type) : base (IntPtr.Zero)
		{
			Init (type);
		}

		void Init (PlayerUseType type)
		{
			rate = 1;
			IntPtr error = IntPtr.Zero;
			Raw = lgm_video_player_new ((int)type, out error);
			if (error != IntPtr.Zero)
				throw new GLib.GException (error);
			
			this.GlibError += (o, args) => {
				if (Error != null)
					Error (this, args.Message);
			};
			
			this.GlibStateChange += (o, args) => {
				if (StateChange != null)
					StateChange (this, args.Playing);
			};
			
			this.GlibReadyToSeek += (sender, e) => {
				if (ReadyToSeek != null)
					ReadyToSeek (this);
			};

			this.GlibEos += (sender, e) => {
				if (Eos != null)
					Eos (this);
			};
		}
		#pragma warning disable 0169

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void ReadyToSeekVMDelegate (IntPtr bvw);

		static ReadyToSeekVMDelegate ReadyToSeekVMCallback;

		static void readytoseek_cb (IntPtr bvw)
		{
			try {
				GstPlayer bvw_managed = GLib.Object.GetObject (bvw, false) as GstPlayer;
				bvw_managed.OnReadyToSeek ();
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideReadyToSeek (GLib.GType gtype)
		{
			if (ReadyToSeekVMCallback == null)
				ReadyToSeekVMCallback = new ReadyToSeekVMDelegate (readytoseek_cb);
			OverrideVirtualMethod (gtype, "ready_to_seek", ReadyToSeekVMCallback);
		}

		[GLib.DefaultSignalHandler (Type = typeof(LongoMatch.Video.Player.GstPlayer), ConnectionMethod = "OverrideReadyToSeek")]
		protected virtual void OnReadyToSeek ()
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (1);
			GLib.Value[] vals = new GLib.Value [1];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal ("ready_to_seek")]
		public event System.EventHandler GlibReadyToSeek {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "ready_to_seek");
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "ready_to_seek");
				sig.RemoveDelegate (value);
			}
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void StateChangeVMDelegate (IntPtr bvw,bool playing);

		static StateChangeVMDelegate StateChangeVMCallback;

		static void statechange_cb (IntPtr bvw, bool playing)
		{
			try {
				GstPlayer bvw_managed = GLib.Object.GetObject (bvw, false) as GstPlayer;
				bvw_managed.OnStateChange (playing);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideStateChange (GLib.GType gtype)
		{
			if (StateChangeVMCallback == null)
				StateChangeVMCallback = new StateChangeVMDelegate (statechange_cb);
			OverrideVirtualMethod (gtype, "state_change", StateChangeVMCallback);
		}

		[GLib.DefaultSignalHandler (Type = typeof(LongoMatch.Video.Player.GstPlayer), ConnectionMethod = "OverrideStateChange")]
		protected virtual void OnStateChange (bool playing)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (playing);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal ("state_change")]
		public event GlibStateChangeHandler GlibStateChange {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "state_change", typeof(StateChangeArgs));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "state_change", typeof(StateChangeArgs));
				sig.RemoveDelegate (value);
			}
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void EosVMDelegate (IntPtr bvw);

		static EosVMDelegate EosVMCallback;

		static void eos_cb (IntPtr bvw)
		{
			try {
				GstPlayer bvw_managed = GLib.Object.GetObject (bvw, false) as GstPlayer;
				bvw_managed.OnEos ();
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideEos (GLib.GType gtype)
		{
			if (EosVMCallback == null)
				EosVMCallback = new EosVMDelegate (eos_cb);
			OverrideVirtualMethod (gtype, "eos", EosVMCallback);
		}

		[GLib.DefaultSignalHandler (Type = typeof(LongoMatch.Video.Player.GstPlayer), ConnectionMethod = "OverrideEos")]
		protected virtual void OnEos ()
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (1);
			GLib.Value[] vals = new GLib.Value [1];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal ("eos")]
		public event System.EventHandler GlibEos {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "eos");
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "eos");
				sig.RemoveDelegate (value);
			}
		}

		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		delegate void ErrorVMDelegate (IntPtr bvw,IntPtr message);

		static ErrorVMDelegate ErrorVMCallback;

		static void error_cb (IntPtr bvw, IntPtr message)
		{
			try {
				GstPlayer bvw_managed = GLib.Object.GetObject (bvw, false) as GstPlayer;
				bvw_managed.OnError (GLib.Marshaller.Utf8PtrToString (message));
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

		[GLib.DefaultSignalHandler (Type = typeof(LongoMatch.Video.Player.GstPlayer), ConnectionMethod = "OverrideError")]
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
		public event GlibErrorHandler GlibError {
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
		delegate void TickVMDelegate (IntPtr bvw,long current_time,long stream_length,double current_position);

		static TickVMDelegate TickVMCallback;

		static void tick_cb (IntPtr bvw, long current_time, long stream_length, double current_position)
		{
			try {
				GstPlayer bvw_managed = GLib.Object.GetObject (bvw, false) as GstPlayer;
				bvw_managed.OnTick (current_time, stream_length, current_position);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideTick (GLib.GType gtype)
		{
			if (TickVMCallback == null)
				TickVMCallback = new TickVMDelegate (tick_cb);
			OverrideVirtualMethod (gtype, "tick", TickVMCallback);
		}

		[GLib.DefaultSignalHandler (Type = typeof(LongoMatch.Video.Player.GstPlayer), ConnectionMethod = "OverrideTick")]
		protected virtual void OnTick (long current_time, long stream_length, double current_position)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (5);
			GLib.Value[] vals = new GLib.Value [5];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (current_time);
			inst_and_params.Append (vals [1]);
			vals [2] = new GLib.Value (stream_length);
			inst_and_params.Append (vals [2]);
			vals [3] = new GLib.Value (current_position);
			inst_and_params.Append (vals [3]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal ("tick")]
		public event GlibTickHandler GlibTick {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "tick", typeof(TickArgs));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "tick", typeof(TickArgs));
				sig.RemoveDelegate (value);
			}
		}
		#pragma warning restore 0169

		public object WindowHandle {
			set {
				lgm_video_player_set_window_handle (Handle, (IntPtr)value);
			}
		}

		public List<object> WindowHandles {
			set {
				WindowHandle = value [0];
			}
		}

		public Time CurrentTime {
			get {
				long ret = lgm_video_player_get_current_time (Handle);
				return new Time { NSeconds = ret } - file.Offset;
			}
		}

		public Time StreamLength {
			get {
				long ret = lgm_video_player_get_stream_length (Handle);
				return new Time { NSeconds = ret };
			}
		}

		public double Volume {
			get {
				return lgm_video_player_get_volume (Handle);
			}
			set {
				lgm_video_player_set_volume (Handle, value);
			}
		}

		public bool Playing {
			get {
				return lgm_video_player_is_playing (Handle);
			}
		}

		public double Rate {
			set {
				lgm_video_player_set_rate (Handle, value);
				rate = value;
			}
			get {
				return rate;
			}
		}

		public Time Offset {
			get {
				return file.Offset;
			}
		}

		public bool Seek (Time time, bool accurate, bool synchronous)
		{
			return lgm_video_player_seek_time (Handle, time.NSeconds + Offset.NSeconds,
				accurate, synchronous);
		}

		public bool SeekToPreviousFrame ()
		{
			return lgm_video_player_seek_to_previous_frame (Handle);
		}

		public bool SeekToNextFrame ()
		{
			return lgm_video_player_seek_to_next_frame (Handle);
		}

		public void Play (bool synchronous = false)
		{
			lgm_video_player_play (Handle, synchronous);
		}

		public void Pause (bool synchronous = false)
		{
			lgm_video_player_pause (Handle, synchronous);
		}

		public void Stop (bool synchronous = false)
		{
			lgm_video_player_stop (Handle, synchronous);
		}

		public void Close ()
		{
			lgm_video_player_close (Handle);
		}

		public bool Open (MediaFile file)
		{
			this.file = file;
			IntPtr native_uri = GLib.Marshaller.StringToPtrGStrdup (file.FilePath);
			IntPtr error = IntPtr.Zero;
			bool ret = lgm_video_player_open (Handle, native_uri, out error);
			GLib.Marshaller.Free (native_uri);
			if (error != IntPtr.Zero)
				throw new GLib.GException (error);
			return ret;
		}

		public bool Open (string filePath)
		{
			return Open (new MediaFile { FilePath = filePath, Offset = new Time (0) });
		}

		public Image GetCurrentFrame (int outwidth = -1, int outheight = -1)
		{
			Gdk.Pixbuf managed, unmanaged;
			IntPtr raw_ret;
			int h, w;
			double rate;
			
			raw_ret = lgm_video_player_get_current_frame (Handle);
			unmanaged = GLib.Object.GetObject (raw_ret) as Gdk.Pixbuf;
			if (unmanaged == null)
				return null;

			h = unmanaged.Height;
			w = unmanaged.Width;
			rate = (double)w / (double)h;
			if (outwidth == -1 || outheight == -1) {
				outwidth = w;
				outheight = h;
			} else if (h > w) {
				outwidth = (int)(outheight * rate);
			} else {
				outheight = (int)(outwidth / rate);
			}

			managed = unmanaged.ScaleSimple (outwidth, outheight, Gdk.InterpType.Bilinear);
			unmanaged.Dispose ();
			lgm_video_player_unref_pixbuf (raw_ret);
			return new Image (managed);
		}

		public void Expose ()
		{
			lgm_video_player_expose (Handle);
		}

		public static new GLib.GType GType {
			get {
				return new GLib.GType (lgm_video_player_get_type ());
			}
		}

		static GstPlayer ()
		{
			LongoMatch.GtkSharp.Video.ObjectManager.Initialize ();
		}
	}

	public class GstFramesCapturer: GstPlayer , IFramesCapturer
	{
	
		public unsafe GstFramesCapturer () : base (PlayerUseType.Capture)
		{
		}

		public Image GetFrame (Time pos, bool accurate, int outwidth = -1, int outheight = -1)
		{
			Image img = null;
			
			Pause ();
			Seek (pos, accurate, true);
			img = GetCurrentFrame (outwidth, outheight);
			return img;
		}
	}
}
