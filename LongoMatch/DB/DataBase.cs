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
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Gtk;
using Mono.Unix;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;
using LongoMatch.TimeNodes;
using LongoMatch.Gui;
using LongoMatch.Video.Utils;

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
		// Lock object
		private object locker;

		private Version dbVersion;

		private const int MAYOR=0;

		private const int MINOR=1;

		/// <summary>
		/// Creates a proxy for the database
		/// </summary>
		/// <param name="file">
		/// A <see cref="System.String"/> with the database file path
		/// </param>
		public DataBase(string file)
		{
			this.file = file;
			if (!System.IO.File.Exists(file)) {
				// Create new DB and add version
				IObjectContainer db = Db4oFactory.OpenFile(file);
				try {
					dbVersion= new Version(MAYOR,MINOR);
					db.Store(dbVersion);
				}
				finally {
					db.Close();
				}
			}
			else {
				IObjectContainer db = Db4oFactory.OpenFile(file);
				try	{
					IQuery query = db.Query();
					query.Constrain(typeof(Version));
					IObjectSet result = query.Execute();
					if (result.HasNext()) {
						dbVersion = (Version)result.Next();
					}
					else {
						dbVersion = new Version(0,0);
					}
				}
				finally
				{
					db.Close();

				}
			}
			locker = new object();
		}

		//// <value>
		/// The database version
		/// </value>
		public Version Version {
			get {
				return dbVersion;
			}
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
			lock (this.locker) {
				SetUpdateCascadeOptions();
				List<ProjectDescription> list = new List<ProjectDescription>();
				IObjectContainer db = Db4oFactory.OpenFile(file);
				db.Ext().Configure().ActivationDepth(1);
				try	{
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					IObjectSet result = query.Execute();
					while (result.HasNext()) {
						try{
							Project p = (Project)result.Next();
							try{
								db.Activate(p.File,3);
								//FIXME: It happens that the project's File object is set to null?¿?¿
								// In that case, reset the value to let the user change it with the
								// projects manager.
								if (p.File.FilePath == null){}							
							}catch{
								MessagePopup.PopupMessage(null, MessageType.Warning, 
								                          Catalog.GetString("Error retrieving the file info for project:")+" "+p.Title+"\n"+
								                          Catalog.GetString("This value will be reset. Remember to change it later with the projects manager"));
								p.File = new PreviewMediaFile(Catalog.GetString("Change Me"),0,0,false,false,"","",0,0,null);
								db.Store(p);
							}
							ProjectDescription pd = new ProjectDescription {
								File = p.File.FilePath,
								LocalName= p.LocalName,
								VisitorName = p.VisitorName,
								Season = p.Season,
								Competition = p.Competition,
								LocalGoals = p.LocalGoals,
								VisitorGoals = p.VisitorGoals,
								MatchDate = p.MatchDate,
								Preview = p.File.Preview,
								VideoCodec = p.File.VideoCodec,
								AudioCodec = p.File.AudioCodec,
								Length = new Time((int)(p.File.Length/1000)),
								Format = String.Format("{0}x{1}@{2}fps", 
								                       p.File.VideoWidth, p.File.VideoHeight, p.File.Fps),
							};
							list.Add(pd);
						}catch{	
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
			lock (this.locker) {
				IObjectContainer db = Db4oFactory.OpenFile(file);
				try	{
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(filename);
					IObjectSet result = query.Execute();
					ret = (Project) db.Ext().PeekPersisted(result.Next(),10,true);
					return ret;
				}
				finally
				{
					CloseDB(db);
				}
			}
		}

		/// <summary>
		/// Add a project to the databse
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to add
		/// </param>
		public void AddProject(Project project) {
			lock (this.locker) {
				IObjectContainer db = Db4oFactory.OpenFile(file);
				try
				{
					if (!this.Exists(project.File.FilePath,db)) {
						db.Store(project);
						db.Commit();
					}
					else throw new Exception(Catalog.GetString("The Project for this video file already exists.")+"\n"+Catalog.GetString("Try to edit it with the Database Manager"));
				}
				finally {
					CloseDB(db);
				}
			}
		}

		/// <summary>
		/// Delete a project from the database
		/// </summary>
		/// <param name="filePath">
		/// A <see cref="System.String"/> with the project's video file path
		/// </param>
		public void RemoveProject(string filePath) {
			lock (this.locker) {
				SetDeleteCascadeOptions();
				IObjectContainer db = Db4oFactory.OpenFile(file);
				try	{
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(filePath);
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
			lock (this.locker) {
				bool error = false;
				// Configure db4o to cascade on delete for each one of the objects stored in a Project
				SetDeleteCascadeOptions();
				IObjectContainer db = Db4oFactory.OpenFile(file);
				try	{
					// We look for a project with the same filename
					if (!Exists(project.File.FilePath,db)) {
						IQuery query = db.Query();
						query.Constrain(typeof(Project));
						query.Descend("file").Descend("filePath").Constrain(previousFileName);
						IObjectSet result = query.Execute();
						//Get the stored project and replace it with the new one
						if (result.Count == 1){
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
					if (error)
						throw new Exception();
				}
			}
		}

		/// <summary>
		/// Updates a project in the databse whose file path hasn't changed
		/// </summary>
		/// <param name="project">
		/// A <see cref="Project"/> to update
		/// </param>
		public void UpdateProject(Project project) {
			lock (this.locker) {
				SetDeleteCascadeOptions();
				IObjectContainer db = Db4oFactory.OpenFile(file);
				try	{
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(project.File.FilePath);
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
		public bool Exists(Project project){
			IObjectContainer db = Db4oFactory.OpenFile(file);
			try{
				return Exists(project.File.FilePath, db);
			}catch{
				return false;
			}finally{
				CloseDB(db);
			}				
		}

		private void CloseDB(IObjectContainer db) {
			db.Ext().Purge();
			db.Close();
		}

		private void SetDeleteCascadeOptions() {
			Db4oFactory.Configure().ObjectClass(typeof(Project)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Sections)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(TimeNode)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Time)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Team)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(HotKey)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Player)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(TeamTemplate)).CascadeOnDelete(true);
			Db4oFactory.Configure().ObjectClass(typeof(Drawing)).CascadeOnDelete(true);
		}

		private void SetUpdateCascadeOptions() {
			Db4oFactory.Configure().ObjectClass(typeof(Project)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Sections)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(TimeNode)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Time)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Team)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(HotKey)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Player)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(TeamTemplate)).CascadeOnUpdate(true);
			Db4oFactory.Configure().ObjectClass(typeof(Drawing)).CascadeOnUpdate(true);
		}

		private bool Exists(string filename, IObjectContainer db) {
			IQuery query = db.Query();
			query.Constrain(typeof(Project));
			query.Descend("file").Descend("filePath").Constrain(filename);
			IObjectSet result = query.Execute();
			return (result.HasNext());
		}
	}
}
