// TimeScale.cs
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
using Cairo;
using Gdk;
using Gtk;
using Pango;
using Mono.Unix;
using LongoMatch.Handlers;
using LongoMatch.TimeNodes;

namespace LongoMatch.Gui.Component
{
	
	
	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public class TimeScale : Gtk.DrawingArea
	{
		private const int SECTION_HEIGHT = 25;
		private const double ALPHA = 0.6;
		
		private uint frames;
		private uint pixelRatio=10;
		
		private object locker;
		
		private int section;
		private string name;
		private Cairo.Color color;
		private List<MediaTimeNode> list;
		
		private MediaTimeNode candidateTN;
		private bool candidateStart;
		private bool movingLimit;
		private MediaTimeNode selected=null;
		
		private uint lastTime=0;
		private uint currentFrame;
		
		private Menu deleteMenu;
		private Menu menu;
		private MenuItem delete;
		private int cursorFrame;
		private Dictionary<MenuItem,MediaTimeNode> dic;
		
		private Pango.Layout layout;
			
		public event NewMarkAtFrameEventHandler NewMarkAtFrameEvent;
		public event TimeNodeChangedHandler TimeNodeChanged;
		public event TimeNodeSelectedHandler TimeNodeSelected;
		public event TimeNodeDeletedHandler TimeNodeDeleted;

		
		public TimeScale(int section,List<MediaTimeNode> list,string name, uint frames,Gdk.Color color)
		{			
			this.section = section;
			this.frames = frames;	
			this.list = list;	
			this.name = name;
			HeightRequest= SECTION_HEIGHT;
			Size((int)(frames/pixelRatio),SECTION_HEIGHT);
			this.color = RGBToCairoColor(color);
			this.color.A = ALPHA;
			Events = EventMask.PointerMotionMask | EventMask.ButtonPressMask | EventMask.ButtonReleaseMask ;
			
			dic = new Dictionary<MenuItem,MediaTimeNode>();
			
			layout =  new Pango.Layout(PangoContext);
			layout.Wrap = Pango.WrapMode.Char;
			layout.Alignment = Pango.Alignment.Left;

			SetMenu();
			locker = new object();
		}
			
		public uint PixelRatio{
			get {return pixelRatio;}
			set {
				lock(locker){
					pixelRatio = value;
					Size((int)(frames/pixelRatio),SECTION_HEIGHT);
				}
			}
		}
		
		public uint CurrentFrame{
			get{return currentFrame;}
			set{currentFrame = value;}
		}
		
		public MediaTimeNode SelectedTimeNode{
			get{return selected;}
			set{selected = value;}
		}
		
		public void ReDraw(){
			Gdk.Region region = GdkWindow.ClipRegion;
			GdkWindow.InvalidateRegion(region,true);
			GdkWindow.ProcessUpdates(true);
		}
		
		private Cairo.Color RGBToCairoColor(Gdk.Color gdkColor){				
			return   new Cairo.Color((double)(gdkColor.Red)/ushort.MaxValue,
			                         (double)(gdkColor.Green)/ushort.MaxValue,
			                         (double)(gdkColor.Blue)/ushort.MaxValue);
		}
		
		private void SetMenu(){			
			MenuItem newPlay;
			
			menu = new Menu();			
			delete = new MenuItem(Catalog.GetString("Delete Play"));			
			newPlay = new MenuItem(Catalog.GetString("Add New Play"));
			
			menu.Append(newPlay);
			menu.Append(delete);
			
			newPlay.Activated += new EventHandler(OnNewPlay);
			
			menu.ShowAll();			
		}
		
		private void DrawTimeNodes(Gdk.Window win){
			lock(locker){
				bool hasSelectedTimeNode=false;
			
				using (Cairo.Context g = Gdk.CairoHelper.Create (win)){	
					int height;
					int width;	

				
					win.Resize((int)(frames/pixelRatio), Allocation.Height);
					win.GetSize(out width, out height);				
				
					g.Operator = Operator.Over;
				
					foreach (MediaTimeNode tn in list){	
						if (tn != selected) {
							g.Rectangle( new Cairo.Rectangle(tn.StartFrame/pixelRatio,3,tn.TotalFrames/pixelRatio,height-6));					
							g.LineWidth = 2;
							g.Color = new Cairo.Color (color.R+0.1, color.G+0.1,color.B+0.1, 1);				
							g.LineJoin = LineJoin.Round;
							g.StrokePreserve();
							g.Color = color;						
							g.Fill();
						}
						else {
							hasSelectedTimeNode = true;
						}								
					}
					//Then we draw the selected TimeNode over the others
					if (hasSelectedTimeNode){					
						g.Rectangle( new Cairo.Rectangle(selected.StartFrame/pixelRatio,3,selected.TotalFrames/pixelRatio,height-6));					
						g.Color = new Cairo.Color (0, 0, 0, 1);		
						g.LineWidth = 3;
						g.LineJoin = LineJoin.Round;
						g.Operator = Operator.Source;
						g.StrokePreserve();
						g.Operator = Operator.Over;
						g.Color = color;						
						g.Fill();
					}				
					DrawLines(win,g,height,width);			
				}
			}
		}
		
		private void DrawLines(Gdk.Window win, Cairo.Context g, int height, int width){
			if (Environment.OSVersion.Platform == PlatformID.Unix){
				g.Operator = Operator.Over;
				g.Color = new Cairo.Color(0,0,0);
				g.LineWidth = 1;
				g.MoveTo(currentFrame/pixelRatio,0);
				g.LineTo(currentFrame/pixelRatio,height);
				g.Stroke();		
				g.Color = new Cairo.Color(0,0,0);
				g.LineWidth = 2;
				g.MoveTo(0,0);
				g.LineTo(width,0);
				g.Stroke();	
				g.MoveTo(0,height);
				g.LineTo(width,height);
				g.Stroke();
			}
			
			else {
				win.DrawLine(Style.DarkGC(StateType.Normal),0,0,width,0);
				win.DrawLine(Style.DarkGC(StateType.Normal),
				             (int)(currentFrame/pixelRatio),
				             0,
				             (int)(currentFrame/pixelRatio),height);
			}
		}
		
		/*private void DrawSectionName(){
			//FIXME 200 is enought?
			layout.Width = Pango.Units.FromPixels(200);
			layout.SetMarkup(name);
			GdkWindow.DrawLayout(Style.TextGC(StateType.Normal),Allocation.X,0,layout);
			
		}*/
		
		private void DrawTimeNodesName(){
			lock(locker){
				foreach (MediaTimeNode tn in list){	
					layout.Width = Pango.Units.FromPixels((int)(tn.TotalFrames/pixelRatio));
					layout.SetMarkup (tn.Name);
					GdkWindow.DrawLayout(Style.TextGC(StateType.Normal),(int)(tn.StartFrame/pixelRatio),2,layout);
				}
			}
		}
		
		private void ProcessButton3(double X){
			cursorFrame =(int) (X*pixelRatio);
			deleteMenu = new Menu();
			delete.Submenu=deleteMenu;
			dic.Clear();
			foreach (MediaTimeNode tn in list){				
				//We scan all the time Nodes looking for one matching the cursor selectcio
				//And we add them to the delete menu
				if (tn.HasFrame(cursorFrame) ){						
					MenuItem del = new MenuItem(Catalog.GetString("Delete "+tn.Name));					
					del.Activated += new EventHandler(OnDelete);				
					deleteMenu.Append(del);
					dic.Add(del,tn);					
				}
				
			}	
			menu.ShowAll();
			menu.Popup();		
		}
		
		private void ProcessButton1(EventButton evnt){
			if (lastTime != evnt.Time){
				candidateTN = null;
				foreach (MediaTimeNode tn in list){	
					int pos = (int) (evnt.X*pixelRatio);
					if (Math.Abs(pos-tn.StopFrame) < 3*pixelRatio){
						candidateStart = false;
						candidateTN = tn;
						movingLimit = true;
						GdkWindow.Cursor = new Gdk.Cursor(CursorType.SbHDoubleArrow);
						TimeNodeChanged(tn,tn.Stop);
						ReDraw();
						break;
					}
					else if (Math.Abs(pos-tn.StartFrame) < 3*pixelRatio){
						candidateStart =true;
						candidateTN = tn;
						movingLimit = true;
						GdkWindow.Cursor = new Gdk.Cursor(CursorType.SbHDoubleArrow);
						TimeNodeChanged(tn,tn.Start);
						ReDraw();
						break;
					}			
				}
			}
			//On Double Click
			else {
				foreach (MediaTimeNode tn in list){
					int pos = (int) (evnt.X*pixelRatio);
					if (TimeNodeSelected!= null && tn.HasFrame(pos) ){							
						TimeNodeSelected(tn);
						break;
					}
				}					
			}
		}
		
		protected void OnNewPlay(object obj, EventArgs args){
			if (NewMarkAtFrameEvent != null)				
				NewMarkAtFrameEvent(section,cursorFrame);			
		}
		
		protected void OnDelete(object obj, EventArgs args){
			MediaTimeNode tNode;
			dic.TryGetValue((MenuItem)obj, out tNode);
			if (TimeNodeDeleted != null && tNode != null){
				TimeNodeDeleted(tNode, section);
			}
		}
		
		protected override bool OnExposeEvent (EventExpose evnt)
		{			
			if (Visible){
				DrawTimeNodes(evnt.Window);
				//We don't need the draw the Sections Names if we also draw the TimeNode name
				//DrawSectionName();
				DrawTimeNodesName();
			}
			return base.OnExposeEvent (evnt);			
		}
		
		protected override bool OnMotionNotifyEvent (EventMotion evnt)
		{
			uint pos = (uint) (evnt.X*pixelRatio);
			
			
			if (movingLimit){				
				
				if (candidateStart && pos  > 0 && pos < candidateTN.StopFrame-10){
					candidateTN.StartFrame = pos;					
					if (TimeNodeChanged != null)
						TimeNodeChanged(candidateTN,candidateTN.Start);
				}
				else if (!candidateStart && pos < frames && pos > candidateTN.StartFrame+10){
					candidateTN.StopFrame = pos;					
					if (TimeNodeChanged != null)
						TimeNodeChanged(candidateTN,candidateTN.Stop);
				}
				
				Gdk.Region region = GdkWindow.ClipRegion;
				GdkWindow.InvalidateRegion(region,true);
				GdkWindow.ProcessUpdates(true);			
				
			}
			return base.OnMotionNotifyEvent (evnt);
		}
		
		protected override bool OnButtonPressEvent (EventButton evnt)
		{		
			if (evnt.Button == 1){
				ProcessButton1(evnt);	
				
				                                       
			}
			// On Right button pressed
			else if (evnt.Button == 3){
				ProcessButton3(evnt.X);
			}
			lastTime = evnt.Time;		
			return base.OnButtonPressEvent (evnt);
		}
		
		protected override bool OnButtonReleaseEvent (EventButton evnt)
		{
			if (movingLimit){
				movingLimit = false;
				candidateTN.Selected = false;
				GdkWindow.Cursor = new Gdk.Cursor(CursorType.Arrow);
				ReDraw();
			}
			return base.OnButtonReleaseEvent (evnt);
		}	
	}
}
