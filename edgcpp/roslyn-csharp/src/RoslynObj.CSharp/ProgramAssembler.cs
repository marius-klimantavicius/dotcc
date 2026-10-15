namespace RoslynObj.CSharp;

/// <summary>Selects strong definitions using captured names and already-selected ABI facts.</summary>
public sealed class ProgramAssembler
{
    public ProgramAssemblyResult Assemble(IReadOnlyList<RoslynObject> inputs, ProgramAssemblyOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs); ArgumentNullException.ThrowIfNull(options);
        return new AssemblyWork(inputs, options, cancellationToken).Run();
    }

    private sealed class AssemblyWork(IReadOnlyList<RoslynObject> inputs, ProgramAssemblyOptions options, CancellationToken cancellationToken)
    {
        private readonly List<TranslationDiagnostic> diagnostics = [];
        private readonly List<Candidate> candidates = [];
        private readonly List<string> artifacts = [];
        private readonly HashSet<Symbol> required = [];
        private readonly List<ProgramFunction> functions = [];
        private readonly List<ProgramStorage> storage = [];
        private readonly Dictionary<FunctionSymbol, ProgramFunction> functionBindings = [];
        private readonly Dictionary<VariableSymbol, ProgramStorage> storageBindings = [];
        private readonly List<AssemblyDeclarationReport> declarations = [];
        private readonly AbiCompatibility compatibility = new();

        internal ProgramAssemblyResult Run()
        {
            if (inputs.Count == 0 || options.MaximumOpenUnits < 1 || inputs.Count > options.MaximumOpenUnits)
                Error("ROC2000", "Input count exceeds the explicit open-unit assembly bound, or no inputs were supplied.");
            if (options.ExternalBindings.GroupBy(b => (b.Kind, b.LinkageName)).Any(g => g.Count() > 1))
                Error("ROC2015", "A native binding has more than one provider declaration.");
            if (diagnostics.Count == 0)
            {
                var identities = new Dictionary<string, int>(StringComparer.Ordinal);
                for (var index = 0; index < inputs.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var obj = inputs[index];
                    if (identities.TryGetValue(obj.Manifest.ArtifactIdentity, out var previous))
                    {
                        var hashes = options.InputSha256;
                        if (!options.DeduplicateIdenticalArtifacts || hashes.Count != inputs.Count || hashes[index] != hashes[previous]
                            || hashes[index].Length != 64 || !hashes[index].All(Uri.IsHexDigit) || obj.Manifest != inputs[previous].Manifest)
                            Error("ROC2001", "Repeated artifact identity requires explicit deduplication and identical pinned bytes/provenance.", $"unit/{index + 1:D4}");
                        continue;
                    }
                    identities.Add(obj.Manifest.ArtifactIdentity, index);
                    artifacts.Add(obj.Manifest.ArtifactIdentity);
                    if (obj.Manifest.Target != inputs[0].Manifest.Target || obj.Manifest.CaptureProfile != "native_lowered")
                    { Error("ROC2002", "Assembly requires matching captured targets and admitted native_lowered inputs.", $"unit/{index + 1:D4}"); continue; }
                    Collect(obj.TranslationUnit, index + 1);
                }
                foreach (var candidate in candidates)
                {
                    candidate.Required = required.Contains(candidate.Symbol) || candidate.Definition || (candidate.Symbol switch
                    {
                        FunctionSymbol function => function.Details.IsGnuAlias == true || function.Details.IsIfunc == true
                            || function.GnuSupplement?.AliasedRoutine is not null || function.PrimaryEntry is not null,
                        VariableSymbol variable => variable.Details.IsGnuAlias || variable.Details.AliasedVariable is not null,
                        _ => false
                    });
                    if (candidate.Required) ValidatePolicy(candidate);
                }
                foreach (var group in candidates.Where(c => c.External && c.LinkName is not null).GroupBy(c => c.LinkName!.Name))
                    if (group.Select(c => c.Kind).Distinct(StringComparer.Ordinal).Count() > 1
                        && (group.Count(c => c.Definition) > 1 || group.Any(c => c.Required)))
                        foreach (var candidate in group)
                            Error("ROC2016", "Final external name is used for conflicting function/data categories: "
                                + string.Join(", ", group.Select(c => c.Locator + " " + c.Kind)) + ".", candidate.Locator);
                // Local candidates are keyed by their actual entity identities, never their names.
                foreach (var candidate in candidates.Where(c => !c.External)) Select([candidate]);
                foreach (var group in candidates.Where(c => c.External && c.LinkName is not null)
                    .GroupBy(c => (c.Kind, c.LinkName!.Name))) Select(group.ToArray());
                foreach (var candidate in candidates.Where(c => c.External && c.LinkName is null))
                {
                    if (candidate.Required) Error("ROC2003", "Required external entity has no captured final linkage identity.", candidate.Locator);
                    Record(candidate, null, null, "unavailable-external-name");
                }
            }
            var report = new ProgramAssemblyReport(artifacts.ToArray(), declarations.OrderBy(d => d.Locator, StringComparer.Ordinal).ToArray(),
                diagnostics.OrderBy(d => d.Locator, StringComparer.Ordinal).ThenBy(d => d.Code, StringComparer.Ordinal).ThenBy(d => d.Message, StringComparer.Ordinal).ToArray());
            return new(diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error) ? null
                : new(functions.ToArray(), storage.ToArray(), functionBindings, storageBindings, report), report);
        }

        private void Collect(TranslationUnit unit, int ordinal)
        {
            var prefix = $"unit/{ordinal:D4}";
            var functionOrdinal = 0;
            foreach (var function in unit.Functions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var locator = prefix + $"/function/{++functionOrdinal:D6}";
                var definition = function.BodyState.Name == "definition" && function.Body is { Root.RepresentationRole.Name: not "metadata" };
                if (function.BodyState.Name == "definition" && !definition)
                    Error("ROC2004", "A retained required definition has no executable body.", locator);
                var external = External(function);
                candidates.Add(new(function, "function", ordinal, locator, definition, external, external ? LinkName(function) : null));
                if (!definition) continue;
                VisitOperations(function.Body!.Root);
                var eh = function.Body.ExceptionHandling;
                if (eh.Details?.Prologue is { } prologue) VisitOperations(prologue);
                foreach (var epilogue in eh.Epilogues) VisitOperations(epilogue);
                Use(eh.RegionTable); Use(eh.ArrayTable);
                foreach (var region in eh.CleanupRegions)
                {
                    Use(region.Condition?.Storage.Variable);
                    foreach (var action in region.Actions)
                    {
                        Use(action.TargetMethod); Use(action.Storage?.Variable);
                        Use(action.AbiDetail?.VttLocation?.Variable); Use(action.AbiDetail?.VlaCountLocation?.Variable);
                    }
                }
                foreach (var lifetime in function.Body.Lifetimes)
                    foreach (var cleanup in lifetime.Cleanups) VisitRecipe(cleanup.Initialization);
            }
            var storageOrdinal = 0;
            foreach (var variable in unit.Variables)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var locator = prefix + $"/storage/{++storageOrdinal:D6}";
                var current = variable.SymbolDetails.Storage?.Name;
                var externalDeclaration = current == "sc_extern";
                var metadata = variable.Details.IsNonreal || variable.Details.IsPrototypeInstantiation || variable.Details.IsTemplateVariable;
                var local = variable.Details.IsParameter || variable.Details.StorageDuration?.Name is "automatic" or "parameter"
                    || current is "sc_auto" or "sc_register";
                if (metadata || local && !externalDeclaration) continue;
                var definition = !externalDeclaration;
                var external = External(variable);
                candidates.Add(new(variable, "variable", ordinal, locator, definition, external, external ? LinkName(variable) : null));
                if (!definition) continue;
                if (current is not ("sc_static" or "sc_unspecified")) Error("ROC2005", "Storage definition has no supported current storage classification.", locator);
                VisitConstant(variable.ConstantInitializer);
                if (variable.Initializer?.Operation is { RepresentationRole.Name: not "metadata" } operation) VisitOperations(operation);
                Use(variable.CleanupRoutine); Use(variable.Details.DynamicInitializationRoutine);
                Use(variable.Details.ThreadInitializationRoutine); Use(variable.Details.ThreadWrapperRoutine);
            }
            foreach (var root in unit.FileScopeInitializationRoutines) Use(root.Routine);
            foreach (var root in unit.Roots.Where(r => r.Kind.Name is "file_scope_statement" or "file_initializer"))
                if (root.Entity is SemanticNode node && node.RepresentationRole.Name != "metadata") VisitOperations(node);
                else if (root.Entity is Initialization recipe && recipe.Operation is { RepresentationRole.Name: not "metadata" } operation) VisitOperations(operation);
        }

        private readonly HashSet<Constant> constants = [];
        private readonly HashSet<ConstantValue> values = [];
        private readonly HashSet<Initialization> recipes = [];
        private void Use(SemanticEntity? entity) { if (entity is Symbol symbol) required.Add(symbol); }
        private void VisitConstant(Constant? constant)
        {
            if (constant is null || !constants.Add(constant)) return;
            VisitValue(constant.Value); Use(constant.AssociatedStorageVariable);
            foreach (var component in constant.Components) VisitConstant(component.Constant);
        }
        private void VisitValue(ConstantValue value)
        {
            if (!values.Add(value)) return;
            cancellationToken.ThrowIfCancellationRequested();
            switch (value)
            {
                case AddressValue address:
                    Use(address.BaseEntity);
                    if (address.BaseEntity is Constant constant) VisitConstant(constant);
                    else if (address.BaseEntity is ConstantValue nested) VisitValue(nested);
                    break;
                case AggregateValue aggregate: foreach (var element in aggregate.Elements) VisitValue(element.Value); break;
                case ConstantValue<RepeatValueDetails> repeat: VisitValue(repeat.Payload.Value); break;
                case ConstantValue<MemberPointerValueDetails> member: Use(member.Payload.Member); break;
                case LabelDifferenceValue difference: VisitValue(difference.FromValue); VisitValue(difference.ToValue); break;
            }
        }
        private void VisitRecipe(Initialization? recipe)
        {
            if (recipe is null || !recipes.Add(recipe)) return;
            Use(recipe.Target); Use(recipe.Constructor); Use(recipe.Destructor); VisitConstant(recipe.Constant);
            // Master is a metadata association; it adds no required execution or import root.
            if (recipe.Operation is { RepresentationRole.Name: not "metadata" } operation) VisitOperations(operation);
        }
        private void VisitOperations(SemanticNode root)
        {
            // This is an import/dependency inventory, not an execution schedule.
            foreach (var operation in root.DescendantOperations(cancellationToken))
                switch (operation)
                {
                    case CallOperation call: Use(call.TargetMethod); break;
                    case ReferenceOperation reference: Use(reference.Target); break;
                    case ConstantOperation constant: VisitConstant(constant.Constant); break;
                    case InitializationOperation initialization: VisitRecipe(initialization.Initialization); break;
                    case AllocationOperation allocation: Use(allocation.Allocator); Use(allocation.Deallocator); VisitRecipe(allocation.ExceptionFreeing); break;
                    case ThrowOperation thrown: Use(thrown.Destructor); break;
                    case LoweredEhOperation lowered:
                        for (var cleanup = lowered.CleanupStart; cleanup is not null && !recipes.Contains(cleanup); cleanup = cleanup.NextInDestructionList) VisitRecipe(cleanup);
                        break;
                }
        }

        private void ValidatePolicy(Candidate candidate)
        {
            if (candidate.Symbol is FunctionSymbol function)
            {
                var details = function.Details;
                var supplement = function.GnuSupplement;
                if (details.IsWeak == true || details.IsWeakref == true || details.IsGnuAlias == true || details.IsIfunc == true
                    || details.IsUseComdat == true || details.IsAliasEntry == true || function.PrimaryEntry is not null
                    || function.AlternateEntryPoints.Count != 0 || supplement?.AliasedRoutine is not null
                    || supplement?.TargetedVersions.Count > 0 || supplement?.Representative is not null
                    || details.IsInline && candidate.External || details.DefinitionForInliningOnly || details.SuppressInlineBody)
                    Error("ROC2010", "Required weak/alias/ifunc/multiversion/alternate-entry/inline/COMDAT function selection is unsupported.", candidate.Locator);
                if (details.SupersededExternal) Error("ROC2011", "Superseded external declarations require an explicit selected ABI adapter.", candidate.Locator);
            }
            else if (candidate.Symbol is VariableSymbol variable)
            {
                var details = variable.Details;
                if (details.IsWeak || details.IsWeakref || details.IsCommon || details.IsGnuAlias || details.AliasedVariable is not null
                    || details.ComdatGroup is not null || details.IsInline && candidate.External || details.SuppressInlineDefinition)
                    Error("ROC2010", "Required weak/common/alias/inline/COMDAT storage selection is unsupported.", candidate.Locator);
                if (details.SupersededExternal) Error("ROC2011", "Superseded external storage requires an explicit selected ABI adapter.", candidate.Locator);
            }
        }

        private void Select(IReadOnlyList<Candidate> group)
        {
            var definitions = group.Where(c => c.Definition).ToArray();
            if (definitions.Length > 1)
            {
                foreach (var candidate in group)
                {
                    if (candidate.Definition) Error("ROC2012", "Duplicate strong definitions: " + string.Join(", ", definitions.Select(d => d.Locator)) + ".", candidate.Locator);
                    Record(candidate, null, null, "duplicate-strong-definitions");
                }
                return;
            }
            if (definitions.Length == 0)
            {
                foreach (var candidate in group)
                {
                    var provider = candidate.External && candidate.LinkName is { } name
                        ? options.ExternalBindings.SingleOrDefault(b => b.Kind == candidate.Kind && b.LinkageName == name.Name) : null;
                    if (candidate.Required && provider is null) Error("ROC2013", "Required symbol has no selected definition or explicit external binding.", candidate.Locator);
                    if (candidate.Required && provider is not null && (string.IsNullOrWhiteSpace(provider.Provider)
                        || string.IsNullOrWhiteSpace(provider.Symbol) || string.IsNullOrWhiteSpace(provider.Signature)
                        || string.IsNullOrWhiteSpace(provider.Ownership) || string.IsNullOrWhiteSpace(provider.ErrorContract)
                        || string.IsNullOrWhiteSpace(provider.ThreadContract) || string.IsNullOrWhiteSpace(provider.CallbackContract)))
                        Error("ROC2015", "Required native provider declaration has an incomplete signature/ownership/error/thread/callback contract.", candidate.Locator);
                    // Provider declaration is an assembly decision, not executable-provider validation.
                    Record(candidate, null, provider?.Provider, provider is null ? candidate.Required ? "unresolved-required" : "unreferenced-declaration" : "explicit-external-provider");
                }
                return;
            }
            var definition = definitions[0];
            foreach (var declaration in group.Where(c => c.Required && !c.Definition))
            {
                var result = declaration.Symbol is FunctionSymbol left && definition.Symbol is FunctionSymbol right
                    ? compatibility.CompareFunctions(left, right)
                    : compatibility.CompareStorage((VariableSymbol)declaration.Symbol, (VariableSymbol)definition.Symbol);
                if (!result.Compatible) Error("ROC2014", $"Selected shared ABI representation is {result.Status.ToString().ToLowerInvariant()} at {result.Path}: {result.Reason} Definition: {definition.Locator}.", declaration.Locator);
            }
            if (definition.Symbol is FunctionSymbol function)
            {
                var selected = new ProgramFunction(definition.Locator, definition.Unit, function, definition.LinkName, group.Select(c => (FunctionSymbol)c.Symbol).ToArray())
                    { SelectedOrdinal = functions.Count + 1 };
                functions.Add(selected);
                foreach (var candidate in group) functionBindings.Add((FunctionSymbol)candidate.Symbol, selected);
            }
            else
            {
                var selected = new ProgramStorage(definition.Locator, definition.Unit, (VariableSymbol)definition.Symbol, definition.LinkName, group.Select(c => (VariableSymbol)c.Symbol).ToArray())
                    { SelectedOrdinal = storage.Count + 1 };
                storage.Add(selected);
                foreach (var candidate in group) storageBindings.Add((VariableSymbol)candidate.Symbol, selected);
            }
            foreach (var candidate in group) Record(candidate, definition.Locator, null, candidate.Definition ? "selected-strong-definition" : "bound-to-strong-definition");
        }

        private static bool External(Symbol symbol) => symbol.SourceCorrespondence?.Linkage.Name is "nlk_external" or "nlk_cplusplus_external" or "external" or "c" or "cpp";
        private static CapturedLinkName? LinkName(Symbol symbol)
        {
            var correspondence = symbol.SourceCorrespondence;
            if (correspondence is null) return null;
            var assemblyName = symbol switch { FunctionSymbol f => f.GnuSupplement?.AssemblyName, VariableSymbol v => v.AssemblyName, _ => null };
            if (!string.IsNullOrEmpty(assemblyName)) return new(assemblyName, correspondence.Linkage.Name, "explicit-assembly-name",
                correspondence.CurrentName, correspondence.NameHasBeenMangled, correspondence.MangledEncoding);
            if (string.IsNullOrEmpty(correspondence.CurrentName)) return null;
            if (correspondence.Linkage.Name is "nlk_cplusplus_external" or "cpp" && correspondence.NameHasBeenMangled != true) return null;
            return new(correspondence.CurrentName, correspondence.Linkage.Name,
                correspondence.NameHasBeenMangled == true ? "captured-mangled-current-name" : "captured-current-name",
                correspondence.CurrentName, correspondence.NameHasBeenMangled, correspondence.MangledEncoding);
        }
        private void Record(Candidate candidate, string? selected, string? provider, string decision)
            => declarations.Add(new(candidate.Locator, candidate.Unit, candidate.Kind, candidate.Symbol.Name, candidate.LinkName,
                candidate.Definition ? "definition" : "declaration", candidate.Required, selected, provider, decision));
        private void Error(string code, string message, string? locator = null)
            => diagnostics.Add(new(code, DiagnosticSeverity.Error, message, Locator: locator, Family: "program-assembly"));
        private sealed class Candidate(Symbol symbol, string kind, int unit, string locator, bool definition, bool external, CapturedLinkName? linkName)
        {
            internal Symbol Symbol { get; } = symbol;
            internal string Kind { get; } = kind;
            internal int Unit { get; } = unit;
            internal string Locator { get; } = locator;
            internal bool Definition { get; } = definition;
            internal bool External { get; } = external;
            internal CapturedLinkName? LinkName { get; } = linkName;
            internal bool Required { get; set; }
        }
    }
}
