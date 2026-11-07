using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using AtomicFixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // ONE parent entry: one candidate/family batch on CoreCLR and NativeAOT.
    // AUTHORED_UNRUN. Existing R69 tests are included, not separate candidates.
    static void CheckFullAtomicFamilyR156(byte* machine)
    {
        Console.WriteLine("BEGIN atomic family CheckFullNativeAtomic(machine)");
        CheckFullNativeAtomic(machine);
        CheckFullPublicationCleanupFastPath();
        CheckFullScalarAtomicR183(machine);
        Console.WriteLine("BEGIN atomic family AtomicFamilyPrefixAndAddressGrammar()");
        AtomicFamilyPrefixAndAddressGrammar();
        CheckFullAtomicAlignmentR156R1(machine);
        Console.WriteLine("BEGIN atomic family AtomicFamilyNativeModel(machine)");
        AtomicFamilyNativeModel(machine);
        Console.WriteLine("BEGIN atomic family AtomicFamilyControllerTable()");
        AtomicFamilyControllerTable();
        CheckAtomicInspectionRetry();
        Console.WriteLine("BEGIN atomic family AtomicFamilyInputAuthentication()");
        AtomicFamilyInputAuthentication();
        Console.WriteLine("BEGIN atomic family AtomicFamilyReaderSchedules()");
        AtomicFamilyReaderSchedules();
        Console.WriteLine("BEGIN atomic family AtomicFamilySimultaneousRmw()");
        AtomicFamilySimultaneousRmw();
        Console.WriteLine("BEGIN atomic family AtomicFamilyAliasesAndEof()");
        AtomicFamilyAliasesAndEof();
        Console.WriteLine("BEGIN atomic family AtomicFamilyPendingFaultAndStop()");
        AtomicFamilyPendingFaultAndStop();
        Console.WriteLine("BEGIN atomic family AtomicFamilySyscallProgress()");
        AtomicFamilySyscallProgress();
        Console.WriteLine("BEGIN atomic family AtomicFamilySigactionCycle()");
        AtomicFamilySigactionCycle();
        Console.WriteLine("BEGIN atomic family AtomicFamilyRecursiveActor()");
        AtomicFamilyRecursiveActor();
        Console.WriteLine("BEGIN atomic family AtomicFamilyBackingRead()");
        AtomicFamilyBackingRead();
        Console.WriteLine("BEGIN atomic family AtomicFamilySnapshotFailureQuota()");
        AtomicFamilySnapshotFailureQuota();
        // Retain the already authored exception/quota/handshake obligations.
        Console.WriteLine("BEGIN atomic family CheckFullSemanticAuthority()");
        CheckFullSemanticAuthority();
        Console.WriteLine("BEGIN atomic family CheckHostMemoryFileRead()");
        CheckHostMemoryFileRead();
    }
    readonly record struct AtomicFamilyRow(string Name, FullAtomicOperation Op, int Width, byte[] Bytes);
    static IEnumerable<AtomicFamilyRow> AtomicFamilyRows()
    {
        foreach (int w in new[]{8,16,32,64})
        {
            foreach ((string n, FullAtomicOperation op, byte[] b) in new (string,FullAtomicOperation,byte[])[] {
                ("xchg",FullAtomicOperation.Exchange,[w==8?(byte)0x86:(byte)0x87,0x0b]),
                ("xadd",FullAtomicOperation.ExchangeAdd,[0x0f,w==8?(byte)0xc0:(byte)0xc1,0x0b]),
                ("cmpxchg",FullAtomicOperation.CompareExchange,[0x0f,w==8?(byte)0xb0:(byte)0xb1,0x0b]) })
            foreach (bool locked in new[]{false,true}) yield return new(n+(locked?"-lock":""),op,w,AtomicEncoding(w,b,locked));
            FullAtomicOperation[] ops=[FullAtomicOperation.Add,FullAtomicOperation.Or,FullAtomicOperation.Adc,
                FullAtomicOperation.Sbb,FullAtomicOperation.And,FullAtomicOperation.Sub,FullAtomicOperation.Xor];
            for (int a=0;a<ops.Length;++a)
            {
                yield return new("alu-reg",ops[a],w,AtomicEncoding(w,[(byte)(a*8+(w==8?0:1)),0x0b]));
                yield return new("alu-imm8",ops[a],w,AtomicEncoding(w,[w==8?(byte)0x80:(byte)0x83,(byte)(3|(a<<3)),0xff]));
                if(w==8)yield return new("selected-82-alias",ops[a],w,AtomicEncoding(w,[0x82,(byte)(3|(a<<3)),0xff]));
                if(w!=8)
                {
                    byte[] imm=w==16?[0xff,0xff]:[0xff,0xff,0xff,0xff];
                    yield return new("alu-imm-full",ops[a],w,AtomicEncoding(w,[0x81,(byte)(3|(a<<3)),..imm]));
                }
            }
            foreach ((FullAtomicOperation op,int group,bool unary) in new[]{
                (FullAtomicOperation.Inc,0,true),(FullAtomicOperation.Dec,1,true),
                (FullAtomicOperation.Not,2,false),(FullAtomicOperation.Neg,3,false)})
                yield return new("unary",op,w,AtomicEncoding(w,[unary?(w==8?(byte)0xfe:(byte)0xff):(w==8?(byte)0xf6:(byte)0xf7),(byte)(3|(group<<3))]));
            if(w==8)continue;
            for(int b=0;b<3;++b)
            {
                var op=(FullAtomicOperation)((int)FullAtomicOperation.BitSet+b);
                yield return new("bit-reg",op,w,AtomicEncoding(w,[0x0f,new byte[]{0xab,0xb3,0xbb}[b],0x0b]));
                yield return new("bit-imm",op,w,AtomicEncoding(w,[0x0f,0xba,(byte)(3|((5+b)<<3)),0xff]));
            }
        }
        foreach(int width in new[]{64,128}) foreach(bool locked in new[]{false,true})
            yield return new("pair",FullAtomicOperation.CompareExchangePair,width,AtomicEncoding(width==128?64:32,[0x0f,0xc7,0x0b],locked));
    }

    static void AtomicFamilyPrefixAndAddressGrammar()
    {
        var diagnostic=new FullBlockFrame { MemoryResume=1, MemoryAccess=FullNativeMemory.AtomicReadWrite,
            MemoryWidth=128,MemoryTlb=(FullMemoryTlbEntry*)1 };
        Check(FullMemoryMissDiagnostics.Observe(in diagnostic,1)==FullMemoryMissReason.WriteBarrier,
            "R156 valid atomic diagnostics never dereference a TLB hint");
        // Structural finite grammar witnesses: every mod/rm, REX nibble, SIB
        // byte, displacement length and prefix class. Counts are not a claim
        // to have executed all encodings or arbitrary 15-byte prefix strings.
        foreach(byte[] prefix in new byte[][] {
            [],[0xf0],[0xf0,0xf0],[0x66,0x66],[0x67,0x67],[0xf2],[0xf3],[0xf2,0xf0],[0xf3,0xf0],
            [0x64,0x65],[0x65,0x64],[0x48,0x66],[0x48,0x40],[0x66,0x48],[0x48,0xf0] })
        {
            byte[] b=[..prefix,0x87,0x0b];var i=AtomicClassify(b);
            Check(i.Length==b.Length,"R156 exact full-prefix input length");
            bool rexW=prefix.Length!=0&&prefix[^1]==0x48;
            Check(i.Width==(rexW?64:Array.IndexOf(prefix,(byte)0x66)>=0?16:32),"R156 legacy resets preceding REX and final REX wins");
        }
        foreach(int width in new[]{8,16,32,64})
        {
            byte[] b=AtomicEncoding(width,[width==8?(byte)0x86:(byte)0x87,0x0b]);
            if(width==8)b=[0x66,..b];
            Check(AtomicClassify(b).Width==width,"R156 redundant operand prefix on byte form");
        }
        foreach(byte[] b in new byte[][] {
            [0xf0,0x44,0x83,0x03,1],[0xf0,0x44,0xff,0x03],[0xf0,0x44,0xf7,0x13],
            [0xf0,0x44,0x0f,0xba,0x2b,1],[0x66,0x44,0x0f,0xc7,0x0b],[0x66,0x4c,0x0f,0xc7,0x0b],
            [0xf0,0x82,0x03,1] }) AtomicClassify(b); // 82 is explicitly selected-source-only.
        for(int mod=0;mod<3;++mod) for(int rm=0;rm<8;++rm) for(int rex=0;rex<16;++rex)
        foreach(bool as32 in new[]{false,true})
        {
            var b=new List<byte>{0xf0};if(as32)b.Add(0x67);b.Add((byte)(0x40|rex));b.Add(0x01);b.Add((byte)((mod<<6)|8|rm));
            if(rm==4)b.Add(0x8b);
            int disp=mod==1?1:mod==2||mod==0&&rm==5?4:0;
            for(int d=0;d<disp;++d)b.Add(0xff);
            var i=AtomicClassify(b.ToArray());
            Check(i.Address.AddressWidth==(as32?32:64)&&i.Address.Scale==(rm==4?2:0),"R156 mod/rm/REX/AS32 address domain");
        }
        for(int sib=0;sib<256;++sib)
        {
            byte[] d=(sib&7)==5?[0xff,0xff,0xff,0xff]:[];
            var i=AtomicClassify([0xf0,0x4b,0x01,0x0c,(byte)sib,..d]);
            Check(i.Address.Scale==sib>>6,"R156 all SIB structural values");
        }
        Check(!FullAtomicLowering.TryClassify(Ip,new byte[16],out _,out _),"R156 architectural instruction length bound");
    }

    // Successful pair16 rows are necessarily in one page (4096 is divisible
    // by16). Pair8 is the valid unaligned/cross-page scatter exclusion witness.
    static int[] AtomicFamilyDataOffsets(int width)=>width==128?[64,4080]:[64,65,4095];
    static UInt128 AtomicFamilyMask(int w)=>w==128?UInt128.MaxValue:((UInt128)1<<w)-1;
    static UInt128 AtomicFamilyRead(byte* a,byte* b,int n,int w)
    { UInt128 x=0;for(int k=0;k<w/8;++k)x|=(UInt128)(k<n?a[k]:b[k-n])<<(k*8);return x; }
    static void AtomicFamilyWrite(byte* a,byte* b,int n,int w,UInt128 x)
    { for(int k=0;k<w/8;++k)if(k<n)a[k]=(byte)(x>>(8*k));else b[k-n]=(byte)(x>>(8*k)); }
    static (UInt128 Value,uint Flags,bool Equal) AtomicFamilyModel(FullAtomicOperation op,int w,UInt128 x,UInt128 y,UInt128 expected,uint flags)
    {
        UInt128 mask=AtomicFamilyMask(w);x&=mask;y&=mask;expected&=mask;
        bool eq=x==expected;
        if(op==FullAtomicOperation.CompareExchangePair)return(eq?y:x,(flags&~64u)|(eq?64u:0),eq);
        if(op==FullAtomicOperation.CompareExchange)
            return(eq?y:x,AtomicExpectedFlags(FullAtomicOperation.Sub,w,(ulong)expected,(ulong)x,flags),eq);
        if(op is FullAtomicOperation.BitSet or FullAtomicOperation.BitReset or FullAtomicOperation.BitComplement)
        {
            UInt128 bit=(UInt128)1<<(int)(y&(uint)(w-1));
            UInt128 z=op==FullAtomicOperation.BitSet?x|bit:op==FullAtomicOperation.BitReset?x&~bit:x^bit;
            return(z,(flags&~1u)|((x&bit)!=0?1u:0),false);
        }
        UInt128 carry=op is FullAtomicOperation.Adc or FullAtomicOperation.Sbb?flags&1u:0;
        UInt128 v=op switch{
            FullAtomicOperation.Exchange=>y,FullAtomicOperation.Not=>~x,FullAtomicOperation.Neg=>unchecked((UInt128)0-x),
            FullAtomicOperation.And=>x&y,FullAtomicOperation.Or=>x|y,FullAtomicOperation.Xor=>x^y,
            FullAtomicOperation.Inc=>x+1,FullAtomicOperation.Dec=>unchecked(x-1),
            FullAtomicOperation.Sub=>unchecked(x-y),FullAtomicOperation.Sbb=>unchecked(x-y-carry),_=>x+y+carry};
        return(v&mask,AtomicExpectedFlags(op,w,(ulong)x,(ulong)y,flags),false);
    }
    static void AtomicFamilyNativeModel(byte* machine)
    {
        byte* a=(byte*)NativeMemory.AllocZeroed(4096),b=(byte*)NativeMemory.AllocZeroed(4096);
        try
        {
            foreach(bool common in new[]{false,true})
            {
                using var scaffold=CreateFullScaffold(out var addresses);
                var previous=fullOwnerScaffold;fullOwnerScaffold=common?addresses:default;
                try
                {
                    foreach(var row in AtomicFamilyRows()) foreach(int offset in AtomicFamilyDataOffsets(row.Width))
                    foreach(uint carry in new uint[]{0,1}) foreach(bool equal in new[]{false,true})
                    {
                        var i=AtomicClassify(row.Bytes);AtomicSeed(machine);
                        Write64(machine,48,0x100000+(uint)offset);Write64(machine,32,5);
                        int n=Math.Min(row.Width/8,4096-offset);byte* first=a+offset,second=n==row.Width/8?null:b;
                        UInt128 x=AtomicFamilyMask(row.Width)^((UInt128)0x15263748<<3);
                        ulong low=(ulong)(x&ulong.MaxValue),high=(ulong)(x>>64);
                        Write64(machine,24,low^(equal?0UL:1UL));Write64(machine,40,high);
                        ((uint*)(machine+12))[0]=(0xa52c0e14u&~0x50100u)|carry;
                        UInt128 y=i.Source.IsImmediate?i.Source.Immediate:5;
                        if(row.Op==FullAtomicOperation.CompareExchangePair)
                        {
                            int h=row.Width/2;UInt128 hm=AtomicFamilyMask(h);
                            Write64(machine,24,(ulong)(x&hm)^(equal?0UL:1UL));Write64(machine,40,(ulong)((x>>h)&hm));
                            y=(((UInt128)5&hm)<<h)|((UInt128)(0x100000+(uint)offset)&hm);
                        }
                        UInt128 expected=row.Op==FullAtomicOperation.CompareExchangePair?
                            ((UInt128)((ulong*)(machine+40))[0]<<(row.Width/2))|((ulong*)(machine+24))[0]:((ulong*)(machine+24))[0];
                        uint flags=((uint*)(machine+12))[0];
                        var model=AtomicFamilyModel(row.Op,row.Width,x,y,expected,flags);
                        new Span<byte>(a,4096).Fill(0xa5);new Span<byte>(b,4096).Fill(0x5a);AtomicFamilyWrite(first,second,n,row.Width,x);
                        AtomicRunTicket(machine,i,first,second,n);
                        Check(AtomicFamilyRead(first,second,n,row.Width)==model.Value&&((uint*)(machine+12))[0]==model.Flags,$"R156 UInt128 family {row.Name}/{row.Op}/{row.Width}/{offset}/{carry}/{equal}");
                    }
                }
                finally {fullOwnerScaffold=previous;}
            }
        }
        finally {NativeMemory.Free(a);NativeMemory.Free(b);}
    }
    static void AtomicFamilyAssertClean(AtomicFixture f)
    {
        var a=f.Jit.Authority;var st=f.Jit.Store.GetStatistics();
        Check(a.PendingAtomicWorker==-1&&a.MappingWriters==0&&a.CopyWriters==0&&a.HostShortReaders==0&&
            a.ActiveBorrows==0&&a.SemanticReadersDrained()&&st.ActiveManagedLeases==0&&st.ActiveNativeEpochs==0&&
            f.Statistics.ActiveDataBorrows==0,"R156 terminal admission/store/passive cleanup");
    }
    static void AtomicFamilySetup(AtomicFixture f,ulong code,ulong data)
    {
        f.Map(code);f.Fill(code);f.Map(data,8192);f.Fill(data,8192);f.Counts(100000);
        // R183 fast cases require EXISTING ordinary native write authority.
        // Fixture Fill uses a read-only raw resolver, so explicitly exercise
        // the ordinary admission barrier before any tested atomic issue.
        lock(f.Jit.Authority.Gate)
        {
            Check(f.Jit.Authority.TryGetNativeDataPermission(f.Physical(data),true,out _), "R183 fixture ordinary RW authority page0");
            Check(f.Jit.Authority.TryGetNativeDataPermission(f.Physical(data+4096),true,out _), "R183 fixture ordinary RW authority page1");
        }
    }
    static void AtomicFamilySeed(AtomicFixture f,ulong data,int worker=0)
    { AtomicSeed((byte*)f.Machine(worker));Write64((byte*)f.Machine(worker),48,data);Write64((byte*)f.Machine(worker),32,5); }
    static void AtomicFamilyControllerTable()
    {
        foreach(var row in AtomicFamilyRows())foreach(int offset in AtomicFamilyDataOffsets(row.Width))
        foreach(bool equal in (row.Op is FullAtomicOperation.CompareExchange or FullAtomicOperation.CompareExchangePair)?new[]{false,true}:new[]{false})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
            AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,[..row.Bytes,0xeb,0xfe]);AtomicFamilySeed(f,data+(uint)offset);
            byte* a=(byte*)f.Physical(data)+offset,b=(byte*)f.Physical(data+4096);int n=Math.Min(row.Width/8,4096-offset);
            UInt128 x=0x813579bdf02468ac;AtomicFamilyWrite(a,b,n,row.Width,x);
            var i=AtomicClassify(row.Bytes);UInt128 y=i.Source.IsImmediate?i.Source.Immediate:5;
            if(row.Op==FullAtomicOperation.CompareExchangePair)y=((UInt128)5<<(row.Width/2))|((UInt128)(data+(uint)offset)&AtomicFamilyMask(row.Width/2));
            if(row.Op==FullAtomicOperation.CompareExchangePair)
            {
                int half=row.Width/2;UInt128 mask=AtomicFamilyMask(half);
                Write64((byte*)f.Machine(),24,(ulong)(x&mask)^(equal?0UL:1UL));Write64((byte*)f.Machine(),40,(ulong)(x>>half));
            }
            else if(row.Op==FullAtomicOperation.CompareExchange)Write64((byte*)f.Machine(),24,(ulong)(x&AtomicFamilyMask(row.Width))^(equal?0UL:1UL));
            UInt128 ex=row.Op==FullAtomicOperation.CompareExchangePair?
                ((UInt128)((ulong*)((byte*)f.Machine()+40))[0]<<(row.Width/2))|((ulong*)((byte*)f.Machine()+24))[0]:((ulong*)((byte*)f.Machine()+24))[0];
            var expected=AtomicFamilyModel(row.Op,row.Width,x,y,ex,f.Machine()->flags);
            Check(f.Step(pc,signalActor:true)&&f.Completed==1&&f.Issued==1&&f.Machine()->ip==pc+(uint)row.Bytes.Length,
                "R156 actual controller performs exactly one family operation on one issue");
            Check(AtomicFamilyRead(a,b,n,row.Width)==expected.Value&&f.Machine()->flags==expected.Flags&&
                f.Statistics.NativeMemoryInstructions==1&&f.Statistics.SemanticHelperInstructions==0&&f.Statistics.InterpreterAttempts==0,
                "R156 controller uses generated atomic computation and independent expected state");
            AtomicFamilyAssertClean(f);
        }
    }
    static void AtomicFamilyInputAuthentication()
    {
        foreach(string field in new[]{"base","source","implicit","flags","fs","gs"})
        foreach(int transportOffset in new[]{0,1,4095})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
            AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xc7,0x0b]);AtomicFamilySeed(f,data+(uint)transportOffset);
            f.AtomicAssertSnapshot(pc,()=>{
                var m=f.Machine();if(field=="flags")m->flags^=1;
                else if(field=="fs")++m->fs.@base;else if(field=="gs")++m->gs.@base;
                else{int r=field=="base"?3:field=="source"?1:2;++((ulong*)((byte*)m+24))[r];}
            });
            AtomicFamilyAssertClean(f);
        }
    }

    sealed class AtomicFamilyThread : IDisposable
    {
        readonly AtomicFixture f;readonly Thread thread;Exception? error;
        internal AtomicFamilyThread(AtomicFixture f,int worker,Action body)
        {
            this.f=f;f.AtomicPrepareBoundWorker(worker);
            thread=new Thread(()=>{try{f.AtomicBoundWorker(worker,body);}catch(Exception e){error=e;}}){IsBackground=true};thread.Start();
        }
        internal void Join()
        { if(!thread.Join(TimeSpan.FromSeconds(8))){f.Stop.RequestStop();throw new InvalidOperationException("R156 bounded worker did not join; owner retained.");}if(error!=null)throw new AggregateException(error); }
        public void Dispose()=>Join();
    }
    static void AtomicFamilyWait(ManualResetEventSlim e,string message)
    { Check(e.Wait(TimeSpan.FromSeconds(5)),message); }
    static void AtomicFamilyWaitUntil(Func<bool> predicate,string message)
    { Check(SpinWait.SpinUntil(predicate,TimeSpan.FromSeconds(5)),message); }

    static void AtomicFamilyNormalRead(AtomicFixture f,int worker,ulong address,string kind,Action? admitted=null)
    {
        var m=f.Machine(worker);((ulong*)((byte*)m+24))[3]=address;
        f.DataScope(worker,()=>{
            // The body starts only AFTER publication AND its pending recheck.
            admitted?.Invoke();
            if(kind=="raw")
            {
                byte* pointer=f.Program.ReserveAddress(m,unchecked((long)address),8,false);
                Check(pointer!=null,"R156 real read-only C raw reservation");
                ((ulong*)((byte*)m+24))[0]=unchecked((ulong)f.Program.Load64Unlocked(pointer));
                if(m->stashaddr!=0)f.Program.CommitStash(m);return;
            }
            if(kind=="copy")
            {
                byte* copied=stackalloc byte[16];
                Check(f.Program.CopyFromUserRead(m,copied,unchecked((long)address),16)==0,"R156 bounded actual CopyFromUserRead");
                ((ulong*)((byte*)m+24))[0]=((ulong*)copied)[0];((ulong*)((byte*)m+40))[0]=((ulong*)copied)[1];return;
            }
            byte[] bytes=kind=="sse"?[0xf3,0x0f,0x6f,0x03]:[0x48,0x8b,0x03];Blink.XedDecodedInst d=default;
            fixed(byte* p=bytes)Check(f.Program.DecodeInstruction(&d,p,(ulong)bytes.Length,Blink.XED_MODE_LONG)==0,"R156 normal reader decode");
            var saved=m->xedd;m->xedd=&d;
            try
            {
                var op=d.op;
                // This is the deliberately normal C reader opposite the native
                // atomic writer, not an interpreter fallback for that writer.
                // GetOp's production dispatch tripwire is at table lookup.
                // Hide only its lookup marker, restore it before invoking the
                // selected body, and retain the real CPU/data read scopes.
                delegate*<Blink,Blink.Machine*,ulong,long,ulong,void> reader;
                f.Program.SetHostGuestCurrentMachine(null);
                try { reader=f.Program.GetOp((long)(op.rde>>40&2047)); }
                finally { f.Program.SetHostGuestCurrentMachine(m); }
                reader(f.Program,m,op.rde,op.disp,op.uimm0);
            }
            finally{m->xedd=saved;}
            if(m->stashaddr!=0)f.Program.CommitStash(m);
            if(kind=="sse") {((ulong*)((byte*)m+24))[0]=((ulong*)((byte*)m+160))[0];((ulong*)((byte*)m+40))[0]=((ulong*)((byte*)m+168))[0];}
        });
    }
    static void AtomicFamilyReaderSchedules()
    {
        foreach(string kind in new[]{"scalar","raw","sse","copy"})
        foreach(int pairWidth in new[]{64,128})
        foreach(bool oldReader in new[]{true,false})foreach(int offset in AtomicFamilyDataOffsets(pairWidth))
        foreach(int mode in kind=="scalar"&&pairWidth==64&&oldReader&&offset==64?new[]{0,1,2,3}:new[]{0})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192,addr=data+(uint)offset;
            AtomicFamilySetup(f,pc,data);
            bool canonical=(mode&1)!=0,scalar=(mode&2)!=0;
            if(canonical)f.StartCanonicalFamily();
            WriteControllerCode(f,pc,scalar?[0xf0,0x48,0x01,0x0b]:AtomicEncoding(pairWidth==128?64:32,[0x0f,0xc7,0x0b]));AtomicFamilySeed(f,addr);
            byte* a=(byte*)f.Physical(data)+offset,b=(byte*)f.Physical(data+4096);int n=Math.Min(16,4096-offset);
            AtomicFamilyWrite(a,b,n,128,0);Write64((byte*)f.Machine(),24,0);Write64((byte*)f.Machine(),40,0);
            using var readerReady=new ManualResetEventSlim();using var proceed=new ManualResetEventSlim();using var pendingSeen=new ManualResetEventSlim();
            f.Jit.Authority.TestSemanticPending=_=>pendingSeen.Set();
            using var reader=new AtomicFamilyThread(f,1,()=>{
                if(!oldReader){readerReady.Set();AtomicFamilyWait(proceed,"R156 new reader starts while exact ticket is active");}
                AtomicFamilyNormalRead(f,1,addr,kind,oldReader?()=>{
                    readerReady.Set();AtomicFamilyWait(proceed,"R156 admitted old reader released after reservation");
                }:null);
            });
            AtomicFamilyWait(readerReady,"R156 foreign bound reader ready");
            f.Jit.Authority.TestReservationPublished=_=>{if(oldReader)proceed.Set();};
            if(!oldReader)f.NativePrepared=p=>{
                var frame=(FullBlockFrame*)p;
                if(frame->MemoryFlags!=FullMemoryFlags.BarrierTicket)return;
                proceed.Set();AtomicFamilyWait(pendingSeen,"R156 new reader observes pending before native pair execution");
            };
            // Initial miss has no ticket; set the ticket hook from reservation
            // callback so the one-shot prepared hook cannot be consumed early.
            if(!oldReader)f.Jit.Authority.TestReservationPublished=_=>f.NativePrepared=p=>{
                Check(((FullBlockFrame*)p)->MemoryFlags==FullMemoryFlags.BarrierTicket,"R156 final prepared ticket");
                proceed.Set();AtomicFamilyWait(pendingSeen,"R156 pending rejects new reader before scatter");
            };
            try{Check(f.Step(pc,signalActor:true),"R156 atomic reader schedule controller completes");reader.Join();}
            finally{proceed.Set();f.Jit.Authority.TestSemanticAdmission=null;f.Jit.Authority.TestSemanticPending=null;f.Jit.Authority.TestReservationPublished=null;f.NativePrepared=null;if(canonical)f.EndCanonicalFamily();}
            if(scalar)Check(*(ulong*)a==5&&f.Completed==1,"cold aligned scalar request makes reservation progress with old reader and active/inactive canonical state");
            ulong observed=((ulong*)((byte*)f.Machine(1)+24))[0];
            // Read64 can straddle guest pages; selected C uses its ordinary stash.
            ulong low=pairWidth==128?addr:((ulong)5<<32)|(uint)addr;
            ulong high=pairWidth==128?5UL:0UL; // 8B leaves the next8 bytes unchanged.
            Check(observed==(oldReader?0UL:low),$"R156 reader kind={kind} width={pairWidth} old={oldReader} offset={offset} actual={observed:x} expected={(oldReader?0UL:low):x}");
            if(kind is "sse" or "copy")Check(((ulong*)((byte*)f.Machine(1)+40))[0]==(oldReader?0UL:high),
                "R156 width-aware pair reader high word and untouched trailing bytes");
            AtomicFamilyAssertClean(f);
        }
    }
    static void AtomicFamilySimultaneousRmw()
    {
        foreach(int offset in new[]{64,65,4095})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192,addr=data+(uint)offset;
            AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xc1,0x0b]);
            byte* a=(byte*)f.Physical(data)+offset,b=(byte*)f.Physical(data+4096);int n=Math.Min(8,4096-offset);
            AtomicFamilyWrite(a,b,n,64,0);
            using var ready=new ManualResetEventSlim();using var start=new ManualResetEventSlim();
            using var thread=new AtomicFamilyThread(f,1,()=>{
                ready.Set();AtomicFamilyWait(start,"R156 simultaneous RMW start");
                for(int k=0;k<32;++k){AtomicFamilySeed(f,addr,1);Write64((byte*)f.Machine(1),32,1);Check(f.Step(pc,1,true),"R156 foreign actual RMW");}
            });
            AtomicFamilyWait(ready,"R156 foreign RMW worker bound");start.Set();
            for(int k=0;k<32;++k){AtomicFamilySeed(f,addr);Write64((byte*)f.Machine(),32,1);Check(f.Step(pc,signalActor:true),"R156 main actual RMW");}
            thread.Join();
            Check(AtomicFamilyRead(a,b,n,64)==64&&f.Completed==64&&f.Issued==64,"R156 two actual controllers serialize every RMW without lost updates");
            AtomicFamilyAssertClean(f);
        }
    }
    static void AtomicFamilyAliasesAndEof()
    {
        using(var f=new AtomicFixture())
        {
            using var deadline=f.AtomicDeadline();
            ulong pc=AtomicFixture.Base,alias=pc+4096,atomic=pc+8192;
            int fd=f.Io.CreateMemoryFile("r156-code-alias",0).Value;
            Check(f.Io.WriteAt(fd,new byte[4096],0).Value==4096,"R156 shared backing initialized");
            f.MapShared(fd,pc);f.MapShared(fd,alias);f.Map(atomic);f.Fill(atomic);
            f.WriteMov(pc,17);Check(f.Acquire(alias,0,out var lease,out _,out int error,1)&&error==0,"R156 watched alias retained");lease.Dispose();
            WriteControllerCode(f,atomic,[0x87,0x0b]);AtomicFamilySeed(f,pc+1);Write64((byte*)f.Machine(),32,29);f.Counts(100);
            Check(f.Step(atomic,signalActor:true),"R156 unaligned XCHG updates watched shared code bytes");
            Check(f.Step(alias,signalActor:true)&&((ulong*)((byte*)f.Machine()+24))[0]==29,"R156 all physical code aliases revalidate after native atomic");
            AtomicFamilyAssertClean(f);Check(f.Io.Close(fd).Succeeded,"R156 alias descriptor close");
        }
        foreach(bool truncateAfterReservation in new[]{false,true})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
            f.Map(pc);f.Fill(pc);int fd=f.Io.CreateMemoryFile("r156-eof",0).Value;
            Check(f.Io.WriteAt(fd,new byte[4096],0).Value==4096,"R156 EOF backing initial bytes");f.MapShared(fd,data);
            WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xc7,0x0b]);AtomicFamilySeed(f,data);
            if(truncateAfterReservation)f.Jit.Authority.TestReservationPublished=_=>Check(f.Io.TruncateFile(fd,0).Succeeded,"R156 truncate inside pending window");
            else Check(f.Io.TruncateFile(fd,0).Succeeded,"R156 truncate before atomic attempt");
            try
            {
                var signal=f.StepSynchronousFault(pc);
                Check(signal.Signal==7&&f.Completed==0&&f.Issued==1,"R156 precise SIGBUS EOF before any pair comparison or effect");
            }
            finally{f.Jit.Authority.TestReservationPublished=null;}
            AtomicFamilyAssertClean(f);f.Io.Close(fd);
        }
    }
    static void AtomicFamilyPendingFaultAndStop()
    {
        foreach(string mode in new[]{"stop","mprotect","munmap","MAP_FIXED","throw"})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
            AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,[0xf0,0x48,0x01,0x0b]);AtomicFamilySeed(f,data);
            // This schedule specifically qualifies exclusive-ticket phases.
            lock(f.Jit.Authority.Gate)
                Check(f.Jit.Authority.PrepareCodeSnapshot(f.Physical(data)), "R183 retain slow pending fixture");
            int reservations=0;var marker=new InvalidOperationException("R156 preparation failure");
            f.Jit.Authority.TestReservationPublished=_=>{
                if(Interlocked.Increment(ref reservations)!=1)return;
                if(mode=="stop")f.Stop.RequestStop();else if(mode=="throw")throw marker;
                else Check(f.Mutate(mode,data)==0,"R156 actual mapping change during pending reservation");
            };
            try
            {
                if(mode is "mprotect" or "munmap")
                {
                    var signal=f.StepSynchronousFault(pc);Check(signal.Signal==11&&f.Completed==0&&f.Issued==1,"R156 final both-page inspection faults same issue");
                }
                else if(mode=="throw")
                {
                    try{f.Step(pc,signalActor:true);Check(false,"R156 injected preparation failure must surface");}
                    catch(InvalidOperationException e)when(ReferenceEquals(e,marker)){}
                }
                else{f.Step(pc,signalActor:true);Check(f.Issued==1&&f.Completed==(mode=="stop"?0UL:1UL),"R156 pending stop/remap exact issue accounting");}
            }
            finally{f.Jit.Authority.TestReservationPublished=null;}
            AtomicFamilyAssertClean(f);
        }
        // Canonical faults precede any mapping assist; include default SS,
        // overridden DS, FS/GS and crossing-end noncanonical endpoints.
        foreach(byte[] bytes in new byte[][]{
            [0xf0,0x48,0x01,0x0b],[0xf0,0x48,0x01,0x4d,0],
            [0x3e,0xf0,0x48,0x01,0x4d,0],[0x64,0xf0,0x48,0x01,0x0b],[0x65,0xf0,0x48,0x01,0x0b]})
        foreach(ulong address in new[]{0x800000000000UL,0x7ffffffffffcUL,ulong.MaxValue-3})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base;f.Map(pc);f.Fill(pc);WriteControllerCode(f,pc,bytes);AtomicFamilySeed(f,address);
            Write64((byte*)f.Machine(),64,address); // RBP at machine+64
            var i=AtomicClassify(bytes);var signal=f.StepSynchronousFault(pc);
            bool ss=FullScalarMiscLowering.UsesStackSegment(i.Address);
            Check(signal.Signal==(ss?7:11)&&f.Machine()->trapno==(ss?12:13)&&
                f.Machine()->faultaddr==0&&f.Completed==0&&f.Issued==1&&f.Statistics.NativeMemoryAssists==0,
                $"R156 atomic canonical {(ss?"#SS/SIGBUS":"#GP/SIGSEGV")} bytes={Convert.ToHexString(bytes)} address={address:x} signal={signal.Signal} trap={f.Machine()->trapno} before effects or MMU lookup");AtomicFamilyAssertClean(f);
        }
    }
    static bool AtomicFamilyFutexEnqueued(AtomicFixture f)
    {
        var bus=(Blink.Bus*)f.Program.Globals.g_bus;
        Check(bus!=null,"R156 real bus for futex enqueue observation");
        Blink.Libc.pthread_mutex_lock(&bus->futexes.@lock);
        try { return bus->futexes.active!=null; }
        finally { Blink.Libc.pthread_mutex_unlock(&bus->futexes.@lock); }
    }
    static void AtomicFamilySyscallProgress()
    {
        foreach(bool samePage in new[]{true,false})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192,addr=data+64,buffer=samePage?data+128:data+4096;
            AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xb1,0x0b]);WriteControllerCode(f,pc+64,[0x0f,0x05]);
            var pipe=f.Io.Pipe().Value;using var cancel=new CancellationTokenSource(TimeSpan.FromSeconds(8));
            // Host descriptors also need the guest fd metadata that SysPipe2
            // normally installs; otherwise SysWrite correctly returns EBADF.
            Check(f.Program.AddFd(&f.Machine()->blink_host_system->fds,pipe.Read,0)!=null&&
                f.Program.AddFd(&f.Machine()->blink_host_system->fds,pipe.Write,1)!=null,
                "R156 guest pipe descriptor access modes installed before worker starts");
            Check(f.Io.WriteAsync(pipe.Write,new byte[65536],cancel.Token).GetAwaiter().GetResult().Value==65536,"R156 full pipe established");
            using var started=new ManualResetEventSlim();
            using var writer=new AtomicFamilyThread(f,1,()=>{started.Set();f.AtomicSyscall(pc+64,1,1,(ulong)pipe.Write,buffer,1);
                Check(((ulong*)((byte*)f.Machine(1)+24))[0]==1,"R156 actual blocked guest write returns one byte");});
            AtomicFamilyWait(started,"R156 actual guest write worker started");
            AtomicFamilyWaitUntil(()=>f.Io.PendingPipeOperations==1,"R156 guest write reached actual blocking pipe operation");
            Check(f.Jit.Authority.ActiveBorrows==0&&f.Jit.Authority.SemanticReadersDrained(),"R156 blocking write retains no authority borrow/CPU/copy permit");
            AtomicFamilySeed(f,addr);Write64((byte*)f.Machine(),24,((ulong*)((byte*)f.Physical(data)+64))[0]);
            Check(f.Step(pc,signalActor:true)&&((ulong*)((byte*)f.Physical(data)+64))[0]==5&&f.Statistics.NativeMemoryInstructions==1,
                "R156 native successful CAS required for full-pipe progress completes first");
            Check(f.Io.ReadAsync(pipe.Read,new byte[1],cancel.Token).GetAwaiter().GetResult().Value==1,"R156 drain releases blocked guest write");writer.Join();
            AtomicFamilyAssertClean(f);f.Io.Close(pipe.Read);f.Io.Close(pipe.Write);
        }
        foreach(bool bitset in new[]{false,true})
        using(var f=new AtomicFixture())
        {
            using var deadline=f.AtomicDeadline();
            ulong pc=AtomicFixture.Base,data=pc+8192;AtomicFamilySetup(f,pc,data);
            WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xb1,0x0b]);WriteControllerCode(f,pc+64,[0x0f,0x05]);
            byte* page=(byte*)f.Physical(data);((uint*)page)[0]=0;
            ((ulong*)(page+256))[0]=(ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds()+30;((ulong*)(page+256))[1]=0;
            using var enqueued=new ManualResetEventSlim();
            // Signal/timeout snapshots also end read scopes. Observe the real
            // futex registry after the bounded scope ends, not a guessed count.
            f.AtomicCopyReadEnded=ptr=>{if(ptr==(nint)f.Machine(1)&&((ulong*)((byte*)ptr+24))[0]==202&&
                AtomicFamilyFutexEnqueued(f)&&f.Jit.Authority.ActiveBorrows==0&&f.Jit.Authority.SemanticReadersDrained())enqueued.Set();};
            using var waiter=new AtomicFamilyThread(f,1,()=>f.AtomicSyscall(pc+64,1,202,data,bitset?0x189UL:128,0,bitset?data+256:0,0,uint.MaxValue));
            try
            {
                AtomicFamilyWait(enqueued,"R156 actual futex compare/enqueue scope has ended");
                // The event witnessed an actually enqueued waiter with no
                // permit held; later bounded signal polls may enter briefly.
                // The following CAS proves that none persists across sleep.
                Check(AtomicFamilyFutexEnqueued(f),"R156 futex remains enqueued before same-page CAS");
                AtomicFamilySeed(f,data+64);Write64((byte*)f.Machine(),24,((ulong*)(page+64))[0]);
                Check(f.Step(pc,signalActor:true)&&((ulong*)(page+64))[0]==5&&f.Statistics.NativeMemoryInstructions==1,
                    "R156 same-page successful CAS proceeds while real futex is blocked");
                f.AtomicSyscall(pc+64,0,202,data,129,1);waiter.Join();
            }
            finally{f.AtomicCopyReadEnded=null;f.Stop.RequestStop();}
            AtomicFamilyAssertClean(f);
        }
    }
    static void AtomicFamilySigactionCycle()
    {
        using var f=new AtomicFixture(workers:3);using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
        AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xc1,0x0b]);WriteControllerCode(f,pc+64,[0x0f,0x05]);
        using var holdsSignalLock=new ManualResetEventSlim();using var releaseA=new ManualResetEventSlim();using var admittedB=new ManualResetEventSlim();
        f.AtomicCopyWriteStarting=(m,_,_)=>{if(m!=(nint)f.Machine(1))return;holdsSignalLock.Set();AtomicFamilyWait(releaseA,"R156 sigaction old-action copy released behind pending");};
        using var action=new AtomicFamilyThread(f,1,()=>f.AtomicSyscall(pc+64,1,13,10,0,data,8));
        AtomicFamilyWait(holdsSignalLock,"R156 real rt_sigaction holds sig_lock at old-action copy");
        f.AtomicCopyReadStarted=m=>{if(m==(nint)f.Machine(2))admittedB.Set();};
        using var consumer=new AtomicFamilyThread(f,2,()=>f.AtomicCheckSignals(2));
        AtomicFamilyWait(admittedB,"R156 ConsumeSignal is admitted before attempting held sig_lock");
        f.Jit.Authority.TestReservationPublished=_=>releaseA.Set();
        try
        {
            AtomicFamilySeed(f,data+128);Check(f.Step(pc,signalActor:true),"R156 third-worker pending RMW permits both signal workers to finish");
            action.Join();consumer.Join();
            Check(((ulong*)((byte*)f.Machine(1)+24))[0]==0 && new ReadOnlySpan<byte>((byte*)f.Physical(data),32).IndexOfAnyExcept((byte)0)<0,
                "R156 sigaction old action and success result remain exact");
        }
        finally{releaseA.Set();f.AtomicCopyWriteStarting=null;f.AtomicCopyReadStarted=null;f.Jit.Authority.TestReservationPublished=null;}
        AtomicFamilyAssertClean(f);
    }
    static void AtomicFamilyRecursiveActor()
    {
        using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
        AtomicFamilySetup(f,pc,data);WriteControllerCode(f,pc,[0xf0,0x48,0x0f,0xc1,0x0b]);AtomicFamilySeed(f,data);
        f.DataScope(0,()=>{
            int depth=f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0));
            f.AtomicResumeActor(pc);
            Check(f.Completed==1&&f.Jit.Authority.CpuSemanticDepth(f.WorkerId(0))==depth&&f.Jit.Authority.SemanticAdmitted(f.WorkerId(0)),
                "R156 actual recursive SignalActor suspends/restores older CPU scopes around native RMW");
        });
        AtomicFamilyAssertClean(f);
    }
    static void AtomicFamilyBackingRead()
    {
        foreach(int route in new[]{0,1,2})foreach(int pairWidth in new[]{64,128})
        {
            using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
            int offset=pairWidth==128?64:4095;ulong address=data+(uint)offset;
            f.Map(pc);f.Fill(pc);int fd=f.Io.CreateMemoryFile("r156-backing-reader",0).Value;
            f.Io.WriteAt(fd,new byte[8192],0);f.AtomicMapSharedPage(data,fd,0);f.AtomicMapSharedPage(data+4096,fd,4096);
            WriteControllerCode(f,pc,AtomicEncoding(pairWidth==128?64:32,[0x0f,0xc7,0x0b]));
            AtomicFamilySeed(f,address);Write64((byte*)f.Machine(),24,0);Write64((byte*)f.Machine(),40,0);
            var mapping=f.Io.BeginMapping(fd).Value;
            using var attempted=new ManualResetEventSlim();using var rejected=new ManualResetEventSlim();using var start=new ManualResetEventSlim();
            // Read the whole backing in every route so crossing8B covers two
            // separately admitted page copies; only the atomic span is compared.
            byte[] bytes=new byte[8192];Exception? error=null;
            var reader=new Thread(()=>{try{AtomicFamilyWait(start,"R156 backing copy starts behind pending");attempted.Set();
                var result=route==0?f.Io.ReadAsync(fd,bytes,CancellationToken.None).GetAwaiter().GetResult():route==1?f.Io.ReadAt(fd,bytes,0):f.Io.MappingReadAt(mapping,bytes,0);
                Check(result.Value==8192,"R156 actual published two-page backing read completes");}catch(Exception e){error=e;}}){IsBackground=true};reader.Start();
            f.Jit.Authority.TestAtomicHostPending=()=>rejected.Set();
            // Rejection is nonblocking under sync; metadata on this thread must
            // remain callable while native preparation owns Authority.Gate.
            f.Jit.Authority.TestReservationPublished=_=>f.NativePrepared=p=>{
                start.Set();AtomicFamilyWait(attempted,"R156 published backing reader launched during ticket");
                AtomicFamilyWait(rejected,"R156 backing reader actually rejects closed pending admission");
                var metadata=Task.Run(()=>f.Io.ReadAtLength(fd));
                Check(metadata.Wait(TimeSpan.FromSeconds(5))&&metadata.Result.Value==8192,"R156 pending backing reader releases io.sync");
            };
            try
            {
                f.Step(pc,signalActor:true);Check(reader.Join(TimeSpan.FromSeconds(5))&&error==null,"R156 backing/native overlap joins");
                ulong low=pairWidth==128?address:((ulong)5<<32)|(uint)address;
                ulong high=pairWidth==128?5UL:0UL;
                Check(BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset))==low&&
                    BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(offset+8))==high,
                    "R156 backing copies observe crossing8B/aligned16B pair with width-aware trailing bytes");AtomicFamilyAssertClean(f);
            }
            finally{start.Set();f.Jit.Authority.TestReservationPublished=null;f.Jit.Authority.TestAtomicHostPending=null;f.Io.EndMapping(mapping);f.Io.Close(fd);}
        }
    }
    static void AtomicFamilySnapshotFailureQuota()
    {
        using var f=new AtomicFixture();using var deadline=f.AtomicDeadline();ulong pc=AtomicFixture.Base,data=pc+8192;
        AtomicFamilySetup(f,pc,data);
        var copies=(IHostGuestMemoryCopies)f.Owner;
        var marker=new InvalidOperationException("R156 actual copy failure");
        ulong retained=0;
        f.DataScope(0,()=>{
            ulong before=f.Owner.GuestCopyBytes;
            void* snapshot=f.Program.SchlepR(f.Machine(),(long)(data+4090),16);
            Check(snapshot!=null&&f.Owner.GuestCopyBytes>before,"R156 actual crossing owned snapshot/freelist charges");
            f.Program.CollectGarbage(f.Machine(),0);
            retained=f.Owner.GuestCopyBytes;
            Check(retained>=before,"R156 actual retained freelist capacity remains charged");
        });
        f.AtomicCopyReadStarted=_=>throw marker;
        try
        {
            try {f.DataScope(0,()=>{f.Program.SchlepR(f.Machine(),(long)data,16);});Check(false,"R156 actual copy exception must unwind");}
            catch(InvalidOperationException e)when(ReferenceEquals(e,marker)){}
        }
        finally {f.AtomicCopyReadStarted=null;f.DataScope(0,()=>f.Program.CollectGarbage(f.Machine(),0));}
        Check(f.Owner.GuestCopyBytes==retained,"R156 failed-copy snapshot ownership is reclaimable without quota leak");
        ulong reserve=AtomicFixture.MemoryLimit-retained;
        Check(copies.TryReserveGuestCopyBytes(reserve),"R156 owner quota filled to exact configured bound");
        try{f.DataScope(0,()=>Check(f.Program.SchlepR(f.Machine(),(long)data,16)==null,"R156 actual snapshot quota exhaustion returns allocation failure"));}
        finally{copies.ReleaseGuestCopyBytes(reserve);}
        Check(f.Owner.GuestCopyBytes==retained,"R156 failed snapshot allocation retires every actual permit and quota delta");
        AtomicFamilyAssertClean(f);
    }

    // Parent passes its once-compiled static guest ELF to this second entry
    // during the SAME family batch/build. This covers registered nested guest
    // handlers as well as the deterministic direct-Actor/controller schedules.
    static void CheckFullAtomicRegisteredR156(string guestElf)
    {
        byte[] image=File.ReadAllBytes(guestElf);const string path="/bin/r156-atomic-signal";
        foreach(var mode in new[]{GuestJitMode.Interpreter,GuestJitMode.Full})
        {
            var io=new InstanceIo(new Dictionary<string,ReadOnlyMemory<byte>>{[path]=image},executablePaths:new HashSet<string>{path});
            var stop=new HostExecutionStop(TimeSpan.FromSeconds(15));
            var owner=new ThreadedGuestExecution(io,stop,64UL<<20,2,jitOptions:new(){Mode=mode,CacheLimitBytes=8L<<20,AllowUnverifiedWindows=true});
            ThreadedGuestExecutionResult? result=null;Exception? error=null;bool joined=false;
            var thread=new Thread(()=>{try{result=owner.Run(path,[path],[],10_000_000);}catch(Exception e){error=e;}}){IsBackground=true};
            try
            {
                thread.Start();joined=thread.Join(TimeSpan.FromSeconds(20));
                if(!joined){stop.RequestStop();joined=thread.Join(TimeSpan.FromSeconds(5));}
                Check(joined,"R156 registered guest owner must join before resource release");
                if(error!=null)throw new AggregateException(error);
                Check(result!=null&&result.Exited&&result.ExitStatus==0&&result.AllWorkersJoined&&result.MemoryReleased&&owner.IsQuiescent,
                    "R156 actual registered nested-signal atomic fixture exits and releases owner");
                if(mode==GuestJitMode.Full)Check(result!.JitStatistics.InterpreterAttempts==0&&result.JitStatistics.NativeMemoryInstructions>0&&
                    result.JitStatistics.ActiveDataBorrows==0&&result.JitStatistics.DeferredAnonymousPages==0,
                    "R156 registered native signal fixture retains strict execution and data cleanup");
            }
            finally
            {
                // Failed-to-join owners intentionally retain their observer,
                // memory and stop state; this is not terminal recovery.
                if(joined){stop.Dispose();io.DisposeAsync().AsTask().GetAwaiter().GetResult();}
            }
        }
    }

}
