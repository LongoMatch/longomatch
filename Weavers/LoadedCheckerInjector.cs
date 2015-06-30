using System.Linq;
using Mono.Cecil;

public class LoadedCheckerInjector
{
	ModuleDefinition moduleDefinition;
	public MethodDefinition CheckIsLoadedMethod;
	const string CHECK_IS_LOADED = "CheckIsLoaded";

	public LoadedCheckerInjector (ModuleDefinition moduleDefinition)
	{
		this.moduleDefinition = moduleDefinition;
	}

	public MethodReference Execute (TypeDefinition type)
	{
		CheckIsLoadedMethod = type.Methods.FirstOrDefault (IsCheckMethod);
		if (CheckIsLoadedMethod == null) {
			TypeDefinition baseType = type.BaseType.Resolve ();
			CheckIsLoadedMethod = baseType.Methods.FirstOrDefault (IsCheckMethod);
		}
		if (CheckIsLoadedMethod != null) {
			if (CheckIsLoadedMethod.IsStatic) {
				throw new WeavingException (CHECK_IS_LOADED + " method can no be static");
			}
			if (!CheckIsLoadedMethod.IsFamily) {
				throw new WeavingException (CHECK_IS_LOADED + " method needs to be protected");
			}
			return  moduleDefinition.Import (CheckIsLoadedMethod);
		} else {
			throw new WeavingException (CHECK_IS_LOADED + " method not found");
		}
	}

	bool IsCheckMethod (MethodDefinition method)
	{
		return method.Name == CHECK_IS_LOADED &&
		method.Parameters.Count == 0;
	}

}