// 
//  Copyright (C) 2009 Andoni Morales Alastruey
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
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 

using System;
using System.Collections;
using System.Collections.Generic;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;
	


namespace LongoMatch.DB.Compat
{
	
	
	public class DatabaseMigrator
	{
		private string oldDBFile;
		
		private string newDBFile;
		
		public DatabaseMigrator(string oldDBFile, string newDBFile)
		{
			this.oldDBFile = oldDBFile;
			this.newDBFile = newDBFile;
		}
		
		public void StartConversion(){
			ArrayList oldDBProjects;
			string results="";
			v00.DataBase oldDB = new v00.DataBase(oldDBFile);
			DataBase newDB = new DataBase(newDBFile);
			
			oldDBProjects = oldDB.GetAllDB();
			
			results += String.Format("Converting {0} (Version:0.0) to {1} (Version:{2})\n\n",oldDBFile, newDBFile, newDB.Version);
			foreach ( v00.Project oldProject in oldDBProjects){
				MediaFile file;
				string localName, visitorName;
				int localGoals, visitorGoals;
				DateTime date;
				Sections sections;
				Project newProject;
				
				localName = oldProject.LocalName;
				visitorName = oldProject.VisitorName;
				localGoals = oldProject.VisitorGoals;
				visitorGoals = oldProject.VisitorGoals;
				date = oldProject.MatchDate;
				results += String.Format("Trying to open project {0}... \n",oldProject.File);
				try{
					file = MediaFile.GetMediaFile(oldProject.File.FilePath);
					results += String.Format("Opened file {0} \n",oldProject.File);
				}
				catch{
					results += String.Format("Failed to open file {0} \n",oldProject.File);
					results += "Cannot scan the file properties\n, using default values";
					file = new MediaFile();
					file.FilePath = oldProject.File.FilePath;
					file.Fps = oldProject.File.Fps;
					file.HasAudio = oldProject.File.HasAudio;
					file.HasVideo =  oldProject.File.HasVideo;
					file.Length = oldProject.File.Length.MSeconds;
					file.VideoHeight = 576;
					file.VideoWidth = 720;				
					file.AudioCodec = "";
					file.VideoCodec = "";
				}
				
				sections = new Sections();
				int i=0;
				
				results += "Converting Sections...\n";
				
				foreach (v00.SectionsTimeNode oldSection in oldProject.Sections.SectionsTimeNodes){
					SectionsTimeNode stn = new SectionsTimeNode(oldSection.Name, 
					                                            new Time(oldSection.Start.MSeconds), 
					                                            new Time(oldSection.Stop.MSeconds),
					                                            new HotKey(),
					                                            oldProject.Sections.GetColor(i));
					sections.AddSection(stn);
					results += String.Format("Adding Section #{0} with name {1}\n",i,oldSection.Name);
					i++;
				}
				
				results += "Converting Sections... success\n";
				
				newProject = new Project(file,
				                         localName, 
				                         visitorName, 
				                         "",
				                         "",
				                         localGoals,
				                         visitorGoals,
				                         date, 
				                         sections, 
				                         TeamTemplate.DefautlTemplate(15),
				                         TeamTemplate.DefautlTemplate(15));
				
				i=0;
				
				results += "Adding Plays List...\n";
				
				foreach (List<v00.MediaTimeNode> list in oldProject.GetDataArray()){
					results += String.Format("Adding Plays List #{0}\n",i);
					foreach (v00.MediaTimeNode oldTN in list){
						MediaTimeNode tn;						
						tn = newProject.AddTimeNode(i, new Time (oldTN.Start.MSeconds), new Time(oldTN.Stop.MSeconds), oldTN.Miniature);
						tn.Name = oldTN.Name;
						tn.Notes = oldTN.Notes;
						results += String.Format("Added Play {0}\n",tn.Name);
					}
					i++;
				}
				
				results += "Project converted successfully \n";
				
				newDB.AddProject(newProject);				
			}	
			
			results += "Conversion finished \n";
		}		
	}
}
