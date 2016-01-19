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
using System.Linq;
using System.IO;
using Gtk;
using System.Collections.Generic;
using LongoMatch.DB;
using LongoMatch.Store;
using LongoMatch.Core;
using LongoMatch.Common;

public partial class MainWindow: Gtk.Window
{	

	string buf;
	List<string> teams, categories, dbs;
	
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();

		buf = "";

		convertbutton.Clicked += HandleConvertClicked;
		closebutton.Clicked += HandleCloseClicked;
	}

	public void Load ()
	{
		FindFiles ();
		UpdateLabel ();
	}
		
	void FindFiles ()
	{
		string dbdir, templatesdir;
		
		dbs = new List<string> ();
		teams = new List<string> (); 
		categories = new List<string> ();
		
		dbdir = System.IO.Path.Combine (LongoMatch.Config.ConfigDir, "db");
		if (Directory.Exists (dbdir)) {
			foreach (string file in Directory.GetFiles (dbdir)) {
				if (file.EndsWith ("1.db")) {
					dbs.Add (file);
				}
			}
		}
		
		templatesdir = System.IO.Path.Combine (LongoMatch.Config.HomeDir, "templates");
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
		
		if (dbs.Count == 0 && teams.Count == 0 && categories.Count == 0) {
			Gtk.MessageDialog dialog = new MessageDialog (this,
			                                              DialogFlags.Modal | DialogFlags.DestroyWithParent,
			                                              MessageType.Info, ButtonsType.Ok, 
			                                              Catalog.GetString ("Nothing to migrate from the old version"));
			dialog.Run();
			Application.Quit ();
		}
	}
	
	void UpdateLabel () {
		label3.Markup = String.Format (" <b> {0} </b>: {1}\n\n <b> {2} </b>: {3}\n\n <b> {4} </b>: {5}\n\n",
		                               Catalog.GetString ("Databases"), dbs.Count,
		                               Catalog.GetString ("Dashboards"), categories.Count,
		                               Catalog.GetString ("Teams"), teams.Count);
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void HandleCloseClicked (object sender, EventArgs e)
	{
		Application.Quit();
	}
	
	void UpdateText (string t) {
		Application.Invoke (delegate {
			buf += t;
			textview1.Buffer.Text = buf;
			progressbar1.Pulse ();
		});
	}

	void StartMigrationThread ()
	{
		string dbdir = System.IO.Path.Combine (LongoMatch.Config.HomeDir, "db");
		string teamdir = System.IO.Path.Combine (LongoMatch.Config.HomeDir, "db", "teams");
		string analysisdir = System.IO.Path.Combine (LongoMatch.Config.HomeDir, "db", "analysis");
		bool withError = false;
		MessageDialog d;

		if (!Directory.Exists (teamdir)) {
			UpdateText ("Creating directory " + teamdir + "\n");
			Directory.CreateDirectory (teamdir);
		}
		if (!Directory.Exists (analysisdir)) {
			UpdateText ("Creating directory " + analysisdir + "\n");
			Directory.CreateDirectory (analysisdir);
		}
		foreach (string dbfile in dbs) {
			UpdateText ("Converting dabase " + dbfile + "..." + "\n");
			try {
				string dboutputdir;
				string dbname;
				DataBase db;
				dbname = System.IO.Path.GetFileName (dbfile).Split ('.') [0] + ".ldb";
				dboutputdir = System.IO.Path.Combine (dbfile, System.IO.Path.Combine (dbdir, dbname));
				if (!Directory.Exists (dboutputdir)) {
					Directory.CreateDirectory (dboutputdir);
				}
				db = new DataBase (dbfile);
				foreach (ProjectDescription pd in db.GetAllProjects ()) {
					try {
						Project p = db.GetProject (pd.UUID);
						UpdateText ("Converting project " + p.Description.Title + "..." + "\n");
						LongoMatch.Migration.Converter.ConvertProject (p, dboutputdir);
					} catch (Exception ex) {
						UpdateText ("ERROR\n");
						UpdateText (ex.ToString ());
						withError = true;
					}
				}
				System.IO.File.Delete (System.IO.Path.Combine (dboutputdir, db.Name + ".ldb"));
				UpdateText ("OK\n");
			} catch (Exception ex) {
				UpdateText ("ERROR\n");
				UpdateText (ex.ToString ());
				withError = true;
			}
		}
		foreach (string f in teams) {
			if (System.IO.Path.GetFileNameWithoutExtension (f) == "default") {
				continue;
			}
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
			if (System.IO.Path.GetFileNameWithoutExtension (f) == "default") {
				continue;
			}
			UpdateText ("Converting analysis template " + f + "...");
			try {
				string p = System.IO.Path.Combine (analysisdir, System.IO.Path.GetFileName (f));
				LongoMatch.Migration.Converter.ConvertCategories (f, p);
				UpdateText ("OK\n");
			}
			catch (Exception ex) {
				UpdateText ("ERROR\n");
				UpdateText (ex.ToString ());
				withError = true;
			}
		}
		Application.Invoke (delegate {
			if (!withError) {
				d = new MessageDialog (this, DialogFlags.Modal, MessageType.Info, ButtonsType.Ok, "Everything migrated correctly!");
				d.Run ();
				Application.Quit ();
			}
			else {
				convertbutton.Visible = false;
				progressbar1.Visible = false;
				d = new MessageDialog (this, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Ok, "Some errors where found migrating the old content.");
				d.Run ();
				d.Destroy ();
			}
		});
	}

	protected void HandleConvertClicked (object sender, EventArgs e)
	{
		scrolledwindow1.Visible = true;
		label2.Visible = false;
		label3.Visible = false;
		textview1.Buffer.Text = buf;
		progressbar1.Visible = true;
		System.Threading.Thread t = new System.Threading.Thread (StartMigrationThread);
		t.Start ();
	}
}
