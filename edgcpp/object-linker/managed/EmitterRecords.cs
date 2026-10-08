using System.Globalization;
using System.Text;

namespace DotObj;

internal sealed partial class Emitter
{
    private static bool IsVolatileField(ObjectFile obj, RecordField field)
    {
        if (field.BitWidth is not null || field.SourceTypeId is null) return false;
        string type = obj.Type(field.TypeId);
        if (type is not ("i8" or "u8" or "i16" or "u16" or "i32" or "u32" or "f32" or "bool") &&
            (!ScalarTypes.IsPointer(type) || RoutineTypes.IsFunctionPointer(type))) return false;
        SourceType source = obj.SourceTypes[field.SourceTypeId.Value];
        while (true)
        {
            if (source.IsVolatile) return true;
            if (source.Kind is not ("typedef" or "qualified")) return false;
            source = obj.SourceTypes[source.UnderlyingTypeId!.Value];
        }
    }

    private void WriteBitfield(StringBuilder output, ObjectFile obj, RecordField field, string property, Names names)
    {
        int width = checked((int)field.BitWidth!.Value), offset = checked((int)field.BitOffset!.Value);
        int bytes = (width + offset + 7) / 8;
        string[] backing = Enumerable.Range(0, bytes).Select(index => names.Allocate("__bits_" + property.TrimStart('@') + "_" + index, "bits")).ToArray();
        for (int index = 0; index < bytes; index++)
            output.Append("        [global::System.Runtime.InteropServices.FieldOffset(").Append(field.ByteOffset + index).Append(")] private byte ").Append(backing[index]).Append(";\n");
        string type = Type(obj.Type(field.TypeId));
        var terms = new List<string>();
        for (int index = 0; index < bytes; index++)
        {
            int shift = index * 8 - offset;
            terms.Add(shift < 0 ? "((ulong)" + backing[index] + " >> " + -shift + ")" : shift == 0 ? "(ulong)" + backing[index]
                : "((ulong)" + backing[index] + " << " + shift + ")");
        }
        string bits = "(" + string.Join(" | ", terms) + ")";
        if (width < 64) bits = "(" + bits + " & 0x" + ((1UL << width) - 1).ToString("x", CultureInfo.InvariantCulture) + "UL)";
        if (field.BitSigned == true && width < 64) bits = "((long)(" + bits + " << " + (64 - width) + ") >> " + (64 - width) + ")";
        output.Append("        public ").Append(type).Append(' ').Append(property).Append("\n        {\n            get => unchecked((").Append(type).Append(')').Append(bits).Append(");\n            set\n            {\n");
        for (int index = 0; index < bytes; index++)
        {
            int first = Math.Max(offset, index * 8), end = Math.Min(offset + width, index * 8 + 8);
            int count = end - first, localShift = first - index * 8, valueShift = first - offset;
            int mask = ((1 << count) - 1) << localShift;
            string incoming = "((ulong)value" + (valueShift == 0 ? "" : " >> " + valueShift) + ")";
            if (localShift != 0) incoming = "(" + incoming + " << " + localShift + ")";
            output.Append("                ").Append(backing[index]).Append(" = unchecked((byte)(((ulong)").Append(backing[index]).Append(" & ")
                .Append(255 ^ mask).Append(") | (").Append(incoming).Append(" & ").Append(mask).Append("UL)));\n");
        }
        output.Append("            }\n        }\n");
    }
}
