using System.Security.Cryptography;
using System.Text;

namespace DotObj;

internal sealed partial class Emitter
{
    private string Intrinsic(ObjectFile obj, Node node, Node[] arguments, Func<Node, string> expression)
    {
        string operation = node.Operator!, result = obj.Type(node.TypeId!.Value);
        string[] types = arguments.Select(argument => obj.Type(argument.TypeId!.Value)).ToArray();
        if (operation == "unreachable") return "throw new global::System.InvalidOperationException(\"Reached source unreachable intrinsic\")";
        if (operation == "bswap") return "global::System.Buffers.Binary.BinaryPrimitives.ReverseEndianness(" + expression(arguments[0]) + ")";
        if (operation is "clz" or "ctz" or "popcount" or "parity")
        {
            string method = operation == "clz" ? "LeadingZeroCount" : operation == "ctz" ? "TrailingZeroCount" : "PopCount";
            string value = "global::System.Numerics.BitOperations." + method + "(" + expression(arguments[0]) + ")";
            if (operation == "parity") value = "(" + value + " & 1)";
            return result == "i32" ? value : "unchecked((" + Type(result) + ")" + value + ")";
        }
        string key = "intrinsic:" + operation + ":" + result + ":" + string.Join(',', types);
        if (!operationHelpers.TryGetValue(key, out var helper))
        {
            string name = generatedMembers.Allocate("__dot_" + operation.Replace('-', '_') + "_" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(key)))[..10], "intrinsic");
            var code = new StringBuilder("    [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\n")
                .Append("    private static ").Append(Type(result)).Append(' ').Append(name).Append('(')
                .AppendJoin(", ", types.Select((type, index) => Type(type) + " value" + index)).Append(")\n    {\n");
            if (operation == "expect") code.Append("        return value0;\n");
            else if (operation is "ffs" or "clrsb")
            {
                string unsigned = obj.StorageSize(types[0]) == 8 ? "ulong" : "uint";
                string value = operation == "ffs"
                    ? "value0 == 0 ? 0 : global::System.Numerics.BitOperations.TrailingZeroCount(unchecked((" + unsigned + ")value0)) + 1"
                    : "global::System.Numerics.BitOperations.LeadingZeroCount(unchecked((" + unsigned + ")(value0 < 0 ? ~value0 : value0))) - 1";
                code.Append("        return unchecked((").Append(Type(result)).Append(")(").Append(value).Append("));\n");
            }
            else if (types.Take(2).Append(ScalarTypes.Pointee(types[2])).Any(ScalarTypes.IsWideInteger))
                code.Append(WideOverflowBody(operation, types[0], types[1], ScalarTypes.Pointee(types[2]), result));
            else
            {
                bool unsignedMath = operation.StartsWith("mul-", StringComparison.Ordinal) && types.Take(2).All(type => type.StartsWith('u'));
                string wide = unsignedMath ? "global::System.UInt128" : "global::System.Int128";
                string symbol = operation.StartsWith("add-", StringComparison.Ordinal) ? "+" : operation.StartsWith("sub-", StringComparison.Ordinal) ? "-" : "*";
                string destination = ScalarTypes.Pointee(types[2]), destinationType = Type(destination);
                code.Append("        ").Append(wide).Append(" mathematical = (").Append(wide).Append(")value0 ").Append(symbol).Append(" (").Append(wide).Append(")value1;\n");
                string narrowed = "unchecked((" + destinationType + ")mathematical)";
                code.Append("        ").Append(operation.EndsWith("-volatile", StringComparison.Ordinal)
                    ? VolatileWrite(destination, "value2", narrowed) : "*value2 = " + narrowed).Append(";\n");
                string overflow = (unsignedMath ? "" : "mathematical < (" + wide + ")" + destinationType + ".MinValue || ") + "mathematical > (" + wide + ")" + destinationType + ".MaxValue";
                code.Append("        return ").Append(result == "bool" ? overflow : "(" + Type(result) + ")(" + overflow + " ? 1 : 0)").Append(";\n");
            }
            code.Append("    }\n\n"); helper = (name, code.ToString()); operationHelpers.Add(key, helper);
        }
        return Owner + "." + helper.Name + "(" + string.Join(", ", arguments.Select(expression)) + ")";
    }
}
