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

using LongoMatch.Common;
using LongoMatch.Store;

namespace LongoMatch.Handlers
{
	public delegate void PlayListSegmentDoneHandler();
	public delegate void SegmentClosedHandler();
	public delegate void SegmentDoneHandler();
	public delegate void SeekEventHandler(Time pos);
	public delegate void VolumeChangedHandler(double level);
	public delegate void NextButtonClickedHandler();
	public delegate void PrevButtonClickedHandler();
	public delegate void ProgressHandler(float progress);
	public delegate void FramesProgressHandler(int actual, int total, Image frame);
	public delegate void DrawFrameHandler (Play play, int drawingIndex);
	public delegate void EllpasedTimeHandler(Time ellapsedTime);
	public delegate void PlaybackRateChangedHandler (float rate);
	
	public delegate void DeviceChangeHandler(int deviceID);
	public delegate void CaptureFinishedHandler (bool close);
	public delegate void ErrorHandler(string message);
	public delegate void PercentCompletedHandler(float percent);
	public delegate void StateChangeHandler(bool playing);
	public delegate void TickHandler(Time currentTime);
	public delegate void EosHandler();
	public delegate void ReadyToSeekHandler();
}
