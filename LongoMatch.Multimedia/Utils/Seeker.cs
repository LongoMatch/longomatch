//
//  Copyright (C) 2013 Andoni Morales Alastruey
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
using LongoMatch.Video.Common;

namespace LongoMatch.Video.Utils
{
	public class Seeker
	{
		public event SeekHandler SeekEvent;
		
		uint timeout;
		int pendingSeekId;
		long start, stop;
		float rate;
		bool inSegment;
		SeekType seekType;
		
		public Seeker (uint timeoutMS=80)
		{
			timeout = timeoutMS;
			pendingSeekId = -1;
			seekType = SeekType.None;
		}
		
		public void Seek (SeekType seekType, float rate=1, bool inSegment=false, long start=0, long stop=0)
		{
			this.seekType = seekType;
			this.start = start;
			this.stop = stop;
			this.rate = rate;
			this.inSegment = inSegment;
			
			if (pendingSeekId != -1)
				return;
			
			HandleSeekTimeout ();
			pendingSeekId = (int) GLib.Timeout.Add (timeout, HandleSeekTimeout);
			
		}
		
		public bool HandleSeekTimeout () {
			pendingSeekId = -1;
			if (seekType != SeekType.None) {
				if (SeekEvent != null) {
					SeekEvent (seekType, rate, inSegment, start, stop);
				}
				seekType = SeekType.None;
			}
			return false;
		}
	}
}

