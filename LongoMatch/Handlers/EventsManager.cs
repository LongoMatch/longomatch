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

using System.Collections.Generic;
using Gdk;
using Gtk;
using LongoMatch.Common;
using LongoMatch.Gui;
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.Handlers;
using LongoMatch.Store;
using LongoMatch.Video.Common;
using LongoMatch.Video.Editor;
using LongoMatch.Video.Utils;
using Mono.Unix;

namespace LongoMatch
{


	public class EventsManager
	{

		private PlaysListTreeWidget treewidget;
		private PlayersListTreeWidget localPlayersList,visitorPlayersList;
		private TagsTreeWidget tagsTreeWidget;
		private ButtonsWidget buttonswidget;
		private PlayListWidget playlist;
		private PlayerBin player;
		private CapturerBin capturer;
		private TimeLineWidget timeline;
		private ProgressBar videoprogressbar;
		private NotesWidget notes;
		private FramesSeriesCapturer fsc;
		private FramesCaptureProgressDialog fcpd;
		private VideoDrawingsManager drawingManager;

		// Current play loaded. null if no play is loaded
		private TimeNode selectedTimeNode=null;
		// current proyect in use
		private Project openedProject;
		private ProjectType projectType;
		private Time startTime;

		public EventsManager(PlaysListTreeWidget treewidget, PlayersListTreeWidget localPlayersList, 
		                     PlayersListTreeWidget visitorPlayersList, TagsTreeWidget tagsTreeWidget,
		                     ButtonsWidget buttonswidget, PlayListWidget playlist, PlayerBin player, 
		                     TimeLineWidget timeline, ProgressBar videoprogressbar,NotesWidget notes,
		                     CapturerBin capturer)
		{
			this.treewidget = treewidget;
			this.localPlayersList = localPlayersList;
			this.visitorPlayersList = visitorPlayersList;
			this.tagsTreeWidget = tagsTreeWidget;
			this.buttonswidget = buttonswidget;
			this.playlist = playlist;
			this.player = player;
			this.timeline = timeline;
			this.videoprogressbar = videoprogressbar;
			this.notes = notes;
			this.capturer = capturer;
			this.drawingManager = new VideoDrawingsManager(player);

			ConnectSignals();
		}

		public  Project OpenedProject {
			set {
				openedProject = value;
			}
		}
		
		public ProjectType OpenedProjectType{
			set {
				projectType = value;
			}
		}

		private void ConnectSignals() {
			/* Adding Handlers for each event */

			/* Connect new mark event */
			buttonswidget.NewMarkEvent += OnNewMark;
			buttonswidget.NewMarkStartEvent += OnNewMarkStart;
			buttonswidget.NewMarkStopEvent += OnNewMarkStop;
			
			/* Connect TimeNodeChanged events */
			treewidget.TimeNodeChanged += OnTimeNodeChanged;
			localPlayersList.TimeNodeChanged += OnTimeNodeChanged;
			visitorPlayersList.TimeNodeChanged += OnTimeNodeChanged;
			tagsTreeWidget.TimeNodeChanged += OnTimeNodeChanged;
			timeline.TimeNodeChanged += OnTimeNodeChanged;
			notes.TimeNodeChanged += OnTimeNodeChanged;

			/* Connect TimeNodeDeleted events */
			treewidget.TimeNodeDeleted += OnTimeNodeDeleted;
			timeline.TimeNodeDeleted += OnTimeNodeDeleted;

			/* Connect TimeNodeSelected events */
			treewidget.TimeNodeSelected += OnTimeNodeSelected;
			localPlayersList.TimeNodeSelected += OnTimeNodeSelected;
			visitorPlayersList.TimeNodeSelected += OnTimeNodeSelected;
			tagsTreeWidget.TimeNodeSelected += OnTimeNodeSelected;
			timeline.TimeNodeSelected += OnTimeNodeSelected;

			/* Connect playlist events */
			playlist.PlayListNodeSelected += OnPlayListNodeSelected;
			playlist.Progress += OnProgress;
			playlist.ApplyCurrentRate += OnApplyRate;

			/* Connect PlayListNodeAdded events */
			treewidget.PlayListNodeAdded += OnPlayListNodeAdded;
			localPlayersList.PlayListNodeAdded += OnPlayListNodeAdded;
			visitorPlayersList.PlayListNodeAdded += OnPlayListNodeAdded;
			tagsTreeWidget.PlayListNodeAdded += OnPlayListNodeAdded;

			/* Connect tags events */
			treewidget.PlayersTagged += OnPlayersTagged;
			treewidget.TagPlay += OnTagPlay;
			
			/* Connect SnapshotSeries events */
			treewidget.SnapshotSeriesEvent += OnSnapshotSeries;
			localPlayersList.SnapshotSeriesEvent += OnSnapshotSeries;
			visitorPlayersList.SnapshotSeriesEvent += OnSnapshotSeries;
			tagsTreeWidget.SnapshotSeriesEvent += OnSnapshotSeries;

			/* Connect timeline events */
			timeline.NewMarkEvent += OnNewMarkAtFrame;
			
			/* Connect player events */
			player.Prev += OnPrev;
			player.Next += OnNext;
			player.Tick += OnTick;
			player.SegmentClosedEvent += OnSegmentClosedEvent;
			player.DrawFrame += OnDrawFrame;
		}

