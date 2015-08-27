//
//  Copyright (C) 2015 Fluendo S.A.
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
using Couchbase.Lite;
using LongoMatch.Core.Interfaces;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LongoMatch.DB
{
	public class SerializationContext
	{

		public SerializationContext (Database db, Type parentType)
		{
			DB = db;
			ParentType = parentType;
			Cache = new StorableObjectsCache ();
			Stack = new Stack<IStorable> ();
		}

		void Init (Database db, Revision rev, StorableObjectsCache cache)
		{
			Cache = cache ?? new StorableObjectsCache ();
		}

		public Database DB {
			get;
			set;
		}

		public Type ParentType {
			get;
			set;
		}

		public JsonConverter Converter {
			get;
			set;
		}

		public StorableObjectsCache Cache {
			get;
			set;
		}

		public Stack<IStorable> Stack {
			get;
			set;
		}
	}
}

