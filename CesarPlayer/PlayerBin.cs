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
using Mono.Unix;
using System.Runtime.InteropServices;

namespace CesarPlayer
{
	
	
	public partial class PlayerBin : Gtk.Bin
	{
		
		public event PlayListSegmentDoneHandler PlayListSegmentDoneEvent;
		public event SegmentClosedHandler SegmentClosedEvent;
		public event TickEventHandler TickEvent;
		public event ErrorEventHandler ErrorEvent;
		
		private TickEventHandler tickEventHandler;
		private IPlayer player;
		private long length=0;
		private string slength;
		private long segmentStartTime;
		private bool hasNext;
		private bool seeking=false;
		private bool IsPlayingPrevState = false;
		protected VolumeWindow vwin;


	

		
		
		public PlayerBin()
		{	
	
			this.Build();
			this.UnSensitive();
			this.PlayerInit();
			vwin = new VolumeWindow();
			vwin.VolumeChanged += new VolumeChangedHandler(OnVolumeChanged);
			
			
		}
		
		private void PlayerInit(){
			PlayerMaker pMaker = new PlayerMaker();
			player = pMaker.getPlayer(320,280);
	
			tickEventHandler = new TickEventHandler(OnTick);
			player.TickEvent += tickEventHandler;
			player.StateChanged += new StateChangedHandler(OnStateChanged);
			player.EndOfStreamEvent += new EndOfStreamEventHandler (OnEndOfStream);
			player.SegmentDoneEvent += new SegmentDoneHandler (OnSegmentDone);
			player.ErrorEvent += new ErrorEventHandler (OnError);
			Widget _videoscreen = player.Window;
			videobox.Add(_videoscreen);
			_videoscreen.Show();
		}
		
		public string File {
			set{
				this.closebutton.Hide();
				this.SetSensitive();
				timescale.Value=0;
				timelabel.Text="";
				this.SegmentClosedEvent();
				this.player.CancelProgramedStop();
				player.FilePath = value;
				player.Play();
			}
			get{
				return player.FilePath;
			}
			
			
			
		}
		
		public IPlayer Player{
			get {return player;}
		}
		
		public void SetLogo (string filename){
			this.player.SetLogo(filename);
		}
		
		public void SetPlayListElement(string fileName,long start, long stop, bool hasNext){
			/*if (this.player.FilePath != fileName){
				//Cambiar archivo
			}*/

			this.hasNext = hasNext;
			if (hasNext)
				this.nextbutton.Sensitive = true;
			else
				this.nextbutton.Sensitive = false;
				
			this.SetStartStop(start, stop);
			
		}
		
		public void UpdateSegmentStartTime (long start){
			this.segmentStartTime = start;
			player.UpdateSegmentStartTime(start);
			
			
		}
		
		public void UpdateSegmentStopTime (long stop){
			player.UpdateSegmentStopTime(stop);
			
		}
		
		public void SetStartStop(long start, long stop){
			this.segmentStartTime = start;
			this.closebutton.Show();
			this.timescale.Sensitive = false;
			player.SegmentSeek(start,stop);
			player.Rate = 0.2f;
			
		}
		
		public void SetSensitive(){
			this.controlsbox.Sensitive = true;
					
		}
		
		public void UnSensitive(){			
			this.controlsbox.Sensitive = false;
				
		}
		
		protected virtual void OnStateChanged(bool playing){
			if (playing){
				playbutton.Hide();
				pausebutton.Show();
			}
			else{
				playbutton.Show();
				pausebutton.Hide();
			}
		}
		
		protected virtual void OnTick(long currentTime, long streamLength, float position, bool seekable){
			//Console.WriteLine ("Current Time:{0}\nCurrent Pos:{1}\nStream Length:{2}\n",currentTime,position, streamLength);
			if (this.length != streamLength){				
				this.length = streamLength;
				this.slength = TimeString.MSecondsToSecondsString(length);
			}
			else {	
			    timelabel.Text = TimeString.MSecondsToSecondsString(currentTime) + "/" + slength;
				timescale.Value = position*65535;
				if (TickEvent != null)
					this.TickEvent(currentTime,streamLength,position,seekable);
			}
			
			
			
		}
		protected virtual void OnTimescaleAdjustBounds(object o, Gtk.AdjustBoundsArgs args)
		{
			if (!seeking){
				seeking = true;
				this.IsPlayingPrevState = player.Playing;
				player.TickEvent -= this.tickEventHandler;
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
				player.TickEvent += this.tickEventHandler;
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
			player.Seek(this.segmentStartTime,true);

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
		
		protected virtual void OnEndOfStream (){
			player.Stop();
			player.LogoMode = true;
		}
		
		protected virtual void OnSegmentDone (){
			if (this.hasNext && this.PlayListSegmentDoneEvent != null )
				PlayListSegmentDoneEvent();
			
				
		}
		
		protected virtual void OnError (String error){
			if(this.ErrorEvent != null)
				this.ErrorEvent(error);
		}

		protected virtual void OnClosebuttonClicked (object sender, System.EventArgs e)
		{
			this.closebutton.Hide();
			this.hasNext = false;
			this.segmentStartTime = 0;
			this.timescale.Sensitive = true;
			this.SegmentClosedEvent();
			this.player.CancelProgramedStop();
		}

		protected virtual void OnPrevbuttonClicked (object sender, System.EventArgs e)
		{
			player.SeekInSegment(this.segmentStartTime);
			player.Play();
		}

		
	}
}
