using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using Raw154 = Managed.Emulation.Execution.FullExtended80;

unsafe partial class Program
{
    // AUTHORED_UNRUN. No broad ResetFull changes, guest activation or helper oracle.
    // Parent wires only this entry after adopting its classifier/core/pre-Issue
    // guards and post-RET fatal marker. Numeric kernel qualification is separate.
    static void CheckFullCanonicalX87Controls(byte* machine)
    {
        _ = machine;
        Check(!RuntimeHelpers.IsReferenceOrContainsReferences<FullX87KernelCall>() &&
            !RuntimeHelpers.IsReferenceOrContainsReferences<FullBlockInstruction>(),"R154 unmanaged IR tuple");
        Check(FullX87KernelCatalog.MaximumKernels >= 69,"R154 requires the coherent parent69-key catalog batch including66..68");
        Check(sizeof(FullBlockFrame) == 416 && sizeof(TranslationBlockHeader) == 160 &&
            sizeof(FullX87RegisterFile) == 192 && sizeof(FullExtended80WorkFrame) == 384 &&
            sizeof(FullX87NativeStorage) == 576,"R154 ABI7 exact owned576");
        Canon154Decode(); Canon154OracleAnchors(); Canon154StateProbeMmu();
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value,4096,2,4L<<20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority,store,true);
        var calls = new FullX87KernelCall[FullX87KernelCatalog.MaximumKernels]; FullX87KernelBinding binding;
        lock (authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes],out _) && catalog.Count == FullX87KernelCatalog.MaximumKernels,
                "R154 real eager complete RX catalog");
            for (int k = 0; k < FullX87KernelCatalog.MaximumKernels; ++k)
            {
                var b = catalog.Find((FullX87KernelKey)k);
                Check(catalog.IsValid(in b),"R154 SAME gate authenticates exact binding");
                calls[k] = new((byte)k,b.Entry,73,b.CodeBytes);
            }
            binding = catalog.Find(FullX87KernelKey.Fcom);
        }
        bool denied = false;
        try { _ = binding.Entry; } catch (InvalidOperationException) { denied = true; }
        Check(denied,"R154 binding Entry inaccessible outside authority gate");
        var token = RetainedPublish(store,0x540000);
        Check(store.TryAcquireToken(token,out var root),"R154 actual TB root lease");
        Check(store.TryBeginNativeEpoch(out var epoch),"R154 actual TB epoch");
        bool enrolled;
        lock (authority.Gate) enrolled = catalog.AdmissionOpen && authority.TryEnterNative(authority.Epoch);
        Check(enrolled,"R154 authority enrollment held through body/kernel/continuation/RET");
        byte* fake = (byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine)+15)&~15),16);
        var state = (FullX87RegisterFile*)NativeMemory.AlignedAlloc(576,16);
        try
        {
            Check(fake != null && state != null && !fullOwnerScaffold.IsValid,"R154 exact isolated storage");
            Console.WriteLine("CONTROL-PHASE standalone register/numeric family");
            Canon154Suite(fake,state,calls);
            using (var common = CreateFullScaffold(out var addresses))
            {
                fullOwnerScaffold = addresses;
                try { Console.WriteLine("CONTROL-PHASE shared register/numeric family"); Canon154Suite(fake,state,calls); }
                finally { fullOwnerScaffold = default; }
            }
            Console.WriteLine("CONTROL-PHASE complete state family");
            Canon154StateSuite(fake,state);
            Canon154Emission(calls); Canon154Fatal(fake,state);
            lock (authority.Gate)
            {
                Check(catalog.IsValid(in binding),"R154 original owner remains alive after body disposal");
                catalog.CloseAdmission(); denied = false;
                try { _ = binding.Entry; } catch (InvalidOperationException) { denied = true; }
                Check(denied && !catalog.AdmissionOpen,"R154 closed catalog rejects retained binding");
            }
        }
        catch (Exception error)
        { Console.Error.WriteLine("PRIMARY R154 family failure: " + error); throw; }
        finally
        {
            NativeMemory.AlignedFree(state); NativeMemory.AlignedFree(fake);
            if (enrolled) authority.ExitNative(); epoch.Dispose(); root.Dispose();
            store.Dispose(); catalog.Dispose();
        }
        Check(catalog.ChargedBytes == 0,"R154 successful store barrier precedes final unmap");
    }

    // Independent documented byte grammar: no product enum-driven generation.
    static IEnumerable<byte[]> Canon154Forms()
    {
        foreach ((byte op,byte m) in new (byte,byte)[] {
            (0xd8,0xd0),(0xd8,0xd8),(0xdd,0xe0),(0xdd,0xe8),
            (0xdb,0xf0),(0xdb,0xe8),(0xdf,0xf0),(0xdf,0xe8),
            (0xda,0xc0),(0xda,0xc8),(0xda,0xd0),(0xda,0xd8),
            (0xdb,0xc0),(0xdb,0xc8),(0xdb,0xd0),(0xdb,0xd8) })
            for (int n = 0; n < 8; ++n) yield return [op,(byte)(m+n)];
        yield return [0xde,0xd9]; yield return [0xda,0xe9];
        yield return [0xd9,0xe4]; yield return [0xd9,0xe5];
        yield return [0xd9,0xfc]; yield return [0xd9,0xfa]; yield return [0xd9,0xf4];
        yield return [0xd9,0xfd]; yield return [0xd9,0xf8]; yield return [0xd9,0xf5];
    }
    static JitInstruction Canon154Classify(ReadOnlySpan<byte> bytes,ulong ip = Ip)
    {
        Check(FullCanonicalX87ControlLowering.TryClassify(ip,bytes,out var i,out var reason),reason ?? "R154 decode");
        Check(FullBlockEmitter.ValidateCanonicalX87Control(i),"R154 typed IR"); return i;
    }
    static FullX87KernelCall Canon154Call(JitInstruction i,FullX87KernelCall[] calls) =>
        FullBlockEmitter.CanonicalX87ControlNeedsKernel(i) ? calls[(int)FullBlockEmitter.CanonicalX87ControlKernel(i)] : default;
    static void Canon154Decode()
    {
        int count = 0;
        foreach (byte[] form in Canon154Forms())
        {
            ++count;
            foreach (byte[] prefix in new byte[][] { [],[0x66],[0x67,0x64,0x4f],[0x4f,0x66,0x40],
                [0x26,0x2e,0x36,0x3e,0x64,0x65,0x66,0x67,0x40,0x4f,0x66,0x67,0x40] })
            {
                byte[] b = new byte[prefix.Length+2]; prefix.CopyTo(b,0); form.CopyTo(b,prefix.Length);
                var i = Canon154Classify(b); int fop = (form[0]&7)<<8|form[1];
                bool reduction = form[0] == 0xd9 && form[1] is 0xfd or 0xf8 or 0xf5;
                bool unary = form[0] == 0xd9 && !reduction;
                Check(i.Length == b.Length && i.Source == new JitOperand(reduction?1:unary?0:form[1]&7,0) &&
                    i.Extra == new JitOperand(0,(uint)fop,IsImmediate:true),"R154 full FOP/logical ST including implicit pair ST1");
                Check(FullBlockEmitter.CanonicalX87ControlRequiresArithmeticPrecision(i) == (fop == 0x1fa),
                    "R154 ONLY FSQRT requires arithmetic precision; compare/round PC01 allowed");
                foreach (var bad in new[] { i with { Width = 64 },i with { Length = 1 },i with { Length = 16 },
                    i with { Options = i.Options|0x10000 },i with { Source = new(8,0) },
                    i with { Source = i.Source with { IsImmediate = true } },i with { SourceWidth = 80 },
                    i with { Destination = new(1,0) },i with { Target = 1 },i with { Condition = 1 },
                    i with { Address = new(1,2,0,0) },i with { Extra = new(0,(uint)fop^0x100,IsImmediate:true) } })
                {
                    bool rejected = false;
                    try { FullBlockEmitter.ValidateCanonicalX87Control(bad); } catch (ArgumentException) { rejected = true; }
                    Check(rejected,"R154 malformed recognized IR refused before publication");
                }
                bool needs = FullBlockEmitter.CanonicalX87ControlNeedsKernel(i);
                var valid = needs ? new FullX87KernelCall((byte)FullBlockEmitter.CanonicalX87ControlKernel(i),(nint)0x1000,73,32) : default;
                Check(FullBlockEmitter.ValidateCanonicalX87Control(i,in valid),"R154 raw paths require no fake binding");
                foreach (var bad in new[] { new FullX87KernelCall(19,(nint)0x2000,73,32),valid with { Entry = 0 },
                    valid with { OwnerCookie = 0 },valid with { CodeBytes = 0 },valid with { CodeBytes = FullX87KernelCatalog.StagingBytes + 1 },
                    valid with { Key = (byte)(valid.Key^1) } })
                {
                    if (bad == valid || !needs && bad == default) continue;
                    bool rejected = false;
                    try { FullBlockEmitter.ValidateCanonicalX87Control(i,in bad); } catch (ArgumentException) { rejected = true; }
                    // A different valid Entry cannot be authenticated by a raw
                    // tuple validator; SAME gate owner check is parent owned.
                    if (needs && bad.Key == valid.Key && bad.Entry != 0 && bad.OwnerCookie != 0 && bad.CodeBytes is > 0 and <= FullX87KernelCatalog.StagingBytes) continue;
                    Check(rejected,"R154 wrong kind/key/header/raw fake binding rejected");
                }
            }
        }
        foreach (byte[] bytes in CanonicalX87Forms())
        {
            if (bytes.AsSpan().SequenceEqual(new byte[] { 0x9b })) continue;
            var shared = Canon154Classify(bytes);
            Check(FullBlockEmitter.CanInlineCanonicalX87Stack(shared) &&
                !FullBlockEmitter.CanonicalX87ControlNeedsKernel(shared),"R154 one family API reuses R143 canonical stack/control IR without duplication");
        }
        Check(count == 138,"R154 66 comparison+64 FCMOV+5 unary+3 scale/remainder forms");
        foreach (byte[] bad in new byte[][] { [],[0x9b],[0xf0,0xd8,0xd0],[0xf2,0xd9,0xfc],[0xf3,0xda,0xc0],
            [0xdc,0xd0],[0xde,0xd0],[0xde,0xd8],[0xdb,0xf8],[0xdf,0xff],[0xd9,0xe6],
            [0xd8,0x10],[0xda,0xe8],[0xd9,0xfc,0] })
            Check(!FullCanonicalX87ControlLowering.TryClassify(Ip,bad,out _,out _),"R154 no undocumented aliases/memory/REP/WAIT");
        Check((byte)FullBlockEmitter.CanonicalX87ControlKernel(Canon154Classify([0xd9,0xf4])) == 31,
            "R154 FXTRACT reuses exact typed existing Extract key31, including PC01");
    }

    static FullExtended80WorkFrame* Canon154Work(FullX87RegisterFile* state) => (FullExtended80WorkFrame*)((byte*)state+192);
    static void Canon154Seed(byte* fake,FullX87RegisterFile* state,int top,ushort cw,uint flags = 0xa50008d6)
    {
        new Span<byte>(state,576).Fill(0xa5); CanonicalX87Seed(fake,state,top,cw);
        ((Blink.Machine*)fake)->fpu.sw = 0x5a004740u|(uint)top<<11;
        ((Blink.Machine*)fake)->flags = flags;
        Canon154Work(state)->Continuation = 0; ((uint*)((byte*)state+192+280))[0] = 0;
    }
    static void Canon154Input(byte* fake,FullX87RegisterFile* state,int n,Raw154 value,int tag = -1)
    {
        int p = ((int)(((Blink.Machine*)fake)->fpu.sw>>11)+n)&7;
        var raw = FullX87RegisterFile.Physical(state,p); raw->Significand = value.Significand; raw->SignExponent = value.SignExponent;
        if (tag < 0) tag = Canon154Tag(value);
        uint tw = unchecked((uint)((Blink.Machine*)fake)->fpu.tw);
        ((Blink.Machine*)fake)->fpu.tw = unchecked((int)((tw&~(3u<<(2*p)))|(uint)tag<<(2*p)));
    }
    static int Canon154Tag(Raw154 x) => (x.SignExponent&0x7fff) == 0 && x.Significand == 0 ? 1 :
        (x.SignExponent&0x7fff) is > 0 and < 0x7fff && (x.Significand>>63) != 0 ? 0 : 2;
    static Raw154 Canon154Read(byte[] raw,int p) => new(BinaryPrimitives.ReadUInt64LittleEndian(raw.AsSpan(64+16*p)),
        BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(72+16*p)));
    static void Canon154Write(byte[] raw,int p,Raw154 x)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(raw.AsSpan(64+16*p),x.Significand);
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(72+16*p),x.SignExponent);
    }

    // Independent raw80 oracle classes: zero0, finite1, infinity2, quiet3,
    // signaling4, unsupported5, true/pseudo denormal6. No product enums.
    static int Canon154Class(Raw154 x)
    {
        int e = x.SignExponent&0x7fff; ulong s = x.Significand;
        if (e == 0) return s == 0 ? 0 : 6;
        if ((s>>63) == 0) return 5;
        if (e != 0x7fff) return 1;
        return s == 0x8000000000000000 ? 2 : (s&0x4000000000000000) != 0 ? 3 : 4;
    }
    static BigInteger Canon154Units(Raw154 x)
    {
        int e = Math.Max(1,x.SignExponent&0x7fff)-1;
        BigInteger n = new BigInteger(x.Significand)<<e;
        return (x.SignExponent&0x8000) != 0 ? -n : n;
    }
    static int Canon154Relation(Raw154 a,Raw154 b)
    {
        if (Canon154Class(a) == 2 || Canon154Class(b) == 2)
        {
            if (Canon154Class(a) == 2 && Canon154Class(b) == 2)
                return ((b.SignExponent>>15)-(a.SignExponent>>15));
            return Canon154Class(a) == 2 ? (a.SignExponent>>15) == 0 ? 1 : -1 : (b.SignExponent>>15) == 0 ? -1 : 1;
        }
        return Canon154Units(a).CompareTo(Canon154Units(b));
    }
    static bool Canon154Parity(uint flags) => (BitOperations.PopCount(flags>>24)&1) == 0;
    static bool Canon154MoveCondition(int fop,uint flags) => (fop&0x7f8) switch
    {
        0x2c0 => (flags&1) != 0,0x2c8 => (flags&64) != 0,0x2d0 => (flags&65) != 0,0x2d8 => Canon154Parity(flags),
        0x3c0 => (flags&1) == 0,0x3c8 => (flags&64) == 0,0x3d0 => (flags&65) == 0,_ => !Canon154Parity(flags)
    };
    static Raw154 Canon154Integer(BigInteger n,bool negative)
    {
        if (n.IsZero) return new(0,negative?(ushort)0x8000:(ushort)0);
        int bits = (int)n.GetBitLength();
        return new((ulong)(n<<(64-bits)),(ushort)(16382+bits|(negative?0x8000:0)));
    }
    static (Raw154 Value,uint Delta,bool Write) Canon154Unary(int fop,Raw154 a,ushort cw)
    {
        int cls = Canon154Class(a); bool negative = (a.SignExponent&0x8000) != 0;
        uint delta = 0; Raw154 result = a;
        if (cls == 5 || fop == 0x1fa && negative && (cls is 1 or 2 or 6))
        { result = new(0xc000000000000000,0xffff); delta = 1; }
        else if (cls == 4) { result = new(a.Significand|0x4000000000000000,a.SignExponent); delta = 1; }
        else if (cls is 1 or 6)
        {
            if (cls == 6) delta = 2;
            int e = Math.Max(1,a.SignExponent&0x7fff)-16383-63, rc = cw>>10&3;
            if (fop == 0x1fc)
            {
                BigInteger numerator = a.Significand, denominator = BigInteger.One;
                if (e >= 0) numerator <<= e; else denominator <<= -e;
                BigInteger n = BigInteger.DivRem(numerator,denominator,out var remainder);
                if (!remainder.IsZero)
                {
                    delta |= 32;
                    bool up = rc == 0 ? 2*remainder > denominator || 2*remainder == denominator && !n.IsEven :
                        rc == 1 ? negative : rc == 2 && !negative;
                    if (up) { ++n; delta |= 0x200; }
                }
                // Huge exact integral inputs already have their correct raw80
                // representation; avoid shifting by a negative number.
                result = n.GetBitLength() > 64 ? a : Canon154Integer(n,negative);
            }
            else
            {
                int p = (cw>>8&3) switch { 0 => 24,2 => 53,_ => 64 };
                int log2 = (int)new BigInteger(a.Significand).GetBitLength()-1+e;
                int resultExponent = log2 >= 0 ? log2/2 : (log2-1)/2;
                int shift = e+2*(p-1-resultExponent);
                BigInteger num = a.Significand, den = BigInteger.One;
                if (shift >= 0) num <<= shift; else den <<= -shift;
                // Rational threshold-square binary search; no digit-pair root,
                // product math, host float or double-rounding reference.
                BigInteger lo = BigInteger.One<<(p-1), hi = BigInteger.One<<p;
                while (lo+1 < hi)
                { BigInteger mid = (lo+hi)>>1; if (mid*mid*den <= num) lo = mid; else hi = mid; }
                bool exact = lo*lo*den == num;
                if (!exact)
                {
                    delta |= 32;
                    int threshold = (4*num).CompareTo((2*lo+1)*(2*lo+1)*den);
                    if (rc == 2 || rc == 0 && (threshold > 0 || threshold == 0 && !lo.IsEven))
                    { ++lo; delta |= 0x200; }
                }
                if (lo == (BigInteger.One<<p)) { lo >>= 1; ++resultExponent; }
                result = new((ulong)(lo<<(64-p)),(ushort)(resultExponent+16383));
            }
        }
        uint stopping = (delta&~cw&1) != 0 ? 1u : (delta&~cw&2) != 0 ? 2u : 0;
        return (result,stopping == 0 ? delta : stopping,stopping == 0);
    }

    // Whole Machine + file192 completion oracle, driven by opcode literals.
    // Architectural undefined CCs are retained as this profile's chosen policy.
    // The oracle never reads kernel metadata or calls product math/Op helpers.
    static void Canon154Expected(JitInstruction i,byte[] m,byte[] raw)
    {
        int fop = (int)i.Extra.Immediate, n = fop&7;
        bool reduction = fop is 0x1fd or 0x1f8 or 0x1f5; if (reduction) n = 1;
        uint sw = CanonicalX87U32(m,648), tw = CanonicalX87U32(m,652), flags = CanonicalX87U32(m,12);
        ushort cw = (ushort)CanonicalX87U32(m,672); int top = (int)(sw>>11)&7, other = (top+n)&7;
        Raw154 a = Canon154Read(raw,top), b = Canon154Read(raw,other);
        bool unary = fop>>8 == 1 && !reduction, move = (fop&0x7e0) is 0x2c0 or 0x3c0;
        if (unary) n = 0;
        bool empty = (tw>>(2*top)&3) == 3 || !unary && (tw>>(2*other)&3) == 3;
        uint delta = 0; bool write = false,push = false; Raw154 result = default,pushed = default;
        int pops = fop is 0x6d9 or 0x2e9 ? 2 : (fop&0x7f8) is 0x0d8 or 0x5e8 or 0x7e8 or 0x7f0 ? 1 : 0;
        bool integerFlags = (fop&0x7f8) is 0x3e8 or 0x3f0 or 0x7e8 or 0x7f0;
        if (reduction)
        {
            if (empty) { delta = 0x41; sw &= ~0x200u; write = (cw&1) != 0; result = new(0xc000000000000000,0xffff); }
            else
            {
                var r = Canon154ReductionReference(fop,a,b,cw); result = r.Value; delta = r.Delta; write = r.Write;
                sw = (sw&~r.Mask)|r.Conditions;
            }
        }
        else if (fop == 0x1f4)
        {
            bool full = (tw>>(2*((top+7)&7))&3) != 3;
            if (empty || full)
            {
                // Intel stack rules plus selected AMD full-pair controls:
                // underflow wins, masked #IS replaces BOTH destinations.
                delta = 0x41; sw = (sw&~0x200u)|(empty?0u:0x200u);
                write = push = (cw&1) != 0; result = pushed = new(0xc000000000000000,0xffff);
            }
            else
            {
                int cls = Canon154Class(a); delta = cls is 4 or 5?1u:cls == 6?2u:cls == 0?4u:0u;
                write = push = (delta&~cw&7) == 0;
                if (cls == 5) result = pushed = new(0xc000000000000000,0xffff);
                else if (cls is 3 or 4) result = pushed = new(a.Significand|0x4000000000000000,a.SignExponent);
                else if (cls == 2) { pushed = a; result = new(0x8000000000000000,0x7fff); }
                else if (cls == 0) { pushed = a; result = new(0x8000000000000000,0xffff); }
                else
                {
                    int bits = (int)new BigInteger(a.Significand).GetBitLength();
                    int exponent = Math.Max(1,a.SignExponent&0x7fff)-16383-64+bits;
                    pushed = new(a.Significand<<(64-bits),(ushort)(0x3fff|(a.SignExponent&0x8000)));
                    result = Canon154Integer(BigInteger.Abs(exponent),exponent<0);
                }
                // Normal C1 and C0/C2/C3 remain explicitly undefined/profile-retained.
            }
        }
        else if (fop == 0x1e5)
        {
            uint cc = (tw>>(2*top)&3) == 3 ? 0x4100u : Canon154Class(a) switch
            { 0 => 0x4000u,1 => 0x400u,2 => 0x500u,3 or 4 => 0x100u,6 => 0x4400u,_ => 0 };
            sw = (sw&~0x4700u)|cc|(uint)(a.SignExponent>>15)<<9;
        }
        else if (move)
        {
            if (empty)
            {
                delta = 0x41; sw &= ~0x200u; write = (cw&1) != 0;
                result = new(0xc000000000000000,0xffff);
            }
            else { write = Canon154MoveCondition(fop,flags); result = b; }
        }
        else if (fop is 0x1fc or 0x1fa)
        {
            if (empty) { delta = 0x41; write = (cw&1) != 0; result = new(0xc000000000000000,0xffff); }
            else (result,delta,write) = Canon154Unary(fop,a,cw);
            sw &= ~0x200u;
        }
        else
        {
            bool quiet = (fop&0x7f8) is 0x5e0 or 0x5e8 or 0x3e8 or 0x7e8 || fop == 0x2e9;
            if (fop == 0x1e4) b = default;
            int ac = Canon154Class(a), bc = Canon154Class(b), relation;
            if (empty) { delta = 0x41; relation = 3; }
            else if (ac is 4 or 5 || bc is 4 or 5 || !quiet && (ac == 3 || bc == 3)) { delta = 1; relation = 3; }
            else if (ac == 3 || bc == 3) relation = 3;
            else
            {
                if (ac == 6 || bc == 6) delta = 2;
                int cmp = Canon154Relation(a,b); relation = cmp > 0 ? 0 : cmp < 0 ? 1 : 2;
            }
            bool hold = (delta&1) != 0 && (cw&1) == 0;
            sw &= ~0x200u;
            if (integerFlags)
            {
                flags &= ~0x890u; // unconditional OF/SF/AF clear, including IE
                if (!hold)
                {
                    uint bits = relation switch { 0 => 0u,1 => 1u,2 => 64u,_ => 65u };
                    uint lazy = relation == 3 ? 0u : 0x01000000u;
                    flags = (flags&~0xff000041u)|bits|lazy; // bit2 remains stored/unused
                }
            }
            else if (!hold)
            {
                uint cc = relation switch { 0 => 0u,1 => 0x100u,2 => 0x4000u,_ => 0x4500u };
                sw = (sw&~0x4500u)|cc;
            }
            if ((delta&~cw&3) != 0) pops = 0; // condition writes remain independent of DE/pop
        }
        sw |= delta;
        if ((delta&~cw&63) != 0) sw |= 0x8080;
        if (write)
        {
            Canon154Write(raw,top,result); tw = (tw&~(3u<<(2*top)))|(uint)Canon154Tag(result)<<(2*top);
        }
        if (push)
        {
            top = (top+7)&7; Canon154Write(raw,top,pushed);
            tw = (tw&~(3u<<(2*top)))|(uint)Canon154Tag(pushed)<<(2*top);
        }
        for (int p = 0; p < pops; ++p) { tw |= 3u<<(2*top); top = (top+1)&7; }
        sw = (sw&~0x3800u)|(uint)top<<11;
        CanonicalX87W32(m,648,sw); CanonicalX87W32(m,652,tw); CanonicalX87W32(m,12,flags);
        CanonicalX87W64(m,664,i.Ip); CanonicalX87W32(m,656,(uint)fop);
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
        CanonicalX87W64(m,0,i.Ip+(uint)i.Length); m[8] = 0;
    }

    static void Canon154One(NativeCodeAllocation code,JitInstruction i,byte* fake,FullX87RegisterFile* state,bool probe = false)
    {
        byte[] expected = CanonicalX87Bytes(fake,sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state,192);
        byte[] workBefore = CanonicalX87Bytes((byte*)Canon154Work(state),384);
        Canon154Expected(i,expected,raw);
        ulong epoch = 11; long issued = 0,completed = 0; int stop = 0; var h = FullHeader(i.Ip,code.Entry);
        var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87 = state;
        var actualExit = probe?FloatRunProbe(code,&f):RunFull(code,&f);
        Check(actualExit == FullBlockExit.Continue && issued == 1 && completed == 1 &&
            f.Executed == 1 && f.FaultTrap == 0 && f.HelperIndex == FullBlockFrame.NoHelperIndex && f.MemoryResume == 0 &&
            f.MemoryAssists == 0 && f.NativeMemoryComputations == 0 && f.HostMxcsrSaved == 0,
            $"R154 one issue/commit op={i.Options:x}/src={i.Source.Register}/fop={i.Extra.Immediate:x} exit={actualExit} issued={issued} done={completed} executed={f.Executed} trap={f.FaultTrap} cw={((Blink.Machine*)fake)->fpu.cw:x} sw={((Blink.Machine*)fake)->fpu.sw:x} fatal={*(uint*)((byte*)Canon154Work(state)+280):x} diagnostic={*(uint*)((byte*)Canon154Work(state)+284):x} status={Convert.ToHexString(new ReadOnlySpan<byte>((byte*)Canon154Work(state)+80,16))}");
        CanonicalX87Same(fake,state,expected,raw);
        Check(((uint*)((byte*)Canon154Work(state)+280))[0] == 0,"R154 valid protocol never fatal");
        Check(new ReadOnlySpan<byte>((byte*)Canon154Work(state)+288,96).SequenceEqual(workBefore.AsSpan(288)),
            "R154 caller reserve288..383 remains entirely untouched");
        if (!FullBlockEmitter.CanonicalX87ControlNeedsKernel(i))
            Check(new ReadOnlySpan<byte>(Canon154Work(state),384).SequenceEqual(workBefore),"R154 raw path never fabricates Work/kernel binding");
    }

    static Raw154[] Canon154Values() => [default,new(0,0x8000),new(0x8000000000000000,0x3fff),
        new(0x8000000000000001,0x3fff),new(0xc000000000000001,0xbfff),new(0x8000000000000000,0x4000),
        new(ulong.MaxValue,0x7ffe),new(1,0),new(1,0x8000),new(0x8000000000000001,0),
        new(0x8000000000000000,0x7fff),new(0x8000000000000000,0xffff),
        new(0xc000000000000123,0xffff),new(0x8000000000000123,0x7fff),new(1,0x4000),new(1,0x7fff)];
    static void Canon154Suite(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        Raw154[] values = Canon154Values();
        foreach (byte[] form in Canon154Forms())
        {
            var i = Canon154Classify(form); var call = Canon154Call(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            // All64 masks, PC/RC and all raw classes. TOP cycles; dedicated
            // alias suite below covers the full independent TOP/index product.
            for (int masks = 0; masks < 64; ++masks)
            for (int pc = 0; pc < 4; ++pc)
            for (int rc = 0; rc < 4; ++rc)
            {
                int row = masks*16+pc*4+rc;
                var a = values[(masks+pc+rc)%values.Length]; var b = values[(masks/4+3*rc+7)%values.Length];
                Canon154Seed(fake,state,row&7,(ushort)(0x40|masks|pc<<8|rc<<10));
                Canon154Input(fake,state,0,a);
                if (form.AsSpan().SequenceEqual(new byte[] { 0xd9,0xf4 })) Canon154Input(fake,state,7,default,3);
                if (i.Source.Register != 0) Canon154Input(fake,state,i.Source.Register,b);
                // All CF/ZF combinations and both lazy-PF values, deliberately
                // contradicting the dormant stored FLAGS_PF bit2.
                ((Blink.Machine*)fake)->flags = 0x54000896u|(uint)(row&1)|(uint)(row>>1&1)<<6|(uint)(row>>2&1)<<24;
                Canon154One(code,i,fake,state,row%127 == 0);
            }
            if (form[0] != 0xd9 && (form[0] != 0xda && form[0] != 0xdb || form[1] >= 0xe0))
            foreach (Raw154 a in values)
            foreach (Raw154 b in values)
            foreach (ushort cw in new ushort[] { 0x37f,0x37e,0x37d,0x340 })
            {
                Canon154Seed(fake,state,7,cw); Canon154Input(fake,state,0,a);
                if (i.Source.Register != 0) Canon154Input(fake,state,i.Source.Register,b);
                Canon154One(code,i,fake,state);
            }
            for (int top = 0; top < 8; ++top)
            for (int mask = 0; mask < 64; ++mask)
            foreach (int empty in new[] { -1,0,1,2 })
            {
                Canon154Seed(fake,state,top,(ushort)(0x340|mask));
                Canon154Input(fake,state,0,new(0x8000000000000123,0x7fff)); // SNaN / unsupported other operand
                if (i.Source.Register != 0) Canon154Input(fake,state,i.Source.Register,new(1,0x7fff));
                if (empty is 0 or 2) Canon154Input(fake,state,0,new(0,0x8000),3);
                if (empty is 1 or 2) Canon154Input(fake,state,i.Source.Register,new(0x8000000000000000,0x3fff),3);
                Canon154One(code,i,fake,state);
            }
        }
        Console.WriteLine("CONTROL-PHASE literals/extract/reductions");
        Canon154Literals(fake,state,calls); Canon154ExtractLiterals(fake,state,calls); Canon154ReductionLiterals(fake,state,calls);
        Console.WriteLine("CONTROL-PHASE boundaries/deferred");
        Canon154Boundaries(fake,state,calls); Canon154Deferred(fake,state,calls);
        Console.WriteLine("CONTROL-PHASE register/numeric family returned");
    }

    static void Canon154ExtractLiterals(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        var i = Canon154Classify([0xd9,0xf4]); var call = Canon154Call(i,calls);
        using var code = CreateFullBlock([new(i,X87Kernel:call)]);
        // Independent literal full/empty pair checks for EVERY TOP and mask.
        // Numeric classes in space use the already qualified true80 kernel;
        // this fixture checks new wrapper completion and does not credit it
        // with any prior numeric qualification.
        for (int top = 0; top < 8; ++top)
        for (int masks = 0; masks < 64; ++masks)
        for (int topology = 0; topology < 4; ++topology)
        foreach (Raw154 input in Canon154Values())
        {
            Canon154Seed(fake,state,top,(ushort)(0x340|masks));
            Canon154Input(fake,state,0,input,topology >= 2?3:-1);
            if (topology is 0 or 3) Canon154Input(fake,state,7,default,3);
            if (topology == 3) ((Blink.Machine*)fake)->fpu.tw |= 0xffff;
            byte[] raw = CanonicalX87Bytes((byte*)state,192);
            Canon154One(code,i,fake,state);
            uint sw = ((Blink.Machine*)fake)->fpu.sw;
            if (topology != 0)
            {
                Check((sw&0x241) == (topology == 1?0x241u:0x41u),"R154 FXTRACT underflow wins over occupied push destination");
                if ((masks&1) == 0)
                    Check(((sw>>11)&7) == (uint)top && new ReadOnlySpan<byte>((byte*)state+64,128).SequenceEqual(raw.AsSpan(64)),
                        "R154 FXTRACT unmasked #IS holds TOP and BOTH raw images/padding");
                else
                {
                    int pushed = (top+7)&7;
                    var a = FullX87RegisterFile.Physical(state,pushed); var b = FullX87RegisterFile.Physical(state,top);
                    Check(((sw>>11)&7) == (uint)pushed && a->Significand == 0xc000000000000000 &&
                        b->Significand == 0xc000000000000000 && a->SignExponent == 0xffff && b->SignExponent == 0xffff,
                        "R154 FXTRACT masked #IS writes negative indefinite to BOTH new ST0/ST1");
                }
            }
        }
        Canon154Seed(fake,state,0,0x37f); Canon154Input(fake,state,0,new(0xc000000000000000,0x4001));
        Canon154Input(fake,state,7,default,3); Canon154One(code,i,fake,state);
        Check(((Blink.Machine*)fake)->fpu.sw == 0x5a007f40u &&
            FullX87RegisterFile.Physical(state,7)->Significand == 0xc000000000000000 &&
            FullX87RegisterFile.Physical(state,7)->SignExponent == 0x3fff &&
            FullX87RegisterFile.Physical(state,0)->Significand == 0x8000000000000000 &&
            FullX87RegisterFile.Physical(state,0)->SignExponent == 0x4000,
            "R154 FXTRACT6 -> significand1.5/exponent2, TOP decrements, undefined CC/C1 retained");
    }

    static void Canon154OracleAnchors()
    {
        var plus = Canon154Unary(0x1fc,new(0xa000000000000000,0x4000),0x37f); // 2.5 -> 2 (even)
        Check(plus.Value.Significand == 0x8000000000000000 && plus.Value.SignExponent == 0x4000 && plus.Delta == 32,
            "R154 independent RN even integral literal");
        var up = Canon154Unary(0x1fa,new(0x8000000000000001,0x3fff),0xb7f);
        Check(up.Value.Significand == 0x8000000000000001 && up.Value.SignExponent == 0x3fff && up.Delta == 0x220,
            "R154 independent sqrt full64 lowbit RC+ primary literal");
        Check(Canon154Relation(default,new(0,0x8000)) == 0 &&
            Canon154Relation(new(0x8000000000000001,0),new(0x8000000000000001,1)) == 0,
            "R154 zero signs and pseudo exponent-one equality");
        Check(Canon154MoveCondition(0x3d0,0) && !Canon154MoveCondition(0x3d0,1) &&
            !Canon154MoveCondition(0x3d0,64),"R154 NBE is CF=0 AND ZF=0, not Vol1 legacy OR typo");
    }

    static void Canon154Literals(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        // Literal assertions are separate from the whole-image oracle.
        foreach ((byte op,byte modrm) in new (byte,byte)[] { (0xd8,0xd9),(0xde,0xd9),(0xdf,0xf1),(0xda,0xe9) })
        {
            var i = Canon154Classify([op,modrm]); var call = Canon154Call(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            Canon154Seed(fake,state,7,0x37d); Canon154Input(fake,state,0,new(1,0));
            Canon154Input(fake,state,1,new(0x8000000000000000,0x3fff));
            Canon154One(code,i,fake,state,true);
            Check((((Blink.Machine*)fake)->fpu.sw&0x82ff) == 0x80c2 &&
                ((((Blink.Machine*)fake)->fpu.sw>>11)&7) == 7,"R154 unmasked DE retains TOP despite requested pop");
            bool eflags = op == 0xdf;
            Check(eflags ? (((Blink.Machine*)fake)->flags&0x41) == 1 : (((Blink.Machine*)fake)->fpu.sw&0x4500) == 0x100,
                "R154 unmasked DE still commits less relation");
            Canon154Seed(fake,state,7,0x37e,0x120008d6); Canon154Input(fake,state,0,new(0x8000000000000123,0x7fff));
            Canon154Input(fake,state,1,new(1,0)); uint old = ((Blink.Machine*)fake)->fpu.sw;
            Canon154One(code,i,fake,state);
            Check((((Blink.Machine*)fake)->fpu.sw&0x4500) == (old&0x4500),"R154 Intel unmasked IE holds status relation");
            if (eflags) Check(((Blink.Machine*)fake)->flags == (0x120008d6u&~0x890u),
                "R154 Intel IE holds CF/ZF/lazyPF and clears OF/SF/AF; AMD discrepancy retained");
        }
        foreach (byte opcode in new byte[] { 0xda,0xdb })
        foreach (byte modrm in new byte[] { 0xc1,0xc9,0xd1,0xd9 })
        foreach (int empty in new[] { 0,1 })
        foreach (bool masked in new[] { false,true })
        {
            var i = Canon154Classify([opcode,modrm]); var call = Canon154Call(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            int fop = (opcode&7)<<8|modrm;
            uint falseFlags = opcode == 0xda ? 0x01000000u : 0x41u;
            Check(!Canon154MoveCondition(fop,falseFlags),"R154 explicit false cc fixture");
            Canon154Seed(fake,state,6,masked?(ushort)0x37f:(ushort)0x37e,falseFlags);
            Canon154Input(fake,state,0,new(0x8000000000000000,0x3fff));
            Canon154Input(fake,state,1,new(0x8000000000000000,0x4000));
            Canon154Input(fake,state,empty,default,3); Canon154One(code,i,fake,state,true);
            Check((((Blink.Machine*)fake)->fpu.sw&0x82ff) == (masked?0x41u:0x80c1u),
                "R154 AP526 reads both before cc: false condition does not hide stack underflow");
            var x = *FullX87RegisterFile.Physical(state,6);
            if (masked) Check(x.Significand == 0xc000000000000000 && x.SignExponent == 0xffff,
                "R154 masked false FCMOV stack fault writes exact indefinite ST0");
        }
        {
            var i = Canon154Classify([0xd9,0xe5]);
            using var code = CreateFullBlock([new(i)]);
            foreach ((Raw154 x,uint cc) in new (Raw154,uint)[] {
                (new(0,0),0x4000),(new(1,0),0x4400),(new(0x8000000000000001,0),0x4400),
                (new(0x8000000000000001,1),0x400),(new(0x8000000000000000,0x7fff),0x500),
                (new(0xc000000000000001,0x7fff),0x100),(new(0x8000000000000001,0x7fff),0x100),
                (new(1,0x7fff),0),(new(1,0x4000),0) })
            foreach (int tag in new[] { 0,1,2,3 })
            foreach (int sign in new[] { 0,1 })
            {
                Canon154Seed(fake,state,7,0x340,0x120008d6);
                Canon154Input(fake,state,0,new(x.Significand,(ushort)(x.SignExponent|sign<<15)),tag);
                uint before = ((Blink.Machine*)fake)->fpu.sw;
                Canon154One(code,i,fake,state);
                Check((((Blink.Machine*)fake)->fpu.sw&0x4700) == ((tag == 3?0x4100u:cc)|(uint)sign<<9),
                    "R154 primary FXAM literal classes/raw sign including empty, independent of nonempty tag");
                Check((((Blink.Machine*)fake)->fpu.sw&~0x4700u) == (before&~0x4700u) &&
                    ((Blink.Machine*)fake)->flags == 0x120008d6,"R154 FXAM no new exceptions/quieting/flags");
            }
        }
    }

    static void Canon154Boundaries(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (byte[] form in Canon154Forms())
        {
            var i = Canon154Classify(form); var call = Canon154Call(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string boundary in new[] { "stop","attention","budget","epoch","preissued","preissued-stop",
                "mf1","mf2","mf4","mf8","mf16","mf32","sf-only","masked-pending","es-only",
                "null","null-header","owner","inactive","bad-version","pc01","tf","rf","mode" })
            {
                Canon154Seed(fake,state,5,0x37f);
                Canon154Input(fake,state,0,new(0x8000000000000000,0x3fff));
                if (i.Source.Register != 0) Canon154Input(fake,state,i.Source.Register,new(0x8000000000000000,0x4000));
                bool pending = boundary.StartsWith("mf",StringComparison.Ordinal);
                if (pending)
                {
                    uint bit = uint.Parse(boundary.AsSpan(2)); ((Blink.Machine*)fake)->fpu.sw |= bit;
                    ((Blink.Machine*)fake)->fpu.cw &= ~bit; Canon154Input(fake,state,0,default,3);
                }
                if (boundary == "masked-pending") ((Blink.Machine*)fake)->fpu.sw |= 63;
                if (boundary == "es-only") ((Blink.Machine*)fake)->fpu.sw |= 0x8080;
                if (boundary == "attention") ((Blink.Machine*)fake)->attention = true;
                if (boundary == "pc01") ((Blink.Machine*)fake)->fpu.cw = 0x17f;
                if (boundary == "tf") ((Blink.Machine*)fake)->flags |= 0x100;
                if (boundary == "rf") ((Blink.Machine*)fake)->flags |= 0x10000;
                if (boundary == "mode") ((Blink.Machine*)fake)->mode.omode = Blink.XED_MODE_LEGACY;
                if (boundary == "inactive") state->ProfileActive = 0;
                if (boundary == "bad-version") state->Version = 99;
                ulong epoch = 11; long issued = boundary.StartsWith("preissued",StringComparison.Ordinal)?1:0,completed = 0;
                int stop = boundary is "stop" or "preissued-stop"?1:0; var h = FullHeader(i.Ip,code.Entry);
                var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:boundary is "budget" or "preissued"?0UL:1UL,limit:1);
                f.X87 = boundary == "null"?null:state; if (boundary == "null-header") f.Header = null;
                if (boundary == "owner") { h.OwnerCookie = f.OwnerCookie = 74; state->OwnerToken = 74; }
                if (boundary.StartsWith("preissued",StringComparison.Ordinal)) f.FirstPreissued = 1;
                if (boundary == "epoch") ++epoch;
                bool kernel = FullBlockEmitter.CanonicalX87ControlNeedsKernel(i);
                bool ownerDenied = boundary == "owner" && kernel;
                bool pcDenied = false; // PC01 is software PC64, with raw CW preserved.
                bool works = boundary is "preissued" or "sf-only" or "masked-pending" or "es-only" ||
                    boundary is "pc01" or "tf" or "rf" || boundary == "owner" && !kernel;
                byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state,192);
                byte[] work = CanonicalX87Bytes((byte*)Canon154Work(state),384); if (works) Canon154Expected(i,m,raw);
                FullBlockExit want = boundary switch { "stop" or "preissued-stop" => FullBlockExit.Stop,
                    "attention" => FullBlockExit.Attention,"budget" => FullBlockExit.Budget,
                    _ => pending?FullBlockExit.GuestFault:works?FullBlockExit.Continue:FullBlockExit.Invalidated };
                bool noIssue = boundary is "stop" or "attention" or "epoch" or "null" or "null-header" or "inactive" or
                    "bad-version" or "mode" || ownerDenied || pcDenied;
                Check(FloatRunProbe(code,&f) == want && completed == (works?1:0) && f.Executed == (works?1UL:0UL) &&
                    issued == (noIssue?0:1),"R154 context/owner/PC/work/stop/budget/preissue/pending MF priority");
                if (pending) Check(f.FaultTrap == 16 && f.FaultCode == 0 && f.FaultAddress == i.Ip,"R154 prior six-mask MF is immediate before all effects");
                if (boundary.StartsWith("preissued",StringComparison.Ordinal))
                    Check(f.FirstPreissued == (boundary == "preissued"?0u:1u),"R154 consume reservation only on admission");
                CanonicalX87Same(fake,state,m,raw);
                if (!works) Check(new ReadOnlySpan<byte>(Canon154Work(state),384).SequenceEqual(work),"R154 denied/pending Work untouched");
            }
        }
    }

    static void Canon154Deferred(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach ((byte[] form,Raw154 input,ushort cw) in new (byte[],Raw154,ushort)[] {
            ([0xd8,0xd9],new(1,0),0x37d),([0xdf,0xf1],new(0x8000000000000123,0x7fff),0x37e),
            ([0xd9,0xfc],new(0xc000000000000000,0x3fff),0x35f),
            ([0xd9,0xfa],new(0x8000000000000000,0x4000),0x35f) })
        {
            var i = Canon154Classify(form); var call = Canon154Call(i,calls);
            Check(FullCanonicalX87StackLowering.TryClassify(Ip+2,[0x9b],out var wait,out _),"R154 next FWAIT");
            using var code = CreateFullBlock([new(i,X87Kernel:call),new(wait)]);
            Canon154Seed(fake,state,6,cw); Canon154Input(fake,state,0,input);
            if (i.Source.Register != 0) Canon154Input(fake,state,i.Source.Register,new(0x8000000000000000,0x3fff));
            byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state,192);
            Canon154Expected(i,m,raw);
            ulong epoch = 11; long issued = 0,completed = 0; int stop = 0; var h = FullHeader(Ip,code.Entry);
            var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop); f.X87 = state;
            Check(FloatRunProbe(code,&f) == FullBlockExit.GuestFault && issued == 2 && completed == 1 && f.Executed == 1 &&
                f.FaultTrap == 16 && f.FaultAddress == Ip+2,"R154 newly raised exception completes, next waiting instruction MF");
            CanonicalX87Same(fake,state,m,raw);
        }
    }

    static void Canon154Emission(FullX87KernelCall[] calls)
    {
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity], common = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] { false,true })
        foreach (bool shared in new[] { false,true })
        {
            FullBlockEmitter.EmitScaffold(common,out var layout,windows); var scaffold = layout.Resolve((nint)0x10000000);
            foreach (byte[] form in Canon154Forms())
            {
                var i = Canon154Classify(form); var call = Canon154Call(i,calls); FullBlockInstruction[] body = [new(i,X87Kernel:call)];
                int length;
                bool good = shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out length,windows):
                    FullBlockEmitter.TryEmit(body,staging,out length,windows);
                Check(good,"R154 both ABIs/shared and standalone actual body encodings");
                int transfers = 0;
                for (int p = 0; p+7 < length; ++p)
                {
                    if (staging[p] != 0x48 || staging[p+1] != 0x8d || staging[p+2] != 5) continue;
                    int target = p+7+BinaryPrimitives.ReadInt32LittleEndian(staging.AsSpan(p+3));
                    if (target < 12 || target+21 > length || !staging.AsSpan(target,7).SequenceEqual(new byte[] {
                        0xf3,0x0f,0x1e,0xfa,0x4d,0x89,0xd9 })) continue;
                    ++transfers;
                    Check(staging.AsSpan(p+7,4).SequenceEqual(new byte[] { 0x49,0x89,0x41,88 }) &&
                        staging[target-12] == 0x48 && staging[target-11] == 0xb8 &&
                        BinaryPrimitives.ReadUInt64LittleEndian(staging.AsSpan(target-10)) == (ulong)call.Entry &&
                        staging[target-2] == 0xff && staging[target-1] == 0xe0,"R154 exact native LEA continuation and literal typed Entry JMP");
                    Check(staging.AsSpan(target+7,7).SequenceEqual(new byte[] { 0x4d,0x8b,0x91,0,1,0,0 }) &&
                        staging.AsSpan(target+14,7).SequenceEqual(new byte[] { 0x4d,0x8b,0x99,8,1,0,0 }),"R154 restore R10/R11 before all architecture writes");
                }
                Check(transfers == (call == default?0:1),"R154 raw paths no kernel, numeric exactly one literal JMP");
                foreach (int cut in new[] { 0,1,7,length-1 })
                {
                    int partial;
                    Check(!(shared?FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),in scaffold,out partial,windows):
                        FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),out partial,windows)) && partial == 0,"R154 short span never publishes partial body");
                }
                for (int warm = 0; warm < 12; ++warm)
                {
                    FullCanonicalX87ControlLowering.TryClassify(Ip,form,out _,out _);
                    if (shared) FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows);
                    else FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int warm = 0; warm < 16; ++warm)
                {
                    good &= FullCanonicalX87ControlLowering.TryClassify(Ip,form,out _,out _);
                    good &= shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):
                        FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                Check(good && GC.GetAllocatedBytesForCurrentThread() == before,"R154 warmed decoder/body emitter allocation zero");
            }
        }
    }

    static void Canon154Fatal(byte* fake,FullX87RegisterFile* state)
    {
        foreach (bool shared in new[] { false,true })
        {
            NativeCodeAllocation? common = null;
            if (shared) { common = CreateFullScaffold(out var addresses); fullOwnerScaffold = addresses; }
            try
            {
                foreach ((byte[] form,int offset,uint poison) in new (byte[],int,uint)[] {
                    ([0xd8,0xd9],12,1),([0xd8,0xd9],48,4),([0xd8,0xd9],54,0x4701),
                    ([0xd8,0xd9],52,0x4500),([0xd8,0xd9],80,0x80),([0xd8,0xd9],82,1),
                    ([0xd8,0xd9],84,2049),([0xd8,0xd9],64,1),([0xdf,0xf1],60,0x8d1),
                    ([0xdf,0xf1],56,0x45),([0xd9,0xfc],84,0),([0xd9,0xfc],80,0x200),
                    ([0xd9,0xfc],60,1),([0xd9,0xfa],84,1),([0xd9,0xfa],82,0x80),
                    ([0xd9,0xfa],52,0),([0xd9,0xfa],0,99),([0xd9,0xfa],8,0),
                    ([0xd9,0xfd],244,0),([0xd9,0xfd],242,0x4700),([0xd9,0xfd],82,32),
                    ([0xd9,0xf8],240,0x47000401),([0xd9,0xf8],244,4),([0xd9,0xf5],80,32),([0xd9,0xf4],84,2),([0xd9,0xf4],80,3),
                    ([0xd9,0xf4],82,1),([0xd9,0xf4],72,0x7fff),([0xd9,0xf4],76,1) })
                foreach (bool preissued in new[] { false,true })
                foreach (uint returned in new uint[] { 0,1 })
                {
                    var i = Canon154Classify(form); bool compare = form[0] != 0xd9;
                    var bytes = new List<byte> { 0xf3,0x0f,0x1e,0xfa };
                    void Put(int at,uint value)
                    {
                        bytes.AddRange(new byte[] { 0x41,0xc7,0x83 });
                        for (int k = 0; k < 4; ++k) bytes.Add((byte)((uint)at>>(8*k)));
                        for (int k = 0; k < 4; ++k) bytes.Add((byte)(value>>(8*k)));
                    }
                    // A fully initialized valid ONE vs TWO comparison/round/root
                    // response is poisoned one field at a time, not stale A5.
                    for (int at = 48; at < 88; at += 4) Put(at,0);
                    Put(12,0);
                    if (compare)
                    {
                        Put(48,1); Put(52,form[0] == 0xdf?0x02000000u:0x47000100u);
                        if (form[0] == 0xdf) { Put(56,1); Put(60,0x8d5); }
                        Put(84,5);
                    }
                    else
                    {
                        Put(52,0x80000000); Put(56,0x3fff);
                        if (form[1] is 0xfd or 0xf8 or 0xf5)
                        {
                            Put(84,17); Put(240,form[1] == 0xfd?0x02000000u:0x47000000u);
                            Put(244,form[1] == 0xfd?1u:form[1] == 0xf8?2u:4u);
                        }
                        else if (form[1] == 0xf4) Put(84,30); // exponent0, Normal=1
                        else Put(84,form[1] == 0xfc?6u:8213u);
                    }
                    Put(offset,poison);
                    bytes.AddRange(new byte[] { 0x49,0xba,0,0,0,0,0,0,0,0,0xb8 }); // clobber R10
                    for (int k = 0; k < 4; ++k) bytes.Add((byte)(returned>>(8*k)));
                    bytes.AddRange(new byte[] { 0x41,0xff,0x63,88 });
                    Check(NativeCodeAllocation.TryCreate(bytes.ToArray(),out var kernel,out var reason),reason);
                    using (kernel)
                    {
                        var call = new FullX87KernelCall((byte)FullBlockEmitter.CanonicalX87ControlKernel(i),kernel!.Entry,73,bytes.Count);
                        using var code = CreateFullBlock([new(i,X87Kernel:call)]);
                        Canon154Seed(fake,state,2,0x37f); Canon154Input(fake,state,0,new(0x8000000000000000,0x3fff));
                        if (form[0] == 0xd9 && form[1] == 0xf4) Canon154Input(fake,state,7,default,3);
                        if (i.Source.Register != 0) Canon154Input(fake,state,i.Source.Register,new(0x8000000000000000,0x4000));
                        byte[] m = CanonicalX87Bytes(fake,sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state,192);
                        ulong epoch = 11; long issued = preissued?1:0,completed = 0; int stop = 0; var h = FullHeader(Ip,code.Entry);
                        var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:preissued?0UL:1UL,limit:1);
                        f.X87 = state; f.FirstPreissued = preissued?1u:0;
                        Check(FloatRunProbe(code,&f) == FullBlockExit.Invalidated && issued == 1 && completed == 0 && f.Executed == 0 &&
                            f.FirstPreissued == 1 && f.FaultTrap == 0 && ((uint*)((byte*)state+192+280))[0] == 0x58383746 &&
                            ((uint*)((byte*)state+192+284))[0] != 0,"R154 protocol poison internal fatal preserves original issue; no CPU fault or retry");
                        CanonicalX87Same(fake,state,m,raw);
                    }
                }
            }
            finally { fullOwnerScaffold = default; common?.Dispose(); }
        }
    }
}
