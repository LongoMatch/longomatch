//
//  Copyright (C) 2009 Andoni Morales Alastruey
//
//  This program is free software; you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation; either version 2 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program; if not, write to the Free Software
//  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//

using System;
using Gdk;

namespace LongoMatch.DB
{

	/// <summary>
	/// I'm used like a presentation card for projects. I speed up the retrieval
	/// from the database be using only the field required to describe a project
	/// </summary>
	public class ProjectDescription :  IComparable
	{
		private string file;

		private string localName;

		private string visitorName;

		private int localGoals;

		private int visitorGoals;

		private DateTime matchDate;

		private string season;

		private string competition;

		private Pixbuf preview;

		public ProjectDescription(string file, String localName, String visitorName, String season, String competition, int localGoals,
		                          int visitorGoals, DateTime matchDate,Pixbuf preview)
		{
			this.file = file;
			this.localName = localName;
			this.visitorName = visitorName;
			this.localGoals = localGoals;
			this.visitorGoals = visitorGoals;
			this.matchDate = matchDate;
			this.season = season;
			this.competition = competition;
			this.preview = preview;
		}

		public String Title {
			get {
				return System.IO.Path.GetFileNameWithoutExtension(file);
			}
		}

		public String File {
			get {
				return file;
			}
			set {
				file = value;
			}
		}

		public String Season {
			get {
				return season;
			}
			set {
				season = value;
			}
		}

		public String Competition {
			get {
				return competition;
			}
			set {
				competition= value;
			}
		}

		public String LocalName {
			get {
				return localName;
			}
			set {
				localName=value;
			}
		}

		public String VisitorName {
			get {
				return visitorName;
			}
			set {
				visitorName=value;
			}
		}

		public int LocalGoals {
			get {
				return localGoals;
			}
			set {
				localGoals=value;
			}
		}

		public int VisitorGoals {
			get {
				return visitorGoals;
			}
			set {
				visitorGoals=value;
			}
		}


		public DateTime MatchDate {
			get {
				return matchDate;
			}
			set {
				matchDate=value;
			}
		}

		public Pixbuf Preview {
			get {
				return this.preview;
			}
			set {
				preview = value;
			}
		}

		public int CompareTo(object obj) {
			if (obj is ProjectDescription) {
				ProjectDescription project = (ProjectDescription) obj;

				return this.File.CompareTo(project.File);
			}
			else
				throw new ArgumentException("object is not a ProjectDescription and cannot be compared");
		}
	}
}