		private void ProcessNewMarkEvent(Category category,Time pos) {
			Time length, startTime, stopTime, start, stop, fStart, fStop;	
			
			if (player == null || openedProject == null)
				return;
						
			//Get the default lead and lag time for the section
			startTime = category.Start;
			stopTime = category.Stop;
			// Calculating borders of the segment depnding
			start = pos - startTime;
			stop = pos + stopTime;
			fStart = (start < new Time {MSeconds =0}) ? new Time {MSeconds = 0} : start;
			
			if (projectType == ProjectType.FakeCaptureProject || 
			    projectType == ProjectType.CaptureProject){
				fStop = stop;					
			}
			else {
				length = new Time {MSeconds = (int)player.StreamLength};
				fStop = (stop > length) ? length: stop;
			}	
			AddNewPlay(fStart, fStop, category);
		}
		
		private void AddNewPlay(Time start, Time stop, Category category){
			Pixbuf miniature;
		
			if (projectType == ProjectType.CaptureProject){
				if (!capturer.Capturing){
					MessagePopup.PopupMessage(capturer, MessageType.Info,
					                          Catalog.GetString("You can't create a new play if the capturer "+
					                                            "is not recording."));
					return;
				}
				miniature = capturer.CurrentMiniatureFrame;
			}
			else if (projectType == ProjectType.FileProject)
				miniature = player.CurrentMiniatureFrame;
			else 
				miniature = null;
			var play = openedProject.AddPlay(category, start, stop,miniature);
			treewidget.AddPlay(play);
			tagsTreeWidget.AddPlay(play);
			timeline.AddPlay(play);
			timeline.QueueDraw();
		}

		protected virtual void OnProgress(float progress) {

			if (progress > (float)EditorState.START && progress <= (float)EditorState.FINISHED && progress > videoprogressbar.Fraction) {
				videoprogressbar.Fraction = progress;
			}

			if (progress == (float)EditorState.CANCELED) {
				videoprogressbar.Hide();
			}

			else if (progress == (float)EditorState.START) {
				videoprogressbar.Show();
				videoprogressbar.Fraction = 0;
				videoprogressbar.Text = "Creating new video";
			}

			else if (progress == (float)EditorState.FINISHED) {
				MessagePopup.PopupMessage(player, MessageType.Info,  Catalog.GetString("The video edition has finished successfully."));
				videoprogressbar.Hide();
			}

			else if (progress == (float)EditorState.ERROR) {
				MessagePopup.PopupMessage(player, MessageType.Error,
				                          Catalog.GetString("An error has occurred in the video editor.")
				                          +Catalog.GetString("Please, try again."));
				videoprogressbar.Hide();
			}
		}

		protected virtual void OnNewMarkAtFrame(Category category, int frame) {
			Time pos = new Time{ MSeconds = frame*1000/openedProject.Description.File.Fps};
			player.CloseActualSegment();
			player.SeekTo ((long)pos.MSeconds, true);
			ProcessNewMarkEvent(category,pos);
		}

