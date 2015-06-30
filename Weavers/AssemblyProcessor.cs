using System;
using System.Collections.Generic;
using Mono.Cecil;

public class AssemblyProcessor
{
	LoadedCheckerInjector freezeCheckerInjector;
	ImplementsInterfaceFinder implementsInterfaceFinder;
	Action<string> logInfo;

	public AssemblyProcessor (LoadedCheckerInjector freezeCheckerInjector, ImplementsInterfaceFinder implementsInterfaceFinder, Action<string> logInfo)
	{
		this.freezeCheckerInjector = freezeCheckerInjector;
		this.implementsInterfaceFinder = implementsInterfaceFinder;
		this.logInfo = logInfo;
	}

	public void Execute (List<TypeDefinition> classes)
	{
		foreach (var type in classes) {
			var baseType = implementsInterfaceFinder.HierarchyImplementsIFreezable (type);
			if (baseType == null) {
				continue;
			}
			var checkMethod = freezeCheckerInjector.Execute (baseType);

			var typeProcessor = new TypeProcessor (logInfo, checkMethod, type);
			typeProcessor.Execute ();
		}
	}
}