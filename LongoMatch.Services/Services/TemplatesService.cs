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
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using Mono.Unix;
using LongoMatch.Services.Services;

namespace LongoMatch.Services
{
	public class TemplatesService: ITemplatesService
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
	}

	public class TemplatesProvider<T>: ITemplateProvider<T> where T: ITemplate
	{
		readonly MethodInfo methodDefaultTemplate;
		List<T> systemTemplates;
		IStorage storage;

		public TemplatesProvider (IStorage storage)
		{
			methodDefaultTemplate = typeof(T).GetMethod ("DefaultTemplate");
			systemTemplates = new List<T> ();
			this.storage = storage;
			// Create the default template, it will be added to the list
			// of system templates to make it always available on the app 
			// and also read-only
			Create ("default", 20);
		}

		public bool Exists (string name)
		{
			// FIXME we can add an Exist(Dictionary args) method on the IStorage?
			Dictionary<string, object> dict = new Dictionary<string, object> ();
			dict.Add ("Name", name);

			List<T> list = storage.Retrieve<T>(dict);
			if (list.Count == 0)
				return false;
			else
				return true;
		}

		/// <summary>
		/// Gets the templates.
		/// </summary>
		/// <value>The templates.</value>
		public List<T> Templates {
			get {
				List<T> templates = storage.RetrieveAll<T>();
				// Now add the system templates, use a copy to prevent modification of system templates.
				foreach (T stemplate in systemTemplates) {
					templates.Add (Cloner.Clone(stemplate));
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
				List<string> l = new List<string> ();
				List<T> templates = Templates;
				foreach (T template in templates)
				{
					l.Add(template.Name);
				}
				return l.Concat (systemTemplates.Select (t => t.Name)).ToList ();
			}
		}

		public T Load (string name)
		{
			T template;
			
			template = systemTemplates.FirstOrDefault (t => t.Name == name);
			if (template != null) {
				// Return a copy to prevent modification of system templates.
				return Cloner.Clone (template);
			} else {
				Dictionary<string, object> dict = new Dictionary<string, object> ();
				dict.Add ("Name", name);

				List<T> list = storage.Retrieve<T>(dict);
				if (list.Count == 0)
					throw new TemplateNotFoundException (name);
				else
					return list[0];
			}
		}

		public T LoadFile (string filename)
		{
			Log.Information ("Loading template file " + filename);
			T template = FileStorage.RetrieveFrom<T>(filename);
			return template;
		}

		public void Save (T template)
		{
			CheckInvalidChars (template.Name);
			Log.Information ("Saving template " + template.Name);
			storage.Store<T>(template);
		}

		public void Update (T template)
		{
			CheckInvalidChars (template.Name);
			Log.Information ("Updating template " + template.Name);
			Save(template);
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
				template = Cloner.Clone (template);
			}
			template.ID = new Guid ();
			template.Name = copy;
			Save (template);
		}

		public void Delete (string templateName)
		{
			try {
				Log.Information ("Deleting template " + templateName);
				T template = Load(templateName);
				if (template != null)
					storage.Delete<T>(template);
			} catch (Exception ex) {
				Log.Exception (ex);
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
		public TeamTemplatesProvider (IStorage storage): base (storage)
		{
		}
	}

	public class CategoriesTemplatesProvider : TemplatesProvider<Dashboard>, ICategoriesTemplatesProvider
	{
		public CategoriesTemplatesProvider (IStorage storage) : base(storage)
		{
		}
	}
}
