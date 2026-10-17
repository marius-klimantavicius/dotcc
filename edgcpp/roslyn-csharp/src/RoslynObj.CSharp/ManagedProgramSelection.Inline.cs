namespace RoslynObj.CSharp;

internal static partial class ManagedProgramSelectionPlanner
{
    private sealed partial class Work
    {
        private const string InlineComdatPolicy = "captured-inline-comdat-manifest-order";
        private readonly Dictionary<string, List<(UnitState Unit, FunctionSymbol Function)>> inlineGroups = new(StringComparer.Ordinal);
        private readonly Dictionary<FunctionSymbol, UnitState> functionOwners = [];

        private (UnitState Unit, Symbol Symbol) SelectInlineCandidate((UnitState Unit, Symbol Symbol)[] definitions, string linkage)
        {
            if (definitionSelection != InlineComdatPolicy)
                Fail("ROC1222", "Required declaration has duplicate exact captured external definitions: " + linkage);
            var selected = SelectInlineRepresentative(definitions.Select(item => (item.Unit, (FunctionSymbol)item.Symbol)).ToArray(), linkage);
            return (selected.Unit, selected.Function);
        }

        private static bool IsInlineComdatMember(FunctionSymbol function, bool requireDefinition)
        {
            var details = function.Details; var supplement = function.GnuSupplement;
            return (!requireDefinition || IsDefinition(function)) && details.IsInline && details.IsUseComdat == true &&
                details.IsWeak == false && details.IsWeakref == false && details.IsGnuAlias == false && details.IsIfunc == false &&
                details.IsAliasEntry == false && function.PrimaryEntry is null &&
                !details.DefinitionForInliningOnly && !details.SuppressInlineBody && !details.SupersededExternal &&
                supplement?.AliasedRoutine is null && supplement?.Representative is null && supplement?.TargetedVersions.Count is not > 0 &&
                supplement?.IsRepresentative != true && supplement?.IsTargetSpecificVersion != true && supplement?.ResolverRequired != true &&
                function.ThisAdjustment == 0 && function.ReturnAdjustment == 0 && function.VirtualCallIndex == 0 && function.VirtualBaseIndex == 0 &&
                function.OverridingFunctionForWrapper is null && function.OverriddenFunctionForWrapper is null &&
                function.Signature.HasPrototype && !function.Signature.IsVariadic && function.Signature.ReturnsValueAsParameter == false &&
                !function.Signature.ReturnsValueViaCopyConstructor && function.Signature.Parameters.All(parameter =>
                    parameter.Role.Name is "ordinary_parameter" or "this_parameter" && parameter.BaseSubobject is null &&
                    !parameter.Descriptor.Details.PassedViaCopyConstructor);
        }

