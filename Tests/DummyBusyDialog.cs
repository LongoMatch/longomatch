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
using System.Threading.Tasks;
using VAS.Core.Interfaces.GUI;

namespace Tests
{
	public class DummyBusyDialog: IBusyDialog
	{
		public DummyBusyDialog ()
		{
		}

		#region IBusyDialog implementation

		public void Pulse ()
		{
		}

		public void Destroy ()
		{
		}

		public void Show (uint pulseIntervalMS = 100u)
		{
		}

		public void ShowSync (Action action, uint pulseIntervalMS = 0u)
		{
			action.Invoke ();
		}

		public void ShowSync (Func<Task> asyncAction, uint pulseIntervalMS = 0)
		{
			asyncAction.Invoke ();
		}

		#endregion
	}
}

