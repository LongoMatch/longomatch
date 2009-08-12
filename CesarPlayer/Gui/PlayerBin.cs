// PlayerBin.cs 
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//
using System;
using Gtk;
using Gdk;
using Mono.Unix;
using System.Runtime.InteropServices;
using LongoMatch.Video.Handlers;
using LongoMatch.Video.Player;
using LongoMatch.Video.Utils;
using LongoMatch.Video;

namespace LongoMatch.Gui
{
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
		
	public partial class PlayerBin : Gtk.Bin
	{
		
		public event SegmentClosedHandler SegmentClosedEvent;
		public event TickHandler Tick;
		public event ErrorHandler Error;
		public event NextButtonClickedHandler Next;
		public event PrevButtonClickedHandler Prev;
		
		private const int THUMBNAIL_WIDTH = 50;
		private TickHandler tickHandler;
		private IPlayer player;
		private long length=0;
		private string slength;
		private long segmentStartTime;
		private long segmentStopTime;
		private bool hasNext;
		private bool seeking=false;
		private bool IsPlayingPrevState = false;
		private float rate=1;
		private double previousVLevel = 1;
		private bool muted=false;
		//the player.mrl is diferent from the filename as it's an uri eg:file:///foo.avi
		private string filename = null;
		protected VolumeWindow vwin;
				
#region Constructors		
		public PlayerBin()
		{		
			this.Build();
			PlayerInit();
			vwin = new VolumeWindow();
			vwin.VolumeChanged += new VolumeChangedHandler(OnVolumeChanged);
			controlsbox.Visible = false;	
			UnSensitive();
			timescale.Adjustment.PageIncrement = 0.0001;
			timescale.Adjustment.StepIncrement = 0.0001;
		}
		
#endregion
	
#region Properties
		
		public IPlayer Player{
			get{return player;}
		}
		
		public long AccurateCurrentTime{
			get{return player.AccurateCurrentTime;}
		}
		
		public long CurrentTime{
			get{return player.CurrentTime;}
		}
		
		public long StreamLength{
			get{return player.StreamLength;}
		}
		
		public float Rate{
			get{return rate;}
			set{vscale1.Value = (int)(value*25);}
		}
		
		public bool FullScreen{
			set{
				if (value)
					GdkWindow.Fullscreen();
				else 
					GdkWindow.Unfullscreen();				
			}
		}
		
		public Pixbuf CurrentFrame{
			get{
				int h,w;
				double rate;
				Pixbuf pixbuf = player.CurrentFrame;
				if (pixbuf != null){
					h = pixbuf.Height;
					w = pixbuf.Width;
					rate = (double)w/(double)h;
					return pixbuf.ScaleSimple(THUMBNAIL_WIDTH,(int)(THUMBNAIL_WIDTH/rate),InterpType.Bilinear);
				}
				else return null;
			}
		}
		
		public bool LogoMode {
			get{return player.LogoMode;}
			set{player.LogoMode = value;}
		}		
		
#endregion	
		
#region Public methods		
		
		public void Open (string mrl){
			filename = mrl;
			ResetGui();
			CloseActualSegment();			
				try{
					player.Open(mrl);
				}
				catch {
				//We handle this error async				
				}
		}
		
		public void Play(){			
			player.Play();			
			float val = GetRateFromScale();							
			if (segmentStartTime == 0 && segmentStopTime==0)
				player.SetRate(val);
			else
				player.SetRateInSegment(val,segmentStopTime);		
		}
		
		public void Pause(){
			player.Pause();
		}		
		
		public void SetLogo (string filename){
			player.Logo=filename;
		}
		
		public void ResetGui(){
			closebutton.Hide();
			SetSensitive();
			timescale.Value=0;
			timelabel.Text="";
			player.CancelProgramedStop();			
		}
	
		public void SetPlayListElement(string fileName,long start, long stop, float rate, bool hasNext){
			this.hasNext = hasNext;
			if (hasNext)
				nextbutton.Sensitive = true;
			else
				nextbutton.Sensitive = false;
			
			if (fileName != filename){
				Open(fileName);				
				player.NewFileSeek(start,stop,rate);		
			}
			else player.SegmentSeek(start,stop,rate);	
			
			segmentStartTime = start;
			segmentStopTime = stop;
			player.LogoMode = false;
			Rate = rate;
		}
		
