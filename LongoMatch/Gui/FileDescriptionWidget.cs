// FileDescriptionWidget.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using Mono.Unix;
using Gtk;
using LongoMatch.DB;
using LongoMatch.Handlers;
using LongoMatch.IO;
using LongoMatch.Gui.Popup;
using LongoMatch.Gui.Dialog;
using LongoMatch.TimeNodes;
using LongoMatch.Video.Utils;


namespace LongoMatch.Gui.Component
{

	public enum UseType{
		NewCaptureProject,
		NewFromFileProject,
		EditProject,		
	}
	//TODO añadir eventos de cambios para realizar el cambio directamente sobre el file data abierto
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class FileDescriptionWidget : Gtk.Bin
	{
		private Project project;
		private LongoMatch.Video.Utils.PreviewMediaFile mFile;
		private CalendarPopup cp;
		private Win32CalendarDialog win32CP;
		private Sections actualSection;
		private TeamTemplate actualVisitorTeam;
		private TeamTemplate actualLocalTeam;
		private UseType useType;		
		
		public FileDescriptionWidget()
		{	
				
			this.Build();
			
			//HACK:The calendar dialog does not respond on win32
			if (Environment.OSVersion.Platform != PlatformID.Win32NT){				
				cp = new CalendarPopup();			
				cp.Hide();			
				cp.DateSelectedEvent += new DateSelectedHandler(OnDateSelected);
			}
			
			FillSections();
			FillTeamsTemplate();
			
			this.Use=UseType.NewFromFileProject;					
		}
		
		public UseType Use{
			set{
				if (value == UseType.NewFromFileProject  || value == UseType.EditProject){					
					videobitratelabel.Hide();
					bitratespinbutton.Hide();
				}
					
				if (value == UseType.EditProject){				
					tagscombobox.Visible = false;
					localcombobox.Visible = false;
					visitorcombobox.Visible = false;
				}
				useType = value;
			}
			get{
				return this.useType;
			}				
		}
		
		public string LocalName {
			get { return localTeamEntry.Text; }
			set { this.localTeamEntry.Text = value;}
		}
		
		public string VisitorName{
			get { return visitorTeamEntry.Text; }
			set { this.visitorTeamEntry.Text = value;}
		}
			
		public string Season{
			get{return seasonentry.Text;}
			set{seasonentry.Text = value;}
		}
		
		public string Competition{
			get{return competitionentry.Text;}
			set{competitionentry.Text = value;}
		}
		
		public int LocalGoals{
			get { return (int)localSpinButton.Value; }
			set { this.localSpinButton.Value = value;}
		}
		
		public int VisitorGoals{
			get { return (int)visitorSpinButton.Value; }
			set { visitorSpinButton.Value = value;}
		}
		
		private string Filename {
			get { return fileEntry.Text;}
			set { fileEntry.Text = value;}
		}
		
		public DateTime Date{
			get {
				//HACK See bug http://bugzilla.gnome.org/show_bug.cgi?id=592934
				//dateEntry is not editable and we set the date manually
				//Why do we get this error?
				DateTime date;
				try{
					date = DateTime.Parse(dateEntry.Text);
				}
				catch{
					date = DateTime.Now;
				}
				
				return DateTime.Parse(dateEntry.Text);}
			set {dateEntry.Text = value.ToString(Catalog.GetString("MM/dd/yyyy"));}
		}
		
		public Sections Sections{
			get {return this.actualSection;}
			set {actualSection = value;}
		}	
		
		public TeamTemplate LocalTeamTemplate{
			get {return actualLocalTeam;}
			set {actualLocalTeam = value;}
		}
		
		public TeamTemplate VisitorTeamTemplate{
			get {return actualVisitorTeam;}
			set {actualVisitorTeam = value;}
		}
		
		private string SectionsFile{
			get {return tagscombobox.ActiveText + ".sct";}
		}
		
		private string LocalTeamTemplateFile{
			get {return localcombobox.ActiveText + ".tem";}
		}
		
		private string VisitorTeamTemplateFile{
			get {return visitorcombobox.ActiveText + ".tem";}
		}

		public void SetProject(Project project){
			this.project = project;
			mFile = project.File;
			Filename = mFile.FilePath;
			LocalName = project.LocalName;
			VisitorName = project.VisitorName;
			LocalGoals = project.LocalGoals;
			VisitorGoals = project.VisitorGoals;
			Date = project.MatchDate;
			Season = project.Season;
			Competition = project.Competition;
			Sections = project.Sections;
			LocalTeamTemplate = project.LocalTeamTemplate;
			VisitorTeamTemplate = project.VisitorTeamTemplate;
		}
		
		public void UpdateProject(){
			project.File= mFile;
			project.LocalName = localTeamEntry.Text;
			project.VisitorName = visitorTeamEntry.Text;
			project.LocalGoals = (int)localSpinButton.Value;
			project.VisitorGoals = (int)visitorSpinButton.Value;
			project.MatchDate = DateTime.Parse(dateEntry.Text);
			project.Competition = competitionentry.Text;
			project.Season = seasonentry.Text;
			project.Sections = Sections;
			project.LocalTeamTemplate = LocalTeamTemplate;
			project.VisitorTeamTemplate = VisitorTeamTemplate;
			
		}	
		
		public Project GetProject(){
			if (this.Filename != ""){								
				if (useType == UseType.NewFromFileProject){
					return new Project(mFile,
					                   LocalName,
					                   VisitorName,
					                   Season,
					                   Competition,
					                   LocalGoals,
					                   VisitorGoals,
					                   Date,					                   
					                   Sections,
					                   LocalTeamTemplate,
					                   VisitorTeamTemplate);
					
				}
				else {
					UpdateProject();
					return project;						
				}				
			}
			else return null;
		}
		
		public void Clear(){			
			this.LocalName = "";
			this.VisitorName = "";
			this.LocalGoals = 0;
			this.VisitorGoals = 0;
			this.Date = System.DateTime.Today;
			this.Filename = "";
			this.mFile = null;		
		}
		
		
		
		private void FillSections(){
			string[] allFiles;
			int i=0;
			int index = 0;
			
			allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*.sct");
			foreach (string filePath in allFiles){
				string fileName = System.IO	.Path.GetFileNameWithoutExtension(filePath);
				tagscombobox.AppendText(fileName);
				//Setting the selected value to the default template
				if (fileName == "default")
					index = i;
				i++;
			}
			tagscombobox.Active = index;			
			SectionsReader reader = new SectionsReader(System.IO.Path.Combine(MainClass.TemplatesDir(),SectionsFile));			
			this.Sections= reader.GetSections();	
		}
		
		private void FillTeamsTemplate(){
			string[] allFiles;
			int i=0;
			int index = 0;
			
			allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*.tem");
			foreach (string filePath in allFiles){
				string fileName = System.IO	.Path.GetFileNameWithoutExtension(filePath);
				localcombobox.AppendText(fileName);
				visitorcombobox.AppendText(fileName);

				//Setting the selected value to the default template
				if (fileName == "default")
					index = i;
				i++;
			}
			localcombobox.Active = index;	
			visitorcombobox.Active = index;	
			LocalTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(),LocalTeamTemplateFile));
			VisitorTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(),VisitorTeamTemplateFile));
		}

		protected virtual void OnDateSelected(DateTime dateTime){
			Date = dateTime;
		}
		
		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{		
			FileChooserDialog fChooser = null;
			
			if (this.useType == UseType.NewCaptureProject){
				fChooser = new FileChooserDialog(Catalog.GetString("Save File as..."),
				                                 (Gtk.Window)this.Toplevel,
				                                 FileChooserAction.Save,
				                                 "gtk-cancel",ResponseType.Cancel,
				                                 "gtk-save",ResponseType.Accept);
				fChooser.SetCurrentFolder(MainClass.VideosDir());
				if (fChooser.Run() == (int)ResponseType.Accept)
					fileEntry.Text = fChooser.Filename;
				fChooser.Destroy();
				
			}else	{
				fChooser = new FileChooserDialog(Catalog.GetString("Open file..."),
				                                 (Gtk.Window)this.Toplevel,
				                                 FileChooserAction.Open,
				                                 "gtk-cancel",ResponseType.Cancel,
				                                 "gtk-open",ResponseType.Accept);			
			
				fChooser.SetCurrentFolder(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal));
		
				if (fChooser.Run() == (int)ResponseType.Accept){
					MessageDialog md=null;
					string filename = fChooser.Filename;
					fChooser.Destroy();
					try{
						md = new MessageDialog((Gtk.Window)this.Toplevel,
			                                     DialogFlags.Modal,
			                                     MessageType.Info,
			                                     Gtk.ButtonsType.None,
			                                     Catalog.GetString("Analyzing video file:")+"\n"+filename);
						md.Icon=Stetic.IconLoader.LoadIcon(this, "longomatch", Gtk.IconSize.Dialog, 48);
						md.Show();
						mFile = LongoMatch.Video.Utils.PreviewMediaFile.GetMediaFile(filename);
						fileEntry.Text = filename;
					}
					catch (Exception ex){
						MessagePopup.PopupMessage(this, MessageType.Error, 
				                          ex.Message);
					}
					finally{
						md.Destroy();
					}					
				}
				fChooser.Destroy();
			}		
		}


		protected virtual void OnCalendarbuttonClicked (object sender, System.EventArgs e)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT){
				win32CP = new Win32CalendarDialog();
				win32CP.TransientFor = (Gtk.Window)this.Toplevel;
				win32CP.Run();
				Date = win32CP.getSelectedDate();
				win32CP.Destroy();
			}
			else {
				cp.TransientFor=(Gtk.Window)this.Toplevel;		
				cp.Show();	
			}
		}

		protected virtual void OnEditbuttonClicked (object sender, System.EventArgs e)
		{			
			TemplateEditorDialog ted = new TemplateEditorDialog();
			ted.Sections = Sections;
			
			if (ted.Run() == (int)ResponseType.Apply){
				this.Sections = ted.Sections;
			}			
			ted.Destroy();
		}
		
		protected virtual void OnCombobox1Changed (object sender, System.EventArgs e)
		{
			SectionsReader reader = new SectionsReader(System.IO.Path.Combine(MainClass.TemplatesDir(),SectionsFile));			
			Sections= reader.GetSections();
		}
		
		protected virtual void OnVisitorcomboboxChanged (object sender, System.EventArgs e)
		{
			VisitorTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(), VisitorTeamTemplateFile));
		}
		
				
		protected virtual void OnLocalcomboboxChanged (object sender, System.EventArgs e)
		{
			LocalTeamTemplate = TeamTemplate.LoadFromFile(System.IO.Path.Combine(MainClass.TemplatesDir(), LocalTeamTemplateFile));
		}
		
			
		protected virtual void OnLocaltemplatebuttonClicked (object sender, System.EventArgs e){			
			TeamTemplateEditor tted = new TeamTemplateEditor();
	
			tted.SetTeamTemplate(LocalTeamTemplate);	
			if (tted.Run() == (int)ResponseType.Apply){
				LocalTeamTemplate = tted.GetTeamTemplate();
			}			
			tted.Destroy();			
		}
		

		protected virtual void OnVisitorbuttonClicked (object sender, System.EventArgs e){
			TeamTemplateEditor tted = new TeamTemplateEditor();
			tted.SetTeamTemplate(VisitorTeamTemplate);	
			if (tted.Run() == (int)ResponseType.Apply){
				VisitorTeamTemplate = tted.GetTeamTemplate();
			}			
			tted.Destroy();			
		}	
	
		
	}	
		
}
