// CSVExport.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.IO;
using System.Collections.Generic;
using Gtk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.Gui;
using Mono.Unix;

namespace LongoMatch.IO
{


	public class CSVExport
	{
		string outputFile;
		Project project;

		#region Constructors
		public CSVExport(Project project,string outputFile)
		{
			this.project = project;
			this.outputFile = outputFile;
		}
		#endregion

		#region Public methods
		public void WriteToFile() {
			/*List<List<Play>> list;
			Dictionary<Tag, List<Play>> tagsDic;
			List<Player> localPlayersList;
			List<Player> visitorPlayersList;
			Dictionary<Player, List<object[]>> localPlayersDic;
			Dictionary<Player, List<object[]>> visitorPlayersDic;

			string[] sectionNames;
			TextWriter tx;

			tx = new StreamWriter(outputFile);
			list = project.GetDataArray();
			sectionNames = project.GetSectionsNames();
			
			tagsDic = new Dictionary<Tag, List<Play>>();
			foreach (Tag tag in project.Tags)
				tagsDic.Add(tag, new List<Play>());
			
			localPlayersList = project.LocalTeamTemplate.GetPlayersList();
			localPlayersDic = new Dictionary<Player, List<object[]>>();
			foreach (Player player in localPlayersList)
				localPlayersDic.Add(player, new List<object[]>());
			
			visitorPlayersList =  project.VisitorTeamTemplate.GetPlayersList();
			visitorPlayersDic = new Dictionary<Player, List<object[]>>();
			foreach (Player player in visitorPlayersList)
				visitorPlayersDic.Add(player, new List<object[]>());
				

			// Write catagories table
			tx.WriteLine(String.Format("{0};{1};{2};{3};{4};{5}",
			             Catalog.GetString("Section"),
			             Catalog.GetString("Name"),
			             Catalog.GetString("Team"),
			             Catalog.GetString("StartTime"),
			             Catalog.GetString("StopTime"),
			             Catalog.GetString("Duration")));
			for (int i=0; i<list.Count; i++) {
				string sectionName = sectionNames[i];
				foreach (Play tn in list[i]) {
					// Parse Play's tags
					foreach (Tag t in tn.Tags)
						tagsDic[t].Add(tn);
					
					// Parse Players data
					foreach (int playerNumber in tn.LocalPlayers){
						object[] o = new object[2];
						o[0] = sectionName;
						o[1] = tn;
						localPlayersDic[localPlayersList[playerNumber]].Add(o);
					}					
					foreach (int playerNumber in tn.VisitorPlayers){
						object[] o = new object[2];
						o[0] = sectionName;
						o[1] = tn;
						visitorPlayersDic[visitorPlayersList[playerNumber]].Add(o);
					}
					
					tx.WriteLine("\""+sectionName+"\";\""+
					             tn.Name+"\";\""+
					             tn.Team+"\";\""+
					             tn.Start.ToMSecondsString()+"\";\""+
					             tn.Stop.ToMSecondsString()+"\";\""+
					             (tn.Stop-tn.Start).ToMSecondsString()+"\"");
				}
			}
			tx.WriteLine();
			tx.WriteLine();			
			
			WriteCatagoriesData(tx, tagsDic);
			
			// Write local players data
			WritePlayersData(tx, localPlayersDic);
			WritePlayersData(tx, visitorPlayersDic);
			
			tx.Close();
			
			MessagePopup.PopupMessage(null, MessageType.Info, Catalog.GetString("CSV exported successfully."));	*/		
		}
		#endregion
		
		#region Private Methods
		
		private void WriteCatagoriesData(TextWriter tx, Dictionary<Tag, List<Play>> tagsDic){
			// Write Tags table
			tx.WriteLine(String.Format("{0};{1};{2};{3};{4};{5}",
			             Catalog.GetString("Tag"),
			             Catalog.GetString("Name"),
			             Catalog.GetString("Team"),
			             Catalog.GetString("StartTime"),
			             Catalog.GetString("StopTime"),
			             Catalog.GetString("Duration")));
			foreach (KeyValuePair<Tag,List<Play>> pair in tagsDic){
				if (pair.Value.Count == 0)
					continue;				
				foreach (Play tn in pair.Value) {
					tx.WriteLine("\""+pair.Key.Value+"\";\""+
					             tn.Name+"\";\""+
					             tn.Team+"\";\""+
					             tn.Start.ToMSecondsString()+"\";\""+
					             tn.Stop.ToMSecondsString()+"\";\""+
					             (tn.Stop-tn.Start).ToMSecondsString()+"\"");
				}				
			}
			tx.WriteLine();
			tx.WriteLine();			
		}
		
		private void WritePlayersData(TextWriter tx, Dictionary<Player, List<object[]>> playersDic){
			// Write Tags table
			tx.WriteLine(String.Format("{0};{1};{2};{3};{4};{5};{6}",
			                           Catalog.GetString("Player"),
			                           Catalog.GetString("Category"),
			                           Catalog.GetString("Name"),
			                           Catalog.GetString("Team"),
			                           Catalog.GetString("StartTime"),
			                           Catalog.GetString("StopTime"),
			                           Catalog.GetString("Duration")));
			foreach (KeyValuePair<Player,List<object[]>> pair in playersDic){
				if (pair.Value.Count == 0)
					continue;			
				foreach (object[] o in pair.Value) {
					string sectionName = (string)o[0];
					Play tn = (Play)o[1];
					tx.WriteLine("\""+pair.Key.Name+"\";\""+
					             sectionName+"\";\""+
					             tn.Name+"\";\""+
					             tn.Team+"\";\""+
					             tn.Start.ToMSecondsString()+"\";\""+
					             tn.Stop.ToMSecondsString()+"\";\""+
					             (tn.Stop-tn.Start).ToMSecondsString()+"\"");
				}				
			}
			tx.WriteLine();
			tx.WriteLine();			
		}
		#endregion
	}
}
