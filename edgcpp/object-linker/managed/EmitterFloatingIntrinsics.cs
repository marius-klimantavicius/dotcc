namespace DotObj;

internal sealed partial class Emitter
{
    private string FloatingClassification(string operation, string type, string value)
    {
        bool sign = operation.StartsWith("__builtin_signbit", StringComparison.Ordinal);
        bool nan = operation.StartsWith("__builtin_isnan", StringComparison.Ordinal);
        bool finite = operation == "__builtin_isfinite";
        if (type is "f32" or "f64")
        {
            if (sign)
                return "unchecked((int)(global::System.BitConverter." +
                    (type == "f32" ? "SingleToUInt32Bits(" : "DoubleToUInt64Bits(") + value + ") >> " +
                    (type == "f32" ? "31" : "63") + "))";
            string method = nan ? "IsNaN" : finite ? "IsFinite" : "IsInfinity";
            return "(global::System." + (type == "f32" ? "Single" : "Double") + "." + method + "(" + value + ") ? 1 : 0)";
        }
        if (type != "f80") throw new InvalidDataException("Unsupported floating classification width");
        if (sign) return "((" + value + ").SignExponent >> 15)";
        if (finite) return "(((" + value + ").SignExponent & 0x7fff) != 0x7fff ? 1 : 0)";
        // Binary80 classification must not round through double. A helper
        // captures the operand once while inspecting both original bit fields.
        string key = "floating-classification:" + (nan ? "nan" : "infinity") + ":f80";
        if (!operationHelpers.TryGetValue(key, out var helper))
        {
            string name = generatedMembers.Allocate(nan ? "__dot_isnan_binary80" : "__dot_isinf_binary80", "intrinsic");
            string code = "    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n" +
                "    private static int " + name + "(global::Edg.Runtime.Float80 value) =>\n" +
                "        (value.SignExponent & 0x7fff) == 0x7fff && value.Significand " +
                (nan ? "!=" : "==") + " 0x8000000000000000UL ? 1 : 0;\n\n";
            helper = (name, code); operationHelpers.Add(key, helper);
        }
        return Owner + "." + helper.Name + "(" + value + ")";
    }
}
