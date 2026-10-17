using System.Globalization;
using RoslynObj;

namespace RoslynObj.CSharp;

internal static partial class ManagedSelectionPlanner
{
    private sealed partial class Work
    {
        private readonly Dictionary<FunctionSymbol, ManagedNormalEhReport> normalEhReports = [];
        private readonly Dictionary<FunctionSymbol, List<ManagedNormalEhMarkerReport>> normalEhMarkers = [];
        private readonly HashSet<LoweredEhOperation> normalEhMarkerSet = [];

        internal ManagedSelectionReport BuildReport(bool complete)
        {
            var definitionCount = functions.Count(ManagedFunctionDefinitionPolicy.IsProviderDefinition);
            return new()
            {
                InputArtifactIdentity = obj.Manifest.ArtifactIdentity, InputSha256 = selectedInputHash?.ToLowerInvariant() ?? "",
                ClosureComplete = complete, WholeInputInventoryComplete = false, WholeInputTranslationComplete = false,
                RelatedTypeMetadataComplete = false, ExecutableBodiesVisited = bodiesVisited,
                RequiredDeclarationCount = functions.Count - definitionCount, StorageDefinitionCount = variables.Count(IsStorageDefinition),
                RequiredVariableCount = variables.Count, ScopedTypeCount = types.Count, RequiredConstantCount = constants.Count,
                NonProviderBodyRequirements = nonProviderBodyRequirements.ToArray(),
                BodyOperationCount = bodyOperationCount, SelectionWorkCount = workCount, AmbientRootRowsInspected = ambientRootRowsInspected,
                AmbientRoutineCandidatesInspected = ambientRoutineCandidatesInspected,
                AmbientInitialization = startup is null
                    ? "Required ambient translation-unit initialization/teardown is checked and unsupported execution is rejected, never erased."
                    : complete
                        ? "Exact current startup roots cover the admitted backend candidates in explicit target input order. Their executable closure is selected; generated explicit initialization and registered teardown still require emitter admission and separate execution evidence."
                        : "Explicit startup selection is incomplete; ambient coverage and executable closure are not claimed complete, and no startup or teardown execution admission is established.",
                Roots = selectedRoots.ToArray(), StartupRoots = selectedStartupRoots.ToArray(), StartupExecutionContract = startup?.Contract,
                NormalExceptionHandling = normalEhReports.Select(pair => pair.Value with
                { Markers = normalEhMarkers.TryGetValue(pair.Key, out var markers) ? markers.ToArray() : [] }).ToArray(),
                Dependencies = dependencies.ToArray()
            };
        }

        private static string? Number(System.Numerics.BigInteger? value) => value?.ToString(CultureInfo.InvariantCulture);
        private string? OptionalLocator(SemanticEntity? entity) => entity is null ? null : Locator(entity);

        private void VisitNormalEhDependencies(FunctionSymbol function, FunctionBody body, string from)
        {
            Tick();
            var eh = body.ExceptionHandling;
            var details = eh.Details;
            var regions = new List<ManagedNormalEhRegionReport>();
            var epilogues = new List<string>();
            Use(eh.RegionTable, from, "normal-eh-region-table");
            Use(eh.ArrayTable, from, "normal-eh-array-table");
            normalEhReports.Add(function, new(from, details is not null, OptionalLocator(details?.Routine),
                OptionalLocator(eh.RegionTable), OptionalLocator(eh.ArrayTable), Number(details?.InitialState), Number(details?.NullState),
                OptionalLocator(details?.Prologue), epilogues, regions)
            { CapturedFunctionName = function.CurrentName });
            if (details is not null && !details.Routine.Equals(function))
                Fail("ROC1214", "The selected normal-EH descriptor has an unsupported routine ownership association.", from);
            foreach (var epilogue in eh.Epilogues) { Tick(); epilogues.Add(Locator(epilogue)); }
            foreach (var region in eh.CleanupRegions)
            {
                Tick();
                var actions = new List<ManagedNormalEhActionReport>();
                regions.Add(new(Locator(region), region.Ordinal, OptionalLocator(region.NextRegion), actions));
                Edge(from, "normal-cleanup-region", region);
                if (!region.Details.FunctionEh.Routine.Equals(function))
                    Fail("ROC1214", "A selected cleanup region has an unsupported owning routine association.", from);
                if (region.NextRegion is { } next) Edge(Locator(region), "normal-cleanup-next-region", next);
                if (region.Details.SupplementOwner is { } supplement) Edge(Locator(region), "normal-cleanup-supplement-owner", supplement);
                if (region.Condition is { } regionCondition)
                    VisitNormalCleanupStorage(regionCondition.Storage, Locator(region), "normal-cleanup-condition-storage");
                foreach (var action in region.Actions)
                {
                    Tick();
                    var storage = action.Storage; var condition = region.Condition;
                    Use(action.TargetMethod, Locator(action), "normal-cleanup-target");
                    VisitNormalCleanupStorage(storage, Locator(action), "normal-cleanup-storage");
                    VisitNormalCleanupStorage(action.AbiDetail?.VttLocation, Locator(action), "normal-cleanup-vtt");
                    VisitNormalCleanupStorage(action.AbiDetail?.VlaCountLocation, Locator(action), "normal-cleanup-count");
                    actions.Add(new(Locator(action), action.Kind.Name, OptionalLocator(action.TargetMethod), storage?.Kind.Name,
                        OptionalLocator(storage?.Variable), OptionalLocator(storage?.Details.BaseEntity), Number(storage?.Offset),
                        storage?.IsIndirect, storage?.UsesThisParameterOffset, condition?.Kind.Name,
                        OptionalLocator(condition?.Storage.Variable), Number(condition?.Details.ComparisonValue)));
                    Edge(Locator(region), "normal-cleanup-action", action);
                    var indirectStorage = action.Details.IsIndirect || storage?.IsIndirect == true ||
                        storage?.Kind.Name == "indirect_variable";
                    if (action.Array is not null || action.Details.IsArray || action.Details.IsVla || action.Details.IsDeallocation ||
                        action.Details.IsLocalStaticGuard || storage?.Kind.Name == "no_object" ||
                        indirectStorage && !CollectNormalIndirectVariableCleanup(action, function, body))
                        Fail("ROC1214", "The selected normal-cleanup action has unsupported array, allocation, incompletely described indirect storage or exception-owner dependencies.", Locator(action));
                }
            }
            if (eh.ArrayTable is not null || eh.Arrays.Count != 0)
                Fail("ROC1214", "Selected normal-EH dependency accounting does not yet admit array cleanup tables.", from);
        }