		public void Close(){
			player.Close();
			filename = null;
			timescale.Value = 0;
			UnSensitive();
		}
		
		public void SeekTo(long time, bool accurate){
			player.SeekTime(time,1,accurate);
		}
		
		public void SeekInSegment(long pos){
			player.SeekInSegment(pos, GetRateFromScale());
		}
		
		public void SeekToNextFrame(bool in_segment){
			if (segmentStopTime==0 | player.CurrentTime < segmentStopTime)
				player.SeekToNextFrame( GetRateFromScale(), in_segment);
		}
		
		public void SeekToPreviousFrame(bool in_segment){
			if (player.CurrentTime > segmentStartTime){
				player.SeekToPreviousFrame( GetRateFromScale(),in_segment);
			}
		}
		
		public void UpdateSegmentStartTime (long start){
			segmentStartTime = start;
			player.SegmentStartUpdate(start, GetRateFromScale());						
		}
		
		public void UpdateSegmentStopTime (long stop){
			segmentStopTime = stop;
			player.SegmentStopUpdate(stop, GetRateFromScale());	
		}
		
		public void SetStartStop(long start, long stop){
			segmentStartTime = start;
			segmentStopTime = stop;
			closebutton.Show();
			vscale1.Value = 25;
			player.SegmentSeek(start,stop, GetRateFromScale());	
			player.Play();
		}
		
		public void CloseActualSegment(){
			closebutton.Hide();
			hasNext = false;
			segmentStartTime = 0;
			segmentStopTime = 0;
			vscale1.Value=25;
			//timescale.Sensitive = true;
			slength = TimeString.MSecondsToSecondsString(length);
			SegmentClosedEvent();
			player.CancelProgramedStop();			
		}
		
		public void SetSensitive(){
			controlsbox.Sensitive = true;
			vscale1.Sensitive = true;					
		}
		
		public void UnSensitive(){			
			controlsbox.Sensitive = false;
			vscale1.Sensitive = false;				
		}		
		
#endregion
		
#region Private methods
		
		private float GetRateFromScale(){
			VScale scale= vscale1;
			double val = scale.Value;
			
			if (val >25 ){
				val = val-25 ;					
			}
			else if (val <=25){			
				val = val/25;
			}
			return (float)val;
		}
		
		private bool InSegment(){
			return  !(segmentStopTime == 0 && segmentStartTime ==0) ;
		}
		
		private void PlayerInit(){
			MultimediaFactory factory;
			Widget playerWidget;
			
			factory= new MultimediaFactory();
			player = factory.getPlayer(320,280);
			
			tickHandler = new TickHandler(OnTick);
			player.Tick += tickHandler;
			player.StateChange += new LongoMatch.Video.Handlers.StateChangeHandler(OnStateChanged);
			player.Eos += new EventHandler (OnEndOfStream);
			player.Error += new ErrorHandler (OnError);
			
			playerWidget = (Widget)player;
			playerWidget.Show();
			videobox.Add(playerWidget);			
		}
		
#endregion
		
#region Callbacks
		protected virtual void OnStateChanged(object o, LongoMatch.Video.Handlers.StateChangeArgs args){
			if (args.Playing){
				playbutton.Hide();
				pausebutton.Show();
			}
			else{
				playbutton.Show();
				pausebutton.Hide();
			}
		}
		
		protected virtual void OnTick(object o,TickArgs args){
			long currentTime = args.CurrentTime;
			float currentposition = args.CurrentPosition;		
			long streamLength = args.StreamLength;		
			bool seekable = args.Seekable;
			
			//Console.WriteLine ("Current Time:{0}\n Length:{1}\n",currentTime, streamLength);
			if (length != streamLength){							
				length = streamLength;
				slength = TimeString.MSecondsToSecondsString(length);				
			}
			
			if  (InSegment()){
				currentTime -= segmentStartTime;
				currentposition = (float)currentTime/(float)(segmentStopTime-segmentStartTime);
				slength = TimeString.MSecondsToSecondsString(segmentStopTime-segmentStartTime);
			}						
			
			timelabel.Text = TimeString.MSecondsToSecondsString(currentTime) + "/" + slength;			    
			timescale.Value = currentposition;
			if (Tick != null)
				Tick(o,args);
			
		}
		
