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
using LongoMatch.DB;
using LongoMatch.IO;

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
			baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
			homeDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			homeDirectory = System.IO.Path.Combine(homeDirectory,"LongoMatch");

		

			
			//Iniciamos la internalización
			//Catalog.Init("longomatch",RelativeToSystemPath("../../share/locale"));
			Catalog.Init("longomatch",baseDirectory);
			
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
		
		public static string RelativeToSystemPath(string relativePath){
			return System.IO.Path.Combine (baseDirectory, relativePath);
		}
		
		public static string PlayListDir(){
			return System.IO.Path.Combine (homeDirectory, "playlists");
		}
		
		public static string TemplatesDir(){
			return System.IO.Path.Combine (homeDirectory, "templates");
		}
		
		public static string ThumbnailsDir(){
			return System.IO.Path.Combine (homeDirectory, "thumbnails");
		}
		
		public static string ImagesDir(){
			return System.IO.Path.Combine (baseDirectory, "./");
		}
		
		public static string DBDir(){
			return System.IO.Path.Combine (homeDirectory, "db");
		}
		
		
		
		public static void CheckDirs(){

			if (!System.IO.Directory.Exists(homeDirectory))
			    System.IO.Directory.CreateDirectory(homeDirectory);
			if (!System.IO.Directory.Exists(TemplatesDir()))
			    System.IO.Directory.CreateDirectory(TemplatesDir());
			if (!System.IO.Directory.Exists(ThumbnailsDir()))
			    System.IO.Directory.CreateDirectory(ThumbnailsDir());
			if (!System.IO.Directory.Exists(PlayListDir()))
			    System.IO.Directory.CreateDirectory(PlayListDir());
			if (!System.IO.Directory.Exists(DBDir()))
			    System.IO.Directory.CreateDirectory(DBDir());
			  
		}
		public static void CheckFiles(){			
			string fConfig;
			fConfig = TemplatesDir()+"/default.sct";
			if (!System.IO.File.Exists(fConfig)){
			    SectionsWriter.CreateNewTemplate("default.sct");
			}
		}
		public static DataBase DB{
			get { return db;}
		}
	}
}