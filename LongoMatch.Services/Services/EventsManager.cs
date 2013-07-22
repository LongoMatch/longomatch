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
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Interfaces.GUI;
using LongoMatch.Store;
using Mono.Unix;
using System.IO;
using LongoMatch.Stats;

namespace LongoMatch.Services
{


	public class EventsManager
	{

		private VideoDrawingsManager drawingManager;

		/* Current play loaded. null if no play is loaded */
		TimeNode selectedTimeNode=null;
		/* current project in use */
		Project openedProject;
		ProjectType projectType;
		Time startTime;
		PlaysFilter filter;
		
		IGUIToolkit guiToolkit;
		IMainWindow mainWindow;
		IPlayer player;
		ICapturer capturer;
		IRenderingJobsManager renderer;

		public EventsManager(IGUIToolkit guiToolkit, IRenderingJobsManager renderer)
		{
			this.guiToolkit = guiToolkit;
			this.renderer = renderer;
			mainWindow = guiToolkit.MainWindow;
			player = mainWindow.Player;
			capturer = mainWindow.Capturer;
			drawingManager = new VideoDrawingsManager(player);
			ConnectSignals();
		}

		void Save (Project project) {
			if (Config.AutoSave) {
				Core.DB.UpdateProject (project);
			}
		}
		
		public void SetProject (Project project, ProjectType projectType,
		                        PlaysFilter filter)
		{
			this.filter = filter;
			this.openedProject = project;
			this.projectType = projectType;
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
			mainWindow.PlayCategoryChanged += OnPlayCategoryChanged;

			/* Connect playlist events */
			mainWindow.PlayListNodeSelectedEvent += (tn) => {selectedTimeNode = tn;};
			/* Connect tags events */
			mainWindow.TagPlayEvent += OnTagPlay;

			/* Connect SnapshotSeries events */
			mainWindow.SnapshotSeriesEvent += OnSnapshotSeries;
			
			mainWindow.ShowProjectStatsEvent += HandleShowProjectStatsEvent;
			
			/* Connect player events */
			player.Prev += OnPrev;
			player.SegmentClosedEvent += OnSegmentClosedEvent;
			player.DrawFrame += OnDrawFrame;
			player.PlaybackRateChanged += HandlePlaybackRateChanged;
		}

		void HandleShowProjectStatsEvent (Project project)
		{
			guiToolkit.ShowProjectStats (project);
		}

		void RenderPlay (Project project, Play play, MediaFile file) {
			PlayList playlist;
			EncodingSettings settings;
			EditionJob job;
			string outputDir, outputFile;
			
			if (Config.AutoRenderDir == null ||
			    !Directory.Exists (Config.AutoRenderDir)) {
				outputDir = Config.VideosDir;
			} else {
				outputDir = Config.AutoRenderDir;
			}
			
			outputFile = String.Format ("{0}-{0}.mp4", play.Category.Name, play.Name);
			outputFile = Path.Combine (outputDir, project.Description.Title, outputFile);
			try {
				Directory.CreateDirectory (Path.GetDirectoryName (outputFile));
				settings = EncodingSettings.DefaultRenderingSettings (outputFile);
				playlist = new PlayList();
				playlist.Add (new PlayListPlay (play, file, true));
			
				job = new EditionJob (playlist, settings, Config.EnableAudio, Config.OverlayTitle); 
				renderer.AddJob (job);
			} catch (Exception ex) {
				Log.Exception (ex);
			}
			
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
			   projectType == ProjectType.CaptureProject ||
			   projectType == ProjectType.URICaptureProject) {
				fStop = stop;
			} else {
				length = new Time {MSeconds = (int)player.StreamLength};
				fStop = (stop > length) ? length: stop;
			}
			AddNewPlay(fStart, fStop, category);
		}

