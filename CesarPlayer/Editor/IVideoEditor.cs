// IVideoEditor.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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
using System.Collections.Generic;

namespace LongoMatch.Video.Editor
{
	
	
	public interface IVideoEditor
	{
		event LongoMatch.Video.Handlers.ProgressHandler Progress;		
		
		VideoQuality VideoQuality{
			set;
		}
		
		AudioQuality AudioQuality{
			set;
		}
		
		VideoFormat VideoFormat{
			set;
		}
		
		string OutputFile{
			set;
		}
		
		bool EnableAudio{
			set;
		}
		
			
		void AddSegment (string filePath, long start, long duration, double rate, string title) ;
		
		void ClearList();
		
		void Start();		
		
		void Cancel();
		
	}
}
