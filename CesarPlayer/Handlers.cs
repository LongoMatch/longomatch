// Handlers.cs
//
//  Copyright (C) 2008 Andoni Maorales Alastruey
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

namespace CesarPlayer
{
	
	public delegate void PlayListSegmentDoneHandler ();
	public delegate void SegmentClosedHandler();
	public delegate void StateChangedHandler (bool playing);
	public delegate void TickEventHandler (long currentTime, long streamLength, float position, bool seekable);
	public delegate void EndOfStreamEventHandler ();
	public delegate void SegmentDoneHandler();
	public delegate void ErrorEventHandler(string error);
	public delegate void InvalidVideoFileHandler (string videoFile);
	public delegate void VolumeChangedHandler (int level);
	
}
