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
using System.Collections;
using System.Reflection;
using Gdk;
using Mono.Unix;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;
using LongoMatch.TimeNodes;

namespace LongoMatch.DB
{
	
	
	public sealed class DataBase
	{
		// Database container
		private IObjectContainer db;
		// File path of the database
		private string file;
		// Lock object 
		private object locker;
		
		private Version dbVersion;
		
		private const int MAYOR=0;
		
		private const int MINOR=1;
		
		public DataBase(string file)
		{
			this.file = file;
			if (!System.IO.File.Exists(file)){
				// Create new DB and add version
				db = Db4oFactory.OpenFile(file);
				try{					
					dbVersion= new Version(MAYOR,MINOR);
					db.Set(dbVersion);
				}
				finally{
					db.Close();
				}
			}
			
			else{
				db = Db4oFactory.OpenFile(file);
				try	{   				
					IQuery query = db.Query();
					query.Constrain(typeof(Version));
					IObjectSet result = query.Execute();
					if (result.HasNext()){
						dbVersion = (Version)result.Next();						
					}					
					else{
						dbVersion = new Version (0,0);
					}
				}				
				finally
				{
					db.Close();
					
				}
			}
			locker = new object();
		}
		
		public Version Version{
			get{return dbVersion;}
		}
		
		public ArrayList GetAllDB(){			
			lock(this.locker){
				ArrayList allDB = new ArrayList();
				db = Db4oFactory.OpenFile(file);
				try	{   				
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					IObjectSet result = query.Execute();
					while (result.HasNext()){
						allDB.Add(result.Next());					
					}					
					return allDB;					
				}				
				finally
				{
					db.Close();
				}		
			}
		}
		
		public Project GetProject(String filename){
			lock(this.locker){
				db = Db4oFactory.OpenFile(file);
				try	{   				
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(filename);
					IObjectSet result = query.Execute();
					return (Project)result.Next();
				}				
				finally
				{
					db.Close();
				}
			}
		}
		
		public void AddProject (Project project){
			lock(this.locker){
				db = Db4oFactory.OpenFile(file);				
				try	
				{
					if (!this.Exists(project.File.FilePath)){
						db.Set (project);
						db.Commit();
					}
					else throw new Exception (Catalog.GetString("The Project for this video file already exists.")+"\n"+Catalog.GetString("Try to edit it whit the Database Manager"));
				}				
				finally {
					db.Close();
				}
			}			
		}
		
		public void RemoveProject(Project project){
			lock(this.locker){
				SetCascadeOptions();
				db = Db4oFactory.OpenFile(file);
				try	{			
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(project.File.FilePath);
					IObjectSet result = query.Execute();
					project = (Project)result.Next();
					db.Delete(project);   			
					db.Commit();
				}				
				finally
				{
					db.Close();
				}
			}
		}
		
		public void UpdateProject(Project project, string previousFileName){
			lock(this.locker){
				bool error = false;
				
				// Configure db4o to cascade on delete for each one of the objects stored in a Project
				SetCascadeOptions();
				db = Db4oFactory.OpenFile(file);
				try	{
					// We look for a project with that uses the same file
					if (!Exists(project.File.FilePath)){
						// Delete the old project
						IQuery query = db.Query();
						query.Constrain(typeof(Project));
						query.Descend("file").Descend("filePath").Constrain(previousFileName);
						IObjectSet result = query.Execute();  
						Project fd = (Project)result.Next();
						db.Delete(fd);
						// Add the updated project
						db.Set(project);	
						db.Commit();
					}
					else 
						error = true;					
				}
				finally{
					db.Close();
					if (error)
						throw new Exception();
				}
			}			
		}
		
		public void UpdateProject(Project project){
			lock(this.locker){
				SetCascadeOptions();				
				db = Db4oFactory.OpenFile(file);
				try	{				
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(project.File.FilePath);
					IObjectSet result = query.Execute();  
					Project fd = (Project)result.Next();
					db.Delete(fd);
					db.Set(project);		
					db.Commit();
				}				
				finally
				{
					db.Close();
				}
			}			
		}
		
		private void SetCascadeOptions(){
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
		
		private bool Exists(string filename){
			
			IQuery query = db.Query();
			query.Constrain(typeof(Project));
			query.Descend("file").Descend("filePath").Constrain(filename);
			IObjectSet result = query.Execute();
			return (result.HasNext());
		}
	}				

}
