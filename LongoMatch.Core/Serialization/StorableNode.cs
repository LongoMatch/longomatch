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
using LongoMatch.Core.Interfaces;
using System.Collections.Generic;

namespace LongoMatch.Core.Serialization
{
	public class StorableNode
	{
		public StorableNode (IStorable storable)
		{
			Storable = storable;
			IsChanged = false;
			Children = new List<StorableNode> ();
		}

		public IStorable Storable {
			get;
			set;
		}

		public StorableNode Parent {
			get;
			set;
		}

		public List<StorableNode> Children {
			get;
			set;
		}

		public bool IsChanged {
			get;
			set;
		}

		public bool Deleted {
			get;
			set;
		}

		public List<IStorable> OrphanChildren {
			get;
			set;
		}

		public bool HasChanges ()
		{
			if (IsChanged) {
				return true;
			}
			foreach (StorableNode child in Children) {
				if (child.HasChanges ()) {
					return true;
				}
			}
			return false;
		}

		public bool ParseTree (ref List<IStorable> storables, ref List<IStorable> changed)
		{
			if (storables == null) {
				storables = new List<IStorable> ();
			}
			if (changed == null) {
				changed = new List<IStorable> ();
			}
			if (IsChanged)
				changed.Add (Storable);
			storables.Add (Storable);
			foreach (StorableNode node in Children) {
				if (!node.ParseTree (ref storables, ref changed))
					return false;
			}
			return true;
		}
	}
}

