using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using E80 = Managed.Emulation.Execution.FullExtended80;
using E80Op = Managed.Emulation.Execution.FullExtended80Operation;

unsafe partial class Program
{
    // AUTHORED_UNRUN. Parent selects after adopting its FullBlockInstruction,
    // pre-Issue Work/owner/PC admission and call-passing seams. No guest profile
    // enablement: only synthetic isolated Active1 storage is ever supplied.
    static void CheckFullCanonicalX87Arithmetic(byte* machine)
    {
        _ = machine;
        Check(!RuntimeHelpers.IsReferenceOrContainsReferences<FullX87KernelCall>() &&
            !RuntimeHelpers.IsReferenceOrContainsReferences<FullBlockInstruction>(), "R148 IR contains no managed owner/binding");
        Check(sizeof(FullBlockFrame) == 416 && sizeof(FullX87RegisterFile) == 192 &&
            sizeof(FullExtended80WorkFrame) == 384 && sizeof(FullX87NativeStorage) == 576,
            "R148 exact ABI7/file192/exclusiveWork384 owned576 layout");
        ArithmeticDecode();
        using var io = new SemanticTestIo();
        using var authority = new FullCodeAuthority(io.Value, 4096, 2, 4L << 20);
        using var store = new RetainedTranslationStore(RetainedOptions() with { PayloadBytes = 0 });
        using var catalog = new FullX87KernelCatalog(authority, store, true);
        var calls = new FullX87KernelCall[4];
        FullX87KernelBinding saved;
        lock (authority.Gate)
        {
            Check(catalog.Initialize(new byte[FullX87KernelCatalog.StagingBytes], out _), "R148 eager all32 real RX kernels");
            Check(catalog.Count == FullX87KernelCatalog.MaximumKernels && catalog.AdmissionOpen, "R148 no partial catalog publication");
            for (int key = 0; key < 4; ++key)
            {
                var binding = catalog.Find((FullX87KernelKey)key);
                Check(catalog.IsValid(in binding), "R148 authenticate binding under SAME authority gate");
                calls[key] = new((byte)key, binding.Entry, 73, binding.CodeBytes);
            }
            saved = catalog.Find(FullX87KernelKey.Add);
        }
        bool unlocked = false;
        try { _ = saved.Entry; } catch (InvalidOperationException) { unlocked = true; }
        Check(unlocked, "R148 raw Entry cannot be recovered outside Authority.Gate");
        var token = RetainedPublish(store, 0x480000);
        Check(store.TryAcquireToken(token, out var root), "R148 retains original real TB root lease");
        Check(store.TryBeginNativeEpoch(out var epochLease), "R148 retains original real TB native epoch");
        bool enrolled;
        lock (authority.Gate) enrolled = catalog.AdmissionOpen && authority.TryEnterNative(authority.Epoch);
        Check(enrolled, "R148 original authority native enrollment spans body/kernel/continuation");
        byte* fake = (byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine) + 15) & ~15), 16);
        // Allocate exactly576, no hidden spill area or overlap with Machine/XMM.
        var state = (FullX87RegisterFile*)NativeMemory.AlignedAlloc(576, 16);
        try
        {
            Check(fake != null && state != null && !fullOwnerScaffold.IsValid, "R148 owned exact fake allocations/standalone entry");
            ArithmeticScaffoldSuite(fake, state, calls);
            using (var common = CreateFullScaffold(out var addresses))
            {
                fullOwnerScaffold = addresses;
                try { ArithmeticScaffoldSuite(fake, state, calls); }
                finally { fullOwnerScaffold = default; }
            }
            ArithmeticEncodings(calls);
            ArithmeticBadProtocol(fake, state); // controlled invalid test kernels only
        }
        finally
        {
            NativeMemory.AlignedFree(state); NativeMemory.AlignedFree(fake);
            if (enrolled) authority.ExitNative();
            epochLease.Dispose(); root.Dispose();
        }
        lock (authority.Gate)
        {
            Check(catalog.IsValid(in saved), "R148 catalog survives all body disposal and TB leases");
            catalog.CloseAdmission();
            bool closed = false;
            try { _ = saved.Entry; } catch (InvalidOperationException) { closed = true; }
            Check(closed && !catalog.AdmissionOpen, "R148 closed lifetime rejects cached binding access");
        }
        store.Dispose(); catalog.Dispose(); // only successful store barrier permits unmap
        Check(catalog.ChargedBytes == 0, "R148 no live kernel allocation outlives final successful disposal");
    }

    static void ArithmeticScaffoldSuite(byte* fake, FullX87RegisterFile* state, FullX87KernelCall[] calls)
    {
        ArithmeticAliases(fake, state, calls);
        ArithmeticMath(fake, state, calls);
        ArithmeticLiterals(fake, state, calls);
        ArithmeticStackPriorityLiterals(fake, state, calls);
        ArithmeticBoundaries(fake, state, calls);
        ArithmeticDeferred(fake, state, calls);
    }

    // This test grammar is independently enumerated from documented D8/DC/DE
    // opcode rows. It deliberately does not use product TryOperation/enum math.
    static IEnumerable<byte[]> ArithmeticForms()
    {
        foreach (byte opcode in new byte[] { 0xd8, 0xdc, 0xde })
        foreach (byte baseModrm in new byte[] { 0xc0, 0xc8, 0xe0, 0xe8, 0xf0, 0xf8 })
        for (int n = 0; n < 8; ++n) yield return [opcode, (byte)(baseModrm + n)];
    }
    static JitInstruction ArithmeticClassify(ReadOnlySpan<byte> bytes, ulong ip = Ip)
    {
        Check(FullCanonicalX87ArithmeticLowering.TryClassify(ip, bytes, out var i, out var reason), reason ?? "R148 classify");
        Check(FullBlockEmitter.ValidateCanonicalX87Arithmetic(i), "R148 exact typed arithmetic IR");
        return i;
    }
    static FullX87KernelCall ArithmeticCall(JitInstruction i, FullX87KernelCall[] calls)
        => calls[(int)FullBlockEmitter.CanonicalX87ArithmeticKernel(i)];

    static void ArithmeticDecode()
    {
        int count = 0;
        foreach (byte[] form in ArithmeticForms())
        {
            ++count;
            foreach (byte[] prefix in new byte[][] { [], [0x66], [0x67,0x64,0x4f], [0x4f,0x66,0x40],
                [0x26,0x2e,0x36,0x3e,0x64,0x65,0x66,0x67,0x40,0x4f,0x66,0x67,0x40] })
            {
                byte[] b = new byte[prefix.Length + 2]; prefix.CopyTo(b, 0); form.CopyTo(b, prefix.Length);
                var i = ArithmeticClassify(b);
                uint fop = (uint)((form[0] & 7) << 8 | form[1]);
                Check(i.Length == b.Length && i.Width == 80 && i.Source.Register == (form[1] & 7) &&
                    i.Extra.Immediate == fop && ((i.Options >> 16) & 1) == (form[0] == 0xde ? 1u : 0u),
                    "R148 all144 forms, logical0..7/no-operand aliases/prefixes/exact complete11bit FOP");
                foreach (var bad in new[] { i with { Width = 64 }, i with { Length = 1 }, i with { Length = 16 },
                    i with { Options = i.Options | 0x20000 }, i with { Source = new(8,0) },
                    i with { Source = i.Source with { IsMemory = true } }, i with { SourceWidth = 80 },
                    i with { Destination = new(1,0) }, i with { Target = 1 }, i with { Condition = 1 },
                    i with { Address = new(1,2,0,0) }, i with { Extra = new(0,fop ^ 0x100,IsImmediate:true) } })
                {
                    bool rejected = false;
                    try { FullBlockEmitter.ValidateCanonicalX87Arithmetic(bad); } catch (ArgumentException) { rejected = true; }
                    Check(rejected, "R148 malformed recognized arithmetic IR refused before publication");
                }
                var valid = new FullX87KernelCall((byte)FullBlockEmitter.CanonicalX87ArithmeticKernel(i), (nint)0x1000, 73, 32);
                foreach (var bad in new[] { default(FullX87KernelCall), valid with { Entry = 0 }, valid with { OwnerCookie = 0 },
                    valid with { CodeBytes = 0 }, valid with { CodeBytes = FullX87KernelCatalog.StagingBytes + 1 }, valid with { Key = (byte)(valid.Key ^ 1) } })
                {
                    bool rejected = false;
                    try { FullBlockEmitter.ValidateCanonicalX87Arithmetic(i, in bad); } catch (ArgumentException) { rejected = true; }
                    Check(rejected, "R148 kernel operation/key/address/owner/length tuple denials");
                }
            }
        }
        Check(count == 144, "R148 exactly eighteen documented register groups times eight indices");
        foreach (byte[] b in new byte[][] { [], [0xd8], [0xd8,0xc0,0], [0xf0,0xd8,0xc0], [0xf2,0xd8,0xc0],
            [0xf3,0xd8,0xc0], [0x9b,0xd8,0xc0], [0xd8,0x00], [0xdc,0x00], [0xde,0x00],
            [0xd8,0xd0], [0xde,0xd9], [0xda,0xc0], [0xdb,0xc0], [0xdd,0xe0], [0xdf,0xc0] })
            Check(!FullCanonicalX87ArithmeticLowering.TryClassify(Ip, b, out _, out _), "R148 exact span/memory/compare/FCMOV/conversion boundaries");
        var historical = new JitInstruction(Ip,2,JitOperation.X87,80,default,default,Options:0x4001);
        Check(!FullBlockEmitter.CanInlineCanonicalX87Arithmetic(historical), "R148 no old R116 arithmetic alias admission");
    }

    static FullExtended80WorkFrame* ArithmeticWork(FullX87RegisterFile* state) => (FullExtended80WorkFrame*)((byte*)state + 192);
    static void ArithmeticSeed(byte* fake, FullX87RegisterFile* state, int top, ushort cw)
    {
        new Span<byte>(state,576).Fill(0xa5);
        CanonicalX87Seed(fake,state,top,cw);
        // Seed sticky masked flags + all undefined CC/SF/high SW bits while
        // avoiding a prior pending unmasked exception unless a test asks for it.
        ((Blink.Machine*)fake)->fpu.sw = 0x5a004740u | (uint)top << 11;
        ArithmeticWork(state)->Continuation = 0;
        ((uint*)((byte*)ArithmeticWork(state)+280))[0] = 0;
    }
    static void ArithmeticInput(byte* fake, FullX87RegisterFile* state, int logical, E80 value, int tag = -1)
    {
        uint sw = ((Blink.Machine*)fake)->fpu.sw;
        int p = ((int)(sw >> 11) + logical) & 7;
        var slot = FullX87RegisterFile.Physical(state,p);
        slot->Significand = value.Significand; slot->SignExponent = value.SignExponent;
        if (tag < 0) tag = ArithmeticResultTag(value);
        uint tw = unchecked((uint)((Blink.Machine*)fake)->fpu.tw);
        ((Blink.Machine*)fake)->fpu.tw = unchecked((int)((tw & ~(3u << (2*p))) | (uint)tag << (2*p)));
    }
    static int ArithmeticResultTag(E80 value)
    {
        int exp = value.SignExponent & 0x7fff;
        if (exp == 0 && value.Significand == 0) return 1;
        return exp is > 0 and < 0x7fff && (value.Significand >> 63) != 0 ? 0 : 2;
    }

    // Test-only mathematical operand mapping uses literal opcode rows, not
    // product enum/destination/reversal calculations. No product math/helper.
    static (E80Op Op,int Left,int Right,int Destination,bool Pop) ArithmeticMeaning(uint fop)
    {
        int n = (int)fop & 7; bool top = (fop >> 8) == 0, pop = (fop >> 8) == 6;
        return ((int)fop & 0xf8,top) switch
        {
            (0xc0,_) => (E80Op.Add,top?0:n,top?n:0,top?0:n,pop),
            (0xc8,_) => (E80Op.Multiply,top?0:n,top?n:0,top?0:n,pop),
            (0xe0,true) => (E80Op.Subtract,0,n,0,false),
            (0xe8,true) => (E80Op.Subtract,n,0,0,false),
            (0xf0,true) => (E80Op.Divide,0,n,0,false),
            (0xf8,true) => (E80Op.Divide,n,0,0,false),
            (0xe0,false) => (E80Op.Subtract,0,n,n,pop),
            (0xe8,false) => (E80Op.Subtract,n,0,n,pop),
            (0xf0,false) => (E80Op.Divide,0,n,n,pop),
            _ => (E80Op.Divide,n,0,n,pop)
        };
    }
    static E80 ArithmeticRead(byte[] raw,int physical) => new(
        BinaryPrimitives.ReadUInt64LittleEndian(raw.AsSpan(64+physical*16)),
        BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(72+physical*16)));

    // Independent COMPLETION table, Intel093 4.9.2/8.5.4-6 and AMD Volume1
    // unmasked register responses: pre faults suppress; post faults store/pop.
    // ReferenceE80 supplies exact BigInteger numeric results only. Completion
    // never calls product validation/stack/status helpers or managed emulation.
    static void ArithmeticExpected(JitInstruction i,byte[] m,byte[] raw)
    {
        uint sw = CanonicalX87U32(m,648), tw = CanonicalX87U32(m,652);
        ushort cw = (ushort)CanonicalX87U32(m,672);
        var meaning = ArithmeticMeaning((uint)i.Extra.Immediate);
        int top = (int)(sw >> 11) & 7, l = (top + meaning.Left) & 7, r = (top + meaning.Right) & 7;
        int d = (top + meaning.Destination) & 7;
        bool empty = ((tw >> (2*l)) & 3) == 3 || ((tw >> (2*r)) & 3) == 3;
        E80 result; uint delta; bool write;
        if (empty)
        {
            result = new(0xc000000000000000,0xffff); delta = 0x41; write = (cw & 1) != 0;
        }
        else
        {
            var numeric = ReferenceE80(meaning.Op,ArithmeticRead(raw,l),ArithmeticRead(raw,r),cw);
            Check(numeric.Admission == FullExtended80Admission.Complete,"R148 independent BigInteger numeric oracle admits valid context");
            uint flags = (uint)numeric.NormalStatus & 63;
            int stopping = (flags & ~cw & 1) != 0 ? 1 : (flags & ~cw & 4) != 0 ? 4 : (flags & ~cw & 2) != 0 ? 2 : 0;
            write = stopping == 0;
            // Highest stopping pre-condition reports no lower post-condition.
            if (!write) { result = default; delta = (uint)stopping; }
            else if ((numeric.Classification & 8) != 0 && (cw & 8) == 0 ||
                     (numeric.Classification & 4) != 0 && (cw & 16) == 0)
            { result = numeric.Wrapped; delta = numeric.WrappedStatus; }
            else { result = numeric.Normal; delta = numeric.NormalStatus; }
        }
        sw = (sw & ~0x200u) | delta;
        if ((delta & ~cw & 63) != 0) sw |= 0x8080;
        // Metadata belongs to this attempted instruction, even when its newly
        // generated fault is deferred and suppresses destination and pop.
        CanonicalX87W64(m,664,i.Ip); CanonicalX87W32(m,656,(uint)i.Extra.Immediate);
        BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40),BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
        if (write)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(raw.AsSpan(64+d*16),result.Significand);
            BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(72+d*16),result.SignExponent);
            tw = (tw & ~(3u << (2*d))) | (uint)ArithmeticResultTag(result) << (2*d);
            if (meaning.Pop)
            {
                tw |= 3u << (2*top);
                sw = (sw & ~0x3800u) | (uint)((top+1)&7) << 11;
            }
        }
        CanonicalX87W32(m,648,sw); CanonicalX87W32(m,652,tw);
        CanonicalX87W64(m,0,i.Ip+(uint)i.Length); m[8] = 0; // parent's ordinary commit
    }

    static void ArithmeticOne(NativeCodeAllocation code,JitInstruction i,byte* fake,FullX87RegisterFile* state,bool probe = false)
    {
        byte[] expected = CanonicalX87Bytes(fake,sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state,192);
        ArithmeticExpected(i,expected,raw);
        ulong epoch = 11; long issued = 0,completed = 0; int stop = 0; var h = FullHeader(i.Ip,code.Entry);
        var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,limit:1); f.X87 = state;
        Check((probe ? FloatRunProbe(code,&f) : RunFull(code,&f)) == FullBlockExit.Continue &&
            issued == 1 && completed == 1 && f.Executed == 1 && f.FaultTrap == 0 &&
            f.HelperIndex == FullBlockFrame.NoHelperIndex && f.MemoryResume == 0 && f.MemoryAssists == 0 &&
            f.NativeMemoryComputations == 0 && f.HostMxcsrSaved == 0,
            "R148 real ordinary native arithmetic exact single issue/completion; no callback or memory assist");
        CanonicalX87Same(fake,state,expected,raw); // exact file padding and every Machine byte, incl XMM/double shadow
        var work = ArithmeticWork(state);
        Check(((uint*)((byte*)work+280))[0] == 0,"R148 valid input never produces internal fatal protocol");
        bool callerReserveIntact = true;
        foreach (byte guard in new ReadOnlySpan<byte>((byte*)work+288,96)) callerReserveIntact &= guard == 0xa5;
        Check(callerReserveIntact,"R148 shared bridge/kernel preserve ALL caller-owned bytes288..383");
        if (work->Version == 1)
            Check(work->Size == 384 && work->Continuation == 0 &&
                ((ulong*)((byte*)work+256))[0] == (ulong)(nuint)(&f) &&
                ((ulong*)((byte*)work+264))[0] == (ulong)(nuint)fake &&
                ((ulong*)((byte*)work+272))[0] == (ulong)(nuint)state,
                "R148 work v1/384 exact saved frame/Machine/file and cleared native continuation before RET");
    }

    static void ArithmeticAliases(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (byte[] form in ArithmeticForms())
        {
            var i = ArithmeticClassify(form); var call = ArithmeticCall(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            for (int top = 0; top < 8; ++top)
            for (int a = 0; a < 4; ++a)
            for (int b = 0; b < 4; ++b)
            foreach (ushort cw in new ushort[] { 0x37f,0x37e })
            {
                ArithmeticSeed(fake,state,top,cw);
                // Other operand can be unsupported while empty outranks it.
                ArithmeticInput(fake,state,0,CanonicalX87Value(top,a),a);
                ArithmeticInput(fake,state,i.Source.Register,CanonicalX87Value((top+i.Source.Register)&7,b),b);
                ArithmeticOne(code,i,fake,state);
            }
        }
    }

    static IEnumerable<(E80 A,E80 B)> ArithmeticNumbers()
    {
        E80 one = new(0x8000000000000000,0x3fff), two = new(0x8000000000000000,0x4000);
        yield return (one,two); yield return (two,one);
        for (int signs = 1; signs < 4; ++signs)
        {
            E80 a = one, b = two;
            a.SignExponent |= (ushort)((signs&1)<<15); b.SignExponent |= (ushort)((signs>>1)<<15);
            yield return (a,b);
        }
        yield return (new(0,0x8000),default); yield return (default,new(0,0x8000));
        yield return (new(ulong.MaxValue,0xfffe),two); // negative overflow and RC
        yield return (new(0x8000000000000000,0x8001),two); // negative tiny
        yield return (new(0x8000000000000001,1),two); // tiny threshold neighbor
        yield return (new(ulong.MaxValue,0),new(0x8000000000000000,0x3fff));
        yield return (one,new(0x8000000000000000,0x3fbf)); // nearest halfway
        yield return (new(0x8000000000000001,0x3fff),new(0x8000000000000000,0x3fbf));
        yield return (new(ulong.MaxValue,0x7ffe),two); // range
        yield return (new(0x8000000000000000,1),two); // exact tiny Divide
        yield return (new(1,0),two); yield return (new(0x8000000000000000,0),two); // DE canonical/pseudo
        yield return (one,new(0xc000000000000000,0x4000)); // 1/3 PE
        yield return (one,default); yield return (default,default); // ZE and IE
        yield return (new(1,0),default); // ZE outranks DE
        yield return (new(0x8000000000000000,0x7fff),default); // infinity/zero no ZE
        yield return (new(0x8000000000000000,0x7fff),new(0x8000000000000000,0xffff));
        yield return (new(0x8000000000000001,0x7fff),one); // sNaN
        yield return (new(0xc000000000000001,0x7fff),new(0x8000000000000002,0xffff)); // q>s
        yield return (new(0xc000000000000001,0x7fff),new(0xc000000000000001,0xffff)); // R126 left tie
        yield return (new(0x8000000000000001,0x7fff),new(0x8000000000000001,0xffff)); // signaling tie
        yield return (new(0x4000000000000000,1),one); // unsupported unnormal
        yield return (new(1,0x7fff),one); yield return (new(0,0x7fff),one); // pseudo NaN/inf
        // R128 single-rounding discriminators for PC24/53/64 underflow.
        yield return (new(0x839c73b5a965ffda,1),new(0xe1dc6c2abef35a44,0x4000));
        yield return (new(0x8967ddc1693bbd38,1),new(0xd218ea34dcb0cac9,0x4000));
        yield return (new(0xb6bf35839a88fb6f,1),new(0xfb11668aaa967ceb,0x4000));
    }

    static void ArithmeticMath(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        // All18 directions/pop forms at i=1. All masks64 x RC4 x PC4;
        // whole-file and whole-Machine oracle, both scaffold selections.
        foreach (byte[] form in ArithmeticForms())
        {
            if ((form[1]&7) != 1) continue;
            var i = ArithmeticClassify(form); var call = ArithmeticCall(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            foreach ((E80 a,E80 b) in ArithmeticNumbers())
            foreach (int pc in new[] { 0,1,2,3 })
            for (int rc = 0; rc < 4; ++rc)
            for (int masks = 0; masks < 64; ++masks)
            {
                ArithmeticSeed(fake,state,(masks+rc)&7,(ushort)(0x40|masks|pc<<8|rc<<10));
                ArithmeticInput(fake,state,0,a); ArithmeticInput(fake,state,1,b);
                ArithmeticOne(code,i,fake,state);
            }
        }
    }

    // Literal completion expectations are independent of ReferenceE80 and of
    // ArithmeticExpected. They catch accidental blanket suppression/commit,
    // normal-vs-wrapped status/C1 selection and exact-tiny classification.
    readonly record struct ArithmeticAnchor(string Name,byte Op,byte Modrm,E80 A,E80 B,ushort Cw,
        E80 Result,uint Delta,bool Write = true);
    static void ArithmeticLiterals(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        E80 one = new(0x8000000000000000,0x3fff),two = new(0x8000000000000000,0x4000),
            three = new(0xc000000000000000,0x4000),min = new(0x8000000000000000,1),
            largest = new(ulong.MaxValue,0x7ffe),indef = new(0xc000000000000000,0xffff),
            snan = new(0x8000000000000001,0x7fff),qnan = new(0xc000000000000001,0x7fff),
            third = new(0xaaaaaaaaaaaaaaab,0x3ffd);
        ArithmeticAnchor[] rows = [
            new("normal 1+2",0xd8,0xc1,one,two,0x37f,three,0),
            new("D8 reverse sub 2-1",0xd8,0xe9,one,two,0x37f,one,0),
            new("DC reverse sub 1-2",0xdc,0xe1,one,two,0x37f,new(one.Significand,0xbfff),0),
            new("D8 reverse div 2/1",0xd8,0xf9,one,two,0x37f,two,0),
            new("DC reverse div 1/2",0xdc,0xf1,one,two,0x37f,new(one.Significand,0x3ffe),0),
            new("PE masked normal up",0xd8,0xf1,one,three,0x37f,third,0x220),
            new("PE unmasked commits pop then pending",0xde,0xf9,three,one,0x35f,third,0x220),
            new("exact tiny masked no UE",0xd8,0xf1,min,two,0x37f,new(0x4000000000000000,0),0),
            new("exact tiny unmasked wraps despite NormalStatus0",0xd8,0xf1,min,two,0x36f,new(one.Significand,0x6000),16),
            new("exact tiny unmasked pop",0xde,0xf9,two,min,0x36f,new(one.Significand,0x6000),16),
            new("exact overflow unmasked no PE",0xd8,0xc9,largest,two,0x377,new(ulong.MaxValue,0x1fff),8),
            new("masked overflow normal infinity+C1",0xd8,0xc9,largest,two,0x37f,new(one.Significand,0x7fff),0x228),
            new("DE stops lower masked underflow precision",0xde,0xf9,two,new(1,0),0x37d,default,2,false),
            new("DE masked plus tiny exact unmasked",0xd8,0xf1,new(1,0),two,0x36f,new(one.Significand,0x5fc1),18),
            new("ZE unmasked suppress",0xd8,0xf1,one,default,0x37b,default,4,false),
            new("ZE masked signed infinity",0xd8,0xf1,one,default,0x37f,new(one.Significand,0x7fff),4),
            new("ZE outranks DE",0xd8,0xf1,new(1,0),default,0x37f,new(one.Significand,0x7fff),4),
            new("infinity over zero exact",0xd8,0xf1,new(one.Significand,0x7fff),default,0x37b,new(one.Significand,0x7fff),0),
            new("IE unmasked SNaN suppress pop",0xde,0xc1,snan,one,0x37e,default,1,false),
            new("IE masked SNaN quiet",0xd8,0xc1,snan,one,0x37f,qnan,1),
            new("quiet opposite sign tie chooses left",0xd8,0xc1,qnan,new(qnan.Significand,0xffff),0x37f,qnan,0),
            new("unsupported outranks NaN",0xd8,0xc1,new(1,0x7fff),qnan,0x37f,indef,1),
            new("invalid zero divided zero",0xd8,0xf1,default,default,0x37f,indef,1)
        ];
        foreach (var row in rows)
        {
            var i = ArithmeticClassify([row.Op,row.Modrm]); var call = ArithmeticCall(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            ArithmeticSeed(fake,state,7,row.Cw); ArithmeticInput(fake,state,0,row.A); ArithmeticInput(fake,state,1,row.B);
            uint beforeSw = ((Blink.Machine*)fake)->fpu.sw;
            byte[] before = CanonicalX87Bytes((byte*)state,192);
            ArithmeticOne(code,i,fake,state,true);
            var meaning = ArithmeticMeaning((uint)i.Extra.Immediate);
            int d = (7+meaning.Destination)&7;
            E80 actual = *FullX87RegisterFile.Physical(state,d);
            E80 original = ArithmeticRead(before,d);
            Check(actual.Significand == (row.Write?row.Result:original).Significand &&
                actual.SignExponent == (row.Write?row.Result:original).SignExponent,"R148 named literal result: "+row.Name);
            uint wantSw = (beforeSw & ~0x200u) | row.Delta;
            if ((row.Delta & ~row.Cw & 63) != 0) wantSw |= 0x8080;
            if (row.Write && meaning.Pop) wantSw &= ~0x3800u; // TOP7 ->0
            Check(((Blink.Machine*)fake)->fpu.sw == wantSw,"R148 literal completion/status/C1/TOP: "+row.Name);
        }
    }

    static void ArithmeticStackPriorityLiterals(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (byte[] form in new byte[][] { [0xd8,0xc1],[0xdc,0xe1],[0xde,0xf9],[0xde,0xc0] })
        foreach (bool masked in new[] { false,true })
        foreach (int empty in new[] { 0,1 })
        {
            var i = ArithmeticClassify(form); var call = ArithmeticCall(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            ArithmeticSeed(fake,state,6,masked?(ushort)0x37f:(ushort)0x37e);
            ((Blink.Machine*)fake)->fpu.sw &= ~0x40u; // literal must prove NEW SF set
            ArithmeticInput(fake,state,0,new(0x8000000000000001,0x7fff)); // signaling other operand
            ArithmeticInput(fake,state,i.Source.Register,new(1,0x7fff)); // unsupported other operand
            ArithmeticInput(fake,state,i.Source.Register==0?0:empty,default,3);
            ArithmeticOne(code,i,fake,state);
            uint sw = ((Blink.Machine*)fake)->fpu.sw;
            Check((sw & 0x82ff) == (masked?0x41u:0x80c1u),
                "R148 literal empty source outranks SNaN/unsupported, IE+SF/C1=0, ES/B iff unmasked");
            var meaning = ArithmeticMeaning((uint)i.Extra.Immediate);
            Check((sw>>11&7) == (masked&&meaning.Pop?7u:6u),
                "R148 literal masked stack underflow pops; unmasked preserves TOP");
            if (masked)
            {
                E80 result = *FullX87RegisterFile.Physical(state,(6+meaning.Destination)&7);
                Check(result.Significand == 0xc000000000000000 && result.SignExponent == 0xffff,
                    "R148 literal stack indefinite exact80 including self-alias popped ST0 raw slot");
            }
        }
    }

    static void ArithmeticBoundaries(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        foreach (byte[] form in ArithmeticForms())
        {
            var i = ArithmeticClassify(form); var call = ArithmeticCall(i,calls);
            using var code = CreateFullBlock([new(i,X87Kernel:call)]);
            foreach (string boundary in new[] { "stop","attention","budget","epoch","preissued","preissued-stop",
                "mf1","mf2","mf4","mf8","mf16","mf32","sf-only","masked-pending","es-only",
                "null","null-header","owner","pc01","tf","rf","mode" })
            {
                ArithmeticSeed(fake,state,5,0x37f);
                ArithmeticInput(fake,state,0,new(0x8000000000000000,0x3fff));
                ArithmeticInput(fake,state,i.Source.Register,new(0x8000000000000000,0x4000));
                bool pending = boundary.StartsWith("mf",StringComparison.Ordinal);
                if (pending)
                {
                    uint bit = uint.Parse(boundary.AsSpan(2));
                    ((Blink.Machine*)fake)->fpu.sw |= bit; ((Blink.Machine*)fake)->fpu.cw &= ~bit;
                    // Pending #MF must outrank even newly empty sources.
                    ArithmeticInput(fake,state,0,default,3);
                }
                if (boundary == "masked-pending") ((Blink.Machine*)fake)->fpu.sw |= 63;
                if (boundary == "es-only") ((Blink.Machine*)fake)->fpu.sw |= 0x8080;
                if (boundary == "attention") ((Blink.Machine*)fake)->attention = true;
                if (boundary == "pc01") ((Blink.Machine*)fake)->fpu.cw = 0x17f;
                if (boundary == "tf") ((Blink.Machine*)fake)->flags |= 0x100;
                if (boundary == "rf") ((Blink.Machine*)fake)->flags |= 0x10000;
                if (boundary == "mode") ((Blink.Machine*)fake)->mode.omode = Blink.XED_MODE_LEGACY;
                byte[] expected = CanonicalX87Bytes(fake,sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state,192);
                byte[] work = CanonicalX87Bytes((byte*)ArithmeticWork(state),384);
                bool works = boundary is "preissued" or "sf-only" or "masked-pending" or "es-only" or "pc01" or "tf" or "rf";
                if (works) ArithmeticExpected(i,expected,raw);
                ulong epoch = 11; long issued = boundary.StartsWith("preissued",StringComparison.Ordinal)?1:0,completed = 0;
                int stop = boundary is "stop" or "preissued-stop"?1:0; var h = FullHeader(i.Ip,code.Entry);
                var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,
                    budget:boundary is "budget" or "preissued"?0UL:1UL,limit:1);
                f.X87 = boundary == "null"?null:state;
                if (boundary == "null-header") f.Header = null;
                if (boundary == "owner") { h.OwnerCookie = f.OwnerCookie = 74; state->OwnerToken = 74; }
                if (boundary.StartsWith("preissued",StringComparison.Ordinal)) f.FirstPreissued = 1;
                if (boundary == "epoch") epoch++;
                f.MemoryTlb = (FullMemoryTlbEntry*)1; f.MemoryTlbMask = 63;
                f.DataDescriptors = (JitDataDescriptor*)1; f.DataDescriptorCount = 4;
                f.StackMemory = (FullStackMemoryDescriptor*)1;
                // Owner mismatch fixture updates all generic owner guards so
                // ONLY the arithmetic call cookie check can deny it.
                if (boundary == "owner") raw = CanonicalX87Bytes((byte*)state,192);
                FullBlockExit want = boundary switch
                {
                    "stop" or "preissued-stop" => FullBlockExit.Stop,
                    "attention" => FullBlockExit.Attention,"budget" => FullBlockExit.Budget,
                    _ => pending?FullBlockExit.GuestFault:works?FullBlockExit.Continue:FullBlockExit.Invalidated
                };
                Check(RunFull(code,&f) == want && completed == (works?1:0) && f.Executed == (works?1UL:0UL) &&
                    issued == (boundary is "stop" or "attention" or "epoch" or "null" or "null-header" or "owner" or "mode"?0:1),
                    "R148 all forms poll/budget/preissued/PC01/owner/live context/pending six flags before metadata/effects");
                if (pending) Check(f.FaultTrap == 16 && f.FaultCode == 0 && f.FaultAddress == i.Ip,
                    "R148 prior pending #MF is immediate at waiting instruction, CPU vector16");
                if (boundary.StartsWith("preissued",StringComparison.Ordinal))
                    Check(f.FirstPreissued == (boundary == "preissued"?0u:1u),"R148 preissue reservation consumed only by admitted operation");
                Check(f.MemoryAssists == 0 && f.NativeMemoryComputations == 0 && f.MemoryResume == 0 &&
                    f.HelperIndex == FullBlockFrame.NoHelperIndex,"R148 register arithmetic never touches memory assist descriptors");
                CanonicalX87Same(fake,state,expected,raw);
                if (!works) Check(new ReadOnlySpan<byte>(ArithmeticWork(state),384).SequenceEqual(work),
                    "R148 denied/pending operation never enters kernel or changes Work");
            }
        }
    }

    static void ArithmeticDeferred(byte* fake,FullX87RegisterFile* state,FullX87KernelCall[] calls)
    {
        // Both a post-operation fault that commits/pop and a pre-operation fault
        // that suppresses result/pop COMPLETE the ordinary instruction; neither
        // signals immediate #MF. The following waiting instruction observes it.
        foreach ((byte[] bytes,E80 a,E80 b,ushort cw,bool write) in new (byte[],E80,E80,ushort,bool)[]
        {
            ([0xde,0xf9],new(0xc000000000000000,0x4000),new(0x8000000000000000,0x3fff),0x35f,true),
            ([0xde,0xc9],new(0x8000000000000000,0x4000),new(ulong.MaxValue,0x7ffe),0x377,true),
            ([0xde,0xf9],new(0x8000000000000000,0x4000),new(0x8000000000000000,1),0x36f,true),
            ([0xde,0xf9],new(0x8000000000000000,0x4000),new(1,0),0x37d,false),
            ([0xde,0xf9],default,new(0x8000000000000000,0x3fff),0x37b,false),
            ([0xde,0xc1],new(0x8000000000000001,0x7fff),new(0x8000000000000000,0x3fff),0x37e,false)
        })
        {
            var i = ArithmeticClassify(bytes); var call = ArithmeticCall(i,calls);
            Check(FullCanonicalX87StackLowering.TryClassify(Ip+2,[0x9b],out var wait,out _),"R148 following FWAIT IR");
            using var code = CreateFullBlock([new(i,X87Kernel:call),new(wait)]);
            ArithmeticSeed(fake,state,6,cw); ArithmeticInput(fake,state,0,a); ArithmeticInput(fake,state,1,b);
            byte[] expected = CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw = CanonicalX87Bytes((byte*)state,192);
            ArithmeticExpected(i,expected,raw);
            ulong epoch = 11; long issued = 0,completed = 0; int stop = 0; var h = FullHeader(Ip,code.Entry);
            var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop); f.X87 = state;
            Check(FloatRunProbe(code,&f) == FullBlockExit.GuestFault && issued == 2 && completed == 1 && f.Executed == 1 &&
                f.FaultTrap == 16 && f.FaultAddress == Ip+2 && ((Blink.Machine*)fake)->ip == Ip+2,
                "R148 newly generated exception completes then next FWAIT issues and observes deferred MF");
            Check((((Blink.Machine*)fake)->fpu.sw>>11&7) == (write?7u:6u),"R148 post exception pops; pre exception preserves TOP");
            CanonicalX87Same(fake,state,expected,raw);
        }
    }

    static void ArithmeticBadProtocol(byte* fake,FullX87RegisterFile* state)
    {
        foreach (bool shared in new[] { false,true })
        {
            NativeCodeAllocation? common = null;
            if (shared) { common = CreateFullScaffold(out var addresses); fullOwnerScaffold = addresses; }
            try
            {
                // This is an intentional TEST-ONLY invalid kernel protocol,
                // not a production registry entry or guest semantic fallback.
                foreach ((uint admission,uint returned,uint classification,ushort status,ushort cw) in
                    new (uint,uint,uint,ushort,ushort)[] { (1,1,0,0,0x37f),(2,2,0,0,0x37f),
                        (3,3,0,0,0x37f),(4,4,0,0,0x37f),(5,5,0,0,0x37f),
                        (1,0,1,0,0x37f),(0,1,1,0,0x37f),(0,0,0,0,0x37f),
                        (0,0,1,0x1000,0x37f),(0,0,5,0,0x36f) })
                foreach (bool preissued in new[] { false,true })
                {
                    var bytes = new List<byte> { 0xf3,0x0f,0x1e,0xfa };
                    void Put(int offset,uint value)
                    {
                        bytes.AddRange(new byte[] { 0x41,0xc7,0x83 });
                        for (int k = 0; k < 4; ++k) bytes.Add((byte)((uint)offset>>(8*k)));
                        for (int k = 0; k < 4; ++k) bytes.Add((byte)(value>>(8*k)));
                    }
                    Put(12,admission); Put(80,status); Put(84,classification);
                    bytes.AddRange(new byte[] { 0x49,0xba,0,0,0,0,0,0,0,0,0xb8 }); // destroy R10
                    for (int k = 0; k < 4; ++k) bytes.Add((byte)(returned>>(8*k)));
                    bytes.AddRange(new byte[] { 0x41,0xff,0x63,88 }); // JMP [r11+88], no RET/CALL
                    Check(NativeCodeAllocation.TryCreate(bytes.ToArray(),out var kernel,out var reason),reason);
                    using (kernel)
                    {
                        var i = ArithmeticClassify([0xde,0xc1]);
                        var call = new FullX87KernelCall((byte)FullX87KernelKey.Add,kernel!.Entry,73,bytes.Count);
                        using var code = CreateFullBlock([new(i,X87Kernel:call)]);
                        ArithmeticSeed(fake,state,2,cw);
                        ArithmeticInput(fake,state,0,new(0x8000000000000000,0x3fff));
                        ArithmeticInput(fake,state,1,new(0x8000000000000000,0x4000));
                        byte[] expected = CanonicalX87Bytes(fake,sizeof(Blink.Machine)),raw = CanonicalX87Bytes((byte*)state,192);
                        ulong epoch = 11; long issued = preissued?1:0,completed = 0; int stop = 0; var h = FullHeader(Ip,code.Entry);
                        var f = FullFrame(fake,&h,&epoch,&issued,&completed,&stop,budget:preissued?0UL:1UL,limit:1);
                        f.X87 = state; f.FirstPreissued = preissued?1u:0;
                        Check(FloatRunProbe(code,&f) == FullBlockExit.Invalidated && issued == 1 && completed == 0 &&
                            f.Executed == 0 && f.FirstPreissued == 1 && f.FaultTrap == 0 &&
                            ((uint*)((byte*)ArithmeticWork(state)+280))[0] == 0x58383746,
                            "R148 unexpected protocol fails closed with original issue reserved, no invented CPU fault");
                        CanonicalX87Same(fake,state,expected,raw);
                        // Simulate parent INSPECTION only. Never resume/fallback:
                        // actual parent must throw its internal-fatal diagnostic.
                        Check(((uint*)((byte*)ArithmeticWork(state)+284))[0] != 0,"R148 parent post-return fatal diagnostic is concrete");
                    }
                }
            }
            finally { fullOwnerScaffold = default; common?.Dispose(); }
        }
    }

    static void ArithmeticEncodings(FullX87KernelCall[] calls)
    {
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity],common = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] { false,true })
        foreach (bool shared in new[] { false,true })
        {
            FullBlockEmitter.EmitScaffold(common,out var layout,windows); var scaffold = layout.Resolve((nint)0x10000000);
            foreach (byte[] form in ArithmeticForms())
            {
                var i = ArithmeticClassify(form); var call = ArithmeticCall(i,calls);
                FullBlockInstruction[] body = [new(i,X87Kernel:call)];
                int length;
                bool good = shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out length,windows):
                    FullBlockEmitter.TryEmit(body,staging,out length,windows);
                Check(good,"R148 both host ABI/standalone/shared encodings for all144 actual operations");
                int found = 0;
                for (int p = 0; p + 7 < length; ++p)
                {
                    if (staging[p] != 0x48 || staging[p+1] != 0x8d || staging[p+2] != 0x05) continue;
                    int target = p+7+BinaryPrimitives.ReadInt32LittleEndian(staging.AsSpan(p+3));
                    if (target < 0 || target+7 > length ||
                        !staging.AsSpan(target,7).SequenceEqual(new byte[] { 0xf3,0x0f,0x1e,0xfa,0x4d,0x89,0xd9 })) continue;
                    ++found;
                    Check(staging.AsSpan(p+7,4).SequenceEqual(new byte[] { 0x49,0x89,0x41,88 }),
                        "R148 exact continuation native RIP LEA -> Work88 (no approximate byte count)");
                    Check(staging[target-12] == 0x48 && staging[target-11] == 0xb8 && staging[target-2] == 0xff && staging[target-1] == 0xe0 &&
                        BinaryPrimitives.ReadUInt64LittleEndian(staging.AsSpan(target-10)) == (ulong)call.Entry,
                        "R148 literal correct-key catalog entry JMP; continuation stays in original emitted body");
                    Check(staging.AsSpan(target+7,7).SequenceEqual(new byte[] { 0x4d,0x8b,0x91,0,1,0,0 }) &&
                        staging.AsSpan(target+14,7).SequenceEqual(new byte[] { 0x4d,0x8b,0x99,8,1,0,0 }),
                        "R148 continuation restores R10 and R11 from Work256/264 before guest effects");
                }
                Check(found == 1,"R148 exactly one true80 continuation per ordinary arithmetic operation");
                foreach (int cut in new[] { 0,1,7,length-1 })
                {
                    int partial;
                    Check(!(shared?FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),in scaffold,out partial,windows):
                        FullBlockEmitter.TryEmit(body,staging.AsSpan(0,cut),out partial,windows)) && partial == 0,
                        "R148 undersized staging never publishes partial body/kernel transfer");
                }
                for (int warm = 0; warm < 12; ++warm)
                {
                    FullCanonicalX87ArithmeticLowering.TryClassify(Ip,form,out _,out _);
                    if (shared) FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows);
                    else FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int warm = 0; warm < 16; ++warm)
                {
                    good &= FullCanonicalX87ArithmeticLowering.TryClassify(Ip,form,out _,out _);
                    good &= shared?FullBlockEmitter.TryEmit(body,staging,in scaffold,out _,windows):
                        FullBlockEmitter.TryEmit(body,staging,out _,windows);
                }
                Check(good && GC.GetAllocatedBytesForCurrentThread() == before,
                    "R148 warmed span decoding/actual arithmetic body emission has no managed allocation or binding lookup");
            }
        }
    }
}
