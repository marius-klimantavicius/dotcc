using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using StateRaw154 = Managed.Emulation.Execution.FullExtended80;

unsafe partial class Program
{
    static (ulong Sig,int Exponent) Canon154Normalized(StateRaw154 x)
    {
        int bits = (int)new BigInteger(x.Significand).GetBitLength();
        return (x.Significand<<(64-bits),Math.Max(1,x.SignExponent&0x7fff)-16383-63+bits-1);
    }
    static (StateRaw154 Value,uint Delta) Canon154Range(ulong sig,int exponent,bool negative,ushort cw,uint delta)
    {
        ushort sign = negative?(ushort)0x8000:(ushort)0;
        if (exponent > 16383)
        {
            if ((cw&8) == 0)
            {
                int wrapped = exponent-24576;
                return (wrapped>16383?new(0x8000000000000000,(ushort)(0x7fff|sign)):
                    new(sig,(ushort)(wrapped+16383|sign)),delta|8);
            }
            int rc = cw>>10&3; bool inf = rc == 0 || rc == 1 && negative || rc == 2 && !negative;
            return (inf?new(0x8000000000000000,(ushort)(0x7fff|sign)):new(ulong.MaxValue,(ushort)(0x7ffe|sign)),
                delta|40u|(inf?512u:0));
        }
        if (exponent < -16382)
        {
            if ((cw&16) == 0)
            {
                int wrapped = exponent+24576;
                return (wrapped < -16382?new(0,sign):new(sig,(ushort)(wrapped+16383|sign)),delta|16);
            }
            int shift = -16382-exponent,rc = cw>>10&3;
            BigInteger q,rest,denominator;
            if (shift > 64) { q = 0; rest = sig; denominator = BigInteger.One<<65; }
            else { denominator = BigInteger.One<<shift; q = BigInteger.DivRem(sig,denominator,out rest); }
            if (!rest.IsZero)
            {
                bool up = rc == 0 ? 2*rest > denominator || 2*rest == denominator && !q.IsEven :
                    rc == 1 ? negative : rc == 2 && !negative;
                if (up) ++q;
                delta |= 48u|(up?512u:0);
            }
            return (new((ulong)q,(ushort)((q >= (BigInteger.One<<63)?1:0)|sign)),delta);
        }
        return (new(sig,(ushort)(exponent+16383|sign)),delta);
    }
    static (StateRaw154 Value,uint Delta,uint Conditions,uint Mask,bool Write) Canon154ReductionReference(
        int fop,StateRaw154 a,StateRaw154 b,ushort cw)
    {
        int ac = Canon154Class(a),bc = Canon154Class(b); bool scale = fop == 0x1fd;
        bool signA = (a.SignExponent&0x8000) != 0,signB = (b.SignExponent&0x8000) != 0;
        var indefinite = new StateRaw154(0xc000000000000000,0xffff);
        if (ac == 5 || bc == 5) return (indefinite,1,0,0x200,(cw&1) != 0);
        if (ac is 3 or 4 || bc is 3 or 4)
        {
            bool left = ac == 3 ? bc != 3 || a.Significand >= b.Significand : bc == 3 ? false :
                ac == 4 && (bc != 4 || a.Significand >= b.Significand);
            StateRaw154 x = left?a:b; uint ie = ac == 4 || bc == 4?1u:0;
            return (new(x.Significand|0x4000000000000000,x.SignExponent),ie,0,0x200,(ie&~cw&1) == 0);
        }
        bool invalid = scale ? bc == 2 && (signB?ac==2:ac==0) : ac == 2 || bc == 0;
        if (invalid) return (indefinite,1,0,0x200,(cw&1) != 0);
        uint de = ac == 6 || bc == 6?2u:0;
        if ((de&~cw) != 0) return (default,2,0,0x200,false);
        if (scale)
        {
            if (ac == 2) return (a,de,0,0x200,true);
            if (bc == 2) return (new(signB?0:0x8000000000000000,(ushort)((signA?0x8000:0)|(signB?0:0x7fff))),de,0,0x200,true);
            if (ac == 0) return (a,de,0,0x200,true);
            var x = Canon154Normalized(a); int factor = 0;
            if (bc != 0)
            {
                var y = Canon154Normalized(b);
                if (y.Exponent >= 16) factor = 65536;
                else if (y.Exponent >= 0) factor = (int)(new BigInteger(y.Sig)>>(63-y.Exponent));
                if (signB) factor = -factor;
            }
            var r = Canon154Range(x.Sig,x.Exponent+factor,signA,cw,de);
            return (r.Value,r.Delta,0,0x200,true);
        }
        if (ac == 0) return (a,de,0,0x4700,true);
        var an = Canon154Normalized(a);
        if (bc == 2)
        {
            var r = Canon154Range(an.Sig,an.Exponent,signA,cw,de);
            return (r.Value,r.Delta,0,0x4700,true);
        }
        var bn = Canon154Normalized(b); int difference = an.Exponent-bn.Exponent;
        BigInteger av = BigInteger.Abs(Canon154Units(a)),bv = BigInteger.Abs(Canon154Units(b));
        BigInteger q,rem; uint conditions; bool negative;
        if (difference >= 64)
        {
            int n = 32+(difference%32),shift = difference-n;
            q = av/(bv<<shift); rem = av-((q*bv)<<shift); conditions = 0x400; negative = signA;
        }
        else
        {
            q = BigInteger.DivRem(av,bv,out rem);
            if (fop == 0x1f5 && (2*rem > bv || 2*rem == bv && !q.IsEven)) { ++q; rem -= bv; }
            negative = rem.Sign < 0 ? !signA : signA; rem = BigInteger.Abs(rem);
            BigInteger signedQ = signA != signB?-q:q; int low3 = (int)((signedQ%8+8)%8);
            conditions = (uint)((low3&4)<<6|(low3&2)<<13|(low3&1)<<9);
        }
        if (rem.IsZero) return (new(0,signA?(ushort)0x8000:(ushort)0),de,conditions,0x4700,true);
        int bits = (int)rem.GetBitLength(),exponent = bits-1-16445; ulong sig;
        if (bits > 64)
        {
            BigInteger quotient = BigInteger.DivRem(rem,BigInteger.One<<(bits-64),out var tail);
            Check(tail.IsZero,"R154 exact mathematical remainder fits full64 significand"); sig = (ulong)quotient;
        }
        else sig = (ulong)(rem<<(64-bits));
        var range = Canon154Range(sig,exponent,negative,cw,de);
        Check((range.Delta&0x228) == 0,"R154 remainder is exact, no OE/PE/numeric-roundup");
        return (range.Value,range.Delta,conditions,0x4700,true);
    }
    static void Canon154ReductionLiterals(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach ((byte modrm,StateRaw154 a,StateRaw154 b,ushort cw,StateRaw154 result,uint delta,uint cc) in
            new (byte,StateRaw154,StateRaw154,ushort,StateRaw154,uint,uint)[] {
                (0xfd,new(0x8000000000000001,0x3fff),new(0xa000000000000000,0xc000),0x17f,new(0x8000000000000001,0x3ffd),0,0),
                (0xfd,new(0x8000000000000000,0x3fff),new(0x8000000000000000,0x400f),0x377,new(0x8000000000000000,0x7fff),8,0),
                (0xfd,new(0x8000000000000000,0xbfff),new(0x8000000000000000,0xc00f),0x36f,new(0,0x8000),16,0),
                (0xf8,new(0xe000000000000000,0x4001),new(0x8000000000000000,0x4000),0x17f,new(0x8000000000000000,0x3fff),0,0x4200),
                (0xf5,new(0xe000000000000000,0x4001),new(0x8000000000000000,0x4000),0x17f,new(0x8000000000000000,0xbfff),0,0x100),
                (0xf8,new(0xe000000000000000,0xc001),new(0x8000000000000000,0x4000),0x17f,new(0x8000000000000000,0xbfff),0,0x300) })
        {
            var i = Canon154Classify([0xd9,modrm]); var call = Canon154Call(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            Canon154Seed(fake,state,7,cw); Canon154Input(fake,state,0,a); Canon154Input(fake,state,1,b);
            var reference = Canon154ReductionReference((int)i.Extra.Immediate,a,b,cw);
            Check(reference.Value.Significand == result.Significand && reference.Value.SignExponent == result.SignExponent &&
                reference.Delta == delta && reference.Conditions == cc,"R154 independent scale/remainder primary literal anchor");
            Canon154One(code,i,fake,state,true);
        }
        foreach (byte modrm in new byte[] { 0xfd,0xf8,0xf5 })
        {
            var i = Canon154Classify([0xd9,modrm]); var call = Canon154Call(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (var a in Canon154Values())
            foreach (var b in Canon154Values())
            foreach (ushort cw in new ushort[] { 0x37f,0x37e,0x37d,0x36f,0x377,0x17f })
            {
                Canon154Seed(fake,state,6,cw); Canon154Input(fake,state,0,a); Canon154Input(fake,state,1,b);
                Canon154One(code,i,fake,state);
            }
        }
        Canon154ReductionRegressions(fake,state,calls);
    }

    // Coherent reduction regressions, reached through the existing family
    // suite on both scaffolds. Literal result/status checks supplement the
    // independent whole-Machine rational oracle; no product enum drives it.
    static void Canon154ReductionRegressions(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (byte modrm in new byte[] { 0xf8,0xf5 })
        {
            var i=Canon154Classify([0xd9,modrm]); var call=Canon154Call(i,calls);
            using var code=CreateFullBlock([new(i,X87Kernel:call)]);
            for (int masks=0; masks<64; ++masks)
            for (int pc=0; pc<4; ++pc)
            for (int rc=0; rc<4; ++rc)
            for (int signs=0; signs<4; ++signs)
            {
                // c78 row12: D16445,N61,q2^61,r2^61 -> +/-2^-63.
                // DE is diagnostic even when it suppresses architectural
                // completion. No UE/PE/wrapped image can occur in this case.
                var a=new StateRaw154(0x8000000000000001,(ushort)(0x3fff|((signs&1)<<15)));
                var b=new StateRaw154(1,(ushort)((signs>>1)<<15));
                Canon154Seed(fake,state,(masks+pc+rc)&7,(ushort)(0x40|masks|pc<<8|rc<<10));
                Canon154Input(fake,state,0,a); Canon154Input(fake,state,1,b);
                var reference=Canon154ReductionReference((int)i.Extra.Immediate,a,b,0x37f);
                Check(reference.Value.Significand == 0x8000000000000000 &&
                    reference.Value.SignExponent == (ushort)(0x3fc0|((signs&1)<<15)) &&
                    reference.Delta == 2 && reference.Conditions == 0x400,
                    "R154 independent c78 partial remainder is exact normal2^-63 with DE and C2");
                Canon154One(code,i,fake,state);
                var w=Canon154Work(state);
                Check(w->Normal.Significand == 0x8000000000000000 &&
                    w->Normal.SignExponent == (ushort)(0x3fc0|((signs&1)<<15)) && w->NormalStatus == 2 &&
                    w->WrappedStatus == 0 && w->Wrapped.Significand == 0 && w->Wrapped.SignExponent == 0 &&
                    (uint)w->Classification == 0x31,
                    "R154 both remainder forms all masks/PC/RC keep exact candidate; never false UE/PE/range");
                Check(BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>((byte*)w+240,2)) == ((masks&2) == 0?0:0x400) &&
                    BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>((byte*)w+242,2)) == ((masks&2) == 0?0x200:0x4700),
                    "R154 unmasked DE suppresses quotient CC/C1 independently of exact masked candidate");
            }
            // All partial choices N32..63, two adjacent large-gap bands, no
            // DE operand: each exact residual is again2^-63. This detects
            // exponent corruption independently from the pre-DE path.
            for (int difference=64; difference<128; ++difference)
            foreach (int signs in new[] { 0,1,2,3 })
            foreach (ushort cw in new ushort[] { 0x40,0x17f,0x77f,0xb7f,0xf7f })
            {
                var a=new StateRaw154(0x8000000000000001,(ushort)(0x3fff|((signs&1)<<15)));
                var b=new StateRaw154(0x8000000000000000,(ushort)((16383-difference)|((signs>>1)<<15)));
                Canon154Seed(fake,state,difference&7,cw); Canon154Input(fake,state,0,a); Canon154Input(fake,state,1,b);
                Canon154One(code,i,fake,state);
                var w=Canon154Work(state);
                bool exactZero = (difference&31) == 31; // N63: q=2^63+1 removes the final bit too.
                Check(w->Normal.Significand == (exactZero?0UL:0x8000000000000000UL) &&
                    w->Normal.SignExponent == (ushort)((exactZero?0:0x3fc0)|((signs&1)<<15)) && w->NormalStatus == 0 &&
                    w->WrappedStatus == 0 && (uint)w->Classification == 0x11 &&
                    BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>((byte*)w+240,2)) == 0x400 && BinaryPrimitives.ReadUInt16LittleEndian(new ReadOnlySpan<byte>((byte*)w+242,2)) == 0x4700,
                    "R154 all32 partial widths preserve EA-N and full64 exact normalization/C2 without RC/PC effects");
            }
            // Complete quotient low3, nearest-even ties and remainder sign:
            // exact (q+.5)/1 distinguishes both algorithms at every q mod8.
            for (int quotient=0; quotient<16; ++quotient)
            for (int signs=0; signs<4; ++signs)
            foreach (ushort cw in new ushort[] { 0x40,0x17f,0x77f,0xb7f,0xf7f })
            {
                var a=Canon154Integer(2*quotient+1,(signs&1) != 0); --a.SignExponent;
                var b=new StateRaw154(0x8000000000000000,(ushort)(0x3fff|((signs>>1)<<15)));
                Canon154Seed(fake,state,quotient&7,cw); Canon154Input(fake,state,0,a); Canon154Input(fake,state,1,b);
                Canon154One(code,i,fake,state);
                int q=quotient+(modrm == 0xf5 && (quotient&1) != 0?1:0);
                int low=((signs&1) != (signs>>1)?-q:q)&7;
                uint cc=(uint)((low&4)<<6|(low&2)<<13|(low&1)<<9);
                Check((((Blink.Machine*)fake)->fpu.sw&0x4700) == cc,
                    "R154 complete C0/C3/C1 are signed quotient mod8; C2 clear and nearest ties ignore RC");
            }
            // Min-normal adjacent operands produce exact min-subnormal.
            // Masked UE is absent; unmasked UE selects the exact wrapped
            // result, while PE and numeric-roundup remain impossible.
            for (int masks=0; masks<64; ++masks)
            for (int rc=0; rc<4; ++rc)
            {
                Canon154Seed(fake,state,3,(ushort)(0x40|masks|rc<<10));
                Canon154Input(fake,state,0,new(0x8000000000000001,1));
                Canon154Input(fake,state,1,new(0x8000000000000000,1));
                Canon154One(code,i,fake,state);
                var w=Canon154Work(state);
                Check(w->Normal.Significand == 1 && w->Normal.SignExponent == 0 && w->NormalStatus == 0 &&
                    w->WrappedStatus == 16 && (uint)w->Classification == 0x17,
                    "R154 exact tiny remainder has no PE; UE wrapping separate from masked normal status");
            }
        }
        var scale=Canon154Classify([0xd9,0xfd]); var scaleCall=Canon154Call(scale,calls);
        using var scaleCode=CreateFullBlock([new(scale,X87Kernel:scaleCall)]);
        foreach ((StateRaw154 left,int factor) in new (StateRaw154,int)[] {
            (new(0x8000000000000001,1),-1),(new(0x8000000000000001,1),-64),
            (new(0x8000000000000000,1),-1),(new(ulong.MaxValue,0x7ffe),1),
            (new(0x8000000000000001,0x3fff),65535),(new(0x8000000000000001,0x3fff),65536),
            (new(0x8000000000000001,0x3fff),-65535),(new(0x8000000000000001,0x3fff),-65536) })
        for (int masks=0; masks<64; ++masks)
        for (int pc=0; pc<4; ++pc)
        for (int rc=0; rc<4; ++rc)
        foreach (int sign in new[] { 0,0x8000 })
        {
            Canon154Seed(fake,state,(masks+pc+rc)&7,(ushort)(0x40|masks|pc<<8|rc<<10));
            var a=new StateRaw154(left.Significand,(ushort)(left.SignExponent|sign));
            var b=Canon154Integer(BigInteger.Abs(factor),factor<0);
            Canon154Input(fake,state,0,a); Canon154Input(fake,state,1,b); Canon154One(scaleCode,scale,fake,state);
            Check(((uint)Canon154Work(state)->NormalStatus&~0x238u) == 0,
                "R154 shared scale range/RC/C1 protocol has no spurious pre-operation exception on finite normal operands");
        }
    }

    // Same family entry calls this table. Independent byte-image oracle;
    // never invokes FullX87StateBridge/HostX87Transfer/semantic Op wrappers.
    readonly record struct Canon154StateRow(byte[] Bytes,int Size,int Kind,bool Load,bool Small=false,bool Wide=false,bool Waiting=false);
    static Canon154StateRow[] Canon154StateRows() => [
        new([0xd9,0x28],2,0,true,Waiting:true),new([0xd9,0x38],2,1,false),new([0xdd,0x38],2,2,false),
        new([0xd9,0x20],28,3,true,Waiting:true),new([0xd9,0x30],28,4,false),new([0x66,0xd9,0x30],14,4,false,true),
        new([0xdd,0x20],108,5,true,Waiting:true),new([0xdd,0x30],108,6,false),new([0x66,0xdd,0x30],94,6,false,true),
        new([0x0f,0xae,0x00],512,7,false),new([0x48,0x0f,0xae,0x00],512,7,false,Wide:true),
        new([0x0f,0xae,0x08],512,8,true),new([0x48,0x0f,0xae,0x08],512,8,true,Wide:true),
        // R163: one state-family table covers the admitted ignored REP and
        // effective REX.W formats through the SAME native/fault/ticket suites.
        new([0xf2,0xd9,0x28],2,0,true,Waiting:true),new([0xf3,0xd9,0x38],2,1,false),
        new([0xf2,0xdd,0x38],2,2,false),
        new([0xf2,0xd9,0x20],28,3,true,Waiting:true),
        new([0xf3,0x66,0xd9,0x30],14,4,false,true),
        new([0xf3,0xdd,0x20],108,5,true,Waiting:true),
        new([0xf2,0x66,0xdd,0x30],94,6,false,true),
        new([0x66,0x48,0xd9,0x20],28,3,true,Waiting:true),
        new([0x66,0x48,0xdd,0x20],108,5,true,Waiting:true),
        new([0x66,0x48,0xd9,0x30],28,4,false),new([0x66,0x48,0xdd,0x30],108,6,false),
        new([0x48,0x66,0xd9,0x30],14,4,false,true),new([0x48,0x66,0xdd,0x30],94,6,false,true),
        new([0xf3,0x0f,0xae,0x00],512,7,false),
        new([0xf2,0x48,0x0f,0xae,0x08],512,8,true,Wide:true),
        // R163r1 closes the former omitted-FOP cell ONLY for the authorized
        // deterministic Linux x86-64 virtual-CPU profile (small restore FOP0).
        new([0x66,0xd9,0x20],14,3,true,true,Waiting:true),
        new([0x66,0xdd,0x20],94,5,true,true,Waiting:true),
        new([0xf2,0x66,0xd9,0x20],14,3,true,true,Waiting:true),
        new([0xf3,0x66,0xdd,0x20],94,5,true,true,Waiting:true),
        new([0x67,0x66,0xd9,0x20],14,3,true,true,Waiting:true),
        new([0x67,0x66,0xdd,0x20],94,5,true,true,Waiting:true)];

    static void Canon154StateSuite(byte* fake,FullX87RegisterFile* state)
    {
        byte* pages = (byte*)NativeMemory.AllocZeroed(12288);
        Check(pages != null,"R154 state discontiguous mapping fixture");
        try
        {
            foreach (bool shared in new[] { false,true })
            {
                NativeCodeAllocation? common = null;
                if (shared) { common = CreateFullScaffold(out var addresses); fullOwnerScaffold = addresses; }
                try
                {
                    foreach (var row in Canon154StateRows())
                    {
                        Console.WriteLine($"STATE-FORM shared={shared} bytes={Convert.ToHexString(row.Bytes)} size={row.Size} load={row.Load}");
                        var i = Canon154Classify(row.Bytes);
                        Check(i.SourceWidth == (row.Size==512?416:row.Size)*8 && !FullBlockEmitter.CanonicalX87ControlNeedsKernel(i) &&
                            !FullBlockEmitter.CanonicalX87ControlRequiresArithmeticPrecision(i),"R154 state raw boundary/no fake numeric binding/no PC rejection");
                        using var code = CreateFullBlock([new(i)]);
                        for (int top = 0; top < 8; ++top)
                        for (int mask = 0; mask < 64; ++mask)
                        foreach (int offset in row.Size == 512?new[] { 16,4080 }:new[] { 0,1,4095 })
                        {
                            new Span<byte>(pages,12288).Fill(0x5a);
                            Canon154Seed(fake,state,top,0x37f);
                            Write64(fake,24,0x100000u+(uint)offset);
                            byte[] image = row.Load?Canon154StateInput(row,top,mask):new byte[row.Size];
                            if (row.Load) Canon154StateCopyIn(pages,offset,image);
                            if (top == 0 && mask == 0) Console.WriteLine($"STATE-FIRST offset={offset}");
                            Canon154StateOne(code,row,i,fake,state,pages,offset);
                        }
                        Console.WriteLine("STATE-PHASE faults");
                        Canon154StateFaults(code,row,i,fake,state,pages);
                        Console.WriteLine("STATE-PHASE ticket");
                        Canon154StateTicket(code,row,i,fake,state,pages);
                        Console.WriteLine("STATE-DONE ticket");
                        if (row.Size == 512) { Console.WriteLine("STATE-PHASE FX permission tail"); Canon154StateFxPermissionTail(code,row,i,fake,state,pages); }
                    }
                    Canon163StateByteLiterals(fake,state,pages);
                    Canon163StateSaveRestoreLiterals(fake,state,pages);
                    Canon163RegisterStatePrefixes(fake,state);
                    Canon163StateCanonicalFaults(fake,state);
                }
                finally { fullOwnerScaffold = default; common?.Dispose(); }
            }
            Canon154StateDecodeAndEmission();
            Canon154StateDeferred(fake,state,pages);
            Canon163LegacyStateMmu();
        }
        finally { NativeMemory.Free(pages); }
    }
    // Leaf tests publish a literal authenticated response after checking the
    // native request. Actual PTE/permission/EOF resolution is tested below
    // through the existing parent MMU, separately from this fixture publisher.
    static void Canon154StateProofReady(FullBlockFrame* f,in JitInstruction i,ulong epoch,bool actual=false)
    {
        var d = f->StackMemory;
        if (!actual)
        {
            Check(FullNativeMemory.AuthenticateCanonicalX87MemoryProbe(f,in i,(Blink.Machine*)f->Machine),
                "R154 exact typed FX512 initial permission request authentication");
            Check(d->Request0.Width == 4096 && f->MemoryWidth == 4096 && d->Request0.Host0 == null &&
                d->Request0.Host1 == null && d->Ticket == 0 && f->MemoryTicket == 0 && f->MemoryFlags == FullMemoryFlags.None,
                "R154 proof alone grants no physical data pointers/ticket");
        }
        else
        {
            Check(f->MemoryWidth == 3328 && f->MemoryResume == 1,"R154 composite retains actual416 request summary");
            *d = new() { Version=FullCanonicalX87MemoryLowering.MemoryProbeTag,Count=1,Site=f->MemorySite,
                Request0=new() { Address=f->MemoryAddress,Access=f->MemoryAccess,Width=4096 } };
        }
        d->Epoch=epoch; d->State=FullStackMemoryState.Ready;
        FullNativeMemory.ResumeSameIssue(f);
    }

    static void Canon154StateProbeMmu()
    {
        foreach (var row in Canon154StateRows())
        {
            if (row.Size != 512) continue;
            var i = Canon154Classify(row.Bytes);
            foreach (int cut in new[] { 416,464,512 })
            foreach (string kind in new[] { "ok","missing","readonly","lazy","eof" })
            {
                using var fixture = new ThreadedGuestExecution.FullControllerFixture();
                const ulong guest = ThreadedGuestExecution.FullControllerFixture.Base;
                fixture.Map(guest); fixture.Fill(guest);
                if (kind is "ok" or "readonly") { fixture.Map(guest+4096); fixture.Fill(guest+4096); }
                if (kind == "readonly") fixture.DataScope(0, () =>
                    Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,
                        (long)(guest+4096),4096,1,false) == 0,"R154 FX tail read-only setup"));
                if (kind == "lazy") fixture.Map(guest+4096);
                if (kind == "eof")
                {
                    var file = fixture.Io.CreateMemoryFile("r154-fx-tail-eof",0);
                    Check(file.Succeeded && fixture.Io.TruncateFile(file.Value,4096).Succeeded,"R154 FX EOF setup");
                    fixture.MapShared(file.Value,guest+4096); fixture.Fill(guest+4096);
                    Check(fixture.Io.TruncateFile(file.Value,0).Succeeded,"R154 FX EOF withdraw");
                }
                var machine = fixture.Machine(); machine->ip=i.Ip;
                Write64((byte*)machine,24,guest+4096-(uint)cut);
                var access = row.Load?FullMemoryAccess.Read:FullMemoryAccess.Write;
                var descriptor = new FullStackMemoryDescriptor {
                    Version=FullCanonicalX87MemoryLowering.MemoryProbeTag,Count=1,State=FullStackMemoryState.Request,
                    Request0=new() { Address=guest+4096-(uint)cut,Access=access,Width=4096 } };
                FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                tlb[7].Host=(byte*)1; tlb[7].Permissions=FullMemoryAccess.Read;
                byte[] oldTlb=CanonicalX87Bytes((byte*)tlb,64*64);
                var f = StackMemoryMmuRequest(&descriptor,tlb); f.Machine=machine; f.MemoryAccess=access;
                Check(FullNativeMemory.AuthenticateCanonicalX87MemoryProbe(&f,in i,machine),"R154 FX exact4096 proof authenticates actual3328 typed operation");
                var result = FillFullMemoryMmu(fixture,&f);
                bool crosses = cut<512,readOnlyOK = row.Load && kind == "readonly";
                var expected = !crosses || kind == "ok" || readOnlyOK?FullMemoryAssistResult.Resolved:
                    kind == "lazy"?FullMemoryAssistResult.DemandPage:FullMemoryAssistResult.GuestFault;
                Check(result == expected,"R154 full512 permission before defined416 effects; FXRSTOR accepts readable tail");
                Check(descriptor.Request0.Host0 == null && descriptor.Request0.Host1 == null && descriptor.Request0.Length0 == 0 &&
                    descriptor.Ticket == 0 && f.MemoryTicket == 0 && f.MemoryFlags == FullMemoryFlags.None &&
                    new ReadOnlySpan<byte>(tlb,64*64).SequenceEqual(oldTlb),"R154512 proof publishes no data capability/TLB/ticket");
                if (result == FullMemoryAssistResult.GuestFault)
                    Check(f.FaultTrap == 14 && f.FaultAddress == guest+4096 &&
                        f.FaultCode == (kind == "readonly"?2u:kind == "eof"?unchecked((uint)-2):1u),
                        "R154 FX tail PF/BUS address/code before invalid image or state writes");
                else if (result == FullMemoryAssistResult.Resolved)
                    Check(descriptor.State == FullStackMemoryState.Ready && descriptor.Epoch == f.ObservedGlobalEpoch,
                        "R154 proof Ready publishes final epoch only");
                // Existing suppressed numeric stores remain WRITE-only at
                // widths16/32/64/80. A read proof is admitted only for exact FX.
                if (row.Load)
                {
                    descriptor.State=FullStackMemoryState.Request; descriptor.Request0.Width=80; f.MemoryWidth=80;
                    Check(!FullNativeMemory.AuthenticateCanonicalX87MemoryProbe(&f,in i,machine),"R154 no broad read-proof admission for old numeric widths");
                }
            }
        }
    }

    static byte[] Canon154StateInput(Canon154StateRow row,int top,int mask)
    {
        byte[] b = new byte[row.Size]; b.AsSpan().Fill(0xa5);
        ushort cw = (ushort)(0x40|mask|(mask&3)<<8|(mask>>2&3)<<10);
        BinaryPrimitives.WriteUInt16LittleEndian(b,cw);
        if (row.Kind == 0) return b;
        bool fx = row.Kind == 8;
        ushort sw = (ushort)(0xc7ff|(top<<11)); sw = (ushort)((sw&~0x3800)|(top<<11));
        // ES1/B1 and deliberately inconsistent ES0/B1 rows prove B reflects ES.
        if ((mask&1) == 0) sw &= 0xff7f;
        BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(fx?2:row.Small?2:4),sw);
        if (fx) b[4] = (byte)(0xa5^mask);
        else BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(row.Small?4:8),(ushort)(0x7b1e^mask*13));
        if (!row.Small) BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(fx?6:18),0xf765);
        if (fx && row.Wide)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(8),0x1234567887654321);
            BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(16),0xabcdef01fedcba98);
        }
        else if (row.Small)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(6),0x7788);
            BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(8),0x3344);
            BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(10),0x99aa);
            BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(12),0x5566);
        }
        else
        {
            BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(fx?8:12),0x87654321);
            BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(fx?12:16),0x23);
            BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(fx?16:20),0xfedcba98);
            BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(fx?20:24),0x2b);
        }
        if (fx) BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(24),(uint)(0x1f80|mask));
        if (row.Kind is 5 or 8)
        {
            var values = Canon154Values();
            for (int logical = 0; logical < 8; ++logical)
            {
                var x = values[(logical+mask)%values.Length]; int at = (fx?32:row.Small?14:28)+logical*(fx?16:10);
                BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at),x.Significand);
                BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at+8),x.SignExponent);
            }
        }
        return b;
    }
    static void Canon154StateCopyIn(byte* pages,int offset,byte[] image)
    {
        for (int b = 0; b < image.Length; ++b)
        { int at = offset+b; pages[at<4096?at:at+4096] = image[b]; }
    }
    static byte[] Canon154StatePages(byte* pages)
    {
        byte[] b = new byte[8192]; new ReadOnlySpan<byte>(pages,4096).CopyTo(b);
        new ReadOnlySpan<byte>(pages+8192,4096).CopyTo(b.AsSpan(4096)); return b;
    }
    static ushort Canon154StateU16(byte[] b,int p) => BinaryPrimitives.ReadUInt16LittleEndian(b.AsSpan(p));
    static void Canon154StateW16(byte[] b,int p,ushort x) => BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(p),x);
    static void Canon154StateExpected(Canon154StateRow row,JitInstruction i,byte[] m,byte[] raw,byte[] page,int offset)
    {
        byte[] b = page.AsSpan(offset,row.Size).ToArray(); bool fx = row.Kind is 7 or 8;
        if (row.Kind == 0) CanonicalX87W32(m,672,Canon154StateU16(b,0));
        else if (row.Kind is 1 or 2) Canon154StateW16(b,0,(ushort)CanonicalX87U32(m,row.Kind==1?672:648));
        else if (row.Load)
        {
            ushort incoming = Canon154StateU16(b,fx?2:row.Small?2:4);
            uint sw = (uint)((incoming&0x7fff)|((incoming&0x80)<<8)); int top = (int)(sw>>11)&7;
            uint present = fx?b[4]:0u;
            if (!fx)
                for (int p = 0; p < 8; ++p) if ((Canon154StateU16(b,row.Small?4:8)>>(2*p)&3) != 3) present |= 1u<<p;
            if (row.Kind is 5 or 8)
            {
                for (int n = 0; n < 8; ++n)
                {
                    int at = (fx?32:row.Small?14:28)+n*(fx?16:10), p = (top+n)&7;
                    raw.AsSpan(64+p*16,16).Clear(); b.AsSpan(at,10).CopyTo(raw.AsSpan(64+p*16));
                }
            }
            uint tags = 0;
            for (int p = 0; p < 8; ++p) tags |= (uint)((present&(1u<<p)) == 0?3:Canon154Tag(Canon154Read(raw,p)))<<(2*p);
            CanonicalX87W32(m,672,Canon154StateU16(b,0)); CanonicalX87W32(m,648,sw); CanonicalX87W32(m,652,tags);
            CanonicalX87W32(m,656,row.Small?0u:(uint)(Canon154StateU16(b,fx?6:18)&0x7ff));
            CanonicalX87W64(m,664,fx&&row.Wide?BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(8)):row.Small?Canon154StateU16(b,6):CanonicalX87U32(b,fx?8:12));
            CanonicalX87W64(m,680,fx&&row.Wide?BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(16)):row.Small?Canon154StateU16(b,10):CanonicalX87U32(b,fx?16:20));
            Canon154StateW16(raw,40,fx&&row.Wide?(ushort)0:Canon154StateU16(b,fx?12:row.Small?8:16));
            Canon154StateW16(raw,42,fx&&row.Wide?(ushort)0:Canon154StateU16(b,fx?20:row.Small?12:24));
            if (fx)
            { CanonicalX87W32(m,688,CanonicalX87U32(b,24)); b.AsSpan(160,256).CopyTo(m.AsSpan(160)); }
        }
        else
        {
            ushort sw = (ushort)CanonicalX87U32(m,648); int top = sw>>11&7;
            Canon154StateW16(b,0,(ushort)CanonicalX87U32(m,672)); Canon154StateW16(b,fx?2:row.Small?2:4,sw);
            if (fx)
            {
                b[4] = 0; b[5] = 0;
                for (int p = 0; p < 8; ++p) if ((CanonicalX87U32(m,652)>>(2*p)&3) != 3) b[4] |= (byte)(1<<p);
                Canon154StateW16(b,6,(ushort)(CanonicalX87U32(m,656)&0x7ff));
                if (row.Wide)
                { m.AsSpan(664,8).CopyTo(b.AsSpan(8)); m.AsSpan(680,8).CopyTo(b.AsSpan(16)); }
                else
                {
                    m.AsSpan(664,4).CopyTo(b.AsSpan(8)); raw.AsSpan(40,2).CopyTo(b.AsSpan(12)); b.AsSpan(14,2).Clear();
                    m.AsSpan(680,4).CopyTo(b.AsSpan(16)); raw.AsSpan(42,2).CopyTo(b.AsSpan(20)); b.AsSpan(22,2).Clear();
                }
                m.AsSpan(688,4).CopyTo(b.AsSpan(24)); CanonicalX87W32(b,28,0xffff);
                for (int n = 0; n < 8; ++n)
                { int at = 32+n*16; b.AsSpan(at,16).Clear(); raw.AsSpan(64+((top+n)&7)*16,10).CopyTo(b.AsSpan(at)); }
                m.AsSpan(160,256).CopyTo(b.AsSpan(160)); // original software tail416..511 retained
            }
            else
            {
                Canon154StateW16(b,row.Small?4:8,(ushort)CanonicalX87U32(m,652));
                m.AsSpan(664,row.Small?2:4).CopyTo(b.AsSpan(row.Small?6:12)); raw.AsSpan(40,2).CopyTo(b.AsSpan(row.Small?8:16));
                m.AsSpan(680,row.Small?2:4).CopyTo(b.AsSpan(row.Small?10:20)); raw.AsSpan(42,2).CopyTo(b.AsSpan(row.Small?12:24));
                if (!row.Small)
                {
                    Canon154StateW16(b,18,(ushort)(CanonicalX87U32(m,656)&0x7ff));
                    foreach (int at in new[] { 2,6,10,26 }) Canon154StateW16(b,at,0xffff);
                }
                if (row.Kind == 6)
                {
                    for (int n = 0; n < 8; ++n) raw.AsSpan(64+((top+n)&7)*16,10).CopyTo(b.AsSpan((row.Small?14:28)+n*10));
                    CanonicalX87W32(m,648,0); CanonicalX87W32(m,652,0xffff); CanonicalX87W32(m,656,0);
                    CanonicalX87W32(m,672,0x37f); CanonicalX87W64(m,664,0); CanonicalX87W64(m,680,0); raw.AsSpan(40,4).Clear();
                }
                else CanonicalX87W32(m,672,(uint)((ushort)CanonicalX87U32(m,672)|63));
            }
        }
        if (!row.Load) b.CopyTo(page,offset);
        CanonicalX87W64(m,0,i.Ip+(uint)i.Length); m[8] = 0;
    }
    static void Canon154StateOne(NativeCodeAllocation code,Canon154StateRow row,JitInstruction i,byte* fake,
        FullX87RegisterFile* state,byte* pages,int offset)
    {
        byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw = CanonicalX87Bytes((byte*)state,192),page = Canon154StatePages(pages);
        Canon154StateExpected(row,i,m,raw,page,offset);
        ulong epoch = 11; long issued = 0,completed = 0; int stop = 0; var h = FullHeader(Ip,code.Entry);
        var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87 = state;
        FullStackMemoryDescriptor descriptor = default; f.StackMemory = &descriptor;
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        f.MemoryTlb = tlb; f.MemoryTlbMask = 63; SetFullMemoryEntry(tlb,0x100000,pages); SetFullMemoryEntry(tlb,0x101000,pages+8192);
        if (row.Size == 512)
        {
            Check(RunFull(code,&f) == FullBlockExit.NeedMemory && issued == 1 && completed == 0 && f.MemoryWidth == 4096,
                "R154 FX mandatory full512 proof even when actual416 TLB mappings are hot");
            Canon154StateProofReady(&f,in i,epoch);
        }
        Check(FloatRunProbe(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1 &&
            f.NativeMemoryComputations == 1 && f.MemoryAssists == 0 && f.HostMxcsrSaved == 0 && f.FaultTrap == 0 &&
            f.HelperIndex == FullBlockFrame.NoHelperIndex,"R154 native state copy completes once, no host FP/helper");
        CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),"R154 independent complete state memory image/page canaries");
    }
    static void Canon154StateFaults(NativeCodeAllocation code,Canon154StateRow row,JitInstruction i,
        byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        foreach (string why in new[] { "first-miss","second-miss","stop","attention","budget","preissued-stop","prior-mf",
            "bad-mxcsr","bad-mxcsr-second-miss","unaligned","readonly-second" })
        {
            if ((why is "bad-mxcsr" or "bad-mxcsr-second-miss") && row.Kind != 8 || why == "unaligned" && row.Size != 512 ||
                why == "readonly-second" && row.Load) continue;
            new Span<byte>(pages,12288).Fill(0x5a); Canon154Seed(fake,state,7,0x37f);
            int offset = why == "unaligned"?4081:why == "bad-mxcsr-second-miss"?4000:
                4096-Math.Min(16,Math.Max(1,row.Size/2)); Write64(fake,24,0x100000u+(uint)offset);
            if (row.Load)
            {
                var image = Canon154StateInput(row,3,7);
                if (why is "bad-mxcsr" or "bad-mxcsr-second-miss") CanonicalX87W32(image,24,0x10000);
                Canon154StateCopyIn(pages,offset,image);
            }
            if (why == "prior-mf") { ((Blink.Machine*)fake)->fpu.sw |= 1; ((Blink.Machine*)fake)->fpu.cw &= ~1u; }
            if (why == "attention") ((Blink.Machine*)fake)->attention = true;
            byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw = CanonicalX87Bytes((byte*)state,192),page = Canon154StatePages(pages);
            ulong epoch = 11; long issued = why == "preissued-stop"?1:0,completed = 0;
            int stop = why is "stop" or "preissued-stop"?1:0; var h = FullHeader(Ip,code.Entry);
            var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:why == "budget"?0UL:1UL,limit:1); f.X87 = state;
            if (why == "preissued-stop") f.FirstPreissued = 1;
            FullStackMemoryDescriptor descriptor = default; f.StackMemory = &descriptor;
            FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            f.MemoryTlb = tlb; f.MemoryTlbMask = 63;
            if (why != "first-miss") SetFullMemoryEntry(tlb,0x100000,pages);
            if (why is not ("first-miss" or "second-miss" or "bad-mxcsr-second-miss")) SetFullMemoryEntry(tlb,0x101000,pages+8192);
            if (why == "readonly-second") tlb[FullNativeMemory.TlbIndex(0x101000,63)].Permissions = FullMemoryAccess.Read;
            bool mf = why == "prior-mf" && row.Waiting;
            FullBlockExit want = why switch { "stop" or "preissued-stop" => FullBlockExit.Stop,
                "attention" => FullBlockExit.Attention,"budget" => FullBlockExit.Budget,
                "bad-mxcsr" or "unaligned" => FullBlockExit.GuestFault,
                _ => mf?FullBlockExit.GuestFault:FullBlockExit.NeedMemory };
            // Nonwaiting prior-MF rows with valid pages COMPLETE normally.
            bool works = why == "prior-mf" && !row.Waiting;
            if (works) { want = FullBlockExit.Continue; Canon154StateExpected(row,i,m,raw,page,offset); }
            if (row.Size == 512 && (why == "bad-mxcsr" || works))
            {
                Check(RunFull(code,&f) == FullBlockExit.NeedMemory && f.MemoryWidth == 4096,
                    "R154 full512 permission gate precedes bad MXCSR and nonwaiting state effects");
                Canon154StateProofReady(&f,in i,epoch);
            }
            Check(RunFull(code,&f) == want && completed == (works?1:0) && f.Executed == (works?1UL:0UL),
                "R154 state waiting/nonwaiting/fault/cancellation/memory priority before architectural copy");
            if (mf) Check(f.FaultTrap == 16 && f.FaultAddress == Ip,"R154 waiting state prior MF before memory");
            if (why is "bad-mxcsr" or "unaligned") Check(f.FaultTrap == 13 && f.FaultCode == 128 && f.FaultAddress == 0,
                "R154 FX invalid image/alignment GP0, Linux SI_KERNEL128, no mutation");
            CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),"R154 state fault preserves all destination bytes and CPU state");
        }
    }
    static void Canon163LegacyStateMmu()
    {
        // Same family table, actual parent MMU. In particular, a66+REX.W
        // state image must not inherit the old14/94-byte permission footprint.
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64];
        foreach (var row in Canon154StateRows())
        {
            if (row.Size == 512) continue; // existing composite FX suite owns it
            foreach (int cut in new[] { 1,14,row.Size })
            foreach (string kind in new[] { "ok","missing","readonly","lazy","eof" })
            {
                using var fixture=new ThreadedGuestExecution.FullControllerFixture();
                const ulong guest=ThreadedGuestExecution.FullControllerFixture.Base;
                fixture.Map(guest); fixture.Fill(guest);
                if (kind is "ok" or "readonly") { fixture.Map(guest+4096); fixture.Fill(guest+4096); }
                if (kind == "readonly") fixture.DataScope(0,()=>
                    Check(fixture.Program.ProtectVirtual(fixture.Machine()->blink_host_system,
                        (long)(guest+4096),4096,1,false) == 0,"R163 legacy state readonly second page"));
                if (kind == "lazy") fixture.Map(guest+4096);
                if (kind == "eof")
                {
                    var file=fixture.Io.CreateMemoryFile("r163-legacy-state-eof",0);
                    Check(file.Succeeded && fixture.Io.TruncateFile(file.Value,4096).Succeeded,"R163 legacy state EOF setup");
                    fixture.MapShared(file.Value,guest+4096); fixture.Fill(guest+4096);
                    Check(fixture.Io.TruncateFile(file.Value,0).Succeeded,"R163 legacy state EOF withdraw");
                }
                var i=Canon154Classify(row.Bytes);
                Check(i.SourceWidth == row.Size*8,"R163 literal expected legacy permission width");
                new Span<FullMemoryTlbEntry>(tlb,64).Clear();
                byte[] before=CanonicalX87Bytes((byte*)tlb,64*64);
                var f=new FullBlockFrame { Version=FullBlockFrame.AbiVersion,Size=FullBlockFrame.ByteSize,
                    Machine=fixture.Machine(),MemoryResume=1,MemoryAddress=guest+4096-(uint)cut,
                    MemoryAccess=row.Load?FullMemoryAccess.Read:FullMemoryAccess.Write,MemoryWidth=(uint)i.SourceWidth,
                    MemoryTlb=tlb,MemoryTlbMask=63 };
                var result=FillFullMemoryMmu(fixture,&f);
                bool crosses=cut<row.Size;
                var expected=!crosses || kind == "ok" || kind == "readonly" && row.Load?FullMemoryAssistResult.Resolved:
                    kind == "lazy"?FullMemoryAssistResult.DemandPage:FullMemoryAssistResult.GuestFault;
                Check(result == expected,"R163 complete legacy width permission/readonly/second-page/EOF boundary");
                if (result != FullMemoryAssistResult.Resolved)
                    Check(new ReadOnlySpan<byte>(tlb,64*64).SequenceEqual(before) && f.MemoryHost0 == null && f.MemoryHost1 == null,
                        "R163 failed legacy precheck publishes no partial mapping capability");
                if (result == FullMemoryAssistResult.GuestFault)
                    Check(f.FaultTrap == 14 && f.FaultAddress == guest+4096 &&
                        f.FaultCode == (kind == "readonly"?2u:kind == "eof"?unchecked((uint)-2):1u),
                        "R163 legacy PF/BUS last-byte failure retains exact address/code");
            }
        }
    }

    static void Canon163StatePrefixLiterals()
    {
        // Independent literal widths, rather than StateBytes/decoder-derived
        // expectations. Ordinary single-group prefixes have ISA credit.
        // Multiple same-group/REX aliases below are finite parser-profile
        // compatibility checks ONLY (AMD24592 3.5.1), never ordinary credit.
        // Prefix order cancels REX, and a later REX replaces it.
        foreach ((byte[] bytes,int size,bool small,bool wide) in new (byte[],int,bool,bool)[] {
            ([0x66,0x48,0xd9,0x20],28,false,false),([0x66,0x48,0xdd,0x20],108,false,false),
            ([0x66,0x48,0xd9,0x30],28,false,false),([0x66,0x48,0xdd,0x30],108,false,false),
            ([0x48,0x66,0xd9,0x30],14,true,false),([0x48,0x66,0xdd,0x30],94,true,false),
            ([0x66,0x48,0xf3,0xd9,0x30],14,true,false),
            ([0x66,0x48,0xf2,0x4f,0xdd,0x20],108,false,false),
            ([0x66,0x48,0x40,0xd9,0x30],14,true,false),
            ([0x66,0xf2,0x48,0x0f,0xae,0x08],416,false,true),
            ([0x48,0xf3,0x66,0x0f,0xae,0x08],416,false,false),
            ([0xf2,0xf3,0x66,0x48,0xd9,0x28],2,false,false),
            ([0xf3,0x66,0x48,0xdd,0x38],2,false,false) })
        {
            var i = Canon154Classify(bytes);
            Check(i.SourceWidth == size*8 &&
                ((i.Options&FullCanonicalX87ControlLowering.StateEnvironment16) != 0) == small &&
                ((i.Options&FullCanonicalX87ControlLowering.StatePointers64) != 0) == wide,
                "R163 literal format widths; mixed/duplicate prefixes separately profile-only");
        }
        foreach (byte[] bytes in new byte[][] {
            [0x66,0xd9,0x20],[0x66,0xdd,0x20],[0x48,0x66,0xd9,0x20],
            [0xf3,0x66,0xdd,0x20],[0x66,0x48,0xf2,0xd9,0x20] })
        {
            var i=Canon154Classify(bytes);
            Check(i.SourceWidth == (bytes[^2] == 0xd9?112:752) &&
                (i.Options&FullCanonicalX87ControlLowering.StateEnvironment16) != 0,
                "R163r1 former omitted-FOP gap admitted by explicit software profile; displaced REX alias only");
        }
        foreach (byte opcode in new byte[] { 0xd9,0xdd })
        {
            var large = Canon154Classify([opcode,0x20]);
            var small = large with { Length=3,SourceWidth=(opcode == 0xd9?14:94)*8,
                Options=large.Options|FullCanonicalX87ControlLowering.StateEnvironment16 };
            Check(FullBlockEmitter.ValidateCanonicalX87Control(small),"R163r1 exact typed small restore is supported");
            foreach (var bad in new[] { small with { Length=2 },small with { SourceWidth=large.SourceWidth },
                small with { Options=small.Options|FullCanonicalX87ControlLowering.StatePointers64 },
                small with { Extra=new(0,0xff,IsImmediate:true) } })
            {
                bool blocked=false;try { FullBlockEmitter.ValidateCanonicalX87Control(bad); }
                catch (ArgumentException) { blocked=true; }
                Check(blocked,"R163r1 new small format retains exact width/flag/opcode validator");
            }
        }
        byte[] longest = new byte[15]; longest.AsSpan(0,13).Fill(0xf3); longest[13]=0xd9; longest[14]=0x38;
        Check(Canon154Classify(longest).Length == 15,"R163 repeated-REP software alias honors exact15-byte limit; no ISA identity credit");
        byte[] tooLong = new byte[16]; longest.CopyTo(tooLong,1); tooLong[0]=0xf2;
        Check(!FullCanonicalX87ControlLowering.TryClassify(Ip,tooLong,out _,out _),"R163 ignored prefix cannot bypass architectural15 limit");
        Check(!FullCanonicalX87ControlLowering.TryClassify(Ip,[0xf3,0xd9,0x38,0],out _,out _) &&
            !FullCanonicalX87ControlLowering.TryClassify(Ip,[0xf2,0x0f,0xae],out _,out _) &&
            !FullCanonicalX87ControlLowering.TryClassify(Ip,[0xf3,0xd9,0x3c],out _,out _),
            "R163 state exact trailing-byte/ModRM/SIB parsing retained");
    }

    static void Canon163RegisterStatePrefixes(byte* fake,FullX87RegisterFile* state)
    {
        foreach (byte[] plain in new byte[][] { [0xdb,0xe2],[0xdb,0xe3],[0xdf,0xe0] })
        foreach (byte[] prefix in new byte[][] { [0xf2],[0xf3],[0xf3,0x66,0x67,0x36,0x48] })
        foreach (bool pending in new[] { false,true })
        {
            byte[] bytes = [..prefix,..plain]; var i = Canon154Classify(bytes); var original = Canon154Classify(plain);
            Check(i == original with { Length=bytes.Length } && !FullBlockEmitter.CanonicalX87ControlNeedsKernel(i),
                "R163 REP register controls reuse exact canonical FNINIT/FNCLEX/FNSTSW AX IR");
            using var code = CreateFullBlock([new(i)]);
            Canon154Seed(fake,state,7,0x017f);
            if (pending) { ((Blink.Machine*)fake)->fpu.sw |= 1; ((Blink.Machine*)fake)->fpu.cw &= ~1u; }
            byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
            byte[] work=CanonicalX87Bytes((byte*)Canon154Work(state),384);
            CanonicalX87Expected(i,m,raw);
            ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state;
            Check(RunFull(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1 &&
                f.HelperIndex == FullBlockFrame.NoHelperIndex && f.NativeMemoryComputations == 0 && f.FaultTrap == 0,
                "R163 nonwaiting prefixed register state controls complete once even with prior MF");
            CanonicalX87Same(fake,state,m,raw);
            Check(new ReadOnlySpan<byte>(Canon154Work(state),384).SequenceEqual(work),"R163 register state controls never touch Work/kernel/host FP");
        }
    }

    static void Canon163StateByteLiterals(byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        // Independent literal protected images. New finite software policy
        // clears omitted FOP only in14/94. Existing CW raw representation,
        // including PC01/all-ones, and SW ES->B normalization are unchanged.
        byte[] env28=Convert.FromHexString("7F01FFFF0098FFFF0000FFFF21436587BC9A65F798BADCFE4523FFFF");
        byte[] env14=Convert.FromHexString("7F010098000088774433AA996655");
        byte[] values=Convert.FromHexString(
            "0100000000000080FF3F1032547698BADCFE00600100000000000000000000000000000000000080"+
            "0100000000000080FF7FF0DEBC9A785634123412000000000000008000000000000000000080FFFF");
        Check(env14.Length == 14 && env28.Length == 28 && values.Length == 80,"R163r1 independent literal state image lengths");
        foreach (var row in Canon154StateRows())
        {
            if (row.Kind is not (3 or 5)) continue;
            var i=Canon154Classify(row.Bytes); using var code=CreateFullBlock([new(i)]);
            foreach (uint priorFop in new uint[] { 0,1,0x123,0x7ff })
            foreach (ushort incomingCw in new ushort[] { 0x017f,0xffff })
            {
                byte[] env=(row.Small?env14:env28).AsSpan().ToArray();
                BinaryPrimitives.WriteUInt16LittleEndian(env,incomingCw);
                byte[] image=row.Kind == 3?env:[..env,..values];
                Canon154Seed(fake,state,7,0x37f); new Span<byte>(pages,12288).Fill(0x5a);
                ((Blink.Machine*)fake)->fpu.op=(int)priorFop;
                // Physical old payloads are fixed independently of restored TOP.
                for (int p=0;p<8;++p)
                { var slot=FullX87RegisterFile.Physical(state,p); *slot=X87Literal(p); }
                Write64(fake,24,0x100fff); Canon154StateCopyIn(pages,4095,image);
                Canon154StateOne(code,row,i,fake,state,pages,4095);
                var m=new ReadOnlySpan<byte>(fake,sizeof(Blink.Machine));
                Check(BinaryPrimitives.ReadUInt32LittleEndian(m.Slice(672)) == incomingCw &&
                    BinaryPrimitives.ReadUInt32LittleEndian(m.Slice(648)) == 0x1800 &&
                    BinaryPrimitives.ReadUInt32LittleEndian(m.Slice(652)) == (row.Kind == 5?0x982au:0xaa60u) &&
                    BinaryPrimitives.ReadUInt32LittleEndian(m.Slice(656)) == (row.Small?0u:0x765u) &&
                    BinaryPrimitives.ReadUInt64LittleEndian(m.Slice(664)) == (row.Small?0x7788UL:0x87654321UL) &&
                    BinaryPrimitives.ReadUInt64LittleEndian(m.Slice(680)) == (row.Small?0x99aaUL:0xfedcba98UL) &&
                    state->CodeSelector == (row.Small?0x3344:0x9abc) && state->DataSelector == (row.Small?0x5566:0x2345),
                    "R163r1 literal prior-FOP0/1/123/7ff, raw CW, B/ES, tags, pointers/selectors; small FOP0 only");
                int[] physical=row.Kind == 5?new[] { 5,6,7,0,1,2,3,4 }:new[] { 0,1,2,3,4,5,6,7 };
                for (int p=0;p<8;++p) X87CheckValue(*FullX87RegisterFile.Physical(state,p),X87Literal(physical[p]));
            }
        }
    }

    static void Canon163StateSaveRestoreLiterals(byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        // Existing independent save-image oracle runs BEFORE the new restore.
        // Literal FOP and low-pointer checks prevent a matching pair of codec
        // bugs from manufacturing a roundtrip success. Values include unsupported
        // encodings, SNaN, pseudo-denormal and full binary80 precision/range.
        foreach (var save in Canon154StateRows())
        {
            if (save.Kind is not (4 or 6)) continue;
            byte[] bytes=save.Small?[0x66,(byte)(save.Kind == 4?0xd9:0xdd),0x20]:
                [(byte)(save.Kind == 4?0xd9:0xdd),0x20];
            var restore=new Canon154StateRow(bytes,save.Size,save.Kind == 4?3:5,true,save.Small,Waiting:true);
            var si=Canon154Classify(save.Bytes);var ri=Canon154Classify(bytes);
            using var saveCode=CreateFullBlock([new(si)]);using var restoreCode=CreateFullBlock([new(ri)]);
            for (int top=0;top<8;++top)
            foreach (ushort cw in new ushort[] { 0x017f,0xffff })
            {
                new Span<byte>(pages,12288).Fill(0x5a);Canon154Seed(fake,state,top,cw);
                var machine=(Blink.Machine*)fake;machine->fpu.sw=(uint)(top<<11);machine->fpu.tw=0;machine->fpu.op=0x5ab;
                Write64(fake,664,0x1122334487657788UL);Write64(fake,680,0xabcdef01fedc99aaUL);
                state->CodeSelector=0x3344;state->DataSelector=0x5566;
                for (int p=0;p<8;++p) { var slot=FullX87RegisterFile.Physical(state,p);*slot=X87Literal(p); }
                Write64(fake,24,0x100fff);Canon154StateOne(saveCode,save,si,fake,state,pages,4095);
                var saved=Canon154StatePages(pages).AsSpan(4095,save.Size);
                Check(BinaryPrimitives.ReadUInt16LittleEndian(saved) == cw &&
                    BinaryPrimitives.ReadUInt16LittleEndian(saved.Slice(save.Small?6:12)) == 0x7788 &&
                    BinaryPrimitives.ReadUInt16LittleEndian(saved.Slice(save.Small?10:20)) == 0x99aa &&
                    (save.Small || BinaryPrimitives.ReadUInt16LittleEndian(saved.Slice(18)) == 0x5ab),
                    "R163r1 independent saved state image literal CW/pointers/FOP before restore");
                // Source image remains in discontiguous pages; install a distinct
                // masked, clean old environment with nonzero FOP before restoring.
                machine->ip=Ip;machine->fpu.cw=0x37f;machine->fpu.sw=0;machine->fpu.tw=-1;machine->fpu.op=0x7ff;
                Write64(fake,664,ulong.MaxValue);Write64(fake,680,ulong.MaxValue);
                state->CodeSelector=0xeeee;state->DataSelector=0xdddd;
                if (save.Kind == 6)
                    for (int p=0;p<8;++p) { var slot=FullX87RegisterFile.Physical(state,p);*slot=new(0x5555555555555555,0x1234); }
                Canon154StateOne(restoreCode,restore,ri,fake,state,pages,4095);
                Check(machine->fpu.cw == cw && machine->fpu.sw == (uint)(top<<11) &&
                    (uint)machine->fpu.tw == 0xaa60 && machine->fpu.op == (save.Small?0u:0x5abu) &&
                    (ulong)machine->fpu.ip == (save.Small?0x7788UL:0x87657788UL) &&
                    (ulong)machine->fpu.dp == (save.Small?0x99aaUL:0xfedc99aaUL) &&
                    state->CodeSelector == 0x3344 && state->DataSelector == 0x5566,
                    "R163r1 save14/28/94/108 -> restore exact software metadata for every TOP/PC01/all-ones CW");
                for (int p=0;p<8;++p) X87CheckValue(*FullX87RegisterFile.Physical(state,p),X87Literal(p));
            }
        }
    }

    static void Canon163StateCanonicalFaults(byte* fake,FullX87RegisterFile* state)
    {
        foreach (var row in Canon154StateRows())
        foreach (bool stack in new[] { false,true })
        foreach (bool pending in new[] { false,true })
        {
            byte[] bytes=stack?[0x36,..row.Bytes]:row.Bytes;
            var i=Canon154Classify(bytes); using var code=CreateFullBlock([new(i)]);
            Canon154Seed(fake,state,7,0x017f);
            // Aligned canonical start, last-byte overflow into noncanonical
            // space. FX must check its entire512 span before its defined416.
            ulong address=0x800000000000UL-(row.Size == 512?16UL:1UL); Write64(fake,24,address);
            if (pending) { ((Blink.Machine*)fake)->fpu.sw |= 1; ((Blink.Machine*)fake)->fpu.cw &= ~1u; }
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
            byte[] work=CanonicalX87Bytes((byte*)Canon154Work(state),384);
            ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state;
            uint trap=pending&&row.Waiting?16u:stack?12u:13u;
            var observed=RunFull(code,&f);
            if(i.Address.AddressWidth==32&&trap!=16)
            {
                // AS32 truncates the poisoned high GPR bits BEFORE linear
                // canonicality. This RAX/SS operand has no FS/GS base: its low
                // 32-bit span is canonical, so it requests mapping, not GP/SS.
                Check(observed==FullBlockExit.NeedMemory&&f.MemoryAddress==(uint)address&&
                    f.MemoryWidth==(uint)(row.Size*8)&&issued==1&&completed==0&&f.Executed==0&&
                    f.NativeMemoryComputations==0,
                    "R163 AS32 state address ignores high poison before checked mapping");
            }
            else Check(observed == FullBlockExit.GuestFault && f.FaultTrap == trap &&
                f.FaultAddress == (trap == 16?Ip:0UL) && (trap == 16 || f.FaultCode == 128) &&
                issued == 1 && completed == 0 && f.Executed == 0 && f.MemoryAssists == 0 && f.NativeMemoryComputations == 0,
                "R163 state span GP/SS and prior MF ordering precede all memory/metadata/raw effects");
            CanonicalX87Same(fake,state,m,raw);
            Check(new ReadOnlySpan<byte>(Canon154Work(state),384).SequenceEqual(work),"R163 failed state span never reaches raw/tag staging");
        }
    }

    static void Canon154StateDecodeAndEmission()
    {
        Check(Canon154Classify([0x66,0xd9,0x20]).SourceWidth == 112 &&
            Canon154Classify([0x66,0xdd,0x20]).SourceWidth == 752,
            "R163r1 protected16 FLDENV14/FRSTOR94 width identities under authorized software profile");
        foreach (var row in Canon154StateRows())
        {
            byte[] locked = new byte[row.Bytes.Length+1]; locked[0] = 0xf0; row.Bytes.CopyTo(locked,1);
            Check(!FullCanonicalX87ControlLowering.TryClassify(Ip,locked,out _,out _),"R163 state LOCK remains invalid");
            // Prepending a second REP to a REP row is an explicit software
            // alias check; AMD marks multiple prefixes of one group undefined.
            foreach (byte prefix in new byte[] { 0xf2,0xf3 })
            {
                byte[] bytes = new byte[row.Bytes.Length+1]; bytes[0] = prefix; row.Bytes.CopyTo(bytes,1);
                var original = Canon154Classify(row.Bytes); var repeated = Canon154Classify(bytes);
                Check(repeated == original with { Length = original.Length+1 },
                    "R163 REP exact state identity; duplicates are software-profile aliases only");
            }
        }
        Canon163StatePrefixLiterals();
        foreach (var row in Canon154StateRows())
        {
            var valid = Canon154Classify(row.Bytes);
            foreach (var bad in new[] { valid with { Width = 64 },valid with { SourceWidth = valid.SourceWidth+8 },
                valid with { Length = 16 },valid with { Target = 1 },valid with { Condition = 1 },
                valid with { Options = valid.Options|0x80000 },valid with { Address = valid.Address with { Base = 16 } },
                valid with { Extra = new(0,0xff,IsImmediate:true) } })
            {
                bool rejected = false;
                try { FullBlockEmitter.ValidateCanonicalX87Control(bad); } catch (ArgumentException) { rejected = true; }
                Check(rejected,"R154 exact state IR shape/modifiers/address/encoding validation");
            }
            for (int bas = 0; bas < 16; ++bas)
            foreach (int index in new[] { -1,0,7,12,15 })
            foreach (bool as32 in new[] { false,true })
            foreach (int segment in new[] { -1,4,5 })
            {
                byte[] bytes = Canon154StateAddress(row,bas,index,as32,segment);
                var i = Canon154Classify(bytes);
                Check(i.Address.Base == bas && i.Address.Index == index && i.Address.Scale == (index<0?0:1) &&
                    i.Address.AddressWidth == (as32?32:64) && i.Address.Segment == segment,
                    "R154 all state base/index/REX.X/B/AS32/FS/GS exact decoder bindings");
            }
        }
        var canceled = Canon154Classify([0x48,0x66,0x0f,0xae,0]);
        var effective = Canon154Classify([0x66,0x48,0x0f,0xae,0]);
        Check((canceled.Options&FullCanonicalX87ControlLowering.StatePointers64) == 0 &&
            (effective.Options&FullCanonicalX87ControlLowering.StatePointers64) != 0,"R154 legacy prefix after REX cancels FX pointer64 selection");
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity],common = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] { false,true })
        foreach (bool shared in new[] { false,true })
        {
            FullBlockEmitter.EmitScaffold(common,out var layout,windows); var scaffold = layout.Resolve((nint)0x10000000);
            foreach (var row in Canon154StateRows())
            {
                var i = Canon154Classify(row.Bytes); FullBlockInstruction[] body = [new(i)]; int length;
                bool okay = shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out length,windows):
                    FullBlockEmitter.TryEmit(body,staging,out length,windows);
                Check(okay,"R154 coherent state body fits bounded emitter in both ABIs/scaffolds");
                foreach (int cut in new[] { 0,1,7,length-1 })
                {
                    int partial;
                    Check(!(shared?FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),in scaffold,out partial,windows):
                        FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),out partial,windows)) && partial == 0,
                        "R154 no partial state body publication on short span");
                }
                for (int warm = 0; warm < 12; ++warm)
                { if (shared) FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows); else FullBlockEmitter.TryEmit(body,staging,out _,windows); }
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int warm = 0; warm < 16; ++warm)
                {
                    okay &= FullCanonicalX87ControlLowering.TryClassify(Ip,row.Bytes,out _,out _);
                    okay &= shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                Check(okay && GC.GetAllocatedBytesForCurrentThread() == before,"R154 native state family warm allocation zero");
            }
        }
    }
    static byte[] Canon154StateAddress(Canon154StateRow row,int bas,int index,bool as32,int segment)
    {
        var bytes = new List<byte>();
        if (row.Small) bytes.Add(0x66); if (as32) bytes.Add(0x67);
        if (segment >= 0) bytes.Add(segment==4?(byte)0x64:(byte)0x65);
        byte rex = (byte)(0x40|(row.Wide?8:0)|(index>=8?2:0)|(bas>=8?1:0));
        if (rex != 0x40) bytes.Add(rex);
        bool fx = row.Kind is 7 or 8;
        if (fx) { bytes.Add(0x0f); bytes.Add(0xae); }
        else bytes.Add(row.Kind is 2 or 5 or 6?(byte)0xdd:(byte)0xd9);
        int group = row.Bytes[^1]>>3&7; bool sib = index>=0 || (bas&7)==4;
        bytes.Add((byte)(0x40|group<<3|(sib?4:bas&7)));
        if (sib) bytes.Add((byte)((index<0?0:1)<<6|(index<0?4:index&7)<<3|(bas&7)));
        bytes.Add(0); return bytes.ToArray();
    }
    static void Canon154StateTicket(NativeCodeAllocation code,Canon154StateRow row,JitInstruction i,
        byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        int offset = 4096-Math.Min(16,Math.Max(1,row.Size/2));
        new Span<byte>(pages,12288).Fill(0x5a); Canon154Seed(fake,state,7,0x37f);
        Write64(fake,24,0x100000u+(uint)offset);
        if (row.Load) Canon154StateCopyIn(pages,offset,Canon154StateInput(row,3,7));
        byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw = CanonicalX87Bytes((byte*)state,192),page = Canon154StatePages(pages);
        ulong epoch = 11; long issued = 0,completed = 0; int stop = 0; var h = FullHeader(Ip,code.Entry);
        var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87 = state;
        FullStackMemoryDescriptor descriptor = default; f.StackMemory = &descriptor;
        if (row.Size == 512)
        {
            Check(RunFull(code,&f) == FullBlockExit.NeedMemory && f.MemoryWidth == 4096,
                "R154 initial512 permission proof before actual416 missing-data request");
            Canon154StateProofReady(&f,in i,epoch);
        }
        Check(RunFull(code,&f) == FullBlockExit.NeedMemory && issued == 1 && completed == 0 &&
            f.MemoryAddress == 0x100000u+(uint)offset && f.MemoryWidth == (row.Size==512?416:row.Size)*8 &&
            f.MemoryAccess == (row.Load?FullMemoryAccess.Read:FullMemoryAccess.Write),"R154 actual state exact span request before any data effect");
        CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),"R154 state miss no partial copy/masking/reset");
        uint first = (uint)(4096-offset);
        if (row.Size == 512)
        {
            // Model parent promotion/revocation + reacquisition at a NEW
            // final header epoch; old proof must never authorize this ticket.
            ++epoch; f.ObservedGlobalEpoch=epoch; f.ObservedCodeAuthorityEpoch=epoch; h.ValidatedGlobalEpoch=epoch;
        }
        FullNativeMemory.PublishBarrierTicket(&f,pages+offset,pages+8192,first,epoch,123);
        if (row.Size == 512) Canon154StateProofReady(&f,in i,epoch,actual:true);
        else FullNativeMemory.ResumeSameIssue(&f);
        f.Budget = 0; stop = 1;
        Check(RunFull(code,&f) == FullBlockExit.Stop && issued == 1 && completed == 0 && f.FirstPreissued == 1,
            "R154 state cancellation preserves prepared same-issue ticket");
        CanonicalX87Same(fake,state,m,raw); stop = 0;
        Canon154StateExpected(row,i,m,raw,page,offset);
        Check(RunFull(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1 &&
            f.FirstPreissued == 0 && f.NativeMemoryComputations == 1,"R154 state fullspan ticket resumes same issue despite later exhausted budget");
        CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),"R154 state ticket exact copy and post-copy state");
    }
    static void Canon154StateDeferred(byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        foreach (var row in Canon154StateRows())
        {
            if (!row.Load) continue;
            var i = Canon154Classify(row.Bytes); ulong waitIp = Ip+(uint)i.Length;
            Check(FullCanonicalX87StackLowering.TryClassify(waitIp,[0x9b],out var wait,out _),"R154 state next waiting instruction");
            using var code = CreateFullBlock([new(i),new(wait)]);
            new Span<byte>(pages,12288).Fill(0x5a); Canon154Seed(fake,state,7,0x37f);
            ((Blink.Machine*)fake)->fpu.sw |= 1; Write64(fake,24,0x100000);
            Canon154StateCopyIn(pages,0,Canon154StateInput(row,3,0));
            byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw = CanonicalX87Bytes((byte*)state,192),page = Canon154StatePages(pages);
            Canon154StateExpected(row,i,m,raw,page,0);
            ulong epoch = 11; long issued = 0,completed = 0; int stop = 0; var h = FullHeader(Ip,code.Entry);
            var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop); f.X87 = state;
            FullStackMemoryDescriptor descriptor = default; f.StackMemory = &descriptor;
            FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            f.MemoryTlb = tlb; f.MemoryTlbMask = 63; SetFullMemoryEntry(tlb,0x100000,pages);
            if (row.Size == 512)
            {
                Check(RunFull(code,&f) == FullBlockExit.NeedMemory && issued == 1 && completed == 0,"R154 FXRSTOR proof before deferred-state load");
                Canon154StateProofReady(&f,in i,epoch);
            }
            Check(RunFull(code,&f) == FullBlockExit.GuestFault && issued == 2 && completed == 1 && f.Executed == 1 &&
                f.FaultTrap == 16 && f.FaultAddress == waitIp,"R154 loaded unmasked flags complete state load, defer MF to next wait");
            CanonicalX87Same(fake,state,m,raw);
        }
    }
    static void Canon154StateFxPermissionTail(NativeCodeAllocation code,Canon154StateRow row,JitInstruction i,
        byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        foreach (int cut in new[] { 416,464,512 })
        {
            Console.WriteLine($"FX-TAIL cut={cut}");
            int offset=4096-cut;
            new Span<byte>(pages,12288).Fill(0x5a); Canon154Seed(fake,state,7,0x37f); Write64(fake,24,0x100000u+(uint)offset);
            if (row.Load) Canon154StateCopyIn(pages,offset,Canon154StateInput(row,3,7));
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),page=Canon154StatePages(pages);
            ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state;
            FullStackMemoryDescriptor descriptor=default; f.StackMemory=&descriptor;
            FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
            f.MemoryTlb=tlb; f.MemoryTlbMask=63; SetFullMemoryEntry(tlb,0x100000,pages);
            Check(RunFull(code,&f) == FullBlockExit.NeedMemory && issued == 1 && completed == 0 &&
                f.MemoryWidth == 4096 && descriptor.Request0.Width == 4096 &&
                descriptor.Request0.Access == (row.Load?FullMemoryAccess.Read:FullMemoryAccess.Write),
                "R154 FX full512 permission proof mandatory when only416/464 data prefix has a TLB mapping");
            CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),
                "R154 no partial FX effects before unused-tail permissions checked");
            // Simulate successful FULL512 permission resolution. No data TLB
            // entry for the unused tail is needed: the copier touches416 only.
            Canon154StateProofReady(&f,in i,epoch);
            Canon154StateExpected(row,i,m,raw,page,offset);
            Check(RunFull(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1 &&
                f.MemoryWidth == 3328 && descriptor.Count == 0,"R154 consume512 proof BEFORE ordinary416 descriptor normalization");
            CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),
                "R154 FX actual effects416, all96 unused tail bytes preserved");
        }
        Console.WriteLine("FX-PHASE canonical tail"); Canon154StateFxCanonicalTail(code,i,fake,state);
        Console.WriteLine("FX-PHASE proof negatives"); Canon154StateFxProofFailures(code,row,i,fake,state,pages);
        Console.WriteLine("FX-PHASE actual TLB"); Canon154StateFxActualTlb(code,row,i,fake,state,pages);
    }

    static void Canon154StateFxCanonicalTail(NativeCodeAllocation code,JitInstruction i,byte* fake,FullX87RegisterFile* state)
    {
        // Actual416 would fit canonically; architectural512 crosses the
        // canonical hole or wraps. Fail before descriptor/TLB pointer access.
        foreach (ulong address in new[] { 0x00007ffffffffe60UL,0xfffffffffffffe60UL })
        {
            Canon154Seed(fake,state,7,0x37e); ((Blink.Machine*)fake)->fpu.sw |= 1;
            Write64(fake,24,address);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192);
            ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state;
            f.StackMemory=(FullStackMemoryDescriptor*)1; f.MemoryTlb=(FullMemoryTlbEntry*)1; f.MemoryTlbMask=63;
            Check(RunFull(code,&f) == FullBlockExit.GuestFault && f.FaultTrap == 13 && f.FaultCode == 128 && f.FaultAddress == 0 &&
                issued == 1 && completed == 0 && f.Executed == 0,"R154 FX full512 canonical endpoint/wrap GP precedes proof, data and priorMF");
            CanonicalX87Same(fake,state,m,raw);
        }
    }

    static void Canon154StateFxActualTlb(NativeCodeAllocation code,Canon154StateRow row,JitInstruction i,
        byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        const int offset=4080;
        new Span<byte>(pages,12288).Fill(0x5a); Canon154Seed(fake,state,7,0x37f); Write64(fake,24,0x100000+offset);
        if (row.Load) Canon154StateCopyIn(pages,offset,Canon154StateInput(row,3,7));
        byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),page=Canon154StatePages(pages);
        ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
        var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state;
        FullStackMemoryDescriptor descriptor=default; f.StackMemory=&descriptor;
        FullMemoryTlbEntry* tlb=stackalloc FullMemoryTlbEntry[64]; new Span<FullMemoryTlbEntry>(tlb,64).Clear();
        f.MemoryTlb=tlb; f.MemoryTlbMask=63;
        Check(RunFull(code,&f) == FullBlockExit.NeedMemory && f.MemoryWidth == 4096,"R154 actual-TLB initial512 proof");
        Canon154StateProofReady(&f,in i,epoch);
        Check(RunFull(code,&f) == FullBlockExit.NeedMemory && f.MemoryWidth == 3328 && descriptor.Count == 0,
            "R154 initial proof does not grant actual data mappings");
        SetFullMemoryEntry(tlb,0x100000,pages); SetFullMemoryEntry(tlb,0x101000,pages+8192);
        Canon154StateProofReady(&f,in i,epoch,actual:true);
        Check(f.MemoryWidth == 3328 && f.MemoryFlags == FullMemoryFlags.None && f.MemoryTicket == 0,
            "R154 composite Ready512 with exact416 TLB summary and no ticket");
        Canon154StateExpected(row,i,m,raw,page,offset);
        Check(RunFull(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1,
            "R154 actual-TLB resume consumes separate final512 proof without another issue");
        CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),"R154 composite TLB complete state/byte effects");
    }

    static void Canon154StateFxProofFailures(NativeCodeAllocation code,Canon154StateRow row,JitInstruction i,
        byte* fake,FullX87RegisterFile* state,byte* pages)
    {
        FullMemoryTlbEntry* attentionTlb=stackalloc FullMemoryTlbEntry[64];
        new Span<FullMemoryTlbEntry>(attentionTlb,64).Clear();
        SetFullMemoryEntry(attentionTlb,0x100000,pages); SetFullMemoryEntry(attentionTlb,0x101000,pages+8192);
        foreach (string mutation in new[] { "site","address","width","access","host0","host1","length","reserved","request1",
            "descriptor-ticket","epoch","summary-width","summary-access","resume","initial-ticket","actual-ticket-zero",
            "actual-ticket-epoch","actual-ticket-missing-proof","actual-ticket-stale-proof","stop","attention" })
        {
            Console.WriteLine($"FX-PROOF negative={mutation}");
            Canon154Seed(fake,state,0,0x37f); Write64(fake,24,0x100000);
            byte[] m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw=CanonicalX87Bytes((byte*)state,192),page=Canon154StatePages(pages);
            ulong epoch=11; long issued=0,completed=0; int stop=0; var h=FullHeader(Ip,code.Entry);
            var f=FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87=state;
            FullStackMemoryDescriptor descriptor=default; f.StackMemory=&descriptor;
            f.MemoryTlb=(FullMemoryTlbEntry*)1; f.MemoryTlbMask=63;
            Check(RunFull(code,&f) == FullBlockExit.NeedMemory && issued == 1,"R154 FX proof poison request setup");
            Canon154StateProofReady(&f,in i,epoch);
            bool actual=mutation.StartsWith("actual-ticket",StringComparison.Ordinal);
            if (actual)
            {
                f.MemoryWidth=3328; f.MemoryFlags=FullMemoryFlags.BarrierTicket; f.MemoryTicket=123; f.MemoryEpoch=epoch;
                f.MemoryHost0=(byte*)1; f.MemoryHost1=(byte*)1; f.MemoryLength0=416;
            }
            switch (mutation)
            {
                case "site": ++descriptor.Site; break;
                case "address": ++descriptor.Request0.Address; break;
                case "width": descriptor.Request0.Width=3328; break;
                case "access": descriptor.Request0.Access=row.Load?FullMemoryAccess.Write:FullMemoryAccess.Read; break;
                case "host0": descriptor.Request0.Host0=(byte*)1; break;
                case "host1": descriptor.Request0.Host1=(byte*)1; break;
                case "length": descriptor.Request0.Length0=1; break;
                case "reserved": descriptor.Request0.Reserved40=1; break;
                case "request1": descriptor.Request1.Address=1; break;
                case "descriptor-ticket": descriptor.Ticket=123; break;
                case "epoch": --descriptor.Epoch; break;
                case "summary-width": f.MemoryWidth=80; break;
                case "summary-access": f.MemoryAccess=row.Load?FullMemoryAccess.Write:FullMemoryAccess.Read; break;
                case "resume": f.MemoryResume=0; break;
                case "initial-ticket": f.MemoryFlags=FullMemoryFlags.BarrierTicket; f.MemoryTicket=123; break;
                case "actual-ticket-zero": f.MemoryTicket=0; break;
                case "actual-ticket-epoch": --f.MemoryEpoch; break;
                case "actual-ticket-missing-proof": descriptor.Count=0; break;
                case "actual-ticket-stale-proof": --descriptor.Epoch; break;
                case "stop": stop=1; break;
                case "attention":
                    // This is already the approved issue. Attention is handled
                    // at the next instruction boundary; STOP still wins now.
                    // Supply real data capability instead of the invalid
                    // pointer used solely by rejected-identity cases above.
                    new Span<byte>(pages,12288).Fill(0x5a);
                    if (row.Load) Canon154StateCopyIn(pages,0,Canon154StateInput(row,3,7));
                    f.MemoryTlb=attentionTlb;
                    ((Blink.Machine*)fake)->attention=true;
                    m=CanonicalX87Bytes(fake,sizeof(Blink.Machine)); page=Canon154StatePages(pages);
                    Canon154StateExpected(row,i,m,raw,page,0);
                    break;
            }
            bool completes=mutation == "attention";
            var expected=mutation == "stop"?FullBlockExit.Stop:completes?FullBlockExit.Continue:FullBlockExit.Invalidated;
            Check(RunFull(code,&f) == expected && issued == 1 && completed == (completes?1:0) && f.Executed == (completes?1UL:0UL),
                "R154 invalid proof has no effects; approved issue finishes once before pending attention");
            if (completes) Check((int)((Blink.Machine*)fake)->attention != 0 && f.FirstPreissued == 0,
                "R154 attention remains pending after the approved FX instruction completes");
            if (actual && mutation != "actual-ticket-zero")
                Check(f.MemoryTicket == 123 && f.MemoryFlags == FullMemoryFlags.BarrierTicket,
                    "R154 failed proof preserves actual416 ticket for parent scope release");
            CanonicalX87Same(fake,state,m,raw); Check(Canon154StatePages(pages).AsSpan().SequenceEqual(page),"R154 failed512 proof preserves all CPU/raw/guest bytes");
        }
    }
}
