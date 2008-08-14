// DB.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.IO;
using System.Collections;
using Mono.Unix;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;

namespace LongoMatch.DB
{
	
	
	public sealed class DataBase
	{
		private IObjectContainer db;
		private string file;
		private object locker;
		
		public DataBase()
		{
			file = Path.Combine (MainClass.DBDir(), "db.yap");
			locker = new object();
		}
		
		
		// Singleton to avoid various instance of DB opened at once
		public static DataBase Instance
		{
			get
			{
				return Nested.instance;
			}
		}
		
		class Nested
		{
			static Nested(){
			}
			internal static readonly DataBase instance = new DataBase();
		}
		
		
		public ArrayList GetAllDB(){
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
		
		public Project GetProject(String filename){
			lock(this.locker){
				db = Db4oFactory.OpenFile(file);
				try	{   				
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("filename").Constrain(filename);
					IObjectSet result = query.Execute();
					return (Project)result.Next();
				}
				
				finally
				{
					db.Close();
				}
			}
		}
		public void AddProject (Project fileData){
			lock(this.locker){
				db = Db4oFactory.OpenFile(file);
				
				try	
				{
					if (!this.Exists(fileData.File.FilePath)){
						db.Set (fileData);
					}
					else throw new Exception (Catalog.GetString("The Project for this video file already exists.\n Try to edit it whit the Database Manager"));
				}
				
			finally
				{
					
					db.Close();
				}
			}
			
		}
		public void RemoveProject(Project project){
			lock(this.locker){
				Db4oFactory.Configure().ObjectClass(typeof(Project)).CascadeOnDelete(true);			
				db = Db4oFactory.OpenFile(file);
				try	{			
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(project.File.FilePath);
					IObjectSet result = query.Execute();
					project = (Project)result.Next();
					db.Delete(project);   				
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
				Db4oFactory.Configure().ObjectClass(typeof(Project)).CascadeOnDelete(true);
				Db4oFactory.Configure().ObjectClass(typeof(ArrayList)).CascadeOnUpdate(true);
				db = Db4oFactory.OpenFile(file);
				try	{
					// Buscamos si ya existe un obejto Project para el archivo multimedia
					if (!Exists(project.File.FilePath)){
						//Borramos el antiguo archivo Project
						IQuery query = db.Query();
						query.Constrain(typeof(Project));
						query.Descend("file").Descend("filePath").Constrain(previousFileName);
						IObjectSet result = query.Execute();  
						Project fd = (Project)result.Next();
						db.Delete(fd);
						// Agregamos el nuevo obejto actualizado
						db.Set(project);	
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
				Db4oFactory.Configure().ObjectClass(typeof(Project)).CascadeOnDelete(true);
				Db4oFactory.Configure().ObjectClass(typeof(ArrayList)).CascadeOnUpdate(true);
				db = Db4oFactory.OpenFile(file);
				try	{				
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(project.File.FilePath);
					IObjectSet result = query.Execute();  
					Project fd = (Project)result.Next();
					db.Delete(fd);
					db.Set(project);				
				}
				
				finally
				{
					db.Close();
				}
			}
			
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
