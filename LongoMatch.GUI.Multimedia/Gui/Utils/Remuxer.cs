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
using System.IO;
using Gtk;
using LongoMatch.Core;

using LongoMatch.Core.Interfaces.Multimedia;
using VAS.Core.Common;
using VAS.Core.Store;

namespace LongoMatch.Video.Utils
{
	public class Remuxer
	{
		MediaFile inputFile;
		string outputFilepath;
		Dialog dialog;
		ProgressBar pb;
		IRemuxer remuxer;
		IMultimediaToolkit multimedia;
		uint timeout;
		bool cancelled;
		VideoMuxerType muxer;
		Window parent;

		public Remuxer (MediaFile inputFile, string outputFilepath = null,
		                VideoMuxerType muxer = VideoMuxerType.Mp4)
		{
			this.inputFile = inputFile;
			this.muxer = muxer;
			
			if (outputFilepath != null) {
				this.outputFilepath = outputFilepath;
			} else {
				this.outputFilepath = Path.ChangeExtension (inputFile.FilePath,
					GetExtension (muxer));
				if (this.outputFilepath == inputFile.FilePath) {
					this.outputFilepath = Path.ChangeExtension (inputFile.FilePath,
						"1." + GetExtension (muxer));
				}

			}
			this.multimedia = new MultimediaToolkit ();
		}

		public string Remux (Window parent)
		{
			VBox box;
			Label label;
			Button cancellButton;
			
			this.parent = parent;
			
			/* Create the dialog */
			dialog = new Dialog (Catalog.GetString ("Remuxing file..."), parent, DialogFlags.Modal);
			dialog.AllowGrow = false;
			dialog.AllowShrink = false;
			dialog.Deletable = false;
			
			/* Add label and progress bar */
			box = new VBox ();
			label = new Label (Catalog.GetString ("Remuxing file, this might take a while..."));
			box.PackStart (label);
			pb = new ProgressBar ();
			box.PackStart (pb);
			box.ShowAll ();
			dialog.VBox.Add (box);
			
			/* Add a button to cancell the task */
			cancellButton = new Button ("gtk-cancel");
			cancellButton.Clicked += (sender, e) => Cancel (); 
			cancellButton.Show ();
			dialog.VBox.Add (cancellButton);
			
			/* Add a timeout to refresh the progress bar */ 
			pb.Pulse ();
			timeout = GLib.Timeout.Add (1000, new GLib.TimeoutHandler (Update));
			
			remuxer = multimedia.GetRemuxer (inputFile, outputFilepath, muxer);
			remuxer.Progress += HandleRemuxerProgress;
			remuxer.Error += HandleRemuxerError;
			remuxer.Start ();
			
			/* Wait until the thread call Destroy on the dialog */
			dialog.Run ();
			if (cancelled) {
				try {
					File.Delete (outputFilepath);
				} catch {
				}
				outputFilepath = null;
			}
			return outputFilepath;
		}

		void Error (string error)
		{
			MessageDialog md = new MessageDialog (parent, DialogFlags.Modal, MessageType.Error,
				                   ButtonsType.Ok,
				                   Catalog.GetString ("Error remuxing file:" + "\n" + error));
			md.Run ();
			md.Destroy ();
			Cancel ();
		}

		void HandleRemuxerError (object sender, string error)
		{
			Application.Invoke (delegate {
				Error (error);
			});
		}

		void HandleRemuxerProgress (float progress)
		{
			if (progress == 1) {
				Application.Invoke (delegate {
					Stop ();
				});
			}
		}

		static protected string GetExtension (VideoMuxerType muxer)
		{
			switch (muxer) {
			case VideoMuxerType.Avi:
				return "avi";
			case VideoMuxerType.Matroska:
				return "mkv";
			case VideoMuxerType.Mp4:
				return "mp4";
			case VideoMuxerType.MpegPS:
				return "mpeg";
			case VideoMuxerType.Ogg:
				return "ogg";
			case VideoMuxerType.WebM:
				return "webm";
			}
			throw new Exception ("Muxer format not supported");
		}

		private bool Update ()
		{
			pb.Pulse ();			
			return true;
		}

		private void Stop ()
		{
			GLib.Source.Remove (timeout);
			dialog.Destroy ();
		}

		void Cancel ()
		{
			cancelled = true;
			Stop ();
		}

		public static bool AskForConversion (Window parent)
		{
			bool ret;
			MessageDialog md = new MessageDialog (parent, DialogFlags.Modal, MessageType.Question,
				                   ButtonsType.YesNo,
				                   Catalog.GetString ("The file you are trying to load is not properly " +
				                   "supported. Would you like to convert it into a more suitable " +
				                   "format?"));
			md.TransientFor = parent;
			ret = md.Run () == (int)ResponseType.Yes;
			md.Destroy ();
			
			return ret;
		}
	}
}