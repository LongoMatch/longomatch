// TimeScale.cs
//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
// Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
//
//

using System;
using System.Collections.Generic;
using System.Linq;
using Cairo;
using Gdk;
using Gtk;
using Pango;
using Mono.Unix;
using LongoMatch.Common;
using LongoMatch.Handlers;
using LongoMatch.Interfaces;
using LongoMatch.Store;

namespace LongoMatch.Gui.Base
{


	[System.ComponentModel.Category("LongoMatch")]
	[System.ComponentModel.ToolboxItem(true)]
	public abstract class TimeScaleBase<T>: Gtk.DrawingArea where T:ITimelineNode
	{
		const int SECTION_HEIGHT = 25;
		const double ALPHA = 0.6;
		const int MIN_CHAR_WIDTH = 50;

		uint frames;
		uint pixelRatio=10;

		Cairo.Color aColor, bColor, cColor;
		List<T> list;
		PlaysFilter filter;

		T candidateTN;
		bool candidateStart;
		bool movingLimit;
		uint lastTime=0;
		uint currentFrame;
		T selected= default(T);
		
		Pango.Layout layout;
		
		protected Dictionary<MenuItem,T> menuToNodeDict;
		protected string elementName = "";
		protected int cursorFrame;

		public TimeScaleBase(List<T> list, uint frames, PlaysFilter filter = null)
		{
			this.frames = frames;
			this.list = list;
			this.filter = filter;
			this.aColor = new Cairo.Color(0, 0, 1);
			this.aColor.A = ALPHA;
			this.bColor = new Cairo.Color(1, 0, 0);
			this.bColor.A = ALPHA;
			this.cColor = new Cairo.Color(0, 1, 0);
			this.cColor.A = ALPHA;
			HeightRequest= SECTION_HEIGHT;
			Size((int)(frames/pixelRatio),SECTION_HEIGHT);
			Events = EventMask.PointerMotionMask | EventMask.ButtonPressMask | EventMask.ButtonReleaseMask ;

			menuToNodeDict = new Dictionary<MenuItem, T>();
			
			layout =  new Pango.Layout(PangoContext);
			layout.Wrap = Pango.WrapMode.Char;
			layout.Ellipsize = EllipsizeMode.End;
			layout.Alignment = Pango.Alignment.Left;
		}

		public uint PixelRatio {
			get {
				return pixelRatio;
			}
			set {
				pixelRatio = value;
				Size((int)(frames/pixelRatio),SECTION_HEIGHT);
			}
		}

		public uint CurrentFrame {
			get {
				return currentFrame;
			}
			set {
				currentFrame = value;
			}
		}

		public T SelectedTimeNode {
			get {
				return selected;
			}
			set {
				selected = value;
			}
		}

		public void AddTimeNode(T timeNode) {
			list.Add(timeNode);
			QueueDraw();
		}

		public void RemoveTimeNode(T timeNode) {
			list.Remove(timeNode);
			QueueDraw();
		}

		public void ReDraw() {
			Gdk.Region region = GdkWindow.ClipRegion;
			GdkWindow.InvalidateRegion(region,true);
			GdkWindow.ProcessUpdates(true);
		}

		abstract protected void HandleTimeNodeDeleted(List<T> timenode);
		
		abstract protected void HandleTimeNodeChanged(T timenode, Time time);
		
		abstract protected void HandleTimeNodeSelected(T timenode);
		
		abstract protected void AddNewTimeNode();
		
		virtual protected void ExpandMenu (List<T> timenodes, Dictionary<T, Menu> menusDict) {}
		
		void ShowMenu (List<T> timenodes) {
			Menu menu;
			Dictionary <T, Menu> menusDict;
			
			if (timenodes.Contains(SelectedTimeNode)) {
				timenodes = timenodes.GetRange(0, 1);
			}
			
			menu = new Menu();
			menusDict = CreateCommonMenu (timenodes, menu);
			ExpandMenu (timenodes, menusDict);
			menu.ShowAll();
			menu.Popup();
		}
		
