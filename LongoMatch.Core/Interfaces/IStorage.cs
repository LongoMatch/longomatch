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

namespace LongoMatch.Core.Interfaces
{
	public interface IStorage
	{
		/// <summary>
		/// Retrieve every object of type T, where T must implement IStorable
		/// </summary>
		/// <typeparam name="T">The type of IStorable you want to retrieve.</typeparam>
		List<T> RetrieveAll<T>() where T : IStorable;

		/// <summary>
		/// Retrieve an object with the specified id.
		/// </summary>
		/// <param name="id">The object unique identifier.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T Retrieve<T> (Guid id) where T : IStorable;

		/// <summary>
		/// Retrieve every object of type T, where T must implement IStorable using on the dictionary as a filter on its properties
		/// </summary>
		/// <typeparam name="T">The type of IStorable you want to retrieve.</typeparam>
		/// <param name="filter">The dictionary used to filter the returned List</param>
		List<T> Retrieve<T>(Dictionary<string,object> filter) where T : IStorable;

		/// <summary>
		/// Store the specified object
		/// </summary>
		/// <param name="t">The object to store.</param>
		/// <typeparam name="T">The type of the object to store.</typeparam>
		void Store<T>(T t) where T : IStorable;

		/// <summary>
		/// Delete the specified object.
		/// </summary>
		/// <param name="t">The object to delete.</param>
		/// <typeparam name="T">The type of the object to delete.</typeparam>
		void Delete<T>(T t) where T : IStorable;

		/// <summary>
		/// Reset this instance. Basically will reset the storage to its initial state.
		/// On a FS it can mean to remove every file. On a DB it can mean to remove every entry.
		/// Make sure you know what you are doing before using this.
		/// </summary>
		void Reset();
	}
}

