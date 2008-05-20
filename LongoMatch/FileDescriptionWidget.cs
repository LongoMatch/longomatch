// FileDescriptionWidget.cs
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
using Mono.Unix;
using Gtk;


namespace LongoMatch
{

	
	public partial class FileDescriptionWidget : Gtk.Bin
	{

		private DateTime date;
		private FileData fData;
		private CalendarPopup cp;
		
		
		public FileDescriptionWidget()
		{
			this.Build();
			cp = new CalendarPopup();
			cp.Hide();
			cp.DateSelectedEvent += new DateSelectedHandler(OnDateSelected);
			date = System.DateTime.Today;
			dateEntry.Text = date.ToString(Catalog.GetString("MM/dd/yyyy"));
			string[] allFiles = System.IO.Directory.GetFiles(MainClass.TemplatesDir(),"*.sct");
			foreach (string filePath in allFiles){
				combobox1.AppendText(System.IO.Path.GetFileNameWithoutExtension(filePath));
			}
			combobox1.Active=0;
			
		}
		
		public string LocalName {
			get { return localTeamEntry.Text; }
			set { this.localTeamEntry.Text = value;}
		}
		
		public string VisitorName{
			get { return visitorTeamEntry.Text; }
			set { this.visitorTeamEntry.Text = value;}
		}
		
		public int LocalGoals{
			get { return (int)localSpinButton.Value; }
			set { this.localSpinButton.Value = value;}
		}
		
		public int VisitorGoals{
			get { return (int)visitorSpinButton.Value; }
			set { this.visitorSpinButton.Value = value;}
		}
		
		public string Filename {
			get { return fileEntry.Text;}
			set { this.fileEntry.Text = value;}
		}
		
		public DateTime Date{
			get {return date;}
			set { this.dateEntry.Text = value.ToString(Catalog.GetString("MM/dd/yyyy"));}
		}
		
		public int VisibleSections{
			get { return (int)dataSpinButton.Value;}
			set { this.dataSpinButton.Value = value;}
		}
		
		public string SectionsFile{
			get {
				string filename =  combobox1.ActiveText + ".sct";
				return filename;
				}
		}
		

		public void SetFileData(FileData fData){
			this.fData = fData;
			this.Filename = fData.Filename;
			this.LocalName = fData.LocalName;
			this.VisitorName = fData.VisitorName;
			this.LocalGoals = fData.LocalGoals;
			this.VisitorGoals = fData.VisitorGoals;
			this.Date= fData.MatchDate;
			this.VisibleSections =  fData.VisibleSections;	
			
			//Cambiamos el gui

			this.combobox1.Visible = false;
			this.editbutton.Sensitive = true;
			
		}
		
		public void UpdateFileData(){
			fData.Filename=this.fileEntry.Text;
			fData.LocalName = this.localTeamEntry.Text;
			fData.VisitorName = this.visitorTeamEntry.Text;
			fData.LocalGoals = (int)this.localSpinButton.Value;
			fData.VisitorGoals = (int)this.visitorSpinButton.Value;
			fData.MatchDate = DateTime.Parse(this.dateEntry.Text);
			fData.VisibleSections = (int)this.dataSpinButton.Value;
		
		}
		
	
		
		public FileData GetFileData(){
			if (this.Filename != ""){
				SectionsReader reader = new SectionsReader(this.SectionsFile);
				Sections sections = reader.GetSections();
				sections.VisibleSections = this.VisibleSections;
				
				return new FileData(this.Filename,
					                    this.LocalName,
					                    this.VisitorName,
					                    this.LocalGoals,
					                    this.VisitorGoals,
					                    this.Date,
					                    sections);
				
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
			this.VisibleSections =  0;
			
		}

		protected virtual void OnDateSelected(DateTime dateTime){
			this.dateEntry.Text = dateTime.ToString(Catalog.GetString("MM/dd/yyyy"));
		}
		
		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			
			FileChooserDialog fChooser = new FileChooserDialog(Catalog.GetString("Choose the file to open"),
			                                                   null,
			                                                   FileChooserAction.Open,
			                                                   "gtk-cancel",ResponseType.Cancel,
			                                                   "gtk-open",ResponseType.Accept);
			if (fChooser.Run() == (int)ResponseType.Accept){
				fileEntry.Text = fChooser.Filename;
			}
		
			fChooser.Destroy();
		}
	







	protected virtual void OnCalendarbuttonClicked (object sender, System.EventArgs e)
	{
			cp.Show();
	}

	protected virtual void OnEditbuttonClicked (object sender, System.EventArgs e)
	{
			
			TemplateEditorDialog ted = new TemplateEditorDialog();
			ted.Sections=fData.Sections;
			
			if (ted.Run() == (int)ResponseType.Apply){
				fData.Sections = ted.Sections;
			}
			
			ted.Destroy();
	}

		
	}
}
