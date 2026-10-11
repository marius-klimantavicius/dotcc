using System.Globalization;

namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private sealed partial class FunctionWriter
    {
        private string WideIntegerConstant(Node node,string type,string text)
        {
            bool signed=type=="global::System.Int128";
            UInt128 bits;
            if(signed)
            {
                if(!Int128.TryParse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out Int128 value))
                    throw unit.Missing(node,"exporter defect: signed 128-bit constant encoding is invalid");
                bits=unchecked((UInt128)value);
            }
            else if(!UInt128.TryParse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out bits))
                throw unit.Missing(node,"exporter defect: unsigned 128-bit constant encoding is invalid");
            if(long.TryParse(text,NumberStyles.AllowLeadingSign,CultureInfo.InvariantCulture,out long small) && (signed || small>=0))
                return "(("+type+")("+small.ToString(CultureInfo.InvariantCulture)+"L))";
            string literal="new global::System.UInt128(0x"+((ulong)(bits>>64)).ToString("x",CultureInfo.InvariantCulture)+
                "UL, 0x"+unchecked((ulong)bits).ToString("x",CultureInfo.InvariantCulture)+"UL)";
            if(signed)literal="unchecked((global::System.Int128)"+literal+")";
            return literal+" /* "+text+" */";
        }

        private string? WidePointerConversion(Node result,Node operand)
        {
            var source=unit.Original(operand.ExecutionType??operand.Type??throw unit.Missing(operand,"conversion source type missing"));
            var target=unit.Original(result.ExecutionType??result.Type??throw unit.Missing(result,"conversion result type missing"));
            // System's wide integers are structs, so C# needs an explicit native
            // integer bridge for the pointer conversion already selected by EDG.
            bool fromPointer=source.Kind is "pointer" or "nullptr" && target.Kind is "i128" or "u128";
            bool toPointer=source.Kind is "i128" or "u128" && target.Kind is "pointer" or "nullptr";
            return fromPointer || toPointer?"unchecked(("+NodeType(result)+")(nuint)"+Expression(operand)+")":null;
        }
    }
}
