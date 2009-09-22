// 
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using System.IO;
using System.Threading;
using Gtk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.IO;

namespace LongoMatch.Compat
{
	
	
	public class TemplatesMigrator
	{
		private string[]  oldTPFiles;
		
		public event ConversionProgressHandler ConversionProgressEvent;
		
		public const string DONE="Templates imported successfully";
		
		public const string ERROR="Error importing templates";		
		
		private Thread  thread;				
		
		public TemplatesMigrator(string[]  oldTPFiles)
		{
			this.oldTPFiles=  oldTPFiles;
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
			foreach (string templateFile in oldTPFiles){
				v00.DB.Sections oldTemplate=null;
				Sections newTemplate= null;
				string newFileName;
				
				SendEvent(String.Format("Converting template: {0}",Path.GetFileName(templateFile)));
				try{				
					v00.IO.SectionsReader reader = new v00.IO.SectionsReader(templateFile);
					oldTemplate = reader.GetSections();
					newTemplate = new Sections();
				}catch{
					oldTemplate= null;
					SendEvent("This file is not a valid template file");
				}
				if (oldTemplate != null){
					int i=0;
					foreach ( v00.TimeNodes.SectionsTimeNode sectionTN in oldTemplate.SectionsTimeNodes ){
						//SendEvent(String.Format("Converting Section #{0}: {1}",i+1,sectionTN.Name));
						SectionsTimeNode newSectionTN = new SectionsTimeNode(sectionTN.Name,
						                                                     new Time(sectionTN.Start.MSeconds),
						                                                     new Time(sectionTN.Stop.MSeconds),
						                                                     new HotKey(),
						                                                     oldTemplate.GetColor(i));
						newTemplate.AddSection(newSectionTN);
						i++;
					}
				}
				newFileName = Path.Combine(MainClass.TemplatesDir(),Path.GetFileName(templateFile));
				File.Copy(templateFile ,templateFile+".old",true);
				File.Delete(templateFile);	
				SectionsWriter.UpdateTemplate(newFileName,newTemplate);
				SendEvent(String.Format("Template {0} converted successfully!",Path.GetFileName(templateFile)));
			}
			SendEvent(DONE);
		}
					
		public void SendEvent (string message){
			if (ConversionProgressEvent != null)					
						Application.Invoke(delegate {ConversionProgressEvent(message);});
		}			
	}
}
