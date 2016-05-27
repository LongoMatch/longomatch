using VAS.Core.Serialization;
using LongoMatch.Core.Migration;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
	/// <summary>
	/// Initializes the module.
	/// </summary>
	public static void Initialize ()
	{
		Serializer.TypesMappings = Mappings.TypesMappings;
		Serializer.NamespacesReplacements = Mappings.NamespacesReplacements;
	}
}