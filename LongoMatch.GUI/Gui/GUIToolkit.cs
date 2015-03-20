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
using LongoMatch.Core.Common;
using LongoMatch.Gui.Helpers;
using LongoMatch.Core.Interfaces.GUI;
using LongoMatch.Core.Store;
using LongoMatch.Core.Store.Playlists;
using LongoMatch.Video.Utils;
using Mono.Unix;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Gui.Panel;
using Image = LongoMatch.Core.Common.Image;
using LongoMatch.Core.Interfaces;

namespace LongoMatch.Gui
{
	public class GUIToolkit: IGUIToolkit
	{
		static GUIToolkit instance;
		MainWindow mainWindow;
		Registry registry;

		public GUIToolkit (Version version)
		{
			Version = version;
			mainWindow = new MainWindow (this);
			(mainWindow as MainWindow).Show ();
			instance = this;
			registry = new Registry ("GUI backend");
		}

		public static GUIToolkit Instance {
			get {
				return instance;
			}
		}

		public IMainController MainController {
			get {
				return mainWindow;
			}
		}

		public IRenderingStateBar RenderingStateBar {
			get {
				return mainWindow.RenderingStateBar;
			}
		}

		public bool FullScreen {
			set {
				if (mainWindow != null) {
					if (value)
						mainWindow.GdkWindow.Fullscreen ();
					else
						mainWindow.GdkWindow.Unfullscreen ();
				}
			}
		}

		public Version Version {
			get;
			set;
		}

		public void Register (int priority, Type interfac, Type elementType)
		{
			registry.Register (priority, interfac, elementType);
		}

		public void InfoMessage (string message, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.InfoMessage (parent as Widget, message);
		}

		public void ErrorMessage (string message, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.ErrorMessage (parent as Widget, message);
		}

		public void WarningMessage (string message, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			MessagesHelpers.WarningMessage (parent as Widget, message);
		}

		public bool QuestionMessage (string question, string title, object parent = null)
		{
			if (parent == null)
				parent = mainWindow as Widget;
			return MessagesHelpers.QuestionMessage (parent as Widget, question, title);
		}

		public string QueryMessage (string key, string title = null, string value = "", object parent = null)
		{
			if (parent == null)
				parent = mainWindow;
			return MessagesHelpers.QueryMessage (parent as Widget, key, title, value);
		}

