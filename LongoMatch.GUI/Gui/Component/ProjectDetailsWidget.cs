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
//Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using Gtk;
using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Popup;
using LongoMatch.Core.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video.Utils;
using LongoMatch.Gui.Helpers;
using Misc = LongoMatch.Gui.Helpers.Misc;
using LongoMatch.Multimedia.Utils;

namespace LongoMatch.Gui.Component
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public partial class ProjectDetailsWidget : Gtk.Bin
	{
		public event EventHandler EditedEvent;
		Project project;
		MediaFile mFile;
		bool edited;
		DateTime date;
		CalendarPopup cp;
		Win32CalendarDialog win32CP;
		Categories actualCategory;
		TeamTemplate actualVisitorTeam;
		TeamTemplate actualLocalTeam;
		
		ICategoriesTemplatesProvider tpc;
		ITeamTemplatesProvider tpt;
		ITemplatesService service;
		ProjectType useType;
		List<Device> videoDevices;
		ListStore videoStandardList, encProfileList, qualList;


		public ProjectDetailsWidget()
		{
			this.Build();

			//HACK:The calendar dialog does not respond on win32
			if(Environment.OSVersion.Platform != PlatformID.Win32NT) {
				cp = new CalendarPopup();
				cp.Hide();
				cp.DateSelectedEvent += new DateSelectedHandler(OnDateSelected);
			}
			
			FillFormats();
			videoDevices = new List<Device>();
			Use=ProjectType.FileProject;
		}
		
		public ITemplatesService TemplatesService {
			set {
				tpc = value.CategoriesTemplateProvider;
				tpt = value.TeamTemplateProvider;
				service = value;
				FillCategories();
				FillTeamsTemplate();
			}
		}
		
		public ProjectType Use {
			set {
				bool deviceVisible, encodingVisible, fileVisible, editionVisible, uriVisible;
				
				deviceVisible = value == ProjectType.CaptureProject;
				encodingVisible = (value == ProjectType.CaptureProject ||
				                   value == ProjectType.URICaptureProject);
				fileVisible = value != ProjectType.FakeCaptureProject;
				editionVisible = value != ProjectType.EditProject;
				uriVisible = value == ProjectType.URICaptureProject;

				filelabel.Visible = fileVisible;
				filehbox.Visible = fileVisible;

				tagscombobox.Visible = editionVisible;
				localcombobox.Visible = editionVisible;
				visitorcombobox.Visible = editionVisible;
				localteamlabel.Visible = !editionVisible;
				visitorteamlabel.Visible = !editionVisible;

				expander1.Visible = encodingVisible;
				
				device.Visible = deviceVisible;
				devicecombobox.Visible = deviceVisible;
				
				urilabel.Visible = uriVisible;
				urientry.Visible = uriVisible;

				useType = value;
			}
			get {
				return useType;
			}
		}

		public bool Edited {
			set {
				edited=value;
			}
			get {
				return edited;
			}
		}

		public string Season {
			get {
				return seasonentry.Text;
			}
			set {
				seasonentry.Text = value;
			}
		}

		public string Competition {
			get {
				return competitionentry.Text;
			}
			set {
				competitionentry.Text = value;
			}
		}

		public int LocalGoals {
			get {
				return (int)localSpinButton.Value;
			}
			set {
				localSpinButton.Value = value;
			}
		}

		public int VisitorGoals {
			get {
				return (int)visitorSpinButton.Value;
			}
			set {
				visitorSpinButton.Value = value;
			}
		}

		private string Filename {
			get {
				return fileEntry.Text;
			}
			set {
				fileEntry.Text = value;
			}
		}

		public DateTime Date {
			get {
				return date;
			}
			set {
				date = value;
				dateEntry.Text = value.ToShortDateString();
			}
		}

		public Categories Categories {
			get {
				return actualCategory;
			}
			set {
				actualCategory = value;
			}
		}

		public TeamTemplate LocalTeamTemplate {
			get {
				return actualLocalTeam;
			}
			set {
				localteamlabel.Text = value.TeamName;
				actualLocalTeam = value;
			}
		}

		public TeamTemplate VisitorTeamTemplate {
			get {
				return actualVisitorTeam;
			}
			set {
				visitorteamlabel.Text = value.TeamName;
				actualVisitorTeam = value;
			}
		}

		private string SelectedCategory {
			get {
				return tagscombobox.ActiveText;
			}
		}

		private string LocalTeamTemplateFile {
			get {
				return localcombobox.ActiveText;
			}
		}

		private string VisitorTeamTemplateFile {
			get {
				return visitorcombobox.ActiveText;
			}
		}

		public CaptureSettings CaptureSettings {
			get {
				TreeIter iter;
				EncodingSettings encSettings = new EncodingSettings();
				CaptureSettings s = new CaptureSettings();
				
				encSettings.OutputFile = fileEntry.Text;
				if (useType == ProjectType.CaptureProject) {
					s.CaptureSourceType = videoDevices[devicecombobox.Active].DeviceType;
					s.DeviceID = videoDevices[devicecombobox.Active].ID;
					s.SourceElement = videoDevices[devicecombobox.Active].SourceElement;
				} else if (useType == ProjectType.URICaptureProject) {
					s.CaptureSourceType = CaptureSourceType.URI;
					s.DeviceID = urientry.Text;
				}
				
				/* Get quality info */
				qualitycombobox.GetActiveIter(out iter);
				encSettings.EncodingQuality = (EncodingQuality) qualList.GetValue(iter, 1);
				
				/* Get size info */
				imagecombobox.GetActiveIter(out iter);
				encSettings.VideoStandard = (VideoStandard) videoStandardList.GetValue(iter, 1);
			
				/* Get encoding profile info */
				encodingcombobox.GetActiveIter(out iter);
				encSettings.EncodingProfile = (EncodingProfile) encProfileList.GetValue(iter, 1);
				
				/* FIXME: Configure with the UI */
				encSettings.Framerate_n = Config.FPS_N;
				encSettings.Framerate_d = Config.FPS_D;
				
				s.EncodingSettings = encSettings;
				return s;
			}
		}

		public void SetProject(Project project) {
			this.project = project;
			if (project == null)
				return;
			var desc = project.Description;
			mFile = desc.File;
			Filename = mFile != null ? mFile.FilePath : "";
			LocalGoals = desc.LocalGoals;
			VisitorGoals = desc.VisitorGoals;
			Date = desc.MatchDate;
			Season = desc.Season;
			Competition = desc.Competition;
			Categories = project.Categories;
			LocalTeamTemplate = project.LocalTeamTemplate;
			VisitorTeamTemplate = project.VisitorTeamTemplate;
			Edited = false;
		}

		public void UpdateProject() {
			var desc = project.Description;
			/* In case the framerate changed, update each play with the new
			 * framerate */
			if (desc.File != null && desc.File.Fps != mFile.Fps) {
				foreach (Play play in project.AllPlays ()) {
					play.Fps = mFile.Fps;
				}
			}
			desc.File= mFile;
			desc.LocalGoals = (int)localSpinButton.Value;
			desc.VisitorGoals = (int)visitorSpinButton.Value;
			desc.MatchDate = DateTime.Parse(dateEntry.Text);
			desc.Competition = competitionentry.Text;
			desc.Season = seasonentry.Text;
			project.Categories = Categories;
			project.LocalTeamTemplate = LocalTeamTemplate;
			project.VisitorTeamTemplate = VisitorTeamTemplate;
		}

		public Project GetProject() {
			if(useType != ProjectType.EditProject) {
				if(Filename == "" && useType != ProjectType.FakeCaptureProject) {
					return null;
				} else if(urientry.Text == "" && useType == ProjectType.URICaptureProject) {
					return null;
				} else {
					if(useType == ProjectType.FakeCaptureProject) {
						mFile = new MediaFile();
						mFile.FilePath = Constants.FAKE_PROJECT;
						mFile.Fps = 25;
					} else if(useType == ProjectType.CaptureProject ||
					          useType == ProjectType.URICaptureProject) {
						mFile = new MediaFile();
						mFile.FilePath = fileEntry.Text;
						mFile.Fps = 25;
					}
					var desc = new ProjectDescription {
						File = mFile,
						VisitorName = VisitorTeamTemplate.TeamName,
						LocalName = LocalTeamTemplate.TeamName,
						Season = Season,
						Competition = Competition,
						LocalGoals = LocalGoals,
						MatchDate = Date
					};

					if (Categories.FieldBackground == null) {
						Categories.FieldBackground =Config.FieldBackground;
					}
					if (Categories.HalfFieldBackground == null) {
						Categories.HalfFieldBackground = Config.HalfFieldBackground;
					}
					if (Categories.GoalBackground == null) {
						Categories.GoalBackground = Config.GoalBackground;
					}
					return new Project {
						Description = desc,
						Categories = Categories,
						LocalTeamTemplate = LocalTeamTemplate,
						VisitorTeamTemplate = VisitorTeamTemplate
					};
				}
			}
			else {
				// New imported project from a fake live analysis will have a null File
				// return null to force selecting a new file.
				if(mFile == null)
					return null;
				UpdateProject();
				return project;
			}
		}

		public void Clear() {
			LocalGoals = 0;
			VisitorGoals = 0;
			Date = System.DateTime.Today;
			localteamlabel.Text = "";
			visitorteamlabel.Text = "";
			Filename = "";
			mFile = null;
			edited = false;
		}

		public void FillDevices(List<Device> devices) {
			videoDevices = devices;

			foreach(Device device in devices) {
				devicecombobox.AppendText(device.Desc);
				devicecombobox.Active = 0;
			}
		}

		private void FillCategories() {
			int i=0;
			int index = 0;

			foreach(string template in  tpc.TemplatesNames) {
				tagscombobox.AppendText(template);
				//Setting the selected value to the default template
				if(template == "default")
					index = i;
				i++;
			}
			tagscombobox.Active = index;
			if (Categories == null)
				Categories = tpc.Load(SelectedCategory);
		}

		private void FillTeamsTemplate() {
			int i=0;
			int index = 0;

			foreach(string template in tpt.TemplatesNames) {
				localcombobox.AppendText(template);
				visitorcombobox.AppendText(template);

				//Setting the selected value to the default template
				if(template == "default")
					index = i;
				i++;
			}
			localcombobox.Active = index;
			visitorcombobox.Active = index;
			if (LocalTeamTemplate == null) {
				LocalTeamTemplate = tpt.Load(LocalTeamTemplateFile);
				VisitorTeamTemplate = tpt.Load(VisitorTeamTemplateFile);
			}
		}

		private void FillFormats() {
			videoStandardList = Misc.FillImageFormat (imagecombobox, Config.CaptureVideoStandard);
			encProfileList = Misc.FillEncodingFormat (encodingcombobox, Config.CaptureEncodingProfile);
			qualList = Misc.FillQuality (qualitycombobox, Config.CaptureEncodingQuality);
		}
		
		private void StartEditor(TemplateEditorDialog editor) {
			editor.TransientFor = (Window)Toplevel;
			editor.Run();
			editor.Destroy();
			OnEdited(this,null);
		}

		protected virtual void OnDateSelected(DateTime dateTime) {
			Date = dateTime;
		}

		protected virtual void OnOpenbuttonClicked(object sender, System.EventArgs e)
		{
			if(useType == ProjectType.CaptureProject || useType == ProjectType.URICaptureProject) {
				string filename;
				
				filename = FileChooserHelper.SaveFile (this, Catalog.GetString("Output file"),
				                                       "Capture.mp4", Config.VideosDir, "MP4",
				                                       new string[] {"*.mp4"});
				if (filename != null)
					fileEntry.Text = System.IO.Path.ChangeExtension(filename, "mp4");

			} else	{
				MessageDialog md=null;
				string folder, filename;
				
				folder = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
				filename = FileChooserHelper.OpenFile (this, Catalog.GetString("Open file"),
				                                       null, folder, null, null);                             
				if (filename == null)
					return;
					
				try {
					md = new MessageDialog((Gtk.Window)this.Toplevel,
					                       DialogFlags.Modal,
					                       MessageType.Info,
					                       Gtk.ButtonsType.None,
					                       Catalog.GetString("Analyzing video file:")+"\n"+filename);
					md.Icon=Stetic.IconLoader.LoadIcon(this, "longomatch", Gtk.IconSize.Dialog);
					md.Show();
					
					mFile = PreviewMediaFile.DiscoverFile(filename);
					if(!mFile.HasVideo || mFile.VideoCodec == "")
						throw new Exception(Catalog.GetString("This file doesn't contain a video stream."));
					if(mFile.HasVideo && mFile.Length == 0)
						throw new Exception(Catalog.GetString("This file contains a video stream but its length is 0."));
					if (GStreamer.FileNeedsRemux(mFile)) {
						string q = Catalog.GetString("The file you are trying to load is not properly supported. " +
						                             "Would you like to convert it into a more suitable format?");
						if (MessagesHelpers.QuestionMessage (this, q)) {
							var remux = new Remuxer(mFile);
							var newFilename = remux.Remux(this.Toplevel as Gtk.Window);
							if (newFilename != null)
								mFile = PreviewMediaFile.DiscoverFile (newFilename);
						}
					}
					fileEntry.Text = mFile.FilePath;
				}
				catch(Exception ex) {
					MessagesHelpers.ErrorMessage (this, ex.Message);
				}
				finally {
					md.Destroy();
				}
			}
		}


		protected virtual void OnCalendarbuttonClicked(object sender, System.EventArgs e)
		{
			if(Environment.OSVersion.Platform == PlatformID.Win32NT) {
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

		protected virtual void OnCombobox1Changed(object sender, System.EventArgs e)
		{
			Categories = tpc.Load(SelectedCategory);
		}

		protected virtual void OnVisitorcomboboxChanged(object sender, System.EventArgs e)
		{
			VisitorTeamTemplate = tpt.Load(VisitorTeamTemplateFile);
		}

		protected virtual void OnLocalcomboboxChanged(object sender, System.EventArgs e)
		{
			LocalTeamTemplate = tpt.Load(LocalTeamTemplateFile);
		}

		protected virtual void OnEditbuttonClicked(object sender, System.EventArgs e)
		{
			var editor = new TemplateEditorDialog<Categories, Category>(
				new CategoriesTemplateEditorWidget(service));
			editor.Template = Categories;
			if (Use == ProjectType.EditProject) {
				editor.Project = project;
				editor.CanExport = true;
			}
			StartEditor(editor);
		}

		protected virtual void OnLocaltemplatebuttonClicked(object sender, System.EventArgs e) {
			var editor = new TemplateEditorDialog<TeamTemplate, Player>(
				new TeamTemplateEditorWidget(tpt));
			editor.Template = LocalTeamTemplate;
			if (Use == ProjectType.EditProject) {
				editor.Project = project;
				editor.CanExport = true;
			}
			StartEditor(editor);
		}

		protected virtual void OnVisitorbuttonClicked(object sender, System.EventArgs e) {
			var editor = new TemplateEditorDialog<TeamTemplate, Player>(
				new TeamTemplateEditorWidget(tpt));
			editor.Template = VisitorTeamTemplate;
			if (Use == ProjectType.EditProject) {
				editor.Project = project;
				editor.CanExport = true;
			}
			StartEditor(editor);
		}

		protected virtual void OnEdited(object sender, System.EventArgs e) {
			Edited = true;
			if(EditedEvent != null)
				EditedEvent(this,null);
		}
	}
}
