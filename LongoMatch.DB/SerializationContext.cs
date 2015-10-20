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
using Newtonsoft.Json.Serialization;

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
			SaveChildren = true;
		}

		void Init (Database db, Revision rev, StorableObjectsCache cache)
		{
			Cache = cache ?? new StorableObjectsCache ();
		}

		/// <summary>
		/// The database used in the store/retrieve process.
		/// </summary>
		public Database DB {
			get;
			set;
		}

		/// <summary>
		/// The type of the parent object being stored/retrieved.
		/// </summary>
		public Type ParentType {
			get;
			set;
		}

		/// <summary>
		/// The ID of the parent object.
		/// </summary>
		/// <value>The root I.</value>
		public Guid RootID {
			get;
			set;
		}

		/// <summary>
		/// The convert used in case it needs to be reused.
		/// </summary>
		public JsonConverter Converter {
			get;
			set;
		}

		/// <summary>
		/// A cache for retrieved objects to improve performace retrieve references of the same object.
		/// </summary>
		public IStorableObjectsCache Cache {
			get;
			set;
		}

		/// <summary>
		/// Defines if <see cref="IStorable"/> children should be also stored in the DB.
		/// </summary>
		public bool SaveChildren {
			get;
			set;
		}

		/// <summary>
		/// A stack with the object stored/retrieved.
		/// </summary>
		/// <value>The stack.</value>
		public Stack<IStorable> Stack {
			get;
			set;
		}

		/// <summary>
		/// The contract resolver in case it needs to be reused.
		/// </summary>
		/// <value>The contract resolver.</value>
		public IContractResolver ContractResolver {
			get;
			set;
		}
	}
}