		public string SaveFile (string title, string defaultName, string defaultFolder,
		                        string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SaveFile (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public string SelectFolder (string title, string defaultName, string defaultFolder,
		                            string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.SelectFolder (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public string OpenFile (string title, string defaultName, string defaultFolder,
		                        string filterName = null, string[] extensionFilter = null)
		{
			return FileChooserHelper.OpenFile (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public List<string> OpenFiles (string title, string defaultName, string defaultFolder,
		                               string filterName, string[] extensionFilter)
		{
			return FileChooserHelper.OpenFiles (mainWindow as Widget, title, defaultName,
				defaultFolder, filterName, extensionFilter);
		}

		public object ChooseOption (Dictionary<string, object> options, object parent = null)
		{
			object res = null;
			ChooseOptionDialog dialog = new ChooseOptionDialog ();
			dialog.Options = options;
			if (parent != null) {
				dialog.TransientFor = (parent as Widget).Toplevel as Gtk.Window;
			} else {
				dialog.TransientFor = mainWindow as Gtk.Window;
			}
			if (dialog.Run () == (int)ResponseType.Ok) {
				res = dialog.SelectedOption;
			}
			dialog.Destroy ();
			return res;
		}

		public List<EditionJob> ConfigureRenderingJob (Playlist playlist)
		{
			VideoEditionProperties vep;
			List<EditionJob> jobs = new List<EditionJob> ();
			int response;
			
			Log.Information ("Configure rendering job");
			if (playlist.Elements.Count == 0) {
				WarningMessage (Catalog.GetString ("The playlist you want to render is empty."));
				return null;
			}

			vep = new VideoEditionProperties ();
			vep.TransientFor = mainWindow as Gtk.Window;
			vep.Playlist = playlist;
			response = vep.Run ();
			while (response == (int)ResponseType.Ok) {
				if (!vep.SplitFiles && vep.EncodingSettings.OutputFile == "") {
					WarningMessage (Catalog.GetString ("Please, select a video file."));
					response = vep.Run ();
				} else if (vep.SplitFiles && vep.OutputDir == null) {
					WarningMessage (Catalog.GetString ("Please, select an output directory."));
					response = vep.Run ();
				} else {
					break;
				}
			}
			if (response == (int)ResponseType.Ok) {
				if (!vep.SplitFiles) {
					jobs.Add (new EditionJob (playlist, vep.EncodingSettings));
				} else {
					int i = 0;
					foreach (IPlaylistElement play in playlist.Elements) {
						EncodingSettings settings;
						Playlist pl;
						string name, ext, filename;

						settings = vep.EncodingSettings;
						pl = new Playlist ();
						if (play is PlaylistPlayElement) {
							name = (play as PlaylistPlayElement).Play.Name;
							ext = settings.EncodingProfile.Extension;
						} else {
							name = "image";
							ext = "png";
						}
						filename = String.Format ("{0}-{1}.{2}", i.ToString ("d4"), name, ext);
						
						pl.Elements.Add (play);
						settings.OutputFile = Path.Combine (vep.OutputDir, filename);
						jobs.Add (new EditionJob (pl, settings));
						i++;
					}
				}
			}
			vep.Destroy ();
			return jobs;
		}

		public void ExportFrameSeries (Project openedProject, TimelineEvent play, string snapshotsDir)
		{
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;

			Log.Information ("Export frame series");
			sd = new SnapshotsDialog ();
			sd.TransientFor = mainWindow as Gtk.Window;
			sd.Play = play.Name;

			if (sd.Run () == (int)ResponseType.Ok) {
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				sd.Destroy ();
				outDir = System.IO.Path.Combine (snapshotsDir, seriesName);
				var fsc = new FramesSeriesCapturer (openedProject.Description.FileSet, play, interval, outDir);
				var fcpd = new FramesCaptureProgressDialog (fsc);
				fcpd.TransientFor = mainWindow as Gtk.Window;
				fcpd.Run ();
				fcpd.Destroy ();
			} else
				sd.Destroy ();
		}

		public void EditPlay (TimelineEvent play, Project project, bool editTags, bool editPos, bool editPlayers, bool editNotes)
		{
			if (play is StatEvent) {
				SubstitutionsEditor dialog = new SubstitutionsEditor ();
				dialog.Load (project, play as StatEvent);
				if (dialog.Run () == (int)ResponseType.Ok) {
					dialog.SaveChanges ();
				}
				dialog.Destroy ();
			} else {
				PlayEditor dialog = new PlayEditor ();
				dialog.LoadPlay (play, project, editTags, editPos, editPlayers, editNotes);
				dialog.Run ();
				dialog.Destroy ();
			}
		}

		public void DrawingTool (Image image, TimelineEvent play, FrameDrawing drawing, Project project)
		{
			DrawingTool dialog = new DrawingTool ();
			dialog.Show ();

			Log.Information ("Drawing tool");
			if (play == null) {
				dialog.LoadFrame (image, project);
			} else {
				dialog.LoadPlay (play, image, drawing, project);
			}
			dialog.TransientFor = mainWindow as Gtk.Window;
			dialog.Run ();
			dialog.Destroy ();	
		}

		public ProjectDescription ChooseProject (List<ProjectDescription> projects)
		{
			Log.Information ("Choosing project");
			ProjectDescription pd = null;
			ChooseProjectDialog dialog = new ChooseProjectDialog ();
			dialog.Fill (projects);
			if (dialog.Run () == (int)ResponseType.Ok) {
				pd = dialog.Project;
			}
			dialog.Destroy ();
			return pd;
		}

		public void SelectProject (List<ProjectDescription> projects)
		{
			Log.Information ("Select project");
			mainWindow.SelectProject (projects);
		}

		public void OpenCategoriesTemplatesManager ()
		{
			SportsTemplatesPanel panel = new SportsTemplatesPanel ();
			Log.Information ("Open sports templates manager");
			mainWindow.SetPanel (panel);
		}

		public void OpenTeamsTemplatesManager ()
		{
			TeamsTemplatesPanel panel = new TeamsTemplatesPanel ();
			Log.Information ("Open teams templates manager");
			mainWindow.SetPanel (panel);
		}

		public void OpenProjectsManager (Project openedProject)
		{
			ProjectsManagerPanel panel = new ProjectsManagerPanel (openedProject);
			Log.Information ("Open projects manager");
			mainWindow.SetPanel (panel);
		}

		public void OpenPreferencesEditor ()
		{
			PreferencesPanel panel = new PreferencesPanel ();
			Log.Information ("Open preferences");
			mainWindow.SetPanel (panel);
		}

		public void OpenDatabasesManager ()
		{
			DatabasesManager dm = new DatabasesManager ();
			Log.Information ("Open db manager");
			dm.TransientFor = mainWindow as Gtk.Window;
			dm.Run ();
			dm.Destroy ();
		}

		public void ManageJobs ()
		{
			RenderingJobsDialog dialog = new RenderingJobsDialog ();
			Log.Information ("Manage jobs");
			dialog.TransientFor = mainWindow as Gtk.Window;
			dialog.Run ();
			dialog.Destroy ();
		}

		public IBusyDialog BusyDialog (string message, object parent = null)
		{
			BusyDialog dialog;

			dialog = new BusyDialog ();
			if (parent != null) {
				dialog.TransientFor = (parent as Widget).Toplevel as Gtk.Window;
			} else {
				dialog.TransientFor = mainWindow as Gtk.Window;
			}
			dialog.Message = message; 
			return dialog;
		}

		public void CreateNewProject (Project project = null)
		{
			mainWindow.CreateNewProject (project);
		}

		public void ShowProjectStats (Project project)
		{
			Log.Information ("Show project stats");
			Addins.AddinsManager.ShowStats (project);
			System.GC.Collect ();
		}

		public string RemuxFile (string inputFile, string outputFile, VideoMuxerType muxer)
		{
			Log.Information ("Remux file");
			Remuxer remuxer = new Remuxer (Config.MultimediaToolkit.DiscoverFile (inputFile),
				                  outputFile, muxer);
			return remuxer.Remux (mainWindow as Gtk.Window);
		}

		public void OpenProject (Project project, ProjectType projectType, 
		                         CaptureSettings props, EventsFilter filter,
		                         out IAnalysisWindow analysisWindow)
		{
			Log.Information ("Open project");
			analysisWindow = mainWindow.SetProject (project, projectType, props, filter);
		}

		public void CloseProject ()
		{
			Log.Information ("Close project");
			mainWindow.CloseProject ();
		}

		public DateTime SelectDate (DateTime date, object widget)
		{
			CalendarDialog dialog = new CalendarDialog (date);
			dialog.TransientFor = (widget as Widget).Toplevel as Gtk.Window;
			dialog.Run ();
			date = dialog.Date;
			dialog.Destroy ();
			return date;
		}

		public EndCaptureResponse EndCapture (string filepath)
		{
			int res;
			EndCaptureDialog dialog = new EndCaptureDialog (filepath);
			dialog.TransientFor = mainWindow.Toplevel as Gtk.Window;
			res = dialog.Run ();
			dialog.Destroy ();
			return (EndCaptureResponse)res;
		}

		public bool SelectMediaFiles (Project project)
		{
			bool ret = false;
			MediaFileSetSelection fileselector = new MediaFileSetSelection (false);
			Gtk.Dialog d = new Gtk.Dialog (Catalog.GetString ("Select video files"),
				               mainWindow.Toplevel as Gtk.Window,
				               DialogFlags.Modal | DialogFlags.DestroyWithParent,
				               Gtk.Stock.Cancel, ResponseType.Cancel,
				               Gtk.Stock.Ok, ResponseType.Ok);
			fileselector.Show ();
			fileselector.FileSet = project.Description.FileSet;
			d.VBox.Add (fileselector);
			WarningMessage (Catalog.GetString ("Some video files are missing for this project"));
			while (d.Run () == (int)ResponseType.Ok) {
				if (!fileselector.FileSet.CheckFiles ()) {
					WarningMessage (Catalog.GetString ("Some video files are still missing for this project."), d);
					continue;
				}
				if (fileselector.FileSet.Count == 0) {
					WarningMessage (Catalog.GetString ("You need at least 1 video file for the main angle"));
					continue;
				}
				ret = true;
				break;
			}
			d.Destroy ();
			return ret;
		}

		public void Quit ()
		{
			Log.Information ("Quit application");
			Gtk.Application.Quit ();
		}

		public HotKey SelectHotkey (HotKey hotkey, object parent = null)
		{
			HotKeySelectorDialog dialog;
			Window w;
			
			w = parent != null ? (parent as Widget).Toplevel as Window : mainWindow;
			dialog = new HotKeySelectorDialog ();
			dialog.TransientFor = w;
			if (dialog.Run () == (int)ResponseType.Ok) {
				hotkey = dialog.HotKey;
			} else {
				hotkey = null;
			}
			dialog.Destroy ();
			return hotkey;
		}

		public void Invoke (EventHandler handler)
		{
			Gtk.Application.Invoke (handler);
		}
	}
}

