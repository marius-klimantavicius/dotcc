using S=Managed.Emulation.Execution.FullBlockEmitter.FullSystemStateOperation;

namespace Managed.Emulation.Execution;

internal static class FullSystemStateLowering
{
    internal static bool TryClassify(ulong ip,ReadOnlySpan<byte> bytes,out JitInstruction instruction,out string? reason)
    {
        instruction=default;reason="NativeSystemStateEncodingMissing";
        if(bytes.Length is <3 or >15)return false;
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
        if(p+3>bytes.Length||bytes[p++]!=0x0f)return false;
        byte op=bytes[p++],modrm=bytes[p++];bool memory=(modrm&0xc0)!=0xc0;int group=modrm>>3&7;
        JitAddress address=default;
        if(memory&&!FullMemoryLowering.Address(bytes,ref p,modrm,rex,address32,segment,out address))return false;
        if(p!=bytes.Length)return false;
        // Architectural prefetch hints do not evaluate or access their data
        // address. The authoritative decoder still owns complete byte fetch.
        if((op==0x18&&group<=3||op==0x0d&&group<=1)&&memory&&rep==0)
        {
            instruction=new(ip,bytes.Length,JitOperation.Nop,64,default,default);reason=null;return true;
        }
        if(op!=0xae)return false;
        S state;int width=64,sourceWidth=0;JitOperand src=default,dst=default;
        if(!memory&&rep==0xf3&&group<=3)
        {
            width=(rex&8)!=0?64:32;
            var register=new JitOperand((modrm&7)|((rex&1)<<3),0);
            state=group switch{0=>S.ReadFsBase,1=>S.ReadGsBase,2=>S.WriteFsBase,_=>S.WriteGsBase};
            if(group<2)dst=register;else src=register;
        }
        else if(memory&&rep==0&&group is 2 or 3)
        {
            state=group==2?S.LoadMxcsr:S.StoreMxcsr;width=sourceWidth=32;
            if(group==2)src=new(0,0,IsMemory:true);else dst=new(0,0,IsMemory:true);
        }
        else if(!memory&&rep==0&&!word&&(modrm&7)==0&&group>=5)
            state=group==5?S.LoadFence:group==6?S.FullFence:S.StoreFence;
        else if(memory&&rep==0&&!word&&group==7)
        {state=S.CacheFlush;width=sourceWidth=8;src=new(0,0,IsMemory:true);}
        else return false;
        instruction=new(ip,bytes.Length,JitOperation.SystemState,width,dst,src,SourceWidth:sourceWidth,
            Address:memory?address:default,Options:(uint)state);reason=null;return true;
    }
}
