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

namespace LongoMatch.Core.Interfaces
{
	public interface IProgressReport
	{
		/// <summary>
		/// Report a progress with a completition percent, a message and an id for the task.
		/// </summary>
		/// <param name="percent">Percent from 0.0 to 1.0.</param>
		/// <param name="message">Message.</param>
		/// <param name="id">Identifier for the task.</param>
		void Report (float percent, string message, Guid id = default (Guid));
	}
}

