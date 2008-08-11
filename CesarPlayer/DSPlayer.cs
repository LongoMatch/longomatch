using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using DirectShowLib;
using System.Runtime.InteropServices;
using System.Timers;
using Gtk;
using Gdk;



namespace CesarPlayer
{
    internal enum PlayState
    {
        Stopped,
        Paused,
        Running,
        Init
    };
	
	
    public class DSPlayer : UserControl, IPlayer, IMetadataReader
    {
		
				// Events

		public event         StateChangedHandler StateChanged;
		public event         TickHandler Tick;
		public event         System.EventHandler Eos;
		public event         SegmentDoneHandler SegmentDoneEvent;		
		public event         CesarPlayer.ErrorHandler Error;
		public event         System.EventHandler GotDuration;
		public event         System.EventHandler SegmentDone;
		
		
        private const int WMGraphNotify = 0x0400 + 13;
        private const int VolumeFull = 0;
        private const int VolumeSilence = -10000;
        private const int MS = 10000;

        private IGraphBuilder graphBuilder = null;
        private IMediaControl mediaControl = null;
        private IMediaEventEx mediaEventEx = null;
        private DirectShowLib.IMediaEvent mediaEvent = null;
        private IVideoWindow videoWindow = null;
        private IBasicAudio basicAudio = null;
        private IBasicVideo basicVideo = null;
        private IMediaSeeking mediaSeeking = null;
        private IMediaPosition mediaPosition = null;
        private IVideoFrameStep frameStep = null;

        private long clipLength;
        private double aspectRatio;
		private int currentVolume;
		private float currentPosition;
		private long currentTime;
        
        private string filename = string.Empty;
        private bool isFullScreen = false;
        private PlayState currentState = PlayState.Stopped;
        private double currentPlaybackRate = 1.0;
        private IntPtr hDrain = IntPtr.Zero;

        private UseType type;
        
		private Widget gtkDrawingWindow;
		private System.Windows.Forms.Panel videoPanel;

		
		private System.Timers.Timer timer;
		
        public bool Open (string filePath)
        {
			
			// Reset status variables
			this.currentState = PlayState.Stopped;
			StateChangedArgs args = new StateChangedArgs();
			args.Args = new object[1];
			args.Args[0] = false ;
			if (this.StateChanged != null)
				this.StateChanged((object)this,args);
			this.currentVolume = VolumeFull;
			
			//Set file name				
			this.filename = filePath;
			
			// Start playing the media file
			InitializePlayer(filePath);
			
			return true;
			
					

        }

        private void InitializePlayer(String filename)
        {
            int hr = 0;

            if (filename == string.Empty)
                return;

            this.graphBuilder = (IGraphBuilder)new FilterGraph();

            // Have the graph builder construct its the appropriate graph automatically
            hr = this.graphBuilder.RenderFile(filename, null);
            DsError.ThrowExceptionForHR(hr);

            // QueryInterface for DirectShow interfaces
            this.mediaControl = (IMediaControl)this.graphBuilder;
            this.mediaEventEx = (IMediaEventEx)this.graphBuilder;
            this.mediaSeeking = (IMediaSeeking)this.graphBuilder;
            this.mediaPosition = (IMediaPosition)this.graphBuilder;

          
			// Query for video interfaces, which may not be relevant for audio files
			this.videoWindow = this.graphBuilder as IVideoWindow;
			this.basicVideo = this.graphBuilder as IBasicVideo;
			
			// Query for audio interfaces, which may not be relevant for video-only files
			this.basicAudio = this.graphBuilder as IBasicAudio;
			
			
			// Have the graph signal event via window callbacks for performance
			hr = this.mediaEventEx.SetNotifyWindow(this.Handle, WMGraphNotify, IntPtr.Zero);
			DsError.ThrowExceptionForHR(hr);
			
			// Setup the video window
			hr = this.videoWindow.put_Owner(this.videoPanel.Handle);
			//this.gtkDrawingWindow = new GtkWin32EmbedWidget ( this.userControl);
			Gdk.Window b = Gdk.Window.ForeignNew((uint)this.Handle); 
			b.Reparent(this.gtkDrawingWindow.GdkWindow,0,0);
			b.Show();
			
			
			DsError.ThrowExceptionForHR(hr);
			
			hr = this.videoWindow.put_WindowStyle(WindowStyle.Child | WindowStyle.ClipSiblings | WindowStyle.ClipChildren);
			DsError.ThrowExceptionForHR(hr);
			
            // Read the default video size and get the aspect ratio
            int lWidth;
            int lHeight;
            hr = this.basicVideo.GetVideoSize(out lWidth, out lHeight);
            DsError.ThrowExceptionForHR(hr);
            aspectRatio = (double)lWidth / (double)lHeight;
                    
            MoveVideoWindow();
            GetFrameStepInterface();
                       
            this.isFullScreen = false;
            this.currentPlaybackRate = 1.0;
         
            // Get duration of the clip
            getDuration();

            // Run the graph to play the media file
            
            	this.Play();
            
        }



     

