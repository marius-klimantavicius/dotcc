#nullable enable
using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // Retain operand types for layout operations which fold before backend selection.
    // A runtime-backed target type may have no unmanaged layout on one backend.
    internal readonly record struct RuntimeLayoutUse(CType Type, string Operation, SrcPos Pos);
    internal List<RuntimeLayoutUse> RuntimeLayoutUses { get; } = new();
}
