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
using System.Collections.Generic;
using System.Drawing;
using Mono.Unix;
using OfficeOpenXml;
using OfficeOpenXml.Style;

using LongoMatch.Stats;
using LongoMatch.Store;

public class GameUnitsStatsSheetFull
{
	ProjectStats stats;
	ExcelWorksheet ws;
	
	public GameUnitsStatsSheetFull (ExcelWorksheet ws, ProjectStats stats)
	{
		this.stats = stats;
		this.ws = ws;
	}
	
	public void Fill() {
		int row = 1;
		
		row = FillGameUnitsStats (stats.GameUnitsStats, row);
		row = FillFirstLevelGameUnitsStats (stats.GameUnitsStats, row);
	}
	
	void SetColoredHeader (string title, int row, int column, int width=1) {
		ws.Cells[row, column].Value = title;
		ws.Cells[row, column].Style.Fill.PatternType =  ExcelFillStyle.Solid;	
		ws.Cells[row, column].Style.Fill.BackgroundColor.SetColor(Color.CadetBlue);
		if (width > 1) {
		 ws.Cells[row, column, row, column + width - 1].Merge = true;	
		}
	}
	
	int FillHeaders (int row) {
		SetColoredHeader(Catalog.GetString("Name"), 1, 1);
		SetColoredHeader(Catalog.GetString("Count"), 1, 2);
		SetColoredHeader(Catalog.GetString("Total Time"), 1, 3, 2);
		SetColoredHeader(Catalog.GetString("Played Time"), 1, 5, 2);
		SetColoredHeader(Catalog.GetString("Paused Time"), 1, 7, 2);
		for (int i=3; i<9; i++) {
			SetColoredHeader(Catalog.GetString("Average"), 2, i);
			i++;
			SetColoredHeader(Catalog.GetString("Deviation"), 2, i);
		}
		row += 2;
		return row;
	}
	
	int FillGameUnitsStats (GameUnitsStats stats, int row) {
		row = FillHeaders(row);
		
		Dictionary<GameUnit, GameUnitStatsNode> gameUnitsNodes  = stats.GameUnitNodes;
		foreach (GameUnit gu in gameUnitsNodes.Keys) {
			row = FillStats (gu.Name, gameUnitsNodes[gu], row);
		}
		return row;
	}
	
	int FillStats (string name, GameUnitStatsNode guStats, int row) {
		ws.Cells[row, 1].Value = name;
		ws.Cells[row, 2].Value = guStats.Count;
		ws.Cells[row, 3].Value = guStats.AverageDuration / (float)1000;
		ws.Cells[row, 4].Value = guStats.DurationTimeStdDeviation / 1000;
		ws.Cells[row, 5].Value = guStats.AveragePlayingTime / (float)1000;
		ws.Cells[row, 6].Value = guStats.PlayingTimeStdDeviation / 1000;
		ws.Cells[row, 7].Value = guStats.AveragePausedTime / (float)1000;
		ws.Cells[row, 8].Value = guStats.PausedTimeStdDeviation / 1000;
		row ++;
		return row;
	}
	
	int FillFirstLevelGameUnitsStats (GameUnitsStats stats, int row) {
		int i=1;
		
		row += 2;
		stats.GameNode.Sort((a, b) => (a.Node.Start - b.Node.Start).MSeconds);
		
		foreach (GameUnitStatsNode node in stats.GameNode){
			row = FillStats (node.Name + " " + i, node, row);
			i++;
		}
		return row;
	}
}
