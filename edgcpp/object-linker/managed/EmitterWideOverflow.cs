using System.Text;

namespace DotObj;

internal sealed partial class Emitter
{
    // Compiler overflow intrinsics compute mathematical integers before storing
    // the low destination bits. A signed magnitude with two UInt128 limbs covers
    // every product and sum of two signed/unsigned 128-bit operands.
    private string WideOverflowBody(string operation, string leftType, string rightType,
        string destination, string result)
    {
        const string wide = "global::System.UInt128";
        bool multiply = operation.StartsWith("mul-", StringComparison.Ordinal);
        bool subtract = operation.StartsWith("sub-", StringComparison.Ordinal);
        bool unsignedProduct = multiply && leftType.StartsWith('u') && rightType.StartsWith('u');
        var code = new StringBuilder();
        if (unsignedProduct)
        {
            code.Append("        ").Append(wide).Append(" high = ").Append(wide)
                .Append(".BigMul((").Append(wide).Append(")value0, (").Append(wide)
                .Append(")value1, out ").Append(wide).Append(" low);\n");
        }
        else
        {
            void Operand(int index, string type)
            {
                code.Append("        bool negative").Append(index).Append(" = ")
                    .Append(type.StartsWith('i') ? "value" + index + " < 0" : "false").Append(";\n");
                code.Append("        ").Append(wide).Append(" magnitude").Append(index)
                    .Append(" = unchecked((").Append(wide).Append(")value").Append(index).Append(");\n");
                if (type.StartsWith('i'))
                    code.Append("        if (negative").Append(index).Append(") magnitude").Append(index)
                        .Append(" = unchecked((").Append(wide).Append(")0 - magnitude").Append(index).Append(");\n");
            }
            Operand(0, leftType);
            Operand(1, rightType);
            code.Append("        ").Append(wide).Append(" high, low;\n        bool negative;\n");
            if (multiply)
            {
                code.Append("        high = ").Append(wide).Append(".BigMul(magnitude0, magnitude1, out low);\n")
                    .Append("        negative = negative0 != negative1;\n");
            }
            else
            {
                if (subtract) code.Append("        negative1 = !negative1;\n");
                code.Append("        if (negative0 == negative1)\n        {\n")
                    .Append("            low = unchecked(magnitude0 + magnitude1);\n")
                    .Append("            high = low < magnitude0 ? (").Append(wide).Append(")1 : 0;\n")
                    .Append("            negative = negative0;\n        }\n")
                    .Append("        else if (magnitude0 >= magnitude1)\n        {\n")
                    .Append("            low = magnitude0 - magnitude1; high = 0; negative = negative0;\n        }\n")
                    .Append("        else\n        {\n")
                    .Append("            low = magnitude1 - magnitude0; high = 0; negative = negative1;\n        }\n");
            }
            code.Append("        negative = negative && (high != 0 || low != 0);\n");
        }
        string bits = unsignedProduct ? "low" : "(negative ? unchecked((" + wide + ")0 - low) : low)";
        string narrowed = "unchecked((" + Type(destination) + ")" + bits + ")";
        code.Append("        ").Append(operation.EndsWith("-volatile", StringComparison.Ordinal)
            ? VolatileWrite(destination, "value2", narrowed) : "*value2 = " + narrowed).Append(";\n");
        string max = "(" + wide + ")" + Type(destination) + ".MaxValue";
        string range;
        if (unsignedProduct) range = "low > " + max;
        else if (destination.StartsWith('u')) range = "negative || low > " + max;
        else
        {
            int width = int.Parse(destination[1..], System.Globalization.CultureInfo.InvariantCulture);
            range = "(negative ? low > ((" + wide + ")1 << " + (width - 1) + ") : low > " + max + ")";
        }
        string overflow = "high != 0 || " + range;
        code.Append("        return ").Append(result == "bool" ? overflow
            : "(" + Type(result) + ")(" + overflow + " ? 1 : 0)").Append(";\n");
        return code.ToString();
    }
}
