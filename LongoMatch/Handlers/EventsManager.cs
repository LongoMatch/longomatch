// EventsManager.cs
//
//  Copyright (C) 2008 Andoni Morales Alastruey
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
using LongoMatch.Gui.Component;
using LongoMatch.Gui.Dialog;
using LongoMatch.TimeNodes;
using LongoMatch.DB;
using LongoMatch.Video.Player;
using LongoMatch.Video.Handlers;
using LongoMatch.Video.Utils;
using LongoMatch.Video.Editor;
using LongoMatch.Handlers;
using LongoMatch.Gui;
using Gtk;
using Gdk;
using Mono.Unix;

namespace LongoMatch
{
	
	
	public class EventsManager 
	{

		private TreeWidget treewidget;
		private PlayersListTreeWidget localPlayersList,visitorPlayersList;
		private ButtonsWidget buttonswidget;
		private PlayListWidget playlist;
		private PlayerBin player;
		private TimeLineWidget timeline;
		private ProgressBar videoprogressbar;
		private NotesWidget notes;
		private FramesSeriesCapturer fsc;
		private FramesCaptureProgressDialog fcpd;
		
		// Current play loaded. null if no play is loaded
		private TimeNode selectedTimeNode=null;
		// current proyect in use
		private Project openedProject;
		
		public EventsManager(TreeWidget treewidget, PlayersListTreeWidget localPlayersList, PlayersListTreeWidget visitorPlayersList,
		                     ButtonsWidget buttonswidget,PlayListWidget playlist, PlayerBin player,
		                     TimeLineWidget timeline, ProgressBar videoprogressbar,NotesWidget notes)
		{
			this.treewidget = treewidget;
			this.localPlayersList = localPlayersList;
			this.visitorPlayersList = visitorPlayersList;
			this.buttonswidget = buttonswidget;
			this.playlist = playlist;
			this.player = player;
			this.timeline = timeline;	
			this.videoprogressbar = videoprogressbar;
			this.notes = notes;
			
			ConnectSignals();
		}
		
		public  Project OpenedProject{
			set{
				openedProject = value;
			}
		}
		
		private void ConnectSignals(){
			//Adding Handlers for each event		
			
			buttonswidget.NewMarkEvent += new Handlers.NewMarkEventHandler(OnNewMark);
			
			treewidget.TimeNodeChanged += new Handlers.TimeNodeChangedHandler(OnTimeNodeChanged);
			localPlayersList.TimeNodeChanged += new Handlers.TimeNodeChangedHandler(OnTimeNodeChanged);
			visitorPlayersList.TimeNodeChanged += new Handlers.TimeNodeChangedHandler(OnTimeNodeChanged);
			timeline.TimeNodeChanged += new Handlers.TimeNodeChangedHandler(OnTimeNodeChanged);
			notes.TimeNodeChanged += new TimeNodeChangedHandler(OnTimeNodeChanged);
			
			treewidget.TimeNodeDeleted += new Handlers.TimeNodeDeletedHandler(OnTimeNodeDeleted);
			timeline.TimeNodeDeleted += new Handlers.TimeNodeDeletedHandler(OnTimeNodeDeleted);
			
			treewidget.TimeNodeSelected += new Handlers.TimeNodeSelectedHandler(OnTimeNodeSelected);			
			localPlayersList.TimeNodeSelected += new Handlers.TimeNodeSelectedHandler(OnTimeNodeSelected);
			visitorPlayersList.TimeNodeSelected += new Handlers.TimeNodeSelectedHandler(OnTimeNodeSelected);
			timeline.TimeNodeSelected += new Handlers.TimeNodeSelectedHandler(OnTimeNodeSelected);
			
			playlist.PlayListNodeSelected += new Handlers.PlayListNodeSelectedHandler(OnPlayListNodeSelected);
			playlist.Progress += new LongoMatch.Video.Handlers.ProgressHandler(OnProgress);
			playlist.ApplyCurrentRate += new ApplyCurrentRateHandler(OnApplyRate);
			
			treewidget.PlayListNodeAdded += new Handlers.PlayListNodeAddedHandler(OnPlayListNodeAdded);
			
			treewidget.PlayersTagged += new PlayersTaggedHandler(OnPlayersTagged);
			
			treewidget.SnapshotSeriesEvent += new Handlers.SnapshotSeriesHandler(OnSnapshotSeries);
			localPlayersList.SnapshotSeriesEvent += new Handlers.SnapshotSeriesHandler(OnSnapshotSeries);
			visitorPlayersList.SnapshotSeriesEvent += new Handlers.SnapshotSeriesHandler(OnSnapshotSeries);

			timeline.NewMarkEvent += new NewMarkAtFrameEventHandler(OnNewMarkAtFrame);
			
			player.Prev += new PrevButtonClickedHandler(OnPrev);
			player.Next += new NextButtonClickedHandler(OnNext);
			player.Tick += new TickHandler(OnTick);
			player.SegmentClosedEvent += new SegmentClosedHandler(OnSegmentClosedEvent);
		}
		
