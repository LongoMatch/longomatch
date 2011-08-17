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
		
		/// <summary>
		/// Initialize the Database
		/// </summary>
		public void Init() {
			/* Create a new DB if it doesn't exists yet */
			if(!System.IO.File.Exists(file))
				CreateNewDB();
			
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
			SetUpdateCascadeOptions();
			List<ProjectDescription> list = new List<ProjectDescription>();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			db.Ext().Configure().ActivationDepth(1);
			try	{
				IQuery query = db.Query();
				query.Constrain(typeof(Project));
				IObjectSet result = query.Execute();
				while(result.HasNext()) {
					try {
						Project p = (Project)result.Next();
						ProjectDescription desc = p.Description;
						db.Activate(desc,3);
						try{
							//FIXME: It happens that the project's File object is set to null?¿?¿
							// In that case, reset the value to let the user change it with the
							// projects manager.
							if(desc.File.FilePath == null) {}
						} catch {
							MessagePopup.PopupMessage(null, MessageType.Warning,
							                          Catalog.GetString("Error retrieving the file " +
							                                            "info for project:")+
							                          " "+ desc.Title+"\n"+
							                          Catalog.GetString("This value will be reset. " +
							                                            "Remember to change it later with the " +
							                                            "projects manager"));
							desc.File = new PreviewMediaFile {
								FilePath = Catalog.GetString("Change Me"),
								VideoHeight = 0,
								VideoWidth = 0,
								HasVideo = false,
								HasAudio = false,
								Length = 0,
								Fps = 0,
								VideoCodec = "",
								AudioCodec = "",
								Preview = null,
							};
							db.Store(p);
						}
						list.Add(desc);
					} catch {
						Console.WriteLine("Error retreiving project. Skip");
					}
				}
				return list;
			}
			finally
			{
				CloseDB(db);
			}
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
		public Project GetProject(String filename) {
			Project ret;
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try	{
				IQuery query = GetQueryWithContrains(db, file);
				IObjectSet result = query.Execute();
				ret = (Project) db.Ext().PeekPersisted(result.Next(),10,true);
				return ret;
			}
			finally
			{
				CloseDB(db);
			}
		}

		/// <summary>
		/// Add a project to the databse
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to add
		/// </param>
		public void AddProject(Project project) {
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try
			{
				if(!Exists(project.Description.File.FilePath,db)) {
					db.Store(project);
					db.Commit();
				}
				else throw new Exception(Catalog.GetString("The Project for this video file already exists.")+"\n"+Catalog.GetString("Try to edit it with the Database Manager"));
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
		public void RemoveProject(string filePath) {
			SetDeleteCascadeOptions();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try	{
				IQuery query = GetQueryWithContrains(db, file);
				IObjectSet result = query.Execute();
				Project project = (Project)result.Next();
				db.Delete(project);
				db.Commit();
			}
			finally
			{
				CloseDB(db);
			}
		}

		/// <summary>
		/// Updates a project in the database. Because a <see cref="LongoMatch.DB.Project"/> has
		/// many objects associated, a simple update would leave in the databse many orphaned objects.
		/// Therefore we need to delete the old project a replace it with the changed one. We need to
		/// now the old file path associate to this project in case it has been changed in the update
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to update
		/// </param>
		/// <param name="previousFileName">
		/// A <see cref="System.String"/> with the old file path
		/// </param>
		public void UpdateProject(Project project, string previousFileName) {
			bool error = false;
			// Configure db4o to cascade on delete for each one of the objects stored in a Project
			SetDeleteCascadeOptions();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try	{
				// We look for a project with the same filename
				if(!Exists(project.Description.File.FilePath,db)) {
					IQuery query = GetQueryWithContrains(db, file);
					IObjectSet result = query.Execute();
					//Get the stored project and replace it with the new one
					if(result.Count == 1) {
						Project fd = (Project)result.Next();
						db.Delete(fd);
						// Add the updated project
						db.Store(project);
						db.Commit();
					} else {
						error = true;
					}
				}
				else
					error = true;
			}
			finally {
				CloseDB(db);
				if(error)
					throw new Exception();
			}
		}

		/// <summary>
		/// Updates a project in the databse whose file path hasn't changed
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to update
		/// </param>
		public void UpdateProject(Project project) {
			SetDeleteCascadeOptions();
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try	{
				IQuery query = GetQueryWithContrains(db, file);
				IObjectSet result = query.Execute();
				//Get the stored project and replace it with the new one
				Project fd = (Project)result.Next();
				db.Delete(fd);
				db.Store(project);
				db.Commit();
			}
			finally
			{
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
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try {
				return Exists(project.Description.File.FilePath, db);
			} catch {
				return false;
			} finally {
				CloseDB(db);
			}
		}
		
		private void CreateNewDB () {
			// Create new DB and add version and last backup date
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try {
				dbVersion= new Version(MAYOR,MINOR);
				lastBackup = new BackupDate { Date = DateTime.UtcNow};
				db.Store(dbVersion);
				db.Store(lastBackup);
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
			BackupDate lastBackup = GetObject<BackupDate> ();
			if (lastBackup == null)
				lastBackup = new BackupDate {Date = DateTime.UtcNow};
			Log.Information("Last DB backup: "+ lastBackup.Date.ToShortDateString());
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
			lastBackup = new BackupDate {Date = now};
			UpdateBackupDate();
		}

		private IQuery GetQueryWithContrains(IObjectContainer db, string filename) {
			IQuery query = db.Query();
			query.Constrain(typeof(Project));
			query.Descend("description").Descend("file").Descend("filePath").Constrain(filename);
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

		private bool Exists(string filename, IObjectContainer db) {
			IQuery query = db.Query();
			query.Constrain(typeof(Project));
			query.Descend("file").Descend("filePath").Constrain(filename);
			IObjectSet result = query.Execute();
			return (result.HasNext());
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
