using System.Security.Cryptography;
using System.Text;

namespace DotObj;

internal sealed partial class Emitter
{
    private string BitfieldResult(ObjectFile obj, RecordField field, string expression)
    {
        int width = checked((int)field.BitWidth!.Value);
        string bits = "unchecked((ulong)(" + expression + "))";
        if (field.BitSigned == true && width < 64)
            bits = "(unchecked((long)(" + bits + " << " + (64 - width) + ")) >> " + (64 - width) + ")";
        else if (width < 64) bits = "(" + bits + " & 0x" + ((1UL << width) - 1).ToString("x", global::System.Globalization.CultureInfo.InvariantCulture) + "UL)";
        return "unchecked((" + Type(obj.Type(field.TypeId)) + ")" + bits + ")";
    }

    private string BitfieldOperation(ObjectFile obj, RecordField field, string operation, string address, string? value, string? valueType = null)
    {
        string type = obj.Type(field.TypeId), owner = obj.Type(field.RecordTypeId); valueType ??= type;
        string key = "bitfield:" + owner + ":" + field.Ordinal + ":" + operation + ":" + valueType;
        if (!operationHelpers.TryGetValue(key, out var helper))
        {
            string name = generatedMembers.Allocate("__dot_bitfield_" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..12], "bitfield_operation");
            string member = "owner->" + fieldNames[(owner, field.Ordinal)];
            var code = new StringBuilder("    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n")
                .Append("    private static ").Append(Type(type)).Append(' ').Append(name).Append('(').Append(Type(owner)).Append("* owner")
                .Append(value is null ? "" : ", " + Type(valueType) + " value").Append(")\n    {\n");
            if (operation != "assign") code.Append("        ").Append(Type(type)).Append(" previous = ").Append(member).Append(";\n");
            string assigned = operation switch
            {
                "assign" => "value",
                "pre-inc" or "post-inc" => "unchecked((" + Type(type) + ")(previous + 1))",
                "pre-dec" or "post-dec" => "unchecked((" + Type(type) + ")(previous - 1))",
                _ => CompoundValue(type, valueType, operation[..^1], "previous", "value")
            };
            code.Append("        ").Append(member).Append(" = ").Append(assigned).Append(";\n        return ")
                .Append(operation is "post-inc" or "post-dec" ? "previous" : member).Append(";\n    }\n\n");
            helper = (name, code.ToString()); operationHelpers.Add(key, helper);
        }
        return Owner + "." + helper.Name + "(" + address + (value is null ? "" : ", " + value) + ")";
    }
    private static bool ModularIntegerOperation(string type, string argument, string op) =>
        ScalarTypes.IsInteger(type) && ScalarTypes.IsInteger(argument) && op is "+" or "-" or "*" or "&" or "|" or "^";
    private static bool ImplicitNumericConversion(string source, string target) => source == target || target switch
    {
        "i16" => source is "i8" or "u8",
        "u16" => source == "u8",
        "i32" => source is "i8" or "u8" or "i16" or "u16",
        "u32" => source is "u8" or "u16",
        "i64" => ScalarTypes.IsInteger(source) && source != "u64",
        "u64" => source is "u8" or "u16" or "u32",
        "f32" => ScalarTypes.IsInteger(source),
        "f64" => ScalarTypes.IsInteger(source) || source == "f32",
        _ => false
    };
    private static bool CompoundPromotionDiffers(string target, string argument, string op) =>
        !ScalarTypes.IsPointer(target) && op is not ("<<" or ">>") && !ModularIntegerOperation(target, argument, op) &&
        (!ImplicitNumericConversion(argument, target) ||
         op is "/" or "%" && target is "i8" or "i16" or "i32" && argument == "u32");
    private string CompoundOperation(string type, string valueType, string op, string address, string value)
    {
        string key = "compound:" + type + ":" + valueType + ":" + op;
        if (!operationHelpers.TryGetValue(key, out var helper))
        {
            string name = generatedMembers.Allocate("__dot_compound_" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..12], "compound_operation");
            string code = "    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n" +
                "    private static " + Type(type) + " " + name + "(" + Type(type) + "* address, " + Type(valueType) + " value)\n    {\n" +
                "        " + Type(type) + " result = " + CompoundValue(type, valueType, op, "*address", "value") + ";\n        *address = result;\n        return result;\n    }\n\n";
            helper = (name, code); operationHelpers.Add(key, helper);
        }
        return Owner + "." + helper.Name + "(" + address + ", " + value + ")";
    }
}
