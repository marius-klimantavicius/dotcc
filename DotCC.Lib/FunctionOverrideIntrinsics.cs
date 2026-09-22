namespace DotCC;

// One closed registry shared by profile validation, C signature checking and
// emission. These targets retain ordinary call effects in the IR.
internal sealed record FunctionOverrideIntrinsic(bool Store, int Bytes, bool Signed)
{
    public string ManagedMethod => (Store ? "Write" : "Read") + (Signed ? "Int" : "UInt") + Bytes * 8 + "LittleEndian";

    public static FunctionOverrideIntrinsic? Find(string name) => name switch
    {
        "load.i32.le" => new(false, 4, true),
        "load.u16.le" => new(false, 2, false),
        "load.u32.le" => new(false, 4, false),
        "load.u64.le" => new(false, 8, false),
        "store.u16.le" => new(true, 2, false),
        "store.u32.le" => new(true, 4, false),
        "store.u64.le" => new(true, 8, false),
        _ => null,
    };
}