		protected virtual void OnTimescaleAdjustBounds(object o, Gtk.AdjustBoundsArgs args)
		{
			float pos;		
				
			if (!seeking){
				seeking = true;
				IsPlayingPrevState = player.Playing;
				player.Tick -= tickHandler;
				player.Pause();
			}
				
			pos = (float)timescale.Value;			
			
			if (InSegment()){
				long seekPos = segmentStartTime + (long)(pos*(segmentStopTime-segmentStartTime));
				player.SeekInSegment(seekPos, GetRateFromScale());	
				timelabel.Text= TimeString.MSecondsToSecondsString(seekPos) + "/" + 
					TimeString.MSecondsToSecondsString(segmentStopTime-segmentStartTime);
			}
			else {
				player.Position = pos;
				timelabel.Text= TimeString.MSecondsToSecondsString(player.CurrentTime) + "/" + slength;
				Rate = 1;				
			}			
		}		

		protected virtual void OnTimescaleValueChanged(object sender, System.EventArgs e)
		{
			if (seeking){
				seeking=false;				
				player.Tick += tickHandler;
				if (IsPlayingPrevState)
					player.Play();
			}
		}

		protected virtual void OnPlaybuttonClicked(object sender, System.EventArgs e)
		{
			  Play();		
		}

		protected virtual void OnStopbuttonClicked(object sender, System.EventArgs e)
		{
			player.SeekTime(segmentStartTime,1,true);
		}

		protected virtual void OnVolumebuttonClicked(object sender, System.EventArgs e)
		{
			vwin.SetLevel(player.Volume);
			vwin.Show();
		}

		protected virtual void OnDestroyEvent(object o, Gtk.DestroyEventArgs args)
		{
			player.Dispose();
		}
		
		protected virtual void OnVolumeChanged(double level){
			player.Volume = level;
			if (level == 0)
				muted = true;
			else
				muted = false;
		}

		protected virtual void OnPausebuttonClicked (object sender, System.EventArgs e)
		{			
			player.Pause();
		}
		
		protected virtual void OnEndOfStream (object o, EventArgs args){
			player.SeekInSegment(0, GetRateFromScale());
			player.Pause();			
		}
		
				
		protected virtual void OnError (object o, ErrorArgs args){
			if(Error != null)
				Error(o,args);
		}

		protected virtual void OnClosebuttonClicked (object sender, System.EventArgs e)
		{
			CloseActualSegment();	
		}

		protected virtual void OnPrevbuttonClicked (object sender, System.EventArgs e)
		{			
			if (Prev != null)
				Prev();
		}

		protected virtual void OnNextbuttonClicked (object sender, System.EventArgs e)
		{
			if (Next != null)
				Next();		
		}

		protected virtual void OnVscale1FormatValue (object o, Gtk.FormatValueArgs args)
		{
			double val = args.Value;
			if (val >25 ){
				val = val-25 ;
				args.RetVal = val +"X";
			}
			else if (val ==25){
				args.RetVal = "1X";
			}
			else if (val <25){
				args.RetVal = "-"+val+"/25"+"X";
			}
		}

		protected virtual void OnVscale1ValueChanged (object sender, System.EventArgs e)
		{
			float val = GetRateFromScale();
			
			// Mute for rate != 1
			if (val != 1 && player.Volume != 0){ 
				previousVLevel = player.Volume;
				player.Volume=0;
			}
			else if  (val != 1 && muted)
			          previousVLevel = 0;			
			else if (val ==1)
				player.Volume = previousVLevel;			
			
			if (InSegment()){
				player.SetRateInSegment(val,segmentStopTime);
			}
			else
				player.SetRate(val);			
			rate = val;
		}

		protected virtual void OnVideoboxButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if (!player.Playing)
				Play();
			else 
				Pause();		
		}
#endregion		
	}
}
