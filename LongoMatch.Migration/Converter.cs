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
using LongoMatch.Common;
using LongoMatch.Store.Templates;
using LongoMatch.Store;
using System.Collections.Generic;
using LongoMatch.DB;
using LongoMatch.Interfaces;
using LongoMatch.Core.Interfaces;
using System.Collections.ObjectModel;

namespace LongoMatch.Migration
{
	public class Converter
	{

		static LongoMatch.Core.Common.Point ConvertPoint (Point newp)
		{
			return new LongoMatch.Core.Common.Point (newp.DX, newp.DY);
		}

		static LongoMatch.Core.Common.Image ConvertImage (Image image)
		{
			if (image == null) {
				return null;
			}
			return new LongoMatch.Core.Common.Image (image.Value); 
		}

		static LongoMatch.Core.Store.Time ConvertTime (Time time)
		{
			return new LongoMatch.Core.Store.Time (time.MSeconds); 
		}

		static LongoMatch.Core.Common.Color ConvertColor (System.Drawing.Color color)
		{
			return new LongoMatch.Core.Common.Color (color.R, color.G, color.B); 
		}

		static LongoMatch.Core.Store.HotKey ConvertHotkey (HotKey hotkey)
		{
			return new LongoMatch.Core.Store.HotKey { Key = hotkey.Key, Modifier = hotkey.Modifier }; 
		}

		static LongoMatch.Core.Store.FrameDrawing ConvertFrameDrawing (Drawing keyFrameDrawing)
		{
			if (keyFrameDrawing == null)
				return null;
			var frameDrawing = new LongoMatch.Core.Store.FrameDrawing ();
			frameDrawing.Freehand = ConvertImage (keyFrameDrawing.Pixbuf);
			frameDrawing.Render = new LongoMatch.Core.Store.Time (keyFrameDrawing.RenderTime);
			frameDrawing.Pause = new LongoMatch.Core.Store.Time (5000);
			return frameDrawing;
		}

		static LongoMatch.Core.Store.MediaFile ConvertMediaFile (MediaFile file)
		{
			var newf = new LongoMatch.Core.Store.MediaFile ();
			newf.AudioCodec = file.AudioCodec;
			newf.Container = file.Container;
			newf.Duration = ConvertTime (file.Duration);
			newf.FilePath = file.FilePath;
			newf.Fps = file.Fps;
			newf.HasAudio = file.HasAudio;
			newf.HasVideo = file.HasVideo;
			newf.Par = file.Par;
			newf.Preview = ConvertImage (file.Preview);
			newf.VideoCodec = file.VideoCodec;
			newf.VideoHeight = file.VideoHeight;
			newf.VideoWidth = file.VideoWidth;
			return newf;
		}

		static LongoMatch.Core.Store.ProjectDescription ConvertProjectDescription (ProjectDescription desc)
		{
			var newdesc = new LongoMatch.Core.Store.ProjectDescription ();
			newdesc.Season = desc.Season;
			newdesc.Competition = desc.Competition;
			newdesc.Category = desc.Category;
			newdesc.LocalName = desc.LocalName;
			newdesc.VisitorName = desc.VisitorName;
			newdesc.LocalGoals = desc.LocalGoals;
			newdesc.VisitorGoals = desc.VisitorGoals;
			newdesc.MatchDate = desc.MatchDate;
			newdesc.LastModified = desc.LastModified;
			newdesc.FileSet = new LongoMatch.Core.Store.MediaFileSet ();
			newdesc.FileSet.Add (ConvertMediaFile (desc.File));
			return newdesc;
		}

		static LongoMatch.Core.Store.Player ConvertPlayer (Player p)
		{
			var player = new LongoMatch.Core.Store.Player ();
			if (p.ID == Guid.Empty) {
				p.ID = new Guid ();
			} else {
				player.ID = p.ID;
			}
			player.Name = p.Name;
			player.Position = p.Position;
			player.Number = p.Number;
			player.Photo = ConvertImage (p.Photo);
			player.Birthday = p.Birthday;
			player.Nationality = p.Nationality;
			player.Height = p.Height;
			player.Weight = p.Weight;
			player.Playing = p.Playing;
			player.Mail = p.Mail;
			return player;
		}

