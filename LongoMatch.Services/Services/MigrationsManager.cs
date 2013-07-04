//
//  Copyright (C) 2013 Andoni Morales Alastruey
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

using LongoMatch.Store.Templates;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using System.Collections.Generic;
using Mono.Unix;
using LongoMatch.Common;

namespace LongoMatch.Services
{
	public class MigrationsManager
	{
		TemplatesService templates;
		IDataBaseManager databaseManager;
		Version currentVersion;
		
		public MigrationsManager (TemplatesService templates, IDataBaseManager databaseManager)
		{
			this.templates = templates;
			this.databaseManager = databaseManager;
			currentVersion = new Version (Constants.DB_MAYOR_VERSION, Constants.DB_MINOR_VERSION);
		}
		
		public void StartMigration () {
			MigrateAllDB();
			MigrateAllTemplates();
		}
		
		void MigrateAllDB () {
			foreach (IDatabase db in databaseManager.Databases) {
				while (db.Version != currentVersion) {
					MigrateDB (db);
				}
			}
		}
		
		void MigrateAllTemplates () {
			foreach (Categories cat in templates.CategoriesTemplateProvider.Templates) {
				MigrateCategories (cat);
			}
			
			foreach (SubCategoryTemplate subcat in templates.SubCategoriesTemplateProvider.Templates) {
				MigrateSubCategories (subcat);
			}
			
			foreach (TeamTemplate team in templates.TeamTemplateProvider.Templates) {
				MigrateTeamTemplates (team);
			}
		}
		
		void MigrateDB (IDatabase db) {
			Version version = db.Version;
			
			if (version == null || version.Major == 2 && version.Minor == 0) {
				MigrateDB_2_0 (db);
			}
		}
		
		void MigrateCategories (Categories cats) {
			Version version = cats.Version;
			
			if (version == null || (version.Major == 2 && version.Minor == 0)) {
				MigrateCat_2_0 (cats);
			}
		}
		
		void MigrateSubCategories (SubCategoryTemplate subcat) {
			Version version = subcat.Version;
			
			if (version == null || (version.Major == 2 && version.Minor == 0)) {
				MigrateSubCat_2_0 (subcat);
			}
		}
		
		void MigrateTeamTemplates (TeamTemplate teamTemplate) {
			Version version = teamTemplate.Version;
			
			if (version == null || (version.Major == 2 && version.Minor == 0)) {
				MigrateTeamTeamplate_2_0 (teamTemplate);
			}
		}
		
		void MigrateDB_2_0 (IDatabase db) {
			Log.Information ("Migrating db " + db.Name + " to 2.1");
			db.Version = new Version (2, 1);
		}
		
		void MigrateSubCat_2_0 (SubCategoryTemplate template) {
			Log.Information ("Migrating sub category " + template.Name + " to 2.1");
			template.Version = new Version (2, 1);
			templates.SubCategoriesTemplateProvider.Update (template);
		}
		
		void MigrateTeamTeamplate_2_0 (TeamTemplate template) {
			Log.Information ("Migrating team template " + template.Name + " to 2.1");
			template.Version = new Version (2, 1);
			templates.TeamTemplateProvider.Update (template);
		}
		
		void MigrateCat_2_0 (Categories cats) {
			/*
			Migrate templates: in this version game periods are a common
			tag for all sub-categories. We need to remove the old Period
			string tag from all categories and add a GamePeriod string list
			with the same values 
			*/
			List<string> periods = null;
			
			Log.Information ("Migrating categories template " + cats.Name + " to 2.1");
			foreach (Category cat in cats) {
				ISubCategory toDelete = null;
				
				foreach (ISubCategory subcat in cat.SubCategories) {
					TagSubCategory tagSubcat = subcat as TagSubCategory;
					
					if (tagSubcat == null)
						continue;
					
					if (subcat.Name == Catalog.GetString("Period")) {
						if (periods == null) {
							periods = new List<string>();
								foreach (string tag in tagSubcat) {
								periods.Add (tag);
							}
						}
						toDelete = subcat;
						Log.Debug ("Migrated Period for category " + cat.Name);
						break;
					}
				}
				if (toDelete != null)
					cat.SubCategories.Remove (toDelete);
			} 
			cats.Version = new Version (2,1);
			templates.CategoriesTemplateProvider.Update (cats);
			Log.Information ("Migration to 2.1 done");
		}
	}
}

