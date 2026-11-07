using F=Managed.Emulation.Execution.FullBlockEmitter.FullFloatOperation;

namespace Managed.Emulation.Execution;

// Six ordinary MMX/SSE conversion forms. The inherited producer stores MMX
// values in low64 of XMM slots; true x87 physical aliasing remains a model gap.
internal static class FullFloatMmxLowering
{
    internal static bool TryClassify(ulong ip,ReadOnlySpan<byte> bytes,out JitInstruction instruction,out string? reason)
    {
        instruction=default;reason="NativeFloatMmxEncodingMissing";
        if(bytes.Length is <3 or >15)return false;
        int p=0,segment=-1;bool pd=false,address32=false;byte rex=0;
        while(p<bytes.Length)
        {
            byte b=bytes[p];
            if(b is 0xf0 or 0xf2 or 0xf3)return false;
            if(b is >=0x40 and <=0x4f){rex=b;++p;continue;}
            if(b==0x66)pd=true;
            else if(b==0x67)address32=true;
            else if(b is 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65)
                segment=b switch{0x26=>0,0x2e=>1,0x36=>2,0x3e=>3,0x64=>4,_=>5};
            else break;
            rex=0;++p;
        }
        if(p+3>bytes.Length||bytes[p++]!=0x0f)return false;
        byte op=bytes[p++];if(op is not(0x2a or 0x2c or 0x2d))return false;
        byte modrm=bytes[p++];bool input=op==0x2a,memory=(modrm&0xc0)!=0xc0;
        int dst=(modrm>>3&7)|(input?(rex&4)<<1:0);
        JitOperand source;JitAddress address=default;
        if(memory)
        {
            if(!FullMemoryLowering.Address(bytes,ref p,modrm,rex,address32,segment,out address))return false;
            source=new(0,0,IsMemory:true);
        }
        else source=new((modrm&7)|(!input?(rex&1)<<3:0),0);
        if(p!=bytes.Length)return false;
        F operation=input?(pd?F.DwordToDouble:F.DwordToSingle):pd?
            (op==0x2c?F.DoubleToDwordTruncate:F.DoubleToDword):
            (op==0x2c?F.SingleToDwordTruncate:F.SingleToDword);
        instruction=new(ip,bytes.Length,JitOperation.VectorFloat,128,new(dst,0),source,
            SourceWidth:!input&&pd?128:64,Address:address,
            Options:(uint)operation|FullBlockEmitter.FloatMmxConversions);
        reason=null;return true;
    }
}
