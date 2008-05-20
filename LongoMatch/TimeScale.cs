// TimeScale.cs created with MonoDevelop
// User: ando at 21:09 06/01/2008
//
// To change standard headers go to Edit->Preferences->Coding->Standard Headers
//

using System;
using System.Runtime.InteropServices;
using Gtk;

namespace LongoMatch
{
	
	public delegate void PosValueChangedHandler(double pos);
	public delegate void StartValueChangedHandler(double pos);
	public delegate void StopValueChangedHandler(double pos);
	
	public partial class TimeScale : Gtk.Bin
	{
		
		public const int POS = 0;
		public const int START = 1;
		public const int STOP = 2;
		
		public event PosValueChangedHandler PosValueChanged;
		public event StartValueChangedHandler StartValueChanged;
		public event StopValueChangedHandler StopValueChanged;
		
		//public event ValueChangedHandler ValueChanged;
		// Callbacks
		private SignalUtils.SignalDelegateDouble adj_pos_cb ;
		private SignalUtils.SignalDelegateDouble adj_in_cb  ;
		private SignalUtils.SignalDelegateDouble adj_out_cb;
		

		private IntPtr Raw;
		
		[DllImport ("liblongomatch")]
		private static extern IntPtr  gtk_timescale_new(double upper);
		
		~ TimeScale(){
			Console.WriteLine("Disposing");
			GLib.Object o = GLib.Object.GetObject(Raw);
			o.Dispose();
			Dispose();
		}
		public TimeScale()
		{
			this.Build();
			Widget timescale;
			Raw = gtk_timescale_new(UInt16.MaxValue);
			timescale =  new Widget(Raw);
			timescale.Show();
			this.Add(timescale);
			
			//Creamos las señales que se pueden lanzar
			
			adj_in_cb    += new SignalUtils.SignalDelegateDouble   (OnInAdjusted);
			adj_out_cb   += new SignalUtils.SignalDelegateDouble   (OnOutAdjusted);
			adj_pos_cb   += new SignalUtils.SignalDelegateDouble   (OnPosAdjusted);
			
			
			//El objeto Player puede lanzar señales que vienen definidas
			//en el archivo palyer.c con los nombre tick end_of_stream y error
			// y serán atendidas por tick_cb eos_cb y error_cb y sus respectivos
			//métodos OnTick OnEndOfStream y OnError
			SignalUtils.SignalConnect (Raw, "pos_changed"         , adj_pos_cb );
			SignalUtils.SignalConnect (Raw, "in_changed"              , adj_in_cb  );
			SignalUtils.SignalConnect (Raw, "out_changed"        , adj_out_cb);
		}
		
		
		[DllImport ("liblongomatch")]
		private static extern void  gtk_timescale_set_bounds(IntPtr timescale,double lower, double upper);
		public void SetBounds(double lower, double upper){
			
			gtk_timescale_set_bounds(Raw,lower, upper);
		}
		
		[DllImport ("liblongomatch")]
		private static extern void gtk_timescale_set_segment(IntPtr timescale,double start, double stop);
		public void SetSegment(double start, double stop){
			gtk_timescale_set_segment(Raw,start,stop);
		}
		
		[DllImport ("liblongomatch")]
		private static extern void  gtk_timescale_adjust_position(IntPtr timescale,double pos,int adj);
		public void AdjustPosition (double pos, int adj){
			
			gtk_timescale_adjust_position(Raw, pos,adj);
			if (adj == START || adj == STOP)
				gtk_timescale_adjust_position(Raw, pos,POS);
				
		}
		
		protected virtual void OnPosAdjusted(IntPtr obj, double val){
			if (PosValueChanged != null )
				this.PosValueChanged(val);
	
		}
		
		protected virtual void OnInAdjusted(IntPtr obj, double val){
			if (StartValueChanged != null)
				this.StartValueChanged(val);
		}
		
		protected virtual void OnOutAdjusted(IntPtr obj, double val){
			if (StopValueChanged != null)
				this.StopValueChanged(val);
		}
	}
}
