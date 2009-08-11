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
using System.Collections.Generic;
using Mono.Unix;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Config;
using LongoMatch.DB.Compat.v00.TimeNodes;

namespace LongoMatch.DB.Compat.v00.DB
{
	
	
	public sealed class DataBase
	{
		// Database container
		private IObjectContainer db;
		// File path of the database
		private string file;
		// Lock object 
		private object locker;
		
		public DataBase(string file)
		{
			this.file = file;
			locker = new object();
		}
		
		
			
		public ArrayList GetAllDB(bool oldNamespace){			
			lock(this.locker){		
				ArrayList allDB = new ArrayList();
				if (oldNamespace){
					//Create an alias for the old namespace scheme
					WildcardAlias wAlias = new WildcardAlias("LongoMatch.*", "LongoMatch.DB.Compat.v00.*");
					IConfiguration configuration= Db4objects.Db4o.Db4oFactory.NewConfiguration();
					configuration.AddAlias(wAlias);	
					db = Db4oFactory.OpenFile(configuration,file);
				}
				else 
					db = Db4oFactory.OpenFile(file);
				try	{   				
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					IObjectSet result = query.Execute();
					while (result.HasNext()){
						allDB.Add(result.Next());					
					}
					return allDB;					
				}finally{
					db.Close();
				}		
			}
		}
		
		
		public Project GetProject(String filename,bool oldNamespace){
			lock(this.locker){
				if (oldNamespace){
					//Create an alias for the old namespace scheme
					WildcardAlias wAlias = new WildcardAlias("LongoMatch.*", "LongoMatch.DB.Compat.v00.*");
					IConfiguration configuration= Db4objects.Db4o.Db4oFactory.NewConfiguration();
					configuration.AddAlias(wAlias);	
					db = Db4oFactory.OpenFile(configuration,file);
				}
				else 
					db = Db4oFactory.OpenFile(file);
				try	{   				
					IQuery query = db.Query();
					query.Constrain(typeof(Project));
					query.Descend("file").Descend("filePath").Constrain(filename);
					IObjectSet result = query.Execute();
					if (result.HasNext())
					    return (Project)result.Next();
					else return null;
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
