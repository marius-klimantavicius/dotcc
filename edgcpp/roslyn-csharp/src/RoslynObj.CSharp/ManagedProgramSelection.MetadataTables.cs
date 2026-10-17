using RoslynObj;

namespace RoslynObj.CSharp;

internal static partial class ManagedProgramSelectionPlanner
{
    private sealed partial class Work
    {
        private sealed record MetadataOnlyInlineTable(UnitState Unit, FunctionSymbol Function, VariableSymbol Table);
        private readonly Dictionary<VariableSymbol, MetadataOnlyInlineTable> metadataOnlyInlineTables = [];

        // This classifies a retained compiler table, not application static
        // state. Its owning body, cleanup dependencies and markers remain in
        // selection, and emission still needs its separate lifetime adapter.
        private bool IsMetadataOnlyInlineTable(UnitState unit, FunctionSymbol function, VariableSymbol table)
        {
            Tick();
            if (!HasFunctionComdatSelectionPolicy || function.Body is not { } body ||
                !IsDefinition(function) || !body.Locals.Contains(table)) return false;
            var eh = body.ExceptionHandling;
            if (eh.Details is not { } descriptor || !descriptor.Routine.Equals(function) ||
                eh.RegionTable?.Equals(table) != true || eh.ArrayTable is not null || eh.Arrays.Count != 0 ||
                table.ExceptionTable is not { } projection || projection.Kind.Name != "cleanup" ||
                !projection.Owner.Equals(table) || !projection.FunctionEh.Routine.Equals(function) ||
                projection.FunctionEh.RegionTable?.Equals(table) != true || projection.FunctionEh.ArrayTable is not null)
                return false;
            var details = table.Details;
            if (table.SymbolDetails.Storage?.Name != "sc_static" || details.StorageDuration?.Name != "static" ||
                details.InitializationKind.Name is not ("initk_function_local" or "initk_static" or "initk_none") ||
                details.InitializationRewrittenAsAssignment || details.IsParameter || table.IsThreadLocal || table.StandardThreadLocalDeclared ||
                table.StandardThreadLocalEffective || table.ExtensionThreadLocal ||
                details.PromotedLocalStatic || details.PromotedLocalStaticInitialization ||
                details.IsWeak || details.IsWeakref || details.IsCommon || details.IsGnuAlias || details.AliasedVariable is not null ||
                details.DynamicInitializationRoutine is not null || details.ThreadInitializationRoutine is not null ||
                details.ThreadWrapperRoutine is not null || table.CleanupRoutine is not null || details.ExtendsLifetime ||
                details.Initializer is not null || details.ConstantInitializer is not null ||
                table.Initializer is not null || table.ConstantInitializer is not null || table.EntitiesDefinedInInitializer.Count != 0)
                return false;
            foreach (var initializer in table.LocalStaticInitializations)
            {
                Tick();
                if (!initializer.Variable.Equals(table) || initializer.Kind.Name != "initk_static" ||
                    initializer.Initialization is not null || initializer.Constant is not null || initializer.Lifetime is not null)
                    return false;
            }
            if (metadataOnlyInlineTables.TryGetValue(table, out var previous))
            {
                if (!ReferenceEquals(previous.Unit, unit) || !previous.Function.Equals(function))
                    Fail("ROC1224", "One metadata-only inline table has more than one selected owning body.");
            }
            else metadataOnlyInlineTables.Add(table, new(unit, function, table));
            return true;
        }

        private void ValidateMetadataOnlyInlineTables()
        {
            if (metadataOnlyInlineTables.Count == 0) return;
            var selectedTables = new Dictionary<string, (MetadataOnlyInlineTable Entry, string Function)>(StringComparer.Ordinal);
            foreach (var entry in metadataOnlyInlineTables.Values)
            {
                Tick();
                var plan = entry.Unit.Plan ?? throw new ManagedSelectionException("ROC1224",
                    "A metadata-only inline table has no completed current-input dependency selection.");
                if (!plan.Report.ClosureComplete || !ReferenceEquals(plan.Source, entry.Unit.Source))
                    Fail("ROC1224", "A metadata-only inline table requires its completed current-input dependency selection.");
                var functionIndex = plan.Functions.ToList().IndexOf(entry.Function);
                var tableIndex = plan.Variables.ToList().IndexOf(entry.Table);
                if (functionIndex < 0 || tableIndex < 0 || variableDefinitions.ContainsKey(entry.Table))
                    Fail("ROC1224", "A metadata-only inline table lost its raw selected owner or acquired a storage representative.");
                var function = $"unit/{entry.Unit.Ordinal:D4}/function/{functionIndex + 1:D6}";
                var table = $"unit/{entry.Unit.Ordinal:D4}/storage/{tableIndex + 1:D6}";
                selectedTables.Add(table, (entry, function));
            }

            var bodyLocals = new HashSet<string>(StringComparer.Ordinal);
            var regionTables = new HashSet<string>(StringComparer.Ordinal);
            foreach (var unit in units)
            {
                Tick();
                if (unit.Plan is not { } plan) continue;
                foreach (var dependency in plan.Report.Dependencies)
                {
                    Tick();
                    if (!selectedTables.TryGetValue(dependency.To, out var selected)) continue;
                    if (!ReferenceEquals(unit, selected.Entry.Unit) || dependency.From != selected.Function)
                        Fail("ROC1224", "A metadata-only inline table has a required use outside its exact retained owning-function associations: " + dependency.Kind);
                    switch (dependency.Kind)
                    {
                        case "body-local": bodyLocals.Add(dependency.To); break;
                        case "normal-eh-region-table": regionTables.Add(dependency.To); break;
                        default:
                            // The closed typed selector records references,
                            // nested constant addresses, associated storage and
                            // initializer/cleanup targets as incoming edges.
                            // None is permitted to turn this table into a value.
                            Fail("ROC1224", "A metadata-only inline table has a required execution, address or storage use: " + dependency.Kind);
                            break;
                    }
                }
            }
            foreach (var table in selectedTables.Keys)
            {
                Tick();
                if (!bodyLocals.Contains(table) || !regionTables.Contains(table))
                    Fail("ROC1224", "A metadata-only inline table lacks its selected body-local or exact EH region-table dependency.");
            }
        }
    }
}
