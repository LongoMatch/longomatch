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
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;


namespace LongoMatch.Services
{

	public class TemplatesService: ITemplatesService
	{
		private Dictionary<Type, ITemplateProvider> dict;
		private List<PlayerSubCategory> playerSubcatList;
		private List<TeamSubCategory> teamSubcatList;
		
		public TemplatesService (string basePath)
		{
			dict = new Dictionary<Type, ITemplateProvider>();
			dict.Add(typeof(SubCategoryTemplate), new SubCategoriesTemplatesProvider(basePath));
			dict.Add(typeof(TeamTemplate), new TeamTemplatesProvider(basePath));
			dict.Add(typeof(Categories), new CategoriesTemplatesProvider (basePath));
			CheckDefaultTemplates();
			CreateDefaultPlayerSubCategories();
			CreateDefaultTeamSubCategories();
			
		}
		
		private void CheckDefaultTemplates () {
			foreach (ITemplateProvider t in dict.Values)
				t.CheckDefaultTemplate();
		}
		
		private void CreateDefaultPlayerSubCategories () {
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

		private void CreateDefaultTeamSubCategories () {
			teamSubcatList = new List<TeamSubCategory>();
			teamSubcatList.Add(new TeamSubCategory());
		}
		
		public ITemplateProvider<T, U> GetTemplateProvider<T, U>() where T: ITemplate<U> {
			if (dict.ContainsKey(typeof(T)))
				return (ITemplateProvider<T, U>)dict[typeof(T)];
			return null;
		}
		
		public ISubcategoriesTemplatesProvider SubCategoriesTemplateProvider {
			get {
				return (ISubcategoriesTemplatesProvider) dict[typeof(SubCategoryTemplate)]; 
			}
		}
		
		public ITeamTemplatesProvider TeamTemplateProvider {
			get {
				return (ITeamTemplatesProvider) dict[typeof(TeamTemplate)]; 
			}
		}

		public ICategoriesTemplatesProvider CategoriesTemplateProvider {
			get {
				return (ICategoriesTemplatesProvider) dict[typeof(Categories)]; 
			}
		}
		
		public List<PlayerSubCategory> PlayerSubcategories {
			get{
				return playerSubcatList;
			}
		}
		
		public List<TeamSubCategory> TeamSubcategories {
			get{
				return teamSubcatList;
			}
		}
	}
	
	public class TemplatesProvider<T, U>: ITemplateProvider<T, U> where T: ITemplate<U>
	{
		readonly string basePath;
		readonly string extension;
		readonly MethodInfo methodLoad;
		readonly MethodInfo methodDefaultTemplate;
		
		public TemplatesProvider (string basePath, string extension)
		{
			this.basePath = basePath;
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
			var template = (T)methodLoad.Invoke(null, new object[] {GetPath(name)});
			template.Name = name;
			return template;
		}
		
		public void Save (ITemplate<U> template) {
			string filename =  GetPath(template.Name);
			
			Log.Information("Saving template " + filename);
			
			if (File.Exists(filename)) {
				throw new Exception (Catalog.GetString("A template already exists with " +
				                                       "the name: ") + filename);
			}
			
			/* Don't cach the Exception here to chain it up */
			template.Save(filename);
		}
		
		public void Update (ITemplate<U> template) {
			string filename =  GetPath(template.Name);
			
			Log.Information("Updating template " + filename);
			/* Don't cach the Exception here to chain it up */
			template.Save(filename);
		}
		
		public void Copy(string orig, string copy) {
			if (File.Exists(copy)) {
				throw new Exception (Catalog.GetString("A template already exists with " +
				                                       "the name: ") + copy);
			}
			
			Log.Information(String.Format("Copying template {0} to {1}", orig, copy));
			File.Copy (GetPath(orig) , GetPath(copy));
		}
		
		public void Delete (string templateName) {
			try {
				Log.Information("Deleting template " + templateName);
				File.Delete (GetPath(templateName));
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
	
	public class TeamTemplatesProvider: TemplatesProvider<TeamTemplate, Player>, ITeamTemplatesProvider
	{
		public TeamTemplatesProvider (string basePath): base (basePath, Constants.TEAMS_TEMPLATE_EXT) {}
		 
	} 
	
	public class CategoriesTemplatesProvider : TemplatesProvider<Categories, Category>, ICategoriesTemplatesProvider
	{
		public CategoriesTemplatesProvider (string basePath): base (basePath, Constants.CAT_TEMPLATE_EXT) {}
		 
	}
	
	public class SubCategoriesTemplatesProvider : TemplatesProvider<SubCategoryTemplate, string>, ISubcategoriesTemplatesProvider
	{
		public SubCategoriesTemplatesProvider (string basePath): base (basePath, Constants.SUBCAT_TEMPLATE_EXT) {}
		 
	} 
}
