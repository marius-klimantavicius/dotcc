namespace DotObj;

// Named fields retain their CLR layout context (explicit offsets and Pack).
// A raw typed dereference does not retain the containing packed field's layout;
// only that boundary requires an alignment-aware access when its address has a
// weaker guarantee than the pointed-to type's own layout.
internal sealed partial class ObjectFile
{
    public long AccessAlignment(string type) => ScalarTypes.IsArray(type)
        ? AccessAlignment(ScalarTypes.Element(type))
        : ScalarTypes.IsRecord(type) ? ClrRecordPack(RecordsByKey[type])
        : Math.Max(1, StorageAlignment(type));
}

internal sealed class AccessAlignment(ObjectFile obj, FunctionBody body)
{
    private readonly Dictionary<long, long> places = [];
    private readonly record struct Origin(long? KnownAlignment, bool HasUnknown);
    private readonly Dictionary<long, Origin> pointerOrigins = [];
    public static long AtOffset(long alignment, long offset)
    {
        while (alignment > 1 && offset % alignment != 0) alignment /= 2;
        return Math.Max(1, alignment);
    }
    private string Type(Node node) => obj.Type(node.TypeId!.Value);
    private Node SequenceTail(Node node) => body.Nodes[body.Children(node.Id, "item").Last().ChildId];
    public bool NeedsUnaligned(Node node) => node.Kind == "sequence" ? NeedsUnaligned(SequenceTail(node)) : node.Kind == "lifetime-scope" ? NeedsUnaligned(body.Child(node.Id, "operand")) : node.ValueCategory == "place" &&
        node.Kind is "dereference" or "storage" &&
        Place(node) < obj.AccessAlignment(Type(node));
    public long Place(Node node)
    {
        if (places.TryGetValue(node.Id, out long value)) return value;
        value = node.Kind switch
        {
            "sequence" => Place(SequenceTail(node)),
            "lifetime-scope" => Place(body.Child(node.Id, "operand")),
            "base-subobject" => AtOffset(Place(body.Child(node.Id, "operand")), obj.BaseSubobjects[(body.Child(node.Id, "operand").TypeId!.Value, obj.BaseSubobjectExpressions[node.Id])].ByteOffset),
            "member" => AtOffset(Place(body.Child(node.Id, "operand")), obj.Fields[node.FieldId!.Value].ByteOffset),
            "dereference" => Pointer(body.Child(node.Id, "operand")),
            "volatile-place" => Place(body.Child(node.Id, "operand")),
            "storage" => Math.Max(1, obj.Storages[node.StorageId!.Value].Alignment),
            _ => Math.Max(1, obj.StorageAlignment(Type(node))),
        };
        places.Add(node.Id, value); return value;
    }
    private long NaturalPointerAlignment(Node node)
    {
        string type = Type(node), pointee = ScalarTypes.IsPointer(type) ? ScalarTypes.Pointee(type) : "void";
        return pointee == "void" || RoutineTypes.IsRoutine(pointee) ? 1 : Math.Max(1, obj.StorageAlignment(pointee));
    }
    public long Pointer(Node node)
    {
        Origin origin = PointerOrigin(node);
        long natural = NaturalPointerAlignment(node);
        long known = origin.KnownAlignment ?? natural;
        return origin.HasUnknown ? Math.Min(known, natural) : known;
    }

    // Opaque void*/char* values do not establish that a typed cast is
    // underaligned. Preserve observed storage origins and stronger source
    // typed-pointer contracts when casts erase that information. A conditional
    // can contain both known origins and unknown paths; keep those separate so
    // a later typed cast applies its contract only to the unknown paths.
    private Origin PointerOrigin(Node node)
    {
        if (pointerOrigins.TryGetValue(node.Id, out Origin value)) return value;
        value = node.Kind switch
        {
            "sequence" => PointerOrigin(SequenceTail(node)),
            "lifetime-scope" => PointerOrigin(body.Child(node.Id, "operand")),
            "address" or "array-decay" => new(Place(body.Child(node.Id, "operand")), false),
            "cleanup-payload-address" => new(obj.ExceptionTypes[obj.ThrowExpressions[obj.CleanupPayloadTargets[node.Id].ThrowNodeId].ExceptionTypeId].PayloadAlignment, false),
            "storage-address" => new(AtOffset(Math.Max(1, obj.Storages[node.StorageId!.Value].Alignment), long.Parse(node.ConstantText!, System.Globalization.CultureInfo.InvariantCulture)), false),
            "convert" when ScalarTypes.IsPointer(Type(body.Child(node.Id, "operand"))) => SourcePointerOrigin(body.Child(node.Id, "operand")),
            "binary" when node.Operator is "pointer-add" or "pointer-subtract" => OffsetPointer(node),
            "conditional" => MergeOrigins(PointerOrigin(body.Child(node.Id, "then")), PointerOrigin(body.Child(node.Id, "else"))),
            _ => new(null, true),
        };
        pointerOrigins.Add(node.Id, value); return value;
    }
    private Origin SourcePointerOrigin(Node node)
    {
        Origin origin = PointerOrigin(node);
        string pointee = ScalarTypes.Pointee(Type(node));
        if (!origin.HasUnknown || pointee is "void" or "i8" or "u8" || RoutineTypes.IsRoutine(pointee)) return origin;
        long natural = NaturalPointerAlignment(node);
        return new(origin.KnownAlignment is long known ? Math.Min(known, natural) : natural, false);
    }
    private static Origin MergeOrigins(Origin left, Origin right) => new(
        left.KnownAlignment is long a && right.KnownAlignment is long b ? Math.Min(a, b) : left.KnownAlignment ?? right.KnownAlignment,
        left.HasUnknown || right.HasUnknown);
    private Origin OffsetPointer(Node node)
    {
        Node left = body.Child(node.Id, "left"), right = body.Child(node.Id, "right");
        Node pointer = ScalarTypes.IsPointer(Type(left)) ? left : right;
        Origin origin = SourcePointerOrigin(pointer);
        return new(origin.KnownAlignment is long alignment
            ? AtOffset(alignment, obj.StorageSize(ScalarTypes.Pointee(Type(pointer)))) : null, origin.HasUnknown);
    }
}
