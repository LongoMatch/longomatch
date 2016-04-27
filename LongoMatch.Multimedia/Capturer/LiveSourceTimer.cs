//
//  Copyright (C) 2010 Andoni Morales Alastruey
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
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
using System;
using LongoMatch.Core.Handlers;
using LongoMatch.Core.Store;
using VAS.Core.Store;

namespace LongoMatch.Video.Capturer
{
	public class LiveSourceTimer
	{
		public event ElapsedTimeHandler ElapsedTime;

		DateTime lastStart;
		TimeSpan ellapsed;
		bool playing;
		bool started;
		uint timerID;

		public LiveSourceTimer ()
		{
			lastStart = DateTime.Now;
			ellapsed = new TimeSpan (0, 0, 0);
			playing = false;
			started = false;
		}

		public int CurrentTime {
			get {
				if (!started)
					return 0;
				else if (playing)
					return (int)(ellapsed + (DateTime.Now - lastStart)).TotalMilliseconds;
				else
					return (int)ellapsed.TotalMilliseconds;
			}
		}

		public void TogglePause ()
		{
			if (!started)
				return;

			if (playing) {
				playing = false;
				ellapsed += DateTime.Now - lastStart;
			} else {
				playing = true;
				lastStart = DateTime.Now;
			}
		}

		public void Start ()
		{
			timerID = GLib.Timeout.Add (100, OnTick);
			lastStart = DateTime.Now;
			playing = true;
			started = true;
		}

		public void Stop ()
		{
			GLib.Source.Remove (timerID);
		}

		protected virtual bool OnTick ()
		{
			if (ElapsedTime != null)
				ElapsedTime (new Time (CurrentTime));
			return true;
		}
	}
}
