namespace RoslynObj.CSharp;

internal static partial class ManagedProgramSelectionPlanner
{
    private sealed partial class Work
    {
        private const string ConstantStoragePolicy = "captured-external-comdat-constant-manifest-order";
        private readonly HashSet<VariableSymbol> constantCompanions = [];
        private string constantStorageFailure = "policy not selected";
        private bool RejectConstantStorage(string reason) { constantStorageFailure = reason; return false; }
        private readonly Dictionary<VariableSymbol, UnitState> variableOwners = [];
        private readonly Dictionary<string, List<(UnitState Unit, VariableSymbol Variable)>> constantGroups = new(StringComparer.Ordinal);

        // A separate storage contract: function representative selection alone
        // never merges state. Facts are read from each original definition.
        private bool TryAdmitConstantCompanion(UnitState unit, VariableSymbol variable)
        {
            if (constantCompanions.Contains(variable)) return true;
            if (promotedConstantStorage != ConstantStoragePolicy || !ConstantStorageFacts(variable, out _)) return false;
            constantCompanions.Add(variable);
            variableOwners[variable] = unit;
            SelectConstantStorage([(unit, variable)], LinkName(variable)!);
            return true;
        }

        private bool ConstantStorageFacts(VariableSymbol variable, out StringValue? value)
        {
            value = null; Tick();
            var d = variable.Details; var linkage = LinkName(variable);
            if (!IsDefinition(variable) || linkage is null || d.ComdatGroup != linkage ||
                !d.PromotedLocalStatic || !d.PromotedLocalStaticInitialization || d.IsParameter ||
                d.StorageDuration?.Name != "static" || d.ExplicitAlignment is { } objectAlignment && objectAlignment != 0 && objectAlignment != 1 || variable.IsThreadLocal || variable.StandardThreadLocalDeclared ||
                variable.StandardThreadLocalEffective || variable.ExtensionThreadLocal || d.IsWeak || d.IsWeakref || d.IsCommon ||
                d.IsGnuAlias || d.AliasedVariable is not null || d.IsInline || d.SuppressInlineDefinition || d.SupersededExternal ||
                d.InitializationRewrittenAsAssignment || d.DynamicInitializationRoutine is not null ||
                d.ThreadInitializationRoutine is not null || d.ThreadWrapperRoutine is not null || variable.CleanupRoutine is not null ||
                d.ExtendsLifetime || variable.ExceptionTable is not null || variable.LocalStaticInitializations.Count != 0 ||
                d.InitializationKind.Name != "initk_static" || d.Initializer is not null || variable.Initializer is not null ||
                variable.EntitiesDefinedInInitializer.Count != 0 ||
                d.ConstantInitializer is not { Details.IsStringLiteral: true, Value: StringValue } constant ||
                constant.AssociatedStorageVariable is not null || !constant.Equals(variable.ConstantInitializer)) return RejectConstantStorage("definition/initializer scheduling or linkage");
            var arrayType = ConstantStorageType(variable.Type, out _);
            if (arrayType is not ArrayType array || array.Bound is not { } bound || bound <= 0 ||
                array.Size != bound || array.Alignment != 1 || variable.Type.Size != bound || variable.Type.Alignment != 1 ||
                array.BoundExpression is not null || array.VlaDimension is not null)
                return RejectConstantStorage("array representation: " + constantStorageFailure + "; bound/expression/VLA=" + (arrayType is ArrayType a ? $"{a.Bound}/{a.BoundExpression?.Kind.Name}/{a.VlaDimension is not null}" : "not-array"));
            var element = ConstantStorageType(array.ElementType, out var isConst);
            if (!isConst || element is not ArithmeticType arithmetic || arithmetic.Arithmetic.Bits != 8 ||
                arithmetic.Arithmetic.ArithmeticKind.Name is not ("signed_integer" or "unsigned_integer") ||
                array.ElementType.Size != 1 || array.ElementType.Alignment != 1) return RejectConstantStorage("const-byte element: " + constantStorageFailure);
            value = (StringValue)constant.Value;
            if (value.UnitKind.Name != "target_bytes" || value.Encoding.Name != "target_code_units" ||
                value.ElementBits != 8 || value.Length != bound || constant.Type is null ||
                !compatibility.CompareTypes(variable.Type, constant.Type).Compatible) return RejectConstantStorage("literal representation: " + (constant.Type is null ? "no type" : compatibility.CompareTypes(variable.Type, constant.Type).Reason));
            long count = 0;
            foreach (var storageUnit in value.StorageUnits)
            {
                Tick();
                if (storageUnit > byte.MaxValue || ++count > bound) return false;
            }
            return count == bound;
        }

