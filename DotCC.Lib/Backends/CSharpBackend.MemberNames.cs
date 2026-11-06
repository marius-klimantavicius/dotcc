using System;
using System.Collections.Generic;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly Dictionary<(string Owner, string Source), string> _enumMemberTargets = new();

    private static string FieldTarget(StructField field) => field.TargetName ?? EmitHelpers.Id(field.Name);

    private string FieldTarget(CType owner, string source)
    {
        while (owner.Unqualified is CType.Pointer pointer) owner = pointer.Pointee;
        if (owner.Unqualified is CType.Named named && _aggregateDefinitions.TryGetValue(named.Name, out var definition))
            foreach (var field in definition.Fields)
                if (field.Name == source) return FieldTarget(field);
        return EmitHelpers.Id(source);
    }

    private string EnumMemberTarget(CType owner, string source)
        => owner.Unqualified is CType.Enum en && _enumMemberTargets.TryGetValue((en.Name, source), out var target)
            ? target : EmitHelpers.Id(source);
}
