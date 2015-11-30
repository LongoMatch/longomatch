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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LongoMatch.Core.Common;
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Migration;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;

namespace LongoMatch.DB
{
	public class FileStorage : IStorage
	{
		string basePath;
		bool deleteOnDestroy;

		public FileStorage (string basePath, bool deleteOnDestroy = false)
		{
			this.basePath = basePath;
			this.deleteOnDestroy = deleteOnDestroy;
			// Make sure to create the directory
			if (!Directory.Exists (basePath)) {
				Log.Information ("Creating directory " + basePath);
				Directory.CreateDirectory (basePath);
			}
			Info = new StorageInfo ();
		}

		~FileStorage ()
		{
			if (deleteOnDestroy)
				Reset ();
		}

		public string BasePath {
			get {
				return basePath;
			}
		}

		private string ResolvePath<T> ()
		{
			string sType = ResolveType (typeof(T));
			string path;

			// Add the different cases of t
			// TODO this cases must go away once the DB is fully implemented
			if (sType == "dashboard") {
				path = "analysis";
			} else if (sType == "team") {
				path = "teams";
			} else {
				path = sType;
			}

			string typePath = Path.Combine (basePath, path);

			if (!Directory.Exists (typePath)) {
				Log.Information ("Creating directory " + typePath);
				Directory.CreateDirectory (typePath);
			}
			return typePath;
		}

		// For a T being a Dashboard, the expected directory should be dashboards
		// What we have so far is this:
		// type -> dir -> extension
		// Dashboard -> analysis (Config.AnalysisDir) -> lct (Constants.CAT_TEMPLATE_EXT)
		// Team -> teams (Config.TeamsDir) -> ltt (Constants.TEAMS_TEMPLATE_EXT)
		static private string ResolveType (Type t)
		{
			// For a type like TestTempltesService.MyService.Template split it by . to only
			// use the last part
			string[] parts = t.ToString ().Split ('.');
			string part = parts [parts.Length - 1];
			// Make it lowercase so we end into something like this: baseDir/template
			return part.ToLower ();
		}

		// Get the name of the file for a template
		static private string ResolveName (IStorable t)
		{
			string sType = ResolveType (t.GetType ());

			// TODO remove this once the DB porting is done
			if (sType == "dashboard") {
				return ((Dashboard)t).Name;
			} else if (sType == "team" || sType == "teamtemplate") {
				return ((Team)t).Name;
			} else {
				return t.ID.ToString ();
			}
		}

		static private string GetExtension (Type t)
		{
			string sType = ResolveType (t);

			// Add the different cases of t
			if (sType == "dashboard") {
				return Constants.CAT_TEMPLATE_EXT;
			} else if (sType == "team") {
				return Constants.TEAMS_TEMPLATE_EXT;
			} else {
				return ".json";
			}
		}

		#region IStorage implementation

		public StorageInfo Info {
			get;
			set;
		}

		public void Fill (IStorable storable)
		{
		}

		public T Retrieve<T> (Guid id) where T : IStorable
		{
			string typePath = ResolvePath<T> ();
			string path = Path.Combine (typePath, id.ToString () + GetExtension (typeof(T)));

			if (File.Exists (path)) {
				T t = RetrieveFrom<T> (path);
				Log.Information ("Retrieving " + path);
				return t;
			} else {
				// In case the file is not stored by id
				// TODO this must go away once the DB port is done
				IEnumerable<T> l = RetrieveAll<T> ();
				foreach (T t in l) {
					if (t.ID == id) {
						Log.Information ("Found id " + id);
						return t;
					}
				}
			}
			return default (T);
		}

		public IEnumerable<T> RetrieveAll<T> () where T : IStorable
		{
			List<T> l = new List<T> ();
			string typePath = ResolvePath<T> ();
			string extension = GetExtension (typeof(T));

			// Get the name of the class and look for a folder on the
			// basePath with the same name
			foreach (string path in Directory.GetFiles (typePath, "*" + extension)) {
				try {
					Log.Information ("Retrieving " + path);
					T t = RetrieveFrom<T> (path);

					string sType = ResolveType (typeof(T));

					// HACK: For backward compatibility issues we can't allow the file name to differ from the Name field
					// for dashboard and teams as the previous file structure was assuming that they were identical.
					if (sType == "dashboard" || sType == "team") {
						FieldInfo finfo = t.GetType ().GetField ("Name");
						PropertyInfo pinfo = t.GetType ().GetProperty ("Name");

						if (pinfo != null)
							pinfo.SetValue (t, Path.GetFileNameWithoutExtension (path));
						else if (finfo != null)
							finfo.SetValue (t, Path.GetFileNameWithoutExtension (path));
					}

					l.Add (t);

				} catch (Exception ex) {
					Log.Warning ("Could not retrieve file <" + path + ">");
					Log.Exception (ex);
				}
			}
			return l;
		}

