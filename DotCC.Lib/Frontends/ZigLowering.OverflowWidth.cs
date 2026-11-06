#nullable enable
using System.Collections.Generic;
using DotCC.Ir;

namespace DotCC.Frontends;

internal sealed partial class ZigLowering
{
    private readonly Dictionary<CExpr, int?[]> _tupleResultBits = new(System.Collections.Generic.ReferenceEqualityComparer.Instance);

    private int?[]? TupleResultBits(CExpr expression) => expression switch
    {
        _ when _tupleResultBits.TryGetValue(expression, out var bits) => bits,
        VarRef variable => _valueTupleElemBits.GetValueOrDefault(variable.Sym),
        Paren paren => TupleResultBits(paren.Inner),
        ComptimeFold fold => TupleResultBits(fold.Inner),
        _ => null,
    };

    private int? DeclaredTupleIndexBits(LALR.CC.LexicalGrammar.Item tuple, LALR.CC.LexicalGrammar.Item index)
        => tuple.Content is Zig.Ident identifier && _symbols.Resolve(Tok(identifier.Arg0)) is { } symbol
            && _valueTupleElemBits.TryGetValue(symbol, out var elements)
            && EvalComptimeValue(index) is LitInt { Value: { } position } && position >= 0 && position < elements.Length
                ? elements[(int)position] : null;

    // The existing helper detects overflow at the carrier width. A narrower
    // source integer also overflows when its carrier result changes on source
    // sign extension/truncation. Keep both tests and evaluate the helper once.
    private CExpr SourceOverflowResult(Call operation, int? bits)
    {
        CExpr result = operation;
        if (bits is { } width && operation.Type.Unqualified is CType.Tuple { Elements: [var element, _] }
            && element.Unqualified is CType.Prim { Integer: true } primitive
            && width > 0 && width < primitive.Bytes * 8)
        {
            var temporary = _symbols.Declare(new Symbol
            {
                Name = "__overflow" + _anfTempCounter++, Kind = SymKind.Var, Type = operation.Type,
            });
            var reference = new VarRef(temporary) { Type = temporary.Type };
            var raw = new TupleIndex(reference, 0, element) { Type = element };
            var overflow = new TupleIndex(reference, 1, CType.UChar) { Type = CType.UChar };
            var shift = IntLit(primitive.Bytes * 8 - width);
            var high = new Cast(element, new Binary(BinOp.Shl, raw, shift) { Type = element }) { Type = element };
            var normalized = new Cast(element, new Binary(BinOp.Shr, high, shift) { Type = element }) { Type = element };
            var changed = new Binary(BinOp.Ne, raw, normalized) { Type = CType.Bool };
            var overflowed = new Binary(BinOp.LogOr,
                new Binary(BinOp.Ne, overflow, IntLit(0)) { Type = CType.Bool }, changed) { Type = CType.Bool };
            var tuple = (CType.Tuple)operation.Type.Unqualified;
            result = new StatementExpression(new Block([new DeclStmt([new(temporary, operation)])]),
                new TupleNew([normalized, new Cast(CType.UChar, overflowed) { Type = CType.UChar }], tuple) { Type = tuple }) { Type = tuple };
        }
        _tupleResultBits[result] = [bits, 1];
        return result;
    }
}
