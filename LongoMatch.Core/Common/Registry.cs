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
using System.Linq;
using System.Collections.Generic;

namespace LongoMatch.Core.Common
{
	public class Registry
	{
		Dictionary<Type, List<RegistryElement>> elements;
		string name;

		internal class RegistryElement
		{
			public Type type;
			public int priority;

			public RegistryElement (Type type, int priority)
			{
				this.type = type;
				this.priority = priority;
			}
		}

		public Registry (string name)
		{
			this.name = name;
			elements = new Dictionary<Type, List<RegistryElement>> ();
		}

		public virtual void Register<I, C> (int priority)
		{
			Type interfac = typeof(I);
			Type klass = typeof(C);
			if (!elements.ContainsKey (interfac)) {
				elements [interfac] = new List<RegistryElement> ();
			}
			elements [interfac].Add (new RegistryElement (klass, priority));
		}

		public virtual T Retrieve<T> (params object[] args)
		{
			Type interfac = typeof(T);
			Type elementType;

			if (!elements.ContainsKey (interfac)) {
				throw new Exception (String.Format ("No {0} available in the {0} registry",
					interfac, name));
			}
			elementType = elements [interfac].OrderByDescending (e => e.priority).First ().type;
			return (T)Activator.CreateInstance (elementType, args);
		}
	}
}
