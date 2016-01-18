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
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	[JsonObject]
	public class MediaFileSet : List<MediaFile>, IStorable
	{
		[NonSerialized]
		IStorage storage;
		bool isLoading;

		public MediaFileSet ()
		{
			ID = Guid.NewGuid ();
		}

		public Guid ID {
			get;
			set;
		}

		#region IStorable

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public IStorage Storage {
			get {
				return storage;
			}
			set {
				storage = value;
			}
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsLoaded {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public List<IStorable> SavedChildren {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool DeleteChildren {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public bool IsChanged {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public string DocumentID {
			get;
			set;
		}

		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Guid ParentID {
			get;
			set;
		}

		#endregion

		[JsonProperty]
		[PropertyChanged.DoNotNotify]
		List<MediaFile> MediaFiles {
			get {
				if (Count == 0) {
					return null;
				} else {
					return new List<MediaFile> (this);
				}
			}
			set {
				this.Clear ();
				if (value != null) {
					AddRange (value);
				}
			}
		}

		[JsonProperty]
		[Obsolete]
		Dictionary <MediaFileAngle, MediaFile> Files {
			set {
				// Transform old Files dict to ordered list
				foreach (KeyValuePair<MediaFileAngle, MediaFile> File in value) {
					if (File.Value != null) {
						// Set the angle as the name
						switch (File.Key) {
						case MediaFileAngle.Angle1:
							File.Value.Name = Catalog.GetString ("Main camera angle");
							break;
						case MediaFileAngle.Angle2:
							File.Value.Name = Catalog.GetString ("Angle 2");
							break;
						case MediaFileAngle.Angle3:
							File.Value.Name = Catalog.GetString ("Angle 3");
							break;
						case MediaFileAngle.Angle4:
							File.Value.Name = Catalog.GetString ("Angle 4");
							break;
						}
						// Add to list
						Add (File.Value);
					}
				}
				// FIXME: Order list
			}
		}

		/// <summary>
		/// Gets the preview of the first file in set or null if the set is empty.
		/// </summary>
		/// <value>The preview.</value>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Image Preview {
			get {
				MediaFile file = this.FirstOrDefault ();

				if (file != null) {
					return file.Preview;
				} else {
					return null;
				}
			}
		}

		/// <summary>
		/// Gets the maximum duration from all files in set.
		/// </summary>
		/// <value>The duration.</value>
		[JsonIgnore]
		[PropertyChanged.DoNotNotify]
		public Time Duration {
			get {
				if (Count != 0) {
					return this.Max (mf => mf == null ? new Time (0) : mf.Duration);
				} else {
					return new Time (0);
				}
			}
		}

		/// <summary>
		/// Search for first file with matching name and replace with new file.
		/// If no file with matching name was found, this is equivalent to adding new file to the set.
		/// Note that new file does not have to use the same name.
		/// </summary>
		/// <param name="name">Name to use for the search.</param>
		/// <param name="file">File.</param>
		/// <returns><c>true</c> if the name was found and a substitution happened, <c>false</c> otherwise.</returns>
		public bool Replace (String name, MediaFile file)
		{
			MediaFile old_file = this.Where (mf => mf.Name == name).FirstOrDefault ();
			return Replace (old_file, file);
		}

		/// <summary>
		/// Search for a file in the set and replace it with a new one.
		/// If old file is not found, this is equivalent to adding new file to the set.
		/// Some properties from the old file are copied to the new file such as Name and Offset.
		/// </summary>
		/// <param name="old_file">Old file.</param>
		/// <param name="new_file">New file.</param>
		/// <returns>><c>true</c> if the old file was found and a substitution happened, <c>false</c> otherwise.</returns>
		public bool Replace (MediaFile old_file, MediaFile new_file)
		{
			bool found = false;

			if (Contains (old_file)) {
				if (new_file != null && old_file != null) {
					new_file.Name = old_file.Name;
					new_file.Offset = old_file.Offset;
				}

				this [IndexOf (old_file)] = new_file;
				found = true;
			} else {
				Add (new_file);
			}

			return found;
		}

		/// <summary>
		/// Checks that all files in the set are valid.
		/// </summary>
		/// <returns><c>true</c>, if files was checked, <c>false</c> otherwise.</returns>
		public bool CheckFiles ()
		{
			if (Count == 0) {
				return false;
			}
			foreach (MediaFile f in this) {
				if (!f.Exists ()) {
					return false;
				}
			}
			return true;
		}

		protected void CheckIsLoaded ()
		{
			if (!IsLoaded && !isLoading) {
				isLoading = true;
				if (Storage == null) {
					throw new StorageException ("Storage not set in preloaded object");
				}
				Storage.Fill (this);
				IsLoaded = true;
				isLoading = false;
			}
		}

		public override bool Equals (object obj)
		{
			MediaFileSet s = obj as MediaFileSet;
			if (s == null)
				return false;
			return ID.Equals (s.ID);
		}

		public override int GetHashCode ()
		{
			return ID.GetHashCode ();
		}
	}
}

