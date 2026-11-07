namespace Managed.Emulation.Execution;

internal static class FullFloatExtendedLowering
{
    internal static bool TryClassify(ulong ip,ReadOnlySpan<byte> bytes,out JitInstruction instruction,out string? reason)
    {
        instruction=default;reason="NativeFloatExtendedEncodingMissing";
        if(bytes.Length is <3 or >15)return false;
        int p=0,segment=-1,rep=0;bool word=false,address32=false;byte rex=0;
        while(p<bytes.Length)
        {
            byte b=bytes[p];if(b==0xf0)return false;
            if(b is >=0x40 and <=0x4f){rex=b;++p;continue;}
            if(b==0x66)word=true;
            else if(b==0x67)address32=true;
            else if(b is 0xf2 or 0xf3)rep=b;
            else if(b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment=b switch{0x26=>0,0x2e=>1,0x36=>2,0x3e=>3,0x64=>4,_=>5};
            else break;
            rex=0;++p;
        }
        if(word&&rep!=0){reason="NativeFloatExtendedMixedMandatoryPrefixMissing";return false;}
        if(p+3>bytes.Length||bytes[p++]!=0x0f)return false;
        byte op=bytes[p++];bool reciprocal=op is 0x52 or 0x53;
        if(reciprocal?(word||rep is not(0 or 0xf3)):
            (op is not(0x7c or 0x7d or 0xd0)||!word&&rep!=0xf2))return false;
        byte modrm=bytes[p++];JitAddress address=default;JitOperand source;
        if((modrm&0xc0)==0xc0)source=new((modrm&7)|((rex&1)<<3),0);
        else
        {
            if(!FullMemoryLowering.Address(bytes,ref p,modrm,rex,address32,segment,out address))return false;
            source=new(0,0,IsMemory:true);
        }
        if(p!=bytes.Length)return false;
        var operation=op switch{0x7c=>FullBlockEmitter.FullFloatExtendedOperation.HorizontalAdd,
            0x7d=>FullBlockEmitter.FullFloatExtendedOperation.HorizontalSubtract,
            0xd0=>FullBlockEmitter.FullFloatExtendedOperation.AlternatingSubtractAdd,
            0x53=>FullBlockEmitter.FullFloatExtendedOperation.Reciprocal,
            _=>FullBlockEmitter.FullFloatExtendedOperation.ReciprocalSquareRoot};
        bool scalar=reciprocal&&rep==0xf3;
        instruction=new(ip,bytes.Length,JitOperation.VectorFloat,128,new((modrm>>3&7)|((rex&4)<<1),0),source,
            SourceWidth:scalar?32:128,Address:address,Options:(uint)operation|
                (word?FullBlockEmitter.FloatDouble:0)|(scalar?FullBlockEmitter.FloatScalar:0));
        reason=null;return true;
    }
}
