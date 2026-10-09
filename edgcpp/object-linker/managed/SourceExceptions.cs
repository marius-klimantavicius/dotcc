namespace DotObj;

internal sealed record ExceptionType(long Id, long SourceTypeId, long LeafSourceTypeId,
    long PayloadTypeId, long PayloadSize, long PayloadAlignment, long? SourceDestructorEntityId, string IdentityKey);
internal sealed record ExceptionBaseConversion(long ExceptionTypeId, long Ordinal, long BaseSourceTypeId,
    long ByteOffset, bool IsPublic, bool IsAmbiguous);
internal sealed record ThrowExpression(long NodeId, long ExceptionTypeId, long? DestructorEntityId, string InitializationKind);

internal sealed partial class ObjectFile
{
    public Dictionary<long, ExceptionType> ExceptionTypes { get; } = [];
    public Dictionary<long, ExceptionBaseConversion[]> ExceptionBases { get; } = [];
    public Dictionary<long, ThrowExpression> ThrowExpressions { get; } = [];
    public Dictionary<long, long> ThrowObjectAddresses { get; } = [];
    public Dictionary<long, long> ThrowMarkers { get; } = [];
    public HashSet<long> ThrowInitializationBoundaries { get; } = [];
    public Dictionary<long, long> ThrowCleanupTransitions { get; } = [];
    public HashSet<long> RethrowExpressions { get; } = [];

    // EH identity follows the original language type, never its C# storage type.
    public string ExceptionTypeIdentity(long id, bool topLevel = true, int depth = 0)
    {
        if (depth > 128) throw new InvalidDataException("Exception type nesting exceeds supported limit");
        SourceType source = SourceTypeById(id);
        bool isConst = false, isVolatile = false;
        while (true)
        {
            isConst |= source.IsConst; isVolatile |= source.IsVolatile;
            if (source.Kind is not ("qualified" or "typedef")) break;
            if (++depth > 128) throw new InvalidDataException("Exception type nesting exceeds supported limit");
            source = SourceTypeById(source.UnderlyingTypeId!.Value);
        }
        string qualifiers = topLevel ? "" : (isConst ? "const:" : "") + (isVolatile ? "volatile:" : "");
        string identity;
        if (source.Kind == "pointer")
            identity = "pointer(" + ExceptionTypeIdentity(source.UnderlyingTypeId!.Value, false, depth + 1) + ")";
        else if (source.Kind == "record")
        {
            RecordType record = RecordsByKey[Type(source.ExecutionTypeId)];
            if (record.RepresentationKind != "complete" || record.Size == 0)
                throw new InvalidDataException("Exception record identity requires a complete original class");
            identity = record.Key;
        }
        else if (source.Kind is "void" or "bool" or "char" or "signed-char" or "unsigned-char" or
            "short" or "unsigned-short" or "int" or "unsigned-int" or "long" or "unsigned-long" or
            "long-long" or "unsigned-long-long" or "int128" or "unsigned-int128" or
            "float" or "double" or "long-double" or "float80" or "float64x") identity = source.Kind;
        else throw new InvalidDataException("Exception type requires an unsupported original identity category");
        return qualifiers + identity;
    }

    private SourceType ExceptionLeaf(long id)
    {
        SourceType source = SourceTypeById(id);
        for (int depth = 0; depth <= 128; ++depth)
        {
            if (source.Kind is not ("qualified" or "typedef" or "pointer")) return source;
            source = SourceTypeById(source.UnderlyingTypeId!.Value);
        }
        throw new InvalidDataException("Exception type nesting exceeds supported limit");
    }

