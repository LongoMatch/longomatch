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
using System.Reflection;
using Mono.Unix;

using LongoMatch.Common;
using LongoMatch.Gui.Component;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;


namespace LongoMatch.Services
{

	public class TemplatesService
	{
		private Dictionary<Type, ITemplateProvider> dict;
		private List<PlayerSubCategory> playerSubcatList;
		
		public TemplatesService (string basePath)
		{
			dict = new Dictionary<Type, ITemplateProvider>();
			dict.Add(typeof(SubCategoryTemplate),
			         new TemplatesProvider<SubCategoryTemplate, string> (basePath,
			                                                 Constants.SUBCAT_TEMPLATE_EXT));
			dict.Add(typeof(TeamTemplate),
			         new TemplatesProvider<TeamTemplate, Player> (basePath,
			                                                 Constants.TEAMS_TEMPLATE_EXT));
			dict.Add(typeof(Categories),
			         new TemplatesProvider<Categories, Category> (basePath,
			                                                 Constants.CAT_TEMPLATE_EXT));
			CheckDefaultTemplates();
			CreateDefaultSubCategories();
			
		}
		
		private void CheckDefaultTemplates () {
			foreach (ITemplateProvider t in dict.Values)
				t.CheckDefaultTemplate();
		}
		
		private void CreateDefaultSubCategories () {
			PlayerSubCategory subcat;
			
			/* Local team players */
			playerSubcatList = new List<PlayerSubCategory>();
			subcat = new PlayerSubCategory{
				Name=Catalog.GetString("Local team players"), AllowMultiple=true, FastTag=true};
			subcat.Add(Team.LOCAL);
			playerSubcatList.Add(subcat);

			/* Visitor team players */
			subcat = new PlayerSubCategory{
				Name=Catalog.GetString("Visitor team players"), AllowMultiple=true, FastTag=true};
			subcat.Add(Team.VISITOR);
			playerSubcatList.Add(subcat);
			
			/* Local and Visitor team players */
			subcat = new PlayerSubCategory{
				Name=Catalog.GetString("All teams players"), AllowMultiple=true, FastTag=true};
			subcat.Add(Team.LOCAL);
			subcat.Add(Team.VISITOR);
			playerSubcatList.Add(subcat);
		}
		
		public ITemplateProvider<T, U> GetTemplateProvider<T, U>() where T: ITemplate<U> {
			if (dict.ContainsKey(typeof(T)))
				return (ITemplateProvider<T, U>)dict[typeof(T)];
			return null;
		}
		
		public ITemplateWidget<T, U> GetTemplateEditor<T, U>() where T: ITemplate<U>{
			if (typeof(T) == typeof(Categories))
				return (ITemplateWidget<T, U>) new CategoriesTemplateEditorWidget ();
			if (typeof(T) == typeof(TeamTemplate))
				return (ITemplateWidget<T, U>) new TeamTemplateEditorWidget();
			return null;
		}
		
		public ITemplateProvider<SubCategoryTemplate, string> SubCategoriesTemplateProvider {
			get {
				return (ITemplateProvider<SubCategoryTemplate, string>) dict[typeof(SubCategoryTemplate)]; 
			}
		}
		
		public ITemplateProvider<TeamTemplate, Player> TeamTemplateProvider {
			get {
				return (ITemplateProvider<TeamTemplate, Player>) dict[typeof(TeamTemplate)]; 
			}
		}

		public ITemplateProvider<Categories, Category> CategoriesTemplateProvider {
			get {
				return (ITemplateProvider<Categories, Category>) dict[typeof(Categories)]; 
			}
		}
		
		public List<PlayerSubCategory> PlayerSubcategories {
			get{
				return playerSubcatList;
			}
		}
	}
	
	public class TemplatesProvider<T, U>: ITemplateProvider<T, U> where T: ITemplate<U>
	{
		private readonly string basePath;
		private readonly string extension;
		private readonly MethodInfo methodLoad;
		private readonly MethodInfo methodDefaultTemplate;
		
		public TemplatesProvider (string basePath, string extension)
		{
			this.basePath = System.IO.Path.Combine(basePath, Constants.TEMPLATES_DIR);
			this.extension = extension;
			methodLoad = typeof(T).GetMethod("Load");
			methodDefaultTemplate = typeof(T).GetMethod("DefaultTemplate");
		}
		
		private string GetPath(string templateName) {
			return System.IO.Path.Combine(basePath, templateName) + extension;
		}
		
		public void CheckDefaultTemplate() {
			string path;
			
			path = GetPath("default");
			if(!File.Exists(path)) {
				Create("default", 20);
			}
		}
		
		public bool Exists (string name) {
			return File.Exists(GetPath(name));
		}
		
		public List<T> Templates{
			get{
				List<T> templates = new List<T>();
				
				foreach (string file in TemplatesNames) {
					try {
						templates.Add(Load(file));
					} catch (Exception ex) {
						Log.Exception(ex);
					}
				}
				return templates;
			}
		}
		
		public List<string> TemplatesNames{
			get{
				List<string> l = new List<string>();
				foreach (string path in Directory.GetFiles (basePath, "*" + extension)) {
					l.Add (Path.GetFileNameWithoutExtension(path));
				}
				return l;
			}
		}
		
		public T Load (string name) {
			Log.Information("Loading template " +  name);
			return (T)methodLoad.Invoke(null, new object[] {GetPath(name)});
		}
		
		public void Save (ITemplate<U> template) {
			string filename =  GetPath(template.Name);
			
			Log.Information("Saving template " + filename);
			
			if (File.Exists(filename)) {
				throw new Exception (Catalog.GetString("A template already exixts with " +
				                                       "the name: ") + filename);
			}
			
			/* Don't cach the Exception here to chain it up */
			template.Save(filename);
		}
		
		public void Copy(string orig, string copy) {
			if (File.Exists(copy)) {
				throw new Exception (Catalog.GetString("A template already exixts with " +
				                                       "the name: ") + copy);
			}
			
			Log.Information(String.Format("Copying template {0} to {1}", orig, copy));
			File.Copy (GetPath(orig) , GetPath(copy));
		}
		
		public void Delete (string templateName) {
			try {
				Log.Information("Deleting template " + templateName);
				File.Delete (templateName);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
		}
		
		public void Create (string templateName, params object[] list) {
			/* Some templates don't need a count as a parameter but we include
			 * so that all of them match the same signature */
			if (list.Length == 0)
				list = new object[] {0};
			Log.Information(String.Format("Creating default {0} template", typeof(T)));
			ITemplate<U> t =(ITemplate<U>)methodDefaultTemplate.Invoke(null, list);
			t.Name = templateName;
			Save(t);
		}
	}
}