		public virtual void OnNewMark(Category category) {
			Time pos;
			
			if (projectType == ProjectType.FakeCaptureProject || 
			    projectType == ProjectType.CaptureProject)
				pos =  new Time { MSeconds = (int)capturer.CurrentTime};
			else 
				pos = new Time {MSeconds = (int)player.CurrentTime};
			ProcessNewMarkEvent(category,pos);
		}
		
		public virtual void OnNewMarkStart(){
			startTime = new Time {MSeconds = (int)player.CurrentTime};
		}
		
		public virtual void OnNewMarkStop(Category category){
			int diff;
			Time stopTime = new Time {MSeconds = (int)player.CurrentTime};
			
			diff = stopTime.MSeconds - startTime.MSeconds;
			
			if (diff < 0){
				MessagePopup.PopupMessage(buttonswidget, MessageType.Warning,
				                          Catalog.GetString("The stop time is smaller than the start time. "+
				                                            "The play will not be added."));
				return;
			}
			if (diff < 500){
				int correction = 500 - diff;
				if (startTime.MSeconds - correction > 0)
					startTime = startTime - correction;
				else 
					stopTime = stopTime + correction;			
			} 
			AddNewPlay(startTime, stopTime, category);		
		}

		protected virtual void OnTimeNodeSelected(Play tNode)
		{
			selectedTimeNode = tNode;
			timeline.SelectedTimeNode = tNode;
			player.SetStartStop(tNode.Start.MSeconds,tNode.Stop.MSeconds);
			notes.Visible = true;
			notes.Play= tNode;
			drawingManager.Play=tNode;
		}

		protected virtual void OnTimeNodeChanged(TimeNode tNode, object val)
		{
			//Si hemos modificado el valor de un nodo de tiempo a través del
			//widget de ajuste de tiempo posicionamos el reproductor en el punto
			//
			if (tNode is Play && val is Time) {
				if (tNode != selectedTimeNode)
					OnTimeNodeSelected((Play)tNode);
				Time pos = (Time)val;
				if (pos == tNode.Start) {
					player.UpdateSegmentStartTime(pos.MSeconds);
				}
				else {
					player.UpdateSegmentStopTime(pos.MSeconds);
				}
			}
			else if (tNode is Category) {
				buttonswidget.Categories = openedProject.Categories;
			}
		}

		protected virtual void OnTimeNodeDeleted(List<Play> plays)
		{
			treewidget.RemovePlays(plays);
			timeline.RemovePlays(plays);
			tagsTreeWidget.RemovePlays(plays);

			localPlayersList.RemovePlays(plays);
			visitorPlayersList.RemovePlays(plays);

			openedProject.RemovePlays(plays);
			if (projectType == ProjectType.FileProject){
				this.player.CloseActualSegment();
				MainClass.DB.UpdateProject(openedProject);
			}
			timeline.QueueDraw();			
		}

		protected virtual void OnPlayListNodeAdded(Play play)
		{
			playlist.Add(new PlayListPlay{
				MediaFile = openedProject.Description.File,
				Start = play.Start,
				Stop = play.Stop,
				Name = play.Name,
				Rate = 1.0f,
			});
		}

		protected virtual void OnPlayListNodeSelected(PlayListPlay plNode, bool hasNext)
		{
			if (openedProject == null) {
				if (plNode.Valid) {
					player.SetPlayListElement(plNode.MediaFile.FilePath,plNode.Start.MSeconds,plNode.Stop.MSeconds,plNode.Rate,hasNext);
					selectedTimeNode = plNode;
				}
			}
			else {
				MessagePopup.PopupMessage(playlist, MessageType.Error,
				                          Catalog.GetString("Please, close the opened project to play the playlist."));
				playlist.Stop();
			}
		}

		protected virtual void OnPlayListSegmentDone()
		{
			playlist.Next();
		}

		protected virtual void OnSegmentClosedEvent()
		{
			selectedTimeNode = null;
			timeline.SelectedTimeNode = null;
			notes.Visible = false;
		}