		Dictionary<T, Menu> CreateCommonMenu(List<T> timenodes, Menu menu) {
			MenuItem newTimeNode;
			Dictionary <T, Menu> menusDict;
			
			menusDict = new Dictionary<T, Menu>();
			menuToNodeDict.Clear();
			newTimeNode = new MenuItem(Catalog.GetString("Add new ") + elementName);

			menu.Append(newTimeNode);
			newTimeNode.Activated += new EventHandler(OnNewTimeNode);

			if (timenodes.Count == 1) {
				menusDict.Add (timenodes[0], menu);
			} else {
				foreach(T tn in timenodes) {
					Menu catMenu = new Menu();
					MenuItem catItem = new MenuItem(tn.Name);
					menu.Append (catItem);
					catItem.Submenu = catMenu;
					menusDict.Add(tn, catMenu);
				}
			}
			
			foreach(T tn in timenodes) {
				//We scan all the time Nodes looking for one matching the cursor selectcio
				//And we add them to the delete menu
				MenuItem del = new MenuItem(Catalog.GetString("Delete "));
				del.Activated += new EventHandler(OnDelete);
				menusDict[tn].Append(del);
				menuToNodeDict.Add(del,tn);
			}
			return menusDict;
		}
		
		Cairo.Color GetColor (ITimelineNode tn) {
			if (tn is Play) {
				Play play = tn as Play;
				if (play.Team == Team.LOCAL) {
					return aColor;
				} else if (play.Team == Team.VISITOR) {
					return bColor;
				} else if (play.Team == Team.BOTH) {
					return cColor;
				}
			}
			return aColor;
		}
		
		void DrawTimeNodes(Gdk.Window win) {
			bool hasSelectedTimeNode=false;
			
			using(Cairo.Context g = Gdk.CairoHelper.Create(win)) {
				int height;
				int width;
				
				win.Resize((int)(frames/pixelRatio), Allocation.Height);
				win.GetSize(out width, out height);
				
				g.Operator = Operator.Over;
				
				foreach(T tn in list) {
					if (filter != null && !filter.IsVisible (tn)) {
						continue;
					}
					
					if(!tn.Equals(selected)) {
						Cairo.Color color = GetColor (tn);
						Cairo.Color borderColor = new Cairo.Color(color.R+0.1, color.G+0.1, color.B+0.1, 1);
						CairoUtils.DrawRoundedRectangle(g,tn.StartFrame/pixelRatio,3,
							tn.TotalFrames/pixelRatio,height-6,
							SECTION_HEIGHT/7, color, borderColor);
					}
					else {
						hasSelectedTimeNode = true;
					}
				}
				//Then we draw the selected TimeNode over the others
				if(hasSelectedTimeNode) {
					Cairo.Color color = GetColor (selected);
					Cairo.Color borderColor = new Cairo.Color(0, 0, 0, 1);
					CairoUtils.DrawRoundedRectangle(g,selected.StartFrame/pixelRatio,3,
						selected.TotalFrames/pixelRatio,height-6,
						SECTION_HEIGHT/7, color, borderColor);
					if(selected.HasDrawings) {
						g.MoveTo(selected.KeyFrame/pixelRatio,3);
						g.LineTo(selected.KeyFrame/pixelRatio,SECTION_HEIGHT-3);
						g.StrokePreserve();
					}
				}
				DrawLines(win,g,height,width);
			}
		}

		void DrawLines(Gdk.Window win, Cairo.Context g, int height, int width) {
			if(Environment.OSVersion.Platform == PlatformID.Unix) {
				Cairo.Color color = new Cairo.Color(0,0,0);
				CairoUtils.DrawLine(g, currentFrame/pixelRatio,0,currentFrame/pixelRatio,height,
				                    1, color);
				CairoUtils.DrawLine(g,0 ,0, width, 0, 1, color);
				CairoUtils.DrawLine(g,0 ,height, width, height, 1, color);
			}

			else {
				win.DrawLine(Style.DarkGC(StateType.Normal),0,0,width,0);
				win.DrawLine(Style.DarkGC(StateType.Normal),
				             (int)(currentFrame/pixelRatio),0,
				             (int)(currentFrame/pixelRatio),height);
			}
		}


		void DrawTimeNodesName() {
			foreach(T tn in list) {
				int width;
				
				if (filter != null && !filter.IsVisible (tn)) {
					continue;
				}
				width = (int)(tn.TotalFrames/pixelRatio);
				if (width < MIN_CHAR_WIDTH)
					continue;
				layout.Width = Pango.Units.FromPixels(width);
				layout.SetMarkup(tn.Name);
				GdkWindow.DrawLayout(Style.TextGC(StateType.Normal),
					(int)(tn.StartFrame/pixelRatio) + 2, 2, layout);
			}
		}

