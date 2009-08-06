// Main.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
using System.IO;
using Gtk;
using Mono.Unix;
using LongoMatch.Gui;
using LongoMatch.Gui.Dialog;
using LongoMatch.DB;
using LongoMatch.IO;
using LongoMatch.TimeNodes;
using System.Runtime.InteropServices;

namespace LongoMatch
	
{
	
	class MainClass
	{
		private static DataBase db;
		private static string baseDirectory;
		private static string homeDirectory;
		private static string configDirectory;
		private const string WIN32_CONFIG_FILE = "longomatch.conf";
		
		public static void Main (string[] args)
		{		
			//Configuramos el directorio base de la ejecucuión y el directorio HOME
			baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"../../");
			homeDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			configDirectory = System.IO.Path.Combine(homeDirectory,".longomatch");
			homeDirectory = System.IO.Path.Combine(homeDirectory,"LongoMatch");
			
			if (Environment.OSVersion.Platform == PlatformID.Win32NT){				
				SetUpWin32Config();
			}
					
			//Iniciamos la internalización
			Catalog.Init("longomatch",RelativeToPrefix("share/locale"));
			
								
			//Iniciamos la aplicación
			Application.Init ();
			
			LongoMatch.Video.Player.GstPlayer.InitBackend("");
			
			if (homeDirectory == null)
				PromptForHomeDir();
			
			//Comprobamos los archivos de inicio
			MainClass.CheckDirs();
			MainClass.CheckFiles();
			
			//Iniciamos la base de datos
			db = new DataBase(Path.Combine(DBDir(),"longomatch.db"));			
			
			try {
				MainWindow win = new MainWindow ();
				win.Show ();			
				Application.Run ();
			}
			catch (Exception ex){
				// Try to save the opened project 
				if (MainWindow.OpenedProject() != null)
					DB.UpdateProject(MainWindow.OpenedProject());
				ProcessExecutionError(ex);				
			}			
		}
		
		public static string RelativeToPrefix(string relativePath){
			return System.IO.Path.Combine (baseDirectory, relativePath);
		}
		
		public static string HomeDir(){
				return homeDirectory;	
		}
		

		public static string PlayListDir(){
			return System.IO.Path.Combine (homeDirectory, "playlists");
		}
		
		public static string SnapshotsDir(){
			return System.IO.Path.Combine (homeDirectory, "snapshots");
		}
		
		public static string TemplatesDir(){
			return System.IO.Path.Combine (configDirectory, "templates");
		}		
				
		public static string VideosDir(){
			return System.IO.Path.Combine (homeDirectory, "videos");
		}
		
		public static string TempVideosDir(){
			return System.IO.Path.Combine (configDirectory, "temp");
		}
		
		public static string ImagesDir(){			
			return RelativeToPrefix("share/longomatch/images");		
		}
		
		public static string DBDir(){
			return System.IO.Path.Combine (configDirectory, "db");
		}	
		
		public static void CheckDirs(){
			if (!System.IO.Directory.Exists(homeDirectory))
			    System.IO.Directory.CreateDirectory(homeDirectory);
			if (!System.IO.Directory.Exists(TemplatesDir()))
			    System.IO.Directory.CreateDirectory(TemplatesDir());
			if (!System.IO.Directory.Exists(SnapshotsDir()))
			    System.IO.Directory.CreateDirectory(SnapshotsDir());
			if (!System.IO.Directory.Exists(PlayListDir()))
			    System.IO.Directory.CreateDirectory(PlayListDir());
			if (!System.IO.Directory.Exists(DBDir()))
			    System.IO.Directory.CreateDirectory(DBDir());
			if (!System.IO.Directory.Exists(VideosDir()))
			    System.IO.Directory.CreateDirectory(VideosDir());
			if (!System.IO.Directory.Exists(TempVideosDir()))
			    System.IO.Directory.CreateDirectory(TempVideosDir());			  
		}
		
		public static void CheckFiles(){			
			string fConfig;
			fConfig = System.IO.Path.Combine(TemplatesDir(),"default.sct");
			if (!System.IO.File.Exists(fConfig)){
			    SectionsWriter.CreateNewTemplate("default.sct");
			}
			
			fConfig = System.IO.Path.Combine(TemplatesDir(),"default.tem");
			if (!System.IO.File.Exists(fConfig)){
				TeamTemplate tt = new TeamTemplate();
				tt.CreateDefaultTemplate(20);
				tt.Save(fConfig);					
			}			
		}
		
		public static DataBase DB{
			get { return db;}
		}
		
		/*private static void setGtkTheme(){
			if (!System.IO.File.Exists(System.IO.Path.Combine(homeDirectory,"../../.gtkrc-2.0"))){
			    System.IO.File.Copy(RelativeToPrefix("etc/gtk-2.0/gtkrc-2.0"),System.IO.Path.Combine(homeDirectory,"../../.gtkrc-2.0"),true);
			}
		}*/
		
		private static void SetUpWin32Config(){
			Environment.SetEnvironmentVariable("GST_PLUGIN_PATH",RelativeToPrefix("lib\\gstreamer-0.10"));
			baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"../");

			try{
				StreamReader reader = new StreamReader(System.IO.Path.Combine(baseDirectory, "etc/"+WIN32_CONFIG_FILE));
				homeDirectory = reader.ReadLine();
				configDirectory = homeDirectory;
				if (!System.IO.Directory.Exists(homeDirectory))
					System.IO.Directory.CreateDirectory(homeDirectory);
				reader.Close();
			}
			catch {
				homeDirectory = null;
			}		
		}
		
		private static void PromptForHomeDir(){
		    StreamWriter writer;
			WorkspaceChooser chooser = new WorkspaceChooser();
				
			chooser.Run();
			homeDirectory = System.IO.Path.Combine(chooser.WorkspaceFolder,"LongoMatch");	
			configDirectory = homeDirectory;				
			chooser.Destroy();
			
			using (writer = new StreamWriter (System.IO.Path.Combine(baseDirectory, "etc/"+WIN32_CONFIG_FILE))){
				writer.WriteLine(homeDirectory);
				writer.Flush();
				writer.Close();
			}
		}
		
		
		private static void ProcessExecutionError(Exception ex){
			string logFile ="LongoMatch-" + DateTime.Now +".log";
			string message;
			
			logFile = logFile.Replace("/","-");
			logFile = logFile.Replace(" ","-");
			logFile = System.IO.Path.Combine(HomeDir(),logFile);
			
			message = String.Format("{0}\n{1}\n{2}",ex.Message,ex.Source,ex.StackTrace);
			using (StreamWriter s = new StreamWriter(logFile)){
				s.WriteLine(message);
			}	 
			
			Console.WriteLine(message);
			//TODO Add bug reports link
			MessagePopup.PopupMessage(null, MessageType.Error, 
			                          Catalog.GetString("The application has finished with an unexpected error.")+"\n"+
			                          Catalog.GetString("A log has been saved at: "+logFile)+ "\n"+
			                          Catalog.GetString("Please, fill a bug report at "));
			Application.Quit();
		}
	}
}
