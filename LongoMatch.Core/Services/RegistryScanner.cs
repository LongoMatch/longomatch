//
//  Copyright (C) 2016 Fluendo S.A.
//
//
using System;
using System.Reflection;

namespace LongoMatch.Core.Services
{
	public static class RegistryScanner
	{
		public static void Scan ()
		{
			Assembly assembly = Assembly.GetCallingAssembly ();
			foreach (Type type in assembly.GetTypes ()) {
				foreach (var attribute in type.GetCustomAttributes (typeof (RegistryAttribute), true)) {
					var regAttribute = (attribute as RegistryAttribute);
					App.Current.DependencyRegistry.Register (regAttribute.InterfaceType, type, regAttribute.Priority);
				}
			}
		}
	}
}
