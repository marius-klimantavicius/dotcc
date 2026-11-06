#nullable enable
using System.Collections.Generic;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly HashSet<(string Container, string Name)> _containerModuleLookups = new();
    private readonly Dictionary<(string Container, string Name), ZigModule> _containerModuleAliases = new();

    private ZigModule? ResolveLexicalModule(string name) =>
        TryContainerModuleAlias(name, out var scoped) ? scoped : ResolveNamedModule(name);

    private bool TryContainerModuleAlias(string name, out ZigModule? module)
    {
        module = null;
        if (!UsesSourceStdlib) return false;
        // A runtime local cannot turn into a same-spelled imported namespace.
        if (_symbols.Resolve(name) is { IsGlobal: false }) return true;
        for (var container = _currentContainer ?? _currentConstContainer; container is not null;
            container = _containerParents.GetValueOrDefault(container))
        {
            if (!_containerConsts.TryGetValue(container, out var declarations)
                || !declarations.TryGetValue(name, out var declaration)) continue;
            // Finding a non-module declaration still shadows an outer import.
            if (declaration.typeItem is not null) return true;
            var key = (container, name);
            if (_containerModuleAliases.TryGetValue(key, out module)) return true;
            if (!_containerModuleLookups.Add(key))
                throw new Ir.IrUnsupportedException($"zig: container '{container}' module alias '{name}' has a dependency loop");
            try
            {
                using var seeds = EnterReifiedSeeds(container);
                module = ContainerConstModule(container, name);
                if (module is not null) _containerModuleAliases[key] = module;
                return true;
            }
            finally { _containerModuleLookups.Remove(key); }
        }
        return false;
    }
}
