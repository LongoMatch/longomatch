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
using System.IO;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;
using System.Threading;
using Gtk;
	
using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Ext;
	

namespace LongoMatch.Compat
{
	
	public delegate void ConversionProgressHandler (string progress);
	
	public class DatabaseMigrator
	{
		
		public event ConversionProgressHandler ConversionProgressEvent;
		
		public const string DONE="Database migrated successfully";
		
		public const string ERROR="Error importing the database";
		
		private string oldDBFile;
		
		private string newDBFile;
		
		private Thread thread;		
		
		public DatabaseMigrator(string oldDBFile)
		{
			this.oldDBFile = oldDBFile;
		}
		
		public void Start(){
			thread = new Thread(new ThreadStart(StartConversion));
			thread.Start();
		}
		
		public void Cancel(){
			if (thread != null && thread.IsAlive)
				thread.Abort();
		}
		
		public void StartConversion(){
			DataBase newDB;
			v00.DB.DataBase backupDB;
			ArrayList backupProjects;		
			ArrayList newProjects = new ArrayList();
			string results="";		
			string backupDBFile=oldDBFile+".bak1";
						
			if (!File.Exists(oldDBFile)){
				SendEvent(String.Format("File {0} doesn't exists",oldDBFile));
				SendEvent(ERROR);
				return;
			}	
			
			//Create a backup of the old DB in which objects are stored using
			//the old namespace scheme. If you try to use the old DB
			//directly, aliases messes-up all the DB.
			File.Copy(oldDBFile,backupDBFile,true);
						
			//Change the namespace of all classes to the new namespace
			ChangeDBNamespace(backupDBFile);
			
			
			newDB = MainClass.DB;			
			backupDB = new LongoMatch.Compat.v00.DB.DataBase(backupDBFile);
			
			backupProjects = backupDB.GetAllDB();
			SendEvent(String.Format("Importing Projects from the old database {0} (Version:0.0) to the current database (Version:{1})\n\n",oldDBFile, newDB.Version));
			SendEvent(String.Format("Found {0} Projects",backupProjects.Count));
	
		
			SendEvent("Creating backup of the old database");
			foreach (v00.DB.Project oldProject in backupProjects){
				PreviewMediaFile file;
				string localName, visitorName;
				int localGoals, visitorGoals;
				DateTime date;
				Sections sections;
				Project newProject;
				
				localName = oldProject.LocalName;
				visitorName = oldProject.VisitorName;
				localGoals = oldProject.LocalGoals;
				visitorGoals = oldProject.VisitorGoals;
				date = oldProject.MatchDate;
				SendEvent(String.Format("Trying to open project {0}...",oldProject.Title));
				try{
					file = PreviewMediaFile.GetMediaFile(oldProject.File.FilePath);
					SendEvent(String.Format("[{0}]Getting properties of file {1}",oldProject.Title,oldProject.File.FilePath));
				}
				catch{
					SendEvent(String.Format("[{0}]Failed to open file {1} \n",oldProject.Title,oldProject.File.FilePath));
					SendEvent(String.Format("[{0}]Cannot scan the file properties\n, using default values",oldProject.Title));
					file = new PreviewMediaFile();
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
				
				SendEvent(String.Format("[{0}]Importing Sections...",oldProject.Title));
				
							
				foreach (v00.TimeNodes.SectionsTimeNode oldSection in oldProject.Sections.SectionsTimeNodes){
					SectionsTimeNode stn = new SectionsTimeNode(oldSection.Name, 
					                                            new Time(oldSection.Start.MSeconds), 
					                                            new Time(oldSection.Stop.MSeconds),
					                                            new HotKey(),
					                                            oldProject.Sections.GetColor(i)
					                                            );
					sections.AddSection(stn);
					SendEvent(String.Format("[{0}]Adding Section #{1} with name {2}",oldProject.Title,i,oldSection.Name));
					i++;
				}
				
				
				SendEvent(String.Format("[{0}]Sections imported successfully",oldProject.Title));
				
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
				
				SendEvent(String.Format("[{0}]Importing all plays ...",oldProject.Title));
				foreach (List<v00.TimeNodes.MediaTimeNode> list in oldProject.GetDataArray()){
					foreach (v00.TimeNodes.MediaTimeNode oldTN in list){
						MediaTimeNode tn;
						Console.WriteLine(oldTN.Name);
						tn = newProject.AddTimeNode(oldTN.DataSection, new Time (oldTN.Start.MSeconds), new Time(oldTN.Stop.MSeconds), oldTN.Miniature);
						tn.Name = oldTN.Name;
						if (oldTN.Team == LongoMatch.Compat.v00.TimeNodes.Team.LOCAL)
							tn.Team =Team.LOCAL;
						else if (oldTN.Team == LongoMatch.Compat.v00.TimeNodes.Team.VISITOR)
							tn.Team=Team.VISITOR;
						else tn.Team=Team.NONE;
						tn.Fps = oldTN.Fps;
						tn.Notes = oldTN.Notes;
						SendEvent(String.Format("[{0}]Added play {1}",oldProject.Title,tn.Name));
					}		
					i++;
				}
				SendEvent(String.Format("[{0}]Project converted successfully",oldProject.Title));
				
				newProjects.Add(newProject);
				File.Copy(oldDBFile, oldDBFile+".bak");
				File.Delete(oldDBFile);
				File.Delete(backupDBFile);
			}	
			foreach (Project project in newProjects){
				try {
					newDB.AddProject(project);
				}
				catch{}
			}
			
			SendEvent(DONE);
		}	
		private void ChangeDBNamespace(string DBFile){
			using (IObjectContainer db = Db4oFactory.OpenFile(DBFile))
			{
				var n = db.Ext().StoredClasses();
				foreach (var x in n)
				{
					string newName;
					string oldName=x.GetName();							
					var c2 = db.Ext().StoredClass(oldName);
					if (c2 != null){
						if(oldName.Contains("LongoMatch.DB")){
							newName=oldName.Replace("LongoMatch.DB","LongoMatch.Compat.v00.DB");
							c2.Rename(newName);
						}
						else if(oldName.Contains("LongoMatch.TimeNodes")){
							newName=oldName.Replace("LongoMatch.TimeNodes","LongoMatch.Compat.v00.TimeNodes");
							c2.Rename(newName);
						}
					}
				}				
			}
		}
		
		public void SendEvent (string message){
			if (ConversionProgressEvent != null)					
						Application.Invoke(delegate {ConversionProgressEvent(message);});
		}
	}
}