        private CppType? ConstantStorageType(CppType type, out bool isConst)
        {
            isConst = false; var seen = new HashSet<CppType>();
            if (type.Details.IsAtomic) return null;
            while (type is TypeReference reference)
            {
                Tick();
                if (type.Details.IsAtomic) return null;
                if (!seen.Add(type) || seen.Count > budgets.MaximumSelectionDepth) return null;
                if (type is QualifiedType qualified)
                {
                    if (qualified.IsVolatile || qualified.Qualification.IsRestrict || qualified.Qualification.IsAtomic ||
                        qualified.Qualification.AddressSpace is not (null or 0) || qualified.Qualification.IsShared ||
                        qualified.Qualification.IsUnaligned ||
                        qualified.Qualification.IsNear || qualified.Qualification.IsFar || qualified.Qualification.IsNullable ||
                        qualified.Qualification.IsNonnull || qualified.Qualification.IsNullUnspecified ||
                        qualified.Qualification.IsUpcStrict || qualified.Qualification.IsUpcRelaxed)
                    {
                        var q = qualified.Qualification;
                        constantStorageFailure = System.Text.Json.JsonSerializer.Serialize(new { q.IsVolatile, q.IsRestrict, q.IsAtomic,
                            q.AddressSpace, q.IsShared, sharedBlockSize = q.SharedBlockSize?.ToString(), q.IsUnaligned, q.IsNear,
                            q.IsFar, q.IsNullable, q.IsNonnull, q.IsNullUnspecified, q.IsUpcStrict, q.IsUpcRelaxed });
                        return null;
                    }
                    isConst |= qualified.IsConst;
                }
                type = reference.UnderlyingType;
            }
            return type.Details.IsAtomic ? null : type;
        }

        private (UnitState Unit, Symbol Symbol) SelectConstantCandidate((UnitState Unit, Symbol Symbol)[] definitions, string linkage)
        {
            if (promotedConstantStorage != ConstantStoragePolicy)
                Fail("ROC1222", "Required storage declaration has duplicate exact captured external definitions: " + linkage);
            var selected = SelectConstantStorage(definitions.Select(item => (item.Unit, (VariableSymbol)item.Symbol)).ToArray(), linkage);
            return (selected.Unit, selected.Variable);
        }

        private (UnitState Unit, VariableSymbol Variable) SelectConstantStorage(
            (UnitState Unit, VariableSymbol Variable)[] members, string linkage)
        {
            if (!constantGroups.TryGetValue(linkage, out var group)) constantGroups.Add(linkage, group = []);
            foreach (var member in members)
            {
                Tick();
                if (promotedConstantStorage != ConstantStoragePolicy || !ConstantStorageFacts(member.Variable, out _))
                    Fail("ROC1224", "Promoted constant storage member has unsupported captured storage, initializer or linkage obligations: " + linkage);
                if (group.All(item => !item.Variable.Equals(member.Variable))) group.Add(member);
                constantCompanions.Add(member.Variable); variableOwners[member.Variable] = member.Unit;
            }
            group.Sort((left, right) => left.Unit.Ordinal.CompareTo(right.Unit.Ordinal));
            if (group.GroupBy(member => member.Unit.Ordinal).Any(sameUnit => sameUnit.Count() != 1))
                Fail("ROC1222", "A constant storage group has more than one definition in one supplied input: " + linkage);
            var selected = group[0];
            var selectedValue = (StringValue)selected.Variable.Details.ConstantInitializer!.Value;
            foreach (var member in group)
            {
                Tick();
                var comparison = compatibility.CompareStorage(member.Variable, selected.Variable);
                var value = (StringValue)member.Variable.Details.ConstantInitializer!.Value;
                if (!comparison.Compatible || value.Encoding.Name != selectedValue.Encoding.Name || value.UnitKind.Name != selectedValue.UnitKind.Name ||
                    value.ElementBits != selectedValue.ElementBits || value.Length != selectedValue.Length ||
                    !SameConstantUnits(value, selectedValue))
                    Fail("ROC1223", "Same-linkage constant storage definitions differ in captured ABI or exact initializer units/encoding: " + linkage);
                // All original initializer dependencies remain in selection.
                if (member.Unit.VariableSeedSet.Add(member.Variable))
                {
                    member.Unit.VariableSeeds.Add(member.Variable); member.Unit.Dirty = true;
                }
            }
            variableDefinitions.Remove(selected.Variable);
            foreach (var member in group)
                if (!member.Variable.Equals(selected.Variable)) variableDefinitions[member.Variable] = selected.Variable;
            foreach (var key in variableDefinitions.Keys.ToArray()) variableDefinitions[key] = ResolveVariable(variableDefinitions[key]);
            return selected;
        }

