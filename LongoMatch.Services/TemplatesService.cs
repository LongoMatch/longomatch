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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LongoMatch.Core;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;

namespace LongoMatch.Services
{
	public class TemplatesService: ITemplatesService, IService
	{
		private Dictionary<Type, ITemplateProvider> dict;

		public TemplatesService (IStorage storage)
		{
			dict = new Dictionary<Type, ITemplateProvider> ();
			dict.Add (typeof(Team),
				new TeamTemplatesProvider (storage));
			dict.Add (typeof(Dashboard),
				new CategoriesTemplatesProvider (storage));
		}

		public ITemplateProvider<T> GetTemplateProvider<T> () where T: ITemplate
		{
			if (dict.ContainsKey (typeof(T)))
				return (ITemplateProvider<T>)dict [typeof(T)];
			return null;
		}

		public ITeamTemplatesProvider TeamTemplateProvider {
			get {
				return (ITeamTemplatesProvider)dict [typeof(Team)]; 
			}
		}

		public ICategoriesTemplatesProvider CategoriesTemplateProvider {
			get {
				return (ICategoriesTemplatesProvider)dict [typeof(Dashboard)]; 
			}
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
			Config.TeamTemplatesProvider = TeamTemplateProvider;
			Config.CategoriesTemplatesProvider = CategoriesTemplateProvider;
			return true;
		}

		public bool Stop ()
		{
			return true;
		}

		#endregion
	}

	public class TemplatesProvider<T>: ITemplateProvider<T> where T: ITemplate
	{
		readonly MethodInfo methodDefaultTemplate;
		protected List<T> systemTemplates;
		IStorage storage;

		public TemplatesProvider (IStorage storage)
		{
			methodDefaultTemplate = typeof(T).GetMethod ("DefaultTemplate");
			systemTemplates = new List<T> ();
			this.storage = storage;
		}

		public bool Exists (string name)
		{
			// FIXME we can add an Exist(Dictionary args) method on the IStorage?
			QueryFilter filter = new QueryFilter ();
			filter.Add ("Name", name);

			return storage.Retrieve<T> (filter).Any ();
		}

		/// <summary>
		/// Gets the templates.
		/// </summary>
		/// <value>The templates.</value>
		public List<T> Templates {
			get {
				List<T> templates = storage.RetrieveAll<T> ().ToList ();
				// Now add the system templates, use a copy to prevent modification of system templates.
				foreach (T stemplate in systemTemplates) {
					templates.Add (Cloner.Clone (stemplate));
				}
				return templates;
			}
		}

		/// <summary>
		/// Gets the templates names.
		/// For that we need to get every ITemplate from the Storage, create a new list
		/// based on every ITemplate name, and return it.
		/// </summary>
		/// <value>The templates names.</value>
		public List<string> TemplatesNames {
			// FIXME we really need to avoid this. Too many roundtrips
			get {
				return Templates.Select (t => t.Name).ToList ();
			}
		}

		public T Load (string name)
		{
			T template;

			template = systemTemplates.FirstOrDefault (t => t.Name == name);
			if (template != null) {
				// Return a copy to prevent modification of system templates.
				return template.Clone ();
			} else {
				QueryFilter filter = new QueryFilter ();
				filter.Add ("Name", name);

				template = storage.Retrieve<T> (filter).FirstOrDefault ();
				if (template == null)
					throw new TemplateNotFoundException<T> (name);
				return template;
			}
		}

		public T LoadFile (string filename)
		{
			Log.Information ("Loading template file " + filename);
			T template = FileStorage.RetrieveFrom<T> (filename);
			return template;
		}

		public void Save (T template)
		{
			CheckInvalidChars (template.Name);
			Log.Information ("Saving template " + template.Name);
			storage.Store<T> (template);
		}

		public void Update (T template)
		{
			CheckInvalidChars (template.Name);
			Log.Information ("Updating template " + template.Name);
			Save (template);
		}

		public void Register (T template)
		{
			systemTemplates.Add (template);
		}

		public void Copy (string orig, string copy)
		{
			T template;

			CheckInvalidChars (copy);
			Log.Information (String.Format ("Copying template {0} to {1}", orig, copy));
			
			template = systemTemplates.FirstOrDefault (t => t.Name == orig);
			
			if (template == null) {
				template = Load (orig);
			} else {
				template = template.Clone ();
			}
			template.ID = new Guid ();
			template.Name = copy;
			Save (template);
		}

		public void Delete (string templateName)
		{
			Log.Information ("Deleting template " + templateName);
			if (systemTemplates.Any (t => t.Name == templateName)) {
				throw new TemplateNotFoundException<T> (templateName);
			}
			T template = Load (templateName);
			storage.Delete<T> (template);
		}

		public void Create (string templateName, params object[] list)
		{
			/* Some templates don't need a count as a parameter but we include
			 * so that all of them match the same signature */
			if (list.Length == 0)
				list = new object[] { 0 };
			Log.Information (String.Format ("Creating default {0} template", typeof(T)));
			T t = (T)methodDefaultTemplate.Invoke (null, list);
			t.Name = templateName;
			t.Static = true;
			// TODO split the registration from the creation, i.e: this function must return T
			// and let the constructor register the returned template. For that we need to refactor
			// the ITemplateProvider and ITemplateProvider<T>, no need to have them separated
			Register (t);
		}

		void CheckInvalidChars (string name)
		{
			List<char> invalidChars;
			
			invalidChars = name.Intersect (Path.GetInvalidFileNameChars ()).ToList ();
			if (invalidChars.Count > 0) {
				throw new InvalidTemplateFilenameException (invalidChars); 
			}
		}
	}

	public class TeamTemplatesProvider: TemplatesProvider<Team>, ITeamTemplatesProvider
	{
		public TeamTemplatesProvider (IStorage storage) : base (storage)
		{
			Create (Catalog.GetString ("Home team"), 20);
			systemTemplates.Last ().TeamName = Catalog.GetString ("Home");
			Create (Catalog.GetString ("Away team"), 20);
			systemTemplates.Last ().TeamName = Catalog.GetString ("Away");
		}
	}

	public class CategoriesTemplatesProvider : TemplatesProvider<Dashboard>, ICategoriesTemplatesProvider
	{
		public CategoriesTemplatesProvider (IStorage storage) : base (storage)
		{
			// Create the default template, it will be added to the list
			// of system templates to make it always available on the app 
			// and also read-only
			Create (Catalog.GetString ("Default dashboard"), 20);
		}
	}
}
