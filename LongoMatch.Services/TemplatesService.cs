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
using LongoMatch.Core.Filters;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store.Templates;
using LongoMatch.DB;

namespace LongoMatch.Services
{
	public class TemplatesService: IService
	{

		public TemplatesService (IStorage storage = null)
		{
			if (storage == null) {
				storage = new CouchbaseStorage (Config.TemplatesDir, "templates");
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

	public class TemplatesProvider<T>: ITemplateProvider<T> where T: ITemplate<T>
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
			if (systemTemplates.Any (t => t.Name == name)) {
				return true;
			}
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
				List<T> templates = storage.RetrieveAll<T> ().OrderBy (t => t.Name).ToList ();
				// Now add the system templates, use a copy to prevent modification of system templates.
				foreach (T stemplate in systemTemplates) {
					templates.Add (stemplate.Clone ());
				}
				return templates;
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
			try {
				storage.Store<T> (template, true);
			} catch (StorageException ex) {
				Config.GUIToolkit.ErrorMessage (ex.Message);
			}
		}

		public void Register (T template)
		{
			systemTemplates.Add (template);
		}

		public void Add (T template)
		{
			storage.Store (template, true);
		}

		public T Copy (T template, string newName)
		{
			CheckInvalidChars (newName);
			Log.Information (String.Format ("Copying template {0} to {1}", template.Name, newName));

			template = template.Copy (newName);
			Save (template);
			return template;
		}

		public void Delete (T template)
		{
			Log.Information ("Deleting template " + template.Name);
			if (systemTemplates.Contains (template)) {
				// System templates can't be deleted
				throw new TemplateNotFoundException<T> (template.Name);
			}
			try {
				storage.Delete<T> (template);
			} catch (StorageException ex) {
				Config.GUIToolkit.ErrorMessage (ex.Message);
			}
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