		void ProcessButton3(double X) {
			List<T> selected;
			
			cursorFrame =(int)(X*pixelRatio);
			selected = list.Where (tn => tn.HasFrame (cursorFrame)).ToList();
			ShowMenu(selected);
		}

		void ProcessButton1(EventButton evnt) {
			if(lastTime != evnt.Time) {
				candidateTN = default(T);
				foreach(T tn in list) {
					int pos = (int)(evnt.X*pixelRatio);
					//Moving from the right side
					if(Math.Abs(pos-tn.StopFrame) < 3*pixelRatio) {
						candidateStart = false;
						candidateTN = tn;
						movingLimit = true;
						GdkWindow.Cursor = new Gdk.Cursor(CursorType.SbHDoubleArrow);
						HandleTimeNodeChanged(tn, tn.Stop);
						ReDraw();
						break;
					}
					//Moving from the left side
					else if(Math.Abs(pos-tn.StartFrame) < 3*pixelRatio) {
						candidateStart =true;
						candidateTN = tn;
						movingLimit = true;
						GdkWindow.Cursor = new Gdk.Cursor(CursorType.SbHDoubleArrow);
						HandleTimeNodeChanged(tn, tn.Start);
						ReDraw();
						break;
					}
				}
			}
			//On Double Click
			else {
				foreach(T tn in list) {
					int pos = (int)(evnt.X*pixelRatio);
					if(tn.HasFrame(pos)) {
						HandleTimeNodeSelected(tn);
						break;
					}
				}
			}
		}
		
		void OnNewTimeNode(object obj, EventArgs args) {
			AddNewTimeNode();
		}

		void OnDelete(object obj, EventArgs args) {
			T tNode;
			menuToNodeDict.TryGetValue((MenuItem)obj, out tNode);
			if(tNode != null) {
				var list = new List<T>();
				list.Add(tNode);
				HandleTimeNodeDeleted(list);
			}
		}

		protected override bool OnExposeEvent(EventExpose evnt)
		{
			if(Visible) {
				DrawTimeNodes(evnt.Window);
				//We don't need the draw the Sections Names if we also draw the TimeNode name
				//DrawSectionName();
				DrawTimeNodesName();
			}
			return base.OnExposeEvent(evnt);
		}

		protected override bool OnMotionNotifyEvent(EventMotion evnt)
		{
			int pos = (int)(evnt.X*pixelRatio);

			//If not moving don't do anything
			if(!movingLimit) {
			}
			//Moving Start time
			else if(candidateStart) {
				if(candidateTN.HasDrawings && pos > 0 && pos > candidateTN.KeyFrame-10)
					candidateTN.StartFrame = candidateTN.KeyFrame-10;
				//Check not to go under start time nor 0
				else if(pos  > 0 && pos < candidateTN.StopFrame-10)
					candidateTN.StartFrame = (uint)pos;
				HandleTimeNodeChanged(candidateTN, candidateTN.Start);
			}
			//Moving Stop time
			else if(!candidateStart) {
				if(candidateTN.HasDrawings &&  pos < candidateTN.KeyFrame+10)
					candidateTN.StopFrame = candidateTN.KeyFrame+10;
				//Check not to go under start time nor 0
				else if(pos < frames && pos > candidateTN.StartFrame+10)
					candidateTN.StopFrame = (uint) pos;
				HandleTimeNodeChanged(candidateTN, candidateTN.Stop);
			}

			Gdk.Region region = GdkWindow.ClipRegion;
			GdkWindow.InvalidateRegion(region,true);
			GdkWindow.ProcessUpdates(true);

			return base.OnMotionNotifyEvent(evnt);
		}

		protected override bool OnButtonPressEvent(EventButton evnt)
		{
			if(evnt.Button == 1)
				ProcessButton1(evnt);
			// On Right button pressed
			else if(evnt.Button == 3)
				ProcessButton3(evnt.X);
			lastTime = evnt.Time;
			return base.OnButtonPressEvent(evnt);
		}

		protected override bool OnButtonReleaseEvent(EventButton evnt)
		{
			if(movingLimit) {
				movingLimit = false;
				candidateTN.Selected = false;
				GdkWindow.Cursor = new Gdk.Cursor(CursorType.Arrow);
				ReDraw();
			}
			return base.OnButtonReleaseEvent(evnt);
		}
	}
}
