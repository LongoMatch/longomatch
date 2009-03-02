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
		private ButtonsWidget buttonswidget;
		private PlayListWidget playlist;
		private PlayerBin player;
		private TimeLineWidget timeline;
		private ProgressBar videoprogressbar;
		private NotesWidget notes;
		private FramesSeriesCapturer fsc;
		private FramesCaptureProgressDialog fcpd;
		
		// Current play in use null if no play is loaded
		private TimeNode selectedTimeNode=null;
		// current proyect in use
		private Project openedProject;
		
		public EventsManager(TreeWidget treewidget,ButtonsWidget buttonswidget,PlayListWidget playlist,
		                     PlayerBin playerbin,TimeLineWidget timeline, ProgressBar videoprogressbar,
		                     NotesWidget notes)
		{
			this.treewidget = treewidget;
			this.buttonswidget = buttonswidget;
			this.playlist = playlist;
			this.player = playerbin;
			this.timeline = timeline;	
			this.videoprogressbar = videoprogressbar;
			this.notes = notes;
			
			//Adding Handlers for each event
			
			
			this.buttonswidget.NewMarkEvent += new Handlers.NewMarkEventHandler(OnNewMark);
			
			this.treewidget.TimeNodeChanged += new Handlers.TimeNodeChangedHandler(OnTimeNodeChanged);
			this.timeline.TimeNodeChanged += new Handlers.TimeNodeChangedHandler(OnTimeNodeChanged);
			this.notes.TimeNodeChanged += new TimeNodeChangedHandler(OnTimeNodeChanged);
			
			this.treewidget.TimeNodeDeleted += new Handlers.TimeNodeDeletedHandler(OnTimeNodeDeleted);
			this.timeline.TimeNodeDeleted += new Handlers.TimeNodeDeletedHandler(OnTimeNodeDeleted);
			
			this.treewidget.TimeNodeSelected += new Handlers.TimeNodeSelectedHandler(OnTimeNodeSelected);
			this.timeline.TimeNodeSelected += new Handlers.TimeNodeSelectedHandler(OnTimeNodeSelected);
			
			this.playlist.PlayListNodeSelected += new Handlers.PlayListNodeSelectedHandler(OnPlayListNodeSelected);
			this.playlist.Progress += new LongoMatch.Handlers.ProgressHandler(OnProgress);
			

			
			this.treewidget.PlayListNodeAdded += new Handlers.PlayListNodeAddedHandler(OnPlayListNodeAdded);
			
			this.treewidget.SnapshotSeriesEvent += new Handlers.SnapshotSeriesHandler(OnSnapshotSeries);

			this.timeline.NewMarkEvent += new NewMarkAtFrameEventHandler(OnNewMarkAtFrame);
			
			playerbin.Prev += new PrevButtonClickedHandler(OnPrev);
			playerbin.Next += new NextButtonClickedHandler(OnNext);
			playerbin.Tick += new TickHandler(OnTick);
			playerbin.SegmentClosedEvent += new SegmentClosedHandler(OnSegmentClosedEvent);
		}
		
		public  Project OpenedProject{
			set{
				this.openedProject = value;
			}
		}
		
		private void ProcessNewMarkEvent(int section,Time pos){
			if (this.player != null && openedProject != null){
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
				Pixbuf miniature = this.player.CurrentFrame;
				MediaTimeNode tn = openedProject.AddTimeNode(section,fStart, fStop,miniature);				
				treewidget.AddTimeNode(tn);
				this.timeline.QueueDraw();
			}
		}
		
		protected virtual void OnProgress(float progress){
			if (progress > 0 && progress <= 1){								
				this.videoprogressbar.Fraction = progress;

			}
			
			if (progress == -1 ){
				this.videoprogressbar.Hide();
			}
			
			else if (progress == 0 ){
				this.videoprogressbar.Show();
				this.videoprogressbar.Fraction = 0;
				this.videoprogressbar.Text = "Creating new video";
			}
			
			else if (progress == 1) {				
				MessageDialog info = new MessageDialog((Gtk.Window)(this.player.Toplevel),
				                                       DialogFlags.Modal,
				                                       MessageType.Info,
				                                       ButtonsType.Ok,
				                                       Catalog.GetString("Finished Video Edition."));
				info.Run();
				info.Destroy();
				this.videoprogressbar.Hide();				
			}		
		}
			
	    protected virtual void OnNewMarkAtFrame(int section, int frame){
			
			Time pos = new Time(frame*1000/this.openedProject.File.Fps);
			ProcessNewMarkEvent(section,pos);
		}
		
		protected virtual void OnNewMark(int i){
			Time pos = new Time((int)player.CurrentTime);
			ProcessNewMarkEvent(i,pos);					
		}
		
		protected virtual void OnTimeNodeSelected (MediaTimeNode tNode)
		{			
			this.selectedTimeNode = tNode;			
			this.timeline.SelectedTimeNode = tNode;
			this.player.SetStartStop(tNode.Start.MSeconds,tNode.Stop.MSeconds);		
			this.notes.Visible = true;
			this.notes.Play= tNode;
		}
		
		
		protected virtual void OnTimeNodeChanged (TimeNode tNode, object val)
		{
			//Si hemos modificado el valor de un nodo de tiempo a través del 
			//widget de ajuste de tiempo posicionamos el reproductor en el punto
			//
			if (tNode is MediaTimeNode && val is Time ){	
				if(tNode != selectedTimeNode)
					this.OnTimeNodeSelected((MediaTimeNode)tNode);
				Time pos = (Time)val;
				//if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				//	this.player.Play();
				//else 
				//	this.player.Pause();
					if (pos == tNode.Start){					
					    this.player.UpdateSegmentStartTime(pos.MSeconds);
				    }				
				    else{
					    this.player.UpdateSegmentStopTime(pos.MSeconds);
				    }
				//if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				//	this.player.Pause();
			}	
			else if (tNode is SectionsTimeNode){
				this.buttonswidget.Sections = openedProject.Sections;
			}
			
		}
		
		protected virtual void OnTimeNodeDeleted (MediaTimeNode tNode)
		{
			this.treewidget.DeleteTimeNode(tNode);
			openedProject.DelTimeNode(tNode);			
			this.timeline.QueueDraw();
			MainClass.DB.UpdateProject(openedProject);
			
			
		}
		
		
		protected virtual void OnPlayListNodeAdded (MediaTimeNode tNode)
		{
			this.playlist.Add(new PlayListTimeNode(openedProject.File.FilePath,tNode));
		}
		
		

		protected virtual void OnPlayListNodeSelected (PlayListTimeNode plNode, bool hasNext)
		{
			if (openedProject == null){
				this.selectedTimeNode = plNode;
				if (plNode.Valid){
					this.player.SetPlayListElement(plNode.FileName,plNode.Start.MSeconds,plNode.Stop.MSeconds,hasNext);
					this.playlist.StartClock();
				}
			}
			else {
				MessageDialog error = new MessageDialog(null,
				                                        DialogFlags.DestroyWithParent,
				                                        MessageType.Error,
				                                        ButtonsType.Ok,
				                                        "Please, close the opened project to play the playlist.");
				error.Run();
				error.Destroy();
				this.playlist.Stop();
			}
		}
		
		protected virtual void OnPlayListSegmentDone ()
		{	
			playlist.Next();
		}

		protected virtual void OnSegmentClosedEvent ()
		{
			this.selectedTimeNode = null;
			this.timeline.SelectedTimeNode = null;
			this.notes.Visible = false;
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
				Console.WriteLine("closed");
				fcpd.Destroy();
			}
		}
		

		protected virtual void OnNext ()
		{
			this.playlist.Next();
		}
		
		protected virtual void OnPrev ()
		{
			if (this.selectedTimeNode is MediaTimeNode){
				this.player.SeekInSegment(this.selectedTimeNode.Start.MSeconds);
				
			}
			else if (this.selectedTimeNode is PlayListTimeNode)
				this.playlist.Prev();
			else if (this.selectedTimeNode == null)
				this.player.SeekTo(0,false);
		}
		
		protected virtual void OnTick (object o, LongoMatch.Video.Handlers.TickArgs args)
		{
			if (args.CurrentTime != 0 && this.timeline != null && openedProject != null)
				this.timeline.CurrentFrame=(uint)(args.CurrentTime * openedProject.File.Fps / 1000);
		}
		
	
		protected virtual void OnTimeline2PositionChanged (Time pos)
		{
			this.player.SeekInSegment(pos.MSeconds);
		}
		
		
		
	}
}
