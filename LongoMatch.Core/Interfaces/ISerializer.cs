//
//  Copyright (C) 2015 vguzman
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
using System.IO;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Interfaces
{
	public interface ISerializer
	{
		void Save<T> (T obj, Stream stream,
		              SerializationType type = SerializationType.Json)
			;

		void Save<T> (T obj, string filepath,
		              SerializationType type = SerializationType.Json)
			;

		T Load<T> (Stream stream,
		           SerializationType type = SerializationType.Json)
			;

		T Load<T> (string filepath,
		           SerializationType type = SerializationType.Json)
			;

		T LoadSafe<T> (string filepath)
			;
	}
}

