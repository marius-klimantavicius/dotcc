using System.Collections.ObjectModel;

namespace RoslynObj.CSharp;

internal sealed record ManagedProgramUnitSelection(int UnitOrdinal, ManagedSelectionPlan Selection);
internal sealed record ManagedProgramFunctionBinding(FunctionSymbol Declaration, int DeclarationUnitOrdinal,
    FunctionSymbol Definition, int DefinitionUnitOrdinal, string LinkageName, AbiCompatibilityResult Compatibility);
internal sealed record ManagedProgramVariableBinding(VariableSymbol Declaration, int DeclarationUnitOrdinal,
    VariableSymbol Definition, int DefinitionUnitOrdinal, string LinkageName, AbiCompatibilityResult Compatibility);

/// <summary>Only current-open public identities. Exact external binding is not C++ type/ODR validation.</summary>
internal sealed record ManagedProgramSelectionPlan(
    IReadOnlyList<ManagedSelectionPlan> Units,
    IReadOnlyList<ManagedProgramUnitSelection> UnitSelections,
    IReadOnlyDictionary<FunctionSymbol, FunctionSymbol> FunctionDefinitions,
    IReadOnlyDictionary<VariableSymbol, VariableSymbol> VariableDefinitions,
    IReadOnlyList<ManagedProgramFunctionBinding> FunctionBindings,
    IReadOnlyList<ManagedProgramVariableBinding> VariableBindings)
{
    internal ManagedSelectionPlan? GetUnit(RoslynObject source)
        => Units.SingleOrDefault(unit => ReferenceEquals(unit.Source, source));
}

/// <summary>Combines bounded per-unit selection through required exact-linkage external declarations only.</summary>
internal static class ManagedProgramSelectionPlanner
{
    internal static ManagedProgramSelectionPlan Select(IReadOnlyList<RoslynObject> inputs,
        IReadOnlyList<string> inputSha256, ManagedDefinitionRoot[] roots, ResourceBudgets budgets,
        CancellationToken token = default, Action? checkBudget = null, ManagedStartupConfiguration? startup = null)
    {
        var work = new Work(inputs, inputSha256, roots, budgets, token, checkBudget, startup);
        try { return work.Run(); }
        catch (ManagedSelectionException exception)
        {
            exception.SelectionReport = work.BuildFailureReport(exception.SelectionReport);
            throw;
        }
    }

