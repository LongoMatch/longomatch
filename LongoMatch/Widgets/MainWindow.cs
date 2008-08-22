// MainWindow.cs
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
using System.IO;
using GLib;
using System.Threading;
using Gdk;
using LongoMatch.DB;
using LongoMatch.TimeNodes;
using LongoMatch.Widgets.Dialog;
using LongoMatch.Widgets;
using LongoMatch.Video.Player;


namespace LongoMatch
{	
	public partial class MainWindow : Gtk.Window
	{
		private static Project openedProject;

		private TimeNode selectedTimeNode;
		private EventsManager eManager;


		
		public MainWindow() : 
				base("LongoMatch")
		{			
			this.Build();
			this.eManager = new EventsManager(this.treewidget1,this.buttonswidget1,this.playlistwidget2,
			                                  this.playerbin1,this.timelinewidget1,this.progressbar1);
			playerbin1.SetLogo("background.png");

			playerbin1.LogoMode = true;
			this.playlistwidget2.SetPlayer(playerbin1);


		}

		

		private void SetProject(Project project){			
			openedProject = project;
			this.eManager.OpenedProject = project;
			if (project!=null){		
				if(!File.Exists(project.File.FilePath)){
					MessageDialog infoDialog = new MessageDialog (this,DialogFlags.Modal,MessageType.Warning,ButtonsType.Ok,Catalog.GetString("The file associated to this proyect doesn't exits.\n If the location of the file has changed try to change it with de DataBase Manager.") );
					infoDialog.Run();
					infoDialog.Destroy();
					this.CloseActualProyect();					
				}
				else {
					
					this.Title = System.IO.Path.GetFileNameWithoutExtension(project.File.FilePath) + " - LongoMatch";
					this.playerbin1.Open(project.File.FilePath);
					this.playlistwidget2.Stop();
					
					this.treewidget1.Project=project;						
					this.timelinewidget1.Project = project;
					this.buttonswidget1.Sections = project.Sections;	
					if (project.File.HasVideo){
						this.playerbin1.LogoMode = false;
						this.FullScreenAction.Sensitive = true;
					}
					this.CloseProjectAction.Sensitive=true;
					this.SaveProjectAction.Sensitive = true;
					this.PlayerAction.Sensitive= true;
					this.CaptureModeAction.Sensitive = true;
					this.AnalyzeModeAction.Sensitive = true;
				
					this.ShowWidgets();
				}
			}			
		}
		
		public static Project OpenedProject(){			
			return openedProject;
		}
		
		private void ShowWidgets(){
			this.leftbox.Show();
			if (this.CaptureModeAction.Active)
				this.buttonswidget1.Show();
			else 
				this.timelinewidget1.Show();
		}
		
		private void HideWidgets(){
			this.leftbox.Hide();
			this.buttonswidget1.Hide();
			this.timelinewidget1.Hide();
		}
				
	    private void CloseActualProyect(){
			this.Title = "LongoMatch";
			this.HideWidgets();
			this.playerbin1.Close();			
			this.playerbin1.LogoMode = true;
			this.SaveDB();			
			openedProject = null;	
			this.eManager.OpenedProject = null;
			this.selectedTimeNode = null;
			this.CloseProjectAction.Sensitive=false;
			this.SaveProjectAction.Sensitive = false;
			this.PlayerAction.Sensitive= false;
			this.CaptureModeAction.Sensitive = false;
			this.AnalyzeModeAction.Sensitive = false;
			this.FullScreenAction.Sensitive = false;
		}
		
		private void SaveDB(){			
			if (openedProject != null){
				MainClass.DB.UpdateProject(OpenedProject());

			}
			
		}
		
		protected virtual void OnUnrealized(object sender, System.EventArgs e){
			this.Destroy();			
			Application.Quit();					
		}
		
		
		protected virtual void OnSectionsTemplatesManagerActivated (object sender, System.EventArgs e)
		{
			SectionsTemplates st = new SectionsTemplates();
			st.Show();
		}

		protected virtual void OnOpenActivated (object sender, System.EventArgs e)
		{
			Project project;
			OpenProjectDialog opd = new OpenProjectDialog();
			int answer=opd.Run();
			while (answer == (int)ResponseType.Reject){
				project = opd.GetSelection();
				MainClass.DB.RemoveProject(project);
				opd.Fill();
				answer=opd.Run();
			}
			if (answer == (int)ResponseType.Ok){
				project = opd.GetSelection();
				this.SetProject(project);
			}
			opd.Destroy();
		}

