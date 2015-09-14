using System.Linq;
using Mono.Cecil;

public class StorableTypeFinder
{
	ModuleDefinition moduleDefinition;
	public TypeReference StorableType;
	public MethodReference GetIsLoadedMethod;
	IAssemblyResolver assemblyResolver;

	public StorableTypeFinder (ModuleDefinition moduleDefinition, IAssemblyResolver assemblyResolver)
	{
		this.moduleDefinition = moduleDefinition;
		this.assemblyResolver = assemblyResolver;
	}

	public void Execute ()
	{
		StorableType = moduleDefinition.Types.FirstOrDefault (x => x.Name == Loader.ISTORABLE);
		if (StorableType != null) {
			return;
		}
		foreach (var reference in moduleDefinition.AssemblyReferences) {
			var mainModule = assemblyResolver.Resolve (reference).MainModule;
			StorableType = mainModule.Types.FirstOrDefault (x => x.Name == Loader.ISTORABLE);
			if (StorableType != null) {
				return;
			}
		}
	}
}