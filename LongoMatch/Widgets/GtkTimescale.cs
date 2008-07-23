// This file was generated by the Gtk# code generator.
// Any changes made will be lost if regenerated.

namespace LongoMatch {

	using System;
	using System.Collections;
	using System.Runtime.InteropServices;

	
	public enum AdjustType {

		ADJ_POS = 0,
		ADJ_IN = 1,
		ADJ_OUT = 2,
	};
	
#region Autogenerated code
	public  class GtkTimescale : Gtk.HBox {

		
	
		[Obsolete]
		protected GtkTimescale(GLib.GType gtype) : base(gtype) {}
		public GtkTimescale(IntPtr raw) : base(raw) {}

		[DllImport("timescale.dll")]
		static extern IntPtr gtk_timescale_new(int upper);

		public GtkTimescale (int upper) : base (IntPtr.Zero)
		{
			if (GetType () != typeof (GtkTimescale)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			Raw = gtk_timescale_new(upper);
		}

		public GtkTimescale() : base (IntPtr.Zero)
		{
			if (GetType () != typeof (GtkTimescale)) {
				throw new InvalidOperationException ("Can't override this constructor.");
			}
			Raw = gtk_timescale_new(UInt16.MaxValue);
			
		}
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void PosChangedSignalDelegate (IntPtr arg0, double arg1, IntPtr gch);

		static void PosChangedSignalCallback (IntPtr arg0, double arg1, IntPtr gch)
		{
			LongoMatch.PosChangedArgs args = new LongoMatch.PosChangedArgs ();
			try {
				GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
				if (sig == null)
					throw new Exception("Unknown signal GC handle received " + gch);

				args.Args = new object[1];
				args.Args[0] = arg1;
				LongoMatch.PosChangedHandler handler = (LongoMatch.PosChangedHandler) sig.Handler;
				handler (GLib.Object.GetObject (arg0), args);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void PosChangedVMDelegate (IntPtr gts, double val);

		static PosChangedVMDelegate PosChangedVMCallback;

		static void poschanged_cb (IntPtr gts, double val)
		{
			try {
				GtkTimescale gts_managed = GLib.Object.GetObject (gts, false) as GtkTimescale;
				gts_managed.OnPosChanged (val);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverridePosChanged (GLib.GType gtype)
		{
			if (PosChangedVMCallback == null)
				PosChangedVMCallback = new PosChangedVMDelegate (poschanged_cb);
			OverrideVirtualMethod (gtype, "pos_changed", PosChangedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(LongoMatch.GtkTimescale), ConnectionMethod="OverridePosChanged")]
		protected virtual void OnPosChanged (double val)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (val);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("pos_changed")]
		public event LongoMatch.PosChangedHandler PosChanged {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "pos_changed", new PosChangedSignalDelegate(PosChangedSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "pos_changed", new PosChangedSignalDelegate(PosChangedSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void OutChangedSignalDelegate (IntPtr arg0, double arg1, IntPtr gch);

		static void OutChangedSignalCallback (IntPtr arg0, double arg1, IntPtr gch)
		{
			LongoMatch.OutChangedArgs args = new LongoMatch.OutChangedArgs ();
			try {
				GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
				if (sig == null)
					throw new Exception("Unknown signal GC handle received " + gch);

				args.Args = new object[1];
				args.Args[0] = arg1;
				LongoMatch.OutChangedHandler handler = (LongoMatch.OutChangedHandler) sig.Handler;
				handler (GLib.Object.GetObject (arg0), args);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void OutChangedVMDelegate (IntPtr gts, double val);

		static OutChangedVMDelegate OutChangedVMCallback;

		static void outchanged_cb (IntPtr gts, double val)
		{
			try {
				GtkTimescale gts_managed = GLib.Object.GetObject (gts, false) as GtkTimescale;
				gts_managed.OnOutChanged (val);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideOutChanged (GLib.GType gtype)
		{
			if (OutChangedVMCallback == null)
				OutChangedVMCallback = new OutChangedVMDelegate (outchanged_cb);
			OverrideVirtualMethod (gtype, "out_changed", OutChangedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(LongoMatch.GtkTimescale), ConnectionMethod="OverrideOutChanged")]
		protected virtual void OnOutChanged (double val)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (val);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("out_changed")]
		public event LongoMatch.OutChangedHandler OutChanged {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "out_changed", new OutChangedSignalDelegate(OutChangedSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "out_changed", new OutChangedSignalDelegate(OutChangedSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void InChangedSignalDelegate (IntPtr arg0, double arg1, IntPtr gch);

		static void InChangedSignalCallback (IntPtr arg0, double arg1, IntPtr gch)
		{
			LongoMatch.InChangedArgs args = new LongoMatch.InChangedArgs ();
			try {
				GLib.Signal sig = ((GCHandle) gch).Target as GLib.Signal;
				if (sig == null)
					throw new Exception("Unknown signal GC handle received " + gch);

				args.Args = new object[1];
				args.Args[0] = arg1;
				LongoMatch.InChangedHandler handler = (LongoMatch.InChangedHandler) sig.Handler;
				handler (GLib.Object.GetObject (arg0), args);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate void InChangedVMDelegate (IntPtr gts, double val);

		static InChangedVMDelegate InChangedVMCallback;

		static void inchanged_cb (IntPtr gts, double val)
		{
			try {
				GtkTimescale gts_managed = GLib.Object.GetObject (gts, false) as GtkTimescale;
				gts_managed.OnInChanged (val);
			} catch (Exception e) {
				GLib.ExceptionManager.RaiseUnhandledException (e, false);
			}
		}

		private static void OverrideInChanged (GLib.GType gtype)
		{
			if (InChangedVMCallback == null)
				InChangedVMCallback = new InChangedVMDelegate (inchanged_cb);
			OverrideVirtualMethod (gtype, "in_changed", InChangedVMCallback);
		}

		[GLib.DefaultSignalHandler(Type=typeof(LongoMatch.GtkTimescale), ConnectionMethod="OverrideInChanged")]
		protected virtual void OnInChanged (double val)
		{
			GLib.Value ret = GLib.Value.Empty;
			GLib.ValueArray inst_and_params = new GLib.ValueArray (2);
			GLib.Value[] vals = new GLib.Value [2];
			vals [0] = new GLib.Value (this);
			inst_and_params.Append (vals [0]);
			vals [1] = new GLib.Value (val);
			inst_and_params.Append (vals [1]);
			g_signal_chain_from_overridden (inst_and_params.ArrayPtr, ref ret);
			foreach (GLib.Value v in vals)
				v.Dispose ();
		}

		[GLib.Signal("in_changed")]
		public event LongoMatch.InChangedHandler InChanged {
			add {
				GLib.Signal sig = GLib.Signal.Lookup (this, "in_changed", new InChangedSignalDelegate(InChangedSignalCallback));
				sig.AddDelegate (value);
			}
			remove {
				GLib.Signal sig = GLib.Signal.Lookup (this, "in_changed", new InChangedSignalDelegate(InChangedSignalCallback));
				sig.RemoveDelegate (value);
			}
		}

		[DllImport("timescale.dll")]
		static extern void gtk_timescale_adjust_position(IntPtr raw, double val, int adj);

		public void AdjustPosition(double val, AdjustType adj) {
			gtk_timescale_adjust_position(Handle, val, (int)adj);
		}

		[DllImport("timescale.dll")]
		static extern IntPtr gtk_timescale_get_type();

		public static new GLib.GType GType { 
			get {
				IntPtr raw_ret = gtk_timescale_get_type();
				GLib.GType ret = new GLib.GType(raw_ret);
				return ret;
			}
		}

		[DllImport("timescale.dll")]
		static extern void gtk_timescale_set_bounds(IntPtr raw, double lower, double upper);

		public void SetBounds(double lower, double upper) {
			gtk_timescale_set_bounds(Handle, lower, upper);
		}

		[DllImport("timescale.dll")]
		static extern void gtk_timescale_set_segment(IntPtr raw, double in_param, double out_param);

		public void SetSegment(double in_param, double out_param) {
			gtk_timescale_set_segment(Handle, in_param, out_param);
		}


		static GtkTimescale ()
		{
			GtkSharp.LongomatchSharp.ObjectManager.Initialize ();
		}
#endregion
	}
}