        private (UnitState Unit, FunctionSymbol Function) SelectInlineRepresentative(
            (UnitState Unit, FunctionSymbol Function)[] members, string linkage)
        {
            Tick();
            if (definitionSelection != InlineComdatPolicy)
                Fail("ROC1222", "Duplicate function selection requires explicit inline COMDAT policy: " + linkage);
            if (!inlineGroups.TryGetValue(linkage, out var group)) inlineGroups.Add(linkage, group = []);
            foreach (var member in members)
            {
                Tick();
                if (!IsInlineComdatMember(member.Function, requireDefinition: true))
                    Fail("ROC1224", "An inline representative group includes a non-inline/COMDAT, weak, alias, adjusted or unsupported execution-role definition: " + linkage);
                if (group.All(item => !item.Function.Equals(member.Function))) group.Add(member);
                functionOwners[member.Function] = member.Unit;
            }
            group.Sort((left, right) => left.Unit.Ordinal.CompareTo(right.Unit.Ordinal));
            if (group.GroupBy(member => member.Unit.Ordinal).Any(sameUnit => sameUnit.Count() != 1))
                Fail("ROC1222", "An explicit inline group has more than one definition in one input: " + linkage);
            var selected = group[0];
            foreach (var member in group)
            {
                Tick();
                var function = member.Function;
                var signature = function.Signature; var chosen = selected.Function.Signature;
                static string? Kind(SemanticKind? kind) => kind is null ? null : kind.Family + ":" + kind.Name;
                if (Kind(function.SpecialFunctionKind) != Kind(selected.Function.SpecialFunctionKind) ||
                    Kind(function.ConstructorOrDestructorKind) != Kind(selected.Function.ConstructorOrDestructorKind) ||
                    Kind(signature.ExceptionSpecification?.Kind) != Kind(chosen.ExceptionSpecification?.Kind) ||
                    signature.ExceptionSpecification?.NoexceptEffective != chosen.ExceptionSpecification?.NoexceptEffective)
                    Fail("ROC1223", "Inline representative entry kind or retained exception specification differs: " + linkage);
                var comparison = compatibility.CompareFunctions(function, selected.Function);
                if (!comparison.Compatible)
                    Fail("ROC1223", $"Inline representative ABI is {comparison.Status} at {comparison.Path}: {comparison.Reason} ({linkage}).");
                var body = function.Body!;
                if (body.Locals.Any(variable => variable.IsThreadLocal || variable.SymbolDetails.Storage?.Name == "sc_static" ||
                    variable.LocalStaticInitializations.Count != 0) || body.Scope?.FunctionStaticLifetime is not null ||
                    body.Scope?.LocalStaticInitializations.Count > 0)
                    Fail("ROC1224", "Inline function selection cannot merge associated local-static/TLS initialization or storage: " + linkage +
                        "; locals=[" + string.Join(", ", body.Locals.Where(variable => variable.IsThreadLocal || variable.SymbolDetails.Storage?.Name == "sc_static" ||
                            variable.LocalStaticInitializations.Count != 0).Select(variable =>
                            $"{variable.CurrentName}:storage={variable.SymbolDetails.Storage?.Name},TLS={variable.IsThreadLocal},promoted={variable.Details.PromotedLocalStatic},promoted-init={variable.Details.PromotedLocalStaticInitialization},region-table={variable.Equals(body.ExceptionHandling.RegionTable)},array-table={variable.Equals(body.ExceptionHandling.ArrayTable)},local-inits={variable.LocalStaticInitializations.Count}")) +
                        $"]; function-static-lifetime={body.Scope?.FunctionStaticLifetime is not null},scope-local-inits={body.Scope?.LocalStaticInitializations.Count ?? 0}");
                // Every captured member body remains in dependency accounting.
                // Selection never hides another input's ambient obligations.
                if (member.Unit.FunctionSeedSet.Add(function))
                {
                    member.Unit.FunctionSeeds.Add(function);
                    member.Unit.Dirty = true;
                }
            }
            functionDefinitions.Remove(selected.Function);
            foreach (var member in group)
                if (!member.Function.Equals(selected.Function)) functionDefinitions[member.Function] = selected.Function;
            foreach (var key in functionDefinitions.Keys.ToArray()) functionDefinitions[key] = ResolveFunction(functionDefinitions[key]);
            return selected;
        }

        private FunctionSymbol ResolveFunction(FunctionSymbol function)
        {
            var seen = new HashSet<FunctionSymbol>();
            while (functionDefinitions.TryGetValue(function, out var next))
            {
                if (!seen.Add(function)) Fail("ROC1222", "Selected function representative mappings contain a cycle.");
                function = next;
            }
            return function;
        }

        private UnitState FunctionOwner(FunctionSymbol function)
            => functionOwners.TryGetValue(function, out var unit) ? unit :
                throw new ManagedSelectionException("ROC1222", "A required function has no current-input ownership in the selected closure.");

        private void FinalizeFunctionBindings()
        {
            functionBindings.Clear();
            foreach (var key in functionDefinitions.Keys.ToArray())
            {
                Tick();
                var selected = ResolveFunction(functionDefinitions[key]); functionDefinitions[key] = selected;
                var comparison = compatibility.CompareFunctions(key, selected);
                if (!comparison.Compatible) Fail("ROC1223", "A flattened selected function binding has incompatible captured ABI.");
                functionBindings.Add(new(key, FunctionOwner(key).Ordinal, selected, FunctionOwner(selected).Ordinal,
                    LinkName(key) ?? throw new ManagedSelectionException("ROC1222", "A selected external binding has no captured linkage."), comparison));
            }
        }

        private IReadOnlyList<ManagedInlineRepresentativeReport> BuildInlineRepresentativeReports()
            => inlineGroups.Where(pair => pair.Value.Count > 1).Select(pair =>
            {
                var selected = pair.Value[0];
                string Locator((UnitState Unit, FunctionSymbol Function) entry)
                    => $"unit/{entry.Unit.Ordinal:D4}/function/{entry.Unit.Plan!.Functions.ToList().IndexOf(entry.Function) + 1:D6}";
                return new ManagedInlineRepresentativeReport(pair.Key, definitionSelection, selected.Unit.Ordinal, Locator(selected),
                    pair.Value.Select(member => new ManagedInlineDefinitionMemberReport(member.Unit.Ordinal, member.Unit.Hash, Locator(member),
                        member.Function.SpecialFunctionKind?.Name, member.Function.ConstructorOrDestructorKind?.Name,
                        member.Function.Signature.ExceptionSpecification?.NoexceptEffective,
                        compatibility.CompareFunctions(member.Function, selected.Function).Reason)).ToArray());
            }).ToArray();
    }
}
