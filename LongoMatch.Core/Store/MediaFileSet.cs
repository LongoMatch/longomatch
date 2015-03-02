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
using LongoMatch.Core.Common;
using Newtonsoft.Json;

namespace LongoMatch.Core.Store
{
	[JsonObject(MemberSerialization.OptIn)]
	[Serializable]
	public class MediaFileSet
	{

		public MediaFileSet ()
		{
			Files = new Dictionary<MediaFileAngle, MediaFile>();
			Files[MediaFileAngle.Angle1] = null;
			Files[MediaFileAngle.Angle2] = null;
			Files[MediaFileAngle.Angle3] = null;
			Files[MediaFileAngle.Angle4] = null;
		}

		[JsonProperty]
		Dictionary <MediaFileAngle, MediaFile> Files {
			get;
			set;
		}

		public Image Preview {
			get {
				if (Files.Count == 0) {
					return null;
				} else {
					return Files[MediaFileAngle.Angle1].Preview;
				}
			}
		}

		public Time Duration {
			get {
				return Files.Values.Max (mf => mf == null ? new Time (0) : mf.Duration);
			}
		}

		public MediaFile GetAngle (MediaFileAngle angle)
		{
			if (Files [angle] != null) {
				return Files [angle];
			} else {
				Log.Warning ("View not found for file set: " + angle);
				return null;
			}
		}
		
		public void SetAngle (MediaFileAngle angle, MediaFile file)
		{
			Files[angle] = file;
		}

		public bool CheckFiles ()
		{
			List<MediaFile> files = ActiveFiles;
			if (files.Count == 0) {
				return false;
			}
			foreach (MediaFile f in files) {
				if (!f.Exists ()) {
					return false;
				}
			}
			return true;
		}

		public List<MediaFile> ActiveFiles {
			get {
				return Files.Values.Where(f => f != null).ToList();
			}
		}
	}
}

