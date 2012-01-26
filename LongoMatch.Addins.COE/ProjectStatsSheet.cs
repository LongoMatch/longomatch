// 
//  Copyright (C) 2012 Andoni Morales Alastruey
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
using System.Drawing;
using Mono.Unix;
using OfficeOpenXml;
using OfficeOpenXml.Style;

using LongoMatch.Stats;
using LongoMatch.Store;

public class ProjectStatsSheet
{
	Project project;
	ExcelWorksheet ws;
	
	public ProjectStatsSheet (ExcelWorksheet ws, Project project)
	{
		this.project = project;
		this.ws = ws;
	}
	
	public void Fill(ProjectStats stats) {
		int row = 1;
		
		row = FillMatchDescription (ws, row, stats);
		row += 3;
		row = FillTeamsData (ws, row, stats);
		row = FillOverallStats(ws, row, stats);
	}
	
	int FillInfoData (ExcelWorksheet ws, int row, string desc, string val) {
		ExcelRange cols = ws.Cells[row, 2, row, 5];
		cols.Style.Fill.PatternType = ExcelFillStyle.Solid;
		cols.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
		cols.Dispose();
		
		ws.Cells[row, 2].Value = desc;
		ws.Cells[row, 2, row, 3].Merge = true;
		ws.Cells[row, 4].Value = val;
		ws.Cells[row, 4, row, 5].Merge = true;
		row++;
		return row;
	}
	
	int FillMatchDescription (ExcelWorksheet ws, int row, ProjectStats stats) {
		row = FillInfoData (ws, row, Catalog.GetString("Project"), stats.ProjectName);
		row = FillInfoData (ws, row, Catalog.GetString("Date"), stats.Date.ToShortDateString());
		row = FillInfoData (ws, row, Catalog.GetString("Competition"), stats.Competition);
		row = FillInfoData (ws, row, Catalog.GetString("Season"), stats.Season);
		return row;
	}
	
	int FillTeamsData (ExcelWorksheet ws, int row, ProjectStats stats) {
		ExcelRange cols = ws.Cells[row, 6, row, 10];
		cols.Style.Fill.PatternType = ExcelFillStyle.Solid;
		cols.Style.Fill.BackgroundColor.SetColor(Color.Red);
		cols.Dispose();
		
		ws.Cells[row, 6].Value = Catalog.GetString("Total");
		ws.Cells[row, 7].Value = stats.LocalTeam; 
		ws.Cells[row, 9].Value = stats.VisitorTeam; 
		ws.Cells[row, 7, row, 8].Merge = true;
		ws.Cells[row, 9, row, 10].Merge = true;
		row++;
		return row;
		
	}
	
	int FillOverallStats (ExcelWorksheet ws, int row, ProjectStats stats) {
		
		foreach (CategoryStats catStat in stats.CategoriesStats) {
			SetSubcatHeaders(ws, row, catStat.Name);
			ws.Cells[row, 6].Value = catStat.TotalCount;
			ws.Cells[row, 7].Value = catStat.LocalTeamCount;
			ws.Cells[row, 9].Value = catStat.VisitorTeamCount;
			row++;
			
			foreach (SubCategoryStat subcatStat in catStat.SubcategoriesStats) {
				SetColoredHeaders(ws, row, subcatStat.Name, 3, 5, Color.DeepSkyBlue);
				row++;
				
				foreach (PercentualStat pStat in subcatStat.OptionStats)  {
					SetSubcatentriesHeaders(ws, row, pStat.Name);
					ws.Cells[row, 6].Value = pStat.TotalCount;
					ws.Cells[row, 7].Value = pStat.LocalTeamCount;
					ws.Cells[row, 9].Value = pStat.VisitorTeamCount;
					row++;
				}
			}
		}
		return row;
	}
	
	void SetColoredHeaders (ExcelWorksheet ws, int row, string name, int startCol,
		int stopCol, Color color)
	{
		ws.Cells[row,startCol].Value = name;
		ExcelRange cols = ws.Cells[row, startCol, row, stopCol];
		cols.Style.Fill.PatternType = ExcelFillStyle.Solid;
		cols.Style.Fill.BackgroundColor.SetColor(color);
		cols.Dispose();
	}
	
	void SetSubcatHeaders (ExcelWorksheet ws, int row, string name)
	{
		SetColoredHeaders(ws, row, name, 2, 5, Color.BlueViolet);
	}
	
	void SetSubcatentriesHeaders (ExcelWorksheet ws, int row, string name)
	{
		SetColoredHeaders(ws, row, name, 4, 5, Color.Cyan);
	}
}
