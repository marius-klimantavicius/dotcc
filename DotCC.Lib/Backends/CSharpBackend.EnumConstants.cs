using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private void AddEnumConstants(IrBuilder unit, IDictionary<string, string> declarations)
    {
        foreach (var group in unit.EnumConstants.GroupBy(e => e.Name))
        {
            var name = EmitHelpers.Id(group.Key);
            // Enum exports use the underlying integer type, including anonymous
            // enums. Store all candidates so selection can happen at object link.
            var fields = group.Select(e => $"    public const {Cs(e.Underlying)} {name} = unchecked(({Cs(e.Underlying)})({e.Value.ToString(CultureInfo.InvariantCulture)}L));\n")
                .Distinct(StringComparer.Ordinal).ToArray();
            declarations.Add(Compiler.EnumConstantPrefix + name, fields.Length == 1 ? fields[0] : "");
        }
    }
}
