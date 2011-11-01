// EventsManager.cs
//
//  Copyright (C2007-2009 Andoni Morales Alastruey
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
using Gdk;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Gui;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Store;
using LongoMatch.Video.Common;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Utils;
using LongoMatch.Multimedia.Interfaces;
using Mono.Unix;

namespace LongoMatch.Services
{


	public class EventsManager
	{

		private FramesSeriesCapturer fsc;
		private FramesCaptureProgressDialog fcpd;
		private VideoDrawingsManager drawingManager;

		/* Current play loaded. null if no play is loaded */
		TimeNode selectedTimeNode=null;
		/* current project in use */
		Project openedProject;
		ProjectType projectType;
		Time startTime;
		
		MainWindow mainWindow;
		PlayerBin player;
		CapturerBin capturer;

		public EventsManager(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			this.player = mainWindow.Player;
			this.capturer = mainWindow.Capturer;
			this.drawingManager = new VideoDrawingsManager(player);
			ConnectSignals();
		}

		public  Project OpenedProject {
			set {
				openedProject = value;
			}
		}

		public ProjectType OpenedProjectType {
			set {
				projectType = value;
			}
		}

		private void ConnectSignals() {
			/* Adding Handlers for each event */

			/* Connect tagging related events */
			mainWindow.NewTagEvent += OnNewTag;
			mainWindow.NewTagStartEvent += OnNewPlayStart;
			mainWindow.NewTagStopEvent += OnNewPlayStop;
			mainWindow.NewTagAtFrameEvent += OnNewTagAtFrame;
			mainWindow.TimeNodeChanged += OnTimeNodeChanged;
			mainWindow.PlaysDeletedEvent += OnPlaysDeleted;
			mainWindow.PlaySelectedEvent += OnPlaySelected;

			/* Connect playlist events */
			mainWindow.PlayListNodeSelectedEvent += (tn) => {selectedTimeNode = tn;};
			/* Connect tags events */
			mainWindow.TagPlayEvent += OnTagPlay;

			/* Connect SnapshotSeries events */
			mainWindow.SnapshotSeriesEvent += OnSnapshotSeries;
			
			/* Connect player events */
			player.Prev += OnPrev;
			player.SegmentClosedEvent += OnSegmentClosedEvent;
			player.DrawFrame += OnDrawFrame;
		}

		private void ProcessNewTag(Category category,Time pos) {
			Time length, startTime, stopTime, start, stop, fStart, fStop;

			if(player == null || openedProject == null)
				return;

			/* Get the default lead and lag time for the category */
			startTime = category.Start;
			stopTime = category.Stop;
			/* Calculate boundaries of the segment */
			start = pos - startTime;
			stop = pos + stopTime;
			fStart = (start < new Time {MSeconds =0}) ? new Time {MSeconds = 0} : start;

			if(projectType == ProjectType.FakeCaptureProject ||
			   projectType == ProjectType.CaptureProject) {
				fStop = stop;
			} else {
				length = new Time {MSeconds = (int)player.StreamLength};
				fStop = (stop > length) ? length: stop;
			}
			AddNewPlay(fStart, fStop, category);
		}

		private void AddNewPlay(Time start, Time stop, Category category) {
			Pixbuf miniature;

			Log.Debug(String.Format("New play created start:{0} stop:{1} category:{2}",
									start, stop, category));
			/* Get the current frame and get a thumbnail from it */
			if(projectType == ProjectType.CaptureProject) {
				if(!capturer.Capturing) {
					MessagePopup.PopupMessage(capturer, MessageType.Info,
					                          Catalog.GetString("You can't create a new play if the capturer "+
					                                            "is not recording."));
					return;
				}
				miniature = capturer.CurrentMiniatureFrame;
			}
			else if(projectType == ProjectType.FileProject)
				miniature = player.CurrentMiniatureFrame;
			else
				miniature = null;
			
			/* Add the new created play to the project and update the GUI*/
			var play = openedProject.AddPlay(category, start, stop,miniature);
			mainWindow.AddPlay(play);
			/* Tag subcategories of the new play */
			LaunchPlayTagger(play);
		}

		protected virtual void OnNewTagAtFrame(Category category, int frame) {
			Time pos = new Time { MSeconds = frame*1000/openedProject.Description.File.Fps};
			player.CloseActualSegment();
			player.SeekTo((long)pos.MSeconds, true);
			ProcessNewTag(category,pos);
		}

		public virtual void OnNewTag(Category category) {
			Time pos;

			if(projectType == ProjectType.FakeCaptureProject ||
			   projectType == ProjectType.CaptureProject) {
				pos =  new Time { MSeconds = (int)capturer.CurrentTime};
			} else {
				pos = new Time {MSeconds = (int)player.CurrentTime};
			}
			ProcessNewTag(category,pos);
		}

