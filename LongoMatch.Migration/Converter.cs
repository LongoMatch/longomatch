//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using System.Linq;
using System.IO;
using LongoMatch.Core.Common;
using LongoMatch.Core.Store.Templates;
using LongoMatch.Core.Store;
using System.Collections.Generic;
using LongoMatch.DB;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Migration
{
	public class Converter
	{
		
		public static void ConvertCategories (string inputPath, string outputPath) {
			Categories cats = SerializableObject.Load<Categories> (inputPath,
			                                                       SerializationType.Binary);
			foreach (Category cat in cats) {
				cat.UUID = Guid.NewGuid ();
				List<SubCategory> l = new List<SubCategory>();
				foreach (ISubCategory subcat in cat.SubCategories) {
					if (subcat is TagSubCategory) {
						SubCategory s = new SubCategory ();
						s.Name = subcat.Name;
						s.AllowMultiple = subcat.AllowMultiple;
						s.Options = (subcat as TagSubCategory).ToList();
						l.Add (s);
					}
				}
				cat.SubCategoriesList = l;
			}
			cats.ID = Guid.NewGuid ();
			SerializableObject.Save (cats, outputPath);			
		}
		
		public static void ConvertTeamTemplate (string inputPath, string outputPath) {
			TeamTemplate team = SerializableObject.Load<TeamTemplate> (inputPath,
			                                                           SerializationType.Binary);
			foreach (Player p in team) {
				p.ID = Guid.NewGuid ();
			}
			team.UUID = Guid.NewGuid ();
			team.FormationStr = "1-4-3-3";
			SerializableObject.Save (team, outputPath);			
		}
		
		public static void ConvertProject (Project project, string outputDir) {
			Dictionary <TagSubCategory, SubCategory> dict = new Dictionary<TagSubCategory, SubCategory>();
			
			project.Timers = new List<Timer>();
			project.Periods = new List<Period>();
			for (int i=0; i < project.Categories.GamePeriods.Count; i++) {
				int duration = project.Description.File.Duration.MSeconds;
				int periodDuration = duration / project.Categories.GamePeriods.Count;
				string period = project.Categories.GamePeriods[i];

				Period p = new Period {Name = period};
				p.Start (new Time (i * periodDuration));
				p.Stop (new Time (i * periodDuration + periodDuration));
				project.Periods.Add (p);
			}
			
			foreach (Player p in project.LocalTeamTemplate) {
				p.ID = Guid.NewGuid ();
			}
			project.LocalTeamTemplate.UUID = Guid.NewGuid ();
			foreach (Player p in project.VisitorTeamTemplate) {
				p.ID = Guid.NewGuid ();
			}
			project.LocalTeamTemplate.UUID = Guid.NewGuid ();
			foreach (Category cat in project.Categories) {
				cat.UUID = Guid.NewGuid ();
				List<SubCategory> l = new List<SubCategory>();
				foreach (ISubCategory subcat in cat.SubCategories) {
					if (subcat is TagSubCategory) {
						SubCategory s = new SubCategory ();
						s.Name = subcat.Name;
						s.AllowMultiple = subcat.AllowMultiple;
						s.Options = (subcat as TagSubCategory).ToList();
						l.Add (s);
						dict.Add (subcat as TagSubCategory, s);
					}
				}
				cat.SubCategoriesList = l;
			}
			project.Categories.ID = Guid.NewGuid ();
			
			foreach (Play play in project.AllPlays ()) {
				Coordinates c;
				
				List<Tag> tags = new List<Tag> ();
				foreach (StringTag t in play.Tags.Tags) {
					tags.Add (new Tag {SubCategory = dict[t.SubCategory as TagSubCategory], Value = t.Value});
				}
				play.TagsList = tags;

				play.ID = Guid.NewGuid ();
				foreach (Player player in play.Players.GetTagsValues()) {
					play.PlayersList.Add (player);
				}
				
				if (play.FieldPosition != null) {
					c = new Coordinates ();
					foreach (Point p in play.FieldPosition) {
						Point newp = p.Normalize (project.Categories.FieldBackground.Width,
						                          project.Categories.FieldBackground.Height);
						c.Add (newp);
					}
					play.FieldPosition = c;
				}
				
				if (play.HalfFieldPosition != null) {
					c = new Coordinates ();
					foreach (Point p in play.HalfFieldPosition) {
						Point newp = p.Normalize (project.Categories.HalfFieldBackground.Width,
						                          project.Categories.HalfFieldBackground.Height);
						c.Add (newp);
					}
					play.HalfFieldPosition = c;
				}
				
				if (play.GoalPosition != null) {
					c = new Coordinates ();
					foreach (Point p in play.GoalPosition) {
						Point newp = p.Normalize (project.Categories.GoalBackground.Width,
						                          project.Categories.GoalBackground.Height);
						c.Add (newp);
					}
					play.GoalPosition = c;
				}
			}
			SerializableObject.Save (project, Path.Combine (outputDir, project.UUID.ToString()));			
		}
		
		public static void ConvertDB (string dbfile, string outputdir) {
			string dboutputdir;
			string dbname;
			DataBase db;
			
			dbname = Path.GetFileName (dbfile).Split('.')[0] + ".ldb";
			
			dboutputdir = Path.Combine (outputdir, Path.Combine (outputdir, dbname));
			if (!Directory.Exists (dboutputdir)) {
				Directory.CreateDirectory (dboutputdir);
			}
			
			db = new DataBase (dbfile);
			foreach (ProjectDescription pd in db.GetAllProjects ()) {
					Project p = db.GetProject (pd.UUID);
					LongoMatch.Migration.Converter.ConvertProject (p, dboutputdir);
			}
		}
	}
}

