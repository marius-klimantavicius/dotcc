#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private readonly Dictionary<Symbol, string> _repeatedArrayStorage = new(ReferenceEqualityComparer.Instance);

    // CLR localloc lives until the method returns, unlike a fixed C automatic
    // array whose lifetime ends when its block exits. Reuse one lazy allocation
    // per lexical fixed array when control can revisit its declaration. The
    // initializer still executes at every original entry. Slots belong to this
    // invocation, so recursive calls and concurrently active blocks stay distinct.
    private void PrepareRepeatedArrayStorage(Block body)
    {
        _repeatedArrayStorage.Clear();
        bool jumps = false;
        WalkAddressStatements(body, statement =>
        {
            jumps |= statement is Goto or SetjmpCapture;
            if (statement is ArrayDecl array) _addressLocalNames.Add(array.Sym.TargetName);
        }, _ => { });
        void Walk(CStmt statement, bool repeated)
        {
            if (statement is ArrayDecl array && repeated && FixedArrayCount(array) > 0
                && !_repeatedArrayStorage.ContainsKey(array.Sym))
            {
                string name;
                do { name = "__dotcc_array_storage" + _clCounter++; }
                while (!_addressLocalNames.Add(name) || !_addressLocalNames.Add(name + "_new"));
                _repeatedArrayStorage.Add(array.Sym, name);
            }
            var loop = statement is While or DoWhile or For or SetjmpCapture;
            foreach (var child in statement is CaseLabelStmt label ? new[] { label.Body } : AddressStatementChildren(statement))
                Walk(child, repeated || loop && !(statement is For f && ReferenceEquals(child, f.Init)));
        }
        Walk(body, jumps);
    }

    private static int FixedArrayCount(ArrayDecl array)
    {
        if (array.Inits is { } values) return values.Count;
        if (array.CountExpr is not LitInt { Value: > 0 and <= int.MaxValue } count) return 0;
        var rows = array.Element.Unqualified is CType.Array row ? FlatCount(row) : 1;
        return checked((int)count.Value * rows);
    }

    private string RepeatedArrayDeclarations() => string.Concat(_repeatedArrayStorage.Values
        .Select(name => "byte* " + name + " = null;\n"));

    private bool EmitRepeatedArray(StringBuilder output, string pad, ArrayDecl array)
    {
        if (!_repeatedArrayStorage.TryGetValue(array.Sym, out var storage)) return false;
        var element = array.Inits is null && array.Element.Unqualified is CType.Array row ? row.FlatElement : array.Element;
        var type = Cs(element);
        var count = FixedArrayCount(array).ToString(CultureInfo.InvariantCulture);
        var bytes = "checked(sizeof(" + type + ") * " + count + ")";
        var alignment = Math.Max(array.Sym.Alignment, TypeAlignment(element));
        var align = array.Sym.Alignment != 0 || alignment > 8;
        _alignedArraySymbols.Add(array.Sym);
        output.Append(pad).Append("if (").Append(storage).Append(" == null)\n").Append(pad).Append("{\n");
        output.Append(pad).Append("    byte* ").Append(storage).Append("_new = stackalloc byte[")
            .Append(align ? "checked(" + bytes + " + " + (alignment - 1) + ")" : bytes).Append("];\n");
        output.Append(pad).Append("    ").Append(storage).Append(" = ").Append(storage).Append("_new;\n")
            .Append(pad).Append("}\n");
        output.Append(pad).Append(type).Append("* ").Append(array.Sym.TargetName).Append(" = (").Append(type).Append("*)")
            .Append(align ? "(((nuint)" + storage + " + " + (alignment - 1) + ") & ~(nuint)" + (alignment - 1) + ")" : storage)
            .Append(";\n");
        output.Append(pad).Append("new global::System.Span<byte>((void*)").Append(array.Sym.TargetName)
            .Append(", ").Append(bytes).Append(").Clear();\n");
        if (array.Inits is { } initializers && !EmitAggregateDataCopies(output, array.Sym.TargetName, array.Element, initializers))
            for (var index = 0; index < initializers.Count; index++)
            {
                if (initializers[index] is LitInt { Value: 0 }) continue;
                var value = Hoist(output, pad, () => Coerced(initializers[index], array.Element));
                output.Append(pad).Append(array.Sym.TargetName).Append('[').Append(index).Append("] = ")
                    .Append(value).Append(";\n");
            }
        return true;
    }
}
