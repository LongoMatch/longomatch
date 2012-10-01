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

namespace LongoMatch.Common
{


	public enum ProjectType {
		CaptureProject,
		FakeCaptureProject,
		FileProject,
		EditProject,
		None,
	}
	
	public enum CapturerType {
		Fake,
		Live,
	}

	public enum EndCaptureResponse {
		Return = 234,
		Quit = 235,
		Save = 236
	}

	public enum TagMode {
		Predifined,
		Free
	}

	public enum SortMethodType {
		SortByName = 0,
		SortByStartTime = 1,
		SortByStopTime = 2,
		SortByDuration = 3
	}

	public enum Team {
		NONE = 0,
		LOCAL = 1,
		VISITOR = 2,
		BOTH = 3,
	}
	
	public enum JobState {
		NotStarted,
		Running,
		Finished,
		Cancelled,
		Error,
	}
	
	public enum VideoEncoderType {
		Mpeg4,
		Xvid,
		Theora,
		H264,
		Mpeg2,
		VP8,
	}

	public enum AudioEncoderType {
		Mp3,
		Aac,
		Vorbis,
	}

	public enum VideoMuxerType {
		Avi,
		Mp4,
		Matroska,
		Ogg,
		MpegPS,
		WebM,
	}
	
	public enum DrawTool {
		PEN,
		LINE,
		DASHED_LINE,
		CIRCLE,
		DASHED_CIRCLE,
		RECTANGLE,
		DASHED_RECTANGLE,
		CROSS,
		DASHED_CROSS,
		ERASER
	}
	
	public enum CaptureSourceType {
		None,
		DV,
		System,
	}
	
	public enum GameUnitEventType {
		Start, 
		Stop,
		Cancel
	}
	
	public enum EditorState
	{
		START = 0,
		FINISHED = 1,
		CANCELED = -1,
		ERROR = -2
	}
}
