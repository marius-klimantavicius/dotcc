namespace Managed.Emulation.Execution;

// Native software implementation needs no host POPCNT/BMI/ABM feature. The
// caller still supplies one exact instruction decoded in guest long mode.
internal static class FullBitCountLowering
{
    internal static bool TryClassify(ulong ip,ReadOnlySpan<byte> bytes,out JitInstruction instruction,out string? reason)
    {
        instruction=default;reason="NativeBitCountEncodingMissing";
        if(bytes.Length is < 4 or > 15)return false;
        int p=0,segment=-1,rep=0;bool word=false,address32=false;byte rex=0;
        while(p<bytes.Length)
        {
            byte b=bytes[p];
            if(b==0xf0){reason="NativeBitCountLockMissing";return false;}
            if(b is >=0x40 and <=0x4f){rex=b;++p;continue;}
            if(b==0x66)word=true;
            else if(b==0x67)address32=true;
            else if(b is 0xf2 or 0xf3)rep=b;
            else if(b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment=b switch{0x26=>0,0x2e=>1,0x36=>2,0x3e=>3,0x64=>4,_=>5};
            else break;
            rex=0;++p;
        }
        if(rep!=0xf3 || p+3>bytes.Length || bytes[p++]!=0x0f)return false;
        byte op=bytes[p++];if(op is not(0xb8 or 0xbc or 0xbd))return false;
        byte modrm=bytes[p++];int width=(rex&8)!=0?64:word?16:32;
        var dst=new JitOperand((modrm>>3&7)|((rex&4)<<1),0);
        JitOperand src;JitAddress address=default;
        if((modrm&0xc0)==0xc0)src=new((modrm&7)|((rex&1)<<3),0);
        else
        {
            if(!FullMemoryLowering.Address(bytes,ref p,modrm,rex,address32,segment,out address))return false;
            src=new(0,0,IsMemory:true);
        }
        if(p!=bytes.Length)return false;
        instruction=new(ip,bytes.Length,op==0xb8?JitOperation.PopulationCount:
            op==0xbc?JitOperation.TrailingZeroCount:JitOperation.LeadingZeroCount,width,dst,src,
            SourceWidth:width,Address:address);
        reason=null;return true;
    }
}
