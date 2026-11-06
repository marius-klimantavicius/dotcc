using System;
using System.Collections.Generic;
using DotCC.Ir;
using DotCC.Layout;

namespace DotCC.Backends;

internal sealed partial class CSharpTarget
{
    internal Func<CType, LayoutInfo>? OptionalPayloadLayout { get; set; }
    private readonly Dictionary<string, (string Payload, int Size, int Alignment)> _alignedOptionals = new(StringComparer.Ordinal);

    private string RenderValueOptional(CType payload)
    {
        var rendered = RenderType(payload);
        var layout = OptionalPayloadLayout?.Invoke(payload);
        if (layout is { Size: 0, Alignment: 1 }) return "ZigOptionalZero<" + rendered + ">";
        if (layout is not { Alignment: > 8 }) return rendered + "?";
        if (layout.Alignment > 128 || layout.Size == 0 || layout.Size % layout.Alignment != 0)
            throw new IrUnsupportedException("unsupported aligned optional payload storage: " + payload.Describe());
        var name = "__DotCcAlignedOptional_" + AggregateIdentities.Hash(ObjectAggregateMetadata.Describe(payload));
        _alignedOptionals[name] = (rendered, layout.Size, layout.Alignment);
        return name;
    }

    internal IEnumerable<KeyValuePair<string, string>> AlignedOptionalTypesText(bool publicTypes)
    {
        foreach (var (name, (payload, size, alignment)) in _alignedOptionals)
        {
            var extent = checked((size + 1 + alignment - 1) & -alignment);
            // Zig's non-niche optional stores the real payload first, then its
            // presence byte. Explicit offsets and extent avoid Nullable<T>'s
            // different layout. The existing aligned-storage path supplies the
            // source alignment for locals, arrays, globals and enclosing values.
            var text = $"[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Explicit, Size = {extent}, Pack = {alignment})]\n"
                + (publicTypes ? "public " : "internal ") + $"unsafe struct {name}\n{{\n"
                + $"    [global::System.Runtime.InteropServices.FieldOffset(0)] private {payload} _value;\n"
                + $"    [global::System.Runtime.InteropServices.FieldOffset({size})] private byte _present;\n"
                + "    public bool HasValue => _present != 0;\n"
                + $"    public {payload} Value => HasValue ? _value : throw new global::System.InvalidOperationException(\"optional value is null\");\n"
                + $"    public static implicit operator {name}({payload} value) => new() {{ _value = value, _present = 1 }};\n"
                + $"    public static implicit operator {name}(void* pointer) => pointer == null ? default : throw new global::System.InvalidOperationException(\"only null converts to an optional\");\n"
                + $"    public static bool operator ==({name} value, void* pointer) => pointer == null ? !value.HasValue : throw new global::System.InvalidOperationException(\"optional pointer comparison requires null\");\n"
                + $"    public static bool operator !=({name} value, void* pointer) => !(value == pointer);\n"
                + $"    public static bool operator ==(void* pointer, {name} value) => value == pointer;\n"
                + $"    public static bool operator !=(void* pointer, {name} value) => !(value == pointer);\n"
                + $"    public static {payload}* PayloadAddress({name}* value) => value->HasValue ? ({payload}*)value : throw new global::System.InvalidOperationException(\"optional value is null\");\n"
                + "    public override bool Equals(object other) => base.Equals(other);\n"
                + "    public override int GetHashCode() => base.GetHashCode();\n}\n";
            yield return new(name, text);
        }
    }
}
