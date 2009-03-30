// Updater.cs
//
//  Copyright (C) 2009 Andoni Morales Alastruey
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.Reflection;
using System.Net;
using System.Threading;

namespace LongoMatch.Updates
{
	
	
	public class Updater
	{
		public event LongoMatch.Handlers.NewVersionHandler NewVersion;
		
		private Version actual;
		private Version update;
		
		private const string UPDATE_INFO_URL="http://www.ylatuya.es/updates/version.xml";
		private string temp_file = null;
		private string downloadURL;
		
		public Updater()
		{
			this.actual = Assembly.GetExecutingAssembly().GetName().Version;	
			this.temp_file = System.IO.Path.Combine(MainClass.TemplatesDir(),"version.xml");
			
		}
		
		private void FetchNewVersion(){			
			WebClient wb = new WebClient();
			try {
				wb.DownloadFile(UPDATE_INFO_URL,temp_file);
				XmlUpdateParser parser = new XmlUpdateParser(temp_file);
				update = parser.UpdateVersion;
				downloadURL = parser.DownloadURL;
				                  
			}
			catch (Exception ex){	
					update = actual;
			}			
		}
		
		private bool ConexionExists(){			
			try{
				System.Net.Dns.GetHostByName("www.ylatuya.es");
				return true;
			}
			catch{
				update = actual;
				return false;
			}
		}
		
		private bool IsOutDated(){
			if(update.Major > actual.Major)
				return true;
			else if (update.Minor > actual.Minor)
				return true;
			else if (update.Build > actual.Build)
				return true;
			else
				return false;
		}
		
		private void CheckForUpdates(){
			if (ConexionExists())
				this.FetchNewVersion();
			if (NewVersion != null && IsOutDated()){	
				Gtk.Application.Invoke(delegate {this.NewVersion(update,downloadURL);});			
			}		
		}
		
		public void Run(){
			Thread thread = new Thread(new ThreadStart(CheckForUpdates));
			thread.Start();
		}
				
		
	}
}
