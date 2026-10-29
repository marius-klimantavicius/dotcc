namespace DotCC.Ir;

public abstract partial record CType
{
    /// <summary>GNU scalar typedef alignment; zero retains natural alignment.
    /// This affects storage/layout, while the scalar remains compatible with
    /// its underlying C type. It may be weaker than natural alignment.</summary>
    public int AlignmentOverride { get; init; }
}
