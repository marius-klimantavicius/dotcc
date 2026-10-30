#nullable enable

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using DotCC.Ir;
using LALR.CC.LexicalGrammar;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private IrBuilder.ComptimeSession? _typeBodyValueSession;

    // Module lowerings share an IR; an enum's exact tag width must cross the
    // same boundary as its type. A trailing hole can require more bits than
    // the largest surviving field, so inferring it from fields is insufficient.
    private static readonly ConditionalWeakTable<IrBuilder, Dictionary<string, int>> TypeBodyEnumWidths = new();

    private void RecordTypeBodyEnumWidth(string name, int? bits)
    {
        if (bits is { } width) { TypeBodyEnumWidths.GetOrCreateValue(_ir)[name] = width; }
    }

    private int? TypeBodyEnumWidth(string name)
        => TypeBodyEnumWidths.TryGetValue(_ir, out var widths) && widths.TryGetValue(name, out var bits) ? bits : null;

    // Keep one frame for the whole type body: tuple members and slices must
    // refer to the arrays the body actually mutated, including across statements.
    private void RunTypeBodyValueStatement(string fnName, Item statement)
    {
        var session = _typeBodyValueSession
            ?? throw new IrUnsupportedException($"type-returning generic '{fnName}': no compile-time value session");
        // Re-lower each outer loop iteration against its current published
        // locals. Lowering the entire loop at once could fold a changing guard
        // against its initial value and discard a branch needed later.
        // The shared executor rejects a return here: a type-returning loop
        // needs TypeBodyResult propagation before that control flow is supported.
        if (statement.Content is Zig.ForStmt or Zig.WhileStmt)
        {
            ExecuteTopComptimeStmt(statement, session);
            return;
        }
        if (ExecuteComptimeSessionStatement(session, statement) is not { Returned: false })
        {
            throw new IrUnsupportedException($"type-returning generic '{fnName}': a body statement must evaluate at compile time"
                + (_ir.ComptimeMiss is { } why ? $" (the interpreter stopped at {why})" : ""));
        }
    }

    // Reification binds these values again in the returned type's methods.
    // Capture after all stores, before EndComptimeSession withdraws the frame.
    private void CaptureTypeBodyValues(IrBuilder.ComptimeSession session)
    {
        foreach (var (symbol, value) in session.Frame)
        {
            if (!ReferenceEquals(_symbols.Resolve(symbol.Name), symbol)) { continue; }
            _typeBodyValueLocals?.RemoveAll(local => local.Name == symbol.Name);
            _typeBodyAggregateLocals?.RemoveAll(local => local.Name == symbol.Name);
            if (value is IrBuilder.CtInt integer && integer.Value >= long.MinValue && integer.Value <= long.MaxValue)
            {
                var type = symbol.Type;
                if (type.Unqualified is CType.Prim { IsComptimeInt: true })
                    type = integer.Value >= int.MinValue && integer.Value <= int.MaxValue ? CType.Int : CType.Long;
                _typeBodyValueLocals?.Add((symbol.Name, (long)integer.Value, type));
            }
            else { _typeBodyAggregateLocals?.Add((symbol.Name, value, symbol.Type)); }
        }
    }

    private IrBuilder.ComptimeValue? EvalTypeBodyValue(Item expression, CType sink)
    {
        using var hoist = EnterFreshHoist();
        var value = LowerExprSink(expression, sink);
        if (_hoist is { Count: > 0 } statements)
        {
            var session = _typeBodyValueSession ?? _ir.BeginComptimeSession();
            try
            {
                if (_ir.RunComptimeSessionStmt(session, TopComptimeStores(new Seq(statements))) is not { Returned: false })
                    return null;
                return _ir.EvalComptimeValue(value);
            }
            finally { if (_typeBodyValueSession is null) { _ir.EndComptimeSession(session); } }
        }
        return _ir.EvalComptimeValue(value);
    }

}
