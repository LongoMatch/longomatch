// FileData.cs
//
//  Copyright (C) 2007 Andoni Morales Alastruey
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
using System.Collections;
using System.Collections.Generic;
using Gtk;
using Gdk;

namespace LongoMatch
{
	
	[Serializable]
	public class FileData : IComparable
	{
		
		private MediaFile file;
		
		
		private string title;
				
		private string localName;

		private string visitorName;

		private int localGoals;

		private int visitorGoals;
		
		private DateTime matchDate;


		private List<TimeNode> 	dataSection1, dataSection2, dataSection3, dataSection4, dataSection5, dataSection6, dataSection7, dataSection8,
					dataSection9, dataSection10, dataSection11, dataSection12, dataSection13, dataSection14, dataSection15, dataSection16,
					dataSection17, dataSection18, dataSection19, dataSection20;

		private Sections sections;

		private List<TimeNode>[] dataSectionArray;
		
	
		
		public FileData(MediaFile file, String localName, String visitorName, int localGoals,
		                int visitorGoals, DateTime matchDate, Sections sections) {
			
			this.file = file;
			this.localName = localName;
			this.visitorName = visitorName;
			this.localGoals = localGoals;
			this.visitorGoals = visitorGoals;
			this.matchDate = matchDate;		
			this.sections = sections;
			dataSectionArray = new List<TimeNode>[20];

		
			dataSection1 = new List<TimeNode>();
			dataSectionArray[0] = dataSection1;
			dataSection2 = new List<TimeNode>();
			dataSectionArray[1] = dataSection2;
			dataSection3 = new List<TimeNode>();
			dataSectionArray[2] = dataSection3;
			dataSection4 = new List<TimeNode>();
			dataSectionArray[3] = dataSection4;
			dataSection5 = new List<TimeNode>();
			dataSectionArray[4] = dataSection5;
			dataSection6 = new List<TimeNode>();
			dataSectionArray[5] = dataSection6;
			dataSection7 = new List<TimeNode>();
			dataSectionArray[6] = dataSection7;
			dataSection8 = new List<TimeNode>();
			dataSectionArray[7] = dataSection8;
			dataSection9 = new List<TimeNode>();
			dataSectionArray[8] = dataSection9;
			dataSection10 = new List<TimeNode>();
			dataSectionArray[9] = dataSection10;
			dataSection11 = new List<TimeNode>();
			dataSectionArray[10] = dataSection11;
			dataSection12 = new List<TimeNode>();
			dataSectionArray[11] = dataSection12;
			dataSection13 = new List<TimeNode>();
			dataSectionArray[12] = dataSection13;
			dataSection14 = new List<TimeNode>();
			dataSectionArray[13] = dataSection14;
			dataSection15 = new List<TimeNode>();
			dataSectionArray[14] = dataSection15;
			dataSection16 = new List<TimeNode>();
			dataSectionArray[15] = dataSection16;
			dataSection17 = new List<TimeNode>();
			dataSectionArray[16] = dataSection17;
			dataSection18 = new List<TimeNode>();
			dataSectionArray[17] = dataSection18;
			dataSection19 = new List<TimeNode>();
			dataSectionArray[18] = dataSection19;
			dataSection20 = new List<TimeNode>();
			dataSectionArray[19] = dataSection20;
			
			
			this.Title = System.IO.Path.GetFileNameWithoutExtension(this.file.FilePath);
			
			System.IO.Directory.CreateDirectory(MainClass.ThumbnailsDir()+"/"+title);
			
			
			
	
		}
	
		public Sections Sections{
			get{ return this.sections;}
			set {this.sections = value;}
			
		}
		
		public string[] GetSectionsNames(){
			return sections.GetSectionsNames();
				
		}
		
		public Time[] GetSectionsStartTimes(){
			return sections.GetSectionsStartTimes();
		}
		
		public Time[] GetSectionsStopTimes(){
			return sections.GetSectionsStopTimes();
		}

		public MediaTimeNode AddTimeNode(int dataSection, Time start, Time stop,Pixbuf miniature) {
			MediaTimeNode tn ;
			string miniaturePath = null;
			List<TimeNode> al= dataSectionArray[dataSection];
			int count= al.Count+1;
			string name = sections.GetName(dataSection) + " " +count;
			if (miniature != null){
				miniaturePath = MainClass.ThumbnailsDir() + "/"+this.Title+"/"+"Section"+dataSection+"-"+name+
					"-"+start.ToMSecondsString()+"-"+stop.ToMSecondsString()+".jpg";				
				miniature.Save(miniaturePath,"jpeg");
			}
			tn = new MediaTimeNode(name, start, stop,this.file.Fps,dataSection,miniaturePath);
			dataSectionArray[dataSection].Add(tn);			
			return tn;

		}

		public void DelTimeNode(MediaTimeNode tNode) {
			dataSectionArray[tNode.DataSection].Remove(tNode);
			if (System.IO.File.Exists(tNode.MiniaturePath))
			    System.IO.File.Delete(tNode.MiniaturePath);
		}
		
		public TreeStore GetModel (){
			Gtk.TreeStore dataFileListStore = new Gtk.TreeStore (typeof (MediaTimeNode));
			for (int i=0;i<20;i++){
				if (this.Sections.GetVisibility(i)){
					Gtk.TreeIter iter = dataFileListStore.AppendValues (sections.GetTimeNode(i));
					foreach(MediaTimeNode tNode in dataSectionArray[i]){
						dataFileListStore.AppendValues (iter,tNode);
					}			
				}
			}
			return dataFileListStore;
		}

		public List<TimeNode>[] GetDataArray() {
			return dataSectionArray;
		}

		/*public String[] GetRoots() {
			return roots;
		}*/

		public MediaFile File {
			get{return file;}
			set{file=value;}
		}
		
	
		
		public String Title {
			get{return title;}
			set{title=value;}
		}
		
		public String LocalName {
			get{ return localName;}
			set{localName=value;}
		}
		
		public String VisitorName {
			get{ return visitorName;}
			set{visitorName=value;}
		}
		
		public int LocalGoals {
			get{ return localGoals;}
			set{localGoals=value;}
		}
		
		public int VisitorGoals {
			get{ return visitorGoals;}
			set{visitorGoals=value;}
		}
		
		
		public int VisibleSections {
			get{ return this.Sections.VisibleSections;}
			set{ this.Sections.VisibleSections=value;}
		}
		
		public DateTime MatchDate {
			get{ return matchDate;}
			set{ matchDate=value;}
		}

		public bool Equals(FileData fileData){
			return this.File.FilePath.Equals(fileData.File.FilePath);
		}
		
		public int CompareTo(object obj) {
			if(obj is FileData) {
				FileData fData = (FileData) obj;

				return this.File.FilePath.CompareTo(fData.File.FilePath);
			}
			else
				throw new ArgumentException("object is not a FileData and cannot be compared");    
		}
	}
}
