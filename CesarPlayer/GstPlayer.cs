// GSTPayer.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//


using System;
using System.Runtime.InteropServices;
using Mono.Unix;
using Gtk;
using Gdk;

namespace CesarPlayer
{
	
	
	public enum BvwUseType {BVW_USE_TYPE_VIDEO,
							BVW_USE_TYPE_AUDIO,
							BVW_USE_TYPE_CAPTURE,
							BVW_USE_TYPE_METADATA};
				
	public class PlayerException : Exception
	{
		public PlayerException (IntPtr p) 
		: base (GLib.Marshaller.PtrToStringGFree (p)) 
		{
		}
	}

	public class GstPlayer : GLib.Object, IPlayer
	{
		// Events

		public event         StateChangedHandler StateChanged;


		public event         TickEventHandler TickEvent;


		public event         EndOfStreamEventHandler EndOfStreamEvent;


		public event         InvalidVideoFileHandler InvalidVideoFile;
		
		public event         SegmentDoneHandler SegmentDoneEvent;
		
		public event         ErrorEventHandler ErrorEvent;


		// Callbacks
		private SignalUtils.SignalDelegateTick tick_cb ;
		private SignalUtils.SignalDelegate    eos_cb  ;
		private SignalUtils.SignalDelegateStr error_cb;
		private SignalUtils.SignalDelegate segment_cb;
		private SignalUtils.SignalDelegateBool state_changed_cb;
		
	

		private long stopTime=0;
		private int THUMBNAIL_WIDTH = 50;


		
		//Ventana
		private Widget window; 

		// Constructor
		[DllImport ("liblongomatch")]
		private static extern IntPtr bacon_video_widget_new (int width, 
															int height,
															BvwUseType type,
						  									out IntPtr error_ptr);
		
		[DllImport ("liblongomatch")]
        private static extern IntPtr bacon_video_widget_get_window(IntPtr player);
        
        [DllImport ("liblongomatch")]
        private static extern void bacon_video_widget_init_backend(IntPtr argv, IntPtr argc);
		
		public GstPlayer (int width, int heigth, BvwUseType type) : base (IntPtr.Zero)
		{
			

			//If there is an error throw an Exception
			//Los errores los gestionamos de forma asincrona
			/*if (error_ptr != IntPtr.Zero)
				throw new PlayerException (error_ptr);*/
			
			//Creamos las señales que se pueden lanzar
			tick_cb  += new SignalUtils.SignalDelegateTick (OnTick       );
			eos_cb   += new SignalUtils.SignalDelegate    (OnEndOfStream);
			error_cb += new SignalUtils.SignalDelegateStr (OnError      );
			segment_cb += new SignalUtils.SignalDelegate  (OnSegmentDone);
			state_changed_cb += new SignalUtils.SignalDelegateBool (OnStateChanged);
			
			IntPtr error_ptr;
			//Create the player
			bacon_video_widget_init_backend(IntPtr.Zero,IntPtr.Zero);
			
			Raw = bacon_video_widget_new (width,heigth,type, out error_ptr);
			
			//Create the widget 
			window = new Widget(bacon_video_widget_get_window(Raw));
			
			//El objeto Player puede lanzar señales que vienen definidas
			//en el archivo palyer.c con los nombre tick end_of_stream y error
			// y serán atendidas por tick_cb eos_cb y error_cb y sus respectivos
			//métodos OnTick OnEndOfStream y OnError
			SignalUtils.SignalConnect (Raw, "tick"         , tick_cb );
			SignalUtils.SignalConnect (Raw, "eos"		   , eos_cb  );
			SignalUtils.SignalConnect (Raw, "error"        , error_cb);
			SignalUtils.SignalConnect (Raw, "segment_done" ,segment_cb);
			SignalUtils.SignalConnect (Raw, "state_changed" ,state_changed_cb);

		}

		// Destructor
		~GstPlayer ()
		{
			Dispose ();
		}

