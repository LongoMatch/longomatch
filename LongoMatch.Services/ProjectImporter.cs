//
//  Copyright (C) 2015 vguzman
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
using LongoMatch.Core.Store;
using System.Collections.Generic;
using VAS.Core.Store;

namespace LongoMatch.Services
{
	public class ProjectImporter
	{
		public Func<Project> ImportFunction {
			get;
			set;
		}

		public string Description {
			get;
			set;
		}

		public string [] Extensions {
			get;
			set;
		}

		public string FilterName {
			get;
			set;
		}

		public bool NeedsEdition {
			get;
			set;
		}

		public bool CanOverwrite {
			get;
			set;
		}

		public bool Internal {
			get;
			set;
		}
	}
}

