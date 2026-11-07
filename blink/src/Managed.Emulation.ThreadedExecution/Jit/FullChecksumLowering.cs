namespace Managed.Emulation.Execution;

internal static class FullChecksumLowering
{
    internal static bool TryClassify(ulong ip, ReadOnlySpan<byte> bytes,
        out JitInstruction instruction, out string? reason)
    {
        instruction=default;reason="NativeCrc32EncodingMissing";
        if(bytes.Length is <5 or >15)return false;
        int p=0,rep=0,segment=-1;bool word=false,address32=false;byte rex=0;
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
        if(rep!=0xf2||p+4>bytes.Length||bytes[p++]!=0x0f||bytes[p++]!=0x38)return false;
        byte op=bytes[p++];if(op is not(0xf0 or 0xf1))return false;
        byte modrm=bytes[p++];int width=(rex&8)!=0?64:32;
        int sourceWidth=op==0xf0?8:(rex&8)!=0?64:word?16:32;
        var destination=new JitOperand((modrm>>3&7)|((rex&4)<<1),0);
        JitOperand source;JitAddress address=default;
        if(modrm>=0xc0)source=FullMemoryLowering.Register((modrm&7)|((rex&1)<<3),sourceWidth,rex!=0);
        else
        {
            if(!FullMemoryLowering.Address(bytes,ref p,modrm,rex,address32,segment,out address))return false;
            source=new(0,0,IsMemory:true);
        }
        if(p!=bytes.Length)return false;
        instruction=new(ip,bytes.Length,JitOperation.ChecksumCrc32,width,destination,source,
            SourceWidth:sourceWidth,Address:address);reason=null;return true;
    }
}
