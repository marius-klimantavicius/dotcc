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
            var definitionCount = functions.Count(function => function.BodyState.Name == "definition" && function.Body is not null);
            return new()
            {
                InputArtifactIdentity = obj.Manifest.ArtifactIdentity, InputSha256 = selectedInputHash?.ToLowerInvariant() ?? "",
                ClosureComplete = complete, WholeInputInventoryComplete = false, WholeInputTranslationComplete = false,
                RelatedTypeMetadataComplete = false, ExecutableBodiesVisited = bodiesVisited,
                RequiredDeclarationCount = functions.Count - definitionCount, StorageDefinitionCount = variables.Count(IsStorageDefinition),
                RequiredVariableCount = variables.Count, ScopedTypeCount = types.Count, RequiredConstantCount = constants.Count,
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
                    if (action.Array is not null || action.Details.IsArray || action.Details.IsVla || action.Details.IsDeallocation ||
                        action.Details.IsLocalStaticGuard || action.Details.IsIndirect || storage?.Kind.Name == "no_object")
                        Fail("ROC1214", "The selected normal-cleanup action has unsupported array, allocation, indirect or exception-owner dependencies.", Locator(action));
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
