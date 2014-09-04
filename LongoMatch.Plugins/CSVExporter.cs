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
using Mono.Unix;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using System.IO;
using LongoMatch.Interfaces;

namespace LongoMatch.Plugins
{
	[Extension]
	public class CSVExporter:IExportProject
	{
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
			string filename = guiToolkit.SaveFile (Catalog.GetString ("Output file"), null,
			                                       Config.HomeDir, "CSV",
			                                       new string[] { ".csv" });
			
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
			foreach (Categoryoutton cat in project.Dashboard.List) {
				ExportCategory (cat);
			}
			File.WriteAllLines (filename, output);
		}

		void ExportCategory (CategoryButton cat)
		{
			string headers;
			List<TimelineEvent> plays;
			
			output.Add ("CATEGORY: " + cat.Name);
			plays = project.PlaysInCategory (cat);
			
			/* Write Headers for this category */
			headers = "Name;Start;Stop;Team";
			foreach (Tag tag in cat.Tags) {
				headers += String.Format (";{0}", tag.Value);
			}
			
			foreach (TimelineEvent play in plays.OrderBy(p=>p.Start)) {
				string line;
				
				line = String.Format ("{0};{1};{2};{3}", play.Name,
				                      play.Start.ToMSecondsString (),
				                      play.Stop.ToMSecondsString (),
				                      play.Team);
				
				/* Strings Tags */
				foreach (Tag tag in cat.Tags) {
					line += ";" + (play.Tags.Contains (tag) ? "1" : "0");
				}
				
				output.Add (line);
			}
			output.Add ("");
		}
	}
}
