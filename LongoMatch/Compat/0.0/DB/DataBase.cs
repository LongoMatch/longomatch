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
using System.Collections.Generic;
using Mono.Unix;
using Db4objects.Db4o;
using Db4objects.Db4o.Query;
using Db4objects.Db4o.Config;
using LongoMatch.Compat.v00.TimeNodes;

namespace LongoMatch.Compat.v00.DB
{


	public sealed class DataBase
	{
		// Database container
		private IObjectContainer db;
		// File path of the database
		private string file;

		public DataBase(string file)
		{
			this.file = file;
		}

		public ArrayList GetAllDB() {
			ArrayList allDB = new ArrayList();
			db = Db4oFactory.OpenFile(file);
			try	{
				IQuery query = db.Query();
				query.Constrain(typeof(Project));
				IObjectSet result = query.Execute();
				while (result.HasNext())
					allDB.Add(result.Next());
				return allDB;
			} finally {
				db.Close();
			}
		}
	}
}
