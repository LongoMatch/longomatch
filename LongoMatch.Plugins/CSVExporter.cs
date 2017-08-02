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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Templates;
using VAS.Core;
using VAS.Core.Store;
using VAS.Plugins;

namespace LongoMatch.Plugins
{
	public class CSVExporter : ProjectExporterBase, IExportProject
	{
		public override string Name {
			get {
				return Catalog.GetString ("CSV export plugin");
			}
		}

		public override string Description {
			get {
				return Catalog.GetString ("Export project to CSV file");
			}
		}

		public override string Format {
			get {
				return "CSV";
			}
		}

		public override string Extension {
			get {
				return ".csv";
			}
		}

		protected override async Task ExportProject (Project project, string filename)
		{
			ProjectToCSV exporter = new ProjectToCSV (project as LMProject, filename);
			await Task.Run (() => exporter.Export ());
			ExportDone = true;
		}
	}

	class ProjectToCSV
	{
		LMProject project;
		string filename;
		List<string> output;

		public ProjectToCSV (LMProject project, string filename)
		{
			this.project = project;
			this.filename = filename;
			output = new List<string> ();
		}

		public void Export ()
		{
			foreach (var eventType in project.EventTypes) {
				ExportCategory (eventType);
			}
			File.WriteAllLines (filename, output);
		}

		string TeamName (IList<LMTeam> teams)
		{
			if (teams.Count == 0) {
				return "";
			} else if (teams.Count == 1) {
				return teams.First ().TeamName;
			} else {
				return "ALL";
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

			foreach (LMTimelineEvent play in plays.OrderBy (p => p.Start)) {
				string line;

				line = String.Format ("{0};{1};{2};{3};{4};{5}", play.Name,
					play.EventTime == null ? "" : play.EventTime.ToMSecondsString (),
					play.Start.ToMSecondsString (),
					play.Stop.ToMSecondsString (),
					TeamName (play.Teams.Cast<LMTeam> ().ToList ()),
					String.Join (" | ", play.Players));

				if (evt is ScoreEventType) {
					line += ";" + (evt as ScoreEventType).Score.Points;
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
