#nullable enable

using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // These gates select the existing registry rows as one coherent family. The dictionaries
    // remain static data. Only the module graph owns the effective invocation-local selection.
    private bool UsesSourceStdlib => _moduleGraph?.StdlibTypes == ZigStdlibTypeModel.Source;
    private bool UsesManagedFunctionPointers => _moduleGraph?.FunctionPointerAbi == ZigFunctionPointerAbi.Managed;
    private bool UsesCuratedStdType(string path) => !UsesSourceStdlib && StdTypes.ContainsKey(path);
    private bool UsesCuratedStdGeneric(string path) => !UsesSourceStdlib && StdGenericTypes.ContainsKey(path);
    private bool UsesCuratedStdAllocator(string path) => !UsesSourceStdlib && StdAllocatorValues.ContainsKey(path);

    // Namespace operations (mem/debug/testing) retain their existing typed compiler lowering.
    // This policy makes no claim to replace every standard-library intrinsic with source.
    private void RejectSourceCuratedCarrier(CType type, string operation)
    {
        if (UsesSourceStdlib && type.Unqualified is CType.Allocator or CType.ZigList
                or CType.Named { Name: VTableTypeName or AlignmentTypeName or FbaTypeName or ArenaTypeName })
            throw new IrUnsupportedException($"zig stdlib_types source: {operation} cannot use legacy curated carrier {type.Describe()}");
    }
}
