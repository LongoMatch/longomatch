// PlayerBin.cs 
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
		
	public partial class PlayerBin : Gtk.Bin
	{
		
		public event PlayListSegmentDoneHandler PlayListSegmentDoneEvent;
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
		private int previousVLevel = 1;
		private bool muted=false;
		//the player.mrl is diferent from the filenameas it's an uri eg:file:///foo.avi
		private string filename = null;
		protected VolumeWindow vwin;
		
		


	

		
		
		public PlayerBin()
		{		
			this.Build();
			this.UnSensitive();
			this.PlayerInit();
			vwin = new VolumeWindow();
			vwin.VolumeChanged += new VolumeChangedHandler(OnVolumeChanged);
			this.controlsbox.Visible = false;
		
						
		}
		
		private void PlayerInit(){
			PlayerMaker pMaker;
			Widget _videoscreen;
			
			pMaker = new PlayerMaker();
			player = pMaker.getPlayer(320,280);
			
			//IF error do something	
			tickHandler = new TickHandler(OnTick);
			player.Tick += tickHandler;
			player.StateChanged += new LongoMatch.Video.Handlers.StateChangedHandler(OnStateChanged);
			player.Eos += new EventHandler (OnEndOfStream);
			player.SegmentDoneEvent += new SegmentDoneHandler (OnSegmentDone);
			player.Error += new ErrorHandler (OnError);
			
			_videoscreen = player.Window;			
			videobox.Add(_videoscreen);
			_videoscreen.Show();
		
		}
		
		public void Open (string mrl){
			this.filename = mrl;
			this.ResetGui();
			this.CloseActualSegment();			
				try{
					player.Open(mrl);
				}
				catch (GLib.GException error) {
				//We handle this error async
				
				}
		}
		
		public void Play(){
			
			player.Play();
			
			float val = this.getRate();			
						
				if (this.segmentStartTime == 0 && this.segmentStopTime==0)
					player.SetRate(val);
				else
					player.SetRateInSegment(val,segmentStopTime);		
		}
		
		public void Pause(){
			player.Pause();
		}
		
		public IPlayer Player{
			get{return this.player;}
		}
		
		public long AccurateCurrentTime{
			get{return this.player.AccurateCurrentTime;}
		}
		
		public long CurrentTime{
			get{return this.player.CurrentTime;}
		}
		
		public long StreamLength{
			get{				
				 return player.StreamLength;				
			}
		}
		
		public bool FullScreen{
			set{
				if (value)
					this.GdkWindow.Fullscreen();
				else 
					this.GdkWindow.Unfullscreen();
				
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
					rate = w/h;
					return pixbuf.ScaleSimple(THUMBNAIL_WIDTH,(int)(THUMBNAIL_WIDTH/rate),InterpType.Bilinear);
				}
				else return null;
			}
		}
		
		public bool LogoMode {
			get{
				return this.player.LogoMode;
			}
			set{
				this.player.LogoMode = value;
			}
		}
		
		public bool PlaylistMode{
			set{				
				//this.timescale.Sensitive = !value;				
			}
		}
		
		public void SetLogo (string filename){
			this.player.Logo=filename;
		}
		
		public void ResetGui(){
			this.closebutton.Hide();
			this.SetSensitive();
			timescale.Value=0;
			timelabel.Text="";
			this.player.CancelProgramedStop();			
		}
	
		public void SetPlayListElement(string fileName,long start, long stop, bool hasNext){
			this.PlaylistMode = true;
			this.hasNext = hasNext;
			if (hasNext)
				this.nextbutton.Sensitive = true;
			else
				this.nextbutton.Sensitive = false;
			if (fileName != this.filename){
				this.Open(fileName);				
				player.NewFileSeek(start,stop);		
				Play();
			}
			else player.SegmentSeek(start,stop);			
			this.segmentStartTime = start;
			this.segmentStopTime = stop;
			player.LogoMode = false;
			this.OnVscale1ValueChanged(this.vscale1,new EventArgs());

			
		}
		
		public void Close(){
			this.player.Close();
			this.filename = null;
			this.timescale.Value = 0;
			this.UnSensitive();
		}
		
		
		
		
		
		public void SeekTo(long time, bool accurate){
			this.player.SeekTo(time,accurate);
		}
		
		public void SeekInSegment(long pos){
			player.SeekInSegment(pos);
		}
		
		public void SeekToNextFrame(bool in_segment){
		
			player.SeekToNextFrame(in_segment);
		}
		
		public void SeekToPreviousFrame(bool in_segment){

			player.SeekToPreviousFrame(in_segment);

		}
		
		public void UpdateSegmentStartTime (long start){
			this.segmentStartTime = start;
			player.UpdateSegmentStartTime(start);						
		}
		
		public void UpdateSegmentStopTime (long stop){
			this.segmentStopTime = stop;
			player.UpdateSegmentStopTime(stop);
			
		}
		
		public void SetStartStop(long start, long stop){

			this.segmentStartTime = start;
			this.segmentStopTime = stop;
			this.closebutton.Show();
			//this.timescale.Sensitive = false;
			this.vscale1.Value = 25;
			player.SegmentSeek(start,stop);
		
			
		}
		
		public void CloseActualSegment(){
			this.closebutton.Hide();
			this.hasNext = false;
			this.segmentStartTime = 0;
			this.segmentStopTime = 0;
			this.vscale1.Value=25;
			//this.timescale.Sensitive = true;
			this.slength = TimeString.MSecondsToSecondsString(length);
			this.SegmentClosedEvent();
			this.player.CancelProgramedStop();
			
		}
		
		public void SetSensitive(){
			this.controlsbox.Sensitive = true;
			this.vscale1.Sensitive = true;
					
		}
		
		public void UnSensitive(){			
			this.controlsbox.Sensitive = false;
			this.vscale1.Sensitive = false;
				
		}
		
		private float getRate(){
			VScale scale= this.vscale1;
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
			return segmentStartTime != 0 && segmentStopTime != 0;
		}
		
		protected virtual void OnStateChanged(object o, LongoMatch.Video.Handlers.StateChangedArgs args){
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
			if (this.length != streamLength){							
				this.length = streamLength;
				this.slength = TimeString.MSecondsToSecondsString(length);				
			}
			
			if  (InSegment()){
				currentTime -= segmentStartTime;
				currentposition = (float)currentTime/(float)(segmentStopTime-segmentStartTime);
				this.slength = TimeString.MSecondsToSecondsString(segmentStopTime-segmentStartTime);
			}						
			
			timelabel.Text = TimeString.MSecondsToSecondsString(currentTime) + "/" + slength;			    
			timescale.Value = currentposition*65535;
			if (Tick != null)
				this.Tick(o,args);
			
		}
		
		protected virtual void OnTimescaleAdjustBounds(object o, Gtk.AdjustBoundsArgs args)
		{
			float pos;
				
			if (!seeking)
				seeking = true;
			this.IsPlayingPrevState = player.Playing;
			player.Tick -= this.tickHandler;
			if (Environment.OSVersion.Platform != PlatformID.Win32NT){
				player.Pause();
				}
			
			pos = (float)timescale.Value/65535;
			
			if (InSegment()){
				player.SeekInSegment(segmentStartTime + (long)(pos*(segmentStopTime-segmentStartTime)));
			}
			else {
				player.Position = pos;
				timelabel.Text= TimeString.MSecondsToSecondsString(player.CurrentTime) + "/" + this.slength;
			}
			
			
		}
		

		protected virtual void OnTimescaleValueChanged(object sender, System.EventArgs e)
		{
			if (seeking){
				seeking=false;
				player.Tick += this.tickHandler;
				if (IsPlayingPrevState)
					Play();
			}
		}

		protected virtual void OnPlaybuttonClicked(object sender, System.EventArgs e)
		{
			  Play();		
		}

		protected virtual void OnStopbuttonClicked(object sender, System.EventArgs e)
		{
			player.SeekTo(this.segmentStartTime,true);

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
		
		protected virtual void OnVolumeChanged(int level){
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
			player.SeekInSegment(0);
			player.Pause();
			
		}
		
		protected virtual void OnSegmentDone (){
			if (this.hasNext && this.PlayListSegmentDoneEvent != null )
				PlayListSegmentDoneEvent();
			
				
		}
		
		protected virtual void OnError (object o, ErrorArgs args){
			if(this.Error != null)
				this.Error(o,args);
		}
		
	

		protected virtual void OnClosebuttonClicked (object sender, System.EventArgs e)
		{
			this.CloseActualSegment();	
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
			float val = this.getRate();
			
			// Mute for rate != 1
			if (val != 1 && player.Volume != 0){ 
				previousVLevel = player.Volume;
				player.Volume=0;
			}
			else if  (val != 1 && muted)
			          previousVLevel = 0;			
			else if (val ==1)
				player.Volume = previousVLevel;
			
			
			if (InSegment())
				player.SetRate(val);
			else
				player.SetRateInSegment(val,segmentStopTime);	
			
		}

		protected virtual void OnVideoboxButtonPressEvent (object o, Gtk.ButtonPressEventArgs args)
		{
			if (!player.Playing)
				Play();
			else 
				Pause();		
		}


		
	}
}