		private void ProcessNewMarkEvent(int section,Time pos){
			if (player != null && openedProject != null){
				//Getting defualt star and stop gap for the section
				Time startTime = openedProject.Sections.GetStartTime(section);
				Time stopTime = openedProject.Sections.GetStopTime(section);
				// Calculating borders of the segment depnding
				Time start = pos - startTime;
				Time stop = pos + stopTime;
				Time fStart = (start < new Time(0)) ? new Time(0) : start;
				//La longitud tiene que ser en ms
				Time length;
				
					length = new Time((int)player.StreamLength);
								
				Time fStop = (stop > length) ? length: stop;
				Pixbuf miniature = player.CurrentFrame;
				MediaTimeNode tn = openedProject.AddTimeNode(section,fStart, fStop,miniature);				
				treewidget.AddTimeNode(tn,section);
				timeline.QueueDraw();
			}
		}
		
		protected virtual void OnProgress(float progress){
			
			if (progress > (float)EditorState.START && progress <= (float)EditorState.FINISHED && progress > videoprogressbar.Fraction ){				
				videoprogressbar.Fraction = progress;
			}
			
			if (progress == (float)EditorState.CANCELED ){
				videoprogressbar.Hide();
			}
			
			else if (progress == (float)EditorState.START ){
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
				                          Catalog.GetString("An error has ocurred in the video editor.")
				                          +Catalog.GetString("Please, retry again."));     
				videoprogressbar.Hide();				
			}	
		}
			
	    protected virtual void OnNewMarkAtFrame(int section, int frame){
			
			Time pos = new Time(frame*1000/openedProject.File.Fps);
			ProcessNewMarkEvent(section,pos);
		}
		
		public virtual void OnNewMark(int i){
			Time pos = new Time((int)player.CurrentTime);
			ProcessNewMarkEvent(i,pos);					
		}
		
		protected virtual void OnTimeNodeSelected (MediaTimeNode tNode)
		{			
			selectedTimeNode = tNode;			
			timeline.SelectedTimeNode = tNode;
			player.SetStartStop(tNode.Start.MSeconds,tNode.Stop.MSeconds);		
			notes.Visible = true;
			notes.Play= tNode;
		}
		
		
		protected virtual void OnTimeNodeChanged (TimeNode tNode, object val)
		{
			//Si hemos modificado el valor de un nodo de tiempo a través del 
			//widget de ajuste de tiempo posicionamos el reproductor en el punto
			//
			if (tNode is MediaTimeNode && val is Time ){	
				if(tNode != selectedTimeNode)
					OnTimeNodeSelected((MediaTimeNode)tNode);
				Time pos = (Time)val;
					if (pos == tNode.Start){					
					    player.UpdateSegmentStartTime(pos.MSeconds);
				    }				
				    else{
					    player.UpdateSegmentStopTime(pos.MSeconds);
				    }

			}	
			else if (tNode is SectionsTimeNode){
				buttonswidget.Sections = openedProject.Sections;
			}
			
		}
		
		protected virtual void OnTimeNodeDeleted (MediaTimeNode tNode,int section)
		{
			treewidget.DeleteTimeNode(tNode,section);
			openedProject.DeleteTimeNode(tNode,section);			
			timeline.QueueDraw();
			MainClass.DB.UpdateProject(openedProject);			
		}
		
		
		protected virtual void OnPlayListNodeAdded (MediaTimeNode tNode)
		{
			playlist.Add(new PlayListTimeNode(openedProject.File.FilePath,tNode));
		}
		
		

