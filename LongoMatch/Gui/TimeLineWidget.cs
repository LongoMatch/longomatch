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
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
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
		private uint pixelRatio;
		private MediaTimeNode selected;
		private uint currentFrame;

		
		public TimeLineWidget()
		{
			this.Build();			
			SetPixelRatio(10);			
		}		
		
		public MediaTimeNode SelectedTimeNode{
			get{return selected;}
			set{
				selected = value;
				if (tsArray != null && tnArray != null){
					foreach (TimeScale  ts in tsArray){
						ts.SelectedTimeNode = value;					
					}
				}				
				if (selected != null){
					if (SelectedTimeNode.StartFrame/pixelRatio < GtkScrolledWindow.Hadjustment.Value ||
					    SelectedTimeNode.StartFrame/pixelRatio > GtkScrolledWindow.Hadjustment.Value +
					    GtkScrolledWindow.Allocation.Width - GtkScrolledWindow.VScrollbar.Allocation.Width)
						AdjustPostion(SelectedTimeNode.StartFrame);
				}
				QueueDraw();
			}
		}
		
		public uint CurrentFrame{
			get{return currentFrame;}
			set{
				currentFrame = value;
				
				if (tsArray != null && tnArray != null){					
					foreach (TimeScale  ts in tsArray){
						ts.CurrentFrame = value;					
					}
					tr.CurrentFrame = value;
				}
				QueueDraw();
			}
		}
		
		public void AdjustPostion(uint currentframe){
			int visibleWidth;
			int realWidth;
			uint pos;
			int scrollbarWidth;
			if (Visible){				
				scrollbarWidth= GtkScrolledWindow.VScrollbar.Allocation.Width;
				visibleWidth = GtkScrolledWindow.Allocation.Width-scrollbarWidth;
				realWidth = vbox1.Allocation.Width;				
				pos = currentframe/pixelRatio;
							if (pos+visibleWidth < realWidth){
					GtkScrolledWindow.Hadjustment.Value = pos;		
				}
				else {
					GtkScrolledWindow.Hadjustment.Value = realWidth-visibleWidth-20;
				}
			}			
		}
		
		
		private void SetPixelRatio(uint pixelRatio){			
			if (tsArray != null && tnArray != null){
				this.pixelRatio = pixelRatio;
				tr.PixelRatio = pixelRatio;
				foreach (TimeScale  ts in tsArray){
					ts.PixelRatio = pixelRatio;					
				}	
				vscale1.Value=pixelRatio;				
			}		
		}
			
		
		public Project Project{
			set{
				sections = value.Sections;
				tnArray = value.GetDataArray();
				tsArray = new TimeScale[20]; 
				
				
				//Unrealize all children
				foreach (Widget w in vbox1.AllChildren){
					w.Unrealize();
					vbox1.Remove(w);
				}				
				
				frames = value.File.GetFrames();
				ushort fps = value.File.Fps;
				
				tr = new TimeReferenceWidget(frames,fps);
				vbox1.PackStart(tr,false,false,0);
				tr.Show();
				for (int i=0; i<20; i++){
					TimeScale ts = new TimeScale(i,tnArray[i],sections.GetName(i),frames,sections.GetColor(i));
					tsArray[i]=ts;
					ts.TimeNodeChanged += new TimeNodeChangedHandler(OnTimeNodeChanged);
					ts.TimeNodeSelected += new TimeNodeSelectedHandler (OnTimeNodeSelected);
					ts.TimeNodeDeleted += new TimeNodeDeletedHandler(OnTimeNodeDeleted);
					ts.NewMarkAtFrameEvent += new NewMarkAtFrameEventHandler(OnNewMark);
					vbox1.PackStart(ts,true,true,0);					
					if (value.Sections.GetVisibility(i)){
						ts.Show();
					}
				}
				SetPixelRatio(3);
			}
			
		}
	
		protected virtual void OnNewMark(int section, int frame){
			if (NewMarkEvent != null)
				NewMarkEvent(section,frame);
		}
		
		protected virtual void OnTimeNodeChanged(TimeNode tn, object val){
			if (TimeNodeChanged != null)			
				TimeNodeChanged(tn,val);
		}
		
		protected virtual void OnTimeNodeSelected(MediaTimeNode tn){
			if (TimeNodeSelected != null)			
				TimeNodeSelected(tn);
		}
		protected virtual void OnTimeNodeDeleted(MediaTimeNode tn){
			if (TimeNodeDeleted != null)			
				TimeNodeDeleted(tn);
		}		

		protected virtual void OnFitbuttonClicked (object sender, System.EventArgs e)
		{
			AdjustPostion(currentFrame);
		}

		protected virtual void OnVscale1ValueChanged (object sender, System.EventArgs e)
		{
			SetPixelRatio((uint)(vscale1.Value)); 
			QueueDraw();
			AdjustPostion(currentFrame);			
		}
	}
}
