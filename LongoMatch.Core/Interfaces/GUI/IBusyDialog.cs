// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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

namespace LongoMatch.Core.Interfaces.GUI
{
	public interface IBusyDialog
	{
		/// <summary>
		/// Advance the progress bar to indicate progress.
		/// </summary>
		void Pulse ();

		/// <summary>
		/// Hide and destroy the dualog.
		/// </summary>
		void Destroy ();

		/// <summary>
		/// Show the dialog asyncrhonously and pulse at the intervall defined by <paramref name="pulseIntervalMS"/>.
		/// Use <see cref="Destroy()"/> to close the dialog.
		/// </summary>
		/// <param name="pulseIntervalMS">The pulse interval in milliseconds.</param>
		void Show (uint pulseIntervalMS=100);

		/// <summary>
		/// Show the dialog synchronously and run the <paramref name="action"/> in background.
		/// The dialog is closed automatically once the task has finished.
		/// </summary>
		/// <param name="action">The action to run in the background.</param>
		/// <param name="pulseIntervalMS">The pulse interval in milliseconds.</param>
		void ShowSync (Action action, uint pulseIntervalMS=0);
	}
}