		private void AddNewPlay(Time start, Time stop, Category category) {
			Image miniature;

			Log.Debug(String.Format("New play created start:{0} stop:{1} category:{2}",
									start, stop, category));
			/* Get the current frame and get a thumbnail from it */
			if(projectType == ProjectType.CaptureProject || projectType == ProjectType.URICaptureProject) {
				if(!capturer.Capturing) {
					guiToolkit.InfoMessage(Catalog.GetString("You can't create a new play if the capturer "+
						"is not recording."));
					return;
				}
				miniature = capturer.CurrentMiniatureFrame;
			}
			else if(projectType == ProjectType.FileProject) {
				miniature = player.CurrentMiniatureFrame;
				player.Pause();
			}
			else
				miniature = null;
			
			/* Add the new created play to the project and update the GUI*/
			var play = openedProject.AddPlay(category, start, stop,miniature);
			/* Tag subcategories of the new play */
			if (!Config.FastTagging)
				LaunchPlayTagger(play, false);
			mainWindow.AddPlay(play);
			filter.Update();
			if (projectType == ProjectType.FileProject) {
				player.Play();
			}
			Save (openedProject);
			
			if (projectType == ProjectType.CaptureProject ||
			    projectType == ProjectType.URICaptureProject) {
			    if (Config.AutoRenderPlaysInLive) {
					RenderPlay (openedProject, play, openedProject.Description.File);
				}
			}
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
			   projectType == ProjectType.CaptureProject ||
			   projectType == ProjectType.URICaptureProject) {
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
				guiToolkit.WarningMessage(Catalog.GetString("The stop time is smaller than the start time. "+
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

		private void LaunchPlayTagger(Play play, bool showAllTags) {
			guiToolkit.TagPlay(play, openedProject.Categories,
			                   openedProject.LocalTeamTemplate,
			                   openedProject.VisitorTeamTemplate,
			                   showAllTags);
		}

		void HandlePlaybackRateChanged (float rate)
		{
			if (selectedTimeNode != null) {
				selectedTimeNode.Rate = rate;
			}
		}

		protected virtual void OnPlaySelected(Play play)
		{
			Log.Debug("Play selected: " + play);
			selectedTimeNode = play;
			player.SetStartStop(play.Start.MSeconds,play.Stop.MSeconds, play.Rate);
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
			filter.Update();
		}

		protected virtual void OnPlaysDeleted(List<Play> plays)
		{
			Log.Debug(plays.Count + " plays deleted");
			mainWindow.DeletePlays(plays);
			openedProject.RemovePlays(plays);

			if(projectType == ProjectType.FileProject) {
				player.CloseActualSegment();
				Save (openedProject);
			}
			filter.Update();
		}

		protected virtual void OnSegmentClosedEvent()
		{
			selectedTimeNode = null;
		}

		protected virtual void OnSnapshotSeries(Play play) {
			player.Pause();
			guiToolkit.ExportFrameSeries(openedProject, play, Config.SnapshotsDir);
		}
		
		protected virtual void OnPrev()
		{
		}

		protected virtual void OnTimeline2PositionChanged(Time pos)
		{
			player.SeekInSegment(pos.MSeconds);
		}

		protected virtual void OnDrawFrame(int time) {
			Image pixbuf = null;
			player.Pause();
			pixbuf = player.CurrentFrame;
			guiToolkit.DrawingTool(pixbuf, selectedTimeNode as Play, time);
		}

		protected virtual void OnTagPlay(Play play) {
			LaunchPlayTagger(play, true);
		}
		
		protected virtual void OnPlayCategoryChanged(Play play, Category cat)
		{
			List<Play> plays = new List<Play>();
			plays.Add(play);
			OnPlaysDeleted(plays);
			var newplay = openedProject.AddPlay(cat, play.Start, play.Stop, play.Miniature);
			newplay.Name = play.Name;
			newplay.Notes = play.Notes;
			newplay.Drawings = play.Drawings;
			mainWindow.AddPlay(newplay);
			Save (openedProject);
		}
	}
}
