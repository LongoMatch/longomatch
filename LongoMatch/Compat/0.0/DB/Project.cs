// Project.cs
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
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
//

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using Gtk;
using Gdk;
using LongoMatch.Compat.v00.TimeNodes;

namespace LongoMatch.Compat.v00.DB
{
	
	[Serializable]
	public class Project : IComparable
	{
		
		private MediaFile file;
		
		
		private string title;
				
		private string localName;

		private string visitorName;

		private int localGoals;

		private int visitorGoals;
		
		private DateTime matchDate;
		
		private Sections sections;

		//This field is not used but must be kept for DataBase compatibility
		private List<MediaTimeNode>[] dataSectionArray;
		
	
		
		public Project(MediaFile file, String localName, String visitorName, int localGoals,
		                int visitorGoals, DateTime matchDate, Sections sections) {
			List<MediaTimeNode> tnArray;
			
			this.file = file;
			this.localName = localName;
			this.visitorName = visitorName;
			this.localGoals = localGoals;
			this.visitorGoals = visitorGoals;
			this.matchDate = matchDate;		
			this.sections = sections;
			dataSectionArray = new List<MediaTimeNode>[20];
			
			for (int i=0;i<20;i++){
				tnArray = new List<MediaTimeNode>();
				dataSectionArray[i]=tnArray;
			}			
			this.Title = System.IO.Path.GetFileNameWithoutExtension(this.file.FilePath);			

		}
	
		public Sections Sections{
			get{ return this.sections;}
			set {this.sections = value;}
			
		}

		public List<MediaTimeNode>[] GetDataArray() {
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
	
		
		public DateTime MatchDate {
			get{ return matchDate;}
			set{ matchDate=value;}
		}

		public bool Equals(Project project){
			return this.File.FilePath.Equals(project.File.FilePath);
		}
		
		public int CompareTo(object obj) {
			if(obj is Project) {
				Project project = (Project) obj;

				return this.File.FilePath.CompareTo(project.File.FilePath);
			}
			else
				throw new ArgumentException("object is not a Project and cannot be compared");    
		}
	}
}