		protected virtual void OnSnapshotSeries(Play tNode) {
			SnapshotsDialog sd;
			uint interval;
			string seriesName;
			string outDir;

			player.Pause();

			sd= new SnapshotsDialog();
			sd.TransientFor= (Gtk.Window) treewidget.Toplevel;
			sd.Play = tNode.Name;

			if (sd.Run() == (int)ResponseType.Ok) {
				sd.Destroy();
				interval = sd.Interval;
				seriesName = sd.SeriesName;
				outDir = System.IO.Path.Combine(MainClass.SnapshotsDir(),seriesName);
				fsc = new FramesSeriesCapturer(openedProject.Description.File.FilePath,
				                               tNode.Start.MSeconds,tNode.Stop.MSeconds,
				                               interval,outDir);
				fcpd = new FramesCaptureProgressDialog(fsc);
				fcpd.TransientFor=(Gtk.Window) treewidget.Toplevel;
				fcpd.Run();
				fcpd.Destroy();
			}
			else
				sd.Destroy();
		}

		protected virtual void OnNext()
		{
			playlist.Next();
		}

		protected virtual void OnPrev()
		{
			if (selectedTimeNode is Play)
				player.SeekInSegment(selectedTimeNode.Start.MSeconds);
			else if (selectedTimeNode is PlayListPlay)
				playlist.Prev();
			else if (selectedTimeNode == null)
				player.SeekTo(0,false);
		}

		protected virtual void OnTick(object o, TickArgs args)
		{
			if (args.CurrentTime != 0 && timeline != null && openedProject != null)
				timeline.CurrentFrame=(uint)(args.CurrentTime * 
				                             openedProject.Description.File.Fps / 1000);
		}

		protected virtual void OnTimeline2PositionChanged(Time pos)
		{
			player.SeekInSegment(pos.MSeconds);
		}

		protected virtual void OnApplyRate(PlayListPlay plNode) {
			plNode.Rate = player.Rate;
		}

		protected virtual void OnDrawFrame(int time) {
			Pixbuf pixbuf=null;
			DrawingTool dialog = new DrawingTool();

			player.Pause();
			pixbuf = player.CurrentFrame;

			dialog.Image = pixbuf;
			dialog.TransientFor = (Gtk.Window)player.Toplevel;
			if (selectedTimeNode != null)
				dialog.SetPlay((selectedTimeNode as Play),
				               time);
			pixbuf.Dispose();
			dialog.Run();
			dialog.Destroy();
		}
		
		protected virtual void OnTagPlay(Play tNode){
			/*TaggerDialog tagger = new TaggerDialog();
			tagger.ProjectTags = openedProject.Tags;
			tagger.Tags = tNode.Tags;
			tagger.TransientFor = (Gtk.Window)player.Toplevel;
			tagger.Run();
			tNode.Tags = tagger.Tags;
			foreach (Tag tag in tagger.Tags){
				openedProject.Tags.AddTag(tag);
			}
			tagsTreeWidget.UpdateTagsList();
			tagger.Destroy();*/
		}

		protected virtual void OnPlayersTagged(Play tNode, Team team) {
			PlayersSelectionDialog dialog = new PlayersSelectionDialog();
			if (team == Team.LOCAL) {
				dialog.SetPlayersInfo(openedProject.LocalTeamTemplate);
				dialog.PlayersChecked = tNode.LocalPlayers;
				if (dialog.Run() == (int) ResponseType.Ok) {
					tNode.LocalPlayers = dialog.PlayersChecked;
					localPlayersList.UpdatePlaysList(openedProject.GetLocalTeamModel());
				}
			}

			else if (team == Team.VISITOR) {
				dialog.SetPlayersInfo(openedProject.VisitorTeamTemplate);
				dialog.PlayersChecked = tNode.VisitorPlayers;
				if (dialog.Run() == (int) ResponseType.Ok) {
					tNode.VisitorPlayers = dialog.PlayersChecked;
					visitorPlayersList.UpdatePlaysList(openedProject.GetVisitorTeamModel());
				}
			}
			dialog.Destroy();
		}
	}
}
