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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LongoMatch.Core.Common;
using LongoMatch.Core.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LongoMatch.Core.Serialization
{
	/// <summary>
	/// Parses <see cref="IStorable"/> objects traversing all its children objects
	/// looking for changes using the IsChanged property.
	/// This can be used before storing an <see cref="IStorable"/> in the database to
	/// know which updates are really needed to persist the object and its children.
	/// </summary>
	public class ObjectChangedParser
	{

		internal int parsedCount;
		StorableNode current;
		internal List<object> parsed;
		IContractResolver resolver;
		Stack<object> stack;
		bool reset;

		public ObjectChangedParser ()
		{
		}

		/// <summary>
		/// Parse an <see cref="IStorable"/> listing all its children
		/// and the ones that changed.
		/// </summary>
		/// <param name="children">List of children.</param>
		/// <param name="changedStorables">List of storables with changes.</param>
		/// <param name="storable">The storable object to parse.</param>
		/// <param name="settings">The serialization settings.</param>
		/// <param name="reset">If set to <c>true</c> reset the IsChanged flag.</param>
		public bool Parse(out StorableNode parentNode, IStorable storable, JsonSerializerSettings settings,
			bool reset = true)
		{
			bool ret = ParseInternal (out parentNode, storable, settings, reset);
			if (ret && current != null) {
				Log.Error ("Stack should be empty");
				return false;
			}
			return ret;
		}

		internal bool ParseInternal(out StorableNode parentNode, IStorable value, JsonSerializerSettings settings,
			bool reset = true)
		{
			stack = new Stack<object> ();
			parentNode = new StorableNode (value);
			parsedCount = 0;
			resolver = settings.ContractResolver ?? new DefaultContractResolver ();
			this.reset = reset;
			try {
				CheckValue (value, parentNode);
				return true;
			} catch (Exception ex) {
				Log.Exception (ex);
				return false;
			}
		}

		void CheckValue (object value, StorableNode node = null)
		{
			IStorable storable;

			if (value == null) {
				return;
			}

			if (stack.Any (o => Object.ReferenceEquals (o, value))) {
				// Value in stack, return to avoid dependency cycles.
				return;
			}
			stack.Push (value);

			storable = value as IStorable;

			if (storable != null) {
				if (node == null) {
					node = new StorableNode (storable);
				}
				// Update parent and children relations
				if (current != null) {
					current.Children.Add (node);
				}
				node.Parent = current;
				current = node;
			}

			// Figure out the type of object we are dealing with and parse it accordingly.
			// Primitives are ignored (not being objects containers) and lists and dictionaries
			// are traversed through all their children.
			JsonContract valueContract = resolver.ResolveContract(value.GetType());
			if (valueContract is JsonObjectContract) {
				CheckObject (value, valueContract as JsonObjectContract);
			} else if (valueContract is JsonArrayContract) {
				CheckEnumerable (value as IEnumerable, valueContract as JsonArrayContract);
			} else if (valueContract is JsonDictionaryContract) {
				CheckEnumerable ((value as IDictionary).Values, valueContract as JsonArrayContract);
			} else {
				// Skip primitive value
			}

			// Now try to find orphaned objects and create nodes with the Deleted flags.
			// These objects are currently saved in the database and cached in IStorable.SavedChildren but
			// the current IStorable does not reference them anymore as real children.
			// We used this cache to find the orphaned children and mark them with the Deleted flag.
			if (storable != null) {
				if (storable.DeleteChildren && storable.SavedChildren != null) {
					var orphaned = storable.SavedChildren.Except (node.Children.Select (n => n.Storable));
					foreach (IStorable st in orphaned) {
						StorableNode onode = new StorableNode (st);
						onode.Deleted = true;
						CheckValue (st, onode);
					}
				}
				current = current.Parent;
			}
			stack.Pop ();
		}

		void CheckObject (object value, JsonObjectContract contract)
		{
			parsedCount++;
			// Traverse all properties in the same way the Json.NET serialized does,
			// by taking in account only the serializable properties and skipping JsonIgnore ones.
			for (int index = 0; index < contract.Properties.Count; index++)
			{
				JsonProperty property = contract.Properties[index];
				try
				{
					object memberValue;

					// Check if the object has the IsChanged flag and update the StorableNode
					// Also reset the flag if it's required.
					if (property.PropertyName == "IsChanged") {
						IValueProvider provider = property.ValueProvider;
						bool changed = (bool) provider.GetValue(value);
						if (changed) {
							if (!current.IsChanged) {
								current.IsChanged = true;
							}
							if (reset) {
								provider.SetValue (value, false);
							}
						}
					} else {
						if (!CalculatePropertyValues(value, property, out memberValue))
							continue;
						CheckValue(memberValue);
					}
				}
				catch (Exception ex)
				{
				}
			}
		}

		void CheckEnumerable (IEnumerable values, JsonArrayContract contract) {
			foreach (object value in values)
			{
				CheckValue (value);
			}
		}

		bool CalculatePropertyValues(object value, JsonProperty property, out object memberValue)
		{
			if (!property.Ignored && property.Readable)
			{
				memberValue = property.ValueProvider.GetValue(value);
				return true;
			} else {
				memberValue = null;
				return false;
			}
		}
	}
}

