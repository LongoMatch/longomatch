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
using System.Linq;
using System.Collections.Generic;
using LongoMatch.Core;
using LongoMatch.Core.Store;
using LongoMatch.Core.Common;
using Gtk;

namespace LongoMatch.Gui.Component
{
	[System.ComponentModel.ToolboxItem (true)]
	public partial class MediaFileSetSelection : Gtk.Bin
	{
		MediaFileSet fileSet;
		List<MediaFileChooser> fileChoosers;
		bool editable = true;

		public MediaFileSetSelection (bool editable = true)
		{
			this.Build ();

			this.editable = editable;

			fileChoosers = new List<MediaFileChooser> ();
		}

		public MediaFileSet FileSet {
			set {
				// In case we don't support multi camera, clip fileset to only one file.
				if (!Config.SupportsMultiCamera && value.Count > 1) {
					fileSet = new MediaFileSet ();
					fileSet.Add (value [0]);
				} else {
					fileSet = value;
				}

				if (fileSet.Count > 0) {
					// Create all choosers
					foreach (MediaFile mf in fileSet) {
						AddMediaFileChooser (mf);
					}
				} else {
					// Add the first media file chooser for main camera
					AddMediaFileChooser (Catalog.GetString ("Main camera angle"));
				}
			}
			get {
				return fileSet;
			}
		}

		/// <summary>
		/// Forces a scroll to bottom of our scrolled window. This is useful for the user to access easily the latest entry field.
		/// </summary>
		void ScrollToBottom ()
		{
			Adjustment adj = mfss_scrolledwindow.Vadjustment;

			adj.Value = adj.Upper - adj.PageSize;
		}

		void HandleChooserAllocated (object sender, EventArgs e)
		{
			ScrollToBottom ();
			(sender as MediaFileChooser).SizeAllocated -= HandleChooserAllocated;
		}

		/// <summary>
		/// Add a media file chooser with given name. If name is null pick a name automatically avoiding duplicates.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="mediaFile">Media File.</param>
		void AddMediaFileChooser (String name, MediaFile mediaFile = null)
		{
			if (name == null) {
				int i = fileChoosers.Count;

				if (i == 0) {
					name = Catalog.GetString ("Main camera angle");
				} else {
					name = String.Format ("{0} {1}", Catalog.GetString ("Angle"), i);
					while (fileChoosers.Any (c => c.MediaFile.Name == name)) {
						name = String.Format ("{0} {1}", Catalog.GetString ("Angle"), ++i);
					}
				}
			}

			MediaFileChooser chooser = new MediaFileChooser (name);

			chooser.ChangedEvent += HandleFileChangedEvent;

			if (mediaFile != null)
				chooser.MediaFile = mediaFile;

			// When the chooser is allocated we scroll to the bottom of the window.
			chooser.SizeAllocated += HandleChooserAllocated;

			chooser.ShowAll ();

			mfss_vbox.PackStart (chooser, true, true, 0);

			fileChoosers.Add (chooser);
		}

		/// <summary>
		/// Add a media file chooser with given name.
		/// </summary>
		/// <param name="mediaFile">Media File.</param>
		void AddMediaFileChooser (MediaFile mediaFile)
		{
			AddMediaFileChooser (mediaFile.Name, mediaFile);
		}

		/// <summary>
		/// Here we just map 1 to 1 from the choosers list to our set. We make sure to use Replace method on the set
		/// to preserve important metadata.
		/// </summary>
		void UpdateFileSet ()
		{
			for (var i = 0; i < fileChoosers.Count; i++) {
				MediaFile mf = fileChoosers [i].MediaFile;
				if (mf != null) {
					fileSet.Replace (fileSet [i], mf);	
				} else {
					// Make sure that CheckFiles will not return true for this MediaFile...
					fileSet [i].FilePath = null;
				}
			}
		}

		/// <summary>
		/// Editable FileSet will just clear the set and recreate from the choosers. It will also figure out
		/// if some choosers should be removed or added to allow more files to come in.
		/// </summary>
		void UpdateEditableFileSet ()
		{
			bool have_empty_chooser = false;
			List<MediaFileChooser> to_remove = new List<MediaFileChooser> ();

			fileSet.Clear ();

			foreach (MediaFileChooser chooser in fileChoosers) {
				if (chooser.MediaFile != null) {
					fileSet.Add (chooser.MediaFile);
				} else {
					// Mark for removal as we only want one empty file chooser at most
					if (fileChoosers.Count > 1) {
						to_remove.Add (chooser);
					} else {
						have_empty_chooser = true;
					}
				}
			}

			foreach (MediaFileChooser chooser in to_remove) {
				chooser.ChangedEvent -= HandleFileChangedEvent;
				fileChoosers.Remove (chooser);
				mfss_vbox.Remove (chooser);
			}

			to_remove.Clear ();

			if (!have_empty_chooser && Config.SupportsMultiCamera) {
				AddMediaFileChooser (null);
			}
		}

		void HandleFileChangedEvent (object sender, EventArgs e)
		{
			if (editable) {
				UpdateEditableFileSet ();
			} else {
				UpdateFileSet ();
			}
		}
	}
}

