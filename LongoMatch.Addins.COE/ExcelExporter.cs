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
using System.Drawing;
using Mono.Addins;
using Mono.Unix;

using OfficeOpenXml;
using OfficeOpenXml.Style;

using LongoMatch;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using LongoMatch.Stats;
using LongoMatch.Common;

[Extension]
public class ExcelExporter:IExportProject
{
	public string GetMenuEntryName () {
		Log.Information("Registering new export entry");
		return Catalog.GetString("Export project to Excel file");
	}
	
	public string GetMenuEntryShortName () {
		return "EPPLUSExport";
	}
	
	public void ExportProject (Project project, IGUIToolkit guiToolkit) {
		string filename = guiToolkit.SaveFile(Catalog.GetString("Output file"), null,
			Config.HomeDir(), "Excel", ".xlsx");
		
		if (filename == null)
			return;
		
		filename = System.IO.Path.ChangeExtension(filename, ".xlsx");
		
		try {
			EPPLUSExporter exporter = new EPPLUSExporter(project, filename);
			exporter.Export();
			guiToolkit.InfoMessage(Catalog.GetString("Project exported successfully"));
		}catch (Exception ex) {
			guiToolkit.ErrorMessage(Catalog.GetString("Error exporting project"));
			Log.Exception(ex);
		}
	}
}

public class EPPLUSExporter {
	
	Project project;
	string filename;
	ExcelPackage package;
	ExcelWorksheet ws;
	int sheetCount = 0;
	

	public EPPLUSExporter(Project project, string filename) {
		this.project = project;
		this.filename = filename;
	}
	
	public void Export() {
		FileInfo newFile = new FileInfo(filename);
		if (newFile.Exists)
		{
			newFile.Delete();  // ensures we create a new workbook
			newFile = new FileInfo(filename);
		}
		
		using (package = new ExcelPackage(newFile)) {
			TeamStatsSheets teamStats;
			
			ws = CreateSheet(package, Catalog.GetString("Project statistics"));
			var statsSheet = new ProjectStatsSheet(ws, project);
			statsSheet.Fill();
			
			ws = CreateSheet(package, project.LocalTeamTemplate.TeamName +
				"(" + Catalog.GetString("Local Team") + ")");
			teamStats = new TeamStatsSheets(ws, project, Team.LOCAL);
			teamStats.Fill();
			
			ws = CreateSheet(package, project.VisitorTeamTemplate.TeamName +
				"(" + Catalog.GetString("Visitor Team") + ")");
			teamStats = new TeamStatsSheets(ws, project, Team.VISITOR);
			teamStats.Fill();
			package.Save();
		}
	}
	
	ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName)
	{
		sheetCount ++;
		p.Workbook.Worksheets.Add(sheetName);
		ExcelWorksheet ws = p.Workbook.Worksheets[sheetCount];
		ws.Name = sheetName;
		return ws;
	}
	
}

