using System.Globalization;

namespace DotObj.Relational;

internal sealed partial class Emitter
{
    private bool usesFloat80,usesFloat128;

    private string ExtendedFloatingType(Unit unit,ProgramType type)
    {
        if(type.Size!=16)
            throw new InvalidDataException("Extended floating storage emission is not supported: "+type.Name+" ("+type.Size+" bytes)");
        if(type.Kind=="f80")
        {
            usesFloat80=true;
            return "global::Edg.Runtime.Float80";
        }
        usesFloat128=true;
        return "global::Edg.Runtime.Float128";
    }

    private void WriteFloatingRuntime(string directory)
    {
        // Reuse the established BCL implementations without narrowing extended
        // values to double. Binary128's conversion adapter also uses Float80.
        if(usesFloat80 || usesFloat128)CopyRuntimeSource(directory,"Float80.cs");
        if(usesFloat128)
        {
            CopyRuntimeSource(directory,"Float128.cs");
            CopyRuntimeSource(directory,"Float128Conversions.cs");
        }
    }

    private sealed partial class FunctionWriter
    {
        private string FloatingConstant(Node node,long id)
        {
            if(!unit.FloatingValues.TryGetValue(id,out var value))
                throw unit.Missing(node,"exporter defect: selected floating literal missing");
            var type=unit.Original(value.Type);
            string bits=value.Bits;
            ulong Part(int start,int length)
            {
                if(start<0 || length<=0 || start+length>bits.Length ||
                    !ulong.TryParse(bits.AsSpan(start,length),NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture,out ulong parsed))
                    throw unit.Missing(node,"exporter defect: invalid floating literal encoding");
                return parsed;
            }
            void Width(int digits)
            {
                if(bits.Length!=digits)throw unit.Missing(node,"exporter defect: floating literal encoding width missing or incorrect");
            }
            string Decimal(string text,string suffix)=>text.StartsWith('-')?"("+text+suffix+")":text+suffix;
            string result;
            switch(type.Kind)
            {
                case "f32":
                    Width(8);
                    uint binary32=(uint)Part(0,8);
                    float single=BitConverter.UInt32BitsToSingle(binary32);
                    result=float.IsFinite(single) && binary32!=0x80000000U
                        ?Decimal(single.ToString("R",CultureInfo.InvariantCulture),"F")
                        :"global::System.BitConverter.UInt32BitsToSingle(0x"+bits+"U)";
                    break;
                case "f64":
                    Width(16);
                    ulong binary64=Part(0,16);
                    double real=BitConverter.UInt64BitsToDouble(binary64);
                    result=double.IsFinite(real) && binary64!=0x8000000000000000UL
                        ?Decimal(real.ToString("R",CultureInfo.InvariantCulture),"D")
                        :"global::System.BitConverter.UInt64BitsToDouble(0x"+bits+"UL)";
                    break;
                case "f80":
                    Width(20);Part(0,4);Part(4,16);
                    result="new "+Type(value.Type)+"(0x"+bits[4..]+"UL, 0x"+bits[..4]+")";
                    break;
                case "f128":
                    Width(32);Part(0,16);Part(16,16);
                    result=Type(value.Type)+".FromBits(((global::System.UInt128)0x"+bits[..16]+"UL << 64) | 0x"+bits[16..]+"UL)";
                    break;
                default:throw unit.Missing(node,"floating literal emission is not supported for "+type.Kind);
            }
            // The literal's own EDG type and the node execution type are both
            // retained; any represented conversion is emitted explicitly.
            return Type(value.Type)==NodeType(node)?result:"unchecked(("+NodeType(node)+")"+result+")";
        }
    }
}