        private bool SameConstantUnits(StringValue left, StringValue right)
        {
            using var a = left.StorageUnits.GetEnumerator(); using var b = right.StorageUnits.GetEnumerator();
            while (true)
            {
                Tick(); var hasA = a.MoveNext(); var hasB = b.MoveNext();
                if (hasA != hasB) return false;
                if (!hasA) return true;
                if (a.Current != b.Current) return false;
            }
        }

        private VariableSymbol ResolveVariable(VariableSymbol variable)
        {
            var seen = new HashSet<VariableSymbol>();
            while (variableDefinitions.TryGetValue(variable, out var next))
            {
                Tick();
                if (!seen.Add(variable)) Fail("ROC1222", "Selected storage representative mappings contain a cycle.");
                variable = next;
            }
            return variable;
        }

        private UnitState VariableOwner(VariableSymbol variable)
            => variableOwners.TryGetValue(variable, out var unit) ? unit : units.Single(unit => unit.Plan?.Variables.Contains(variable) == true);

        private void FinalizeVariableBindings()
        {
            variableBindings.Clear();
            foreach (var key in variableDefinitions.Keys.ToArray())
            {
                Tick(); var selected = ResolveVariable(variableDefinitions[key]); variableDefinitions[key] = selected;
                var comparison = compatibility.CompareStorage(key, selected);
                if (!comparison.Compatible) Fail("ROC1223", "A flattened selected storage binding has incompatible captured ABI.");
                variableBindings.Add(new(key, VariableOwner(key).Ordinal, selected, VariableOwner(selected).Ordinal,
                    LinkName(key) ?? throw new ManagedSelectionException("ROC1222", "A selected storage binding has no captured external linkage."), comparison));
            }
        }

        private IReadOnlyList<ManagedConstantStorageRepresentativeReport> BuildConstantStorageReports()
            => constantGroups.Select(pair =>
            {
                var selected = pair.Value[0];
                string Locator((UnitState Unit, VariableSymbol Variable) entry)
                    => $"unit/{entry.Unit.Ordinal:D4}/storage/{entry.Unit.Plan!.Variables.ToList().IndexOf(entry.Variable) + 1:D6}";
                return new ManagedConstantStorageRepresentativeReport(pair.Key, promotedConstantStorage, selected.Unit.Ordinal, Locator(selected),
                    pair.Value.Select(member =>
                    {
                        var value = (StringValue)member.Variable.Details.ConstantInitializer!.Value;
                        var array = (ArrayType)ConstantStorageType(member.Variable.Type, out _)!;
                        var element = (ArithmeticType)ConstantStorageType(array.ElementType, out _)!;
                        return new ManagedConstantStorageMemberReport(member.Unit.Ordinal, member.Unit.Hash, Locator(member),
                            member.Variable.Type.Size!.ToString()!, member.Variable.Type.Alignment!.ToString()!,
                            element.Arithmetic.ArithmeticKind.Name, value.Encoding.Name, value.StorageUnits.ToArray());
                    }).ToArray());
            }).ToArray();
    }
}
