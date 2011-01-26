// 
//  Copyright (C) 2011 Andoni Morales Alastruey
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
//  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301, USA.
// 
using System;
using System.Collections.Generic;
using LongoMatch.Common;

namespace LongoMatch.Store
{
	public class SubCategory
	{
		public SubCategory (){
			Options = new List<object>();
		}
		
		/// <summary>
		/// Name of the subcategory
		/// </summary>
		public String Name {
			get;
			set;
		}
		
		public List<object> Options {
			get;
			set;
		}
		
		public bool FastTag {
			get;
			set;
		}
	}
	
	public class TagSubCategory: SubCategory
	{
		public TagSubCategory (){
			Options = new List<string>();
		}
		
		public new List<string> Options {
			get;
			set;
		}
	}
	
	public class PlayerSubCategory: SubCategory
	{
		public PlayerSubCategory (){
			Options = new List<Team>();
		}
		
		public new List<Team> Options {
			get;
			set;
		}
	}
	
	public class TeamSubCategory: SubCategory
	{
		public TeamSubCategory (){
			Options = new List<Team>();
		}
		
		public new List<Team> Options {
			get;
			set;
		}
	}
}