		public static LongoMatch.Core.Store.Templates.Dashboard ConvertCategories (Categories cats,
		                                                                           out Dictionary <TagSubCategory, List<LongoMatch.Core.Store.Tag>> dict,
		                                                                           out Dictionary <Category, LongoMatch.Core.Store.EventType > eventTypesDict)
		{
			dict = new Dictionary<TagSubCategory, List<LongoMatch.Core.Store.Tag>> ();
			eventTypesDict = new Dictionary<Category, LongoMatch.Core.Store.EventType> ();
			int i = 0;
			var dashboard = new LongoMatch.Core.Store.Templates.Dashboard ();
			dashboard.Name = cats.Name;
			dashboard.Image = ConvertImage (cats.Image);
			if (cats.FieldBackground != null) {
				dashboard.FieldBackground = ConvertImage (cats.FieldBackground);
			} else {
				dashboard.FieldBackground = Config.FieldBackground;
			}
			if (cats.HalfFieldBackground != null) {
				dashboard.HalfFieldBackground = ConvertImage (cats.HalfFieldBackground);
			} else {
				dashboard.HalfFieldBackground = Config.HalfFieldBackground;
			}
			if (cats.GoalBackground != null) {
				dashboard.GoalBackground = ConvertImage (cats.GoalBackground);
			} else {
				dashboard.GoalBackground = Config.GoalBackground;
			}
			dashboard.ID = cats.ID;
			if (dashboard.ID == Guid.Empty) {
				dashboard.ID = Guid.NewGuid ();
			}
			dashboard.GamePeriods = new ObservableCollection<string> { "1", "2" };
			
			foreach (Category cat in cats) {
				var button = new LongoMatch.Core.Store.AnalysisEventButton {
					Position = new LongoMatch.Core.Common.Point (10 + (i % 7) * (120 + 10),
					                                             10 + (i / 7) * (80 + 10)),
					Width = 120,
					Height = 80,
				};
				button.BackgroundColor = ConvertColor (cat.Color);
				button.HotKey = ConvertHotkey (cat.HotKey);
				button.TagsPerRow = 4;
				button.ShowSubcategories = false;
				button.Start = ConvertTime (cat.Start);
				button.Stop = ConvertTime (cat.Stop);
				button.EventType = new LongoMatch.Core.Store.AnalysisEventType ();
				var evt = button.AnalysisEventType;
				evt.Name = cat.Name;
				evt.ID = cat.UUID;
				evt.TagGoalPosition = cat.TagGoalPosition;
				evt.TagFieldPosition = cat.TagGoalPosition;
				evt.TagHalfFieldPosition = cat.TagHalfFieldPosition;
				evt.HalfFieldPositionIsDistance = cat.HalfFieldPositionIsDistance;
				evt.FieldPositionIsDistance = cat.FieldPositionIsDistance;
				evt.SortMethod = (LongoMatch.Core.Common.SortMethodType)cat.SortMethod;
				evt.Color = ConvertColor (cat.Color);
				foreach (ISubCategory subcat in cat.SubCategories) {
					if (subcat is TagSubCategory) {
						var tagsub = subcat as TagSubCategory;
						var l = new List<LongoMatch.Core.Store.Tag> ();
						foreach (string s in tagsub) {
							var t = new LongoMatch.Core.Store.Tag (s, subcat.Name);
							l.Add (t);
							evt.Tags.Add (t);
						}
						dict.Add (tagsub, l);
					}
				}
				eventTypesDict.Add (cat, button.EventType);
				dashboard.List.Add (button);
				i++;
			}
			foreach (GameUnit gu in cats.GameUnits) {
				var timer = new LongoMatch.Core.Store.TimerButton {
					Position = new LongoMatch.Core.Common.Point (10 + (i % 7) * (120 + 10),
					                                             10 + (i / 7) * (80 + 10)),
					Width = 120,
					Height = 80,
				};
				timer.Timer = new LongoMatch.Core.Store.Timer {
					Name = gu.Name
				};
				dashboard.List.Add (timer);
				i++;
			}
			return dashboard;
		}