        private void VisitNormalCleanupStorage(CleanupStorageLocation? storage, string from, string kind)
        {
            if (storage is null) return;
            Tick(); Edge(from, kind, storage);
            Use(storage.Variable, Locator(storage), "normal-cleanup-storage-variable");
            Use(storage.Details.BaseEntity, Locator(storage), "normal-cleanup-storage-base");
        }

        // Dependency accounting only: the captured owned pointer slot locates
        // an object whose actual cleanup target is already selected above.
        // Execution still needs an independent normal-lifetime adapter.
        private bool CollectNormalIndirectVariableCleanup(CleanupAction action, FunctionSymbol owner, FunctionBody body)
        {
            Tick();
            if (action.Kind.Name != "destructor" || action.TargetMethod is null || !action.Details.IsIndirect ||
                action.Storage is not { Kind.Name: "indirect_variable", IsIndirect: true,
                    UsesThisParameterOffset: false, Variable: { } variable } storage ||
                storage.Offset != 0 || storage.Details.BaseEntity is not null)
                return false;
            Parameter? selectedParameter = null;
            foreach (var parameter in owner.Parameters)
            {
                Tick();
                if (parameter.Symbol?.Equals(variable) != true) continue;
                if (selectedParameter is not null) return false;
                selectedParameter = parameter;
            }
            if (selectedParameter is null && !body.Locals.Contains(variable) || NormalCleanupPointerType(variable.Type) is not
                    PointerType { IsReference: false, IsRValueReference: false } pointer ||
                selectedParameter is not null && !pointer.Equals(NormalCleanupPointerType(selectedParameter.Type)) ||
                pointer.Size is not { } size || size <= 0 || pointer.Alignment is not { } alignment || alignment <= 0)
                return false;
            RequireType(variable.Type, true);
            RequireType(variable.DeclaredType, false);
            if (selectedParameter is not null)
            {
                RequireType(selectedParameter.Type, true);
                RequireType(selectedParameter.DeclaredType, false);
            }
            RequireType(pointer.PointeeType, true);
            Edge(Locator(storage), "normal-cleanup-indirect-pointee", pointer.PointeeType);
            return true;
        }

        private CppType NormalCleanupPointerType(CppType type)
        {
            var seen = new HashSet<CppType>();
            var depth = 0;
            while (type is TypeReference reference)
            {
                Tick(++depth);
                if (!seen.Add(type)) Fail("ROC1214", "An indirect cleanup pointer has an unsupported cyclic type wrapper.");
                type = type is QualifiedType qualified ? qualified.UnderlyingType : reference.UnderlyingType;
            }
            Tick(depth);
            return type;
        }

        private void VisitNormalEhMarker(LoweredEhOperation marker, FunctionSymbol owner, string from, int depth)
        {
            Tick(depth);
            var payload = marker.ExceptionOperation;
            if (normalEhMarkerSet.Add(marker))
            {
                if (!normalEhMarkers.TryGetValue(owner, out var rows)) normalEhMarkers.Add(owner, rows = []);
                rows.Add(new(Locator(marker), marker.ExceptionKind.Name, OptionalLocator(payload.FunctionEh?.Routine),
                    OptionalLocator(payload.Routine), OptionalLocator(marker is CleanupStateOperation selectedState ? selectedState.Region : payload.Region),
                    Number(marker is CleanupStateOperation numberedState ? numberedState.StateNumber : payload.StateNumber),
                    marker is CleanupStateOperation state ? state.IsUnreachable : null, OptionalLocator(payload.Initialization)));
            }
            if (marker.ExceptionKind.Name is not ("leck_function_prologue" or "leck_function_epilogue" or "leck_cleanup_state" or
                    "leck_unreachable_cleanup_state" or "leck_initialization_completed") || payload.Handler is not null ||
                payload.TryBlock is not null || payload.TryExpression is not null || payload.CatchExpression is not null ||
                payload.FunctionEh is { } descriptor && !descriptor.Routine.Equals(owner) || payload.Routine is { } routine && !routine.Equals(owner))
                Fail("ROC1214", "Selected normal-EH marker accounting does not admit throw, handler or foreign-owner propagation.", from);
            if (payload.Region is { } region)
            {
                if (!region.Details.FunctionEh.Routine.Equals(owner))
                    Fail("ROC1214", "A normal state marker has an unsupported region-owner association.", from);
                Edge(from, "normal-marker-region", region);
            }
            if (marker is CleanupStateOperation { Region: { } stateRegion })
            {
                if (!stateRegion.Details.FunctionEh.Routine.Equals(owner))
                    Fail("ROC1214", "A normal cleanup-state facet has an unsupported region-owner association.", from);
                Edge(from, "normal-state-region", stateRegion);
            }
            if (marker.CleanupStart is { } cleanup) VisitRecipe(cleanup, from, true, depth + 1);
            else VisitRecipe(marker.Initialization, from, false, depth + 1);
        }
    }
}
