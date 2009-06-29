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
using Gtk;
using Mono.Unix;
using LongoMatch.Gui;
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
		
		public static void Main (string[] args)
		{	
			
			
			
			//Configuramos el directorio base de la ejecucuión y el directorio HOME
			baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"../../");
			homeDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			homeDirectory = System.IO.Path.Combine(homeDirectory,"LongoMatch");
			
			if (Environment.OSVersion.Platform == PlatformID.Win32NT){
				baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"../");
				Environment.SetEnvironmentVariable("GST_PLUGIN_PATH",RelativeToPrefix("lib\\gstreamer-0.10"));
				setGtkTheme();
			}
					
			//Iniciamos la internalización
			Catalog.Init("longomatch",RelativeToPrefix("share/locale"));
			//Catalog.Init("longomatch",LocaleDir());
			
			//Comprobamos los archivos de inicio
			MainClass.CheckDirs();
			MainClass.CheckFiles();
			
			//Iniciamos la base de datos
			db = new DataBase();
			
			
			//Iniciamos la aplicación
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();
			
			Application.Run ();
			
		}
		
		public static string RelativeToPrefix(string relativePath){
			return System.IO.Path.Combine (baseDirectory, relativePath);
		}
		
		public static string HomeDir(){
				return homeDirectory;
	
		}
		
		public static string LocaleDir(){
				return RelativeToPrefix("share/images");
	
		}
		
		public static string PlayListDir(){
			return System.IO.Path.Combine (homeDirectory, "playlists");
		}
		
		public static string SnapshotsDir(){
			return System.IO.Path.Combine (homeDirectory, "snapshots");
		}
		
		public static string TemplatesDir(){
			return System.IO.Path.Combine (homeDirectory, "templates");
		}
		
		public static string ThumbnailsDir(){
			return System.IO.Path.Combine (homeDirectory, "thumbnails");
		}
		
		public static string VideosDir(){
			return System.IO.Path.Combine (homeDirectory, "videos");
		}
		
		public static string TempVideosDir(){
			return System.IO.Path.Combine (VideosDir(), "temp");
		}
		
		public static string ImagesDir(){			
			return RelativeToPrefix("share/images");		
		}
		
		public static string DBDir(){
			return System.IO.Path.Combine (homeDirectory, "db");
		}

		
		
		
		
		public static void CheckDirs(){

			if (!System.IO.Directory.Exists(homeDirectory))
			    System.IO.Directory.CreateDirectory(homeDirectory);
			if (!System.IO.Directory.Exists(TemplatesDir()))
			    System.IO.Directory.CreateDirectory(TemplatesDir());
			if (!System.IO.Directory.Exists(SnapshotsDir()))
			    System.IO.Directory.CreateDirectory(SnapshotsDir());
			if (!System.IO.Directory.Exists(ThumbnailsDir()))
			    System.IO.Directory.CreateDirectory(ThumbnailsDir());
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
			fConfig = TemplatesDir()+"/default.sct";
			if (!System.IO.File.Exists(fConfig)){
			    SectionsWriter.CreateNewTemplate("default.sct");
			}
			
			fConfig = TemplatesDir()+"/default.tem";
			if (!System.IO.File.Exists(fConfig)){
				TeamTemplate tt = new TeamTemplate();
				tt.CreateDefaultTemplate(20);
				foreach (Player p in tt.GetPlayersList())
					Console.WriteLine(p.Name);
				tt.Save(fConfig);					
			}
			
			
		}
		public static DataBase DB{
			get { return db;}
		}
		
		private static void setGtkTheme(){
			if (!System.IO.File.Exists(System.IO.Path.Combine(homeDirectory,"../../.gtkrc-2.0"))){
			    System.IO.File.Copy(RelativeToPrefix("etc/gtk-2.0/gtkrc-2.0"),System.IO.Path.Combine(homeDirectory,"../../.gtkrc-2.0"),true);
			   
			}
		}
		
		
		

	}
}
