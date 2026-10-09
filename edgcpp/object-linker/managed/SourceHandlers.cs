namespace DotObj;

internal sealed record SourceHandler(long NodeId, long TryNodeId, long FunctionId, long Ordinal, long? SourceTypeId, long? MatchTypeId, long? ParameterBindingId);
internal sealed record HandlerMarker(long NodeId, long OwnerNodeId, string Kind);

internal sealed partial class ObjectFile
{
    public Dictionary<long, long> SourceTries { get; } = [];
    public Dictionary<long, SourceHandler> SourceHandlers { get; } = [];
    public Dictionary<long, long> CaughtObjectAddresses { get; } = [];
    public Dictionary<long, HandlerMarker> HandlerMarkers { get; } = [];

    private void ReadSourceHandlers()
    {
        using (var query = database.Prepare("SELECT t.node_id,t.source_lifetime_id,t.capture_proof,n.function_id,n.kind FROM source_tries t LEFT JOIN nodes n ON n.id=t.node_id"))
            while (query.Read())
            {
                if (!SourceLifetimes.TryGetValue(query.Integer(1), out SourceLifetime? lifetime) || lifetime.Kind != "try-block" ||
                    query.OptionalInteger(3) is not long owner || OriginalFunction(owner) != lifetime.FunctionEntityId ||
                    query.Text(2) != "observed-ordinary-try" || query.OptionalText(4) != "try" || !SourceTries.TryAdd(query.Integer(0), lifetime.Id))
                    throw new InvalidDataException("Source try lacks its exact original lifetime and function");
            }
        using (var query = database.Prepare("SELECT h.node_id,h.try_node_id,h.ordinal,h.match_kind,h.capture_proof,n.kind,n.function_id,t.function_id,h.source_type_id,h.match_type_id,h.parameter_binding_id,b.function_id,b.type_id,d.source_type_id FROM source_handlers h LEFT JOIN nodes n ON n.id=h.node_id LEFT JOIN nodes t ON t.id=h.try_node_id LEFT JOIN bindings b ON b.id=h.parameter_binding_id LEFT JOIN source_declarations d ON d.binding_id=b.id ORDER BY h.try_node_id,h.ordinal"))
            while (query.Read())
            {
                long handler = query.Integer(0), owner = query.Integer(1);
                long? source = query.OptionalInteger(8), match = query.OptionalInteger(9), parameter = query.OptionalInteger(10);
                if (!SourceTries.ContainsKey(owner) || query.Integer(2) < 0 || query.Text(3) is not ("typed" or "ellipsis") ||
                    query.Text(4) != "observed-source-handler" || query.OptionalText(5) != "catch" ||
                    query.OptionalInteger(6) is not long function || query.OptionalInteger(7) != function)
                    throw new InvalidDataException("Source catch lacks its exact handler and owning try");
                if (query.Text(3) == "ellipsis")
                {
                    if (source is not null || match is not null || parameter is not null)
                        throw new InvalidDataException("Ellipsis handler cannot own a typed parameter");
                }
                else
                {
                    if (source is null || match is null || parameter is null || query.OptionalInteger(11) != function ||
                        query.OptionalInteger(12) != SourceTypeById(source.Value).ExecutionTypeId || query.OptionalInteger(13) != source)
                        throw new InvalidDataException("Typed handler lacks its original parameter binding");
                    SourceType declared = Original(SourceTypeById(source.Value));
                    long expected = declared.Kind == "lvalue-reference" ? declared.UnderlyingTypeId!.Value : declared.Id;
                    if (Original(SourceTypeById(match.Value)).Id != Original(SourceTypeById(expected)).Id || declared.Kind == "rvalue-reference")
                        throw new InvalidDataException("Catch match type differs from original parameter type");
                    _ = ExceptionTypeIdentity(match.Value);
                }
                if (!SourceHandlers.TryAdd(handler, new(handler, owner, function, query.Integer(2), source, match, parameter)))
                    throw new InvalidDataException("Duplicate source handler");
            }
        foreach (var group in SourceHandlers.Values.GroupBy(handler => handler.TryNodeId))
        {
            SourceHandler[] handlers = group.OrderBy(handler => handler.Ordinal).ToArray();
            for (int i = 0; i < handlers.Length; ++i)
                if (handlers[i].Ordinal != i || handlers[i].MatchTypeId is null && i != handlers.Length - 1)
                    throw new InvalidDataException("Catch order must be dense with ellipsis last");
        }
        using (var query = database.Prepare("SELECT a.node_id,a.handler_node_id,a.capture_proof,n.kind,n.function_id,n.type_id FROM caught_object_addresses a LEFT JOIN nodes n ON n.id=a.node_id"))
            while (query.Read())
            {
                if (!SourceHandlers.TryGetValue(query.Integer(1), out SourceHandler? handler) || handler.MatchTypeId is null ||
                    query.Text(2) != "observed-caught-object-address" || query.OptionalText(3) != "caught-object-address" ||
                    query.OptionalInteger(4) != handler.FunctionId || query.OptionalInteger(5) is not long type || Type(type) != "ptr(void)" ||
                    !CaughtObjectAddresses.TryAdd(query.Integer(0), handler.NodeId))
                    throw new InvalidDataException("Caught address lacks its typed handler ownership");
            }
        if (!SourceTries.Keys.ToHashSet().SetEquals(SourceHandlers.Values.Select(handler => handler.TryNodeId)))
            throw new InvalidDataException("Source try requires its owned handler");
        var releases = CleanupStates.Values.Where(state => state.HandlerNodeId is not null).ToArray();
        if (!SourceHandlers.Keys.ToHashSet().SetEquals(releases.Select(state => state.HandlerNodeId!.Value)) ||
            releases.Select(state => state.HandlerNodeId).Distinct().Count() != releases.Length ||
            releases.Any(state => SourceHandlers[state.HandlerNodeId!.Value].FunctionId != state.FunctionEntityId))
            throw new InvalidDataException("Source handler requires its exact observed release state");
        using (var query = database.Prepare("SELECT m.node_id,m.owner_node_id,m.kind,m.capture_proof,n.kind,n.function_id,o.function_id FROM handler_markers m LEFT JOIN nodes n ON n.id=m.node_id LEFT JOIN nodes o ON o.id=m.owner_node_id"))
            while (query.Read())
            {
                long node = query.Integer(0), owner = query.Integer(1); string kind = query.Text(2);
                if ((kind == "try-exit" ? !SourceTries.ContainsKey(owner) : !SourceHandlers.ContainsKey(owner)) ||
                    kind is not ("caught" or "handler-exit" or "try-exit") || query.Text(3) != "observed-handler-marker" ||
                    query.OptionalText(4) != "handler-marker" || query.OptionalInteger(5) != query.OptionalInteger(6) ||
                    !HandlerMarkers.TryAdd(node, new(node, owner, kind)))
                    throw new InvalidDataException("Handler marker lacks its exact owner and kind");
            }
        foreach (SourceHandler handler in SourceHandlers.Values)
            if (HandlerMarkers.Values.Count(marker => marker.OwnerNodeId == handler.NodeId && marker.Kind == "caught") != 1)
                throw new InvalidDataException("Handler requires exactly one observed caught marker");
        using var nodes = database.Prepare("SELECT id,kind FROM nodes WHERE kind IN ('try','catch','handler-marker','caught-object-address')");
        while (nodes.Read())
            if (!(nodes.Text(1) switch { "try" => SourceTries.ContainsKey(nodes.Integer(0)), "catch" => SourceHandlers.ContainsKey(nodes.Integer(0)), "caught-object-address" => CaughtObjectAddresses.ContainsKey(nodes.Integer(0)), _ => HandlerMarkers.ContainsKey(nodes.Integer(0)) }))
                throw new InvalidDataException("Source handler node has no owned provenance");
    }
}
