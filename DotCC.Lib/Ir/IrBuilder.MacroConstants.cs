using System.Collections.Generic;
using LALR.CC.LexicalGrammar;

namespace DotCC.Ir;

internal sealed partial class IrBuilder
{
    private bool _macroEvaluation;
    internal IEnumerable<string> MacroTypeNames => _typedefs.Keys;

    // Macro discovery is speculative. Give it the TU's type/layout environment
    // and private symbols, without sharing program bodies or diagnostic/import
    // sinks. In particular, sizeof(&global) must not mark the real global as
    // address-taken, and sizeof(external()) must not request a native import.
    internal IrBuilder CreateMacroEvaluator() => new(this);

    private IrBuilder(IrBuilder source)
    {
        _macroEvaluation = true;
        _symbols = source._symbols.CopyForEvaluation();
        _embeds = source._embeds;
        _warnings = source._warnings;
        _file = source._file;
        Copy(source._typedefs, _typedefs);
        Copy(source._structFields, _structFields);
        Copy(source._structIsUnion, _structIsUnion);
        Copy(source._enumTypes, _enumTypes);
        Copy(source._structPacks, _structPacks);
        Copy(source._structAlignments, _structAlignments);
        Copy(source._aggregateTags, _aggregateTags);
        Copy(source._promoted, _promoted);
        _packedStructs.UnionWith(source._packedStructs);
        _runtimeAggregateTags.UnionWith(source._runtimeAggregateTags);
        _emittedTypes.UnionWith(source._emittedTypes);

        static void Copy<T>(Dictionary<string, T> from, Dictionary<string, T> to)
        {
            foreach (var entry in from) to.Add(entry.Key, entry.Value);
        }
    }

    // Bind only the initializer, without registering a synthetic global or
    // allowing a replacement list to inject another declaration.
    internal CExpr? LowerMacroInitializer(Item root)
    {
        if (root.Content is C.FnsOne one) root = one.Arg0;
        if (root.Content is not C.GlobalDeclList global) return null;
        var list = global.Arg1;
        if (list.Content is C.DeclItemListOne single) list = single.Arg0;
        if (list.Content is not C.DeclItemInit initializer) return null;
        Diagnostics.Clear();
        var result = FoldMacroConstant(BuildExpr(initializer.Arg2));
        return Diagnostics.Exists(d => d.Severity == Severity.Error) ? null : result;
    }
}
