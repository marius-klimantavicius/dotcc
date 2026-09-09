using System.Collections.Generic;
using LALR.CC;

namespace DotCC;

public static partial class C
{
    /// <summary>Use the generated grammar, parse table and identity actions,
    /// recording file provenance when each action builds its AST object.</summary>
    internal static Parser BuildSourceLocatedParser()
    {
        var actions = BuildActions(IdentityVisitor.Instance);
        var groups = new PrecedenceGroup[Definition.PrecedenceGroups.Length];
        var productionIndex = 0;
        for (var index = 0; index < groups.Length; ++index)
        {
            var source = Definition.PrecedenceGroups[index];
            var productions = new List<Production>();
            foreach (var production in source.Productions)
            {
                // Generated alongside BuildActions; the ordering is the same
                // one used by the generated BuildParser visitor overload.
                var actionName = _productionActionNames[productionIndex++];
                if (actionName is not null && actions.TryGetValue(actionName, out var action))
                    productions.Add(new Production(production.Left,
                        (lhs, children) => SourceFileOrigin.Attach(action(lhs, children), children), production.Right));
                else productions.Add(production);
            }
            groups[index] = new PrecedenceGroup(source.Derivation, productions.ToArray());
        }
        return new Parser(new Grammar(Definition.SymbolNames, groups), ParseTable);
    }
}
