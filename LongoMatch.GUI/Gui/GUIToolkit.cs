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
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Gtk;
using LongoMatch.Core.Store;
using LongoMatch.Drawing;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Services.State;
using VAS.Core;
using VAS.Core.Common;
using VAS.Core.Events;
using VAS.Core.Interfaces.GUI;
using VAS.Core.MVVMC;
using VAS.Core.Store;
using VAS.Core.ViewModel;
using VAS.Drawing;
using VAS.UI;
using VAS.Video.Utils;

namespace LongoMatch.Gui
{
	public sealed class GUIToolkit : GUIToolkitBase, INavigation
	{
		static readonly GUIToolkit instance = new GUIToolkit ();

		public static GUIToolkit Instance {
			get {
				return instance;
			}
		}

		new MainWindow MainWindow {
			get {
				return base.MainWindow as MainWindow;
			}
			set {
				base.MainWindow = value;
			}
		}

		private GUIToolkit ()
		{
			MainWindow = new MainWindow (this);
			MainWindow.Hide ();
			Scanner.ScanViews (App.Current.ViewLocator);
			DrawingInit.ScanViews ();
			LMDrawingInit.ScanViews ();
		}

		public override IMainController MainController {
			get {
				return MainWindow;
			}
		}

		public Task<bool> Push (IPanel panel)
		{
			bool result = MainWindow.SetPanel (panel);
			return AsyncHelpers.Return (result);
		}

		public Task<bool> Pop (IPanel panel)
		{
			// In Gtk+ poping a panel is equivalent to replacing the current panel with the previous panel
			// in the stack
			return Push (panel);
		}

		public Task PushModal (IPanel panel, IPanel parent)
		{
			ShowModalWindow (panel, parent);
			return AsyncHelpers.Return ();
		}

		public Task PopModal (IPanel panel)
		{
			RemoveModalPanelAndWindow (panel);
			return AsyncHelpers.Return ();
		}

		public override void ExportFrameSeries (Project openedProject, TimelineEvent play, string snapshotsDir)
		{
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;

			Log.Information ("Export frame series");
			sd = new SnapshotsDialog (MainWindow as Gtk.Window);
			sd.Play = play.Name;

			if (sd.Run () == (int)ResponseType.Ok) {
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				sd.Destroy ();
				outDir = System.IO.Path.Combine (snapshotsDir, seriesName);
				var fsc = new FramesSeriesCapturer (((LMProject)openedProject).Description.FileSet, play,
							  interval, outDir);
				var fcpd = new FramesCaptureProgressDialog (fsc, MainWindow as Gtk.Window);
				fcpd.Run ();
				fcpd.Destroy ();
			} else
				sd.Destroy ();
		}

		public override Task EditPlay (TimelineEvent play, Project project, bool editTags, bool editPos, bool editPlayers,
									   bool editNotes)
		{
			if (play is StatEvent) {
				SubstitutionsEditor dialog = new SubstitutionsEditor (MainWindow as Gtk.Window);
				dialog.Load (project as LMProject, play as StatEvent);
				if (dialog.Run () == (int)ResponseType.Ok) {
					dialog.SaveChanges ();
				}
				dialog.Destroy ();
			} else {
				PlayEditor dialog = new PlayEditor (MainWindow as Gtk.Window);
				dialog.LoadPlay (play as LMTimelineEvent, project as LMProject, editTags, editPos,
					editPlayers, editNotes);
				dialog.Run ();
				dialog.Destroy ();
			}
			return Task.Factory.StartNew (() => {
			});
		}

		public override Project ChooseProject (List<Project> projects)
		{
			Log.Information ("Choosing project");
			LMProject project = null;
			ChooseProjectDialog dialog = new ChooseProjectDialog (MainWindow);
			dialog.Fill (projects.OfType<LMProject> ().ToList ());
			if (dialog.Run () == (int)ResponseType.Ok) {
				project = dialog.Project;
			}
			dialog.Destroy ();
			return project;
		}

		public override void OpenDatabasesManager ()
		{
			DatabasesManager dm = new DatabasesManager (MainWindow);
			Log.Information ("Open db manager");
			dm.Run ();
			dm.Destroy ();
		}

		public override void LoadPanel (IPanel panel)
		{
			MainWindow.SetPanel (panel);
		}

		public override void ShowProjectStats (Project project)
		{
			Log.Information ("Show project stats");
			Addins.AddinsManager.ShowStats (project as LMProject);
			System.GC.Collect ();
		}