		public virtual void OnNewPlayStart() {
			startTime = new Time {MSeconds = (int)player.CurrentTime};
			Log.Debug("New play start time: " + startTime);
		}

		public virtual void OnNewPlayStop(Category category) {
			int diff;
			Time stopTime = new Time {MSeconds = (int)player.CurrentTime};

			Log.Debug("New play stop time: " + stopTime);
			diff = stopTime.MSeconds - startTime.MSeconds;

			if(diff < 0) {
				MessagePopup.PopupMessage(mainWindow, MessageType.Warning,
				                          Catalog.GetString("The stop time is smaller than the start time. "+
				                                            "The play will not be added."));
				return;
			}
			if(diff < 500) {
				int correction = 500 - diff;
				if(startTime.MSeconds - correction > 0)
					startTime = startTime - correction;
				else
					stopTime = stopTime + correction;
			}
			AddNewPlay(startTime, stopTime, category);
		}

		private void LaunchPlayTagger(Play play) {
			TaggerDialog tg = new TaggerDialog(play.Category, play.Tags, play.Players, play.Teams,
			                                   openedProject.LocalTeamTemplate, openedProject.VisitorTeamTemplate);
			tg.TransientFor = mainWindow as Gtk.Window;
			tg.Run();
			tg.Destroy();
		}

		protected virtual void OnPlaySelected(Play play)
		{
			Log.Debug("Play selected: " + play);
			selectedTimeNode = play;
			player.SetStartStop(play.Start.MSeconds,play.Stop.MSeconds);
			drawingManager.Play=play;
			mainWindow.UpdateSelectedPlay(play);
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode, object val)
		{
			/* FIXME: Tricky, create a new handler for categories */
			if(tNode is Play && val is Time) {
				if(tNode != selectedTimeNode)
					OnPlaySelected((Play)tNode);
				Time pos = (Time)val;
				if(pos == tNode.Start) {
					player.UpdateSegmentStartTime(pos.MSeconds);
				}
				else {
					player.UpdateSegmentStopTime(pos.MSeconds);
				}
			}
			else if(tNode is Category) {
				mainWindow.UpdateCategories(openedProject.Categories);
			}
		}

		protected virtual void OnPlaysDeleted(List<Play> plays)
		{
			Log.Debug(plays.Count + " plays deleted");
			mainWindow.DeletePlays(plays);
			openedProject.RemovePlays(plays);

			if(projectType == ProjectType.FileProject) {
				player.CloseActualSegment();
				Core.DB.UpdateProject(openedProject);
			}
		}

		protected virtual void OnSegmentClosedEvent()
		{
			selectedTimeNode = null;
		}

		protected virtual void OnSnapshotSeries(Play tNode) {
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;

			player.Pause();

			sd= new SnapshotsDialog();
			sd.TransientFor= mainWindow as Gtk.Window;
			sd.Play = tNode.Name;

			if(sd.Run() == (int)ResponseType.Ok) {
				sd.Destroy();
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				outDir = System.IO.Path.Combine(Config.SnapshotsDir(),seriesName);
				fsc = new FramesSeriesCapturer(openedProject.Description.File.FilePath,
				                               tNode.Start.MSeconds,tNode.Stop.MSeconds,
				                               interval,outDir);
				fcpd = new FramesCaptureProgressDialog(fsc);
				fcpd.TransientFor = mainWindow as Gtk.Window;
				fcpd.Run();
				fcpd.Destroy();
			}
			else
				sd.Destroy();
		}
		
		protected virtual void OnPrev()
		{
			if(selectedTimeNode is Play)
				player.SeekInSegment(selectedTimeNode.Start.MSeconds);
			else if(selectedTimeNode == null)
				player.SeekTo(0,false);
		}

		protected virtual void OnTimeline2PositionChanged(Time pos)
		{
			player.SeekInSegment(pos.MSeconds);
		}

		protected virtual void OnDrawFrame(int time) {
			Pixbuf pixbuf=null;
			DrawingTool dialog = new DrawingTool();

			player.Pause();
			pixbuf = player.CurrentFrame;

			dialog.Image = pixbuf;
			dialog.TransientFor = (Gtk.Window)player.Toplevel;
			if(selectedTimeNode != null)
				dialog.SetPlay((selectedTimeNode as Play),
				               time);
			pixbuf.Dispose();
			dialog.Run();
			dialog.Destroy();
		}

		protected virtual void OnTagPlay(Play play) {
			LaunchPlayTagger(play);
		}
	}
}