		static string FixPath (string outputPath)
		{
			int i = 0;
			
			while (System.IO.File.Exists (outputPath)) {
				var dir = System.IO.Path.GetDirectoryName (outputPath);
				var name = System.IO.Path.GetFileNameWithoutExtension (outputPath);
				var ext = System.IO.Path.GetExtension (outputPath);
				outputPath = System.IO.Path.Combine (dir, String.Format ("{0}_{1}{2}", name, i, ext));
				i++;
			}
			return outputPath;
		}

		public static void ConvertCategories (string inputPath, string outputPath)
		{
			Categories cats = SerializableObject.Load<Categories> (inputPath,
			                                                       SerializationType.Binary);
			Dictionary<TagSubCategory, List<LongoMatch.Core.Store.Tag>> ignore1;
			Dictionary <Category, LongoMatch.Core.Store.EventType > ignore2;
			var dashboard = ConvertCategories (cats, out ignore1, out ignore2);
			outputPath = FixPath (outputPath);
			LongoMatch.Core.Common.Serializer.Instance.Save (dashboard, outputPath);
		}

		public static LongoMatch.Core.Store.Templates.Team ConvertTeamTemplate (TeamTemplate team,
		                                                                              Dictionary <Player, LongoMatch.Core.Store.Player> teamsDict)
		{
			var newteam = new LongoMatch.Core.Store.Templates.Team ();
			newteam.Name = team.Name;
			newteam.TeamName = team.TeamName;
			newteam.Shield = ConvertImage (team.Shield);
			newteam.ID = team.UUID;
			
			foreach (Player p in team) {
				var newplayer = ConvertPlayer (p);
				newteam.List.Add (newplayer);
				if (teamsDict != null) {
					if (!teamsDict.ContainsKey (p)) {
						teamsDict.Add (p, newplayer);
					}
				}
			}
			return newteam;
		}

		public static void ConvertTeamTemplate (string inputPath, string outputPath)
		{
			TeamTemplate team = SerializableObject.Load<TeamTemplate> (inputPath,
			                                                           SerializationType.Binary);
			var newteam = ConvertTeamTemplate (team, null);
			outputPath = FixPath (outputPath);
			LongoMatch.Core.Common.Serializer.Instance.Save (newteam, outputPath);
		}

