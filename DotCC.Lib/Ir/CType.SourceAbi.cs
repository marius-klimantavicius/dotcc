namespace DotCC.Ir;

public abstract partial record CType
{
    /// <summary>Sixteen-byte source x87 storage. Arithmetic/conversion are
    /// deliberately unsupported; raw storage copies retain all sixteen bytes.</summary>
    public sealed record LongDoubleStorage16 : CType
    {
        public override int SizeOf => 16;
    }
}