		public override string RemuxFile (string inputFile, string outputFile, VideoMuxerType muxer)
		{
			Log.Information ("Remux file");
			try {
				Remuxer remuxer = new Remuxer (App.Current.MultimediaToolkit.DiscoverFile (inputFile),
									  outputFile, muxer);
				return remuxer.Remux (MainWindow as Gtk.Window);
			} catch (Exception e) {
				Log.Exception (e);
				return null;
			}
		}

		public override void OpenProject (ProjectVM project, CaptureSettings props)
		{
			Log.Information ($"Open project {project.ProjectType}");
			dynamic settings = new ExpandoObject ();
			settings.Project = project;
			settings.CaptureSettings = props;

			if (project.ProjectType == ProjectType.FileProject || project.ProjectType == ProjectType.EditProject) {
				App.Current.StateController.MoveTo (ProjectAnalysisState.NAME, settings, true);
			} else if (project.ProjectType == ProjectType.FakeCaptureProject) {
				App.Current.StateController.MoveTo (FakeLiveProjectAnalysisState.NAME, settings, true);
			} else {
				App.Current.StateController.MoveTo (LiveProjectAnalysisState.NAME, settings, true);
			}
		}

		public override EndCaptureResponse EndCapture (bool isCapturing)
		{
			int res;
			EndCaptureDialog dialog = new EndCaptureDialog (MainWindow, isCapturing);
			res = dialog.Run ();
			dialog.Destroy ();
			return (EndCaptureResponse)res;
		}

		public override bool SelectMediaFiles (MediaFileSet fileSet)
		{
			bool ret = false;
			MediaFileSetSelection fileselector = new MediaFileSetSelection (false);
			Gtk.Dialog d = new Gtk.Dialog (Catalog.GetString ("Select video files"),
							   MainWindow.Toplevel as Gtk.Window,
							   DialogFlags.Modal | DialogFlags.DestroyWithParent,
							   Gtk.Stock.Cancel, ResponseType.Cancel,
							   Gtk.Stock.Ok, ResponseType.Ok);
			fileselector.Show ();
			fileselector.FileSet = fileSet.Clone ();
			d.VBox.Add (fileselector);
			App.Current.Dialogs.WarningMessage (Catalog.GetString ("Some video files are missing for this project"));
			while (d.Run () == (int)ResponseType.Ok) {
				if (!fileselector.FileSet.CheckFiles ()) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("Some video files are still missing for this project."), d);
					continue;
				}
				if (fileselector.FileSet.Count == 0) {
					App.Current.Dialogs.WarningMessage (Catalog.GetString ("You need at least 1 video file for the main angle"));
					continue;
				}
				ret = true;
				break;
			}
			if (ret) {
				// We need to update the fileset as it might have changed. Indeed if multi camera is not supported
				// widget will propose only one media file selector and will return a smaller fileset than the 
				// one provided originally.
				fileSet.Clear ();
				for (int i = 0; i < fileselector.FileSet.Count; i++) {
					fileSet.Add (fileselector.FileSet [i]);
				}
			}
			d.Destroy ();
			return ret;
		}

		public override HotKey SelectHotkey (HotKey hotkey, object parent = null)
		{
			HotKeySelectorDialog dialog;
			Window w;

			w = parent != null ? (parent as Widget).Toplevel as Window : MainWindow;
			dialog = new HotKeySelectorDialog (w);
			if (dialog.Run () == (int)ResponseType.Ok) {
				hotkey = dialog.HotKey;
			} else {
				hotkey = null;
			}
			dialog.Destroy ();
			return hotkey;
		}

		public override Task<bool> CreateNewTemplate<T> (IList<T> availableTemplates, string defaultName,
														 string countText, string emptyText,
														 CreateEvent<T> evt)
		{
			bool ret = false;
			EntryDialog dialog = new EntryDialog (MainWindow as Gtk.Window);
			dialog.ShowCount = true;
			dialog.Title = dialog.Text = Catalog.GetString (defaultName);
			dialog.SelectText ();
			dialog.CountText = Catalog.GetString (countText);
			dialog.AvailableTemplates = availableTemplates.Select (t => t.Name).ToList ();

			while (dialog.Run () == (int)ResponseType.Ok) {
				if (dialog.Text == "") {
					App.Current.Dialogs.ErrorMessage (Catalog.GetString (emptyText), dialog);
					continue;
				} else {
					evt.Name = dialog.Text;
					evt.Count = dialog.Count;
					evt.Source = availableTemplates.FirstOrDefault (t => t.Name == dialog.SelectedTemplate);
					ret = true;
					break;
				}
			}
			dialog.Destroy ();
			return Task.Factory.StartNew (() => ret);
		}
	}
}

