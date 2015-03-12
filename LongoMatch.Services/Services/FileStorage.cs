//
//  Copyright (C) 2015 jl
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
using System.Collections.Generic;
using System.Reflection;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Common;

namespace LongoMatch.Services.Services
{
	public class FileStorage : IStorage
	{
		private string basePath;
		private bool deleteOnDestroy;

		public FileStorage (string basePath, bool deleteOnDestroy = false)
		{
			this.basePath = basePath;
			this.deleteOnDestroy = deleteOnDestroy;
			// Make sure to create the directory
			if (!Directory.Exists (basePath)) {
				Log.Information ("Creating directory " + basePath);
				Directory.CreateDirectory (basePath);
			}
		}

		~FileStorage()
		{
			if (deleteOnDestroy)
				Reset();
		}

		private string ResolvePath<T> ()
		{
			string typePath = Path.Combine(basePath, typeof(T).ToString());

			if (!Directory.Exists (typePath)) {
				Log.Information ("Creating directory " + typePath);
				Directory.CreateDirectory (typePath);
			}
			return typePath;
		}

		static private string GetExtension (Type t)
		{
			// Add the different cases of t
			return ".json";
		}

		#region IStorage implementation

		public List<T> RetrieveAll<T> () where T : IStorable
		{
			List<T> l = new List<T> ();
			string typePath = ResolvePath<T>();
			string extension = GetExtension(typeof(T));

			// Get the name of the class and look for a folder on the
			// basePath with the same name
			foreach (string path in Directory.GetFiles (typePath, "*" + extension)) {
				T t = (T)Serializer.LoadSafe<T>(path);
				Log.Information ("Retrieving " + t.ID.ToString() + " at " + typePath);
				l.Add (t);
			}
			return l;
		}

		public List<T> Retrieve<T> (Dictionary<string,object> dict) where T : IStorable
		{
			List<T> l = new List<T> ();
			string typePath = ResolvePath<T>();
			string extension = GetExtension(typeof(T));

			if (dict == null)
				return RetrieveAll<T>();

			// Get the name of the class and look for a folder on the
			// basePath with the same name
			foreach (string path in Directory.GetFiles (typePath, "*" + extension)) {
				T t = (T)Serializer.LoadSafe<T>(path);
				bool matches = true;

				foreach (KeyValuePair<string, object> entry in dict)
				{
					FieldInfo finfo = t.GetType().GetField(entry.Key);
					PropertyInfo pinfo = t.GetType().GetProperty(entry.Key);
					object ret = null;

					if (pinfo == null && finfo == null)
					{
						Log.Warning ("Property/Field does not exist " + entry.Key);
						matches = false;
						break;
					}

					if (pinfo != null)
						ret = pinfo.GetValue(t, null);
					else
						ret = finfo.GetValue(t);

					if (ret == null && entry.Value != null)
					{
						matches = false;
						break;
					}

					if (ret != null && entry.Value == null)
					{
						matches = false;
						break;
					}

					if (ret.GetType() == entry.Value.GetType())
					{
						if (Object.Equals(ret, entry.Value))
						{
							matches = true;
						}
					}
				}

				if (matches)
				{
					Log.Information ("Retrieving " + t.ID.ToString() + " at " + typePath);
					l.Add (t);
				}
			}
			return l;
		}


		public void Store<T> (T t) where T : IStorable
		{
			string typePath = ResolvePath<T>();
			string extension = GetExtension(typeof(T));

			// Save the object as a file on disk
			Log.Information ("Storing " + t.ID.ToString() + " at " + typePath);
			Serializer.Save<T>(t, Path.Combine(typePath, t.ID.ToString()) + extension);
		}

		public void Delete<T> (T t) where T : IStorable
		{
			string typePath = ResolvePath<T>();
			string extension = GetExtension(typeof(T));

			try {
				Log.Information ("Deleting " + t.ID.ToString() + " at " + typePath);
				File.Delete (Path.Combine(typePath, t.ID.ToString()) + extension);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}

		public void Reset()
		{
			Log.Information ("Deleting " + basePath + " recursively");
			Directory.Delete(basePath, true);
		}
		#endregion
	}
}

