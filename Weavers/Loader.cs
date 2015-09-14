using System;
using System.Linq;
using Mono.Cecil;

public class Loader
{
	public Action<string> LogInfo { get; set; }

	public ModuleDefinition ModuleDefinition { get; set; }

	public IAssemblyResolver AssemblyResolver { get; set; }

	public const string CHECK_IS_LOADED = "CheckIsLoaded";
	public const string ID = "ID";
	public const string ISTORABLE = "IStorable";

	public Loader ()
	{
		LogInfo = s => {
		};
	}

	public void Execute ()
	{
		var storableTypeFinder = new StorableTypeFinder (ModuleDefinition, AssemblyResolver);
		storableTypeFinder.Execute ();

		var exceptionFinder = new ExceptionFinder (ModuleDefinition, AssemblyResolver);
		exceptionFinder.Execute ();

		var checkLoadedInjector = new LoadedCheckerInjector (ModuleDefinition);

		var typeResolver = new TypeResolver ();
		var implementsInterfaceFinder = new ImplementsInterfaceFinder (typeResolver);

		var classes = ModuleDefinition.GetTypes ()
            .Where (x => x.IsClass)
            .ToList ();
		var assemblyProcessor = new AssemblyProcessor (checkLoadedInjector, implementsInterfaceFinder, LogInfo);
		assemblyProcessor.Execute (classes);
	}
}