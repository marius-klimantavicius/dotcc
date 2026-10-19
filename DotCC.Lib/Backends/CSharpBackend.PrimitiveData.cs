using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DotCC.Ir;

namespace DotCC.Backends;

internal sealed partial class CSharpBackend
{
    private sealed record PrimitiveData(string Backing, string View, int Width, byte[] Bytes);

    private bool TryPrimitiveData(CType element, IReadOnlyList<CExpr> values, out PrimitiveData data)
    {
        data = null!;
        var physical = element.Unqualified is CType.Enum enumeration ? enumeration.Underlying : element;
        if (physical.Unqualified is not CType.Prim { Bytes: 1 or 2 or 4 or 8 } primitive) return false;
        // An explicit whitelist describes this backend's existing representations.
        // Neither pointer relocations nor Int128/Float128 become narrowed blobs.
        string view = Cs(element);
        if (element.Unqualified is not CType.Enum && view is not
            ("byte" or "sbyte" or "short" or "ushort" or "int" or "uint" or "long" or "ulong"
             or "char" or "CBool" or "float" or "double")) return false;
        int width = primitive.Bytes;
        string backing = width switch { 1 => "byte", 2 => "ushort", 4 => "uint", _ => "ulong" };
        var bytes = new byte[checked(values.Count * width)];
        for (int i = 0; i < values.Count; i++)
        {
            ulong bits;
            if (primitive.Integer)
            {
                var folded = _foldConstant(new Cast(physical, values[i]) { Type = physical });
                if (!PayloadInteger(folded, out var number)) return false;
                bits = unchecked((ulong)number);
            }
            else
            {
                if (_foldFloatingPayloadBits(values[i], physical) is not { } floating) return false;
                bits = floating;
            }
            for (int b = 0; b < width; b++) bytes[i * width + b] = (byte)(bits >> (b * 8));
        }
        data = new(backing, view, width, bytes);
        return true;
    }

    private static bool PayloadInteger(CExpr? value, out Int128 number)
    {
        bool negative = value is Unary { Op: UnOp.Neg };
        if (negative) value = ((Unary)value!).Operand;
        if (value is LitInt literal && Int128.TryParse(literal.Digits, NumberStyles.Integer,
            CultureInfo.InvariantCulture, out number))
        {
            if (negative) number = -number;
            return true;
        }
        number = 0;
        return false;
    }

    private string PrimitiveProperty(PrimitiveData data, int start, int count)
    {
        string backing = _literals.AddPayload(data.Backing, data.Bytes.AsSpan(start * data.Width, count * data.Width));
        if (data.View == data.Backing) return backing;
        // A named typed getter borrows the same fixed-width RVA data. Enum,
        // signed, char, CBool and floating views do no scalar conversions.
        string body = "    public static global::System.ReadOnlySpan<" + data.View + "> Values => "
            + "global::System.Runtime.InteropServices.MemoryMarshal.Cast<" + data.Backing + ", " + data.View + ">(" + backing + ");\n";
        string name = "__DotccPrimitiveView_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
        _aggregateFactories.TryAdd(name, "internal static class " + name + "\n{\n" + body + "}\n");
        return name + ".Values";
    }

    private IEnumerable<(int Offset, int Count)> PrimitiveChunks(PrimitiveData data, bool zeroed)
    {
        int length = data.Bytes.Length / data.Width, index = 0;
        bool Zero(int i) => data.Bytes.AsSpan(i * data.Width, data.Width).IndexOfAnyExcept((byte)0) < 0;
        while (index < length)
        {
            int start = index;
            while (index < length && index - start < LiteralPool.PayloadElements)
            {
                if (zeroed && Zero(index))
                {
                    int end = index + 1;
                    while (end < length && Zero(end)) end++;
                    if (end - index >= LiteralPool.PayloadElements)
                    {
                        if (index != start) yield return (start, index - start);
                        index = start = end;
                        continue;
                    }
                    // Consume a short zero run once; avoid rescanning a dense
                    // near-boundary run once for every zero element.
                    index = Math.Min(end, start + LiteralPool.PayloadElements);
                    continue;
                }
                index++;
            }
            if (index != start) yield return (start, index - start);
        }
    }

    private bool EmitPrimitiveCopies(StringBuilder output, string destination, CType element,
        IReadOnlyList<CExpr> values, bool zeroed)
    {
        if (!TryPrimitiveData(element, values, out var data)) return false;
        foreach (var chunk in PrimitiveChunks(data, zeroed))
        {
            string property = PrimitiveProperty(data, chunk.Offset, chunk.Count);
            output.Append("        ").Append(property).Append(".CopyTo(new global::System.Span<")
                .Append(data.View).Append(">((").Append(data.View).Append("*)(").Append(destination).Append(')');
            if (chunk.Offset != 0) output.Append(" + ").Append(chunk.Offset);
            output.Append(", ").Append(chunk.Count).Append("));\n");
        }
        return true;
    }

    private bool TryPrimitiveArrayText(PinnedArray array, int requestedAlignment, out string text)
    {
        text = "";
        if (array.Elems is not { Count: > 0 } values || !TryPrimitiveData(array.Element, values, out var data)) return false;
        int alignment = Math.Max(requestedAlignment, TypeAlignment(array.Element));
        bool aligned = requestedAlignment > 0 || alignment > 8;
        if (values.Count <= LiteralPool.PayloadElements)
        {
            string property = PrimitiveProperty(data, 0, values.Count);
            text = "Libc.Global" + (aligned ? "AlignedFrom" : "ArrayFrom") + "<" + data.View + ">(" + property
                + (aligned ? ", " + alignment : "") + ")";
            return true;
        }
        // Expression contexts still need owned C storage. Reuse the existing
        // typed helper-declaration machinery instead of a temporary T[] payload.
        var stores = new StringBuilder();
        EmitPrimitiveCopies(stores, "destination", array.Element, values, zeroed: true);
        var helpers = new StringBuilder();
        string calls = InitializerHelpers(helpers, stores.ToString(), data.View + "*", "destination", "Copy");
        string body = "    public static unsafe " + data.View + "* Create()\n    {\n        " + data.View
            + "* destination = Libc.Global" + (aligned ? "AlignedZeroed" : "ArrayZeroed") + "<" + data.View + ">(" + values.Count
            + (aligned ? ", " + alignment : "") + ");\n" + calls + "        return destination;\n    }\n" + helpers;
        string name = "__DotccPrimitiveInit_" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
        _aggregateFactories.TryAdd(name, "internal static class " + name + "\n{\n" + body + "}\n");
        text = name + ".Create()";
        return true;
    }
}
