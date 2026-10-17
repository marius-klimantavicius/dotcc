namespace RoslynObj.CSharp;

internal static partial class ManagedProgramSelectionPlanner
{
    private sealed partial class Work
    {
        private const string InlineComdatPolicy = "captured-inline-comdat-manifest-order";
        private readonly Dictionary<string, List<(UnitState Unit, FunctionSymbol Function)>> inlineGroups = new(StringComparer.Ordinal);
        private readonly Dictionary<FunctionSymbol, UnitState> functionOwners = [];

        private string DescribeRejectedInlineStorage(ManagedSelectionPlan plan, VariableSymbol variable)
        {
            // Only the already-required identity and shallow prerequisite facts.
            // This is diagnostic observation, never a storage admission waiver.
            var types = new List<object>();
            for (var current = variable.Type; ;)
            {
                Tick();
                if (types.Count >= budgets.MaximumSelectionDepth)
                    Fail("ROC1221", "Rejected-storage diagnostic type chain exceeds its explicit depth bound.");
                types.Add(new { current.Name, kind = current.Kind.Name, size = current.Size?.ToString(), alignment = current.Alignment?.ToString(),
                    isConst = (current as QualifiedType)?.IsConst, isVolatile = (current as QualifiedType)?.IsVolatile,
                    arrayBound = (current as ArrayType)?.Bound?.ToString(), pointerReference = (current as PointerType)?.IsReference });
                if (current is not TypeReference reference) break;
                current = reference.UnderlyingType;
            }
            List<object> TypeRows(CppType? type)
            {
                var rows = new List<object>();
                for (var current = type; current is not null;)
                {
                    Tick();
                    if (rows.Count >= budgets.MaximumSelectionDepth) Fail("ROC1221", "Diagnostic prerequisite type depth exceeded.");
                    rows.Add(new { current.Name, kind = current.Kind.Name, size = current.Size?.ToString(), alignment = current.Alignment?.ToString(),
                        isConst = (current as QualifiedType)?.IsConst, isVolatile = (current as QualifiedType)?.IsVolatile,
                        arithmeticKind = (current as ArithmeticType)?.Arithmetic.ArithmeticKind.Name,
                        bits = (current as ArithmeticType)?.Arithmetic.Bits.ToString(), signed = (current as ArithmeticType)?.Arithmetic.Signed });
                    current = current is TypeReference reference ? reference.UnderlyingType : null;
                }
                return rows;
            }
            object? Constant(Constant? constant)
            {
                if (constant is null) return null;
                var stored = new List<uint>(); var stringValue = constant.Value as StringValue;
                if (stringValue is not null)
                    foreach (var unit in stringValue.StorageUnits)
                    {
                        Tick();
                        if (stored.Count == 4096) break;
                        stored.Add(unit);
                    }
                return new
                {
                    kind = constant.Value.Kind.Name, type = constant.Type?.Kind.Name, size = constant.Type?.Size?.ToString(),
                    integer = (constant.Value as IntegerValue)?.Value.ToString(), stringLength = stringValue?.Length.ToString(),
                    stringUnitKind = stringValue?.UnitKind.Name, stringEncoding = stringValue?.Encoding.Name,
                    stringElementBits = stringValue?.ElementBits, exactStringUnits = stored,
                    exactStringUnitsTruncated = stringValue is not null && stringValue.Length > stored.Count,
                    constant.Details.IsStringLiteral, associatedStorageIsVariable = constant.AssociatedStorageVariable?.Equals(variable),
                    addressBase = (constant.Value as AddressValue)?.BaseEntity?.Name
                };
            }
            object? Initialization(Initialization? initializer) => initializer is null ? null : new
            {
                kind = initializer.Kind.Name, target = initializer.Target?.Name, constructor = initializer.Constructor?.CurrentName,
                destructor = initializer.Destructor?.CurrentName, expression = initializer.Expression?.Kind.Name,
                operation = initializer.Operation?.Kind.Name, representation = initializer.Operation?.RepresentationRole.Name,
                constant = Constant(initializer.Constant), hasLifetime = initializer.Lifetime is not null,
                hasInitExpressionLifetime = initializer.InitExpressionLifetime is not null, operands = initializer.Operands.Count
            };
            var statics = new List<object>();
            foreach (var descriptor in variable.LocalStaticInitializations)
            {
                Tick();
                if (statics.Count == 16) break;
                statics.Add(new { kind = descriptor.Kind.Name, variableMatches = descriptor.Variable.Equals(variable),
                    initializer = Initialization(descriptor.Initialization), constant = Constant(descriptor.Constant), hasLifetime = descriptor.Lifetime is not null });
            }
            var owners = new List<object>();
            foreach (var function in plan.Functions)
            {
                Tick();
                foreach (var parameter in function.Parameters)
                {
                    Tick();
                    if (parameter.Symbol?.Equals(variable) == true)
                        owners.Add(new { function = function.CurrentName, relation = "execution-parameter", ordinal = parameter.Ordinal });
                }
                if (function.Body?.Locals.Contains(variable) == true)
                    owners.Add(new { function = function.CurrentName, relation = "body-local", ordinal = -1 });
            }
            var locator = $"unit/{units.Single(unit => ReferenceEquals(unit.Source, plan.Source)).Ordinal:D4}/storage/{plan.Variables.ToList().IndexOf(variable) + 1:D6}";
            var uses = new List<ManagedSelectionDependencyReport>();
            var useCount = 0;
            foreach (var dependency in plan.Report.Dependencies)
            {
                Tick();
                if (dependency.To != locator) continue;
                ++useCount;
                if (uses.Count < 64) uses.Add(dependency);
            }
            var addresses = new List<object>();
            for (var index = 0; index < plan.Constants.Count; ++index)
            {
                Tick();
                var constant = plan.Constants[index];
                if (constant.Value is not AddressValue address || address.BaseEntity?.Equals(variable) != true) continue;
                var constantLocator = $"unit/{units.Single(unit => ReferenceEquals(unit.Source, plan.Source)).Ordinal:D4}/constant/{index + 1:D8}";
                var incoming = new List<ManagedSelectionDependencyReport>();
                foreach (var dependency in plan.Report.Dependencies)
                {
                    Tick();
                    if (dependency.To == constantLocator) incoming.Add(dependency);
                }
                var selectedOwners = new List<object>();
                for (var functionIndex = 0; functionIndex < plan.Functions.Count; ++functionIndex)
                {
                    Tick();
                    var ownerLocator = $"unit/{units.Single(unit => ReferenceEquals(unit.Source, plan.Source)).Ordinal:D4}/function/{functionIndex + 1:D6}";
                    if (incoming.Any(use => use.From == ownerLocator || use.From.StartsWith(ownerLocator + "/", StringComparison.Ordinal)))
                        selectedOwners.Add(new { function = plan.Functions[functionIndex].CurrentName, locator = ownerLocator });
                }
                CppType? addressType = constant.Type;
                while (addressType is TypeReference reference) { Tick(); addressType = reference.UnderlyingType; }
                addresses.Add(new { locator = constantLocator, type = TypeRows(constant.Type),
                    pointee = TypeRows((addressType as PointerType)?.PointeeType), byteOffset = address.ByteOffset.ToString(),
                    address.OnePastEnd, hasSubobjectPath = address.SubobjectPath is not null,
                    incomingCount = incoming.Count, incoming = incoming.Take(64).ToArray(), incomingTruncated = incoming.Count > 64, selectedOwners });
            }
            CppType elementRoot = variable.Type;
            while (elementRoot is TypeReference reference) { Tick(); elementRoot = reference.UnderlyingType; }
            var containing = variable.ContainingEntity;
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                inputSha256 = plan.Report.InputSha256, locator, variable.CurrentName, variable.Name,
                explicitObjectAlignment = variable.Details.ExplicitAlignment?.ToString(), storageClass = variable.SymbolDetails.Storage?.Name, storageDuration = variable.Details.StorageDuration?.Name,
                variable.Details.IsParameter, variable.IsThreadLocal, variable.Details.PromotedLocalStatic, variable.Details.PromotedLocalStaticInitialization,
                variable.Details.InitializationRewrittenAsAssignment, variable.Details.IsInline, variable.Details.IsWeak,
                variable.Details.IsWeakref, variable.Details.IsCommon, variable.Details.IsGnuAlias, variable.Details.AddressTaken,
                variable.IsCompilerGenerated, variable.Details.LoweringGenerated, hasCleanupRoutine = variable.CleanupRoutine is not null,
                hasAliasedVariable = variable.Details.AliasedVariable is not null, comdatGroup = variable.Details.ComdatGroup,
                linkage = variable.SourceCorrespondence?.Linkage.Name, containingName = containing?.Name,
                containingScopeKind = (containing as Scope)?.Kind.Name, containingScopeEntity = (containing as Scope)?.AssociatedEntity?.Name,
                types, arrayElementTypes = TypeRows((elementRoot as ArrayType)?.ElementType), addresses, definitionTypeKind = variable.DefinitionType?.Kind.Name, rawInitializationKind = variable.Details.InitializationKind.Name,
                rawInitializer = Initialization(variable.Details.Initializer), rawConstant = Constant(variable.Details.ConstantInitializer),
                wrapperInitializer = Initialization(variable.Initializer), wrapperConstant = Constant(variable.ConstantInitializer),
                exceptionTable = variable.ExceptionTable?.Kind.Name, localStaticDescriptors = statics,
                localStaticDescriptorCount = variable.LocalStaticInitializations.Count, owners, useCount, uses, usesTruncated = useCount > uses.Count
            });
        }

        private (UnitState Unit, Symbol Symbol) SelectInlineCandidate((UnitState Unit, Symbol Symbol)[] definitions, string linkage)
        {
            if (!HasFunctionComdatSelectionPolicy)
                Fail("ROC1222", "Required declaration has duplicate exact captured external definitions: " + linkage);
            var selected = SelectInlineRepresentative(definitions.Select(item => (item.Unit, (FunctionSymbol)item.Symbol)).ToArray(), linkage);
            return (selected.Unit, selected.Function);
        }

        private static bool IsInlineComdatMember(FunctionSymbol function, bool requireDefinition)
            => function.Details.IsInline && HasComdatMemberShape(function, requireDefinition);

        private static bool HasComdatMemberShape(FunctionSymbol function, bool requireDefinition)
        {
            var details = function.Details; var supplement = function.GnuSupplement;
            return (!requireDefinition || IsDefinition(function)) && details.IsUseComdat == true &&
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

        private static string DescribeInlineMember(FunctionSymbol function)
        {
            var d = function.Details; var g = function.GnuSupplement; var signature = function.Signature;
            return System.Text.Json.JsonSerializer.Serialize(new
            {
                function.CurrentName, function.IsDefined, bodyState = function.BodyState.Name,
                executionRole = function.Body?.Root.RepresentationRole.Name,
                d.IsInline, d.IsUseComdat, d.DefinitionNeeded, d.InlineInstanceRequired, d.NeedOutOfLineCopy, d.IsWeak, d.IsWeakref, d.IsGnuAlias, d.IsIfunc, d.IsAliasEntry,
                d.ExplicitInstantiation, d.ClassExplicitlyInstantiated, d.ExplicitDoNotInstantiate,
                explicitClassComdatProvider = IsExplicitClassComdatProvider(function),
                primaryEntry = function.PrimaryEntry?.CurrentName, d.DefinitionForInliningOnly, d.SuppressInlineBody, d.SupersededExternal,
                aliasedRoutine = g?.AliasedRoutine?.CurrentName, representative = g?.Representative?.CurrentName,
                inlinePartner = g?.InlinePartner?.CurrentName,
                inlinePartnerDefined = g?.InlinePartner?.IsDefined, inlinePartnerBodyState = g?.InlinePartner?.BodyState.Name,
                inlinePartnerSuppress = g?.InlinePartner?.Details.SuppressInlineBody, inlinePartnerComdat = g?.InlinePartner?.Details.IsUseComdat,
                targetedVersions = g?.TargetedVersions.Count, g?.IsRepresentative, g?.IsTargetSpecificVersion, g?.ResolverRequired,
                thisAdjustment = function.ThisAdjustment.ToString(), returnAdjustment = function.ReturnAdjustment.ToString(),
                virtualCallIndex = function.VirtualCallIndex.ToString(), virtualBaseIndex = function.VirtualBaseIndex.ToString(),
                overridingFunction = function.OverridingFunctionForWrapper?.CurrentName,
                overriddenFunction = function.OverriddenFunctionForWrapper?.CurrentName,
                kind = function.SpecialFunctionKind?.Name, entryKind = function.ConstructorOrDestructorKind?.Name,
                signature.HasPrototype, signature.IsVariadic, signature.ReturnsValueAsParameter, signature.ReturnsValueViaCopyConstructor,
                noexceptEffective = signature.ExceptionSpecification?.NoexceptEffective,
                parameters = signature.Parameters.Select(parameter => new { parameter.Ordinal, role = parameter.Role.Name,
                    hasBaseSubobject = parameter.BaseSubobject is not null, parameter.Descriptor.Details.PassedViaCopyConstructor }).ToArray(),
                alternates = function.AlternateEntryPoints.Select(entry => new { entry.CurrentName,
                    kind = entry.ConstructorOrDestructorKind?.Name, primaryMatches = entry.PrimaryEntry?.Equals(function),
                    entry.Details.IsAliasEntry, entry.IsDefined, bodyState = entry.BodyState.Name }).ToArray()
            });
        }

        private (UnitState Unit, FunctionSymbol Function) SelectInlineRepresentative(
            (UnitState Unit, FunctionSymbol Function)[] members, string linkage)
        {
            Tick();
            if (!HasFunctionComdatSelectionPolicy)
                Fail("ROC1222", "Duplicate function selection requires an explicit captured COMDAT representative policy: " + linkage);
            if (!inlineGroups.TryGetValue(linkage, out var group)) inlineGroups.Add(linkage, group = []);
            foreach (var member in members)
            {
                Tick();
                if (!IsSelectedComdatMember(member.Function, requireDefinition: true))
                    Fail("ROC1224", "A captured COMDAT representative group includes a definition outside the selected inline/template policy or with unsupported weak, alias, adjustment or execution-role facts: " + linkage + ". Captured group members: " + string.Join("; ", members.Select(item => "unit=" + item.Unit.Ordinal + ",sha256=" + item.Unit.Hash + ":" + DescribeInlineMember(item.Function))));
                if (group.All(item => !item.Function.Equals(member.Function))) group.Add(member);
                functionOwners[member.Function] = member.Unit;
            }
            group.Sort((left, right) => left.Unit.Ordinal.CompareTo(right.Unit.Ordinal));
            if (group.GroupBy(member => member.Unit.Ordinal).Any(sameUnit => sameUnit.Count() != 1))
                Fail("ROC1222", "An explicit COMDAT representative group has more than one definition in one input: " + linkage);
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
                    Fail("ROC1223", "COMDAT representative entry kind or retained exception specification differs: " + linkage);
                var comparison = compatibility.CompareFunctions(function, selected.Function);
                if (!comparison.Compatible)
                    Fail("ROC1223", $"COMDAT representative ABI is {comparison.Status} at {comparison.Path}: {comparison.Reason} ({linkage}).");
                var body = function.Body!;
                if (body.Locals.Any(variable => (variable.IsThreadLocal || variable.SymbolDetails.Storage?.Name == "sc_static" ||
                    variable.LocalStaticInitializations.Count != 0) && !TryAdmitConstantCompanion(member.Unit, variable) &&
                    !IsMetadataOnlyInlineTable(member.Unit, function, variable)) || body.Scope?.FunctionStaticLifetime is not null ||
                    body.Scope?.LocalStaticInitializations.Any(initializer =>
                        !initializer.Variable.LocalStaticInitializations.Contains(initializer) ||
                        !IsMetadataOnlyInlineTable(member.Unit, function, initializer.Variable)) == true)
                    Fail("ROC1224", "COMDAT function selection cannot merge associated local-static/TLS initialization or storage: " + linkage +
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