		protected virtual void OnPlayListNodeSelected (PlayListTimeNode plNode, bool hasNext)
		{
			if (openedProject == null){
				if (plNode.Valid){
					player.SetPlayListElement(plNode.FileName,plNode.Start.MSeconds,plNode.Stop.MSeconds,plNode.Rate,hasNext);
					selectedTimeNode = plNode;
				}
			}
			else {
				MessagePopup.PopupMessage(playlist, MessageType.Error, 
				                          Catalog.GetString("Please, close the opened project to play the playlist."));
				playlist.Stop();
			}
		}
		
		protected virtual void OnPlayListSegmentDone ()
		{	
			playlist.Next();
		}

		protected virtual void OnSegmentClosedEvent ()
		{
			selectedTimeNode = null;
			timeline.SelectedTimeNode = null;
			notes.Visible = false;
		}
		
		protected virtual void OnSnapshotSeries(MediaTimeNode tNode){
			SnapshotsDialog sd;
			IFramesCapturer capturer;
			uint interval;
			string seriesName;
			string outDir;
			
			// We need to close the actual segment to seek freely along the stream
			player.CloseActualSegment();
			capturer = (IFramesCapturer)(player.Player);			
			
			sd= new SnapshotsDialog();
			sd.TransientFor= (Gtk.Window) treewidget.Toplevel;
			sd.Play = tNode.Name;
			
			if (sd.Run() == (int)ResponseType.Ok){
				sd.Destroy();
				
				interval = sd.Interval;
				seriesName = sd.SeriesName;			
				outDir = System.IO.Path.Combine(MainClass.SnapshotsDir(),seriesName);				
				fsc = new FramesSeriesCapturer(capturer,openedProject.File.FilePath,tNode.Start.MSeconds,tNode.Stop.MSeconds,interval,outDir);
				fcpd = new FramesCaptureProgressDialog(fsc);
				fcpd.TransientFor=(Gtk.Window) treewidget.Toplevel;
				fcpd.Run();
				fcpd.Destroy();
			}
			else 
				sd.Destroy();
		}
		

		protected virtual void OnNext ()
		{
			playlist.Next();
		}
		
		protected virtual void OnPrev ()
		{
			if (selectedTimeNode is MediaTimeNode)
				player.SeekInSegment(selectedTimeNode.Start.MSeconds);				
			else if (selectedTimeNode is PlayListTimeNode)
				playlist.Prev();
			else if (selectedTimeNode == null)
				player.SeekTo(0,false);			
		}
		
		protected virtual void OnTick (object o, LongoMatch.Video.Handlers.TickArgs args)
		{
			if (args.CurrentTime != 0 && timeline != null && openedProject != null)
				timeline.CurrentFrame=(uint)(args.CurrentTime * openedProject.File.Fps / 1000);
		}
		
	
		protected virtual void OnTimeline2PositionChanged (Time pos)
		{
			player.SeekInSegment(pos.MSeconds);
		}
		
		protected virtual void OnApplyRate (PlayListTimeNode plNode){
			plNode.Rate = player.Rate;
		}
		
		protected virtual void OnPlayersTagged (MediaTimeNode tNode, Team team){
			PlayersSelectionDialog dialog = new PlayersSelectionDialog();
			if (team == Team.LOCAL){
				dialog.SetPlayersInfo(openedProject.LocalTeamTemplate);
				dialog.PlayersChecked = tNode.LocalPlayers;
				if (dialog.Run() == (int) ResponseType.Ok){				
					tNode.LocalPlayers = dialog.PlayersChecked;
					localPlayersList.UpdatePlaysList(openedProject.GetLocalTeamModel());					
				}
			}
			
			else if (team == Team.VISITOR){
				dialog.SetPlayersInfo(openedProject.VisitorTeamTemplate);
				dialog.PlayersChecked = tNode.VisitorPlayers;
				if (dialog.Run() == (int) ResponseType.Ok){				
					tNode.VisitorPlayers = dialog.PlayersChecked;
					visitorPlayersList.UpdatePlaysList(openedProject.GetVisitorTeamModel());
				}
			}			
			dialog.Destroy();		         
		}
		
		
		
	}
}
