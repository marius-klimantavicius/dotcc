using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using ProcessorFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

// R166: AUTHOREDUNRUN. Parent integrates this single family entry and serially
// builds/runs. No nested C runtime owners: every oracle fixture ends before any
// full-controller fixture begins. Scalar models do not call product lowering.
unsafe partial class Program
{
    const uint ProcessorFlags = 0xa5a50ad5u & ~0x50100u;
    static readonly uint[] ProcessorLeaves = [0,1,2,3,4,5,6,7,8,0x40000000,0x40031337,0x40031338,
        0x80000000,0x80000001,0x80000007,0xffffffff,0x40031339,0x80000002];
    static readonly uint[] ProcessorSubleaves = [0,1,2,3,4,5,0x80000000,0xffffffff];

    static JitInstruction ProcessorClassify(byte[] bytes, ulong pc = Ip)
    {
        Check(FullProcessorQueryLowering.TryClassify(pc,bytes,out var i,out var why),why ?? "processor grammar");
        Check(FullBlockEmitter.CanInline(i) && FullBlockEmitter.ValidateProcessorQuery(i),"processor native admission");
        Check(i.Length == bytes.Length && i.Ip == pc,"processor original span identity"); return i;
    }

    sealed class ProcessorBacking : IDisposable
    {
        internal readonly Blink.System* System;
        internal readonly byte* Real;
        internal readonly byte* Pages;
        internal ProcessorBacking()
        {
            System=(Blink.System*)NativeMemory.AllocZeroed((nuint)sizeof(Blink.System));
            Real=(byte*)NativeMemory.AlignedAlloc(FullProcessorQueryAdmission.RealSize,4096);
            Pages=(byte*)NativeMemory.AlignedAlloc(8192,4096);
            if(System==null||Real==null||Pages==null){Dispose();throw new OutOfMemoryException();}
            new Span<byte>(Real,(int)FullProcessorQueryAdmission.RealSize).Clear();
            new Span<byte>(Pages,8192).Fill(0xa5);
            System->real=Real;
        }
        public void Dispose(){NativeMemory.Free(System);NativeMemory.AlignedFree(Real);NativeMemory.AlignedFree(Pages);}
    }

    static void ProcessorSeed(byte* machine, ProcessorBacking backing)
    {
        ResetFull(machine);var m=(Blink.Machine*)machine;m->blink_host_system=backing.System;m->flags=ProcessorFlags;
        m->cs.sel=3; // ordinary managed Linux CPL3; never host CS
        for(int r=0;r<16;++r)Write64(machine,24+r*8,0xdeadbeef76540000UL+(uint)(r*0x101));
    }

    static FullBlockFrame ProcessorRun(NativeCodeAllocation code,byte* machine, JitInstruction i,
        out FullBlockExit exit,out long issued,out long completed,ProcessorBacking? backing=null,
        FullMemoryTlbEntry* tlb=null,ulong budget=100, bool bind=false)
    {
        ulong epoch=11;long issue=0,done=0;int stop=0;var header=FullHeader(i.Ip,code.Entry);
        var f=FullFrame(machine,&header,&epoch,&issue,&done,&stop,budget,1);
        FullMemoryTlbEntry* empty=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(empty,64).Clear();
        f.MemoryTlb=tlb==null?empty:tlb;f.MemoryTlbMask=63;
        f.FsBase=((Blink.Machine*)machine)->fs.@base;f.GsBase=((Blink.Machine*)machine)->gs.@base;
        if(bind)Check(backing!=null&&FullProcessorQueryAdmission.BindRetainedDescriptorMapping(&f,backing.System,
            backing.Real,FullProcessorQueryAdmission.RealSize),"test allocation retained through native RET");
        try{exit=RunFull(code,&f);}
        finally{FullProcessorQueryAdmission.UnbindDescriptorMapping(&f);}
        issued=issue;completed=done;return f; // pointer fields are never dereferenced by the caller
    }

    static void CheckFullNativeProcessorQueries(byte* machine)
    {
        CheckProcessorGrammar();
        using(var backing=new ProcessorBacking())
        {
            CheckProcessorLeaves(machine,backing);CheckProcessorDescriptors(machine,backing);
            CheckProcessorMemory(machine,backing);CheckProcessorGates(machine,backing);CheckProcessorMappingAdmission(machine,backing);
            using(var common=CreateFullScaffold(out var shared))
            {
                fullOwnerScaffold=shared;
                try{CheckProcessorLeaves(machine,backing);CheckProcessorDescriptors(machine,backing);
                    CheckProcessorMemory(machine,backing);CheckProcessorGates(machine,backing);}
                finally{fullOwnerScaffold=default;}
            }
            CheckProcessorEmission();ResetFull(machine);
        }
        CheckProcessorSelectedOracle(); // fixture disposed on method return
        CheckProcessorControllers();
        CheckProcessorControllerFaults();
        CheckProcessorMmuFaults();ResetFull(machine);
    }