        private void MoveVideoWindow()
        {
            int hr = 0;
            int pHeight;
            int pWidth;
            // Track the movement of the container window and resize as needed
            if (this.videoWindow != null)
            {
                pHeight=videoPanel.ClientRectangle.Height;
                pWidth = videoPanel.ClientRectangle.Width;
                
                if ((double)pWidth/(double)pHeight > aspectRatio)
                {
                    
                    // Heigth still constant, Width changed to fix aspect ratio
                    int fWidth = (int)(pHeight * aspectRatio);
                    // Center the image
                    int wOffset=(pWidth - fWidth) / 2;
                    hr = this.videoWindow.SetWindowPosition(
                      videoPanel.ClientRectangle.Left+wOffset,
                      videoPanel.ClientRectangle.Top,
                      fWidth,
                      pHeight
                      );
                    DsError.ThrowExceptionForHR(hr);
                }
                else
                {
                    // Width still constant, Height changed to fix aspect ratio
                    int fHeight = (int)(pWidth / aspectRatio);
                    
                    // Center the image
                    int hOffset = (pHeight - fHeight) / 2;
                    hr = this.videoWindow.SetWindowPosition(
                     videoPanel.ClientRectangle.Left,
                     videoPanel.ClientRectangle.Top + hOffset,
                     pWidth ,
                     fHeight
                     );
                    DsError.ThrowExceptionForHR(hr);
                }
            }
        }

       
        //
        // Some video renderers support stepping media frame by frame with the
        // IVideoFrameStep interface.  See the interface documentation for more
        // details on frame stepping.
        //
        private bool GetFrameStepInterface()
        {
            int hr = 0;

            IVideoFrameStep frameStepTest = null;

            // Get the frame step interface, if supported
            frameStepTest = (IVideoFrameStep)this.graphBuilder;

            // Check if this decoder can step
            hr = frameStepTest.CanStep(0, null);
            if (hr == 0)
            {
                this.frameStep = frameStepTest;
                return true;
            }
            else
            {
                Marshal.ReleaseComObject(frameStepTest);
                return false;
            }
        }

        private void CloseInterfaces()
        {
            int hr = 0;

            try
            {
                lock (this)
                {
                    // Relinquish ownership (IMPORTANT!) after hiding video window
                  
                        hr = this.videoWindow.put_Visible(OABool.False);
                        DsError.ThrowExceptionForHR(hr);
                        hr = this.videoWindow.put_Owner(IntPtr.Zero);
                        DsError.ThrowExceptionForHR(hr);
                    

                    if (this.mediaEventEx != null)
                    {
                        hr = this.mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
                        DsError.ThrowExceptionForHR(hr);
                    }


                    // Release and zero DirectShow interfaces
                    if (this.mediaEventEx != null)
                        this.mediaEventEx = null;
                    if (this.mediaSeeking != null)
                        this.mediaSeeking = null;
                    if (this.mediaPosition != null)
                        this.mediaPosition = null;
                    if (this.mediaControl != null)
                        this.mediaControl = null;
                    if (this.basicAudio != null)
                        this.basicAudio = null;
                    if (this.basicVideo != null)
                        this.basicVideo = null;
                    if (this.videoWindow != null)
                        this.videoWindow = null;
                    if (this.frameStep != null)
                        this.frameStep = null;
                    if (this.graphBuilder != null)
                        Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null;

                    System.GC.Collect();
                }
            }
            catch
            {
            }
        }

        /*
         * Media Related methods
         */

