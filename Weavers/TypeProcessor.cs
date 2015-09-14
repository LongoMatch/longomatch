using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Mono.Collections.Generic;

public class TypeProcessor
{
	Action<string> logInfo;
	MethodReference isLoadedChecker;
	TypeDefinition type;

	public TypeProcessor (Action<string> logInfo, MethodReference isLoadedChecker, TypeDefinition type)
	{
		this.logInfo = logInfo;
		this.isLoadedChecker = isLoadedChecker;
		this.type = type;
	}

	public void Execute ()
	{
		logInfo ("\t" + type.FullName);

		foreach (var property in type.Properties) {
			if (property.Name == Loader.CHECK_IS_LOADED ||
			    property.Name == String.Format ("{0}.{1}", Loader.ISTORABLE, Loader.CHECK_IS_LOADED) ||
				property.Name == Loader.ID ||
				property.Name == String.Format ("{0}.{1}", Loader.ISTORABLE, Loader.ID)
			) {
				logInfo ("\tSkip property: " + property.Name);
				continue;
			}

			var getMethod = property.GetMethod;
			if (getMethod == null) {
				continue;
			}
			if (getMethod.IsPrivate) {
				continue;
			}
			if (getMethod.IsAbstract) {
				continue;
			}

			if (property.CustomAttributes.
				FirstOrDefault (a => a.AttributeType.Name == "JsonIgnoreAttribute") != null) {
				logInfo ("\tSkip ignored property: " + property.Name);
				continue;
			}

			if (property.CustomAttributes.
				FirstOrDefault (a => a.AttributeType.Name == "LongoMatchPropertyPreload") != null) {
				logInfo ("\tSkip preloaded property: " + property.Name);
				continue;
			}

			ProcessProperty (getMethod);
		}
	}

	void ProcessProperty (MethodDefinition method)
	{
		logInfo ("\tProcessing property: " + method.Name);
		var instructions = method.Body.Instructions;
		if (AlreadyContainsCheck (instructions)) {
			return;
		}
		method.Body.SimplifyMacros ();
		method.Body.Instructions.Insert (0, Instruction.Create (OpCodes.Callvirt, isLoadedChecker));
		method.Body.Instructions.Insert (0, Instruction.Create (OpCodes.Ldarg_0));
		method.Body.OptimizeMacros ();
	}

	static bool AlreadyContainsCheck (Collection<Instruction> instructions)
	{
		return instructions
            .Select (instruction => instruction.Operand)
            .OfType<MethodReference> ()
			.Any (operand => operand.Name == Loader.CHECK_IS_LOADED);
	}
}