    static void CheckProcessorGrammar()
    {
        foreach(byte[] prefix in new byte[][]{[],[0x66],[0x67],[0x48],[0x4f],[0x48,0x66],
            [0x66,0x48],[0xf2],[0xf3],[0x65,0x67,0x66,0x41],Enumerable.Repeat((byte)0x66,13).ToArray()})
            Check(ProcessorClassify([..prefix,0x0f,0xa2]).Width==32,"CPUID implicit widths/prefixes ignored");
        foreach(int rex in Enumerable.Range(0x40,16))foreach(int rm in Enumerable.Range(0,8))
        foreach(byte[] prefix in new byte[][]{[0xf3],[0x66,0xf3],[0xf2,0xf3],[0x65,0x67,0xf3]})
        {
            var i=ProcessorClassify([..prefix,(byte)rex,0x0f,0xc7,(byte)(0xf8|rm)]);
            Check(i.Width==64&&i.Destination.Register==(rm|((rex&1)<<3)),"RDPID all REX fields/no16bit form");
        }
        var cancelled=ProcessorClassify([0xf3,0x49,0x66,0x0f,0xc7,0xfa]);
        Check(cancelled.Width==64&&cancelled.Destination.Register==2,"RDPID legacy cancels REX.B but ignores66");
        foreach(int width in new[]{16,32,64})foreach(int dst in new[]{0,3,8,15})foreach(int src in new[]{0,1,9,15})
        {
            byte rex=(byte)(0x40|(width==64?8:0)|((dst>>3)<<2)|(src>>3));
            byte[] word=width==16?[0x66]:[];
            var i=ProcessorClassify([..word,rex,0x0f,0x03,(byte)(0xc0|(dst&7)<<3|(src&7))]);
            Check(i.Width==width&&i.SourceWidth==16&&i.Destination.Register==dst&&i.Source.Register==src,
                "LSL independent source16 and GPR16/32/64 with alias/highREX");
        }
        foreach(byte[] bytes in new byte[][]{[0x0f,0x03,0x03],[0x66,0x0f,0x03,0x45,0],
            [0x67,0x64,0x4f,0x0f,0x03,0x44,0xfe,0x80],[0x0f,0x03,0x05,0,0,0,0],
            [0x36,0x0f,0x03,0x04,0x24],[0x41,0x0f,0x03,0x45,0]})
            Check(ProcessorClassify(bytes).Source.IsMemory,"LSL source m16 address grammar");
        foreach(byte[] bytes in new byte[][]{[],[0x0f],[0x0f,0xa2,0],[0xf0,0x0f,0xa2],
            [0xf0,0x0f,0x03,0xc0],[0xf0,0xf3,0x0f,0xc7,0xf8],[0xf3,0x0f,0xc7,0x38],
            [0x0f,0xc7,0xf8],[0xf2,0x0f,0xc7,0xf8],[0xf3,0xf2,0x0f,0xc7,0xf8],
            [0xf3,0x0f,0xc7,0xf0],[0x0f,0x03,0x05,0],Enumerable.Repeat((byte)0x66,16).ToArray()})
            Check(!FullProcessorQueryLowering.TryClassify(Ip,bytes,out _,out var why)&&why!=null,
                "reserved/truncated/overlong/nonfamily forms fail closed");
        var cpuid=ProcessorClassify([0x0f,0xa2]);var lsl=ProcessorClassify([0x0f,0x03,0xc1]);
        foreach(var bad in new[]{cpuid with{Width=64},cpuid with{SourceWidth=32},cpuid with{Target=1},
            cpuid with{Extra=new(1,0)},lsl with{SourceWidth=32},lsl with{Destination=new(16,0)},
            lsl with{Source=new(0,0,IsImmediate:true)},lsl with{Address=new(0,-1,0,0)}})
        {
            bool rejected=false;try{FullBlockEmitter.ValidateProcessorQuery(bad);}catch(ArgumentException){rejected=true;}
            Check(rejected,"processor malformed IR before native emission");
        }
        byte[] original=[0xf3,0x49,0x0f,0xc7,0xff];var frozen=ProcessorClassify(original);
        Array.Fill(original,(byte)0xcc);
        Check(frozen.Length==5&&frozen.Destination.Register==15,"value IR owns no caller byte storage");
    }

    // Independent scalar CPUID oracle. String lanes are assembled from ASCII,
    // rather than sharing the emitter's table or immediate hex constants.
    static (uint A,uint B,uint C,uint D) ProcessorCpuidOracle(uint leaf,uint subleaf,bool selected=false)
    {
        static uint Lane(string s,int offset)
        {uint v=0;for(int j=0;j<4;++j)if(offset+j<s.Length)v|=(uint)s[offset+j]<<(8*j);return v;}
        if(leaf is 0 or 0x80000000)return(leaf==0?7u:0x80000001u,Lane("GenuineIntel",0),Lane("GenuineIntel",8),Lane("GenuineIntel",4));
        if(leaf==0x40000000)return(0,Lane("GenuineBlink",0),Lane("GenuineBlink",4),Lane("GenuineBlink",8));
        if(leaf is 0x40031337 or 0x40031338)
        {string name=selected?"Unknown":leaf==0x40031337?"Linux":"x86_64";return(0,Lane(name,0),Lane(name,4),Lane(name,8));}
        if(leaf==1)return(0,0,1u<<31,(1u<<0)|(1u<<4)|(1u<<6)|(1u<<8)|(1u<<15)|(1u<<19)|(1u<<23)|(1u<<24)|(1u<<25)|(1u<<26));
        if(leaf==0x80000001)return(0,0,0,(1u<<0)|(1u<<8)|(1u<<11)|(1u<<15)|(1u<<20)|(1u<<23)|(1u<<24)|(1u<<29));
        if(leaf==2)return(0x76035a01,0xf0b0ff,0,0xca0000);
        if(leaf==4)return subleaf switch{0=>(0x1c004121u,0x1c0003fu,63u,0u),1=>(0x1c004122u,0x1c0003fu,63u,0u),
            2=>(0x1c004143u,0x1c0003fu,511u,0u),3=>(0x1c03c163u,0x3c0003fu,8191u,6u),_=>(0,0,0,0)};
        return(0,0,0,0);
    }

