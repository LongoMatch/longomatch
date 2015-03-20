//
//  Copyright (C) 2015 FLUENDO S.A.
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
using LongoMatch.Core.Interfaces.Multimedia;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Handlers;
using System.Collections.Generic;
using Image = LongoMatch.Core.Common.Image;

namespace LongoMatch.Core.Store
{
	public class PlayerController : IPlayerController, IDisposable
	{
		IPlayer player;

		public PlayerController ()
		{
			player = Config.MultimediaToolkit.GetPlayer ();
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			player.Dispose ();
		}
		#endregion

		#region IPlayer implementation
		public event ErrorHandler Error;
		public event EosHandler Eos;
		public event StateChangeHandler StateChange;
		public event ReadyToSeekHandler ReadyToSeek;

		public Time StreamLength {
			get {
				throw new NotImplementedException ();
			}
		}

		public Time CurrentTime {
			get {
				throw new NotImplementedException ();
			}
		}

		public double Volume {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public bool Playing {
			get {
				throw new NotImplementedException ();
			}
		}

		public double Rate {
			set {
				throw new NotImplementedException ();
			}
		}

		public List<IntPtr> WindowHandles {
			set {
				throw new NotImplementedException ();
			}
		}

		public IntPtr WindowHandle { 
			set {
				throw new NotImplementedException ();
			}
		}

		public bool Open (List<string> mrls)
		{
			throw new NotImplementedException ();
		}

		public bool Open (string mrl)
		{
			throw new NotImplementedException ();
		}

		public void Play()
		{
			throw new NotImplementedException ();
		}

		public void Pause()
		{
			throw new NotImplementedException ();
		}

		public void Stop()
		{
			throw new NotImplementedException ();
		}

		public void Close()
		{
			throw new NotImplementedException ();
		}

		public bool Seek (Time time, bool accurate = false, bool synchronous = false)
		{
			throw new NotImplementedException ();
		}

		public bool SeekToNextFrame()
		{
			throw new NotImplementedException ();
		}
		public bool SeekToPreviousFrame()
		{
			throw new NotImplementedException ();
		}

		public Image GetCurrentFrame (int width=-1, int height=-1)
		{
			throw new NotImplementedException ();
		}

		public void Expose ()
		{
			throw new NotImplementedException ();
		}
		#endregion

		#region IPlayerController implementation
		public void LoadEvent (MediaFileSet file, TimelineEvent ev, Time seekTime, bool playing)
		{
			throw new NotImplementedException ();
		}

		public void LoadPlayListEvent (Playlist playlist, IPlaylistElement ev)
		{
			throw new NotImplementedException ();
		}

		public void UnloadCurrentEvent()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
}