		public static void ConvertProject (Project project, string outputDir)
		{
			Dictionary<TagSubCategory, List<LongoMatch.Core.Store.Tag>> subcatsDict;
			Dictionary <Category, LongoMatch.Core.Store.EventType > eventTypesDict;
			Image field, halffield, goal;
			var teamsDict = new Dictionary <Player, LongoMatch.Core.Store.Player> ();
			var newproject = new LongoMatch.Core.Store.Project ();
			newproject.ID = project.UUID;
			newproject.LocalTeamTemplate = ConvertTeamTemplate (project.LocalTeamTemplate, teamsDict);
			newproject.VisitorTeamTemplate = ConvertTeamTemplate (project.VisitorTeamTemplate, teamsDict);
			newproject.VisitorTeamTemplate.ActiveColor = 1;
			newproject.Dashboard = ConvertCategories (project.Categories, out subcatsDict, out eventTypesDict);
			newproject.UpdateEventTypesAndTimers ();
			newproject.Description = ConvertProjectDescription (project.Description);

			if (project.Categories.GamePeriods != null) {
				for (int i = 0; i < project.Categories.GamePeriods.Count; i++) {
					int duration = project.Description.File.Duration.MSeconds;
					int periodDuration = duration / project.Categories.GamePeriods.Count;
					string period = project.Categories.GamePeriods [i];
					
					var p = new LongoMatch.Core.Store.Period { Name = period };
					p.Nodes.Add (new LongoMatch.Core.Store.TimeNode {
						Name = period,
						Start = new LongoMatch.Core.Store.Time (i * periodDuration),
						Stop = new LongoMatch.Core.Store.Time (i * periodDuration + periodDuration)
					});
					newproject.Periods.Add (p);
				}
			}
			
			field = project.Categories.FieldBackground;
			halffield = project.Categories.HalfFieldBackground;
			goal = project.Categories.GoalBackground;
			
			if (field == null) {
				field = LongoMatch.Common.Config.FieldBackground;
			}
			if (halffield == null) {
				halffield = LongoMatch.Common.Config.HalfFieldBackground;
			}
			if (goal == null) {
				goal = LongoMatch.Common.Config.GoalBackground;
			}
				
			foreach (Play play in project.AllPlays ()) {
				LongoMatch.Core.Common.Coordinates c;

				var newplay = new LongoMatch.Core.Store.TimelineEvent ();
				newplay.CamerasConfig.Add (new LongoMatch.Core.Store.CameraConfig (0));
				var fd = ConvertFrameDrawing (play.KeyFrameDrawing);
				if (fd != null) {
					newplay.Drawings.Add (fd);
				}
				newplay.EventTime = ConvertTime (play.Start + play.Duration / 2);
				newplay.Name = play.Name;
				newplay.Notes = play.Notes;
				newplay.Rate = play.Rate;
				newplay.Start = ConvertTime (play.Start);
				newplay.Stop = ConvertTime (play.Stop);
				newplay.Teams = new ObservableCollection<LongoMatch.Core.Store.Templates.Team> ();
				if (play.Team == Team.LOCAL || play.Team == Team.BOTH) {
					newplay.Teams.Add (newproject.LocalTeamTemplate);
				}
				if (play.Team == Team.VISITOR || play.Team == Team.BOTH) {
					newplay.Teams.Add (newproject.VisitorTeamTemplate);
				}

				newplay.EventType = eventTypesDict [play.Category];
				foreach (Player player in play.Players.GetTagsValues()) {
					newplay.Players.Add (teamsDict [player]);
				}

				foreach (StringTag t in play.Tags.Tags) {
					var tags = subcatsDict [t.SubCategory as TagSubCategory];
					newplay.Tags.Add (tags.FirstOrDefault (e => e.Value == t.Value));
				}

				if (play.FieldPosition != null) {
					c = new LongoMatch.Core.Common.Coordinates ();
					foreach (Point p in play.FieldPosition) {
						Point newp = p.Normalize (field.Width, field.Height);
						c.Points.Add (ConvertPoint (newp));
					}
					newplay.FieldPosition = c;
				}
				
				if (play.HalfFieldPosition != null) {
					c = new LongoMatch.Core.Common.Coordinates ();
					foreach (Point p in play.HalfFieldPosition) {
						Point newp = p.Normalize (halffield.Width, halffield.Height);
						c.Points.Add (ConvertPoint (newp));
					}
					newplay.HalfFieldPosition = c;
				}
				
				if (play.GoalPosition != null) {
					c = new LongoMatch.Core.Common.Coordinates ();
					foreach (Point p in play.GoalPosition) {
						Point newp = p.Normalize (goal.Width, goal.Height);
						c.Points.Add (ConvertPoint (newp));
					}
					newplay.GoalPosition = c;
				}
				newproject.Timeline.Add (newplay);
			}
			field.Dispose ();
			halffield.Dispose ();
			goal.Dispose ();
			LongoMatch.Core.Common.Serializer.Instance.Save (newproject, Path.Combine (outputDir, project.UUID.ToString ()));
		}

		public static void ConvertDB (string dbfile, string outputdir)
		{
			string dboutputdir;
			string dbname;
			DataBase db;
			
			dbname = Path.GetFileName (dbfile).Split ('.') [0] + ".ldb";
			
			dboutputdir = Path.Combine (outputdir, Path.Combine (outputdir, dbname));
			if (!Directory.Exists (dboutputdir)) {
				Directory.CreateDirectory (dboutputdir);
			}
			
			db = new DataBase (dbfile);
			foreach (ProjectDescription pd in db.GetAllProjects ()) {
				Project p = db.GetProject (pd.UUID);
				ConvertProject (p, dboutputdir);
			}
		}
	}
}

