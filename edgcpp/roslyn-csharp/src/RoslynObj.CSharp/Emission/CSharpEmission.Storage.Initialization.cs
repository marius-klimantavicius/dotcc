using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly HashSet<VariableSymbol> staticLocals = [];
        private readonly HashSet<Initialization> staticInitializers = [];

        private bool IsStaticLocalCandidate(VariableSymbol variable)
            => variable.InitializationKind.Name == "initk_function_local" || variable.LocalStaticInitializations.Count != 0 ||
                variable.Details.StorageDuration?.Name == "static" && currentFunction?.Body?.Locals.Contains(variable) == true;

        // Static constant recipes contribute to the class initialization, never to
        // the per-call local declarations or executable initializer occurrences.
        private bool TryAdmitStaticLocal(VariableSymbol variable)
        {
            if (staticLocals.Contains(variable)) return true;
            if (!IsStaticLocalCandidate(variable)) return false;
            if (variable.IsThreadLocal || variable.StandardThreadLocalEffective || variable.ExtensionThreadLocal ||
                variable.Details.StorageDuration?.Name == "thread")
                throw Limit("Thread-local storage requires its selected thread initialization contract.", "storage");
            if (variable.Details.StorageDuration?.Name != "static" || variable.InitializationKind.Name is not ("initk_function_local" or "initk_static" or "initk_none"))
                throw Limit("A local-static recipe lacks the captured static-duration/function-local association.", "storage");
            if (variable.CleanupRoutine is not null || variable.Details.IsWeak || variable.Details.IsCommon ||
                variable.Details.IsGnuAlias || variable.Details.IsWeakref || variable.Details.AliasedVariable is not null)
                throw Limit("A local static has cleanup or unresolved weak/common/alias obligations.", "storage");
            if (variable.LocalStaticInitializations.Count == 0 && variable.InitializationKind.Name == "initk_none")
            {
                if (variable.SymbolDetails.Storage?.Name != "sc_static" || variable.Details.Initializer is not null ||
                    variable.Details.ConstantInitializer is not null || Unwrap(variable.Type) is not RecordType)
                    throw Limit("An initializer-free local static requires its captured ordinary static record storage.", "initialization");
                AddStorage(variable, variable.Type, null, variable.Name ?? "static_local");
                staticLocals.Add(variable);
                return true;
            }
            if (variable.LocalStaticInitializations.Count == 0 && variable.InitializationKind.Name == "initk_static")
            {
                var constant = variable.Details.ConstantInitializer;
                var acceptedValue = constant?.Value is IntegerValue || constant?.Value is StringValue && Unwrap(variable.Type) is ArrayType;
                if (variable.Details.Initializer is not null || !acceptedValue)
                    throw Limit("A recipe-free local static requires its retained integer or target-byte array constant initializer.", "initialization");
                AddStorage(variable, variable.Type, variable.Details.ConstantInitializer, variable.Name ?? "static_local");
                staticLocals.Add(variable);
                return true;
            }
            if (variable.LocalStaticInitializations.Count != 1)
                throw Limit("A local static requires exactly one retained initialization recipe.", "initialization");
            var recipe = variable.LocalStaticInitializations[0];
            if (!recipe.Variable.Equals(variable) || recipe.Kind.Name is not ("initk_static" or "initk_none") ||
                recipe.Lifetime?.Cleanups.Count > 0)
                throw Limit("Dynamic or cleanup-bearing local-static initialization is unsupported.", "initialization");
            var initializations = new[] { variable.Details.Initializer, recipe.Initialization }
                .OfType<Initialization>().Distinct().ToArray();
            if (initializations.Length > 1)
                throw Limit("A local static has multiple distinct retained initializer recipes.", "initialization");
            var constants = new[] { variable.Details.ConstantInitializer, recipe.Constant,
                    initializations.SingleOrDefault()?.Constant }.OfType<Constant>().Distinct().ToArray();
            if (constants.Length > 1 || recipe.Kind.Name == "initk_static" && constants.Length != 1)
                throw Limit("A static initializer requires one exact retained constant recipe.", "initialization");
            foreach (var initialization in initializations)
            {
                if (initialization.Kind.Name != "dik_constant" || initialization.Constant is null ||
                    initialization.Expression is not null || initialization.Constructor is not null || initialization.Destructor is not null ||
                    initialization.Target is not null && !initialization.Target.Equals(variable) ||
                    initialization.Lifetime?.Cleanups.Count > 0 || initialization.InitExpressionLifetime?.Cleanups.Count > 0 ||
                    initialization.Details.IsConditional || initialization.Details.IsConditionalGuard ||
                    initialization.Details.IsReusedValue || initialization.Details.ConstantNonconstant ||
                    initialization.NextInDestructionList is not null)
                    throw Limit("A local-static executable recipe is not an unconditional cleanup-free constant.", "initialization");
            }
            AddStorage(variable, variable.Type, constants.SingleOrDefault(), variable.Name ?? "static_local");
            staticLocals.Add(variable);
            foreach (var initialization in initializations) staticInitializers.Add(initialization);
            return true;
        }

        private bool IsAllocatedStaticLocal(VariableSymbol variable) => staticLocals.Contains(variable);

        private void ValidateStaticDeclaration(DeclarationOperation declaration)
        {
            if (!declaration.HasStaticOrThreadVariable) return;
            var variables = declaration.Entities.OfType<VariableSymbol>().ToArray();
            if (variables.Length == 0 || !variables.Any(IsStaticLocalCandidate))
                throw Limit("A static declaration has no retained selected local-static variable.", "storage");
            foreach (var variable in variables)
                if (variable.IsThreadLocal || variable.Details.StorageDuration?.Name == "thread" ||
                    IsStaticLocalCandidate(variable) && !IsAllocatedStaticLocal(variable))
                    throw Limit("A declaration has an unadmitted static/thread initialization obligation.", "storage");
        }

        private bool TryStaticInitialization(Initialization initialization)
        {
            if (initialization.Target is not VariableSymbol variable || !IsAllocatedStaticLocal(variable)) return false;
            if (!staticInitializers.Contains(initialization))
                throw Limit("A static variable has an executable initializer outside its admitted constant recipe.", "initialization");
            return true;
        }
    }
}
