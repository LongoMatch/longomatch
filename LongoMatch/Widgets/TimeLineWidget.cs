// TimeLineWidget.cs
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
using System.Collections.Generic;
using Gtk;
using Gdk;
using LongoMatch.DB;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component {
	
	
	public partial class TimeLineWidget : Gtk.Bin
	{
		
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeDeletedHandler TimeNodeDeleted;
		public event NewMarkAtFrameEventHandler NewMarkEvent;
		//public event PlayListNodeAddedHandler PlayListNodeAdded;
		
		private TimeScale[] tsArray;
		private List<MediaTimeNode>[] tnArray;
		private Sections sections;
		private TimeReferenceWidget tr;
		private uint frames;
		private uint pixelRatio=1;
		private MediaTimeNode selected;
		private uint currentFrame;

		
		public TimeLineWidget()
		{
			this.Build();	
			
		}
		
		
		public MediaTimeNode SelectedTimeNode{
			get{return this.selected;}
			set{
				this.selected = value;
				if (tsArray != null && tnArray != null){
					foreach (TimeScale  ts in tsArray){
						ts.SelectedTimeNode = value;					
					}
				}
				this.QueueDraw();
				
				if (this.selected != null){
					//TODO 
					/*if (SelectedTimeNode.StartFrame/pixelRatio < this.GtkScrolledWindow.Hadjustment.Value)
						this.AdjustPostion(SelectedTimeNode.StartFrame/pixelRatio);*/
				}
			}
		}
		
		public uint CurrentFrame{
			get{return this.currentFrame;}
			set{
				this.currentFrame = value;
				
				if (tsArray != null && tnArray != null){
					
					foreach (TimeScale  ts in tsArray){
						ts.CurrentFrame = value;					
					}
					tr.CurrentFrame = value;
				}
				this.QueueDraw();

			}
		}
		
		public void AdjustPostion(uint currentframe){
			int visibleWidth;
			int realWidth;
			uint pos;
			int scrollbarWidth;
			if (this.Visible){
				
				scrollbarWidth= this.GtkScrolledWindow.VScrollbar.Allocation.Width;
				visibleWidth = this.GtkScrolledWindow.Allocation.Width-scrollbarWidth;
				realWidth = this.vbox1.Allocation.Width;				
				pos = currentframe/pixelRatio;				
				if (pos+visibleWidth < realWidth){
					this.GtkScrolledWindow.Hadjustment.Value = pos;		
				}
				else {
					this.GtkScrolledWindow.Hadjustment.Value = realWidth-visibleWidth-20;
				}

			}
			
		}
		
		
		public void SetPixelRatio(uint pixelRatio){
			
		
			if (tsArray != null && tnArray != null){
				this.pixelRatio = pixelRatio;
				this.tr.PixelRatio = pixelRatio;
				foreach (TimeScale  ts in tsArray){
					ts.PixelRatio = this.pixelRatio;
					
				}	
				tr.Size((int)(this.frames/pixelRatio),50);				
			}
					
			
			
			
		}
		
		
		
		public Project Project{
			set{
				sections = value.Sections;
				tnArray = value.GetDataArray();
				tsArray = new TimeScale[20]; 
				this.pixelRatio=1;
				
				//Unrealize all children
				foreach (Widget w in vbox1.AllChildren){
					w.Unrealize();
					this.vbox1.Remove(w);
				}				
				
				this.frames = value.File.GetFrames();
				ushort fps = value.File.Fps;
				
				tr = new TimeReferenceWidget(frames,fps);
				tr.PixelRatio = 1;
				this.vbox1.PackStart(tr,false,false,0);
				tr.Show();
				for (int i=0; i<20; i++){
					TimeScale ts = new TimeScale(i,tnArray[i],frames,sections.GetColor(i));
					ts.PixelRatio = 1;
					tsArray[i]=ts;
					ts.TimeNodeChanged += new TimeNodeChangedHandler(OnTimeNodeChanged);
					ts.TimeNodeSelected += new TimeNodeSelectedHandler (OnTimeNodeSelected);
					ts.TimeNodeDeleted += new TimeNodeDeletedHandler(OnTimeNodeDeleted);
					ts.NewMarkAtFrameEvent += new NewMarkAtFrameEventHandler(OnNewMark);
					this.vbox1.PackStart(ts,true,true,0);					
					if (value.Sections.GetVisibility(i)){
						ts.Show();
					}
				}
			}
			
		}
	
		protected virtual void OnNewMark(int section, int frame){
			if (this.NewMarkEvent != null)
				this.NewMarkEvent(section,frame);
		}
		
		protected virtual void OnTimeNodeChanged(TimeNode tn, object val){
			if (this.TimeNodeChanged != null)			
				this.TimeNodeChanged(tn,val);
		}
		
		protected virtual void OnTimeNodeSelected(MediaTimeNode tn){
			if (this.TimeNodeSelected != null)			
				this.TimeNodeSelected(tn);
		}
		protected virtual void OnTimeNodeDeleted(MediaTimeNode tn){
			if (this.TimeNodeDeleted != null)			
				this.TimeNodeDeleted(tn);
		}

		protected virtual void OnZoominbuttonClicked (object sender, System.EventArgs e)
		{
			if (this.pixelRatio > 2){
				this.pixelRatio--;
				this.pixelRatio--;
				this.SetPixelRatio(this.pixelRatio);				
				this.QueueDraw();
				this.AdjustPostion(currentFrame);
			}
			
		}

		protected virtual void OnZoomoutbuttonClicked (object sender, System.EventArgs e)
		{
			if (this.pixelRatio <99){
				this.pixelRatio++;
				this.pixelRatio++;
				this.SetPixelRatio(this.pixelRatio);  				
				this.QueueDraw();				
				this.AdjustPostion(currentFrame);
			}
			
		}

		protected virtual void OnFitbuttonClicked (object sender, System.EventArgs e)
		{
			this.AdjustPostion(currentFrame);
		}
		
		

	}
}
