//
//  Copyright (C) 2017 ${CopyrightHolder}
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
using LongoMatch;
using LongoMatch.Core.Store;
using LongoMatch.Core.ViewModel;
using LongoMatch.Services.State;
using NUnit.Framework;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.ViewModel;

namespace Tests.Services
{
	public class TestLMStateHelper
	{
		[Test]
		public void OpenProject_NotifiesProjectOpened_Ok ()
		{
			// Arrange
			bool projectOpened = false;
			LMProjectVM viewmodel = new LMProjectVM ();
			viewmodel.Model = new LMProject ();
			CaptureSettings settings = new CaptureSettings();
			App.Current.EventsBroker.Subscribe<OpenEvent<ProjectVM>> ((e) => { projectOpened = true; });

			// Act
			LMStateHelper.OpenProject (viewmodel, settings);

			// Assert
			Assert.True (projectOpened);
		}
	}
}
