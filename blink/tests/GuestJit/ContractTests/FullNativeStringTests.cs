using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using StringFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE-AUTHORED / UNRUN. Parent owns enum/core/ordered-MMU integration,
    // all builds and execution. Synthetic leaf pointers below are test-owned;
    // they do not establish native enrollment or actual concurrency safety.
    static void CheckFullNativeStrings(byte* machine)
    {
        Check(sizeof(FullBlockFrame)==416 && FullBlockFrame.AbiVersion==0x46420007 &&
            sizeof(TranslationBlockHeader)==160 && sizeof(FullStackMemoryDescriptor)==128 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.StackMemory)).ToInt32()==384,
            "R94 retains ABI5/frame400/header160/descriptor128");
        Check(!fullOwnerScaffold.IsValid,"R94 starts with standalone scaffold");
        using(var oracle=new StringFixture(interpreterOracle:true))
        {
            oracle.Map(StringFixture.Base,16384);oracle.Fill(StringFixture.Base,16384);
            CheckStringClassifier(oracle.Program);
            CheckStringOracles(oracle);
            CheckStringLeaves(machine);
            using var common=CreateFullScaffold(out var addresses);fullOwnerScaffold=addresses;
            try { CheckStringOracles(oracle);CheckStringLeaves(machine); }
            finally { fullOwnerScaffold=default; }
        }
        CheckStringOrderedMmu();
        CheckStringRemap();
        CheckStringController();
        CheckStringControllerFaults();
        CheckStringWatchedAlias();
        CheckStringAdmission();
        CheckStringEmission();
        ResetFull(machine);
    }

    static byte[] StringBytes(byte firstOpcode,int width,bool address32=false,int segment=-1)
    {
        var b=new List<byte>(5);
        if(segment>=0)b.Add(segment==4?(byte)0x64:(byte)0x65);
        if(width==16)b.Add(0x66);
        if(address32)b.Add(0x67);
        if(width==64)b.Add(0x48);
        b.Add((byte)(firstOpcode+(width==8?0:1)));
        return b.ToArray();
    }

    static JitInstruction StringClassify(byte[] bytes,ulong pc=Ip)
    {
        Check(FullStringLowering.TryClassify(pc,bytes,out var i,out string? reason),reason??"R94 classify");
        Check(FullBlockEmitter.CanInlineString(i)&&FullBlockEmitter.ValidateString(i)&&FullBlockEmitter.CanInline(i),
            "R94 canonical string IR reaches the core emitter");
        return i;
    }

    static void CheckStringClassifier(Blink p)
    {
        foreach(byte opcode in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(bool address32 in new[]{false,true})
        foreach(int segment in new[]{-1,4,5})
        {
            byte[] bytes=StringBytes(opcode,width,address32,segment);
            Blink.XedDecodedInst decoded=default;
            fixed(byte* b=bytes)Check(p.DecodeInstruction(&decoded,b,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0&&
                decoded.length==bytes.Length,"R94 trusted XED exact string length");
            var i=StringClassify(bytes);
            Check(i.Width==width&&i.SourceWidth==width&&i.Address.AddressWidth==(address32?32:64)&&
                i.Address.Base==(FullStringLowering.HasSource(i)?6:7)&&
                i.Address.Segment==(FullStringLowering.HasSource(i)?segment:-1),
                "R94 implicit source/destination, width, address-size and source-only override");
            if(FullStringLowering.IsPair(i))Check(FullStringLowering.PairSecondAccess(i)==
                (opcode==0xa4?FullMemoryAccess.Write:FullMemoryAccess.Read),"R94 MOVS RW versus CMPS RR shape");
            for(int cut=0;cut<bytes.Length;++cut)
                Check(!FullStringLowering.TryClassify(Ip,bytes.AsSpan(0,cut),out _,out _),"R94 truncated exact span rejects");
            Check(!FullStringLowering.TryClassify(Ip,[..bytes,0x90],out _,out _),"R94 trailing instruction rejects");
            foreach(byte repeat in new byte[]{0xf2,0xf3})
                Check(!FullStringLowering.TryClassify(Ip,[repeat,..bytes],out _,out string? missing)&&
                    missing?.Contains("REP")==true,"R94 ALL REP forms remain explicit MISSING, including count0/1");
        }
        foreach(byte op in new byte[]{0xa4,0xa5,0xa6,0xa7,0xaa,0xab,0xac,0xad,0xae,0xaf})
        for(byte rex=0x40;rex<=0x4f;++rex)
        {
            var i=StringClassify([0x66,0x67,rex,op]);
            Check(i.Width==((op&1)==0?8:(rex&8)!=0?64:16)&&i.Address.AddressWidth==32,
                "R94 REX.W overrides66 only on nonbyte forms; REX.RXB never redirect implicit registers");
        }
        foreach(byte segment in new byte[]{0x26,0x2e,0x36,0x3e,0x64,0x65})
        foreach(byte[] prefix in new byte[][]{[segment,0x66,0x67],[0x67,segment,0x66],[0x66,0x67,segment],
            [segment,0x67,0x66],[0x66,segment,0x67],[0x67,0x66,segment]})
        {
            Check(StringClassify([..prefix,0x48,0xa5]).Address.Segment==
                (segment==0x36?2:segment==0x64?4:segment==0x65?5:-1),"R162 source SS/FS/GS intergroup prefix permutations");
            Check(StringClassify([..prefix,0x48,0xab]).Address.Segment==-1,
                "R94 every destination-only override leaves ES:DI unchanged");
        }
        foreach(byte[] b in new byte[][]{[],[0x66],[0x48],[0xa4,0],new byte[16],
            [0xf0,0xa4],[0xf0,0xae],[0x66,0x66,0xad],[0x67,0x67,0xab],[0x64,0x65,0xa5],
            [0x48,0x66,0xa5],[0x40,0x48,0xa5],[0x6c],[0x6d],[0x6e],[0x6f],[0x0f,0xa4,0xc0,1]})
            Check(!FullStringLowering.TryClassify(Ip,b,out var miss,out string? reason)&&miss==default&&reason!=null,
                "R94 invalid/extra/reordered prefixes and INS/OUTS do not masquerade as strings");
        foreach(var good in new[]{StringClassify([0xa4]),StringClassify([0xae])})
        foreach(var bad in new[]{good with{Length=0},good with{Length=16},good with{Width=128},
            good with{SourceWidth=64},good with{Options=1},good with{Target=1},good with{Condition=1},
            good with{Source=new(1,0)},good with{Destination=new(0,1,true)},good with{Extra=new(1,0)},
            good with{Address=good.Address with{Index=0}},good with{Address=good.Address with{Displacement=1}},
            good with{Address=good.Address with{RipRelative=true}},good with{Address=good.Address with{AddressWidth=16}},
            good with{Address=good.Address with{Segment=3}}})
        {
            bool rejected=false;try{FullBlockEmitter.ValidateString(bad);}catch(ArgumentException){rejected=true;}
            Check(rejected,"R94 malformed string IR fails before emission");
        }
    }

    static void StringSeed(byte* m,ulong source,ulong destination,ulong accumulator,bool df)
    {
        // Preserve the actual fixture's owner, bus, MMU and lifetime fields.
        // Clearing sizeof(Machine) would invalidate the normal GetOp oracle.
        var machine=(Blink.Machine*)m;machine->ip=Ip;machine->oplen=0;
        machine->attention=false;machine->stashaddr=0;
        machine->mode.omode=Blink.XED_MODE_LONG;machine->mode.genmode=Blink.XED_GEN_MODE_PROTECTED;
        for(int r=0;r<16;++r)Write64(m,24+8*r,0xfedcba9876543210UL^(0x0101010101010101UL*(uint)r));
        Write64(m,24,accumulator);Write64(m,32,0x8877665500000000); // non-REP count must not change, even low0
        Write64(m,72,source);Write64(m,80,destination);
        ((Blink.Machine*)m)->flags=(0xa52c0e15u&~0x50500u)|(df?1024u:0u);
    }

    static ulong StringMask(int width)=>width==64?ulong.MaxValue:(1UL<<width)-1;
    static uint StringSubFlags(uint old,ulong a,ulong b,int width)
    {
        ulong mask=StringMask(width),sign=1UL<<(width-1);a&=mask;b&=mask;
        ulong result=unchecked(a-b)&mask;
        return (old&~0xff0008d1u)|(a<b?1u:0)|(uint)((a^b^result)&16)|
            (result==0?64u:0)|((result&sign)!=0?128u:0)|
            (((a^b)&(a^result)&sign)!=0?2048u:0)|((uint)(byte)result<<24);
    }
    static ulong StringNext(ulong initial,int width,int bytes,bool df)
        =>width==32?unchecked((uint)initial+(df?0u-(uint)bytes:(uint)bytes)):
            unchecked(initial+(df?0UL-(uint)bytes:(uint)bytes));
    static ulong StringRead(byte* first,byte* second,int firstLength,int width)
    {
        ulong value=0;
        for(int b=0;b<width/8;++b)value|=(ulong)(b<firstLength?first[b]:second[b-firstLength])<<(8*b);
        return value;
    }
    static void StringWrite(byte* first,byte* second,int firstLength,int width,ulong value)
    {
        for(int b=0;b<width/8;++b)
            if(b<firstLength)first[b]=(byte)(value>>(8*b));else second[b-firstLength]=(byte)(value>>(8*b));
    }
    static void StringPointers(ref FullStackMemoryRequest request,byte* page,byte* next)
    {
        request.Length0=Math.Min(request.Width/8,4096u-(uint)(request.Address&4095));
        request.Host0=page+(int)(request.Address&4095);
        request.Host1=request.Length0==request.Width/8?null:next;
    }
    static void StringTlb(FullBlockFrame* f,ulong address,byte* page,FullMemoryAccess access)
    {
        f->MemoryTlb[FullNativeMemory.TlbIndex(address,f->MemoryTlbMask)]=new(){Page=address&~4095UL,
            Host=page,AuthorityEpoch=f->ObservedGlobalEpoch,Context=f->Context,Permissions=FullMemoryAccess.Read|access,
            Identity=(ulong)page,ValidEnd=4096};
    }
    static void StringSupply(FullBlockFrame* f,byte* source,byte* sourceNext,byte* destination,byte* destinationNext,bool ticket)
    {
        if(f->StackMemory->Count==2)
        {
            StringPointers(ref f->StackMemory->Request0,source,sourceNext);
            StringPointers(ref f->StackMemory->Request1,destination,destinationNext);
            f->StackMemory->Epoch=f->ObservedGlobalEpoch;f->StackMemory->Ticket=ticket?123u:0u;
            f->StackMemory->State=ticket?FullStackMemoryState.TicketReady:FullStackMemoryState.Ready;
        }
        else
        {
            var r=new FullStackMemoryRequest{Address=f->MemoryAddress,Width=f->MemoryWidth,Access=f->MemoryAccess};
            StringPointers(ref r,source,sourceNext);
            if(ticket)
            {
                f->MemoryHost0=r.Host0;f->MemoryHost1=r.Host1;f->MemoryLength0=r.Length0;
                f->MemoryEpoch=f->ObservedGlobalEpoch;f->MemoryFlags=FullMemoryFlags.BarrierTicket;f->MemoryTicket=123;
            }
            else
            {
                StringTlb(f,r.Address,source,r.Access);
                if(r.Host1!=null)StringTlb(f,(r.Address&~4095UL)+4096,sourceNext,r.Access);
            }
        }
    }

    static void CheckStringLeaves(byte* m)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),p1=(byte*)NativeMemory.Alloc(4096);
        byte* q=(byte*)NativeMemory.Alloc(4096),q1=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(byte opcode in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
            foreach(int width in new[]{8,16,32,64})
            foreach(bool address32 in new[]{false,true})
            foreach(int segment in new[]{-1,4,5})
            {
                var i=StringClassify(StringBytes(opcode,width,address32,segment));using var code=CreateFullBlock([new(i)]);
                foreach(int offset in new[]{0,65,4095})
                foreach(bool df in new[]{false,true})
                foreach(bool ticket in new[]{false,true})
                foreach(ulong value in new[]{0UL,1UL,0x807f0000ffffffffUL})
                {
                    new Span<byte>(p,4096).Fill(0xa5);new Span<byte>(p1,4096).Fill(0x5a);
                    new Span<byte>(q,4096).Fill(0xcc);new Span<byte>(q1,4096).Fill(0xdd);
                    new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                    ulong source=0x100000+(uint)offset,destination=0x700000+(uint)offset;
                    ulong rawSource=source|(address32?0xdeadbeef00000000UL:0);
                    ulong rawDestination=destination|(address32?0xfedcba9800000000UL:0);
                    ulong fs=0x200000,gs=0x400000,ea=source+(i.Address.Segment==4?fs:i.Address.Segment==5?gs:0);
                    int n=Math.Min(width/8,4096-offset);ulong other=0x0123456789abcdef;
                    StringWrite(p+offset,p1,n,width,value);StringWrite(q+offset,q1,n,width,other);
                    StringSeed(m,rawSource,rawDestination,value,df);
                    byte[] expected=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                    byte[] ep=new ReadOnlySpan<byte>(p,4096).ToArray(),ep1=new ReadOnlySpan<byte>(p1,4096).ToArray();
                    byte[] eq=new ReadOnlySpan<byte>(q,4096).ToArray(),eq1=new ReadOnlySpan<byte>(q1,4096).ToArray();
                    if(FullStringLowering.HasSource(i))BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(72,8),StringNext(rawSource,i.Address.AddressWidth,width/8,df));
                    if(FullStringLowering.HasDestination(i))BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(80,8),StringNext(rawDestination,i.Address.AddressWidth,width/8,df));
                    if(i.Operation==JitOperation.StringLoad)
                    {
                        ulong loaded=width==64?value:width==32?(uint)value:(value&StringMask(width))|
                            (BinaryPrimitives.ReadUInt64LittleEndian(expected.AsSpan(24,8))&~StringMask(width));
                        BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24,8),loaded);
                    }
                    if(i.Operation is JitOperation.StringMove or JitOperation.StringStore)
                        for(int b=0;b<width/8;++b)if(b<n)eq[offset+b]=(byte)(value>>(8*b));else eq1[b-n]=(byte)(value>>(8*b));
                    if(i.Operation is JitOperation.StringCompare or JitOperation.StringScan)
                        BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(12,4),StringSubFlags(((Blink.Machine*)m)->flags,value,other,width));
                    BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(0,8),Ip+(uint)i.Length);expected[8]=0;
                    byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                    ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                    var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop);
                    f.StackMemory=&d;f.FsBase=fs;f.GsBase=gs;
                    Check(RunFull(code,&f)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&f.MemoryResume==1&&
                        new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),"R94 cold preflight has no architectural effect");
                    if(FullStringLowering.IsPair(i))Check(d.Count==2&&d.Request0.Address==ea&&d.Request1.Address==destination&&
                        d.Request0.Access==FullMemoryAccess.Read&&d.Request1.Access==FullStringLowering.PairSecondAccess(i),
                        "R94 source-first independent pair requests include FS/GS after AS32 truncation");
                    else Check(d.Count==0&&f.MemoryAddress==(i.Operation==JitOperation.StringLoad?ea:destination),
                        "R94 single request uses implicit SI or unoverridden DI");
                    f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                    StringSupply(&f,i.Operation==JitOperation.StringLoad||FullStringLowering.IsPair(i)?p:q,
                        i.Operation==JitOperation.StringLoad||FullStringLowering.IsPair(i)?p1:q1,q,q1,ticket);
                    f.Budget=0;FullNativeMemory.ResumeSameIssue(&f);
                    Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&f.Executed==1&&
                        f.MemoryResume==0&&f.FirstPreissued==0&&f.NativeMemoryComputations==1&&
                        f.MemoryFlags==(ticket?FullMemoryFlags.BarrierConsumed:FullMemoryFlags.None)&&
                        new ReadOnlySpan<byte>(m,expected.Length).SequenceEqual(expected),
                        "R94 native data/flags/DF/index/count/IP and one exhausted-budget approved issue");
                    Check(new ReadOnlySpan<byte>(p,4096).SequenceEqual(ep)&&new ReadOnlySpan<byte>(p1,4096).SequenceEqual(ep1)&&
                        new ReadOnlySpan<byte>(q,4096).SequenceEqual(eq)&&new ReadOnlySpan<byte>(q1,4096).SequenceEqual(eq1),
                        "R94 two noncontiguous page pairs preserve every untouched byte");
                }
            }
            CheckStringOverlap(m,p);
            CheckStringIndexWrap(m,p,p1);
            CheckStringPolls(m,p,tlb);
            CheckStringCanonicalFaults(m,p,tlb);
            CheckStringReadyAuthentication(m,p,q);
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(p1);NativeMemory.Free(q);NativeMemory.Free(q1);}
    }

    static void CheckStringOverlap(byte* m,byte* page)
    {
        foreach(int width in new[]{8,16,32,64})
        {
            var i=StringClassify(StringBytes(0xa4,width));using var code=CreateFullBlock([new(i)]);
            for(int delta=1-width/8;delta<width/8;++delta)
            foreach(bool df in new[]{false,true})
            {
                for(int b=0;b<4096;++b)page[b]=(byte)(b*17+3);
                byte[] expected=new ReadOnlySpan<byte>(page,4096).ToArray();
                expected.AsSpan(128,width/8).ToArray().CopyTo(expected,128+delta);
                StringSeed(m,0x100080,(ulong)(0x100080+delta),0xdeadbeef,df);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R94 overlap setup");
                StringSupply(&f,page,null,page,null,false);FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    new ReadOnlySpan<byte>(page,4096).SequenceEqual(expected),
                    "R94 MOVS snapshots one entire element for both overlap directions and identity, independently of DF");
            }
        }
    }

    static void CheckStringPolls(byte* m,byte* page,FullMemoryTlbEntry* tlb)
    {
        foreach(byte op in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        {
            var i=StringClassify(StringBytes(op,64));using var code=CreateFullBlock([new(i)]);
            foreach(string why in new[]{"stop","attention","TF","RF","budget"})
            {
                StringSeed(m,0x100080,0x100100,77,false);
                if(why=="attention")((Blink.Machine*)m)->attention=true;
                if(why=="TF")((Blink.Machine*)m)->flags|=256;
                if(why=="RF")((Blink.Machine*)m)->flags|=65536;
                byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                ulong epoch=11;long issued=0,completed=0;int stop=why=="stop"?1:0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,why=="budget"?0UL:100UL);f.StackMemory=&d;
                var expected=why=="stop"?FullBlockExit.Stop:why=="attention"?FullBlockExit.Attention:
                    why=="budget"?FullBlockExit.Budget:FullBlockExit.Invalidated;
                Check(RunFull(code,&f)==expected&&issued==(why=="budget"?1:0)&&completed==0&&
                    new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
                    "R94 common poll/context/budget boundary precedes all string effects: "+why);
            }
            StringSeed(m,0x100080,0x100100,77,false);new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            ulong ep=11;long iss=1,done=0;int stopped=0;var header=FullHeader(Ip,code.Entry);
            var descriptor=new FullStackMemoryDescriptor{Version=1};var frame=FullFrame(m,&header,&ep,&iss,&done,&stopped,0);frame.StackMemory=&descriptor;
            frame.FirstPreissued=1;
            Check(RunFull(code,&frame)==FullBlockExit.NeedMemory&&iss==1&&done==0&&frame.FirstPreissued==0,
                "R94 demand FirstPreissued consumed once without another lock-xadd or budget rejection");
            frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;StringSupply(&frame,page,null,page,null,false);
            FullNativeMemory.ResumeSameIssue(&frame);stopped=1;
            Check(RunFull(code,&frame)==FullBlockExit.Stop&&iss==1&&done==0&&frame.FirstPreissued==1&&
                ((ulong*)m)[0]==Ip,"R94 stop while mapping request is pending preserves approved issue and IP");
            stopped=0;
            Check(RunFull(code,&frame)==FullBlockExit.Continue&&iss==1&&done==1&&frame.FirstPreissued==0,
                "R94 same approved instruction resumes after stop without duplicate completion");
        }
    }

    static void CheckStringIndexWrap(byte* m,byte* page,byte* next)
    {
        foreach(byte op in new byte[]{0xaa,0xac})
        foreach(int width in new[]{8,16,32,64})
        foreach(int addressWidth in new[]{32,64})
        foreach(bool df in new[]{false,true})
        foreach(ulong raw in new[]{0UL,0xffffffffUL,0xfeedface00000000UL,0xfeedfaceffffffffUL})
        {
            if(addressWidth==64&&raw>0xffffffffUL)continue; // noncanonical forms belong to the fault group
            var i=StringClassify(StringBytes(op,width,addressWidth==32));using var code=CreateFullBlock([new(i)]);
            StringSeed(m,raw,raw,0x1122334455667788,df);
            ulong ep=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&ep,&issued,&completed,&stop);f.StackMemory=&d;
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R94 wrap index request");
            StringSupply(&f,page,next,page,next,true);FullNativeMemory.ResumeSameIssue(&f);
            Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                ((ulong*)(m+(op==0xac?72:80)))[0]==StringNext(raw,addressWidth,width/8,df)&&
                ((ulong*)(m+(op==0xac?80:72)))[0]==raw,
                "R94 index arithmetic wraps at AS32/64, clears high32 only on written indices, and leaves the unused index alone");
        }
    }

    static void CheckStringCanonicalFaults(byte* m,byte* page,FullMemoryTlbEntry* tlb)
    {
        foreach(byte op in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(ulong bad in new[]{0x800000000000UL,0x7ffffffffffdUL,ulong.MaxValue-2})
        foreach(bool source in new[]{true,false})
        {
            var i=StringClassify(StringBytes(op,64));
            if(source&&!FullStringLowering.HasSource(i)||!source&&!FullStringLowering.HasDestination(i))continue;
            using var code=CreateFullBlock([new(i)]);StringSeed(m,source?bad:0x100080,source?0x100100:bad,77,true);
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ulong ep=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&ep,&issued,&completed,&stop);f.StackMemory=&d;
            f.MemoryTlb=tlb;f.MemoryTlbMask=63;new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            StringTlb(&f,0x100000,page,FullMemoryAccess.Read|FullMemoryAccess.Write);
            Check(RunFull(code,&f)==FullBlockExit.GuestFault&&f.FaultTrap==13&&f.FaultCode==128&&f.FaultAddress==0&&
                issued==1&&completed==0&&f.MemoryResume==0&&new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
                "R94 noncanonical/wrapped string range is precise #GP before any data/index/flags effect");
        }
    }

    static void CheckStringReadyAuthentication(byte* m,byte* p,byte* q)
    {
        foreach(byte op in new byte[]{0xa4,0xa6})
        foreach(string corrupt in new[]{"count","site","source","destination","read-access","other-access","width","first-pointer","second-pointer","length","epoch","ticket"})
        {
            var i=StringClassify(StringBytes(op,64));using var code=CreateFullBlock([new(i)]);
            StringSeed(m,0x100fff,0x700fff,77,false);byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ulong ep=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&ep,&issued,&completed,&stop);f.StackMemory=&d;
            Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R94 pair auth setup");
            StringSupply(&f,p,p,q,q,false);
            switch(corrupt)
            {
                case "count":d.Count=1;break;case "site":d.Site=1;break;case "source":++d.Request0.Address;break;
                case "destination":++d.Request1.Address;break;case "read-access":d.Request0.Access=FullMemoryAccess.Write;break;
                case "other-access":d.Request1.Access=op==0xa4?FullMemoryAccess.Read:FullMemoryAccess.Write;break;
                case "width":d.Request1.Width=32;break;case "first-pointer":d.Request0.Host0=null;break;
                case "second-pointer":d.Request1.Host1=null;break;case "length":d.Request0.Length0=2;break;
                case "epoch":--d.Epoch;break;case "ticket":d.Ticket=1;break;
            }
            FullNativeMemory.ResumeSameIssue(&f);
            Check(RunFull(code,&f)==(corrupt=="epoch"?FullBlockExit.NeedMemory:FullBlockExit.Invalidated)&&issued==1&&completed==0&&
                new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),"R94 independent RR/RW Ready validation: "+corrupt);
        }
    }

    static void CheckStringOracles(StringFixture fixture)
    {
        // Actual selected normal GetOp inside BOTH actual fixture.DataScope
        // and its CPU participation permit. SCAS unequal and LODS32 highbits
        // are intentional selected gaps; separate native model tests above
        // establish the desired behavior instead of normalizing their oracle.
        foreach(byte op in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(bool address32 in new[]{false,true})
        foreach(bool df in new[]{false,true})
        {
            byte[] bytes=StringBytes(op,width,address32);var i=StringClassify(bytes);
            byte* m=(byte*)fixture.Machine();ulong source=StringFixture.Base+4095,destination=StringFixture.Base+12287;
            byte* p=(byte*)fixture.Physical(source&~4095UL),p1=(byte*)fixture.Physical((source&~4095UL)+4096);
            byte* q=(byte*)fixture.Physical(destination&~4095UL),q1=(byte*)fixture.Physical((destination&~4095UL)+4096);
            const ulong value=0x12345678;StringWrite(p+4095,p1,1,width,value);StringWrite(q+4095,q1,1,width,value);
            StringSeed(m,source|(address32?0xdeadbeef00000000UL:0),destination|(address32?0xfedcba9800000000UL:0),value,df);
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            nint[] pages=[(nint)p,(nint)p1,(nint)q,(nint)q1];byte[][] original=new byte[4][];
            for(int k=0;k<4;++k)original[k]=new ReadOnlySpan<byte>((void*)pages[k],4096).ToArray();
            fixture.DataScope(0,()=>IntegerReference(fixture.Program,(byte*)fixture.Machine(),bytes));
            byte[] expected=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();byte[][] memory=new byte[4][];
            for(int k=0;k<4;++k)memory[k]=new ReadOnlySpan<byte>((void*)pages[k],4096).ToArray();
            before.CopyTo(new Span<byte>(m,before.Length));for(int k=0;k<4;++k)original[k].CopyTo(new Span<byte>((void*)pages[k],4096));
            using var code=CreateFullBlock([new(i)]);ulong ep=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
            var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&ep,&issued,&completed,&stop);f.StackMemory=&d;
            FullMemoryTlbEntry* tlb=(FullMemoryTlbEntry*)NativeMemory.AllocZeroed(64,64);
            try
            {
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory,"R94 selected successful oracle mapping setup");
                f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                StringSupply(&f,i.Operation==JitOperation.StringLoad||FullStringLowering.IsPair(i)?p:q,
                    i.Operation==JitOperation.StringLoad||FullStringLowering.IsPair(i)?p1:q1,q,q1,false);
                FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue&&issued==1&&completed==1&&
                    new ReadOnlySpan<byte>(m,9).SequenceEqual(expected.AsSpan(0,9))&&
                    new ReadOnlySpan<byte>(m+12,4).SequenceEqual(expected.AsSpan(12,4))&&
                    new ReadOnlySpan<byte>(m+24,128).SequenceEqual(expected.AsSpan(24,128))&&
                    new ReadOnlySpan<byte>(m+160,256).SequenceEqual(expected.AsSpan(160,256))&&
                    new ReadOnlySpan<byte>(m+456,236).SequenceEqual(expected.AsSpan(456,236)),
                    "R94 normal selected oracle where source is correct, including AS32/DF and crossing pages");
                for(int k=0;k<4;++k)Check(new ReadOnlySpan<byte>((void*)pages[k],4096).SequenceEqual(memory[k]),
                    "R94 selected successful oracle exact memory footprint");
            }
            finally{NativeMemory.Free(tlb);}
        }
        // These assertions RECORD the selected source behavior; they are NOT
        // expected native results and must not be used as parity acceptance.
        byte* machine=(byte*)fixture.Machine();const ulong addr=StringFixture.Base+128;
        fixture.DataScope(0,()=>
        {
            byte* page=fixture.Program.LookupAddress2(fixture.Machine(),(long)addr,0,0);page[0]=2;
            StringSeed(machine,addr,addr,1,false);IntegerReference(fixture.Program,machine,new byte[]{0xae});
        });
        Check(((Blink.Machine*)machine)->flags==StringSubFlags((0xa52c0e15u&~0x50500u),2,1,8),
            "R94-S1 selected SCAS is memory minus accumulator; native intentionally computes architectural reverse");
        fixture.DataScope(0,()=>
        {
            byte* page=fixture.Program.LookupAddress2(fixture.Machine(),(long)addr,0,0);((uint*)page)[0]=0x12345678;
            StringSeed(machine,addr,addr,0xaabbccddffffffff,false);IntegerReference(fixture.Program,machine,new byte[]{0xad});
        });
        Check(((ulong*)(machine+24))[0]==0xaabbccdd12345678UL,
            "R94-S2 selected LODS32 retains upper RAX; native independent model requires zero extension");
    }

    static FullStackMemoryDescriptor StringPairRequest(ulong source,ulong destination,int width,bool compare)
        =>new(){Version=1,Count=2,State=FullStackMemoryState.Request,
            Request0=new(){Address=source,Width=(uint)width,Access=FullMemoryAccess.Read},
            Request1=new(){Address=destination,Width=(uint)width,Access=compare?FullMemoryAccess.Read:FullMemoryAccess.Write}};

    // Exact proposed parent seam; deliberately NOT the stack mapper. Mapping
    // only, with real Gate and passive physical lifetime as in the old MMU
    // contracts. The parent must implement this API before building R94 tests.
    static FullMemoryAssistResult FillStringPairMmu(StringFixture fixture,FullBlockFrame* frame,bool compare)
    {
        FullMemoryAssistResult result=default;
        nint pointer=(nint)frame;
        fixture.DataScope(0,()=>
        {
            lock(fixture.Jit.Authority.Gate)
            {
                var f=(FullBlockFrame*)pointer;
                f->ObservedGlobalEpoch=fixture.Jit.Authority.Epoch;
                fixture.Program.ResetTlb(fixture.Machine());
                result=FullNativeMemory.TryFillStringPair(fixture.Program,fixture.Machine(),f,fixture.Jit.Authority,compare);
            }
        });
        return result;
    }

    static void CheckStringOrderedMmu()
    {
        using var fixture=new StringFixture();const ulong source=StringFixture.Base,destination=source+0x42000;
        fixture.Map(source,8192);fixture.Fill(source,8192);fixture.Map(destination,8192);fixture.Fill(destination,8192);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        foreach(int width in new[]{8,16,32,64})
        foreach(bool compare in new[]{false,true})
        {
            var d=StringPairRequest(source+4095,destination+4095,width,compare);var f=StackMemoryMmuRequest(&d,tlb);
            Check(FillStringPairMmu(fixture,&f,compare)==FullMemoryAssistResult.Resolved&&d.State==FullStackMemoryState.Ready&&
                d.Request0.Host0!=null&&d.Request1.Host0!=null&&
                (width==8||d.Request0.Host1!=null&&d.Request1.Host1!=null),
                "R94 parent ordered mapper supports RR/RW widths8/16/32/64 and direct-map collision");
            d=StringPairRequest(source+8192,destination+8192,width,compare);f=StackMemoryMmuRequest(&d,tlb);
            Check(FillStringPairMmu(fixture,&f,compare)==FullMemoryAssistResult.GuestFault&&f.FaultTrap==14&&f.FaultCode==1&&
                f.FaultAddress==source+8192&&d.State!=FullStackMemoryState.Ready,"R94 source fault precedes destination fault");
            d=StringPairRequest(source+4095,destination+8192,width,compare);f=StackMemoryMmuRequest(&d,tlb);
            Check(FillStringPairMmu(fixture,&f,compare)==FullMemoryAssistResult.GuestFault&&f.FaultAddress==destination+8192,
                "R94 complete source validation reaches precise destination fault");
        }
        fixture.DataScope(0, () => Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,
            (long)(destination+4096),4096,1,false)==0,
            "R94 second destination page truly read-only (generic fixture mprotect means PROT_NONE)"));
        foreach(bool compare in new[]{false,true})
        {
            var d=StringPairRequest(source+4095,destination+4095,64,compare);var f=StackMemoryMmuRequest(&d,tlb);
            Check(FillStringPairMmu(fixture,&f,compare)==(compare?FullMemoryAssistResult.Resolved:FullMemoryAssistResult.GuestFault)&&
                (compare||f.FaultCode==2&&f.FaultAddress==destination+4096),
                "R94 CMPS only reads both pages; MOVS needs both destination pages writable");
        }
        var noncanonical=StringPairRequest(source,0x800000000000UL,64,false);var fault=StackMemoryMmuRequest(&noncanonical,tlb);
        Check(FillStringPairMmu(fixture,&fault,false)==FullMemoryAssistResult.GuestFault&&fault.FaultTrap==13&&
            fault.FaultCode==128&&fault.FaultAddress==0,"R94 parent ordered miss preserves string #GP versus #PF");
        var file=fixture.Io.CreateMemoryFile("r94-string-eof",0);Check(file.Succeeded,"R94 EOF memory file");
        Check(fixture.Io.TruncateFile(file.Value,4096).Succeeded,"R94 EOF initial extent");
        const ulong eof=destination+0x20000;fixture.MapShared(file.Value,eof);fixture.Fill(eof);
        Check(fixture.Io.TruncateFile(file.Value,0).Succeeded,"R94 EOF backing withdrawn");
        foreach(bool sourceEof in new[]{false,true})
        {
            var d=StringPairRequest(sourceEof?eof:source,sourceEof?destination:eof,64,false);var f=StackMemoryMmuRequest(&d,tlb);
            Check(FillStringPairMmu(fixture,&f,false)==FullMemoryAssistResult.GuestFault&&f.FaultTrap==14&&
                unchecked((int)f.FaultCode)==-2&&f.FaultAddress==eof&&d.State!=FullStackMemoryState.Ready,
                "R94 either request EOF uses BUS_ADRERR sentinel before any string effect");
        }
    }

    static void CheckStringController()
    {
        foreach(byte op in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        {
            using var f=new StringFixture();const ulong pc=StringFixture.Base,source=pc+8192,destination=pc+0x42000;
            f.Map(pc);f.Fill(pc);f.Map(source,8192);f.Fill(source,8192);f.Map(destination,8192); // lazy destination
            byte[] bytes=StringBytes(op,width);WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            var i=StringClassify(bytes,pc);var m=f.Machine();Write64((byte*)m,72,source+4095);Write64((byte*)m,80,destination+4095);
            Write64((byte*)m,24,0x1122334455667788);Write64((byte*)m,32,0);
            Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
            f.NativePrepared=cap;f.NativeReturned=_=>f.NativePrepared=cap;
            f.Counts(1);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            Check(f.Step(pc)&&f.Issued==1&&f.Completed==1&&m->ip==pc+(uint)bytes.Length&&m->oplen==0&&
                f.Statistics.NativeMemoryInstructions==1&&f.Statistics.SemanticHelperInstructions==0&&
                f.Statistics.SemanticHelperCalls==0&&f.Statistics.InterpreterAttempts==0&&
                ((ulong*)((byte*)m+32))[0]==0&&
                ((ulong*)((byte*)m+72))[0]==source+4095+(FullStringLowering.HasSource(i)?(uint)(width/8):0)&&
                ((ulong*)((byte*)m+80))[0]==destination+4095+(FullStringLowering.HasDestination(i)?(uint)(width/8):0),
                "R94 actual demand compiler/controller selects native strings and one issued/completed operation");
            var state=f.Jit.Store.GetStatistics();Check(state.ActiveManagedLeases==0&&state.ActiveNativeEpochs==0&&
                f.Statistics.ActiveDataBorrows==0&&f.Jit.Authority.PendingAtomicWorker==-1,
                "R94 controller releases every mapping/native/store/ticket capability");
        }
    }

    static void CheckStringRemap()
    {
        using var f=new StringFixture();const ulong source=StringFixture.Base,destination=source+8192;
        f.Map(source);f.Fill(source);f.Map(destination);f.Fill(destination);
        var i=StringClassify([0x48,0xa5]);using var code=CreateFullBlock([new(i)]);var m=f.Machine();
        Write64((byte*)m,72,source+64);Write64((byte*)m,80,destination+64);m->ip=Ip;m->oplen=0;
        var h=FullHeader(Ip,code.Entry);h.ValidatedGlobalEpoch=f.Jit.Authority.CodeEpoch;
        long issued=0,completed=0;int stop=0;FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        new Span<FullMemoryTlbEntry>(tlb,64).Clear();var d=new FullStackMemoryDescriptor{Version=1};
        var frame=FullFrame((byte*)m,&h,f.Jit.Authority.EpochPointer,&issued,&completed,&stop,limit:1,
            codeEpoch:f.Jit.Authority.CodeEpochPointer);frame.StackMemory=&d;frame.MemoryTlb=tlb;frame.MemoryTlbMask=63;
        Check(RunFull(code,&frame)==FullBlockExit.NeedMemory&&FillStringPairMmu(f,&frame,false)==FullMemoryAssistResult.Resolved,
            "R94 real source and destination checked before mapping replacement");
        ulong old=d.Epoch;Check(f.Mutate("MAP_FIXED",source)==0,"R94 actual source remap");f.Fill(source);
        f.DataScope(0,()=>((ulong*)f.Program.LookupAddress2(m,(long)source+64,0,0))[0]=0x8877665544332211);
        frame.ObservedGlobalEpoch=f.Jit.Authority.Epoch;h.ValidatedGlobalEpoch=f.Jit.Authority.CodeEpoch;
        frame.ObservedCodeAuthorityEpoch=h.ValidatedGlobalEpoch;FullNativeMemory.ResumeSameIssue(&frame);
        Check(old!=frame.ObservedGlobalEpoch&&RunFull(code,&frame)==FullBlockExit.NeedMemory&&issued==1&&completed==0&&
            ((ulong*)((byte*)m+72))[0]==source+64&&((ulong*)((byte*)m+80))[0]==destination+64,
            "R94 stale string Ready epoch cannot read or publish old mapped data");
        Check(FillStringPairMmu(f,&frame,false)==FullMemoryAssistResult.Resolved,"R94 fresh mapping request resolves");
        frame.ObservedGlobalEpoch=f.Jit.Authority.Epoch;frame.ObservedCodeAuthorityEpoch=f.Jit.Authority.CodeEpoch;
        h.ValidatedGlobalEpoch=frame.ObservedCodeAuthorityEpoch;FullNativeMemory.ResumeSameIssue(&frame);
        Check(RunFull(code,&frame)==FullBlockExit.Continue&&issued==1&&completed==1&&
            ((ulong*)((byte*)f.Physical(destination)+64))[0]==0x8877665544332211,
            "R94 native MOVS reloads replacement backing and completes the original approved issue once");
    }

    static void CheckStringControllerFaults()
    {
        foreach(byte op in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(bool sourceFault in new[]{false,true})
        {
            byte[] bytes=StringBytes(op,64);var i=StringClassify(bytes);
            if(sourceFault&&!FullStringLowering.HasSource(i)||!sourceFault&&!FullStringLowering.HasDestination(i))continue;
            using var f=new StringFixture();const ulong pc=StringFixture.Base,data=pc+8192,missing=pc+0x40000;
            f.Map(pc);f.Fill(pc);f.Map(data);f.Fill(data);WriteControllerCode(f,pc,bytes);
            var m=f.Machine();Write64((byte*)m,72,sourceFault?missing:data);Write64((byte*)m,80,sourceFault?missing+4096:missing);
            // For destination-only instructions the unused source is irrelevant.
            uint flags=m->flags;ulong si=((ulong*)((byte*)m+72))[0],di=((ulong*)((byte*)m+80))[0],ax=((ulong*)((byte*)m+24))[0];
            f.Counts(1);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            var signal=f.StepSynchronousFault(pc);
            Check(signal.Signal==11&&m->trapno==14&&m->faultaddr==(long)(sourceFault?si:di)&&m->ip==pc&&m->oplen==0&&
                m->flags==flags&&((ulong*)((byte*)m+72))[0]==si&&((ulong*)((byte*)m+80))[0]==di&&
                ((ulong*)((byte*)m+24))[0]==ax&&f.Issued==1&&f.Completed==0&&
                f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "R94 actual string #PF preserves complete faulting iteration and exact instruction accounting");
        }
    }

    static void CheckStringWatchedAlias()
    {
        using var f=new StringFixture();const ulong pc=StringFixture.Base,source=pc+8192,alias=pc+0x20000;
        var file=f.Io.CreateMemoryFile("r94-string-code-alias",0);Check(file.Succeeded,"R94 code alias memory file");
        Check(f.Io.TruncateFile(file.Value,4096).Succeeded,"R94 code alias extent");
        f.MapShared(file.Value,pc);f.Fill(pc);f.MapShared(file.Value,alias);f.Map(source);f.Fill(source);
        WriteControllerCode(f,pc,[0x48,0xa5,0xeb,0xfe]);var m=f.Machine();
        Write64((byte*)m,72,source+128);Write64((byte*)m,80,alias+256);
        f.DataScope(0,()=>((ulong*)f.Program.LookupAddress2(m,(long)source+128,0,0))[0]=0x1122334455667788);
        bool consumed=false;Action<nint> cap=p=>((FullBlockFrame*)p)->NativeLimit=1;
        f.NativePrepared=cap;f.NativeReturned=p=>{consumed|=((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.BarrierConsumed;f.NativePrepared=cap;};
        f.Counts(1);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
        Check(f.Step(pc)&&consumed&&f.Issued==1&&f.Completed==1&&m->ip==pc+2&&
            ((ulong*)((byte*)f.Physical(pc)+256))[0]==0x1122334455667788&&
            f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
            "R94 MOVS to a physical code alias needs final checked exclusive ticket and one native store");
    }

    static void CheckStringAdmission()
    {
        foreach(byte opcode in new byte[]{0xa4,0xa6})
        {
            using var f=new StringFixture();const ulong pc=StringFixture.Base,source=pc+8192,destination=pc+16384;
            f.Map(pc);f.Fill(pc);f.Map(source);f.Fill(source);f.Map(destination);f.Fill(destination);
            WriteControllerCode(f,pc,[0x48,opcode==0xa4?(byte)0xa5:(byte)0xa7,0xeb,0xfe]);var m=f.Machine();
            Write64((byte*)m,72,source+128);Write64((byte*)m,80,destination+128);
            bool reserved=false,ready=false,observed=false;int preissues=0;
            f.Preissued=(_,_)=>++preissues;uint flags=m->flags;
            Action<nint>? prepare=null;
            prepare=p=>
            {
                var frame=(FullBlockFrame*)p;frame->NativeLimit=1;
                if(!reserved){lock(f.Jit.Authority.Gate)Check(f.Jit.Authority.TryReserveNativeTicket(1),"R94 reserve foreign ticket");reserved=true;}
                ready=frame->StackMemory!=null&&frame->StackMemory->State==FullStackMemoryState.Ready;
                f.NativePrepared=prepare;
            };
            f.NativePrepared=prepare;f.NativeReturned=p=>
            {
                if(!ready||observed)return;var frame=(FullBlockFrame*)p;
                Check(frame->Exit==FullBlockExit.NeedMemory&&frame->Executed==0&&f.Completed==0&&m->ip==pc&&
                    m->flags==flags&&((ulong*)((byte*)m+72))[0]==source+128&&((ulong*)((byte*)m+80))[0]==destination+128,
                    "R94 R86-01: denied enrollment discards both RR and RW Ready pointers before any string effect");
                observed=true;f.Jit.Authority.AbortNativeReservation(1);
            };
            f.Counts(1);using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(5),Timeout.InfiniteTimeSpan);
            try
            {
                Check(f.Step(pc)&&observed&&f.Issued==1&&f.Completed==1&&preissues==1&&m->ip==pc+2,
                    "R94 string Ready admission recovers same approved issue after foreign reservation ends");
                var state=f.Jit.Store.GetStatistics();Check(state.ActiveNativeEpochs==0&&state.ActiveManagedLeases==0&&
                    f.Statistics.ActiveDataBorrows==0&&f.Jit.Authority.PendingAtomicWorker==-1,"R94 admission cleanup");
            }
            finally{f.NativePrepared=null;f.NativeReturned=null;f.Jit.Authority.AbortNativeReservation(1);}
        }
    }

    static void CheckStringEmission()
    {
        byte[] staging=new byte[FullBlockEmitter.CodeCapacity],slab=new byte[FullBlockEmitter.ScaffoldCapacity],tiny=new byte[7];
        foreach(bool windows in new[]{false,true})
        {
            FullBlockEmitter.EmitScaffold(slab,out var layout,windows);var scaffold=layout.Resolve((nint)0x10000000);
            foreach(byte op in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
            foreach(int width in new[]{8,16,32,64})
            foreach(bool shared in new[]{false,true})
            {
                byte[] bytes=StringBytes(op,width,true,4);var i=StringClassify(bytes);FullBlockInstruction[] body=[new(i)];
                for(int n=0;n<8;++n)Check(shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):
                    FullBlockEmitter.TryEmit(body,staging,out _,windows),"R94 warm both ABI/scaffold emission paths");
                long before=GC.GetAllocatedBytesForCurrentThread();bool good=true;
                for(int n=0;n<32;++n)
                {
                    good&=FullStringLowering.TryClassify(Ip,bytes,out _,out _);
                    good&=shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):
                        FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                Check(good&&GC.GetAllocatedBytesForCurrentThread()==before,"R94 warmed classifier/emitter allocate zero");
                int length=shared?FullBlockEmitter.Emit(body,staging,in scaffold,windows):FullBlockEmitter.Emit(body,staging,windows);
                Check(length>0&&staging.AsSpan(0,7).SequenceEqual(windows?new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xca}:
                    new byte[]{0xf3,0x0f,0x1e,0xfa,0x49,0x89,0xfa}),"R94 both host ABI entry encodings");
                bool fits=shared?FullBlockEmitter.TryEmit(body,tiny,in scaffold,out int emitted,windows):
                    FullBlockEmitter.TryEmit(body,tiny,out emitted,windows);
                Check(!fits&&emitted==0,"R94 short span has no publishable partial code");
            }
        }
    }
}
