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

using LongoMatch;
using LongoMatch.Addins.ExtensionPoints;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using OfficeOpenXml.Style;
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
	
	public void ExportProject (Project project, string filename) {
		EPPLUSExporter exporter = new EPPLUSExporter(project, filename);
		exporter.Export();
	}
}

public class EPPLUSExporter {
	
	Project project;
	string filename;
	ExcelPackage package;
	ExcelWorksheet ws;
	int duration;
	
	const int TIMELINE_START = 6;

	public EPPLUSExporter(Project project, string filename) {
		this.project = project;
		this.filename = filename;
		duration = (int) (project.Description.File.Length / 1000); // 60);
	}
	
	public void Export() {
		FileInfo newFile = new FileInfo(filename);
		if (newFile.Exists)
		{
			newFile.Delete();  // ensures we create a new workbook
			newFile = new FileInfo(filename);
		}
		
		using (package = new ExcelPackage(newFile)) {
			ws = CreateSheet(package, "LongoMatch");
			FillSheet(ws);
			Console.WriteLine ("Saving");
			package.Save();
		}
	}
	
	ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName)
	{
		p.Workbook.Worksheets.Add(sheetName);
		ExcelWorksheet ws = p.Workbook.Worksheets[1];
		ws.Name = sheetName; //Setting Sheet's name
		return ws;
	}
	
	void FillSheet(ExcelWorksheet ws) {
		int row = 1;
		
		for (int i = 0; i < TIMELINE_START; i++)
			ws.Column(i).Width = 10;
		for (int i=0; i<duration; i++)
			ws.Column(TIMELINE_START + i).Width = 2;
		
		row = FillTimeline(ws, row);
		row = FillGameUnits(ws, row);
		row = FillCategories(ws, row);
	}
	
	void SetColoredHeaders (ExcelWorksheet ws, int row, string name, int startCol,
		int stopCol, Color color, bool withSum)
	{
		ws.Cells[row,startCol].Value = name;
		if (withSum) {
			ws.Cells[row,stopCol].Formula = String.Format("Sum({0})",
				new ExcelAddress(row, TIMELINE_START, row, TIMELINE_START +duration).Address);
		}
		ExcelRange cols = ws.Cells[row, startCol, row, stopCol];
		cols.Style.Fill.PatternType = ExcelFillStyle.Solid;
		cols.Style.Fill.BackgroundColor.SetColor(color);
		cols.Dispose();
	}
		
	void SetHeader (ExcelWorksheet ws, int row, string title) {
		ws.Cells[row, 1].Value = title;
		ws.Cells[row, 1].Style.Fill.PatternType =  ExcelFillStyle.Solid;	
		ws.Cells[row, 1].Style.Fill.BackgroundColor.SetColor(Color.Yellow);
		ws.Cells[row, 1, row, 3].Merge = true;
	}
	
	int FillTimeline(ExcelWorksheet ws, int row) {
		SetHeader(ws, row, Catalog.GetString("Timeline"));
		
		for (int i=0; i < duration; i+=5) {
			ws.Cells[row, TIMELINE_START + i].Value = i;
		}
		row++;
		return row;
	}
	
	int FillGameUnits(ExcelWorksheet ws, int row) {
		SetHeader(ws, row, Catalog.GetString("Game Units"));
		ws.Cells[row, 5].Value = Catalog.GetString("Duration (min)");
		row ++;
		
		foreach (GameUnit gu in project.GameUnits) {
			ExcelRange cols;
			
			SetColoredHeaders(ws, row, gu.Name, 3, 5, Color.DeepSkyBlue, true);
			cols = ws.Cells[row, TIMELINE_START, row, duration];
			cols.Style.Fill.PatternType = ExcelFillStyle.Solid;
			cols.Style.Fill.BackgroundColor.SetColor(Color.Red);
			cols.Dispose();
			
			foreach (TimelineNode unit in gu) {
				int start, stop;
				
				start = TIMELINE_START + unit.Start.Seconds; // 60;
				stop = TIMELINE_START + unit.Stop.Seconds; // 60;
				
				ws.Cells[row, start].Value = stop - start;
				cols = ws.Cells[row, start, row, stop];
				cols.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
				cols.Style.Fill.PatternType =  ExcelFillStyle.Solid;	
				cols.Style.Fill.BackgroundColor.SetColor(Color.Green);
			}
			row ++;
		}
		return row;
	}
	
	void SetSubcatHeaders (ExcelWorksheet ws, int row, string name)
	{
		SetColoredHeaders(ws, row, name, 2, 5, Color.BlueViolet, true);
	}
	
	void SetSubcatentriesHeaders (ExcelWorksheet ws, int row, string name)
	{
		SetColoredHeaders(ws, row, name, 4, 5, Color.Cyan, true);
	}
	
	int FillCategoriesDescription(ExcelWorksheet ws, int row, List<Category> categories,
		Dictionary<Category, int> catsDict, Dictionary<ISubCategory, int> subCatsDict)
	{
		foreach (Category ca in categories) {
			SetSubcatHeaders(ws, row, ca.Name);
			catsDict.Add(ca, row);
			
			foreach (var subcat in ca.SubCategories) {
				if (subcat is PlayerSubCategory)
				continue;
				
				row++;
				SetColoredHeaders(ws, row, subcat.Name, 3, 5, Color.DeepSkyBlue, false);
				
				subCatsDict.Add(subcat, row);
				if (subcat is TeamSubCategory) {
					row++;
					SetSubcatentriesHeaders(ws, row, project.LocalTeamTemplate.Name);
					row++;
					SetSubcatentriesHeaders(ws, row, project.VisitorTeamTemplate.Name);
					
				}
				/*	
			    else if (subcat is PlayerSubCategory) {
					if ((subcat as PlayerSubCategory).Contains(Team.LOCAL)) {
						row++;
						SetSubcatentriesHeaders(ws, row, project.LocalTeamTemplate.Name);
					}
					if ((subcat as PlayerSubCategory).Contains(Team.VISITOR)) {
						row++;
						SetSubcatentriesHeaders(ws, row, project.VisitorTeamTemplate.Name);
					}
				} 
				*/
				else if (subcat is TagSubCategory) {
					foreach (string s in subcat.ElementsDesc()) {
						row++;
						SetSubcatentriesHeaders(ws, row, s);
					}
				}
			}
			row++;
		}
		return row;
	}
	
	int FillCategoriesData(ExcelWorksheet ws, int row, List<Category> categories,
		Dictionary<Category, int> catsDict, Dictionary<ISubCategory, int> subCatsDict)
	{
		foreach (Category ca in project.Categories) {
			foreach (Play play in project.PlaysInCategory(ca)) {
				int time;
				int catRow;
				
				/* Add the category's overal stats */
				catRow = catsDict[ca];
				time = TIMELINE_START + play.Start.Seconds; // /60;
				object val = ws.Cells[catRow , time].Value;
				if (val is int)
					ws.Cells[catRow, time].Value = ((int)val) + 1;
				else
					ws.Cells[catRow, time].Value =  1;
				
				/* Add the tags stats */
				foreach (StringTag tag in play.Tags.Tags) {
					int subcatRow = subCatsDict[tag.SubCategory];
					subcatRow += tag.SubCategory.ElementsDesc().IndexOf(tag.Value);
					/* FIXME: add +1 to the previous results */
					ws.Cells[subcatRow, time].Value = 1;
				}
				
				/* Add the teams stats */
				foreach (TeamTag tag in play.Teams.Tags) {
					int subcatRow = subCatsDict[tag.SubCategory];
					if (tag.Value == Team.LOCAL) {
						ws.Cells[subcatRow + 1, time].Value = 1;
					} else if (tag.Value == Team.VISITOR) {
						ws.Cells[subcatRow + 2, time].Value = 1;
					}
					/* FIXME: add +1 to the previous results */
				}
			}
		}
		return row;
	}
	
	int FillCategories(ExcelWorksheet ws, int row) {
		Dictionary<Category, int> catsDict = new Dictionary<Category, int>();
		Dictionary<ISubCategory, int>  subCatsDict = new Dictionary<ISubCategory, int>();
		
		
		SetHeader(ws, row, Catalog.GetString("Categories"));
		ws.Cells[row, 5].Value = Catalog.GetString("Count");
		row ++;
		
		row = FillCategoriesDescription(ws, row, project.Categories, catsDict, subCatsDict);
		row = FillCategoriesData(ws, row, project.Categories, catsDict, subCatsDict);
		return row;
	}
}