    private void ReadExceptionMetadata()
    {
        using (var query = database.Prepare("SELECT id,source_type_id,leaf_source_type_id,payload_type_id,payload_size,payload_alignment,source_destructor_entity_id,capture_proof FROM exception_types ORDER BY id"))
            while (query.Read())
            {
                long id = query.Integer(0), sourceId = query.Integer(1), leafId = query.Integer(2), payloadId = query.Integer(3);
                SourceType source = SourceTypeById(sourceId), leaf = ExceptionLeaf(sourceId);
                string payload = Type(payloadId), identity = ExceptionTypeIdentity(sourceId);
                if (id <= 0 || SourceLanguage != "c++" || source.ExecutionTypeId != payloadId || leaf.Id != leafId ||
                    payload == "void" || ScalarTypes.IsArray(payload) || RoutineTypes.IsRoutine(payload) ||
                    query.Integer(4) != StorageSize(payload) || query.Integer(5) != StorageAlignment(payload) ||
                    query.Integer(4) <= 0 || query.Integer(5) <= 0 || query.Text(7) != "before-throw-lowering" ||
                    !ExceptionTypes.TryAdd(id, new(id, sourceId, leafId, payloadId, query.Integer(4), query.Integer(5), query.OptionalInteger(6), identity)))
                    throw new InvalidDataException("Invalid original exception type, leaf or payload layout");
                if (query.OptionalInteger(6) is long primary && (!ScalarTypes.IsRecord(payload) ||
                    !EntitySourceRoutines.TryGetValue(primary, out RoutineType? destructor) ||
                    SourceFunctionKinds.GetValueOrDefault(primary) != "destructor" || destructor.ObjectTypeId != leafId))
                    throw new InvalidDataException("Exception descriptor destruction obligation must name its exact original payload destructor");
            }
        var bases = new Dictionary<long, List<ExceptionBaseConversion>>();
        using (var query = database.Prepare("SELECT exception_type_id,ordinal,base_source_type_id,byte_offset,is_public,is_ambiguous,capture_proof FROM exception_base_conversions ORDER BY exception_type_id,ordinal"))
            while (query.Read())
            {
                long id = query.Integer(0), targetId = query.Integer(2);
                if (!ExceptionTypes.TryGetValue(id, out ExceptionType? descriptor) ||
                    SourceTypeById(descriptor.LeafSourceTypeId).Kind != "record" || SourceTypeById(targetId).Kind != "record" ||
                    query.Integer(1) < 0 || query.Integer(3) < 0 || query.Integer(4) is not (0 or 1) ||
                    query.Integer(5) is not (0 or 1) || query.Text(6) != "observed-complete-base-layout")
                    throw new InvalidDataException("Invalid exception base conversion provenance");
                if (!bases.TryGetValue(id, out var list)) bases.Add(id, list = []);
                if (query.Integer(1) != list.Count) throw new InvalidDataException("Exception base candidate order is not dense");
                list.Add(new(id, query.Integer(1), targetId, query.Integer(3), query.Integer(4) == 1, query.Integer(5) == 1));
            }
        foreach (ExceptionType descriptor in ExceptionTypes.Values)
        {
            ExceptionBaseConversion[] candidates = bases.GetValueOrDefault(descriptor.Id, []).ToArray();
            VerifyExceptionBases(descriptor, candidates);
            ExceptionBases.Add(descriptor.Id, candidates);
        }
        using (var query = database.Prepare("SELECT x.node_id,x.exception_type_id,x.destructor_entity_id,x.initialization_kind,x.capture_proof,n.kind FROM throw_expressions x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
            {
                var item = new ThrowExpression(query.Integer(0), query.Integer(1), query.OptionalInteger(2), query.Text(3));
                if (!ExceptionTypes.TryGetValue(item.ExceptionTypeId, out ExceptionType? descriptor) ||
                    item.InitializationKind is not ("empty" or "expression") || query.Text(4) != "observed-throw-lowering" ||
                    query.OptionalText(5) != "throw" || (descriptor.SourceDestructorEntityId is null) != (item.DestructorEntityId is null) ||
                    !ThrowExpressions.TryAdd(item.NodeId, item)) throw new InvalidDataException("Invalid owned throw expression");
                if (item.InitializationKind == "empty" && !ScalarTypes.IsRecord(Type(descriptor.PayloadTypeId)))
                    throw new InvalidDataException("Empty throw initialization requires an original class payload");
                if (item.DestructorEntityId is long callbackId)
                {
                    long primary = descriptor.SourceDestructorEntityId!.Value;
                    if (!ScalarTypes.IsRecord(Type(descriptor.PayloadTypeId)) || !Entities.TryGetValue(callbackId, out Entity? callback) ||
                        !EntitySourceRoutines.TryGetValue(primary, out RoutineType? sourceDestructor) ||
                        SourceFunctionKinds.GetValueOrDefault(primary) != "destructor" ||
                        sourceDestructor.ObjectTypeId != descriptor.LeafSourceTypeId || OriginalFunction(callbackId) != primary ||
                        AbiRoutineOrigins.TryGetValue(callbackId, out AbiRoutineOrigin? origin) && origin.EntryKind != "complete" ||
                        Type(callback.ReturnTypeId) != "void" || Routines[callback.RoutineTypeId].Variadic ||
                        !Parameters.GetValueOrDefault(callbackId, []).SequenceEqual(["ptr(" + Type(descriptor.PayloadTypeId) + ")"]))
                        throw new InvalidDataException("Exception payload destructor lacks its exact source and complete-object ABI provenance");
                }
            }
        using (var query = database.Prepare("SELECT throw_node_id,cleanup_policy,capture_proof FROM throw_initialization_boundaries"))
            while (query.Read())
                if (!ThrowExpressions.ContainsKey(query.Integer(0)) || query.Text(1) != "unwind-to-entry-before-release" ||
                    query.Text(2) != "observed-throw-initializer" || !ThrowInitializationBoundaries.Add(query.Integer(0)))
                    throw new InvalidDataException("Invalid owned throw initialization cleanup boundary");
        if (!ThrowInitializationBoundaries.SetEquals(ThrowExpressions.Keys))
            throw new InvalidDataException("Throw requires its initialization cleanup/release boundary");
        ReadThrowOwnerMap("throw_cleanup_transitions", "cleanup-transition", "observed-throw-initializer-transition", ThrowCleanupTransitions, false);
        ReadThrowOwnerMap("throw_object_addresses", "thrown-object-address", "observed-thrown-object-address", ThrowObjectAddresses, false);
        ReadThrowOwnerMap("throw_markers", "throw-marker", "observed-throw-start", ThrowMarkers, true);
        using (var query = database.Prepare("SELECT x.node_id,x.capture_proof,n.kind FROM rethrow_expressions x LEFT JOIN nodes n ON n.id=x.node_id ORDER BY x.node_id"))
            while (query.Read())
                if (query.Text(1) != "observed-rethrow" || query.OptionalText(2) != "rethrow" || !RethrowExpressions.Add(query.Integer(0)))
                    throw new InvalidDataException("Invalid observed rethrow");
        using var nodes = database.Prepare("SELECT id,kind FROM nodes WHERE kind IN ('throw','thrown-object-address','throw-marker','rethrow')");
        while (nodes.Read())
            if (!(nodes.Text(1) switch { "throw" => ThrowExpressions.ContainsKey(nodes.Integer(0)),
                "thrown-object-address" => ThrowObjectAddresses.ContainsKey(nodes.Integer(0)),
                "throw-marker" => ThrowMarkers.ContainsKey(nodes.Integer(0)), _ => RethrowExpressions.Contains(nodes.Integer(0)) }))
                throw new InvalidDataException("Source exception node lacks its observed provenance");
    }

    private void ReadThrowOwnerMap(string table, string nodeKind, string proof, Dictionary<long, long> target, bool marker)
    {
        using var query = database.Prepare("SELECT x.node_id,x.throw_node_id,x.capture_proof,n.kind,n.function_id,t.function_id" +
            (marker ? ",x.kind" : "") + " FROM " + table + " x LEFT JOIN nodes n ON n.id=x.node_id LEFT JOIN nodes t ON t.id=x.throw_node_id ORDER BY x.node_id");
        while (query.Read())
            if (!ThrowExpressions.TryGetValue(query.Integer(1), out ThrowExpression? expression) || expression.InitializationKind != "expression" ||
                query.Text(2) != proof || query.OptionalText(3) != nodeKind || query.OptionalInteger(4) != query.OptionalInteger(5) ||
                marker && query.Text(6) != "started" || !target.TryAdd(query.Integer(0), query.Integer(1)))
                throw new InvalidDataException("Exception initializer marker lacks exact throw ownership");
    }

    private void VerifyExceptionBases(ExceptionType descriptor, ExceptionBaseConversion[] candidates)
    {
        SourceType leaf = SourceTypeById(descriptor.LeafSourceTypeId);
        if (leaf.Kind != "record")
        {
            if (candidates.Length != 0) throw new InvalidDataException("Non-class exception leaf owns class base conversions");
            return;
        }
        RecordType record = RecordsByKey[Type(leaf.ExecutionTypeId)];
        var expected = new List<(long SourceTypeId, long Offset)>();
        void Collect(RecordType current, long parentOffset, int depth)
        {
            if (depth > 128) throw new InvalidDataException("Exception base nesting exceeds supported limit");
            foreach (SourceBase source in OriginalBases(current.SourceRecordId!.Value))
            {
                if (source.IsVirtual) throw new InvalidDataException("Virtual exception base conversions require an expanded layout contract");
                SourceType baseType = SourceTypeById(source.BaseSourceTypeId);
                if (!BaseSubobjects.TryGetValue((current.TypeId, source.Id), out RecordBaseSubobject? physical))
                    throw new InvalidDataException("Exception base lacks its observed direct subobject layout");
                long offset = checked(parentOffset + physical.ByteOffset);
                expected.Add((baseType.Id, offset));
                Collect(RecordsByKey[Type(baseType.ExecutionTypeId)], offset, depth + 1);
            }
        }
        Collect(record, 0, 0);
        var actual = candidates.Select(item => (SourceTypeId: item.BaseSourceTypeId, Offset: item.ByteOffset));
        if (!expected.OrderBy(item => item.SourceTypeId).ThenBy(item => item.Offset)
            .SequenceEqual(actual.OrderBy(item => item.SourceTypeId).ThenBy(item => item.Offset)) ||
            candidates.Any(item => item.ByteOffset > record.Size || RecordsByKey[Type(SourceTypeById(item.BaseSourceTypeId).ExecutionTypeId)].Size == 0))
            throw new InvalidDataException("Exception base candidates do not match complete original class subobject paths");
    }
}
