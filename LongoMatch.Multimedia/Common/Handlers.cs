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
using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Video.Common
{
	public delegate void ProgressHandler (float progress);
	public delegate void FramesProgressHandler (int actual,int total,Image frame);
	public delegate void DrawFrameHandler (int time);
	public delegate void GlibErrorHandler (object o,ErrorArgs args);
	public delegate void GlibPercentCompletedHandler (object o,PercentCompletedArgs args);
	public delegate void GlibStateChangeHandler (object o,StateChangeArgs args);
	public delegate void GlibTickHandler (object o,TickArgs args);
	public delegate void GlibMediaInfoHandler (object o,MediaInfoArgs args);
	public delegate void GlibDeviceChangeHandler (object o,DeviceChangeArgs args);
	public class ErrorArgs : GLib.SignalArgs
	{
		public string Message {
			get {
				return (string)Args [0];
			}
		}
	}

	public class PercentCompletedArgs : GLib.SignalArgs
	{
		public float Percent {
			get {
				return (float)Args [0];
			}
		}
	}

	public class StateChangeArgs : GLib.SignalArgs
	{
		public bool Playing {
			get {
				return (bool)Args [0];
			}
		}
	}

	public class TickArgs : GLib.SignalArgs
	{
		public Time CurrentTime {
			get {
				return new Time { NSeconds = (long)Args [0] };
			}
		}

		public Time StreamLength {
			get {
				return new Time { NSeconds = (long)Args [1] };
			}
		}

		public double CurrentPosition {
			get {
				return (double)Args [2];
			}
		}
	}

	public class DeviceChangeArgs : GLib.SignalArgs
	{
		public int DeviceChange {
			get {
				return (int)Args [0];
			}
		}
	}

	public class MediaInfoArgs : GLib.SignalArgs
	{
		public int Width {
			get {
				return (int)Args [0];
			}
		}

		public int Height {
			get {
				return (int)Args [1];
			}
		}

		public int ParN {
			get {
				return (int)Args [2];
			}
		}

		public int ParD {
			get {
				return (int)Args [3];
			}
		}
	}
}