        public void TogglePlay()
        {
            if (this.mediaControl == null)
                return;

            // Toggle play/pause behavior
            if ((this.currentState == PlayState.Paused) || (this.currentState == PlayState.Stopped))
            {
                if (this.mediaControl.Run() >= 0){
                    this.currentState = PlayState.Running;
					StateChangedArgs args = new StateChangedArgs();
					args.Args = new object[1];
					args.Args[0] = true ;
					if (this.StateChanged != null)						
						this.StateChanged((object)this,args);

					// Start Timer
					timer.Start();
				}
            }
            else
            {
                if (this.mediaControl.Pause() >= 0){
                    this.currentState = PlayState.Paused;
					StateChangedArgs args = new StateChangedArgs();
					args.Args = new object[1];
					args.Args[0] = false ;
					if (this.StateChanged != null)
					this.StateChanged((object)this,args);					
					// Stop Timer
					timer.Stop();
				}
            }

            
        }

        public void Pause()
        {
            if (this.mediaControl == null)
                return;

            if (this.currentState == PlayState.Running)
            {
                if (this.mediaControl.Pause() >= 0){
                    this.currentState = PlayState.Paused;
					StateChangedArgs args = new StateChangedArgs();
					args.Args = new object[1];
					args.Args[0] = false ;
					if (this.StateChanged != null)
					this.StateChanged((object)this,args);
					// Stop Timer
					timer.Stop();
				}
            }
        }
        public bool Play()
        {
            if (this.mediaControl == null)
                return false;

            if ((this.currentState == PlayState.Paused) || (this.currentState == PlayState.Stopped))
            {
                if (this.mediaControl.Run() >= 0){
                    this.currentState = PlayState.Running;
					StateChangedArgs args = new StateChangedArgs();
					args.Args = new object[1];
					args.Args[0] = true ;
					if (this.StateChanged != null)
					this.StateChanged((object)this,args);

					// Start Timer
					timer.Start();
				}
            }
			return true;
        }
   
