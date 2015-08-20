//
//  Copyright (C) 2015 Fluendo S.A.
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
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;
using LongoMatch.Core.Common;

namespace LongoMatch.Core.Store
{
	[Serializable]
	[PropertyChanged.ImplementPropertyChanged]
	public class StorableBase: IStorable
	{

		public StorableBase ()
		{
			IsLoaded = true;
		}

		#region IStorable implementation

		[JsonIgnore]
		public virtual List<IStorable> Children {
			get {
				return null;
			}
		}

		[JsonIgnore]
		public bool IsLoaded {
			get;
			set;
		}

		[JsonIgnore]
		public IStorage Storage {
			get;
			set;
		}

		[JsonIgnore]
		bool IsLoading {
			get;
			set;
		}

		[JsonIgnore]
		public bool IsChanged {
			get;
			set;
		}

		protected virtual void CheckIsLoaded ()
		{
			if (!IsLoaded && !IsLoading) {
				IsLoading = true;
				if (Storage == null) {
					throw new StorageException ("Storage not set in preloaded object");
				}
				Storage.Fill (this);
				IsLoaded = true;
				IsLoading = false;
			}
		}

		#endregion

		#region IIDObject implementation

		[JsonProperty (Order = -100)]
		public virtual Guid ID {
			get;
			set;
		}

		#endregion
	}
}

