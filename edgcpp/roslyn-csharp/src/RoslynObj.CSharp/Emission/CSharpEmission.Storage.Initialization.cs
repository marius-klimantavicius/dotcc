using RoslynObj;

namespace RoslynObj.CSharp;

public sealed partial class CSharpEmitter
{
    private sealed partial class Emission
    {
        private readonly HashSet<VariableSymbol> staticLocals = [];
        private readonly HashSet<Initialization> staticInitializers = [];

        private static bool HasThreadStorage(VariableSymbol variable)
            => variable.IsThreadLocal || variable.StandardThreadLocalDeclared || variable.StandardThreadLocalEffective ||
                variable.ExtensionThreadLocal || variable.Details.StorageDuration?.Name == "thread";

        private bool TryAdmitConstantThreadStorage(VariableSymbol variable)
        {
            if (!HasThreadStorage(variable)) return false;
            var details = variable.Details;
            if (variable.SourceCorrespondence?.EnclosingRoutine is not null || details.PromotedLocalStatic ||
                details.PromotedLocalStaticInitialization || variable.LocalStaticInitializations.Count != 0 ||
                variable.InitializationKind.Name == "initk_function_local")
                throw Limit("Thread-local function storage requires its retained local initialization/guard contract.", "thread-initialization");
            if (details.ThreadInitializationRoutine is not null || details.ThreadWrapperRoutine is not null ||
                details.DynamicInitializationRoutine is not null)
                throw Limit("Thread-local storage has a retained initialization/wrapper routine; its dynamic per-thread execution contract is unsupported.", "thread-initialization");
            if (variable.CleanupRoutine is not null || details.ExtendsLifetime)
                throw Limit("Thread-local storage has thread-exit cleanup/lifetime obligations; thread-exit destruction is unsupported.", "thread-initialization");
            if (details.StorageDuration?.Name != "thread" || variable.InitializationKind.Name is not ("initk_static" or "initk_zero"))
                throw Limit("Thread-local storage requires its captured thread duration and zero/constant initialization kind.", "thread-initialization");
            if (!IsConstantThreadStorageType(variable.Type))
                throw Limit("Thread-local constant storage supports arithmetic values, ordinary pointers and arrays of those values; class lifetime obligations require a separate contract.", "thread-initialization");
            if (details.InitializationRewrittenAsAssignment || variable.InitializationKind.Name == "initk_zero" &&
                (details.HasExplicitInitializer || details.Initializer is not null || details.ConstantInitializer is not null))
                throw Limit("Thread-local zero storage cannot omit an explicit or rewritten initializer's per-thread execution.", "thread-initialization");
            if (details.IsWeak || details.IsCommon || details.IsGnuAlias || details.IsWeakref || details.AliasedVariable is not null ||
                details.ComdatGroup is not null)
                throw Limit("Thread-local storage requires an explicit weak/common/alias/COMDAT selection policy.", "link-policy");
            var initializer = details.Initializer;
            if (initializer is not null && (initializer.Kind.Name != "dik_constant" || initializer.Constant is null ||
                initializer.Expression is not null || initializer.Constructor is not null || initializer.Destructor is not null ||
                initializer.Target is not null && !initializer.Target.Equals(variable) ||
                initializer.Lifetime?.Cleanups.Count > 0 || initializer.InitExpressionLifetime?.Cleanups.Count > 0 ||
                initializer.Details.IsConditional || initializer.Details.IsConditionalGuard || initializer.Details.IsReusedValue ||
                initializer.Details.ConstantNonconstant || initializer.NextInDestructionList is not null))
                throw Limit("Thread-local storage requires an unconditional cleanup-free retained constant initializer.", "thread-initialization");
            var constants = new[] { details.ConstantInitializer, initializer?.Constant }.OfType<Constant>().Distinct().ToArray();
            if (constants.Length > 1 || variable.InitializationKind.Name == "initk_static" && constants.Length != 1)
                throw Limit("Thread-local constant initialization requires one exact retained constant value.", "thread-initialization");
            AddStorage(variable, variable.Type, constants.SingleOrDefault(), variable.Name ?? "thread_storage");
            return true;
        }

        private static bool IsConstantThreadStorageType(CppType type)
            => Unwrap(type) switch
            {
                ArithmeticType => true,
                PointerType pointer => !pointer.IsReference && !pointer.IsRValueReference,
                ArrayType array => IsConstantThreadStorageType(array.ElementType),
                _ => false
            };

        private bool IsStaticLocalCandidate(VariableSymbol variable)
            => IsSelectedConstantStorageCompanion(variable) || variable.InitializationKind.Name == "initk_function_local" || variable.LocalStaticInitializations.Count != 0 ||
                variable.Details.StorageDuration?.Name == "static" && currentFunction?.Body?.Locals.Contains(variable) == true;

        // Static constant recipes contribute to the class initialization, never to
        // the per-call local declarations or executable initializer occurrences.
        private bool TryAdmitStaticLocal(VariableSymbol variable)
        {
            if (staticLocals.Contains(variable)) return true;
            if (TryAdmitSelectedConstantStorageCompanion(variable)) return true;
            if (!IsStaticLocalCandidate(variable)) return false;
            if (HasThreadStorage(variable))
                throw Limit("Thread-local function storage requires its retained local initialization/guard contract.", "thread-initialization");
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