        public void Stop()
        {
            int hr = 0;
            DsLong pos = new DsLong(0);

            if ((this.mediaControl == null) || (this.mediaSeeking == null))
                return;

            // Stop and reset postion to beginning
            if ((this.currentState == PlayState.Paused) || (this.currentState == PlayState.Running))
            {
                hr = this.mediaControl.Stop();
                DsError.ThrowExceptionForHR(hr);
                this.currentState = PlayState.Stopped;
				StateChangedArgs args = new StateChangedArgs();
				args.Args = new object[1];
				args.Args[0] = false ;
if (this.StateChanged != null)
				this.StateChanged((object)this,args);


                // Seek to the beginning
                hr = this.mediaSeeking.SetPositions(pos, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
                DsError.ThrowExceptionForHR(hr);

                // Display the first frame to indicate the reset condition
                hr = this.mediaControl.Pause();
                DsError.ThrowExceptionForHR(hr);

			
            }
            
        }
		
		public void Close(){
			this.Stop();
			this.CloseInterfaces();
		}
		

		public void UpdateSegmentStartTime(long start){
			int hr = 0;
            hr = this.mediaSeeking.SetPositions(start*MS, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
		    DsError.ThrowExceptionForHR(hr);
		
		}
		
		public void UpdateSegmentStopTime(long stop){
			int hr = 0;
            hr = this.mediaSeeking.SetPositions(null, AMSeekingSeekingFlags.NoPositioning, stop*MS, AMSeekingSeekingFlags.AbsolutePositioning);
		    DsError.ThrowExceptionForHR(hr);
		}
		
        private void getDuration()
        {
            
            int hr = 0;
            long length;
            hr = this.mediaSeeking.GetDuration(out length);
            this.clipLength = length/MS; 
            DsError.ThrowExceptionForHR(hr);

           
        }
		
		public string Mrl{
			
			get{return this.filename;}
		}
		
		
		public float Position{
			get { 
        		this.updateTime();
        		return currentPosition;
        	}
			set { 
        		this.SeekTo((long)(value*this.clipLength),false);
        	}
			
		}
		
		public long CurrentTime {
			get { this.updateTime();
        		return currentTime;
        	}
			
		}
		
		public long AccurateCurrentTime{
			get {return this.CurrentTime;}
		}
		
		public bool LogoMode{
			set{}
			get{ return false;}
		}
		
		public string Logo{
			set{}
		}
		
		public Pixbuf CurrentFrame{
			get{return null;}
		}
		
		public void CancelProgramedStop(){
			int hr = 0;
            hr = this.mediaSeeking.SetPositions(null, AMSeekingSeekingFlags.NoPositioning, this.StreamLength*MS, AMSeekingSeekingFlags.AbsolutePositioning);
		    DsError.ThrowExceptionForHR(hr);
		}
		
		public long StreamLength {
			get{return this.clipLength;}
		}
		
		public bool Playing {
			get{return this.currentState == PlayState.Running;}
		}
		
		public int Volume {
			get{
				return this.currentVolume+100;
			}
			
			set{
				int hr = 0;

				if ((this.graphBuilder == null) || (this.basicAudio == null) || value >100 || value <0)
					return;

				// Read current volume
				hr = this.basicAudio.get_Volume(out this.currentVolume);
				if (hr == -1) //E_NOTIMPL
				{
					// Fail quietly if this is a video-only media file
					return;
				}
				else if (hr < 0)
				{
					return;
				}

				// Switch volume levels
				this.currentVolume = (value-100)*100;

				// Set new volume
				hr = this.basicAudio.put_Volume(this.currentVolume);

			}
		}
        public object GetMetadata(GstPlayerMetadataType type){
        	if (type == GstPlayerMetadataType.Duration){
        		this.getDuration();
        		return this.clipLength;
        	}
        	else return null;
        		
        }
        private void updateTime()
        {
			
				int hr=0;
				long cTime;
				hr = this.mediaSeeking.GetCurrentPosition(out cTime);
				this.currentTime = cTime/MS;
				this.currentPosition = (float)this.currentTime/(float)this.clipLength;
				DsError.ThrowExceptionForHR(hr);
			

        }
		
		public Widget Window{
			get {return this.gtkDrawingWindow;}
		}

        public bool SeekTo(long pos,bool accurate)
        {
            int hr = 0;
			DsLong dsPos = new DsLong (pos*MS);  
			if (accurate){
			hr = this.mediaSeeking.SetPositions(dsPos, AMSeekingSeekingFlags.AbsolutePositioning,
			                                    null, AMSeekingSeekingFlags.NoPositioning);
			}
			else {
				hr = this.mediaSeeking.SetPositions(dsPos, AMSeekingSeekingFlags.AbsolutePositioning ,
			                                    null, AMSeekingSeekingFlags.NoPositioning);
		
			}
			DsError.ThrowExceptionForHR(hr);
			return true;
        }

		public bool SeekInSegment(long pos){
			int hr = 0;
            this.Pause();
            hr = this.mediaSeeking.SetPositions(pos*MS, AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.Segment, null, AMSeekingSeekingFlags.NoPositioning | AMSeekingSeekingFlags.Segment);
		    DsError.ThrowExceptionForHR(hr);
		    this.Play();
			return true;
		
		}
		
		
		public bool SegmentSeek (long start, long stop){
			int hr = 0;
            this.Pause();
            hr = this.mediaSeeking.SetPositions(start*MS, AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.Segment, stop*MS, AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.Segment);
		    DsError.ThrowExceptionForHR(hr);
		    this.Play();
			return true;
		}
		
		public bool NewFileSeek (long start, long stop){
			this.SetStartStop(start,stop);
			return true;
		}
		
        public void SetStartStop(long start, long stop)
        {
            int hr = 0;
            this.Pause();
            hr = this.mediaSeeking.SetPositions(start*MS, AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.Segment, stop*MS, AMSeekingSeekingFlags.AbsolutePositioning | AMSeekingSeekingFlags.Segment);
		    DsError.ThrowExceptionForHR(hr);
		    this.Play();
            
        }
		
		

        private int ToggleMute()
        {
            int hr = 0;

            if ((this.graphBuilder == null) || (this.basicAudio == null))
                return 0;

            // Read current volume
            hr = this.basicAudio.get_Volume(out this.currentVolume);
            if (hr == -1) //E_NOTIMPL
            {
                // Fail quietly if this is a video-only media file
                return 0;
            }
            else if (hr < 0)
            {
                return hr;
            }

            // Switch volume levels
            if (this.currentVolume == VolumeFull)
                this.currentVolume = VolumeSilence;
            else
                this.currentVolume = VolumeFull;

            // Set new volume
            hr = this.basicAudio.put_Volume(this.currentVolume);

           
            return hr;
        }

        private int ToggleFullScreen()
        {
            int hr = 0;
            OABool lMode;

            // Don't bother with full-screen for audio-only files
            if (this.videoWindow == null)
                return 0;

            // Read current state
            hr = this.videoWindow.get_FullScreenMode(out lMode);
            DsError.ThrowExceptionForHR(hr);

            if (lMode == OABool.False)
            {
                // Save current message drain
                hr = this.videoWindow.get_MessageDrain(out hDrain);
                DsError.ThrowExceptionForHR(hr);

                // Set message drain to application main window
                hr = this.videoWindow.put_MessageDrain(this.Handle);
                DsError.ThrowExceptionForHR(hr);

                // Switch to full-screen mode
                lMode = OABool.True;
                hr = this.videoWindow.put_FullScreenMode(lMode);
                DsError.ThrowExceptionForHR(hr);
                this.isFullScreen = true;
            }
            else
            {
                // Switch back to windowed mode
                lMode = OABool.False;
                hr = this.videoWindow.put_FullScreenMode(lMode);
                DsError.ThrowExceptionForHR(hr);

                // Undo change of message drain
                hr = this.videoWindow.put_MessageDrain(hDrain);
                DsError.ThrowExceptionForHR(hr);

                // Reset video window
                hr = this.videoWindow.SetWindowForeground(OABool.True);
                DsError.ThrowExceptionForHR(hr);

                // Reclaim keyboard focus for player application
                //this.Focus();
                this.isFullScreen = false;
            }

            return hr;
        }

        private int StepOneFrame()
        {
            int hr = 0;

            // If the Frame Stepping interface exists, use it to step one frame
            if (this.frameStep != null)
            {
                // The graph must be paused for frame stepping to work
                if (this.currentState != PlayState.Paused)
                    Pause();

                // Step the requested number of frames, if supported
                hr = this.frameStep.Step(1, null);
            }

            return hr;
        }

        private int StepFrames(int nFramesToStep)
        {
            int hr = 0;

            // If the Frame Stepping interface exists, use it to step frames
            if (this.frameStep != null)
            {
                // The renderer may not support frame stepping for more than one
                // frame at a time, so check for support.  S_OK indicates that the
                // renderer can step nFramesToStep successfully.
                hr = this.frameStep.CanStep(nFramesToStep, null);
                if (hr == 0)
                {
                    // The graph must be paused for frame stepping to work
                    if (this.currentState != PlayState.Paused)
                        Pause();

                    // Step the requested number of frames, if supported
                    hr = this.frameStep.Step(nFramesToStep, null);
                }
            }

            return hr;
        }

		
		public bool SetRate (float rate){
			
			this.ModifyRate(rate);
			return true;
		}
		
		public bool SetRateInSegment (float rate,long stopTime){
			return this.SetRate (rate);
			
		}
		
		
        private int ModifyRate(double dRateAdjust)
        {
            int hr = 0;
            double dRate;

            // If the IMediaPosition interface exists, use it to set rate
            if ((this.mediaPosition != null) && (dRateAdjust != 0.0))
            {
                hr = this.mediaPosition.get_Rate(out dRate);
                if (hr == 0)
                {
                    // Add current rate to adjustment value
                    double dNewRate = dRate + dRateAdjust;
                    hr = this.mediaPosition.put_Rate(dNewRate);

                    // Save global rate
                    if (hr == 0)
                    {
                        this.currentPlaybackRate = dNewRate;
                        
                    }
                }
            }

            return hr;
        }
		
        private string formatTime(long time)
        {            
            TimeSpan ts = new TimeSpan(time);
            return ts.Hours+ ":" + ts.Minutes + ":" + ts.Seconds;
        }
		
        private int SetRate(double rate)
        {
            int hr = 0;

            // If the IMediaPosition interface exists, use it to set rate
            if (this.mediaPosition != null)
            {
                hr = this.mediaPosition.put_Rate(rate);
                if (hr >= 0)
                {
                    this.currentPlaybackRate = rate;
                    
                }
            }

            return hr;
        }

        private void HandleGraphEvent()
        {
        	
            int hr = 0;
            EventCode evCode;
            IntPtr evParam1, evParam2;

            // Make sure that we don't access the media event interface
            // after it has already been released.
            if (this.mediaEventEx == null)
                return;
           
            // Process all queued events
            while (this.mediaEventEx.GetEvent(out evCode, out evParam1, out evParam2, 0) == 0)
            {

                // Free memory associated with callback, since we're not using it
                hr = this.mediaEventEx.FreeEventParams(evCode, evParam1, evParam2);
				DsError.ThrowExceptionForHR(hr);
                // If this is the end of the clip, reset to beginning
                if (evCode == EventCode.Complete)
                {
                	Console.WriteLine("segmento completo");
                	if (this.Eos != null)
                	this.Eos((object)this,new EventArgs());
                    
                }
            }
        }

        /*
         * WinForm Related methods
         */

        protected override void WndProc(ref Message m)
        {
        
            switch (m.Msg)
            {
                case WMGraphNotify:
                    {
                        HandleGraphEvent();
                        break;
                    }
            }
            
            

            // Pass this message to the video window for notification of system changes
            if (this.videoWindow != null)
                this.videoWindow.NotifyOwnerMessage(m.HWnd, m.Msg, m.WParam, m.LParam);

            base.WndProc(ref m);
        }


    

       /* private void MainForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    {
                        StepOneFrame();
                        break;
                    }
                case Keys.Left:
                    {
                        ModifyRate(-0.25);
                        break;
                    }
                case Keys.Right:
                    {
                        ModifyRate(+0.25);
                        break;
                    }
                case Keys.Down:
                    {
                        SetRate(1.0);
                        break;
                    }
                case Keys.P:
                    {
                        PauseClip();
                        break;
                    }
                case Keys.S:
                    {
                        StopClip();
                        break;
                    }
                case Keys.M:
                    {
                        ToggleMute();
                        break;
                    }
                case Keys.F:
                case Keys.Return:
                    {
                        ToggleFullScreen();
                        break;
                    }
                case Keys.H:
                    {
                        InitVideoWindow(1, 2);
                        CheckSizeMenu(menuFileSizeHalf);
                        break;
                    }
                case Keys.N:
                    {
                        InitVideoWindow(1, 1);
                        CheckSizeMenu(menuFileSizeNormal);
                        break;
                    }
                case Keys.D:
                    {
                        InitVideoWindow(2, 1);
                        CheckSizeMenu(menuFileSizeDouble);
                        break;
                    }
                case Keys.T:
                    {
                        InitVideoWindow(3, 4);
                        CheckSizeMenu(menuFileSizeThreeQuarter);
                        break;
                    }
                case Keys.Escape:
                    {
                        if (this.isFullScreen)
                            ToggleFullScreen();
                        else
                            CloseClip();
                        break;
                    }
                case Keys.F12 | Keys.Q | Keys.X:
                    {
                        CloseClip();
                        break;
                    }
            }
        }
        */
        public DSPlayer(UseType type)
        {
			this.timer = new System.Timers.Timer(100);
			this.timer.Elapsed += new ElapsedEventHandler(OnTimerElapsed);
			this.timer.Enabled = true;
			this.timer.Start();
			this.type = type;
            InitializeComponent();
           

        }
		
		~DSPlayer(){

			this.Destruct();
		}

      
		public void Destruct(){

			this.CloseInterfaces();
			this.gtkDrawingWindow.Dispose();
			base.Dispose();
			
		}
		
        private void InitializeComponent()
        {
			this.gtkDrawingWindow = new DrawingArea();
			this.gtkDrawingWindow.SizeAllocated += new SizeAllocatedHandler (OnResized);
			
			//this.userControl = new System.Windows.Forms.UserControl();
			this.videoPanel = new System.Windows.Forms.Panel();
			this.SuspendLayout();
		
			// 
			// panel1
			// 
			this.videoPanel.BackColor = System.Drawing.SystemColors.WindowFrame;
			this.videoPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.videoPanel.Location = new System.Drawing.Point(0, 0);
			this.videoPanel.Name = "videoPanel";
			this.videoPanel.Size = new System.Drawing.Size(640, 420);
			this.videoPanel.TabIndex = 0;
		
			// 
			// UserControl1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.videoPanel);
			this.Name = "UserControl";
			this.Size = new System.Drawing.Size(640, 420);
			this.Move += new System.EventHandler(this.PlayerWindow_Move);
            this.Resize += new System.EventHandler(this.PlayerWindow_Resize);
			this.ResumeLayout(false);	
		

        }

       

       

        protected virtual void OnResized ( object sender, Gtk.SizeAllocatedArgs args){
        	this.Width = args.Allocation.Width;
        	this.Height = args.Allocation.Height;
        }
        
        
		protected virtual void OnTimerElapsed(object sender, ElapsedEventArgs e) {
      
			TickArgs args = new TickArgs();
			args.Args = new object[4];
		
			args.Args[0] = this.CurrentTime;
			args.Args[1] = this.clipLength;
				
			args.Args[2] = this.currentPosition;
			args.Args[3] = true;
			
			if (this.Tick != null)
			this.Tick((object)this,args);		
		}

        private void PlayerWindow_Move(object sender, EventArgs e)
        {
            MoveVideoWindow();            
        }

        private void PlayerWindow_Resize(object sender, EventArgs e)
        {
            MoveVideoWindow();
        }



    

      
       

   

    }
}
