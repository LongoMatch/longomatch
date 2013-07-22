// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
using System.Collections.Generic;
using System.IO;
using Gtk;
using Gdk;
using Mono.Unix;

using Image = LongoMatch.Common.Image;
using LongoMatch.Common;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Popup;
using LongoMatch.Store;
using LongoMatch.Store.Templates;
using LongoMatch.Video.Utils;
using LongoMatch.Gui.Helpers;
using LongoMatch.Stats;

namespace LongoMatch.Gui
{
	public class GUIToolkit: IGUIToolkit
	{
		static GUIToolkit instance;
		IMainWindow mainWindow;
		
		public GUIToolkit (Version version)
		{
			Version = version;
			mainWindow = new MainWindow(this);
			(mainWindow as MainWindow).Show();
			instance = this;
		}
		
		public static GUIToolkit Instance {
			get {
				return instance;
			}
		}
		
		public IMainWindow MainWindow{
			get {
				return mainWindow;
			}
		}
		
		public Version Version {
			get;
			set;
		}
		
		public void InfoMessage(string message, Widget parent=null) {
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.InfoMessage(parent, message);
		}
		
		public void ErrorMessage(string message, Widget parent=null) {
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.ErrorMessage (parent, message);
		}
		
		public void WarningMessage(string message, Widget parent=null) {
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.WarningMessage (parent, message);
		}
		
		public bool QuestionMessage(string question, string title, Widget parent=null) {
			if (parent == null)
				parent = mainWindow as Widget;
			return MessagesHelpers.QuestionMessage (parent, question, title);
		}
		
		public string SaveFile(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SaveFile (mainWindow as Widget, title, defaultName,
			                                   defaultFolder, filterName, extensionFilter);
		}
		
		public string SelectFolder(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SelectFolder (mainWindow as Widget, title, defaultName,
			                                       defaultFolder, filterName, extensionFilter);
		}
		
		public string OpenFile(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.OpenFile (mainWindow as Widget, title, defaultName,
			                                   defaultFolder, filterName, extensionFilter);
		}
		
		public List<string> OpenFiles(string title, string defaultName, string defaultFolder,
			string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.OpenFiles (mainWindow as Widget, title, defaultName,
			                                    defaultFolder, filterName, extensionFilter);
		}
		
		public List<EditionJob> ConfigureRenderingJob (IPlayList playlist)
		{
			VideoEditionProperties vep;
			List<EditionJob> jobs = new List<EditionJob>();
			int response;
			
			if (playlist.Count == 0) {
				WarningMessage(Catalog.GetString("The playlist you want to render is empty."));
				return null;
			}

			vep = new VideoEditionProperties();
			vep.TransientFor = mainWindow as Gtk.Window;
			response = vep.Run();
			while(response == (int)ResponseType.Ok) {
				if (!vep.SplitFiles && vep.EncodingSettings.OutputFile == "") {
					WarningMessage(Catalog.GetString("Please, select a video file."));
					response=vep.Run();
				} else if (vep.SplitFiles && vep.OutputDir == "") {
					WarningMessage(Catalog.GetString("Please, select an output directory."));
					response=vep.Run();
				} else {
					break;
				}
			}
			if(response ==(int)ResponseType.Ok) {
				if (!vep.SplitFiles) {
					jobs.Add(new EditionJob(playlist, vep.EncodingSettings,
					                        vep.EnableAudio, vep.TitleOverlay));
				} else {
					int i = 0;
					foreach (PlayListPlay play in playlist) {
						EncodingSettings settings = vep.EncodingSettings;
						PlayList pl = new PlayList();
						string filename = String.Format ("{0}-{1}.{2}", i.ToString("d4"), play.Name,
						                                 settings.EncodingProfile.Extension);
						
						pl.Add(play);
						settings.OutputFile = Path.Combine (vep.OutputDir, filename);
						jobs.Add(new EditionJob(pl, settings, vep.EnableAudio, vep.TitleOverlay));
						i++;
					}
				}
			}
			vep.Destroy();
			return jobs;
		}
		
		public void ExportFrameSeries(Project openedProject, Play play, string snapshotsDir) {
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;


			sd= new SnapshotsDialog();
			sd.TransientFor= mainWindow as Gtk.Window;
			sd.Play = play.Name;

			if(sd.Run() == (int)ResponseType.Ok) {
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				sd.Destroy();
				outDir = System.IO.Path.Combine(snapshotsDir, seriesName);
				var fsc = new FramesSeriesCapturer(openedProject.Description.File.FilePath,
				                               play.Start.MSeconds, play.Stop.MSeconds,
				                               interval, outDir);
				var fcpd = new FramesCaptureProgressDialog(fsc);
				fcpd.TransientFor = mainWindow as Gtk.Window;
				fcpd.Run();
				fcpd.Destroy();
			}
			else
				sd.Destroy();
		}
		
		public void TagPlay (Play play, Categories categories, TeamTemplate local, TeamTemplate visitor, bool showAllTags) {
			TaggerDialog tg = new TaggerDialog(play, categories, local, visitor, showAllTags);
			tg.TransientFor = mainWindow as Gtk.Window;
			tg.Run();
			tg.Destroy();
		}

		public void DrawingTool (Image image, Play play, int stopTime) {
			DrawingTool dialog = new DrawingTool();

			dialog.Image = image.Value;
			if (play != null)
				dialog.SetPlay(play, stopTime);
			dialog.TransientFor = mainWindow as Gtk.Window;
			image.Dispose();
			dialog.Run();
			dialog.Destroy();	
		}
		
