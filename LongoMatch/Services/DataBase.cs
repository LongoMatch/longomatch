// DB.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Gui;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video.Utils;
using Mono.Unix;

namespace LongoMatch.DB
{

	/// <summary>
	/// I am a proxy for the db4o database. I can store,retrieve, update and search
	/// <see cref="LongoMatch.DB.Projects"/>.
	/// Projects are uniquely indentified by their filename, assuming that you can't
	/// create two projects for the same video file.
	/// </summary>
	public sealed class DataBase
	{
		// File path of the database
		private string file;

		private Version dbVersion;
		
		private BackupDate lastBackup;

		private const int MAYOR=2;

		private const int MINOR=0;
		
		private TimeSpan maxDaysWithoutBackup = new TimeSpan(5, 0, 0, 0);
		
		private const string backupFilename = Constants.DB_FILE + ".backup";

		
		/// <summary>
		/// Creates a proxy for the database
		/// </summary>
		/// <param name="file">
		/// A <see cref="System.String"/> with the database file path
		/// </param>
		public DataBase(string file)
		{
			this.file = file;
			Init();
			BackupDB();
		}
		
		/// <value>
		/// The database version
		/// </value>
		public Version Version {
			get {
				return dbVersion;
			}
		}
		
		public void ListObjects() {
			Dictionary<Type, int> dict = new Dictionary<Type, int>();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			
			IQuery query = db.Query();
			query.Constrain(typeof(object));
			IObjectSet result = query.Execute();
			while(result.HasNext()) {
				var res = result.Next();
				Type type = res.GetType();
				
				if (dict.ContainsKey(type))
					dict[type]++;
				else
					dict.Add(type, 1);
				
			}
			foreach (Type t in dict.Keys) {
				Log.Information(t.ToString()+":" + dict[t]);
			}
			CloseDB(db);
			
		}
		
		/// <summary>
		/// Initialize the Database
		/// </summary>
		public void Init() {
			/* Create a new DB if it doesn't exists yet */
			if(!System.IO.File.Exists(file))
				CreateNewDB();
			
			Log.Information ("Using database file: " + file);
			
			GetDBVersion();
			GetBackupDate();
			CheckDB();
		}
		
		/// <summary>
		/// Retrieve all the projects from the database. This method don't return the
		/// the whole <see cref="LongoMatch.DB.Project"/> but the projects fields to
		/// create a <see cref="LongoMatch.DB.ProjectDescription"/> to make the seek
		/// faster.
		/// </summary>
		/// <returns>
		/// A <see cref="List"/>
		/// </returns>
		public List<ProjectDescription> GetAllProjects() {
			List<ProjectDescription> list = new List<ProjectDescription>();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			
			try	{
				IQuery query = db.Query();
				query.Constrain(typeof(ProjectDescription));
				IObjectSet result = query.Execute();
				while(result.HasNext()) {
					try {
						ProjectDescription desc = (ProjectDescription)result.Next();
						list.Add(desc);
					} catch (Exception e) {
						Log.Warning("Error retreiving project. Skip");
						Log.Exception(e);
					}
				}
			}
			finally
			{
				CloseDB(db);
			}
			return list;
		}

		/// <summary>
		/// Search and return a project in the database. Returns null if the
		/// project is not found
		/// </summary>
		/// <param name="filename">
		/// A <see cref="System.String"/> with the project's video file name
		/// </param>
		/// <returns>
		/// A <see cref="LongoMatch.DB.Project"/>
		/// </returns>
		public Project GetProject(Guid id) {
			Project ret = null;
			IObjectContainer db = Db4oFactory.OpenFile(file);
			
			try	{
				IQuery query = GetQueryProjectById (db, id);
				IObjectSet result = query.Execute();
				ret = (Project) db.Ext().PeekPersisted(result.Next(),10,true);
			} catch (Exception e) {
				Log.Error("Could not get project with ID: " + id);
				Log.Exception(e);
			} finally {
				CloseDB(db);
			}
			return ret;
		}

		/// <summary>
		/// Add a project to the databse
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to add
		/// </param>
		public void AddProject(Project project) {
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try {
				db.Store(project);
				db.Commit();
			} catch (Exception e) {
				Log.Error("Could not add project");
				Log.Exception(e);
			}
			finally {
				CloseDB(db);
			}
		}

		/// <summary>
		/// Delete a project from the database
		/// </summary>
		/// <param name="filePath">
		/// A <see cref="System.String"/> with the project's video file path
		/// </param>
		public void RemoveProject(Guid id) {
			SetDeleteCascadeOptions();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try	{
				IQuery query = GetQueryProjectById(db, id);
				IObjectSet result = query.Execute();
				Project project = (Project)result.Next();
				db.Delete(project);
				db.Commit();
			} catch (Exception e) {
				Log.Error("Could not delete project");
				Log.Exception(e);
			} finally {
				CloseDB(db);
			}
			ListObjects();
		}

