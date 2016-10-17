//
//  Copyright (C) 2016 Fluendo S.A.
//
//
using System;
namespace LongoMatch.Core.Services
{
	/// <summary>
	/// Attribute used to register IView components.
	/// </summary>
	[AttributeUsage (AttributeTargets.Class, AllowMultiple = true)]
	public class RegistryAttribute : Attribute
	{
		public RegistryAttribute (Type interfaceType, int priority)
		{
			InterfaceType = interfaceType;
			Priority = priority;
		}

		/// <summary>
		/// Gets or sets the type of the interface implemented.
		/// </summary>
		/// <value>The type of the interface implemented.</value>
		public Type InterfaceType { get; set; }

		/// <summary>
		/// Gets or sets the priority.
		/// </summary>
		/// <value>The priority.</value>
		public int Priority { get; set; }
	}
}