		// Properties
		// Properties :: Song (set; get;)
		[DllImport ("liblongomatch")]
		private static extern bool bacon_video_widget_open(IntPtr player, 
															string filename,
															out IntPtr error_ptr);
		
		[DllImport ("liblongomatch")]
		private static extern string bacon_video_widget_get_mrl (IntPtr player);
		

		public string FilePath {
			set {
				IntPtr err;
			   	bacon_video_widget_open(Raw,"file://"+value, out err);			
			}

			get { return bacon_video_widget_get_mrl (Raw); }
		}
		


		// Properties :: Playing (get;)
		[DllImport ("liblongomatch")]
		private static extern bool bacon_video_widget_is_playing (IntPtr player);
		
		public bool Playing {
			get { return bacon_video_widget_is_playing (Raw); }
		}

		[DllImport ("liblongomatch")]
		private static extern long bacon_video_widget_get_stream_length (IntPtr player);
		
		// Properties :: Length (get;)
		
		public long Length {
			//Return length in seconds
			get { return bacon_video_widget_get_stream_length (Raw); }
		}
		
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_set_logo_mode (IntPtr player,bool logoMode);
		[DllImport ("liblongomatch")]
		private static extern bool bacon_video_widget_get_logo_mode (IntPtr player);
		
		// Properties :: Logo Mode (set; get;)
		
		public bool LogoMode {
			set { bacon_video_widget_set_logo_mode(Raw,value);
			}
			get { return bacon_video_widget_get_logo_mode (Raw);}
		}
		
		// Properties :: Position (set; get;)
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_seek_time (IntPtr player, long t,bool accurate);
		
		public void Seek(long time, bool accurate){
			bacon_video_widget_seek_time (Raw, time, accurate);
		}

		[DllImport ("liblongomatch")]
		private static extern long bacon_video_widget_get_current_time (IntPtr player);

		public long CurrentTime {

			get { return bacon_video_widget_get_current_time (Raw); }
		}
		
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_seek (IntPtr player, float t);
		
		[DllImport ("liblongomatch")]
		private static extern float bacon_video_widget_get_position (IntPtr player);
		
		public float Position {
			
			get { return bacon_video_widget_get_position (Raw); }
			
			set { bacon_video_widget_seek(Raw,value);}
		}
		
		
		// Properties :: Volume (set; get;)
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_set_volume (IntPtr player, int volume);

		[DllImport ("liblongomatch")]
		private static extern int bacon_video_widget_get_volume(IntPtr player);

		public int Volume {
			set { bacon_video_widget_set_volume (Raw, value); }
			get { return bacon_video_widget_get_volume(Raw); }
		}


		// Methods
		// Methods :: Public
		// Methods :: Public :: Play
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_play (IntPtr player);

		public void Play ()
		{	
			bacon_video_widget_play (Raw);
			/*if (StateChanged != null)
				StateChanged (this.Playing);*/
				
		}

		// Methods :: Public :: Pause
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_pause (IntPtr player);

		public void Pause ()
		{
			
			bacon_video_widget_pause (Raw);
	
			/*if (StateChanged != null)
				StateChanged (this.Playing);*/

		}

		// Methods :: Public :: Stop
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_stop (IntPtr player);

		public void Stop ()
		{
			
			bacon_video_widget_stop  (Raw);
			
			/*if (StateChanged != null)
				StateChanged (this.Playing);*/
			
			//Anulamos la parada programada
			this.CancelProgramedStop();
		}
		
				// Methods :: Public :: Close
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_close (IntPtr player);

		public void Close ()
		{
			bacon_video_widget_close  (Raw);
			
			/*if (StateChanged != null)
				StateChanged (this.Playing);*/
		}
		
		[DllImport ("liblongomatch")]
		private static extern bool bacon_video_widget_set_rate (IntPtr player, float rate);
		
		public float Rate {
			get{return 1;}
			set{ bacon_video_widget_set_rate (Raw,value);
			}
		}
		
