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
    private readonly Dictionary<long, long> places = [], pointers = [];
    public static long AtOffset(long alignment, long offset)
    {
        while (alignment > 1 && offset % alignment != 0) alignment /= 2;
        return Math.Max(1, alignment);
    }
    private string Type(Node node) => obj.Type(node.TypeId!.Value);
    public bool NeedsUnaligned(Node node) => node.ValueCategory == "place" &&
        node.Kind is "dereference" or "storage" &&
        Place(node) < obj.AccessAlignment(Type(node));
    public long Place(Node node)
    {
        if (places.TryGetValue(node.Id, out long value)) return value;
        value = node.Kind switch
        {
            "member" => AtOffset(Place(body.Child(node.Id, "operand")), obj.Fields[node.FieldId!.Value].ByteOffset),
            "dereference" => Pointer(body.Child(node.Id, "operand")),
            "volatile-place" => Place(body.Child(node.Id, "operand")),
            "storage" => Math.Max(1, obj.Storages[node.StorageId!.Value].Alignment),
            _ => Math.Max(1, obj.StorageAlignment(Type(node))),
        };
        places.Add(node.Id, value); return value;
    }
    public long Pointer(Node node)
    {
        if (pointers.TryGetValue(node.Id, out long value)) return value;
        string type = Type(node), pointee = ScalarTypes.IsPointer(type) ? ScalarTypes.Pointee(type) : "void";
        long natural = pointee == "void" || RoutineTypes.IsRoutine(pointee) ? 1 : Math.Max(1, obj.StorageAlignment(pointee));
        value = node.Kind switch
        {
            "address" or "array-decay" => Place(body.Child(node.Id, "operand")),
            "storage-address" => AtOffset(Math.Max(1, obj.Storages[node.StorageId!.Value].Alignment), long.Parse(node.ConstantText!, System.Globalization.CultureInfo.InvariantCulture)),
            "convert" when ScalarTypes.IsPointer(Type(body.Child(node.Id, "operand"))) => Math.Min(natural, Pointer(body.Child(node.Id, "operand"))),
            "binary" when node.Operator is "pointer-add" or "pointer-subtract" => OffsetPointer(node),
            "conditional" => Math.Min(Pointer(body.Child(node.Id, "then")), Pointer(body.Child(node.Id, "else"))),
            _ => natural,
        };
        pointers.Add(node.Id, value); return value;
    }
    private long OffsetPointer(Node node)
    {
        Node left = body.Child(node.Id, "left"), right = body.Child(node.Id, "right");
        Node pointer = ScalarTypes.IsPointer(Type(left)) ? left : right;
        return AtOffset(Pointer(pointer), obj.StorageSize(ScalarTypes.Pointee(Type(pointer))));
    }
}
