//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using Mono.Unix;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Interfaces.Multimedia;
using LongoMatch.Store;

namespace LongoMatch.Utils
{
	public class Open
	{
		public static MediaFile OpenFile (object parent) {
			IBusyDialog busy = null;
			MediaFile mediaFile = null;
			IGUIToolkit gui = Config.GUIToolkit;
			IMultimediaToolkit multimedia = Config.MultimediaToolkit; 
			string folder, filename;
			
			
			folder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			filename = gui.OpenFile (Catalog.GetString("Open file"), null, folder, null, null);                             
			if (filename == null)
					return null;
			
			try {
				busy = gui.BusyDialog (Catalog.GetString("Analyzing video file:")+"\n"+filename,
				                       parent);
				busy.Show ();
				mediaFile = multimedia.DiscoverFile (filename);
				busy.Destroy ();

				if(!mediaFile.HasVideo || mediaFile.VideoCodec == "")
					throw new Exception(Catalog.GetString("This file doesn't contain a video stream."));
				if(mediaFile.HasVideo && mediaFile.Duration.MSeconds == 0)
					throw new Exception(Catalog.GetString("This file contains a video stream but its length is 0."));
				if (multimedia.FileNeedsRemux (mediaFile)) {
					string q = Catalog.GetString("The file you are trying to load is not properly supported. " +
						                             "Would you like to convert it into a more suitable format?");
					if (gui.QuestionMessage (q, Catalog.GetString ("Convert"), null)) {
						string newFilename = multimedia.RemuxFile (mediaFile, parent);
						if (newFilename != null)
							mediaFile = multimedia.DiscoverFile (newFilename);
					}
				}
			}
			catch(Exception ex) {
				busy.Destroy ();
				gui.ErrorMessage (ex.Message, parent);
				return null;
			}
			
			return mediaFile;
		}

	}
}

