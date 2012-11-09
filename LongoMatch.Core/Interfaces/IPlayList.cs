// IPlayList.cs
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
using System.Collections;
using LongoMatch.Store;

namespace LongoMatch.Interfaces {

	public interface IPlayList:IEnumerable
	{
		int Count {
			get;
		}
		
		void Save();
		
		void Save(string path);

		void Add(PlayListPlay play);

		bool Remove(PlayListPlay play);

		PlayListPlay Next();

		PlayListPlay Prev();

		PlayListPlay Select(int index);
		
		void SetActive(PlayListPlay play);

		void Reorder(int indexIn, int indexOut);

		int GetCurrentIndex();

		bool HasNext();

		bool HasPrev();

		IPlayList Copy();
	}
}
