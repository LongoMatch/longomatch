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

using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Stats;
using LongoMatch.Store;

public class TeamStatsSheet
{
	ExcelWorksheet ws;
	ProjectStats stats;
	Team team;
	Dictionary<Player, int> playerRow;
	
	public TeamStatsSheet (ExcelWorksheet ws, ProjectStats stats, Team team)
	{
		this.ws = ws;
		this.stats = stats;
		this.team = team;
		playerRow = new Dictionary<Player, int>();
	}
	
	public void Fill() {
		FillStats(ws);
	}
	
	void FillStats (ExcelWorksheet ws) {
		int categoryColumn = 2;
		foreach (CategoryStats catStats in stats.CategoriesStats) {
			int subcategoryWidth = 0;
			int subcategoryColumn = categoryColumn;
			
			foreach (SubCategoryStat subcatStats in catStats.SubcategoriesStats){
				Dictionary<string, List<PlayersStats>> playersStats;
				int optionColumn  = subcategoryColumn;
				int optionWidth = 0;
				int statsWidth;
				
				if (team == Team.LOCAL)
					playersStats = subcatStats.LocalPlayersStats;
				else 
					playersStats = subcatStats.VisitorPlayersStats;
				
				if (playersStats.Count == 0)
					continue;
				
				foreach (string option in playersStats.Keys) {
					List<PlayersStats> pStatsList = playersStats[option];
					if (pStatsList.Count > 1) {
						FillPlayerStatsSubcat(pStatsList, optionColumn);
					}
					statsWidth = 0;
					foreach (PlayersStats pStats in pStatsList) { 
						foreach (Player player in pStats.Players.Keys) {
							FillPlayersStats (player, pStats.Players[player], optionColumn + statsWidth);
						}
						statsWidth += 1;
					}
					FillOptionName(option, optionColumn, statsWidth);
					optionWidth += statsWidth + 1;
					optionColumn += statsWidth + 1;
				}
				optionWidth -= 1;
				FillSubcategoryName(subcatStats.Name, subcategoryColumn, optionWidth); 
				subcategoryWidth += optionWidth + 1;
				subcategoryColumn += subcategoryWidth;
			}
			subcategoryWidth -= 1;
			FillCategoryName(catStats.Name, categoryColumn, subcategoryWidth);
			categoryColumn += subcategoryWidth + 1;
		}
	}
		
	void SetTitleAndColor (int row, int column, int width, string title, Color color) {
		ws.Cells[row, column].Value = title;
		ws.Cells[row, column].Style.Fill.PatternType =  ExcelFillStyle.Solid;	
		ws.Cells[row, column].Style.Fill.BackgroundColor.SetColor(color);
		ws.Cells[row, column, row, column + width - 1].Merge = true;
	}
	
	void FillOptionName(string name, int column, int width) {
		if (width <= 0)
			return;
		SetTitleAndColor(3, column, width, name, Color.IndianRed);
		ws.Column(column + width).Width = 2;
		for (int i=column; i <= column + width - 1; i++)
			ws.Column(i).AutoFit();
	}
	
	void FillSubcategoryName(string name, int column, int width) {
		if (width <= 0)
			return;
		SetTitleAndColor(2, column, width, name, Color.MediumVioletRed);
	}
	
	void FillCategoryName(string name, int column, int width) {
		if (width <= 0)
			return;
		SetTitleAndColor(1, column, width, name, Color.OrangeRed);
	}
	
	void FillPlayersStats(Player player, int count, int column) {
		ws.Cells[GetPlayerRow(player), column].Value = count;
	}
	
	void FillPlayerStatsSubcat(List<PlayersStats> list, int column) {
		int i = 0;
		foreach (PlayersStats pStats in list) {
			SetTitleAndColor(4, column + i, 1, pStats.Name, Color.PaleVioletRed);
			i++;
		}
	}
	
	int GetPlayerRow (Player player) {
		if (!playerRow.ContainsKey(player)) {
			int row = playerRow.Count + 5;
			playerRow.Add(player, row);
			ws.Cells[row, 1].Value = player.Name;
		}
		return playerRow[player];
	}
}
