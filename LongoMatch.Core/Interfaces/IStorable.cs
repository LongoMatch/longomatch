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
	public interface IStorable : IIDObject, IChanged
	{
		List<IStorable> Children { get; }

		/// <summary>
		/// The storage associated to this object in cases it's partially
		/// loaded and needs to storage to be filled.
		/// </summary>
		IStorage Storage {get; set;}

		/// <summary>
		/// Defines if an object is partially or fully loaded.
		/// Some database queries can return partially loaded objects
		/// that are filled once the first unintialized property is accessed.
		/// </summary>
		bool IsLoaded {get; set;}
	}
}

