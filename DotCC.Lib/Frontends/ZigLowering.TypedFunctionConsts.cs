#nullable enable
using System.Linq;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // An annotation on a source function constant is a signature, not a reason
    // to discard the callable declaration. Resolve both sides in its definition
    // environment, including the container's own generic seeds.
    private Symbol? ResolveTypedContainerFunctionConst(string container, string name, Item typeItem, Item rhs)
    {
        using var definition = new DefinitionBindingScope(this);
        using var seeds = EnterReifiedSeeds(container);
        using var scope = EnterContainer(container);
        if (LowerType(typeItem).Unqualified is not CType.Func expected) return null;
        var sibling = rhs.Content is Zig.Ident id ? EnsureMethodDeclared(container, Tok(id.Arg0)) : null;
        var function = sibling is not null && !IsFnTemplate(sibling) ? sibling
            : TryResolveComptimeFnValue(rhs, this)?.Fn;
        if (function is null) return null;
        if (function.Type.Unqualified is not CType.Func actual
            || expected.Variadic != actual.Variadic
            || !EquivalentFunctionParameterType(expected.Return, actual.Return)
            || expected.Params.Count != actual.Params.Count
            || !expected.Params.Zip(actual.Params).All(pair => EquivalentFunctionParameterType(pair.First, pair.Second)))
            throw new CompileException($"zig: container function constant '{container}.{name}' has an incompatible declared signature");
        _containerFnConsts[(container, name)] = function;
        return function;
    }
}
