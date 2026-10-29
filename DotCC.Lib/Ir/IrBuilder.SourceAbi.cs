namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    internal CSourceAbi SourceAbi { get; init; } = new();
    private CType SourceLongDouble => SourceAbi.LongDouble == CLongDoubleRepresentation.X87Storage16
        ? new CType.LongDoubleStorage16() : CType.LongDouble;
}
