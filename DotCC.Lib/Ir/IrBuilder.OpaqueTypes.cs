using System;
using System.Collections.Generic;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    // Tags referenced by C source, independently of the complete layout table.
    // An opaque handle needs a target-language identity behind its pointer, but
    // an empty emitted declaration must never masquerade as a known C layout.
    private readonly Dictionary<string, bool> _aggregateTags = new(StringComparer.Ordinal);
    private readonly HashSet<string> _runtimeAggregateTags = new(StringComparer.Ordinal);

    private CType ReferenceAggregate(global::LALR.CC.LexicalGrammar.Item tag, bool isUnion)
    {
        var name = Tok(tag);
        // time.h and locale.h deliberately omit these bodies: the library owns
        // their storage and imported APIs. Recognize this header contract only
        // at synthetic declarations, never by an arbitrary user's tag spelling.
        if (tag.Position.Line >= SrcPos.SyntheticLineBase && name is "tm" or "timespec" or "lconv")
            RegisterRuntimeAggregate(name);
        _aggregateTags.TryAdd(name, isUnion);
        return new CType.Named(name);
    }

    private void RequireCompleteObject(CType type, string use)
    {
        switch (type.Unqualified)
        {
            case CType.Named named when _aggregateTags.ContainsKey(named.Name)
                && !_structFields.ContainsKey(named.Name) && !_runtimeAggregateTags.Contains(named.Name):
                throw new IrUnsupportedException($"{use} requires a complete type: incomplete aggregate '{named.Name}'");
            case CType.Func { IsFunctionType: true }:
                throw new IrUnsupportedException(use + " requires an object type, not a function type");
            case CType.Array array:
                RequireCompleteObject(array.Element, use);
                break;
        }
    }

    private CExpr BuildSizeOf(CType type)
    {
        RequireCompleteObject(type, "sizeof");
        return new SizeOfExpr(type) { Type = CType.SizeT };
    }

    /// <summary>Run after every input TU has bound. File-scope tentative objects
    /// may precede their completed type; unresolved objects cannot be emitted.
    /// Remaining tags become identity-only types for pointer signatures. They
    /// deliberately stay absent from the compiler's aggregate layout table.</summary>
    internal void FinishAggregateTypes()
    {
        foreach (var global in Globals) RequireCompleteObject(global.Sym.Type, "object '" + global.Sym.Name + "'");
        foreach (var tag in _aggregateTags)
        {
            if (!_runtimeAggregateTags.Contains(tag.Key) && _emittedTypes.Add(tag.Key))
                Types.Add(new StructTypeDef(tag.Key, System.Array.Empty<StructField>(), tag.Value, IsIncomplete: true));
        }
    }
}
