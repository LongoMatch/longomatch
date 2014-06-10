//
//  Copyright (C) 2014 Andoni Morales Alastruey
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
using Gtk;
using System.Collections.Generic;
using LongoMatch.DB;
using LongoMatch.Store;

public partial class MainWindow: Gtk.Window
{	

	string baseDirectory;
	string homeDirectory;
	string configDirectory;
	string buf;
	List<string> teams, categories, dbs;
	
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
		SetupBasedir ();
		FindFiles ();
		UpdateLabel ();
		buf = "";
	}
	
	void SetupBasedir () {
		string home;
			
		baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory,"../");
		if (!System.IO.Directory.Exists(System.IO.Path.Combine(baseDirectory, "share", "longomatch"))) {
			baseDirectory = System.IO.Path.Combine(baseDirectory, "../");
		}
		
		home = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
		homeDirectory = System.IO.Path.Combine(home, "LongoMatch");
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)
			configDirectory = homeDirectory;
		else
			configDirectory = System.IO.Path.Combine(home,"." + "longomatch");
	}
	
	void FindFiles () {
		string dbdir, templatesdir;
		
		dbs = new List<string>();
		teams = new List<string>(); 
		categories = new List<string>();
		
		dbdir = System.IO.Path.Combine (configDirectory, "db");
		if (Directory.Exists (dbdir)) {
			foreach (string file in Directory.GetFiles (dbdir)) {
				if (file.EndsWith ("1.db")) {
					dbs.Add (file);
				}
			}
		}
		
		templatesdir = System.IO.Path.Combine (homeDirectory, "templates");
		if (Directory.Exists (templatesdir)) {
			foreach (string file in Directory.GetFiles (templatesdir)) {
				if (file.EndsWith (".lct")) {
					categories.Add (file);
				}
				if (file.EndsWith (".ltt")) {
					teams.Add (file);
				}
			}
		}
	}
	
	void UpdateLabel () {
		label3.Markup = " <b> Databases </b>: " + dbs.Count + "\n\n" + 
			" <b> Analysis templates </b>: " + categories.Count + "\n\n" +
				" <b> Teams templates: </b>: " + teams.Count + "\n\n";
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void OnButton2Clicked (object sender, EventArgs e)
	{
		Application.Quit();
	}
	
	void UpdateText (string t) {
		buf += t;
		textview1.Buffer.Text = buf;
	}

	protected void OnButton1Clicked (object sender, EventArgs e)
	{
		string dbdir =  System.IO.Path.Combine (homeDirectory, "db"); 
		string teamdir =  System.IO.Path.Combine (homeDirectory, "db", "teams"); 
		string analysisdir =  System.IO.Path.Combine (homeDirectory, "db", "analysis"); 
		bool withError;
		MessageDialog d;
		
		scrolledwindow1.Visible = true ;
		label2.Visible = false;
		label3.Visible = false;
		withError = false;
		
		textview1.Buffer.Text = buf;
		if (!Directory.Exists (teamdir)) {
			UpdateText ("Creating directory " + teamdir + "\n");
			Directory.CreateDirectory (teamdir);
		}
		if (!Directory.Exists (analysisdir)) {
			UpdateText ("Creating directory " + analysisdir + "\n");
			Directory.CreateDirectory (analysisdir);
		}
		
		foreach (string f in dbs) {
			UpdateText ("Converting dabase " + f + "...");
			try {
				LongoMatch.Migration.Converter.ConvertDB (f, dbdir);
				UpdateText ("OK\n");
			} catch (Exception ex) {
				UpdateText ("ERROR\n");
				UpdateText (ex.ToString ());
				withError = true;
			}
		}
		foreach (string f in teams) {
			UpdateText ("Converting team template " + f + "...");
			try {
				string p = System.IO.Path.Combine (teamdir, System.IO.Path.GetFileName (f));
				LongoMatch.Migration.Converter.ConvertTeamTemplate (f, p);
				UpdateText ("OK\n");
			} catch (Exception ex) {
				UpdateText ("ERROR\n");
				UpdateText (ex.ToString ());
				withError = true;
			}
		}
		foreach (string f in categories) {
			UpdateText ("Converting analysis template " + f + "...");
			try {
				string p = System.IO.Path.Combine (analysisdir, System.IO.Path.GetFileName (f));
				LongoMatch.Migration.Converter.ConvertCategories (f, p);
				UpdateText ("OK\n");
			} catch (Exception ex) {
				UpdateText ("ERROR\n");
				UpdateText (ex.ToString ());
				withError = true;
			}
		}
		
		if (!withError) {
			d = new MessageDialog (this, DialogFlags.Modal, MessageType.Info,
			                       ButtonsType.Ok, "Everything migrated correctly!");
			d.Run();
			Application.Quit();
		} else {
			button1.Visible = false;
			d = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Error,
			                       ButtonsType.Ok, "Some errors where found migrating the old content.");
			d.Run();
		}
	}
}
