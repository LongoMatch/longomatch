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

using System.Collections.ObjectModel;
using LongoMatch.Core.Store;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Core.Handlers
{
	public delegate void PlayListSegmentDoneHandler ();
	public delegate void SegmentClosedHandler ();
	public delegate void SegmentDoneHandler ();
	public delegate void SeekEventHandler (Time pos,bool accurate,bool synchronous = false,bool throttled = false);
	public delegate void TogglePlayEventHandler (bool playing);
	public delegate void VolumeChangedHandler (double level);
	public delegate void NextButtonClickedHandler ();
	public delegate void PrevButtonClickedHandler ();
	public delegate void ProgressHandler (float progress);
	public delegate void FramesProgressHandler (int actual,int total,Image frame);
	public delegate void DrawFrameHandler (TimelineEventLongoMatch play,int drawingIndex,CameraConfig camConfig,bool current);
	public delegate void ElapsedTimeHandler (Time ellapsedTime);
	public delegate void PlaybackRateChangedHandler (float rate);
	public delegate void SeekHandler (SeekType type,Time start,float rate);

	public delegate void DeviceChangeHandler (int deviceID);
	public delegate void CaptureFinishedHandler (bool close,bool reopen);
	public delegate void PercentCompletedHandler (float percent);
	public delegate void TickHandler (Time currentTime);
	public delegate void TimeChangedHandler (Time currentTime,Time duration,bool seekable);
	public delegate void MediaInfoHandler (int width,int height,int parN,int parD);
	public delegate void LoadDrawingsHandler (FrameDrawing frameDrawing);
	public delegate void ElementLoadedHandler (object element,bool hasNext);
	public delegate void MediaFileSetLoadedHandler (MediaFileSet fileset,ObservableCollection<CameraConfig> camerasConfig = null);
	public delegate void ScopeStateChangedHandler (int index,bool visible);
	public delegate void PrepareViewHandler ();

	public delegate void ErrorHandler (object sender,string message);
	public delegate void EosHandler (object sender);
	public delegate void ReadyToSeekHandler (object sender);
	public delegate void StateChangeHandler (object sender,bool playing);
	public delegate void ReadyToCaptureHandler (object sender);
}
