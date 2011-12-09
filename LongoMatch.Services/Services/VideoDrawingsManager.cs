//
//  Copyright (C) 2009 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
using System.Collections.Generic;
using System.Threading;

using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;

namespace LongoMatch.Services
{


	public class VideoDrawingsManager
	{
		IPlayer player;
		Timer timeout;
		bool inKeyFrame;
		bool canStop;
		Play loadedPlay;

		public VideoDrawingsManager(IPlayer player)
		{
			this.player = player;
		}

		~ VideoDrawingsManager() {
			StopClock();
		}

		public Play Play {
			set {
				loadedPlay = value;
				inKeyFrame = false;
				canStop = true;
				ResetPlayerWindow();
				ConnectSignals();
				StartClock();
			}
		}

		private Drawing Drawing {
			get {
				return loadedPlay.KeyFrameDrawing;
			}
		}

		private void StartClock() {
			if(timeout == null)
				timeout = new Timer(new TimerCallback(CheckStopTime),
					this, 20, 20);	
		}

		private void StopClock() {
			if(timeout != null) {
				timeout.Dispose();
				timeout = null;
			}
		}

		private void ConnectSignals() {
			player.PlayStateChanged += OnStateChanged;
			player.SeekEvent += OnSeekEvent;
			player.SegmentClosedEvent += OnSegmentCloseEvent;
		}

		private void DisconnectSignals() {
			player.PlayStateChanged -= OnStateChanged;
			player.SeekEvent -= OnSeekEvent;
			player.SegmentClosedEvent -= OnSegmentCloseEvent;
		}

		private int NextStopTime() {
			return Drawing.RenderTime;
		}

		private void PrintDrawing() {
			Image frame = null;
			Image drawing = null;

			player.Pause();
			player.SeekInSegment(Drawing.RenderTime);
			while(frame == null)
				frame = player.CurrentFrame;
			player.LogoPixbuf = frame;
			drawing = Drawing.Pixbuf;
			player.DrawingPixbuf = drawing;
			player.LogoMode = true;
			player.DrawingMode = true;
			inKeyFrame = true;
			frame.Dispose();
			drawing.Dispose();
		}

		private void ResetPlayerWindow() {
			player.LogoMode = false;
			player.DrawingMode = false;
			player.SetLogo(System.IO.Path.Combine(Config.ImagesDir(),"background.png"));
		}

		private void CheckStopTime(object self) {
			int currentTime = (int)player.AccurateCurrentTime;

			if(Drawing == null || !canStop)
				return;
			if((currentTime)>NextStopTime()) {
				StopClock();
				PrintDrawing();
			}
			return;
		}

		protected virtual void OnStateChanged(object sender, bool playing) {
			//Check if we are currently paused displaying the key frame waiting for the user to
			//go in to Play. If so we can stop
			if(inKeyFrame) {
				ResetPlayerWindow();
				inKeyFrame = false;
			}
		}

		protected virtual void OnSeekEvent(long time) {
			if(Drawing == null)
				return;
			if(inKeyFrame) {
				ResetPlayerWindow();
				inKeyFrame = false;
			}
			canStop = time < Drawing.RenderTime;
			if(canStop)
				StartClock();
			else StopClock();
		}

		protected virtual void OnSegmentCloseEvent() {
			ResetPlayerWindow();
			DisconnectSignals();
			StopClock();
		}

	}
}