		public IEnumerable<T> RetrieveFull<T> (QueryFilter filter, IStorableObjectsCache cache) where T : IStorable
		{
			return Retrieve<T> (filter);
		}

		public IEnumerable<T> Retrieve<T> (QueryFilter filter) where T : IStorable
		{
			List<T> l = new List<T> ();
			string typePath = ResolvePath<T> ();
			string extension = GetExtension (typeof(T));

			if (filter == null)
				return RetrieveAll<T> ();

			// In case the only keyword is name try to find the files by name
			if (filter.ContainsKey ("Name") && filter.Keys.Count == 1) {
				foreach (string name in filter["Name"]) {
					string path = Path.Combine (typePath, name + GetExtension (typeof(T)));

					if (File.Exists (path)) {
						T t = RetrieveFrom<T> (path);
						Log.Information ("Retrieving by filename " + path);
						// To avoid cases where the name of the file does not match the name of the template
						// overwrite the template name
						FieldInfo finfo = t.GetType ().GetField ("Name");
						PropertyInfo pinfo = t.GetType ().GetProperty ("Name");

						if (pinfo != null)
							pinfo.SetValue (t, name);
						else if (finfo != null)
							finfo.SetValue (t, name);

						l.Add (t);
						return l;
					}
				}
			}

			// Get the name of the class and look for a folder on the
			// basePath with the same name
			foreach (string path in Directory.GetFiles (typePath, "*" + extension)) {
				T t = RetrieveFrom<T> (path);
				bool matches = true;

				foreach (KeyValuePair<string, List<object>> entry in filter) {
					foreach (object val in entry.Value) {
						FieldInfo finfo = t.GetType ().GetField (entry.Key);
						PropertyInfo pinfo = t.GetType ().GetProperty (entry.Key);
						object ret = null;

						if (pinfo == null && finfo == null) {
							Log.Warning ("Property/Field does not exist " + entry.Key);
							matches = false;
							break;
						}

						if (pinfo != null)
							ret = pinfo.GetValue (t, null);
						else
							ret = finfo.GetValue (t);

						if (ret == null && val != null) {
							matches = false;
							break;
						}

						if (ret != null && val == null) {
							matches = false;
							break;
						}

						if (ret.GetType () == val.GetType ()) {
							if (!Object.Equals (ret, val)) {
								matches = false;
							}
						}
					}
				}

				if (matches) {
					Log.Information ("Retrieving " + path);
					l.Add (t);
				}
			}
			return l;
		}

		public void Store<T> (T t, bool forceUpdate = false) where T : IStorable
		{
			string typePath = ResolvePath<T> ();
			string extension = GetExtension (typeof(T));

			// Save the object as a file on disk
			string path = Path.Combine (typePath, ResolveName (t)) + extension;
			Log.Information ("Storing " + path);
			Serializer.Instance.Save<T> (t, path);
		}

		public void Delete<T> (T t) where T : IStorable
		{
			string typePath = ResolvePath<T> ();
			string extension = GetExtension (typeof(T));

			try {
				string path = Path.Combine (typePath, ResolveName (t)) + extension; 
				Log.Information ("Deleting " + path);
				File.Delete (path);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}

		public void Reset ()
		{
			if (File.Exists (basePath)) {
				Log.Information ("Deleting " + basePath + " recursively");
				Directory.Delete (basePath, true);
			}
		}

		#endregion

		/// <summary>
		/// Retrieves an object of type T from a file path
		/// </summary>
		/// <returns>The object found.</returns>
		/// <param name="from">The file path to retrieve the object from</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public static T RetrieveFrom<T> (string from) where T : IStorable
		{
			Log.Information ("Loading " + from);
			T storable = Serializer.Instance.LoadSafe<T> (from);
			if (storable is Project) {
				ProjectMigration.Migrate (storable as Project);
			} else if (storable is Team) {
				TeamMigration.Migrate (storable as Team);
			} else if (storable is Dashboard) {
				DashboardMigration.Migrate (storable as Dashboard);
			}
			return storable;
		}

		/// <summary>
		/// Stores an object of type T at file at
		/// </summary>
		/// <param name="t">The object to store</param>
		/// <param name="at">The filename to store the object</param>
		/// <typeparam name="T">The type of the object.</typeparam>
		public static void StoreAt<T> (T t, string at) where T : IStorable
		{
			Log.Information ("Saving " + t.ID.ToString () + " to " + at);

			if (File.Exists (at)) {
				throw new Exception ("A file already exists at " + at);
			}

			if (!Directory.Exists (Path.GetDirectoryName (at))) {
				Directory.CreateDirectory (Path.GetDirectoryName (at));
			}

			/* Don't cach the Exception here to chain it up */
			Serializer.Instance.Save<T> ((T)t, at);
		}
	}
}

