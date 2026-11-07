using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static void CheckFullMaskedStoreUnion(byte* machine)
    {
        CheckMaskedStoreDenseLeaves(machine);
        using(var owner=CreateFullScaffold(out var scaffold))
        {fullOwnerScaffold=scaffold;try{CheckMaskedStoreDenseLeaves(machine);}finally{fullOwnerScaffold=default;}}
    }
    static void CheckMaskedStoreDenseLeaves(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        try
        {
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
            foreach(bool zero in new[]{false,true})foreach(int shape in new[]{0,1,2})
            {
                if(zero&&shape==2)continue; // MOVS genuinely uses the poisoned DI.
                var masked=MaskedClassify([0x66,0x0f,0xf7,0xc1]);
                ulong next=Ip+(uint)masked.Length;
                JitInstruction dense=shape==0?
                    new(next,2,JitOperation.LoadMov,32,new(0,0),new(0,0,IsMemory:true),SourceWidth:32,Address:new(3,-1,0,0)):
                    shape==1?new(next,2,JitOperation.Push,64,default,new(0,0,IsMemory:true),SourceWidth:64,Address:new(3,-1,0,0)):
                    new(next,2,JitOperation.StringMove,64,default,default,SourceWidth:64,Address:new(6,-1,0,0));
                using var code=CreateFullBlock([new(masked),new(dense)]);
                MaskedSeed(machine,masked,zero?0x0000800000000000ul:0x100010,zero?0u:1u);
                ((Blink.Machine*)machine)->flags&=~1024u;
                Write64(machine,48,0x100020);Write64(machine,56,0x100180);Write64(machine,72,0x100020);
                new Span<byte>(data,4096).Clear();*(ulong*)(data+32)=0x1234567887654321;
                ulong epoch=11;long issued=0,completed=0;int stop=0;var header=FullHeader(Ip,code.Entry);
                var frame=FullFrame(machine,&header,&epoch,&issued,&completed,&stop,limit:2);
                var d=MaskedMmuDescriptor(machine,masked,zero?0x0000800000000000ul:0x100010,zero?0u:1u);
                if(!zero)MaskedPublishSynthetic(ref d,data,null,0x100000,epoch);
                frame.StackMemory=(FullStackMemoryDescriptor*)&d;frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,0x100000,data);
                Check(FloatRunProbe(code,&frame)==FullBlockExit.Continue&&issued==2&&completed==2&&frame.Executed==2,
                    "sparse Ready/zero cancellation transitions to dense scalar/stack/string within one native entry");
                Check(frame.StackMemory->Version==1&&frame.StackMemory->Count==(shape==0?0u:2u),
                    "sparse tag/kind cannot be mistaken for dense Count or Ready state");
                if(shape==0)Check(*(ulong*)(machine+24)==0x87654321,"following ordinary dense load uses its own mapped bytes");
                else if(shape==1)Check(*(ulong*)(data+0x178)==0x1234567887654321&&*(ulong*)(machine+56)==0x100178,
                    "following two-access stack prepares a fresh owned dense request");
                else Check(*(ulong*)(data+16)==0x1234567887654321&&*(ulong*)(machine+72)==0x100028&&*(ulong*)(machine+80)==0x100018,
                    "following string pair prechecks and commits its own source/destination");
            }
        }
        finally{NativeMemory.AlignedFree(data);}
    }
}
