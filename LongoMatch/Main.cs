// project created on 25/11/2007 at 3:00
using System;
using Gtk;
using Mono.Unix;

namespace LongoMatch
	
{
	
	class MainClass
	{
		private static DB db;
		private static string baseDirectory;
		private static string homeDirectory;
		
		public static void Main (string[] args)
		{		
			
			//Iniciamos la base de datos
			db = new DB();
			
			//Configuramos el directorio base de la ejecucuión y el directorio HOME
			baseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
			homeDirectory = System.Environment.GetEnvironmentVariable("HOME")+"/LongoMatch";
		

			
			//Iniciamos la internalización
			//Catalog.Init("longomatch",RelativeToSystemPath("../../share/locale"));
			Catalog.Init("longomatch",baseDirectory);
			
			//Comprobamos los archivos de inicio
			MainClass.CheckDirs();
			MainClass.CheckFiles();
			
			
			
			//Iniciamos la aplicación
			Application.Init ();
			MainWindow win = new MainWindow ();
			win.Show ();		
			Application.Run ();
			
		}
		
		public static string RelativeToSystemPath(string relativePath){
			return System.IO.Path.Combine (baseDirectory, relativePath);
		}
		
		public static string TemplatesDir(){
			return System.IO.Path.Combine (homeDirectory, "templates");
		}
		
		public static string ImagesDir(){
			return System.IO.Path.Combine (baseDirectory, "./");
		}
		
		public static string DBDir(){
			return System.IO.Path.Combine (baseDirectory, "db");
		}
		
		public static void CheckDirs(){

			if (!System.IO.Directory.Exists(homeDirectory))
			    System.IO.Directory.CreateDirectory(homeDirectory);
			if (!System.IO.Directory.Exists(TemplatesDir()))
			    System.IO.Directory.CreateDirectory(TemplatesDir());
			  
		}
		public static void CheckFiles(){			
			string fConfig;
			fConfig = TemplatesDir()+"/default.sct";
			if (!System.IO.File.Exists(fConfig)){
			    SectionsWriter.CreateNewTemplate("default.sct");
			}
		}
		public static DB DB{
			get { return db;}
		}
	}
}