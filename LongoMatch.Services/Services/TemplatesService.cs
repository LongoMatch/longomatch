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

namespace LongoMatch.Services
{
	public class TemplatesService: ITemplatesService
	{
		private Dictionary<Type, ITemplateProvider> dict;

		public TemplatesService ()
		{
			dict = new Dictionary<Type, ITemplateProvider> ();
			dict.Add (typeof(TeamTemplate),
			          new TeamTemplatesProvider (Config.TeamsDir));
			dict.Add (typeof(Dashboard), new CategoriesTemplatesProvider (Config.AnalysisDir));
			CheckDefaultTemplates ();
		}

		private void CheckDefaultTemplates ()
		{
			foreach (ITemplateProvider t in dict.Values)
				t.CheckDefaultTemplate ();
		}

		public ITemplateProvider<T> GetTemplateProvider<T> () where T: ITemplate
		{
			if (dict.ContainsKey (typeof(T)))
				return (ITemplateProvider<T>)dict [typeof(T)];
			return null;
		}

		public ITeamTemplatesProvider TeamTemplateProvider {
			get {
				return (ITeamTemplatesProvider)dict [typeof(TeamTemplate)]; 
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
		readonly string basePath;
		readonly string extension;
		readonly MethodInfo methodLoad;
		readonly MethodInfo methodDefaultTemplate;
		List<T> systemTemplates;

		public TemplatesProvider (string basePath, string extension)
		{
			this.basePath = basePath;
			this.extension = extension;
			methodLoad = typeof(T).GetMethod ("Load");
			methodDefaultTemplate = typeof(T).GetMethod ("DefaultTemplate");
			systemTemplates = new List<T> ();
		}

		public virtual void CheckDefaultTemplate ()
		{
			string path;
			
			path = GetPath ("default");
			if (!File.Exists (path)) {
				Create ("default", 20);
			}
		}

		public bool Exists (string name)
		{
			return File.Exists (GetPath (name));
		}

		public List<T> Templates {
			get {
				List<T> templates = new List<T> ();
				
				foreach (string file in TemplatesNames) {
					try {
						templates.Add (Load (file));
					} catch (Exception ex) {
						Log.Exception (ex);
					}
				}
				return templates;
			}
		}

		public List<string> TemplatesNames {
			get {
				List<string> l = new List<string> ();
				if (!Directory.Exists (basePath)) {
					Directory.CreateDirectory (basePath);
				}
				foreach (string path in Directory.GetFiles (basePath, "*" + extension)) {
					l.Add (Path.GetFileNameWithoutExtension (path));
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
				Log.Information ("Loading template " + name);
				template = (T)methodLoad.Invoke (null, new object[] { GetPath(name) });
				template.Name = name;
				return template;
			}
		}

		public void Save (ITemplate template)
		{
			CheckInvalidChars (template.Name);
			string filename = GetPath (template.Name);
			
			Log.Information ("Saving template " + filename);
			
			if (File.Exists (filename)) {
				throw new Exception (Catalog.GetString ("A template already exists with " +
					"the name: ") + filename);
			}
			
			if (!Directory.Exists (Path.GetDirectoryName (filename))) {
				Directory.CreateDirectory (Path.GetDirectoryName (filename));
			}
			
			/* Don't cach the Exception here to chain it up */
			template.Save (filename);
		}

		public void Update (ITemplate template)
		{
			CheckInvalidChars (template.Name);
			string filename = GetPath (template.Name);
			Log.Information ("Updating template " + filename);
			/* Don't cach the Exception here to chain it up */
			template.Save (filename);
		}

		public void Register (T template)
		{
			systemTemplates.Add (template);
		}

		public void Copy (string orig, string copy)
		{
			T template;

			if (File.Exists (copy)) {
				throw new Exception (Catalog.GetString ("A template already exists with " +
					"the name: ") + copy);
			}
			
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
				File.Delete (GetPath (templateName));
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
			ITemplate t = (ITemplate)methodDefaultTemplate.Invoke (null, list);
			t.Name = templateName;
			Save (t);
		}

		void CheckInvalidChars (string name)
		{
			List<char> invalidChars;
			
			invalidChars = name.Intersect (Path.GetInvalidFileNameChars ()).ToList ();
			if (invalidChars.Count > 0) {
				throw new InvalidTemplateFilenameException (invalidChars); 
			}
		}

		string GetPath (string templateName)
		{
			return System.IO.Path.Combine (basePath, templateName) + extension;
		}
	}

	public class TeamTemplatesProvider: TemplatesProvider<TeamTemplate>, ITeamTemplatesProvider
	{
		public TeamTemplatesProvider (string basePath): base (basePath, Constants.TEAMS_TEMPLATE_EXT)
		{
		}
	}

	public class CategoriesTemplatesProvider : TemplatesProvider<Dashboard>, ICategoriesTemplatesProvider
	{
		public CategoriesTemplatesProvider (string basePath): base (basePath, Constants.CAT_TEMPLATE_EXT)
		{
		}

		public override void CheckDefaultTemplate ()
		{
			/* Do nothing now that we have a plugin that creates the default template */
		}
	}
}
