//
//  Copyright (C) 2007-2009 Andoni Morales Alastruey
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
using LongoMatch.Interfaces;
using LongoMatch.Store;
using Newtonsoft.Json;

namespace LongoMatch.Store.Templates
{

	[Serializable]
	[JsonObject]
	public class SubCategoryTemplate: TagSubCategory, ITemplate<string>
	{

		public Version version;
		
		public SubCategoryTemplate() {
		}

		public Version Version {
			get;
			set;
		}
		public string AddDefaultItem (int index) {
			throw new Exception("Not implemented yet");
		}

		public void Save(string filePath) {
			SerializableObject.Save(this, filePath);
		}

		public static SubCategoryTemplate Load(string filePath) {
			return SerializableObject.LoadSafe<SubCategoryTemplate>(filePath);
		}
		
		public static SubCategoryTemplate DefaultTemplate (int not_used) {
			SubCategoryTemplate template = new SubCategoryTemplate();
			template.Version = new Version (Constants.DB_MAYOR_VERSION, Constants.DB_MINOR_VERSION);
			return template;
		}
	}
}