    private sealed class Work(IReadOnlyList<RoslynObject> inputs, IReadOnlyList<string> hashes,
        ManagedDefinitionRoot[] roots, ResourceBudgets budgets, CancellationToken token, Action? checkBudget,
        ManagedStartupConfiguration? startup)
    {
        private readonly AbiCompatibility compatibility = new();
        private readonly Dictionary<FunctionSymbol, FunctionSymbol> functionDefinitions = [];
        private readonly Dictionary<VariableSymbol, VariableSymbol> variableDefinitions = [];
        private readonly List<ManagedProgramFunctionBinding> functionBindings = [];
        private readonly List<ManagedProgramVariableBinding> variableBindings = [];
        private readonly HashSet<FunctionSymbol> examinedFunctions = [];
        private readonly HashSet<VariableSymbol> examinedVariables = [];
        private readonly Dictionary<(int Unit, string Lookup, string Linkage), Symbol[]> candidateCache = [];
        private UnitState[] units = [];
        private long work;

        internal ManagedProgramSelectionPlan Run()
        {
            Tick();
            if (inputs.Count < 1 || inputs.Count > budgets.MaximumOpenUnits || hashes.Count != inputs.Count ||
                roots.Length == 0 && startup?.Definitions.Length is not > 0)
                Fail("ROC1220", "Managed program selection requires a finite explicit pinned input set within its open-unit budget and managed roots.");
            if (hashes.Any(hash => hash.Length != 64 || !hash.All(Uri.IsHexDigit)) ||
                hashes.Distinct(StringComparer.OrdinalIgnoreCase).Count() != hashes.Count ||
                inputs.Select(input => input.Manifest.ArtifactIdentity).Distinct(StringComparer.Ordinal).Count() != inputs.Count)
                Fail("ROC1220", "Managed program inputs require distinct immutable hashes and artifact identities; implicit deduplication is unsupported.");
            if (startup is not null && (startup.Contract != "edg7-itanium-explicit-startup-atexit-v1" || startup.Definitions.Length == 0 ||
                !startup.UnitInputOrder.SequenceEqual(hashes, StringComparer.OrdinalIgnoreCase)))
                Fail("ROC1220", "Explicit startup requires its admitted lifecycle policy and complete target input-order contract.");
            if (inputs.Any(input => input.Manifest.Target != inputs[0].Manifest.Target || input.Manifest.CaptureProfile != "native_lowered"))
                Fail("ROC1220", "Managed program inputs must retain matching captured targets and the admitted native_lowered profile.");
            units = inputs.Select((input, index) => new UnitState(input, hashes[index], index + 1,
                roots.Where(root => root.InputSha256.Equals(hashes[index], StringComparison.OrdinalIgnoreCase)).ToArray())).ToArray();
            if (roots.Concat(startup?.Definitions ?? []).Any(root => hashes.Count(hash => hash.Equals(root.InputSha256, StringComparison.OrdinalIgnoreCase)) != 1))
                Fail("ROC1220", "Each managed root must identify exactly one explicit current input hash.");

            var rounds = 0;
            while (units.FirstOrDefault(unit => unit.Dirty) is { } next)
            {
                Tick();
                if (++rounds > (long)budgets.MaximumSelectedFunctions + budgets.MaximumSelectedStorage + inputs.Count)
                    Fail("ROC1221", "Managed program fixed-point iteration bound exceeded.");
                next.Dirty = false;
                var plan = ManagedSelectionPlanner.SelectSeeded(next.Source, next.Roots,
                    next.FunctionSeeds, next.VariableSeeds, next.Hash, next.Ordinal, budgets, token, checkBudget, startup);
                next.Plan = plan;
                CheckAggregateBounds();
                CheckSelectedDefinitions();
                foreach (var function in plan.Functions)
                {
                    Tick();
                    if (!examinedFunctions.Add(function) || IsDefinition(function) || function.BuiltinKind is not null || !External(function)) continue;
                    BindFunction(next, function);
                }
                foreach (var variable in plan.Variables)
                {
                    Tick();
                    if (!examinedVariables.Add(variable) || IsDefinition(variable) || !External(variable)) continue;
                    BindVariable(next, variable);
                }
            }
            Tick();
            var active = units.Where(unit => unit.Plan is not null).ToArray();
            return new(active.Select(unit => unit.Plan!).ToArray(),
                active.Select(unit => new ManagedProgramUnitSelection(unit.Ordinal, unit.Plan!)).ToArray(),
                new ReadOnlyDictionary<FunctionSymbol, FunctionSymbol>(functionDefinitions),
                new ReadOnlyDictionary<VariableSymbol, VariableSymbol>(variableDefinitions),
                functionBindings.ToArray(), variableBindings.ToArray());
        }

        internal ManagedSelectionReport BuildFailureReport(ManagedSelectionReport? failedUnit)
        {
            var rows = units.Where(unit => unit.Plan is not null)
                .Select(unit => new ManagedUnitSelectionReport(unit.Ordinal, unit.Source.Manifest.ArtifactIdentity, unit.Hash, unit.Plan!.Report)).ToList();
            if (failedUnit is not null)
            {
                var failed = units.FirstOrDefault(unit => unit.Hash.Equals(failedUnit.InputSha256, StringComparison.OrdinalIgnoreCase));
                if (failed is not null)
                {
                    rows.RemoveAll(row => row.UnitOrdinal == failed.Ordinal);
                    rows.Add(new(failed.Ordinal, failed.Source.Manifest.ArtifactIdentity, failed.Hash, failedUnit with { ClosureComplete = false }));
                }
            }
            rows.Sort((left, right) => left.UnitOrdinal.CompareTo(right.UnitOrdinal));
            return new()
            {
                Scope = "partial-failed-multi-input-selection", ClosureComplete = false, UnitSelections = rows,
                ExecutableBodiesVisited = rows.Sum(row => row.Selection.ExecutableBodiesVisited),
                RequiredDeclarationCount = rows.Sum(row => row.Selection.RequiredDeclarationCount),
                StorageDefinitionCount = rows.Sum(row => row.Selection.StorageDefinitionCount),
                RequiredVariableCount = rows.Sum(row => row.Selection.RequiredVariableCount),
                ScopedTypeCount = rows.Sum(row => row.Selection.ScopedTypeCount),
                RequiredConstantCount = rows.Sum(row => row.Selection.RequiredConstantCount),
                BodyOperationCount = rows.Sum(row => row.Selection.BodyOperationCount), SelectionWorkCount = work,
                AmbientRootRowsInspected = rows.Sum(row => row.Selection.AmbientRootRowsInspected),
                AmbientRoutineCandidatesInspected = rows.Sum(row => row.Selection.AmbientRoutineCandidatesInspected),
                Roots = rows.SelectMany(row => row.Selection.Roots).ToArray(), StartupRoots = rows.SelectMany(row => row.Selection.StartupRoots).ToArray(),
                StartupExecutionContract = startup?.Contract,
                UnvisitedInput = "Only the retained partial per-input selections are reported. Cross-input binding and the combined executable closure failed; no program completeness or publication is claimed.",
                AmbientInitialization = "Partial per-input ambient observations do not establish a completed combined initialization or teardown closure."
            };
        }

        private void Tick()
        {
            token.ThrowIfCancellationRequested();
            var limit = (long)budgets.MaximumSelectedFunctions * budgets.MaximumSelectionOperationsPerBody +
                budgets.MaximumSelectedStorage + budgets.MaximumSelectedTypes;
            if (++work > limit) Fail("ROC1221", "Managed program orchestration work bound exceeded.");
            if ((work & 31) == 1) checkBudget?.Invoke();
        }

        private void CheckAggregateBounds()
        {
            var plans = units.Where(unit => unit.Plan is not null).Select(unit => unit.Plan!).ToArray();
            if (plans.Sum(plan => (long)plan.Functions.Count) > budgets.MaximumSelectedFunctions ||
                plans.Sum(plan => (long)plan.Variables.Count) > budgets.MaximumSelectedStorage ||
                plans.Sum(plan => (long)plan.Types.Count) > budgets.MaximumSelectedTypes)
                Fail("ROC1221", "The combined managed program exceeds its global function, variable/storage or scoped-type bound.");
        }

        private void CheckSelectedDefinitions()
        {
            // Only already-selected identities participate. No unit-wide ODR or
            // definition scan is performed, and local helpers retain TU identity.
            var required = units.Where(unit => unit.Plan is not null)
                .SelectMany(unit => unit.Plan!.Functions.Cast<Symbol>().Concat(unit.Plan.Variables)
                    .Where(External).Select(symbol => (Symbol: symbol, Name: LinkName(symbol)))).ToArray();
            foreach (var group in required.Where(item => item.Name is not null).GroupBy(item => item.Name!, StringComparer.Ordinal))
            {
                Tick();
                if (group.Select(item => item.Symbol is FunctionSymbol ? "function" : "variable").Distinct(StringComparer.Ordinal).Count() > 1)
                    Fail("ROC1222", "One required captured external linkage has both function and data requirements: " + group.Key);
            }
            var selected = units.Where(unit => unit.Plan is not null)
                .SelectMany(unit => unit.Plan!.Definitions.Cast<Symbol>().Concat(unit.Plan.StorageDefinitions)
                    .Where(External).Select(symbol => (unit.Ordinal, Symbol: symbol, Name: LinkName(symbol)))).ToArray();
            foreach (var group in selected.Where(item => item.Name is not null).GroupBy(item => item.Name!, StringComparer.Ordinal))
            {
                Tick();
                if (group.Select(item => item.Symbol is FunctionSymbol ? "function" : "variable").Distinct(StringComparer.Ordinal).Count() > 1)
                    Fail("ROC1222", "One required captured external linkage names both function and data definitions: " + group.Key);
                if (group.Select(item => item.Symbol).Distinct().Count() > 1)
                    Fail("ROC1222", "Duplicate selected external definitions require an explicit unsupported selection policy: " + group.Key +
                        "; " + string.Join("; ", group.Select(item => item.Symbol is FunctionSymbol function
                            ? $"unit {item.Ordinal}: kind={function.SpecialFunctionKind?.Name}, entry={function.ConstructorOrDestructorKind?.Name}, weak={function.Details.IsWeak}, COMDAT={function.Details.IsUseComdat}, inline={function.Details.IsInline}, alias-entry={function.Details.IsAliasEntry}, primary={function.PrimaryEntry?.CurrentName}, body={function.BodyState.Name}"
                            : $"unit {item.Ordinal}: variable definition")));
            }
        }

        private void BindFunction(UnitState source, FunctionSymbol declaration)
        {
            var name = LinkName(declaration);
            var lookup = declaration.SourceCorrespondence?.SourceName;
            // No source spelling is synthesized for unnamed or fabricated helpers.
            // They remain unresolved required declarations for ordinary provider admission.
            if (name is null || string.IsNullOrEmpty(lookup)) return;
            ValidatePolicy(declaration, name);
            var candidates = Candidates(lookup, name);
            CheckKinds(candidates, true, name);
            var definitions = candidates.Where(candidate => candidate.Symbol is FunctionSymbol function && IsDefinition(function)).ToArray();
            if (definitions.Length > 1) Fail("ROC1222", "Required declaration has duplicate exact captured external definitions: " + name);
            if (definitions.Length == 0) return;
            var target = definitions[0]; var definition = (FunctionSymbol)target.Symbol;
            ValidatePolicy(definition, name);
            Tick();
            var result = compatibility.CompareFunctions(declaration, definition);
            if (!result.Compatible) Fail("ROC1223", $"Selected function ABI representation is {result.Status} at {result.Path}: {result.Reason} ({name}).");
            functionDefinitions.Add(declaration, definition);
            functionBindings.Add(new(declaration, source.Ordinal, definition, target.Unit.Ordinal, name, result));
            if (target.Unit.FunctionSeedSet.Add(definition))
            {
                target.Unit.FunctionSeeds.Add(definition);
                target.Unit.Dirty = true;
            }
        }

        private void BindVariable(UnitState source, VariableSymbol declaration)
        {
            var name = LinkName(declaration); var lookup = declaration.SourceCorrespondence?.SourceName;
            if (name is null || string.IsNullOrEmpty(lookup)) return;
            ValidatePolicy(declaration, name);
            var candidates = Candidates(lookup, name);
            CheckKinds(candidates, false, name);
            var definitions = candidates.Where(candidate => candidate.Symbol is VariableSymbol variable && IsDefinition(variable)).ToArray();
            if (definitions.Length > 1) Fail("ROC1222", "Required storage declaration has duplicate exact captured external definitions: " + name);
            if (definitions.Length == 0) return;
            var target = definitions[0]; var definition = (VariableSymbol)target.Symbol;
            ValidatePolicy(definition, name);
            Tick();
            var result = compatibility.CompareStorage(declaration, definition);
            if (!result.Compatible) Fail("ROC1223", $"Selected storage ABI representation is {result.Status} at {result.Path}: {result.Reason} ({name}).");
            variableDefinitions.Add(declaration, definition);
            variableBindings.Add(new(declaration, source.Ordinal, definition, target.Unit.Ordinal, name, result));
            if (target.Unit.VariableSeedSet.Add(definition))
            {
                target.Unit.VariableSeeds.Add(definition);
                target.Unit.Dirty = true;
            }
        }

        private (UnitState Unit, Symbol Symbol)[] Candidates(string lookup, string linkage)
        {
            var result = new List<(UnitState, Symbol)>();
            foreach (var unit in units)
            {
                Tick();
                if (!candidateCache.TryGetValue((unit.Ordinal, lookup, linkage), out var matches))
                {
                    var indexed = unit.Source.TranslationUnit.SemanticModel.FindSymbols(lookup).Take(budgets.MaximumSelectionCandidates + 1).ToArray();
                    token.ThrowIfCancellationRequested(); checkBudget?.Invoke();
                    if (indexed.Length > budgets.MaximumSelectionCandidates)
                        Fail("ROC1221", "Managed program indexed candidate budget plus overflow sentinel exceeded; uniqueness is unknown for " + lookup);
                    matches = indexed.Where(symbol => (symbol is FunctionSymbol or VariableSymbol) && External(symbol) && LinkName(symbol) == linkage).ToArray();
                    candidateCache.Add((unit.Ordinal, lookup, linkage), matches);
                }
                result.AddRange(matches.Select(symbol => (unit, symbol)));
            }
            return result.ToArray();
        }

        private static void CheckKinds((UnitState Unit, Symbol Symbol)[] candidates, bool function, string linkage)
        {
            if (candidates.Any(candidate => (candidate.Symbol is FunctionSymbol) != function))
                Fail("ROC1222", "A required captured external linkage has conflicting function/data candidates: " + linkage);
        }

        private static bool External(Symbol symbol) => symbol.SourceCorrespondence?.Linkage.Name is
            "nlk_external" or "nlk_cplusplus_external" or "external" or "c" or "cpp";

        private static string? LinkName(Symbol symbol)
        {
            var correspondence = symbol.SourceCorrespondence;
            if (correspondence is null || !External(symbol)) return null;
            var assembly = symbol switch { FunctionSymbol function => function.GnuSupplement?.AssemblyName, VariableSymbol variable => variable.AssemblyName, _ => null };
            if (!string.IsNullOrEmpty(assembly)) return assembly;
            if (string.IsNullOrEmpty(correspondence.CurrentName) ||
                correspondence.Linkage.Name is "nlk_cplusplus_external" or "cpp" && correspondence.NameHasBeenMangled != true) return null;
            return correspondence.CurrentName;
        }

        private static bool IsDefinition(FunctionSymbol function) => function.IsDefined && function.BodyState.Name == "definition" &&
            function.Body is { Root.RepresentationRole.Name: "execution" };

        private static bool IsDefinition(VariableSymbol variable) => variable.SymbolDetails.Storage?.Name != "sc_extern" &&
            !variable.Details.IsParameter && variable.Details.StorageDuration?.Name is not ("automatic" or "parameter") &&
            !variable.Details.IsNonreal && !variable.Details.IsPrototypeInstantiation && !variable.Details.IsTemplateVariable;

        // These retained associations describe independent ABI entry identities.
        // The selected primary body is still located by exact external linkage;
        // no alternate body is selected, substituted or canonicalized here.
        private static bool HasPrimaryCompleteEntryAssociations(FunctionSymbol function)
        {
            if (function.PrimaryEntry is not null || function.Details.IsAliasEntry == true ||
                function.ThisAdjustment != 0 || function.ReturnAdjustment != 0 ||
                function.VirtualCallIndex != 0 || function.VirtualBaseIndex != 0 ||
                function.OverridingFunctionForWrapper is not null || function.OverriddenFunctionForWrapper is not null ||
                function.ConstructorOrDestructorKind?.Name != "cdk_complete" ||
                function.SpecialFunctionKind?.Name is not ("sfk_constructor" or "sfk_destructor") ||
                function.ContainingType is null || !function.Signature.HasThisParameter ||
                function.Signature.Parameters.Count == 0 || function.Signature.Parameters[0].Role.Name != "this_parameter")
                return false;
            foreach (var alternate in function.AlternateEntryPoints)
                if (!function.Equals(alternate.PrimaryEntry) || alternate.SpecialFunctionKind != function.SpecialFunctionKind ||
                    !function.ContainingType.Equals(alternate.ContainingType) ||
                    alternate.ConstructorOrDestructorKind?.Name != "cdk_subobject")
                    return false;
            return true;
        }

        private static void ValidatePolicy(Symbol symbol, string linkage)
        {
            if (symbol is FunctionSymbol function)
            {
                var details = function.Details; var supplement = function.GnuSupplement;
                if (details.IsWeak == true || details.IsWeakref == true || details.IsGnuAlias == true || details.IsIfunc == true ||
                    details.IsUseComdat == true || details.IsAliasEntry == true || function.PrimaryEntry is not null ||
                    function.AlternateEntryPoints.Count != 0 && !HasPrimaryCompleteEntryAssociations(function) || supplement?.AliasedRoutine is not null ||
                    supplement?.TargetedVersions.Count > 0 || supplement?.Representative is not null || details.IsInline ||
                    details.DefinitionForInliningOnly || details.SuppressInlineBody || details.SupersededExternal)
                {
                    var facts = new List<string>();
                    if (details.IsWeak == true) facts.Add("weak");
                    if (details.IsWeakref == true) facts.Add("weakref");
                    if (details.IsGnuAlias == true) facts.Add("GNU-alias");
                    if (details.IsIfunc == true) facts.Add("ifunc");
                    if (details.IsUseComdat == true) facts.Add("COMDAT");
                    if (details.IsAliasEntry == true) facts.Add("alias-entry");
                    if (function.PrimaryEntry is not null) facts.Add("primary-entry-association");
                    if (function.AlternateEntryPoints.Count != 0 && !HasPrimaryCompleteEntryAssociations(function)) facts.Add("unsupported-alternate-entry-associations");
                    if (supplement?.AliasedRoutine is not null) facts.Add("aliased-routine");
                    if (supplement?.TargetedVersions.Count > 0) facts.Add("targeted-versions");
                    if (supplement?.Representative is not null) facts.Add("representative");
                    if (details.IsInline) facts.Add("inline");
                    if (details.DefinitionForInliningOnly) facts.Add("inlining-only-definition");
                    if (details.SuppressInlineBody) facts.Add("suppressed-inline-body");
                    if (details.SupersededExternal) facts.Add("superseded-external");
                    Fail("ROC1224", $"Required cross-unit function selection has unsupported retained policy facts [{string.Join(", ", facts)}] on {(IsDefinition(function) ? "definition" : "declaration")}: {linkage}");
                }
            }
            else if (symbol is VariableSymbol variable)
            {
                var details = variable.Details;
                if (details.IsWeak || details.IsWeakref || details.IsCommon || details.IsGnuAlias || details.AliasedVariable is not null ||
                    details.ComdatGroup is not null || details.IsInline || details.SuppressInlineDefinition || details.SupersededExternal)
                    Fail("ROC1224", "Required cross-unit weak/common/alias/inline/COMDAT storage selection is unsupported: " + linkage);
            }
        }

        private static void Fail(string code, string message) => throw new ManagedSelectionException(code, message);

        private sealed class UnitState(RoslynObject source, string hash, int ordinal, ManagedDefinitionRoot[] roots)
        {
            internal RoslynObject Source { get; } = source;
            internal string Hash { get; } = hash;
            internal int Ordinal { get; } = ordinal;
            internal ManagedDefinitionRoot[] Roots { get; } = roots;
            internal List<FunctionSymbol> FunctionSeeds { get; } = [];
            internal HashSet<FunctionSymbol> FunctionSeedSet { get; } = [];
            internal List<VariableSymbol> VariableSeeds { get; } = [];
            internal HashSet<VariableSymbol> VariableSeedSet { get; } = [];
            internal ManagedSelectionPlan? Plan { get; set; }
            // Even an initially unseeded explicit input must perform its own
            // bounded ambient initialization/teardown admission check.
            internal bool Dirty { get; set; } = true;
        }
    }
}
