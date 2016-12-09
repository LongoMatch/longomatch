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
using System.Linq;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;
using VAS.Core;
using VAS.Core.Interfaces;
using VAS.Services;

namespace LongoMatch.Services
{
	public class TemplatesService : IService
	{
		public TemplatesService (IStorage storage = null)
		{
			if (storage == null) {
				storage = new CouchbaseStorageLongoMatch (App.Current.TemplatesDir, "templates");
			}
			TeamTemplateProvider = new TeamTemplatesProvider (storage);
			CategoriesTemplateProvider = new CategoriesTemplatesProvider (storage);
		}

		public ITeamTemplatesProvider TeamTemplateProvider {
			get;
			protected set;
		}

		public ICategoriesTemplatesProvider CategoriesTemplateProvider {
			get;
			protected set;
		}

		#region IService

		public int Level {
			get {
				return 10;
			}
		}

		public string Name {
			get {
				return "Templates provider";
			}
		}

		public bool Start ()
		{
			App.Current.TeamTemplatesProvider = TeamTemplateProvider;
			App.Current.CategoriesTemplatesProvider = CategoriesTemplateProvider;
			return true;
		}

		public bool Stop ()
		{
			return true;
		}

		#endregion
	}

	public class TeamTemplatesProvider : TemplatesProvider<LMTeam>, ITeamTemplatesProvider
	{
		public TeamTemplatesProvider (IStorage storage) : base (storage)
		{
			Register (Create (Catalog.GetString ("Home team"), 20));
			systemTemplates.Last ().TeamName = Catalog.GetString ("Home");
			Register (Create (Catalog.GetString ("Away team"), 20));
			systemTemplates.Last ().TeamName = Catalog.GetString ("Away");
		}
	}

	public class CategoriesTemplatesProvider : TemplatesProvider<LMDashboard>, ICategoriesTemplatesProvider
	{
		public CategoriesTemplatesProvider (IStorage storage) : base (storage)
		{
			// Create the default template, it will be added to the list
			// of system templates to make it always available on the app 
			// and also read-only
			Register (Create (Catalog.GetString ("Default dashboard"), 20));
		}
	}
}
