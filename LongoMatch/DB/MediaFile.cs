// MediaFile.cs
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

namespace LongoMatch
{
	
	
	public class MediaFile
	{
		string filePath;
		Time length;
		ushort fps;
		
		public MediaFile(string filePath,Time length,ushort fps)
		{
			this.filePath = filePath;
			this.length = length;
			this.fps = fps;
		}
		
		public string FilePath{
			get {return this.filePath;}
			set {this.filePath = value;}
		}
		
		public Time Length{
			get {return this.length;}
			set {this.length = value;}
		}
		
		public ushort Fps{
			get {return this.fps;}
			set {this.fps = value;}
		}
		
		public uint GetFrames(){
			return (uint) (Fps*Length.Seconds);
		}
	}
}
