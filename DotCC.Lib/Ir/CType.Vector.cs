namespace DotCC.Ir;

public abstract partial record CType
{
    /// <summary>A declared GNU vector alias whose lowering is not implemented.
    /// Retain its type identity for unused declarations; it is never a scalar
    /// alias or a usable layout. ResolveTypeName rejects materialization.</summary>
    public sealed record UnsupportedVector(CType Element, int Bytes) : CType
    {
        public override int SizeOf => throw new IrUnsupportedException(
            "GNU vector_size(" + Bytes + ") requires vector type lowering");
    }
}
