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
using NUnit.Framework;
using System;
using LongoMatch.Services;
using LongoMatch.Services.Services;
using System.IO;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;

namespace Tests.Services
{
	[TestFixture ()]
	public class TestTemplatesService
	{
		[TearDown]
		public void RemoveStorage ()
		{
			try {
				Directory.Delete (Path.Combine (Path.GetTempPath (), "TestTemplatesService"), true);
			} catch {
			}
		}

		[Test ()]
		public void TestSystemTemplates ()
		{
			FileStorage fs = new FileStorage (
				                 Path.Combine (Path.GetTempPath (), "TestTemplatesService"));
			TemplatesService ts = new TemplatesService (fs);
			// Start service
			ts.Start ();
			ICategoriesTemplatesProvider ctp = ts.CategoriesTemplateProvider;

			// We must have at least one template provider called 'Default'
			Dashboard dash = ctp.Load ("default");
			Assert.AreNotSame (dash, null);

			// Test we dont have a template
			bool found = ctp.Exists ("NonExistingTemplate");
			Assert.AreEqual (found, false);

			// Test saving the default template
			dash.Name = "NewDefault";
			ctp.Save (dash);

			// Test loading a template from a file
			Dashboard newDefault = ctp.Load ("NewDefault");
			Assert.AreEqual (newDefault.Name, "NewDefault");
		}
	}
}

