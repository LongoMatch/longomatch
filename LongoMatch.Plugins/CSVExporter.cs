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
using System.Collections.Generic;
using System.Linq;
using Mono.Addins;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Core;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using System.IO;
using LongoMatch.Core.Common;

namespace LongoMatch.Plugins
{
	[Extension]
	public class CSVExporter:IExportProject
	{
		public string Name {
			get {
				return Catalog.GetString ("CSV export plugin");
			}
		}

		public string Description {
			get {
				return Catalog.GetString ("Export project into CSV format");
			}
		}

		public string GetMenuEntryName ()
		{
			Log.Information ("Registering new export entry");
			return Catalog.GetString ("Export project to CSV file");
		}

		public string GetMenuEntryShortName ()
		{
			return "CSVExport";
		}

		public void ExportProject (Project project, IGUIToolkit guiToolkit)
		{
			string filename = guiToolkit.SaveFile (Catalog.GetString ("Output file"),
				                  Utils.SanitizePath (project.Description.Title + ".csv"),
				                  Config.HomeDir, "CSV", new [] { "*.csv" });
			
			if (filename == null)
				return;
			
			filename = System.IO.Path.ChangeExtension (filename, ".csv");
			
			try {
				ProjectToCSV exporter = new ProjectToCSV (project, filename);
				exporter.Export ();
				guiToolkit.InfoMessage (Catalog.GetString ("Project exported successfully"));
			} catch (Exception ex) {
				guiToolkit.ErrorMessage (Catalog.GetString ("Error exporting project"));
				Log.Exception (ex);
			}
		}
	}

	class ProjectToCSV
	{
		Project project;
		string filename;
		List<string> output;

		public ProjectToCSV (Project project, string filename)
		{
			this.project = project;
			this.filename = filename;
			output = new List<string> ();
		}

		public void Export ()
		{
			foreach (EventType eventType in project.EventTypes) {
				ExportCategory (eventType);
			}
			File.WriteAllLines (filename, output);
		}

		string TeamName (TeamType team)
		{
			if (team == TeamType.LOCAL) {
				return project.LocalTeamTemplate.TeamName;
			} else if (team == TeamType.VISITOR) {
				return project.VisitorTeamTemplate.TeamName;
			} else if (team == TeamType.BOTH) {
				return "ALL";
			} else {
				return "";
			}
		}

		void ExportCategory (EventType evt)
		{
			string headers;
			List<TimelineEvent> plays;
			
			output.Add ("CATEGORY: " + evt.Name);
			plays = project.EventsByType (evt);
			
			/* Write Headers for this category */
			headers = "Name;Time;Start;Stop;Team;Player";
			if (evt is ScoreEventType) {
				headers += ";Score";
			}
			if (evt is AnalysisEventType) {
				foreach (Tag tag in (evt as AnalysisEventType).Tags) {
					headers += String.Format (";{0}", tag.Value);
				}
			}
			output.Add (headers);
			
			foreach (TimelineEvent play in plays.OrderBy(p=>p.Start)) {
				string line;
				
				line = String.Format ("{0};{1};{2};{3};{4};{5}", play.Name,
					play.EventTime == null ? "" : play.EventTime.ToMSecondsString (),
					play.Start.ToMSecondsString (),
					play.Stop.ToMSecondsString (),
					TeamName (play.TaggedTeam),
					String.Join (" | ", play.Players));

				if (evt is ScoreEventType) {
					line += ";" + (play as ScoreEvent).Score.Points;
				}

				/* Strings Tags */
				if (evt is AnalysisEventType) {
					foreach (Tag tag in (evt as AnalysisEventType).Tags) {
						line += ";" + (play.Tags.Contains (tag) ? "1" : "0");
					}
				}
				output.Add (line);
			}
			output.Add ("");
		}
	}
}