		public ProjectDescription SelectProject(List<ProjectDescription> projects) {
			ProjectDescription project = null;
			OpenProjectDialog opd = new OpenProjectDialog();
			
			opd.Fill(projects);	
			opd.TransientFor = mainWindow as Gtk.Window;
			if(opd.Run() == (int)ResponseType.Ok)
				project = opd.SelectedProject;
			opd.Destroy();
			return project;
		}
		
		public void OpenCategoriesTemplatesManager(ITemplatesService ts)
		{
			var tManager = new TemplatesManager<Categories, Category> (ts.CategoriesTemplateProvider,
			                                                           new CategoriesTemplateEditorWidget(ts));
			tManager.TransientFor = mainWindow as Gtk.Window;
			tManager.Show();
		}

		public void OpenTeamsTemplatesManager(ITeamTemplatesProvider teamProvider)
		{
			var tManager = new TemplatesManager<TeamTemplate, Player> (teamProvider,
			                                                           new TeamTemplateEditorWidget(teamProvider));
			tManager.TransientFor = mainWindow as Gtk.Window;
			tManager.Show();
		}
		
		public void OpenProjectsManager(Project openedProject, IDatabase db, ITemplatesService ts)
		{
			Gui.Dialog.ProjectsManager pm = new Gui.Dialog.ProjectsManager(openedProject, db, ts);
			pm.TransientFor = mainWindow as Gtk.Window;
			pm.Show();
		}
		
		public void OpenPreferencesEditor()
		{
			PropertiesEditor pe = new PropertiesEditor();
			pe.TransientFor = mainWindow as Gtk.Window;
			pe.Run();
			pe.Destroy();
		}
		
		public void OpenDatabasesManager(IDataBaseManager manager)
		{
			DatabasesManager dm = new DatabasesManager (manager);
			dm.TransientFor = mainWindow as Gtk.Window;
			dm.Run();
			dm.Destroy();
		}
		
		public void ManageJobs(IRenderingJobsManager manager) {
			RenderingJobsDialog dialog = new RenderingJobsDialog(manager);
			dialog.TransientFor = mainWindow as Gtk.Window;
			dialog.Run();
			dialog.Destroy();
		}
		
		public ProjectType SelectNewProjectType () {
			ProjectSelectionDialog psd;
			int response;

			psd = new ProjectSelectionDialog();
			psd.TransientFor = mainWindow as Gtk.Window;;
			response = psd.Run();
			psd.Destroy();
			if(response != (int)ResponseType.Ok)
				return ProjectType.None;
			return psd.ProjectType;
		}
		
		public Project NewCaptureProject(IDatabase db, ITemplatesService ts,
			List<LongoMatch.Common.Device> devices, out CaptureSettings captureSettings)
		{
			return NewProject(db, null, ProjectType.CaptureProject, ts, devices, out captureSettings);
		}
		
		public Project NewURICaptureProject(IDatabase db, ITemplatesService ts,
		                                    out CaptureSettings captureSettings)
		{
			return NewProject(db, null, ProjectType.URICaptureProject, ts, null, out captureSettings);
		}
		
		public Project NewFakeProject(IDatabase db, ITemplatesService ts) {
			CaptureSettings captureSettings = new CaptureSettings();
			return NewProject(db, null, ProjectType.FakeCaptureProject, ts, null, out captureSettings);
		}
		
		public Project NewFileProject(IDatabase db, ITemplatesService ts) {
			CaptureSettings captureSettings = new CaptureSettings();
			return NewProject(db, null, ProjectType.FileProject, ts, null, out captureSettings);
		}
		
		public Project EditFakeProject(IDatabase db, Project project, ITemplatesService ts) {
			CaptureSettings captureSettings = new CaptureSettings();
			return NewProject(db, project, ProjectType.EditProject, ts, null, out captureSettings);
		}
		
		public IBusyDialog BusyDialog(string message) {
			BusyDialog dialog;

			dialog = new BusyDialog();
			dialog.TransientFor = mainWindow as Gtk.Window;
			dialog.Message = message; 
			return dialog;
		}
		
		Project NewProject(IDatabase db, Project project, ProjectType type,
			ITemplatesService tps, List<LongoMatch.Common.Device> devices, out CaptureSettings captureSettings)
		{
			NewProjectDialog npd = new NewProjectDialog();
			
			npd.TransientFor = mainWindow as Gtk.Window;
			npd.Use = type;
			npd.TemplatesService = tps;
			npd.Project = project;
			if(type == ProjectType.CaptureProject)
				npd.Devices = devices;
			int response = npd.Run();
			while(true) {
				if(response != (int)ResponseType.Ok) {
					project = null;
					break;
				} else if(npd.Project == null) {
					InfoMessage(Catalog.GetString("Please, select a video file."));
					response=npd.Run();
				} else {
					project = npd.Project;
					break;
				}
			}	
			if (type == ProjectType.CaptureProject || type == ProjectType.URICaptureProject)
				captureSettings = npd.CaptureSettings;
			else
				captureSettings = new CaptureSettings();
			npd.Destroy();
			return project;
		}
		
		public void ShowProjectStats (Project project) {
			StatsViewer dialog = new StatsViewer ();
			dialog.LoadStats (project);
			dialog.TransientFor = mainWindow as Gtk.Window;
			dialog.Run();
			dialog.Destroy();
		}
		
	}
}