		protected virtual void OnNewActivated (object sender, System.EventArgs e)
		{
			Project project;
			NewProjectDialog npd = new NewProjectDialog();
			// Esperamos a que se pulse el boton aceptar y se cumplan las condiciones para 
			// crear un nuevo objeto del tipo Project
			int response = npd.Run();
			while (response == (int)ResponseType.Ok && npd.GetProject() == null){
				MessageDialog md = new MessageDialog(npd,
				                                     DialogFlags.DestroyWithParent,
				                                     MessageType.Info,
				                                     ButtonsType.Ok,
				                                     Catalog.GetString("Please, select a video file."));
				md.Run();
				md.Destroy();	
				response=npd.Run();
			}
			npd.Destroy();
			// Si se cumplen las condiciones y se ha pulsado el botón aceptar continuamos
			if (response ==(int)ResponseType.Ok){
				project = npd.GetProject();
				if (project != null){
					try{
						MainClass.DB.AddProject(project);
						this.SetProject(project);
					}
					catch {						
						MessageDialog error = new MessageDialog(this,
						                                        DialogFlags.DestroyWithParent,
						                                        MessageType.Error,
						                                        ButtonsType.Ok,
						                                        "The Project for this file already exists.\nTry to edit it.");
						error.Run();
						error.Destroy();							
					}
				}
			}
		}

		
		
		protected virtual void OnCloseActivated (object sender, System.EventArgs e)
		{

			this.SaveDB();
			this.CloseActualProyect();			
		}

		protected virtual void OnDatabaseManagerActivated (object sender, System.EventArgs e)
		{
			DBManager db = new DBManager();
			db.Show();
		}		

		protected virtual void OnTimeprecisionadjustwidget1SizeRequested (object o, Gtk.SizeRequestedArgs args)
		{
			if (args.Requisition.Width>= hpaned.Position)
				hpaned.Position = args.Requisition.Width;
		}
		
		



		protected virtual void OnDeleteEvent (object o, Gtk.DeleteEventArgs args)
		{
			this.SaveDB();
			this.playerbin1.Dispose();
			Application.Quit();
					
		}


		protected virtual void OnQuitActivated (object sender, System.EventArgs e)
		{

			this.SaveDB();
			this.playerbin1.Destroy();
			Application.Quit();
		}

		protected virtual void OnPlaylistActionToggled (object sender, System.EventArgs e)
		{			
				this.playlistwidget2.Visible=((Gtk.ToggleAction)sender).Active;			
		}

		protected virtual void OnOpenPlaylistActionActivated (object sender, System.EventArgs e)
		{
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Open playlist"),
			                                                   (Gtk.Window)this.Toplevel,
			                                                   FileChooserAction.Open,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-open",ResponseType.Accept);
			fChooser.SetCurrentFolder(MainClass.PlayListDir());
			FileFilter filter = new FileFilter();
			filter.Name = "LGM playlist";
			filter.AddPattern("*.lgm");
			
			fChooser.AddFilter(filter);
			if (fChooser.Run() == (int)ResponseType.Accept){
				if (openedProject != null)
					this.CloseActualProyect();
				this.playlistwidget2.Load(fChooser.Filename);				
				this.PlaylistAction.Active = true;				
			}		
			fChooser.Destroy();			
		}

		protected virtual void OnPlayerbin1Error (object o,LongoMatch.Handlers.ErrorArgs args)
		{
			MessageDialog errorDialog = new MessageDialog (this,DialogFlags.Modal,MessageType.Error,ButtonsType.Ok,Catalog.GetString 
			                                               ("The actual Proyect will bo closed due to this error on the media player:\n") +args.Message);
			errorDialog.Run();
			errorDialog.Destroy();	
			this.CloseActualProyect();
		}

		


		protected virtual void OnCaptureModeActionToggled (object sender, System.EventArgs e)
		{

			if (((Gtk.ToggleAction)sender).Active){
				this.buttonswidget1.Show();
				this.timelinewidget1.Hide();
			}
			else{
				this.buttonswidget1.Hide();
				this.timelinewidget1.Show();
			}
			
			
		}

		

		protected virtual void OnFullScreenActionToggled (object sender, System.EventArgs e)
		{
		
				this.playerbin1.FullScreen = ((Gtk.ToggleAction)sender).Active;
		}

		protected virtual void OnSaveProjectActionActivated (object sender, System.EventArgs e)
		{
			this.SaveDB();
		}

		
		

		
	}
}
