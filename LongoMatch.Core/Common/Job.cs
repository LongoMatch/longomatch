// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;

using LongoMatch.Core.Common;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;

namespace LongoMatch.Core.Common
{
	[Serializable]
	public class Job
	{
		public Job (EncodingSettings encSettings)
		{
			EncodingSettings = encSettings;
			State = JobState.NotStarted;
		}
		
		public string Name {
			get {
				return System.IO.Path.GetFileName(EncodingSettings.OutputFile);
			}
		}
		
		public JobState State {
			get;
			set;
		}
		
		public string StateIconName{
			get{
				switch (State) {
				case JobState.Error:
					return "gtk-dialog-error";
				case JobState.Finished:
					return "gtk-ok";
				case JobState.Cancelled:
					return "gtk-cancel";
				case JobState.NotStarted:
					return "gtk-execute";
				case JobState.Running:
					return "gtk-media-record";
				}
				return "";
			}
		}
		
		public EncodingSettings EncodingSettings {
			get;
			set;
		}
	}
	
	public class EditionJob: Job
	{
		public EditionJob (Playlist playlist, EncodingSettings encSettings): base (encSettings)
		{
			Playlist = Cloner.Clone(playlist);
		}
		
		public Playlist Playlist{
			get;
			set;
		}
	}
	
	public class ConversionJob: Job
	{
		public ConversionJob (List<MediaFile> files, EncodingSettings encSettings): base (encSettings)
		{
			InputFiles = files;
		}
		
		public List<MediaFile> InputFiles {
			get;
			set;
		}
	}
}