    static void CheckProcessorLeaves(byte* machine,ProcessorBacking backing)
    {
        var m=(Blink.Machine*)machine;var cpuid=ProcessorClassify([0x66,0x4f,0x0f,0xa2]);
        using(var code=CreateFullBlock([new(cpuid)]))
        foreach(uint leaf in ProcessorLeaves)foreach(uint sub in ProcessorSubleaves)
        {
            ProcessorSeed(machine,backing);Write64(machine,24,0xbadcafe000000000UL|leaf);Write64(machine,32,0xfefefefe00000000UL|sub);
            byte[] before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            var want=ProcessorCpuidOracle(leaf,sub);Write64Expected(before,24,want.A);Write64Expected(before,48,want.B);
            Write64Expected(before,32,want.C);Write64Expected(before,40,want.D);Write64Expected(before,0,Ip+(uint)cpuid.Length);
            var f=ProcessorRun(code,machine,cpuid,out var exit,out long issued,out long done);
            Check(exit==FullBlockExit.Continue&&issued==1&&done==1&&f.Executed==1&&
                new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).SequenceEqual(before),
                $"software CPUID all guest state, zeroextension, leaf={leaf:x}/sub={sub:x}");
        }
        foreach(int reg in Enumerable.Range(0,16))foreach(byte[] prefix in new byte[][]{[0xf3],[0x66,0xf3],[0xf3,0x48]})
        {
            byte rex=(byte)(0x40|(reg>>3));var i=ProcessorClassify([..prefix,rex,0x0f,0xc7,(byte)(0xf8|(reg&7))]);
            using var code=CreateFullBlock([new(i)]);ProcessorSeed(machine,backing);m->traprdtsc=true;
            var want=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();Write64Expected(want,24+reg*8,0);Write64Expected(want,0,Ip+(uint)i.Length);
            ProcessorRun(code,machine,i,out var exit,out long issue,out long done);
            Check(exit==FullBlockExit.Continue&&issue==1&&done==1&&new ReadOnlySpan<byte>(machine,want.Length).SequenceEqual(want),
                "RDPID full64 zero AUX preserves flags/otherstate and ignores RDTSC trap");
        }
    }

    static void Write64Expected(byte[] bytes,int offset,ulong value)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset,8),value);
        if(offset==0)bytes[8]=0; // successful architectural Commit clears seeded oplen7
    }
    static ulong ProcessorRead64(byte* machine,int offset)=>BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(machine+offset,8));
    static ulong ProcessorDescriptor(uint limit,int type,int dpl,bool system,bool present,bool granular)
        =>(limit&0xffffUL)|((ulong)(limit>>16&15)<<48)|((ulong)type<<40)|(system?0:1UL<<44)|
            ((ulong)dpl<<45)|(present?1UL<<47:0)|(granular?1UL<<55:0);

    // Architectural CPL3/no-LDTR scalar oracle, independent field extraction.
    static bool ProcessorLslOracle(ushort selector,ulong tableBase,ushort tableLimit,ulong descriptor,ulong upper,out uint limit)
    {
        limit=0;int offset=selector/8*8;
        if((selector&4)!=0||offset==0||offset+7>tableLimit)return false;
        bool system=((descriptor>>44)&1)==0;int type=(int)(descriptor>>40)&15;
        int count=system?16:8;
        if(system&&(type is not(2 or 9 or 11)||offset+15>tableLimit||(upper>>40&31)!=0))return false;
        if(tableBase>FullProcessorQueryAdmission.RealSize-(uint)count||(ulong)offset>FullProcessorQueryAdmission.RealSize-(uint)count-tableBase)return false;
        bool conforming=!system&&(type&8)!=0&&(type&4)!=0;
        if(!conforming&&(int)(descriptor>>45&3)<3)return false;
        uint raw=(uint)(descriptor&65535)|(uint)(descriptor>>48&15)<<16;
        limit=(descriptor>>55&1)==0?raw:(raw+1)*4096u-1;return true;
    }

    static void CheckProcessorDescriptors(byte* machine,ProcessorBacking backing)
    {
        var m=(Blink.Machine*)machine;var s=backing.System;s->gdt_base=128;s->gdt_limit=31;
        var i=ProcessorClassify([0x0f,0x03,0xc1]);using(var code=CreateFullBlock([new(i)]))
        foreach(int type in Enumerable.Range(0,32))foreach(int dpl in Enumerable.Range(0,4))
        foreach(int rpl in Enumerable.Range(0,4))foreach(bool present in new[]{false,true})
        {
            ulong descriptor=ProcessorDescriptor(0xabcde,type&15,dpl,type<16,present,(type&1)!=0);
            BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+136,8),descriptor);
            BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+144,8),0);
            ProcessorSeed(machine,backing);Write64(machine,32,0xfedcba9800000008UL|(uint)rpl);
            ulong old=ProcessorRead64(machine,24);bool success=ProcessorLslOracle((ushort)(8|rpl),128,31,descriptor,0,out uint limit);
            var expected=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            Write64Expected(expected,0,Ip+(uint)i.Length);Write64Expected(expected,424,136);Write64Expected(expected,440,8);
            if(success)Write64Expected(expected,24,limit);
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(12,4),ProcessorFlags&~64u|(success?64u:0u));
            var f=ProcessorRun(code,machine,i,out var exit,out long issued,out long done,backing,bind:true);
            Check(exit==FullBlockExit.Continue&&issued==1&&done==1&&f.Executed==1&&
                ProcessorRead64(machine,24)==(success?limit:old)&&new ReadOnlySpan<byte>(machine,expected.Length).SequenceEqual(expected),
                "LSL exhaustive IA32e type/DPL/RPL/P/granularity; CPL3 conforming exception; only ZF changes");
        }
        ulong ordinary=ProcessorDescriptor(0xfffff,3,3,false,true,true);
        foreach(int width in new[]{16,32,64})foreach((int dst,int src) in new[]{(0,1),(8,9),(15,15),(1,1)})
        {
            byte[] word=width==16?[0x66]:[];byte rex=(byte)(0x40|(width==64?8:0)|((dst>>3)<<2)|(src>>3));
            var op=ProcessorClassify([..word,rex,0x0f,0x03,(byte)(0xc0|(dst&7)<<3|(src&7))]);using var code=CreateFullBlock([new(op)]);
            foreach(ushort selector in new ushort[]{0,1,3,4,7,8,11,12,0xffff})
            {
                BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+136,8),ordinary);
                ProcessorSeed(machine,backing);Write64(machine,24+src*8,0xfacecafedead0000UL|selector);
                ulong old=ProcessorRead64(machine,24+dst*8);bool success=ProcessorLslOracle(selector,128,31,ordinary,0,out uint limit);
                ProcessorRun(code,machine,op,out var exit,out _,out _,backing,bind:true);
                ulong want=success?width==16?(old&~65535UL)|(ushort)limit:limit:old;
                Check(exit==FullBlockExit.Continue&&ProcessorRead64(machine,24+dst*8)==want&&m->flags==(ProcessorFlags&~64u|(success?64u:0u)),
                    "LSL aliases/high registers/null/TI/indexbound and16merge/32zeroextend/64zeroextend");
            }
        }
        // Inclusive GDT and real-allocation boundaries, overflow, full system
        // descriptor reservation, and missing parent mapping lease.
        foreach((ulong tableBase,ushort tableLimit,ulong upper,bool system,bool bind) in new (ulong,ushort,ulong,bool,bool)[]{
            (128,14,0,false,true),(128,15,0,false,true),(128,22,0,true,true),(128,23,0,true,true),
            (128,23,1UL<<40,true,true),(128,23,1UL<<44,true,true),(128,23,1UL<<45,true,true),
            (0x1000000-16,15,0,false,true),(0x1000000-15,15,0,false,true),
            (0x1000000-24,23,0,true,true),(0x1000000-23,23,0,true,true),
            (ulong.MaxValue-7,31,0,false,true),(128,31,0,false,false)})
        {
            s->gdt_base=tableBase;s->gdt_limit=tableLimit;
            ulong descriptor=ProcessorDescriptor(0x12345,system?2:3,3,system,false,false);
            if(tableBase<=0x1000000-16)BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+(nint)tableBase+8,8),descriptor);
            if(tableBase<=0x1000000-24)BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+(nint)tableBase+16,8),upper);
            ProcessorSeed(machine,backing);Write64(machine,32,8);ulong old=ProcessorRead64(machine,24);
            bool success=ProcessorLslOracle(8,tableBase,tableLimit,descriptor,upper,out uint limit);
            using var code=CreateFullBlock([new(i)]);ProcessorRun(code,machine,i,out var exit,out _,out long done,backing,bind:bind);
            Check(exit==(bind?FullBlockExit.Continue:FullBlockExit.Invalidated)&&done==(bind?1:0)&&
                ProcessorRead64(machine,24)==(bind&&success?limit:old)&&m->flags==(bind?ProcessorFlags&~64u|(success?64u:0):ProcessorFlags),
                "bounded nonempty native descriptor computation or explicit MISSING admission");
        }
        s->gdt_base=0;s->gdt_limit=0;
        ProcessorSeed(machine,backing);Write64(machine,32,8);
        using(var code=CreateFullBlock([new(i)]))
        {ProcessorRun(code,machine,i,out var exit,out _,out _,backing);Check(exit==FullBlockExit.Continue&&(m->flags&64)==0,"proven default empty Linux GDT needs no real lease");}
    }

    static void CheckProcessorMemory(byte* machine,ProcessorBacking backing)
    {
        var m=(Blink.Machine*)machine;backing.System->gdt_base=128;backing.System->gdt_limit=31;
        ulong descriptor=ProcessorDescriptor(0xabcde,3,3,false,true,true);
        BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+136,8),descriptor);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach(int offset in new[]{0,4094,4095})foreach(bool nonempty in new[]{false,true})
        foreach(byte[] prefix in new byte[][]{[],[0x66],[0x48],[0x64],[0x65,0x67]})
        {
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();new Span<byte>(backing.Pages,8192).Fill(0xa5);
            BinaryPrimitives.WriteUInt16LittleEndian(new(backing.Pages+offset,2),8);
            SetFullMemoryEntry(tlb,0x100000,backing.Pages,FullMemoryAccess.Read);
            SetFullMemoryEntry(tlb,0x101000,backing.Pages+4096,FullMemoryAccess.Read);
            var i=ProcessorClassify([..prefix,0x0f,0x03,0x03]);using var code=CreateFullBlock([new(i)]);
            ProcessorSeed(machine,backing);backing.System->gdt_limit=nonempty?(ushort)31:(ushort)0;
            m->fs.@base=m->gs.@base=64;
            bool seg=i.Address.Segment is 4 or 5;ulong address=(ulong)(0x100000+offset-(seg?64:0));
            Write64(machine,48,i.Address.AddressWidth==32?0xfeedface00000000UL|address:address);
            ulong old=ProcessorRead64(machine,24);uint limit=0xabcdefff;
            var f=ProcessorRun(code,machine,i,out var exit,out long issue,out long done,backing,tlb,bind:nonempty);
            Check(exit==FullBlockExit.Continue&&issue==1&&done==1&&f.NativeMemoryComputations==1&&
                ProcessorRead64(machine,24)==(nonempty?i.Width==16?(old&~65535UL)|(ushort)limit:limit:old)&&
                m->flags==(ProcessorFlags&~64u|(nonempty?64u:0)),"LSL exact m16 one/split page RO source/FS/GS/EA32");
            Check(backing.Pages[offset]==8&&backing.Pages[offset+1]==0&&
                (offset==0||backing.Pages[offset-1]==0xa5)&&backing.Pages[offset+2]==0xa5,
                "LSL read canaries stay inside actual8192 allocation");
        }
        // Source translation precedes even a null selector or missing descriptor
        // mapping. A real fixed TLB is supplied on every attempted memory path.
        var mem=ProcessorClassify([0x0f,0x03,0x03]);using(var code=CreateFullBlock([new(mem)]))
        {
            ProcessorSeed(machine,backing);Write64(machine,48,0x100fff);
            backing.Pages[4095]=0;backing.Pages[4096]=0;
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,0x100000,backing.Pages,FullMemoryAccess.Read);
            var before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            var f=ProcessorRun(code,machine,mem,out var exit,out long issue,out long done,backing,tlb);
            Check(exit==FullBlockExit.NeedMemory&&issue==1&&done==0&&f.MemoryAddress==0x100fff&&f.MemoryWidth==16&&
                new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"missing second sourcepage before null-selector/ZF effects");
            // Same issue re-entry with actual storage, using fresh invocation pointers.
            ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var resumed=FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            resumed.MemoryTlb=tlb;resumed.MemoryTlbMask=63;resumed.FirstPreissued=1;
            SetFullMemoryEntry(tlb,0x101000,backing.Pages+4096,FullMemoryAccess.Read);
            Check(RunFull(code,&resumed)==FullBlockExit.Continue&&issued==1&&completed==1&&(m->flags&64)==0,
                "LSL source miss resumes same reserved issue, null selector fails in native code");
        }
        foreach(byte[] bytes in new byte[][]{[0x0f,0x03,0x03],[0x36,0x0f,0x03,0x03],[0x0f,0x03,0x04,0x24],
            [0x3e,0x0f,0x03,0x04,0x24],[0x41,0x0f,0x03,0x45,0]})
        foreach(ulong address in new[]{0x0000800000000000UL,0x00007fffffffffffUL,0xffff7fffffffffffUL,ulong.MaxValue})
        {
            var i=ProcessorClassify(bytes);using var code=CreateFullBlock([new(i)]);ProcessorSeed(machine,backing);
            Write64(machine,24+i.Address.Base*8,address);new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            var before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            var f=ProcessorRun(code,machine,i,out var exit,out long issue,out long done,backing,tlb);
            Check(exit==FullBlockExit.GuestFault&&f.FaultTrap==(FullScalarMiscLowering.UsesStackSegment(i.Address)?12:13)&&
                f.FaultCode==128&&f.FaultAddress==0&&issue==1&&done==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                "LSL source GP/SS/endcanonical/wrap before descriptor/MMU/guest effects");
        }
        // A destination that also forms the EA must use the original address;
        // high REX.R/B fields cannot exchange source/destination roles.
        foreach((int reg,byte rex) in new[]{(3,(byte)0x48),(9,(byte)0x4d),(15,(byte)0x4d)})
        {
            backing.System->gdt_base=128;backing.System->gdt_limit=31;
            BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+136,8),descriptor);
            backing.Pages[0]=8;backing.Pages[1]=0;
            var i=ProcessorClassify([rex,0x0f,0x03,(byte)((reg&7)<<3|(reg&7))]);
            using var code=CreateFullBlock([new(i)]);ProcessorSeed(machine,backing);Write64(machine,24+reg*8,0x100000);
            new Span<FullMemoryTlbEntry>(tlb,64).Clear();SetFullMemoryEntry(tlb,0x100000,backing.Pages,FullMemoryAccess.Read);
            ProcessorRun(code,machine,i,out var exit,out _,out _,backing,tlb,bind:true);
            Check(exit==FullBlockExit.Continue&&ProcessorRead64(machine,24+reg*8)==0xabcdefff,
                "LSL destination aliases effective-address GPR, including high registers");
        }
    }

    static void CheckProcessorGates(byte* machine,ProcessorBacking backing)
    {
        foreach(byte[] bytes in new byte[][]{[0x0f,0xa2],[0x0f,0x03,0xc1],[0xf3,0x0f,0xc7,0xf8]})
        {
            var i=ProcessorClassify(bytes);using var code=CreateFullBlock([new(i)]);
            foreach(string gate in new[]{"stop","attention","mode","TF","RF","AC","metal","authority","budget","cpuidtrap"})
            {
                ProcessorSeed(machine,backing);var m=(Blink.Machine*)machine;
                if(gate=="attention")m->attention=true;if(gate=="mode")m->mode.omode=0;
                if(gate=="TF")m->flags|=0x100;if(gate=="RF")m->flags|=0x10000;if(gate=="AC")m->flags|=0x40000;
                if(gate=="metal")m->metal=true;if(gate=="cpuidtrap")m->trapcpuid=true;
                ulong epoch=11;long issue=0,done=0;int stop=gate=="stop"?1:0;var h=FullHeader(Ip,code.Entry);
                var frame=FullFrame(machine,&h,&epoch,&issue,&done,&stop,gate=="budget"?0UL:100UL,1);
                if(gate=="authority")++epoch;
                FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
                var before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
                FullBlockExit want=gate=="stop"?FullBlockExit.Stop:gate=="attention"?FullBlockExit.Attention:
                    gate=="budget"?FullBlockExit.Budget:gate=="cpuidtrap"?(i.Options==(uint)FullProcessorQueryOperation.Cpuid?FullBlockExit.GuestFault:FullBlockExit.Continue):FullBlockExit.Invalidated;
                var exit=RunFull(code,&frame);
                Check(exit==want&&done==(want==FullBlockExit.Continue?1:0)&&
                    issue==(gate is "budget" or "cpuidtrap" or "metal"?1:0),"processor poll/context/budget/trap precedence "+gate);
                if(want!=FullBlockExit.Continue)Check(new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),"blocked processor query no guest effects");
                if(want==FullBlockExit.GuestFault)Check(frame.FaultTrap==14&&frame.FaultCode==unchecked((uint)(int)m->segvcode)&&frame.FaultAddress==0,
                    "selected trapcpuid trap14/current signed segvcode before register writes/serialization");
            }
            using var cancellation=new CancellationTokenSource();using var stopOwner=new HostExecutionStop(cancellation:cancellation.Token);
            using var poll=stopOwner.AcquireNativePoll();cancellation.Cancel();ProcessorSeed(machine,backing);
            ulong global=11;long issued=1,completed=0;var header=FullHeader(Ip,code.Entry);
            var cancelled=FullFrame(machine,&header,&global,&issued,&completed,poll.Pointer,0,1);cancelled.FirstPreissued=1;
            Check(RunFull(code,&cancelled)==FullBlockExit.Stop&&issued==1&&completed==0&&cancelled.FirstPreissued==1,
                "external cancellation preserves approved first issue at Budget0");
            ProcessorSeed(machine,backing);issued=1;completed=0;int running=0;header=FullHeader(Ip,code.Entry);
            var approved=FullFrame(machine,&header,&global,&issued,&completed,&running,0,1);approved.FirstPreissued=1;
            Check(RunFull(code,&approved)==FullBlockExit.Continue&&issued==1&&completed==1&&approved.FirstPreissued==0,
                "processor approved first issue completes once at Budget0");
        }
        // Selected CPUID faults preserve signed Linux segvcode. Code0 remains0
        // until HaltMachine's default-to-MAPERR delivery, and -2 selects SIGBUS.
        var cpuid=ProcessorClassify([0x0f,0xa2]);using var trapped=CreateFullBlock([new(cpuid)]);
        foreach(sbyte signalCode in new sbyte[]{0,1,2,unchecked((sbyte)-2)})
        {
            ProcessorSeed(machine,backing);var m=(Blink.Machine*)machine;m->trapcpuid=true;m->segvcode=signalCode;
            var before=new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            var f=ProcessorRun(trapped,machine,cpuid,out var exit,out long issue,out long done);
            Check(exit==FullBlockExit.GuestFault&&f.FaultTrap==14&&f.FaultCode==unchecked((uint)(int)signalCode)&&
                issue==1&&done==0&&new ReadOnlySpan<byte>(machine,before.Length).SequenceEqual(before),
                "CPUID exact selected segmentation fault path before all guest writes");
        }
    }

    static void CheckProcessorMappingAdmission(byte* machine,ProcessorBacking backing)
    {
        ProcessorSeed(machine,backing);var f=new FullBlockFrame{Machine=machine};
        foreach(nuint length in new nuint[]{0,4096,FullProcessorQueryAdmission.RealSize-1})
        {
            f.Reserved=FullProcessorQueryAdmission.DescriptorMappingTag;
            Check(!FullProcessorQueryAdmission.BindRetainedDescriptorMapping(&f,backing.System,backing.Real,length)&&f.Reserved==0,
                "short mapping cannot inherit descriptor admission tag");
        }
        Check(!FullProcessorQueryAdmission.BindRetainedDescriptorMapping(&f,backing.System,backing.Pages,FullProcessorQueryAdmission.RealSize)&&f.Reserved==0,
            "pointer identity required even when supplied length claims16MiB");
        Check(FullProcessorQueryAdmission.BindRetainedDescriptorMapping(&f,backing.System,backing.Real,FullProcessorQueryAdmission.RealSize),
            "owned stable test mapping admitted");
        FullProcessorQueryAdmission.UnbindDescriptorMapping(&f);Check(f.Reserved==0,"unbound before releasing actual lifetime owner");
        var old=((Blink.Machine*)machine)->blink_host_system;((Blink.Machine*)machine)->blink_host_system=null;
        Check(!FullProcessorQueryAdmission.BindRetainedDescriptorMapping(&f,backing.System,backing.Real,FullProcessorQueryAdmission.RealSize),
            "mapping lease cannot transfer to another machine system");((Blink.Machine*)machine)->blink_host_system=old;
    }

    static void CheckProcessorEmission()
    {
        FullBlockInstruction[] steps=[new(ProcessorClassify([0x0f,0xa2])),new(ProcessorClassify([0x48,0x0f,0x03,0xc1],Ip+2)),
            new(ProcessorClassify([0x66,0xf3,0x49,0x0f,0xc7,0xff],Ip+6))];
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity];
        foreach(bool windows in new[]{false,true})
        {
            Check(FullBlockEmitter.TryEmit(steps,staging,out int n,windows)&&n>0,"processor family bothABI encoding");
            ReadOnlySpan<byte> serial=[0x49,0x89,0xd9,0x31,0xc0,0x0f,0xa2,0x4c,0x89,0xcb];
            Check(staging.AsSpan(0,n).IndexOf(serial)>=0,"serialization CPUID restores exact RBX via volatile R9");
        }
        for(int k=0;k<32;++k)FullBlockEmitter.Emit(steps,staging);
        long allocated=GC.GetAllocatedBytesForCurrentThread();for(int k=0;k<1000;++k)FullBlockEmitter.Emit(steps,staging);
        Check(GC.GetAllocatedBytesForCurrentThread()==allocated,"processor warm span emission allocates zero");
    }

    static void CheckProcessorSelectedOracle()
    {
        using var f=new ProcessorFixture(interpreterOracle:true);var m=f.Machine();byte* machine=(byte*)m;
        Check(m->blink_host_system->gdt_limit==0&&m->blink_host_system->gdt_base==0&&m->blink_host_system->real==null,
            "selected actual long NewSystem proves empty GDT/no real allocation");
        foreach(uint leaf in ProcessorLeaves)foreach(uint sub in ProcessorSubleaves)
        {
            m->ip=Ip;m->flags=ProcessorFlags;Write64(machine,24,0xdeadbeef00000000UL|leaf);Write64(machine,32,0xfeedface00000000UL|sub);
            f.DataScope(0,()=>ControlFlagReference(f.Program,(byte*)f.Machine(),[0x0f,0xa2]));
            var want=ProcessorCpuidOracle(leaf,sub,selected:true);
            Check(ProcessorRead64(machine,24)==want.A&&ProcessorRead64(machine,48)==want.B&&ProcessorRead64(machine,32)==want.C&&ProcessorRead64(machine,40)==want.D&&m->flags==ProcessorFlags,
                "actual selected GetOp CPUID oracle, isolated from product native execution");
            if(leaf is 0x40031337 or 0x40031338)Check(want!=ProcessorCpuidOracle(leaf,sub),
                "EXPECTED SELECTED GAP: Unknown OS/arch differs from required Linux guest profile");
        }
        foreach(byte[] bytes in new byte[][]{[0xf3,0x0f,0xc7,0xf8],[0x66,0xf3,0x49,0x0f,0xc7,0xff]})
        {
            m->ip=Ip;m->flags=ProcessorFlags;Write64(machine,24,ulong.MaxValue);Write64(machine,144,ulong.MaxValue);
            var i=ProcessorClassify(bytes);f.DataScope(0,()=>ControlFlagReference(f.Program,(byte*)f.Machine(),bytes));
            Check(ProcessorRead64(machine,24+i.Destination.Register*8)==0&&m->flags==ProcessorFlags,"selected RDPID AUX0/fullwidth oracle");
        }
        using var backing=new ProcessorBacking();var system=m->blink_host_system;
        byte* savedReal=system->real;ulong savedBase=system->gdt_base;ushort savedLimit=system->gdt_limit;
        try
        {
            foreach((ushort selector,ulong descriptor,ulong upper,bool expectedGap) in new (ushort,ulong,ulong,bool)[]{
                (8,ProcessorDescriptor(0xabcde,3,3,false,true,true),0,false),
                (12,ProcessorDescriptor(0xabcde,3,3,false,true,true),0,true), // selected drops TI
                (8,ProcessorDescriptor(0xabcde,3,0,false,true,true),0,true), // selected ignores DPL
                (8,ProcessorDescriptor(0xabcde,0,3,true,true,true),0,true), // reserved system type
                (8,ProcessorDescriptor(0xabcde,2,3,true,true,true),1UL<<40,true),
                (8,ProcessorDescriptor(0xabcde,12,0,false,false,true),0,false)}) // conforming, P ignored
            {
                BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+8,8),descriptor);
                BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+16,8),upper);
                m->ip=Ip;m->flags=ProcessorFlags;Write64(machine,24,0xdeadbeef);Write64(machine,32,selector);
                // Borrowed test storage is installed ONLY inside the oracle's
                // binding scope and restored before any full fixture/disposal.
                f.DataScope(0,()=>{system->real=backing.Real;system->gdt_base=0;system->gdt_limit=31;
                    try{ControlFlagReference(f.Program,(byte*)f.Machine(),[0x0f,0x03,0xc1]);}
                    finally{system->real=savedReal;system->gdt_base=savedBase;system->gdt_limit=savedLimit;}});
                bool architecture=ProcessorLslOracle(selector,0,31,descriptor,upper,out uint architecturalLimit);
                uint selectedLimit=(uint)(descriptor&65535)|((uint)(descriptor>>48)&15)<<16;
                if((descriptor>>55&1)!=0)selectedLimit=(selectedLimit<<12)|4095;
                Check((m->flags&64)!=0&&ProcessorRead64(machine,24)==selectedLimit&&((m->flags^ProcessorFlags)&~64u)==0,
                    "actual selected GetOp descriptor extraction oracle");
                Check(expectedGap?!architecture:architecture&&architecturalLimit==selectedLimit,
                    "selected missing TI/type/privilege/upperdescriptor validation recorded as expected gaps");
            }
        }
        finally{system->real=savedReal;system->gdt_base=savedBase;system->gdt_limit=savedLimit;}
    }

    static void CheckProcessorControllers()
    {
        // Oracle fixture is already disposed. NativePrepared limits are applied
        // to every fresh frame; the ordinary controller remains the product path.
        using(var f=new ProcessorFixture())
        {
            const ulong pc=ProcessorFixture.Base;f.Map(pc);f.Fill(pc);WriteControllerCode(f,pc,[0x0f,0xa2,0xeb,0xfe]);
            Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
            foreach(uint leaf in ProcessorLeaves)foreach(uint sub in ProcessorSubleaves)
            {
                var m=f.Machine();m->flags=ProcessorFlags;Write64((byte*)m,24,leaf);Write64((byte*)m,32,sub);
                f.Counts(1);f.Step(pc);for(int k=0;k<8&&f.Completed<1;++k)f.Continue();
                var want=ProcessorCpuidOracle(leaf,sub);
                Check(f.Completed==1&&ProcessorRead64((byte*)m,24)==want.A&&ProcessorRead64((byte*)m,48)==want.B&&
                    ProcessorRead64((byte*)m,32)==want.C&&ProcessorRead64((byte*)m,40)==want.D&&m->flags==ProcessorFlags,
                    "real controller all software query leaves/subleaves");
                Check(f.Statistics.SemanticHelperCalls==0&&f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                    "CPUID missing family closed by emitted computation, not GetOp runtime dispatch");
            }
        }
        foreach(byte[] bytes in new byte[][]{[0x0f,0x03,0xc1],[0x66,0x45,0x0f,0x03,0xff],
            [0x48,0x0f,0x03,0xc1],[0xf3,0x0f,0xc7,0xf8],[0x66,0xf3,0x49,0x0f,0xc7,0xff]})
        {
            using var f=new ProcessorFixture();const ulong pc=ProcessorFixture.Base;f.Map(pc);f.Fill(pc);WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            var i=ProcessorClassify(bytes,pc);var m=f.Machine();m->flags=ProcessorFlags;
            Write64((byte*)m,24+i.Destination.Register*8,0xdeadbeefcafebabe);Write64((byte*)m,24+i.Source.Register*8,8);
            ulong before=ProcessorRead64((byte*)m,24+i.Destination.Register*8);
            Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
            f.Counts(1);f.Step(pc);for(int k=0;k<8&&f.Completed<1;++k)f.Continue();
            Check(f.Completed==1&&ProcessorRead64((byte*)m,24+i.Destination.Register*8)==(i.Options==(uint)FullProcessorQueryOperation.Rdpid?0:before)&&
                m->flags==(i.Options==(uint)FullProcessorQueryOperation.Rdpid?ProcessorFlags:ProcessorFlags&~64u)&&
                f.Statistics.SemanticHelperCalls==0&&f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "ordinary controller default empty GDT/RDPID computed natively");
        }
        CheckProcessorControllerRetainedMapping();CheckProcessorSnapshotLifetime();
    }

    static void CheckProcessorControllerRetainedMapping()
    {
        using var f=new ProcessorFixture();using var backing=new ProcessorBacking();const ulong pc=ProcessorFixture.Base;
        f.Map(pc,8192);f.Fill(pc,8192);WriteControllerCode(f,pc,[0x48,0x0f,0x03,0x03,0xeb,0xfe]);
        var m=f.Machine();var s=m->blink_host_system;byte* real=s->real;ulong tableBase=s->gdt_base;ushort tableLimit=s->gdt_limit;
        ((byte*)f.Physical(pc+4096))[16]=8;((byte*)f.Physical(pc+4096))[17]=0;
        BinaryPrimitives.WriteUInt64LittleEndian(new(backing.Real+8,8),ProcessorDescriptor(0xabcde,3,3,false,false,true));
        Write64((byte*)m,48,pc+4096+16);m->flags=ProcessorFlags;
        int bound=0,returned=0;
        Action<nint> bind=p=>
        {
            // Test-only implementation of the published parent mapping seam.
            // No decoder, result computation or OpLsl call is performed here.
            s->real=backing.Real;s->gdt_base=0;s->gdt_limit=31;var frame=(FullBlockFrame*)p;frame->NativeLimit=1;
            Check(FullProcessorQueryAdmission.BindRetainedDescriptorMapping(frame,s,backing.Real,FullProcessorQueryAdmission.RealSize),"controller retained test allocation bind");++bound;
        };
        f.NativePrepared=bind;f.NativeReturned=p=>
        {FullProcessorQueryAdmission.UnbindDescriptorMapping((FullBlockFrame*)p);s->real=real;s->gdt_base=tableBase;s->gdt_limit=tableLimit;++returned;f.NativePrepared=bind;};
        try
        {
            f.Counts(1);f.Step(pc);for(int k=0;k<12&&f.Completed<1;++k)f.Continue();
            Check(f.Completed==1&&ProcessorRead64((byte*)m,24)==0xabcdefff&&m->flags==(ProcessorFlags|64)&&bound>0&&bound==returned&&
                f.Statistics.NativeMemoryInstructions==1&&f.Statistics.SemanticHelperCalls==0&&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "real controller nonempty GDT computation/m16 mapping assist with mapping-only test seam");
        }
        finally{f.NativePrepared=null;f.NativeReturned=null;s->real=real;s->gdt_base=tableBase;s->gdt_limit=tableLimit;}
        // Production parent lease acquisition remains MISSING until integrated;
        // this test injection cannot qualify product lifecycle by itself.
    }

    static void CheckProcessorSnapshotLifetime()
    {
        using var f=new ProcessorFixture();const ulong pc=ProcessorFixture.Base;f.Map(pc);f.Fill(pc);
        byte[] original=[0x66,0xf3,0x49,0x0f,0xc7,0xff];WriteControllerCode(f,pc,[..original,0xeb,0xfe]);
        Check(f.Acquire(pc,0,out var lease,out _,out int error,requestedCount:1)&&error==0,"processor retain original snapshot");
        using(lease)
        {
            var plan=FullTranslationJit.Plans(f.Jit.Store,in lease)[0];var decoded=plan.Decoded;
            Check(new ReadOnlySpan<byte>(decoded.bytes,decoded.length).SequenceEqual(original),"retained payload preserves redundant original bytes");
            Array.Fill(original,(byte)0xcc);
            Check(decoded.length==6&&decoded.bytes[0]==0x66&&decoded.bytes[3]==0x0f,"snapshot independent caller lifetime");
        }
        Check(f.Jit.Store.GetStatistics().ActiveManagedLeases==0,"snapshot lease released before fixture disposal");
    }

    static void CheckProcessorControllerFaults()
    {
        foreach(byte[] bytes in new byte[][]{[0x0f,0xa2],[0x0f,0x03,0x03],[0x36,0x0f,0x03,0x03]})
        {
            using var f=new ProcessorFixture();const ulong pc=ProcessorFixture.Base;f.Map(pc);f.Fill(pc);
            // Prior ordinary MOV must commit before the faulting family site.
            WriteControllerCode(f,pc,[0x41,0xb8,0x78,0x56,0x34,0x12,..bytes,0xeb,0xfe]);
            var m=f.Machine();m->flags=ProcessorFlags;Write64((byte*)m,48,0x0000800000000000);
            if(bytes[1]==0xa2)m->trapcpuid=true;
            ulong ax=ProcessorRead64((byte*)m,24);var fault=f.StepSynchronousFault(pc);
            uint vector=bytes[1]==0xa2?14u:bytes[0]==0x36?12u:13u;
            Check(fault.Signal==(vector==12?7:11)&&m->trapno==vector&&m->ip==pc+6&&f.Completed==1&&
                ProcessorRead64((byte*)m,88)==0x12345678&&ProcessorRead64((byte*)m,24)==ax&&m->flags==ProcessorFlags&&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "controller prior MOV commit then selected CPUID trap14/LSL GP/SS before query effects");
        }
    }

    static void CheckProcessorMmuFaults()
    {
        using var f=new ProcessorFixture();const ulong pc=ProcessorFixture.Base, data=pc+4096;
        f.Map(pc,8192);f.Fill(pc,8192);WriteControllerCode(f,pc,[0x0f,0x03,0x03,0xeb,0xfe]);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        var m=f.Machine();m->flags=ProcessorFlags;Write64((byte*)m,48,data+4095);
        var i=ProcessorClassify([0x0f,0x03,0x03]);using var code=CreateFullBlock([new(i)]);
        ulong epoch=11;long issue=0,done=0;int stop=0;m->ip=Ip;var h=FullHeader(Ip,code.Entry);
        var frame=FullFrame((byte*)m,&h,&epoch,&issue,&done,&stop,limit:1);frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
        Check(RunFull(code,&frame)==FullBlockExit.NeedMemory&&issue==1&&done==0,"LSL actual native source request before MMU faults");
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultTrap==14&&frame.FaultCode==1&&frame.FaultAddress==data+4096,
            "LSL cross-page actual MMU MAPERR before null/empty-GDT effects");
        Check(f.Mutate("mprotect",data)==0,"processor source protection removed");frame.MemoryAddress=data;
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultTrap==14&&frame.FaultCode==2,
            "LSL actual MMU ACCERR source read before ZF");
        var file=f.Io.CreateMemoryFile("processor-query-eof",0);Check(file.Succeeded&&f.Io.TruncateFile(file.Value,4096).Succeeded,"processor EOF file");
        ulong shared=pc+0x10000;f.MapShared(file.Value,shared);f.Fill(shared);Check(f.Io.TruncateFile(file.Value,0).Succeeded,"processor EOF truncate");
        frame.MemoryAddress=shared;
        Check(FillFullMemoryMmu(f,&frame)==FullMemoryAssistResult.GuestFault&&frame.FaultTrap==14&&unchecked((int)frame.FaultCode)==-2&&frame.FaultAddress==shared,
            "LSL EOF keeps selected BUS_ADRERR sentinel, never fabricated descriptor failure");
    }
}
