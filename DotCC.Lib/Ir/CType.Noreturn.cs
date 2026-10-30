#nullable enable

namespace DotCC.Ir;

public abstract partial record CType
{
    /// <summary>Zig's uninhabited type. Unlike void, no value or storage can exist.</summary>
    public sealed record NoReturnType : CType
    {
        public override int SizeOf => throw new IrUnsupportedException("no size available for uninstantiable type 'noreturn'");
    }

    public static readonly CType NoReturn = new NoReturnType();
}