		public void TogglePlay(){

			if(!this.Playing){
				this.Play();
			}
			else{
				this.Pause();
			}
			
		}
		
		
		[DllImport ("liblongomatch")]		
		private static extern long bacon_video_widget_get_accurate_current_time (IntPtr player);
		
		public long GetAccurateCurrentTime(){
			
			
			return bacon_video_widget_get_accurate_current_time(Raw);
		}
		
		[DllImport ("liblongomatch")]		
		private static extern IntPtr bacon_video_widget_get_current_frame (IntPtr player);
		
		public Pixbuf GetCurrentFrame(){
			
			IntPtr ptr = bacon_video_widget_get_current_frame (Raw);				
			Pixbuf pixbuf = new Pixbuf(ptr);
			return pixbuf;
		}
		
		public Pixbuf GetCurrentThumbnail(){
			int h,w;
			double rate;
			Pixbuf pixbuf = this.GetCurrentFrame();
			h = pixbuf.Height;
			w = pixbuf.Width;
			rate = w/h;
			return pixbuf.ScaleSimple(THUMBNAIL_WIDTH,(int)(THUMBNAIL_WIDTH/rate),InterpType.Bilinear);
		}
		
		[DllImport ("liblongomatch")]
		private static extern bool bacon_video_widget_segment_start_update(IntPtr player, long start);
			
		public void UpdateSegmentStartTime(long start){
			bacon_video_widget_segment_start_update(Raw,start);
		}
		
		[DllImport ("liblongomatch")]
		private static extern bool bacon_video_widget_segment_stop_update(IntPtr player, long stop);
			
		public void UpdateSegmentStopTime(long stop){
			bacon_video_widget_segment_stop_update(Raw,stop);
		}

		// Methods :: Public :: SetLogo
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_set_logo (IntPtr player,String fileName);
		
		public void SetLogo (String fileName){
			bacon_video_widget_set_logo (Raw,fileName);
			
		}

		
		// Methods :: Public :: Window
		

		public Widget Window{
			get{return window;}
		}
		
		public void CancelProgramedStop(){
			this.SegmentSeek(this.CurrentTime,this.Length);		
		}
		

		
		[DllImport ("liblongomatch")]
		private static extern void bacon_video_widget_segment_seek (IntPtr player,long start, long stop);
		public void SegmentSeek(long start, long stop){
			bacon_video_widget_segment_seek(Raw,start,stop);
			this.Play();
		
		}
		
		[DllImport ("liblongomatch")]
		private static extern bool bacon_video_widget_seek_in_segment (IntPtr player, long pos );
		public void SeekInSegment(long pos){
			bacon_video_widget_seek_in_segment(Raw,pos);
			this.Play();
		}


		// Handlers
		// Handlers :: OnTick
		//Es una señal que se produce cada 20ms y que nos va indicando la 
		//posición actual de la fuente, como los ticks de un reloj
		private void OnTick (IntPtr obj, long currentTime, long streamLength, float position, bool seekable)
		{	
			
			if (TickEvent != null)
				TickEvent (currentTime,streamLength,position,seekable);

			//Si llegamos al punto de parada programado	
			if (stopTime >0 && currentTime >= stopTime ){	
					this.Pause();
					this.CancelProgramedStop();
			}
			
		}
		
		
		// Handlers :: OnEndOfStream
		private void OnEndOfStream (IntPtr obj)
		{

			if (EndOfStreamEvent != null){
				EndOfStreamEvent ();
			}
			
		}
			
		private void OnSegmentDone (IntPtr obj)
		{
			if (SegmentDoneEvent != null)
				this.SegmentDoneEvent();
			
		}
		
		private void OnStateChanged (IntPtr obj, bool playing){
			if (StateChanged != null)
				this.StateChanged(playing);
		}

		// Handlers :: OnError
		private void OnError (IntPtr obj, string error)
		{
			if (ErrorEvent !=null)
				this.ErrorEvent(error);

		}
	}
}
