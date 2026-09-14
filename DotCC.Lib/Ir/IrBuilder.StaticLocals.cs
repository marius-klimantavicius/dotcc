namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // These have block scope and no C linkage, despite their program-lifetime
    // backing fields. Separate object emission must qualify the exact shared
    // symbols; ordinary externally linked globals must retain their names.
    internal System.Collections.Generic.List<Symbol> StaticLocals { get; } = new();

    private void RegisterStaticLocal(Symbol symbol, CExpr? initializer)
    {
        Globals.Add(new GlobalVar(symbol, initializer));
        StaticLocals.Add(symbol);
        _symbols.DeclareAlias(symbol);
    }
}
