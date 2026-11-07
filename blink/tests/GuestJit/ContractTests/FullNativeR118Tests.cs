using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink=Managed.Emulation.BlinkCore;

unsafe partial class Program
{
    static void CheckFullNativeR118(byte* machine)
    {
        CheckR118FaultLeaves(machine);CheckR118BitOffsets(machine);
        using(var common=CreateFullScaffold(out var shared))
        {fullOwnerScaffold=shared;try{CheckR118FaultLeaves(machine);CheckR118BitOffsets(machine);}finally{fullOwnerScaffold=default;}}
        CheckR118BitController();ResetFull(machine);
    }
    static void CheckR118FaultLeaves(byte* machine)
    {
        var encodings=new List<byte[]>();
        foreach(var form in VectorExtendedForms())encodings.Add(VectorExtendedEncoding(form,memory:true));
        encodings.AddRange(new byte[][]{[0x48,0x0f,0xab,0x0b],[0x48,0x0f,0xb3,0x0b],[0x48,0x0f,0xbb,0x0b],
            [0x48,0x0f,0xa3,0x0b],[0x48,0xc1,0x23,1],[0x48,0xf7,0x33]});
        foreach(byte[] raw in encodings)foreach(bool ss in new[]{false,true})
        {
            byte[] bytes=ss?[0x36,..raw]:raw;
            bool found=FullVectorExtendedLowering.TryClassify(Ip,bytes,out var i,out _)||
                FullMemoryIntegerLowering.TryClassify(Ip,bytes,out i,out _);
            Check(found,"R118 affected operation native classification");using var code=CreateFullBlock([new(i)]);
            ResetFull(machine);Write64(machine,48,0x0000800000000000);Write64(machine,32,0);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            f.MemoryTlb=(FullMemoryTlbEntry*)1;f.MemoryTlbMask=63;
            Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==(ss?12u:13u)&&f.FaultCode==128&&
                f.FaultAddress==0&&issued==1&&completed==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                "R118 aligned noncanonical packed/bit operand GP orSS before poisoned MMU and all effects");
        }
    }
    static void CheckR118BitOffsets(byte* machine)
    {
        byte* data=(byte*)NativeMemory.AlignedAlloc(4096,4096);
        try
        {
            foreach(byte opcode in new byte[]{0xa3,0xab,0xb3,0xbb})
            foreach(bool immediate in new[]{false,true})foreach(bool gs in new[]{false,true})
            {
                int group=opcode==0xa3?4:opcode==0xab?5:opcode==0xb3?6:7;
                byte seg=gs?(byte)0x65:(byte)0x64;
                byte[] bytes=immediate?[seg,0x67,0x0f,0xba,(byte)(group<<3),0]:[seg,0x67,0x0f,opcode,0x08];
                var i=MemoryIntegerClassify(bytes);using var code=CreateFullBlock([new(i)]);ResetFull(machine);
                Write64(machine,24,0x100);Write64(machine,32,0);new Span<byte>(data,4096).Clear();data[0x100]=4;
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
                if(gs)f.GsBase=0x100000000;else f.FsBase=0x100000000;
                FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&f.MemoryAddress==0x100000100&&issued==1&&completed==0,
                    "R118 EA32 truncates offset before preserving high FS/GS for register and immediate bit forms");
                SetFullMemoryEntry(tlb,0x100000000,data);FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    data[0x100]==(opcode is 0xab or 0xbb?5:4)&&(((Blink.Machine*)machine)->flags&1)==0,
                    "R118 high-segment bit operation uses correct word and CF");
            }
        }
        finally{NativeMemory.AlignedFree(data);}
    }
    static void CheckR118BitController()
    {
        using var f=new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc=ThreadedGuestExecution.FullControllerFixture.Base,low=0x200000,high=0x100200000;
        f.Map(pc);f.Fill(pc);f.Map(low);f.Fill(low);f.Map(high);f.Fill(high);
        WriteControllerCode(f,pc,[0x64,0x67,0x0f,0xab,0x08,0xeb,0xfe]);
        byte* a=(byte*)f.Physical(low),b=(byte*)f.Physical(high);
        BinaryPrimitives.WriteUInt32LittleEndian(new(a+0x100,4),1);BinaryPrimitives.WriteUInt32LittleEndian(new(b+0x100,4),4);
        var m=f.Machine();Write64((byte*)m,24,low+0x100);Write64((byte*)m,32,0);m->fs.@base=0x100000000;
        f.Counts(1);f.Step(pc);for(int n=0;n<12&&f.Completed<1;++n)f.Continue();
        Check(f.Completed==1&&BinaryPrimitives.ReadUInt32LittleEndian(new(a+0x100,4))==1&&
            BinaryPrimitives.ReadUInt32LittleEndian(new(b+0x100,4))==5&&f.Statistics.SemanticHelperInstructions==0,
            "R118 actual controller distinguishes low alias from high FS target with generated BTS");
    }
}
