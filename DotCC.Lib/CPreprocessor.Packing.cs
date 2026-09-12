#nullable enable
using System;
using System.Collections.Generic;
using LALR.CC.LexicalGrammar;

namespace DotCC;

internal sealed partial class CPreprocessor
{
    private int _packing;
    private readonly List<(string? Label, int Packing)> _packingStack = new();

    private void ApplyPackingPragma(IReadOnlyList<Item> args)
    {
        if (args.Count < 3 || args[1].Content as string != "(" || args[^1].Content as string != ")")
            throw new CompileException("malformed #pragma pack");
        var values = new List<string>();
        for (int i = 2; i < args.Count - 1; ++i)
        {
            if ((i & 1) == 1)
            {
                if (args[i].Content as string != ",") throw new CompileException("malformed #pragma pack");
            }
            else values.Add(args[i].Content?.ToString() ?? "");
        }
        if (args.Count > 3 && (args.Count & 1) == 1) throw new CompileException("malformed #pragma pack");
        if (values.Count == 0) { _packing = 0; return; }
        if (values[0] is not ("push" or "pop"))
        {
            if (values.Count != 1) throw new CompileException("malformed #pragma pack");
            _packing = PackingValue(values[0]);
            return;
        }
        string? label = null;
        int? requested = null;
        for (int i = 1; i < values.Count; ++i)
        {
            var value = values[i];
            if (value.Length > 0 && char.IsAsciiDigit(value[0]))
            {
                if (requested.HasValue || i != values.Count - 1) throw new CompileException("malformed #pragma pack");
                requested = PackingValue(value);
            }
            else
            {
                if (label is not null || value.Length == 0) throw new CompileException("malformed #pragma pack");
                label = value;
            }
        }
        if (values[0] == "push") _packingStack.Add((label, _packing));
        else
        {
            int match = label is null ? _packingStack.Count - 1
                : _packingStack.FindLastIndex(entry => entry.Label == label);
            if (match < 0) _diag.WriteLine("dotcc: #pragma pack(pop) has no matching push");
            else
            {
                _packing = _packingStack[match].Packing;
                _packingStack.RemoveRange(match, _packingStack.Count - match);
            }
        }
        if (requested is { } packing) _packing = packing;
    }

    private static int PackingValue(string value) => value switch
    {
        "0" => 0, "1" => 1, "2" => 2, "4" => 4, "8" => 8, "16" => 16,
        _ => throw new CompileException("#pragma pack requires 0, 1, 2, 4, 8, or 16"),
    };
}
