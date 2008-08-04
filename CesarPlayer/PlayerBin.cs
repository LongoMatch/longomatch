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

namespace CesarPlayer
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
			
			PlayerMaker pMaker = new PlayerMaker();
			player = pMaker.getPlayer(320,280);
			//IF error do something	
			tickHandler = new TickHandler(OnTick);
			player.Tick += tickHandler;
			player.StateChanged += new StateChangedHandler(OnStateChanged);
			player.Eos += new EventHandler (OnEndOfStream);
			player.SegmentDoneEvent += new SegmentDoneHandler (OnSegmentDone);
			player.Error += new ErrorHandler (OnError);
			Widget _videoscreen = player.Window;
			videobox.Add(_videoscreen);
			_videoscreen.Show();
		
		}
		
		public string File {
			set{
				this.filename = value;
				this.ResetGui();
				player.Open(value);
			
			}
			get{
				return this.filename;
			}
			
			
			
		}
		
		public IPlayer Player{
			get {return player;}
		}
		
		public bool FullScreen{
			set{
				if (value)
					this.GdkWindow.Fullscreen();
				else 
					this.GdkWindow.Unfullscreen();
			}
		}
		public Pixbuf CurrentThumbnail{
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
			
			this.hasNext = hasNext;
			if (hasNext)
				this.nextbutton.Sensitive = true;
			else
				this.nextbutton.Sensitive = false;
			if (fileName != this.File){
				this.File = fileName;				
				player.NewFileSeek(start,stop);		
				player.Play();
			}
			else player.SegmentSeek(start,stop);
						
			this.segmentStartTime = start;
			this.segmentStopTime = stop;
			player.LogoMode = false;
			

			
		}
		
		public void Close(){
			this.Player.Close();
			this.filename = null;
			this.timescale.Value = 0;
			this.UnSensitive();
		}
		
		public void Pause(){
			Player.Pause();
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
			this.timescale.Sensitive = false;
			this.vscale1.Value = 25;
			player.SegmentSeek(start,stop);
		
			
		}
		
		public void CloseActualSegment(){
			this.closebutton.Hide();
			this.hasNext = false;
			this.segmentStartTime = 0;
			this.segmentStopTime = 0;
			this.timescale.Value = 0;
			this.timescale.Sensitive = true;
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
		
		protected virtual void OnStateChanged(object o, StateChangedArgs args){
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
			long streamLength = args.StreamLength;
			float currentposition = args.CurrentPosition;
			bool seekable = args.Seekable;
			
			//Console.WriteLine ("Current Time:{0}\nCurrent Pos:{1}\nStream Length:{2}\n",currentTime,position, streamLength);
			if (this.length != streamLength){				
				this.length = streamLength;
				this.slength = TimeString.MSecondsToSecondsString(length);
			}
			else if (seekable) {	
			    timelabel.Text = TimeString.MSecondsToSecondsString(currentTime) + "/" + slength;
				timescale.Value = currentposition*65535;
				if (Tick != null)
					this.Tick(o,args);
			}
			
			
			
		}
		protected virtual void OnTimescaleAdjustBounds(object o, Gtk.AdjustBoundsArgs args)
		{
			if (!seeking){
				seeking = true;
				this.IsPlayingPrevState = player.Playing;
				player.Tick -= this.tickHandler;
				player.Pause();

			}
			
			    float pos = (float)timescale.Value/65535;
				player.Position = pos;
				timelabel.Text= TimeString.MSecondsToSecondsString(player.CurrentTime) + "/" + this.slength;
				
			
		}

		protected virtual void OnTimescaleValueChanged(object sender, System.EventArgs e)
		{
			if (seeking){
				seeking=false;
				player.Tick += this.tickHandler;
				if (IsPlayingPrevState)
					player.Play();
			}
		}

		protected virtual void OnPlaybuttonClicked(object sender, System.EventArgs e)
		{
			player.TogglePlay();
			
			
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
			
		}

		protected virtual void OnPausebuttonClicked (object sender, System.EventArgs e)
		{
			player.TogglePlay();
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
			player.SeekInSegment(this.segmentStartTime);
			player.Play();
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
			Console.WriteLine(args.Value);
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
			VScale scale= (VScale)sender;
			double val = scale.Value;
			if (val >25 ){
				val = val-25 ;	
				if (this.segmentStartTime == 0 && this.segmentStopTime==0)
					player.SetRate((float)val);
				else
					player.SetRateInSegment((float) val,segmentStopTime);
				
			}
			else if (val <=25){			
				if (this.segmentStartTime == 0 && this.segmentStopTime==0)
					player.SetRate((float)(val/25));
				else
					player.SetRateInSegment((float)(val/25),segmentStopTime);
			}
			
				
			
		}

		
	}
}