		/// <summary>
		/// Updates a project in the database. Because a <see cref="LongoMatch.DB.Project"/> has
		/// many objects associated, a simple update would leave in the databse many orphaned objects.
		/// Therefore we need to delete the old project a replace it with the changed one.
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to update
		/// </param>
		/// <param name="previousFileName">
		/// A <see cref="System.String"/> with the old file path
		/// </param>
		public void UpdateProject(Project project) {
			// Configure db4o to cascade on delete for each one of the objects stored in a Project
			SetDeleteCascadeOptions();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			
			try	{
				IQuery query = GetQueryProjectById(db, project.UUID);
				IObjectSet result = query.Execute();
				//Get the stored project and replace it with the new one
				if(result.Count == 1) {
					Project fd = (Project)result.Next();
					db.Delete(fd);
					db.Store(project);
					db.Commit();
				} else {
					Log.Warning("Project with ID " + project.UUID + "not found");
				}
			} catch (Exception e) {
				Log.Error("Could not update project");
				Log.Exception(e);
			} finally {
				CloseDB(db);
			}
		}

		/// <summary>
		/// Checks if a project already exists in the DataBase with the same file
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to compare
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/>
		/// </returns>
		public bool Exists(Project project) {
			bool ret;
			IObjectContainer db = Db4oFactory.OpenFile(file);
		
			try {
				IQuery query = GetQueryProjectById(db, project.UUID);
				IObjectSet result = query.Execute();
				ret = result.HasNext();
			} catch {
				ret = false;
			} finally {
				CloseDB(db);
			}
			
			return ret;
		}
		
		private void CreateNewDB () {
			// Create new DB and add version and last backup date
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try {
				dbVersion= new Version(MAYOR,MINOR);
				lastBackup = new BackupDate { Date = DateTime.UtcNow};
				db.Store(dbVersion);
				db.Store(lastBackup);
				Log.Information("Created new database:" + file);
			}
			finally {
				db.Close();
			}
		}
		
		private void GetDBVersion () {
			dbVersion = GetObject<Version>();
			if (dbVersion == null)
				dbVersion = new Version(MAYOR, MINOR);
			Log.Information("DB version: "+ dbVersion.ToString());
		}
		
		private void GetBackupDate () {
			lastBackup = GetObject<BackupDate> ();
			if (lastBackup == null)
				lastBackup = new BackupDate {Date = DateTime.UtcNow};
			Log.Information("DB last backup: "+ lastBackup.Date.ToShortDateString());
		}
		
		private void UpdateBackupDate () {
			UpdateObject(lastBackup);
		}
		
		private T GetObject<T>() {
			T ret = default(T);
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try	{
				IQuery query = db.Query();
				query.Constrain(typeof(T));
				IObjectSet result = query.Execute();
				if(result.HasNext())
					ret = (T) result.Next();
			} finally {
				db.Close();
			}
			return ret;
		}
		
		private void UpdateObject<T>(this T element) {
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try	{
				IQuery query = db.Query();
				query.Constrain(typeof(T));
				IObjectSet result = query.Execute();
				if(result.HasNext()) {
					T obj = (T) result.Next();
					obj = element;
					db.Store(obj);
				}
			} finally {
				db.Close();
			}
		}
		
		private void CheckDB() {
			/* FIXME: Check for migrations here */
		}
		
		private void BackupDB () {
			DateTime now = DateTime.UtcNow;
			if (lastBackup.Date + maxDaysWithoutBackup >= now)
				return;
		
			File.Move(file, Path.Combine(MainClass.DBDir(), backupFilename));
			Log.Information ("Created backup for database at ", backupFilename);
			lastBackup = new BackupDate {Date = now};
			UpdateBackupDate();
		}

		private IQuery GetQueryProjectById(IObjectContainer db, Guid id) {
			IQuery query = db.Query();
			query.Constrain(typeof(Project));
			query.Descend("_UUID").Constrain(id);
			return query;
		}

		private void CloseDB(IObjectContainer db) {
			db.Ext().Purge();
			db.Close();
		}

		private void SetDeleteCascadeOptions() {
			Db4oFactory.Configure().ObjectClass(typeof(Project)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Categories)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Category)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(TimeNode)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Play)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Time)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Team)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(HotKey)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Player)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(StringTag)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(TeamTemplate)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Drawing)).CascadeOnDelete(true);
		}

		private void SetUpdateCascadeOptions() {
			Db4oFactory.Configure().ObjectClass(typeof(Project)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Categories)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Category)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(TimeNode)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Play)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Time)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Team)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(HotKey)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Player)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(TeamTemplate)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(StringTag)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Drawing)).CascadeOnUpdate(true);
		}


		/* Dummy class to allow having a single instance of BackupDateTime in the DB and make it
		 * easIer to query */
		protected class BackupDate 
		{
			public DateTime Date {
				get;
				set;
			}			
		}
		
	}
	
	
}
