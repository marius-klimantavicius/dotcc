using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using R162Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // SOURCE_READY_UNEXECUTED. One coalesced family entry; existing executable
    // helpers are test fixtures, never a semantic executor in the product.
    static void CheckFullStringFormsR162(byte* machine)
    {
        Check(sizeof(FullBlockFrame)==416 && FullBlockFrame.AbiVersion==0x46420007 &&
            sizeof(FullStackMemoryDescriptor)==128 && sizeof(FullRepState)==256,
            "R162 retains ABI7/frame416/descriptor128/REP256");
        using (var decoder=new R162Fixture(interpreterOracle:true)) R162Grammar(decoder.Program);
        R162OrdinaryBodies(machine);
        using (var common=CreateFullScaffold(out var addresses))
        {
            fullOwnerScaffold=addresses;
            try { R162OrdinaryBodies(machine); }
            finally { fullOwnerScaffold=default; }
        }
        R162IndexBoundaries(machine);
        R162CanonicalBodies(machine);
        R162RepBodies(machine);
        R162As32SegmentFaults(machine);
        R162RepCanonicalRanges(machine);
        R162RepLaterCanonicalFault(machine);
        R162OrderedMmu();
        R162SpecializationStore();
        R162Transport();
        R162ControllerFaults();
        R162ControllerContinuation();
        ResetFull(machine);
    }

    readonly record struct R162Form(byte Op, int Width, bool As32, byte Segment, byte Repeat)
    {
        internal bool Source => Op is 0xa4 or 0xac or 0xa6;
        internal bool Destination => Op!=0xac;
        internal bool Compares => Op is 0xa6 or 0xae;
        internal int SourceSegment => !Source ? -1 : Segment==0x36 ? 2 : Segment==0x64 ? 4 : Segment==0x65 ? 5 : -1;
        internal byte[] Bytes => Repeat==0 ? [..Prefixes(),(byte)(Op+(Width==8?0:1))] :
            [Repeat,..Prefixes(),(byte)(Op+(Width==8?0:1))];
        byte[] Prefixes()
        {
            var b=new List<byte>(); if(Segment!=0)b.Add(Segment);
            if(Width==16)b.Add(0x66);if(As32)b.Add(0x67);if(Width==64)b.Add(0x48);
            return b.ToArray();
        }
    }

    static IEnumerable<R162Form> R162Forms(bool repeated)
    {
        foreach(byte op in new byte[]{0xa4,0xaa,0xac,0xa6,0xae})
        foreach(int width in new[]{8,16,32,64})
        foreach(bool as32 in new[]{false,true})
        foreach(byte segment in new byte[]{0,0x26,0x2e,0x36,0x3e,0x64,0x65})
        foreach(byte repeat in repeated ? new byte[]{0xf3,0xf2} : new byte[]{0})
        {
            if(repeat==0xf2 && op is not (0xa6 or 0xae))continue;
            yield return new(op,width,as32,segment,repeat);
        }
    }

    static void R162Decode(Blink p, byte[] b)
    {
        Blink.XedDecodedInst d=default;
        fixed(byte* q=b)Check(p.DecodeInstruction(&d,q,(ulong)b.Length,Blink.XED_MODE_LONG)==0 && d.length==b.Length,
            "R162 authoritative long-mode decode exact length "+Convert.ToHexString(b));
    }

    static void R162Grammar(Blink p)
    {
        foreach(bool repeated in new[]{false,true})
        foreach(var row in R162Forms(repeated))
        {
            byte[] b=row.Bytes,original=b.ToArray();R162Decode(p,b);
            if(!repeated)
            {
                Check(FullTranslationJit.R162ClassifyString(Ip,b,out var i) && FullBlockEmitter.ValidateString(i) &&
                    i.Width==row.Width && i.SourceWidth==row.Width && i.Address.AddressWidth==(row.As32?32:64) &&
                    i.Address.Segment==row.SourceSegment && i.Ip==Ip && i.Length==b.Length,
                    "R162 complete ordinary family width/address/source segment table");
            }
            else
            {
                Check(FullCanonicalEncoding.TryClassifyRep(Ip,b,out var i,out _) && FullRepLowering.IsCanonical(in i) &&
                    i.SourceSegment==row.SourceSegment && i.Width==row.Width && i.AddressWidth==(row.As32?32:64) &&
                    i.Repeat==(!row.Compares?FullRepRepeat.Count:row.Repeat==0xf3?FullRepRepeat.WhileEqual:FullRepRepeat.WhileNotEqual) &&
                    i.Length==b.Length && i.Ip==Ip,"R162 documented REP/REPE/REPZ/REPNE/REPNZ aliases");
            }
            Check(b.SequenceEqual(original),"R162 classification retains original Plan bytes");
        }
        foreach(byte op in new byte[]{0xa4,0xa6,0xaa,0xac,0xae})
        foreach(byte repeat in new byte[]{0,0xf3,0xf2})
        {
            if(repeat==0xf2 && op is not (0xa6 or 0xae))continue;
            byte[] groups=repeat==0 ? [0x36,0x66,0x67] : [0x36,0x66,0x67,repeat];
            foreach(var prefix in RepPrefixPermutations(groups))
            {
                byte[] b=[..prefix,0x4f,(byte)(op+1)];R162Decode(p,b);
                if(repeat==0)
                    Check(FullTranslationJit.R162ClassifyString(Ip,b,out var i) && i.Width==64 &&
                        i.Address.Segment==(op is 0xa4 or 0xa6 or 0xac?2:-1),"R162 ordered groups and all ignored REX fields");
                else
                    Check(FullCanonicalEncoding.TryClassifyRep(Ip,b,out var i,out _) && i.Width==64 &&
                        i.SourceSegment==(op is 0xa4 or 0xa6 or 0xac?2:-1),"R162 REP all legacy group permutations");
            }
            foreach(byte[] b in new byte[][] {
                [0x36,0x36,0x67,0x67,0x66,0x66,..(repeat==0?Array.Empty<byte>():new byte[]{repeat,repeat}),0x4f,0x48,(byte)(op+1)],
                [0x4f,0x36,0x67,0x66,..(repeat==0?Array.Empty<byte>():new byte[]{repeat}),(byte)(op+1)] })
            {
                R162Decode(p,b);int width=b[^2]==0x48?64:16;
                if(repeat==0)
                    Check(FullTranslationJit.R162ClassifyString(Ip,b,out var i) && i.Width==width && i.Length==b.Length,
                        "R162 redundant prefixes/REX cancellation retain original instruction length");
                else
                    Check(FullCanonicalEncoding.TryClassifyRep(Ip,b,out var i,out _) && i.Width==width && i.Length==b.Length,
                        "R162 REP canonical wrapper retains original instruction length");
            }
        }
        foreach(byte op in new byte[]{0xa4,0xaa,0xac})
        {
            Check(!FullCanonicalEncoding.TryClassifyRep(Ip,[0xf2,op],out _,out var reason) && reason?.Contains("undocumented F2")==true,
                "R162 selected F2 count permissiveness remains a named undocumented exclusion");
        }
        foreach(byte[] b in new byte[][]{[0xf2,0xf3,0xa6],[0x36,0x64,0xf3,0xa4],[0xf0,0xf3,0xa4],[0xf3,0x6c],[0xf3,0x6e]})
            Check(!FullCanonicalEncoding.TryClassifyRep(Ip,b,out _,out _),"R162 conflicting group1/segments, LOCK and port families remain excluded");
        var good=new R162Form(0xa4,64,false,0x36,0).Bytes;
        Check(FullTranslationJit.R162ClassifyString(Ip,good,out var ordinary),"R162 SS validation prerequisite");
        foreach(int segment in new[]{0,1,3,6})
        {
            bool rejected=false;
            try { FullBlockEmitter.ValidateString(ordinary with{Address=ordinary.Address with{Segment=segment}}); }
            catch(ArgumentException){rejected=true;}
            Check(rejected,"R162 canonical validators reject unsupported explicit segment identities");
        }
    }

    static JitInstruction R162Ordinary(R162Form row)
    {
        Check(FullTranslationJit.R162ClassifyString(Ip,row.Bytes,out var i) && FullBlockEmitter.ValidateString(i),"R162 ordinary classify");
        return i;
    }

    static FullRepInstruction R162Rep(R162Form row)
    {
        Check(FullCanonicalEncoding.TryClassifyRep(Ip,row.Bytes,out var i,out var why),why??"R162 REP classify");return i;
    }

    static void R162OrdinaryBodies(byte* m)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),p1=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096),q1=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(var row in R162Forms(false).Where(x=>x.Segment is 0x36 or 0x64 or 0x65))
            foreach(bool df in new[]{false,true})
            foreach(bool ticket in new[]{false,true})
            {
                var i=R162Ordinary(row);using var code=CreateFullBlock([new(i)]);
                int n=row.Width/8,offset=4095;
                ulong source=0x100000UL+(uint)offset,destination=0x702000UL+(uint)offset;
                if(row.As32){source|=0xdeadbeef00000000UL;destination|=0xcafef00d00000000UL;}
                const ulong value=0xfedcba9876543281UL,other=0x0123456789abce7fUL;
                new Span<byte>(p,4096).Fill(0xcc);new Span<byte>(p1,4096).Fill(0xcc);
                new Span<byte>(q,4096).Fill(0xaa);new Span<byte>(q1,4096).Fill(0xaa);
                StringWrite(p+offset,p1,1,row.Width,value);StringWrite(q+offset,q1,1,row.Width,other);
                StringSeed(m,source,destination,value,df);
                byte[] expected=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(0,8),Ip+(uint)i.Length);expected[8]=0;
                if(row.Source)BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(72,8),StringNext(source,row.As32?32:64,n,df));
                if(row.Destination)BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(80,8),StringNext(destination,row.As32?32:64,n,df));
                if(row.Op==0xac)BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24,8),row.Width==32?unchecked((uint)value):value);
                if(row.Compares)BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(12,4),StringSubFlags(((Blink.Machine*)m)->flags,value,other,row.Width));
                byte[] wantedQ=new ReadOnlySpan<byte>(q,4096).ToArray(),wantedQ1=new ReadOnlySpan<byte>(q1,4096).ToArray();
                if(row.Op is 0xa4 or 0xaa)for(int k=0;k<n;++k)
                { if(k==0)wantedQ[offset]=unchecked((byte)value);else wantedQ1[k-1]=(byte)(value>>(8*k)); }
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                f.MemoryTlb=tlb;f.MemoryTlbMask=63;new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                f.FsBase=0x300000;f.GsBase=0x500000;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && issued==1 && completed==0 &&
                    RepRaw(m,72)==source && RepRaw(m,80)==destination,"R162 full element maps precede all index effects");
                ulong baseAddress=row.SourceSegment==4?f.FsBase:row.SourceSegment==5?f.GsBase:0;
                ulong ea=(row.As32?(uint)source:source)+baseAddress;
                Check(row.Op is 0xa4 or 0xa6 ? d.Request0.Address==ea && d.Request1.Address==(row.As32?(uint)destination:destination) :
                    f.MemoryAddress==(row.Op==0xac?ea:(row.As32?(uint)destination:destination)),
                    "R162 AS32 truncates offset before FS/GS, ES destination ignores source override");
                StringSupply(&f,row.Source?p:q,row.Source?p1:q1,q,q1,ticket);
                f.Budget=0;FullNativeMemory.ResumeSameIssue(&f);
                Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 && f.Executed==1 &&
                    new ReadOnlySpan<byte>(m,expected.Length).SequenceEqual(expected) &&
                    new ReadOnlySpan<byte>(q,4096).SequenceEqual(wantedQ) && new ReadOnlySpan<byte>(q1,4096).SequenceEqual(wantedQ1),
                    "R162 generated split read/write/compare/LODS, DF and same-issue retry are precise");
            }
        }
        finally { NativeMemory.Free(p);NativeMemory.Free(p1);NativeMemory.Free(q);NativeMemory.Free(q1); }
    }

    static void R162IndexBoundaries(byte* m)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(var row in R162Forms(false).Where(x=>x.As32 && x.Segment is 0x36 or 0x64 or 0x65))
            foreach(bool source in new[]{false,true})
            foreach(bool df in new[]{false,true})
            {
                if(source&&!row.Source || !source&&!row.Destination)continue;
                var i=R162Ordinary(row);using var code=CreateFullBlock([new(i)]);int n=row.Width/8;
                uint edge=df?0:uint.MaxValue-(uint)n+1;
                ulong si=(source?edge:0x100080u)|0xdeadbeef00000000UL;
                ulong di=(source?0x702080u:edge)|0xcafef00d00000000UL;
                ulong initialAx=row.Compares?5UL:0xaabbccdd00000005UL;
                StringSeed(m,si,di,initialAx,df);uint flags=((Blink.Machine*)m)->flags;
                RepPutElement(p+(int)(si&4095),row.Width,5);RepPutElement(q+(int)(di&4095),row.Width,2);
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                f.MemoryTlb=tlb;f.MemoryTlbMask=63;new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                f.FsBase=0x300000;f.GsBase=0x500000;
                Check(RunFull(code,&f)==FullBlockExit.NeedMemory && RepRaw(m,72)==si && RepRaw(m,80)==di,
                    "R162 AS32 boundary cannot update indices before complete mapping");
                StringSupply(&f,row.Source?p:q,null,q,null,false);FullNativeMemory.ResumeSameIssue(&f);
                ulong expectedAx=row.Op!=0xac?initialAx:row.Width is 32 or 64?5UL:
                    (0xaabbccdd00000005UL&~StringMask(row.Width))|5UL;
                Check(RunFull(code,&f)==FullBlockExit.Continue && issued==1 && completed==1 &&
                    RepRaw(m,72)==(row.Source?StringNext(si,32,n,df):si) &&
                    RepRaw(m,80)==(row.Destination?StringNext(di,32,n,df):di) && RepRaw(m,24)==expectedAx &&
                    ((Blink.Machine*)m)->flags==(row.Compares?StringSubFlags(flags,5,2,row.Width):flags),
                    "R162 SS/FS/GS AS32 DF increment/decrement wraps only the successfully written indices");
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static ulong RepRaw(byte* m,int at)=>BinaryPrimitives.ReadUInt64LittleEndian(new(m+at,8));
    static IEnumerable<ulong> R162BadRanges(int width)
    {
        yield return 0x800000000000UL; // noncanonical start
        if(width>8)
        {
            yield return 0x800000000000UL-(ulong)(width/8)+1; // canonical start, noncanonical end
            yield return ulong.MaxValue-(ulong)(width/8)+2; // canonical start, wrapped end
        }
    }

    static void R162CanonicalBodies(byte* m)
    {
        byte* page=(byte*)NativeMemory.Alloc(4096);FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(var row in R162Forms(false).Where(x=>!x.As32 && x.Segment is 0 or 0x36 or 0x64 or 0x65))
            foreach(bool source in new[]{false,true})
            foreach(ulong bad in R162BadRanges(row.Width))
            {
                if(source&&!row.Source || !source&&!row.Destination)continue;
                var i=R162Ordinary(row);using var code=CreateFullBlock([new(i)]);
                StringSeed(m,source?bad:0x100080,source?0x702080:bad,77,true);
                byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                ulong epoch=11;long issued=0,completed=0;int stop=0;var h=FullHeader(Ip,code.Entry);
                var d=new FullStackMemoryDescriptor{Version=1};var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                f.MemoryTlb=tlb;f.MemoryTlbMask=63;new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                StringTlb(&f,0x100000,page,FullMemoryAccess.Read);StringTlb(&f,0x702000,page,FullMemoryAccess.Read|FullMemoryAccess.Write);
                Check(RunFull(code,&f)==FullBlockExit.GuestFault && f.FaultTrap==(source&&row.SourceSegment==2?12u:13u) &&
                    f.FaultCode==128 && f.FaultAddress==0 && f.MemoryResume==0 && issued==1 && completed==0 &&
                    new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
                    "R162 native source #SS12 versus source/destination #GP13 start/end/carry before effects");
            }
        }
        finally{NativeMemory.Free(page);}
    }

    static void R162RepBodies(byte* m)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(var row in R162Forms(true).Where(x=>x.Segment is 0x36 or 0x64 or 0x65))
            foreach(bool df in new[]{false,true})
            foreach(int count in new[]{0,1,3})
            {
                var i=R162Rep(row);using var kernel=RepKernel(i,out _);ResetFull(m);
                int n=row.Width/8,offset=df?256:128;
                ulong si=count==0?0x800000000000UL:0x100000UL+(uint)offset;
                ulong di=count==0?ulong.MaxValue:0x702000UL+(uint)offset;
                if(row.As32){si|=0xdeadbeef00000000UL;di|=0xcafef00d00000000UL;}
                ulong cx=(uint)count|(row.As32?0x1234567800000000UL:0);
                ulong initialAx=row.Compares?5UL:0xaabbccdd00000005UL;
                Write64(m,72,si);Write64(m,80,di);Write64(m,32,cx);Write64(m,24,initialAx);
                uint flags=(0xa52c0e15u&~0x50540u)|(df?1024u:0)|(row.Repeat==0xf2?64u:0);((Blink.Machine*)m)->flags=flags;
                new Span<byte>(p,4096).Fill(0xcc);new Span<byte>(q,4096).Fill(0xaa);
                for(int k=0;k<count;++k)
                {int at=offset+(df?-k:k)*n;RepPutElement(p+at,row.Width,5);RepPutElement(q+at,row.Width,row.Compares?(row.Repeat==0xf2?2UL:5UL):9UL);}
                ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,kernel.Entry);h.Live=0;
                var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);
                var state=RepActive(i,m,epoch);state.SourceBase=row.SourceSegment==4?0x300000UL:row.SourceSegment==5?0x500000UL:0;
                var d=new FullStackMemoryDescriptor{Version=1};f.StackMemory=&d;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                if(count!=0)
                {StringTlb(&f,0x100000+state.SourceBase,p,FullMemoryAccess.Read);StringTlb(&f,0x702000,q,FullMemoryAccess.Read|FullMemoryAccess.Write);}
                Check(RunRep(kernel.Entry,&f,&state)==FullBlockExit.Continue && state.CompletedIterations==(ulong)count &&
                    state.QuantumLimit==0 && issued==1 && completed==1 && f.Executed==1 && (count!=0 || d.Count==0),
                    "R162 all documented REP aliases use unbounded existing loop, count0 performs no access");
                ulong delta=(ulong)(count*n),mask=row.As32?uint.MaxValue:ulong.MaxValue;
                ulong wantedSi=count==0||!row.Source?si:unchecked(si+(df?0UL-delta:delta))&mask;
                ulong wantedDi=count==0||!row.Destination?di:unchecked(di+(df?0UL-delta:delta))&mask;
                ulong ax=row.Op==0xac&&count!=0 ? row.Width is 32 or 64?5UL:(0xaabbccdd00000005UL&~StringMask(row.Width))|5 : initialAx;
                uint wantedFlags=row.Compares&&count!=0?StringSubFlags(flags,5,row.Repeat==0xf2?2UL:5UL,row.Width):flags;
                Check(RepRaw(m,72)==wantedSi && RepRaw(m,80)==wantedDi && RepRaw(m,32)==(count==0?cx:0) &&
                    RepRaw(m,24)==ax && ((Blink.Machine*)m)->flags==wantedFlags,
                    "R162 native REP AS32/DF/RCX/partial-register and comparison flags ordering");
                if(row.Op is 0xa4 or 0xaa)for(int k=0;k<count;++k)
                {
                    int at=offset+(df?-k:k)*n;ulong wanted=row.Op==0xa4?5UL:0xaabbccdd00000005UL;
                    for(int t=0;t<n;++t)Check(q[at+t]==(byte)(wanted>>(8*t)),"R162 REP emits each actual store");
                }
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static void R162As32SegmentFaults(byte* m)
    {
        foreach(byte op in new byte[]{0xa4,0xac,0xa6})
        foreach(int width in new[]{8,16,32,64})
        foreach(byte segment in new byte[]{0x64,0x65})
        foreach(byte repeat in new byte[]{0,0xf3})
        foreach(ulong bad in R162BadRanges(width))
        foreach(int count in repeat==0?new[]{1}:new[]{0,2})
        {
            var row=new R162Form(op,width,true,segment,repeat);
            StringSeed(m,0xdeadbeef00000080UL,0xcafef00d00702080UL,77,false);
            Write64(m,32,(uint)count|0x1234567800000000UL);
            byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
            ulong epoch=11;long issued=repeat==0?0:1,completed=0;int stop=0;
            var d=new FullStackMemoryDescriptor{Version=1};ulong sourceBase=unchecked(bad-128);
            if(repeat==0)
            {
                var i=R162Ordinary(row);using var code=CreateFullBlock([new(i)]);var h=FullHeader(Ip,code.Entry);
                var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop);f.StackMemory=&d;
                if(segment==0x64)f.FsBase=sourceBase;else f.GsBase=sourceBase;
                Check(RunFull(code,&f)==FullBlockExit.GuestFault && f.FaultTrap==13 && f.FaultCode==128 &&
                    f.FaultAddress==0 && f.MemoryResume==0 && issued==1 && completed==0 &&
                    new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
                    "R162 AS32 offset truncation precedes FS/GS canonical start/end/wrap guard");
            }
            else
            {
                var i=R162Rep(row);using var kernel=RepKernel(i,out _);var h=FullHeader(Ip,kernel.Entry);
                var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);f.StackMemory=&d;
                var state=RepActive(i,m,epoch);state.SourceBase=sourceBase;
                var result=RunRep(kernel.Entry,&f,&state);
                if(count==0)
                    Check(result==FullBlockExit.Continue && issued==1 && completed==1 && state.CompletedIterations==0 &&
                        RepRaw(m,72)==0xdeadbeef00000080UL && RepRaw(m,32)==0x1234567800000000UL && d.Count==0,
                        "R162 REP count0 does not touch a noncanonical FS/GS source or truncate indices/count");
                else
                    Check(result==FullBlockExit.GuestFault && f.FaultTrap==13 && f.FaultCode==128 && f.FaultAddress==0 &&
                        issued==1 && completed==0 && state.CompletedIterations==0 &&
                        new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
                        "R162 issued REP latched FS/GS base uses the AS32 range guard before maps/effects");
            }
        }
    }

    static void R162RepCanonicalRanges(byte* m)
    {
        byte* page=(byte*)NativeMemory.Alloc(4096);FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(var row in R162Forms(true).Where(x=>!x.As32 && x.Segment==0x36))
            foreach(bool source in new[]{false,true})
            foreach(ulong bad in R162BadRanges(row.Width))
            {
                if(source&&!row.Source || !source&&!row.Destination)continue;
                var i=R162Rep(row);using var kernel=RepKernel(i,out _);ResetFull(m);
                Write64(m,72,source?bad:0x100080);Write64(m,80,source?0x702080:bad);
                Write64(m,32,2);Write64(m,24,77);((Blink.Machine*)m)->flags&=~0x50500u;
                byte[] before=new ReadOnlySpan<byte>(m,sizeof(Blink.Machine)).ToArray();
                ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,kernel.Entry);
                var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);var state=RepActive(i,m,epoch);
                var d=new FullStackMemoryDescriptor{Version=1};f.StackMemory=&d;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                StringTlb(&f,0x100000,page,FullMemoryAccess.Read);StringTlb(&f,0x702000,page,FullMemoryAccess.Read|FullMemoryAccess.Write);
                Check(RunRep(kernel.Entry,&f,&state)==FullBlockExit.GuestFault && f.FaultTrap==(source?12u:13u) &&
                    f.FaultCode==128 && f.FaultAddress==0 && f.MemoryResume==0 && state.CompletedIterations==0 &&
                    issued==1 && completed==0 && new ReadOnlySpan<byte>(m,before.Length).SequenceEqual(before),
                    "R162 REP start/end/carry fault before first iteration has no partial effects");
            }
        }
        finally{NativeMemory.Free(page);}
    }

    static void R162RepLaterCanonicalFault(byte* m)
    {
        byte* p=(byte*)NativeMemory.Alloc(4096),q=(byte*)NativeMemory.Alloc(4096);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        try
        {
            foreach(var row in R162Forms(true).Where(x=>!x.As32 && x.Segment==0x36))
            foreach(bool source in new[]{false,true})
            foreach(bool df in new[]{false,true})
            {
                if(source&&!row.Source || !source&&!row.Destination)continue;
                var i=R162Rep(row);using var kernel=RepKernel(i,out _);ResetFull(m);int n=row.Width/8;
                ulong edge=df?0xffff800000000000UL:0x800000000000UL-(uint)n;
                ulong si=source?edge:0x101080,di=source?0x702080:edge;
                Write64(m,72,si);Write64(m,80,di);Write64(m,32,2);Write64(m,24,5);
                uint flags=(0xa52c0e15u&~0x50540u)|(df?1024u:0)|(row.Repeat==0xf2?64u:0);((Blink.Machine*)m)->flags=flags;
                new Span<byte>(p,4096).Fill(0xcc);new Span<byte>(q,4096).Fill(0xaa);
                int srcAt=(int)(si&4095),dstAt=(int)(di&4095);
                RepPutElement(p+srcAt,row.Width,5);RepPutElement(q+dstAt,row.Width,row.Compares?(row.Repeat==0xf2?2UL:5UL):9UL);
                byte[] qBefore=new ReadOnlySpan<byte>(q,4096).ToArray();
                ulong epoch=11;long issued=1,completed=0;int stop=0;var h=FullHeader(Ip,kernel.Entry);
                var f=FullFrame(m,&h,&epoch,&issued,&completed,&stop,budget:0,limit:1);var state=RepActive(i,m,epoch);
                var d=new FullStackMemoryDescriptor{Version=1};f.StackMemory=&d;f.MemoryTlb=tlb;f.MemoryTlbMask=63;
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                StringTlb(&f,si&~4095UL,p,FullMemoryAccess.Read);StringTlb(&f,di&~4095UL,q,FullMemoryAccess.Read|FullMemoryAccess.Write);
                Check(RunRep(kernel.Entry,&f,&state)==FullBlockExit.GuestFault && f.FaultTrap==(source?12u:13u) &&
                    f.FaultCode==128 && f.FaultAddress==0 && f.MemoryResume==0 && state.Phase==FullRepPhase.FaultFinal &&
                    state.CompletedIterations==1 && state.RequestGeneration==2 && issued==1 && completed==0 && f.Executed==0 &&
                    RepRaw(m,32)==1 && RepRaw(m,72)==(row.Source?unchecked(si+(df?0UL-(uint)n:(uint)n)):si) &&
                    RepRaw(m,80)==(row.Destination?unchecked(di+(df?0UL-(uint)n:(uint)n)):di) &&
                    ((Blink.Machine*)m)->flags==flags && state.LastFlags==flags,
                    "R162 second iteration #SS12/#GP13 retains first effects, restores original compare flags, no reissue");
                if(row.Op is 0xa4 or 0xaa)for(int k=0;k<n;++k)qBefore[dstAt+k]=(byte)(5UL>>(8*k));
                Check(new ReadOnlySpan<byte>(q,4096).SequenceEqual(qBefore),"R162 no partial later store or destination corruption");
            }
        }
        finally{NativeMemory.Free(p);NativeMemory.Free(q);}
    }

    static void R162OrderedMmu()
    {
        using var fixture=new R162Fixture();const ulong src=R162Fixture.Base,dst=src+0x42000;
        fixture.Map(src);fixture.Fill(src);fixture.Map(dst);fixture.Fill(dst);
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach(bool compare in new[]{false,true})
        foreach(int width in new[]{8,16,32,64})
        foreach(ulong bad in R162BadRanges(width))
        foreach(bool source in new[]{false,true})
        foreach(bool inspect in new[]{false,true})
        {
            var d=StringPairRequest(source?bad:src,source?dst:bad,width,compare);
            var f=StackMemoryMmuRequest(&d,tlb);nint address=(nint)(&f);FullMemoryAssistResult result=default;
            fixture.DataScope(0,()=>
            {
                lock(fixture.Jit.Authority.Gate)
                {
                    var frame=(FullBlockFrame*)address;frame->ObservedGlobalEpoch=fixture.Jit.Authority.Epoch;
                    fixture.Program.ResetTlb(fixture.Machine());
                    result=inspect?FullNativeMemory.InspectStringPairTicket(fixture.Program,fixture.Machine(),frame,fixture.Jit.Authority,compare,out _,out _,2):
                        FullNativeMemory.TryFillStringPair(fixture.Program,fixture.Machine(),frame,fixture.Jit.Authority,compare,2);
                }
            });
            Check(result==FullMemoryAssistResult.GuestFault && f.FaultTrap==(source?12u:13u) && f.FaultCode==128 &&
                f.FaultAddress==0 && f.MemoryResume==0 && d.State==FullStackMemoryState.Request && d.Ticket==0,
                "R162 ordered mapper and promoted-ticket reinspection retain source SS/destination GP range rules");
        }
        // Source paging wins over a bad destination range after a source TLB miss.
        var ordered=StringPairRequest(src+0x80000,0x800000000000UL,64,false);
        var fault=StackMemoryMmuRequest(&ordered,tlb);nint faultAddress=(nint)(&fault);FullMemoryAssistResult outcome=default;
        fixture.DataScope(0,()=>
        {
            lock(fixture.Jit.Authority.Gate)
                outcome=FullNativeMemory.TryFillStringPair(fixture.Program,fixture.Machine(),(FullBlockFrame*)faultAddress,fixture.Jit.Authority,false,2);
        });
        Check(outcome==FullMemoryAssistResult.GuestFault && fault.FaultTrap==14 && fault.FaultAddress==src+0x80000,
            "R162 source #PF precedes destination #GP and no pair effects publish");
    }

    static void R162SpecializationStore()
    {
        Check(FullRepKernelStore.MaximumSpecializations==152,"R162 finite REP domain includes 32 source SS specializations");
        using var io=new SemanticTestIo();
        using var authority=new FullCodeAuthority(io.Value,4096,2,4L<<20);
        using var store=new RetainedTranslationStore(RetainedOptions() with{PayloadBytes=0});
        using var pool=new FullRepKernelStore(authority,store,2,allowUnverifiedWindows:true);
        byte[] staging=new byte[FullBlockEmitter.RepKernelCapacity];
        lock(authority.Gate)
        {
            int count=0;
            foreach(var op in Enum.GetValues<FullRepOperation>())
            foreach(int width in new[]{8,16,32,64})
            foreach(int address in new[]{32,64})
            foreach(int segment in new[]{-1,2,4,5})
            foreach(var repeat in new[]{FullRepRepeat.Count,FullRepRepeat.WhileEqual,FullRepRepeat.WhileNotEqual})
            {
                var i=new FullRepInstruction(Ip,2,op,width,address,segment,repeat);
                if(!FullRepLowering.IsCanonical(in i))continue;
                Check(pool.TryBind(in i,staging,out var binding,out var failure),"R162 complete finite REP store binding: "+failure);
                var alias=i with{Ip=Ip+4096,Length=15};long bytes=pool.EmittedBytes;
                Check(pool.TryBind(in alias,staging,out var reused,out _) && reused==binding && pool.EmittedBytes==bytes,
                    "R162 PC/ordered-prefix length aliases retain one owner specialization");
                ++count;
            }
            Check(count==152 && pool.Count==152 && pool.ReservedBytes==store.ExternalReservedBytes &&
                pool.ReservedBytes<=152L*FullBlockEmitter.RepKernelCapacity && store.ChargedBytes<=RetainedOptions().BudgetBytes,
                "R162 all 152 specializations fit the existing packed-code/quota formula without slot reuse");
            pool.AssertQuiescent();
        }
    }

    static void R162Transport()
    {
        foreach(var row in R162Forms(true))
        foreach(uint trap in new uint[]{0,12,13,14,16})
        foreach(uint code in new uint[]{1,128})
        foreach(ulong address in new ulong[]{0,1})
        {
            var i=R162Rep(row);var f=new FullBlockFrame{FaultTrap=trap,FaultCode=code,FaultAddress=address};
            bool wanted=trap==14 || code==128 && address==0 &&
                (trap==12 && row.SourceSegment==2 || trap==13 && (row.SourceSegment!=2 || row.Destination));
            Check(ThreadedGuestExecution.R162RepFault(i,in f)==wanted,"R162 same-issued REP typed CPU12/13/14 transport rejects forged fault domains");
        }
    }

    static void R162ControllerFaults()
    {
        foreach(var row in R162Forms(false).Concat(R162Forms(true)).Where(x=>!x.As32 && x.Segment==0x36))
        foreach(bool source in new[]{false,true})
        {
            if(source&&!row.Source || !source&&!row.Destination)continue;
            using var f=new R162Fixture();const ulong pc=R162Fixture.Base,data=pc+8192;
            f.Map(pc);f.Fill(pc);f.Map(data);f.Fill(data);byte[] bytes=row.Bytes;WriteControllerCode(f,pc,bytes);
            ulong si=source?0x800000000000UL:data+128,di=source?data+256:0x800000000000UL;
            uint flags=f.Machine()->flags&~0x50500u;RepSeed(f,si,di,0xaabbccdd12345678UL,3,flags);f.Counts(1);
            uint returnedTrap=0,returnedCode=0;ulong returnedAddress=1;
            f.NativeReturned=pointer=>
            {
                var frame=(FullBlockFrame*)pointer;
                if(frame->Exit==FullBlockExit.GuestFault){returnedTrap=frame->FaultTrap;returnedCode=frame->FaultCode;returnedAddress=frame->FaultAddress;}
            };
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(8),Timeout.InfiniteTimeSpan);
            try
            {
                var signal=f.StepSynchronousFault(pc);
                // An ordinary pair must resolve valid source paging before
                // rejecting the destination. With a cold source TLB its mapper
                // reports the destination range fault directly; REP instead
                // returns through its native fault-finalization entry.
                bool mapperFault=row.Repeat==0 && !source && row.Source;
                bool exactTransport=mapperFault ? returnedTrap==0 && f.Statistics.NativeMemoryAssists>0 :
                    returnedTrap==(source?12u:13u) && returnedCode==128 && returnedAddress==0;
                Check(signal.Signal==(source?7:11) && f.Machine()->trapno==(source?12:13) &&
                    exactTransport && f.Machine()->faultaddr==0 &&
                    f.Machine()->ip==pc && f.Machine()->oplen==0 && f.Machine()->flags==flags &&
                    RepRegister(f,72)==si && RepRegister(f,80)==di && RepRegister(f,32)==3 &&
                    RepRegister(f,24)==0xaabbccdd12345678UL && f.Issued==1 && f.Completed==0,
                    $"R162 bytes={Convert.ToHexString(bytes)} source={source} mapper={mapperFault} signal={signal.Signal} trap={f.Machine()->trapno} returned={returnedTrap}/{returnedCode}/{returnedAddress:x} ip={f.Machine()->ip:x} flags={f.Machine()->flags:x}/{flags:x} issued={f.Issued} completed={f.Completed}: exact CPU12/13 Linux transport and uncommitted iteration");
                RepClean(f);
            }
            finally{f.NativeReturned=null;}
        }
    }

    static void R162ControllerContinuation()
    {
        foreach(byte op in new byte[]{0xa4,0xac,0xa6})
        foreach(byte repeat in op==0xa6?new byte[]{0xf3,0xf2}:new byte[]{0xf3})
        {
            using var f=new R162Fixture();const ulong pc=R162Fixture.Base,src=pc+0x10000,dst=pc+0x20000;
            f.Map(pc);f.Fill(pc);f.Map(src,8192);f.Fill(src,8192);f.Map(dst);f.Fill(dst);
            byte[] bytes=[0x36,0x36,0x67,0x67,repeat,repeat,0x48,(byte)(op+1)];
            WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            f.DataScope(0,()=>
            {
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+4088,0,0),64,5);
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)src+4096,0,0),64,5);
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)dst+128,0,0),64,repeat==0xf2?2UL:5UL);
                RepPutElement(f.Program.LookupAddress2(f.Machine(),(long)dst+136,0,0),64,repeat==0xf2?2UL:5UL);
            });
            uint flags=(f.Machine()->flags&~0x50540u)|(repeat==0xf2?64u:0);
            RepSeed(f,(src+4088)|0xdeadbeef00000000UL,(dst+128)|0xcafef00d00000000UL,5,0x1234567800000002UL,flags);f.Counts(1);
            bool changed=false;ulong attempt=0;
            f.Owner.RepPausedForTests=(pointer,reason)=>
            {
                var state=(FullRepState*)pointer;
                Check(state->SourceSegment==2 && state->InstructionLength==bytes.Length &&
                    new ReadOnlySpan<byte>(state->DecodedBytes,bytes.Length).SequenceEqual(bytes),"R162 immutable source SS/original ordered-prefix latch identity");
                if(attempt==0)attempt=state->AttemptGeneration;
                Check(attempt==state->AttemptGeneration,"R162 all per-iteration handoffs retain the issued attempt");
                if(changed || reason!=FullRepReason.NeedMemory || state->CompletedIterations!=1)return;
                RepClean(f); // called only after old capabilities have ended
                WriteControllerCode(f,pc,[0xf3,0xaa,0xeb,0xfe]);changed=true;
            };
            using var deadline=new Timer(_=>f.Stop.RequestStop(),null,TimeSpan.FromSeconds(8),Timeout.InfiniteTimeSpan);
            try
            {
                Check(f.Step(pc,signalActor:true) && changed && attempt!=0 && f.Issued==1 && f.Completed==1 &&
                    RepRegister(f,72)==src+4104 && RepRegister(f,80)==(op==0xac?((dst+128)|0xcafef00d00000000UL):dst+144) &&
                    RepRegister(f,32)==0 && f.Machine()->ip==pc+(uint)bytes.Length,
                    "R162 per-iteration mapping resumes original SS/AS32/REP instruction after opcode/prefix replacement");
                RepClean(f);
            }
            finally{f.Owner.RepPausedForTests=null;}
        }
    }
}
