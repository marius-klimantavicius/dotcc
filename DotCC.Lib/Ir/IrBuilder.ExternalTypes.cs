using System;
using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private readonly Dictionary<string, ExternalTypeOverride> _externalTypes = new(StringComparer.Ordinal);

    internal void ConfigureExternalTypes(IReadOnlyList<ExternalTypeOverride>? types)
    {
        foreach (var type in types ?? Array.Empty<ExternalTypeOverride>())
        {
            _externalTypes.Add(type.Name, type);
            _typedefs.Add(type.Name, new CType.Named(type.Name) { IsExternal = true, ExternalLayout = type.Layout });
        }
    }

    private void RejectExternalDefinition(string name)
    {
        if (_externalTypes.ContainsKey(name))
            throw new IrUnsupportedException("C definition conflicts with externalTypes registration: " + name);
    }
}
