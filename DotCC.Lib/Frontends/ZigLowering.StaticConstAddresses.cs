using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    // A file-as-struct's immutable declarations have the same program lifetime
    // as a container const. Demand their storage in the defining module, rather
    // than materializing a value in the function that happened to take its address.
    private CExpr? TryStaticModuleConstAddress(string name)
    {
        if (!_lazyValueConsts.ContainsKey(name)) return null;
        if (!_lazyConstStatics.TryGetValue(name, out var symbol))
        {
            using var definition = new DefinitionBindingScope(this);
            using var function = new FnStateScope(this);
            var value = LowerLazyValueConst(name);
            if (value is null || value.Type.Unqualified is not (CType.Named or CType.Array)) return null;
            // Lowering an array or statement-producing initializer can already
            // have established its static definition. Reuse that exact object.
            if (!_lazyConstStatics.TryGetValue(name, out symbol))
            {
                if (!IsStaticInitializer(value))
                {
                    var folded = _ir.ResolveComptimeFold(value);
                    if (folded is null || !IsStaticInitializer(folded)) return null;
                    value = folded;
                }
                symbol = _symbols.Declare(new Symbol
                {
                    Name = QualifyTypeName(name) + "__static", Kind = SymKind.Var,
                    Type = value.Type, Storage = Storage.Static, IsGlobal = true,
                });
                _ir.Globals.Add(new GlobalVar(symbol, value));
                _ir.ConstGlobalInits[symbol] = value;
                _lazyConstStatics[name] = symbol;
            }
        }
        symbol.AddressTaken = true;
        _zigConstBindings.Add(symbol);
        var pointer = SourcePointer(symbol.Type.WithQuals(TypeQual.Const));
        var reference = new VarRef(symbol) { Type = symbol.Type, IsLValue = true };
        return symbol.Type.Unqualified is CType.Array
            ? reference with { Type = pointer }
            : new Unary(UnOp.AddrOf, reference) { Type = pointer };
    }
}
