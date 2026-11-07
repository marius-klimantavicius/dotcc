using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using CX = Managed.Emulation.Execution.FullCanonicalX87StackOperation;

unsafe partial class Program
{
    // AUTHORED_UNRUN. Parent must adopt translation seams before selecting this
    // entry. Synthetic Active1 belongs solely to these isolated allocations.
    static void CheckFullCanonicalX87Stack(byte* machine)
    {
        _ = machine;
        Check(sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87)).ToInt32() == 400 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87Reserved)).ToInt32() == 408 &&
            sizeof(FullX87RegisterFile) == 192 && sizeof(FullExtended80) == 16,
            "R143 ABI7/R133 layout without new slots");
        byte* fake = (byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine) + 15) & ~15), 16);
        var state = (FullX87RegisterFile*)NativeMemory.AlignedAlloc(192, 16);
        try
        {
            Check(fake != null && state != null, "R143 isolated allocation");
            Check(!fullOwnerScaffold.IsValid, "R143 starts with standalone scaffold");
            CanonicalX87DecodeTests();
            CanonicalX87R174DomainTests();
            CanonicalX87LeafTests(fake, state);
            CanonicalX87RawTests(fake, state);
            CanonicalX87ConstantTests(fake, state);
            CanonicalX87BoundaryTests(fake, state);
            CanonicalX87SelectorTests(fake, state);
            CanonicalX87R174ConditionTests(fake, state);
            using (var common = CreateFullScaffold(out var addresses))
            {
                fullOwnerScaffold = addresses;
                try
                {
                    CanonicalX87LeafTests(fake, state);
                    CanonicalX87RawTests(fake, state);
                    CanonicalX87ConstantTests(fake, state);
                    CanonicalX87BoundaryTests(fake, state);
                    CanonicalX87SelectorTests(fake, state);
                    CanonicalX87R174ConditionTests(fake, state);
                }
                finally { fullOwnerScaffold = default; }
            }
            CanonicalX87EncodingTests();
            CanonicalX87R174ControllerTests();
            CanonicalX87R174WaitPriorityTests();
            CanonicalX87R174CancellationTests();
        }
        finally { NativeMemory.AlignedFree(state); NativeMemory.AlignedFree(fake); }
    }

    static uint CanonicalX87U32(byte[] b, int p) => BinaryPrimitives.ReadUInt32LittleEndian(b.AsSpan(p));
    static void CanonicalX87W32(byte[] b, int p, uint v) => BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(p), v);
    static void CanonicalX87W64(byte[] b, int p, ulong v) => BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(p), v);
    static byte[] CanonicalX87Bytes(byte* p, int n) => new ReadOnlySpan<byte>(p, n).ToArray();
    static void CanonicalX87Same(byte* fake, FullX87RegisterFile* state, byte[] expected, byte[] raw)
    {
        Check(new ReadOnlySpan<byte>(fake, sizeof(Blink.Machine)).SequenceEqual(expected),
            "R143 independent whole-Machine oracle (XMM, shadow, all GPRs, metadata)");
        Check(new ReadOnlySpan<byte>(state, 192).SequenceEqual(raw),
            "R143 independent complete192-byte file oracle (raw80, selectors, padding/header)");
    }
    static JitInstruction CanonicalX87Classify(byte[] bytes)
    {
        Check(FullCanonicalX87StackLowering.TryClassify(Ip, bytes, out var i, out var reason), reason ?? "R143 decode");
        Check(FullBlockEmitter.CanInlineCanonicalX87Stack(i) && FullBlockEmitter.ValidateCanonicalX87Stack(i), "R143 IR admission");
        var reference = CanonicalX87R174StackReference(bytes);
        Check(i.Options == reference.Options && i.Source == new JitOperand(reference.Index, 0),
            "R174 independent selector table authenticates the raw80 reference operation and alias profile");
        return i;
    }
    static IEnumerable<byte[]> CanonicalX87Forms()
    {
        for (int n = 0; n < 8; ++n)
        {
            yield return [0xd9, (byte)(0xc0 + n)]; yield return [0xd9, (byte)(0xc8 + n)];
            yield return [0xdd, (byte)(0xc0 + n)]; yield return [0xdd, (byte)(0xd0 + n)];
            yield return [0xdd, (byte)(0xd8 + n)];
            yield return [0xdf, (byte)(0xc0 + n)]; // documented AMD FFREEP
            yield return [0xd9, (byte)(0xd8 + n)]; // explicit selected aliases
            yield return [0xdd, (byte)(0xc8 + n)]; yield return [0xdf, (byte)(0xc8 + n)];
            yield return [0xdf, (byte)(0xd0 + n)]; yield return [0xdf, (byte)(0xd8 + n)];
        }
        foreach (byte m in new byte[] { 0xd0, 0xe0, 0xe1, 0xf6, 0xf7, 0xe8, 0xe9, 0xea, 0xeb, 0xec, 0xed, 0xee })
            yield return [0xd9, m];
        for (byte m = 0xd1; m <= 0xd7; ++m) yield return [0xd9, m];
        for (byte m = 0xe1; m <= 0xe7; ++m) yield return [0xdf, m];
        yield return [0x9b]; yield return [0xdb, 0xe2]; yield return [0xdb, 0xe3]; yield return [0xdf, 0xe0];
    }
    static void CanonicalX87DecodeTests()
    {
        foreach (byte[] form in CanonicalX87Forms())
        foreach (byte[] prefix in new byte[][] { [], [0x66], [0x67, 0x64, 0x4f], [0x4f, 0x66, 0x40] })
        {
            byte[] b = new byte[prefix.Length + form.Length]; prefix.CopyTo(b, 0); form.CopyTo(b, prefix.Length);
            var i = CanonicalX87Classify(b);
            ulong fop = form.Length == 1 ? 0 : (ulong)((form[0] & 7) << 8 | form[1]);
            Check(i.Length == b.Length && i.Extra == new JitOperand(0, fop, IsImmediate: true),
                "R143 ignored register prefixes/REX and full ModRM+low3 opcode FOP");
            foreach (var bad in new[] { i with { Width = 64 }, i with { Options = i.Options ^ 0x10000 }, i with { Options = i.Options | 0x20000 },
                i with { Extra = i.Extra with { Immediate = fop ^ 0x100 } },
                i with { Source = i.Source with { IsMemory = true } }, i with { Source = new(-1, 0) },
                i with { Source = new(8, 0) }, i with { Extra = i.Extra with { Immediate = fop | 0x800 } }, i with { Length = 0 },
                i with { Destination = new(1, 0) }, i with { SourceWidth = 80 }, i with { Target = 1 },
                i with { Address = new(1, 2, 0, 0) }, i with { Extra = i.Extra with { IsImmediate = false } } })
            {
                bool rejected = false; try { FullBlockEmitter.ValidateCanonicalX87Stack(bad); } catch (ArgumentException) { rejected = true; }
                Check(rejected, "R143 malformed recognized IR throws before publication");
            }
        }
        foreach (byte m in new byte[] { 0xe0, 0xe1, 0xe4 })
            Check(!FullCanonicalX87StackLowering.TryClassify(Ip, [0xdb, m], out _, out var why) &&
                why == "CanonicalX87ObsoleteNoEffectButSelectedUdExcluded",
                "R174 selected DB/4 OpUd is retained despite Intel obsolete integer-NOP behavior");
        foreach (byte[] b in new byte[][] { [0xf0,0xd9,0xc0], [0xf2,0xd9,0xc0], [0xf3,0xd9,0xc0],
            [0xd9], [0xd9,0xc0,0], [0x9b,0xdb,0xe3], [0xd9,0x00], [0xd8,0xc0], [0xda,0xc0], [0xd9,0xfa] })
            Check(!FullCanonicalX87StackLowering.TryClassify(Ip, b, out _, out _), "R143 exact length/prefix/following-workstream boundary");
        var old = new JitInstruction(Ip, 2, JitOperation.X87, 80, default, default, Options: 0x4001);
        Check(!FullBlockEmitter.CanInlineCanonicalX87Stack(old), "R143 no historical R116 namespace dependency");
    }

    static FullExtended80 CanonicalX87Value(int p, int tag) => tag switch
    {
        0 => new(0x8000000000000001UL + (ulong)p * 0x102030405UL, (ushort)(0x3fff + p)),
        1 => new(0, (ushort)((p & 1) << 15)),
        2 => new(0x4123456789abcdefUL ^ (ulong)p, (ushort)(0x4000 + p)),
        _ => new(0xdead000000000001UL + (ulong)p, (ushort)(0xf123 + p))
    };
    static void CanonicalX87Seed(byte* fake, FullX87RegisterFile* state, int top, uint cw = 0x37f)
    {
        ResetFull(fake);
        for (int r = 0; r < 16; ++r) Write64(fake, 24 + 8 * r, 0x9abcde0100000000UL + (ulong)r * 0x12345679UL);
        for (int b = 160; b < 416; ++b) fake[b] = (byte)(b * 37 + 13); // independent XMM image
        ((Blink.Machine*)fake)->cs.sel = 0x33;
        ((Blink.Machine*)fake)->fpu.sw = 0x5a004700u | (uint)top << 11;
        ((Blink.Machine*)fake)->fpu.tw = 0x12340000; ((Blink.Machine*)fake)->fpu.cw = cw;
        Write64(fake, 656, 0x0000000000000765UL); Write64(fake, 664, 0x1234567812345678UL);
        Write64(fake, 680, 0xabcdef01fedcba98UL);
        FullX87StateStorage.Initialize(state, (nint)fake, 101, 73); state->ProfileActive = 1;
        state->CodeSelector = 0x1357; state->DataSelector = 0x2468;
        for (int p = 0; p < 8; ++p)
        {
            var slot = FullX87RegisterFile.Physical(state, p); *slot = CanonicalX87Value(p, 0);
            FullX87RegisterFile.Physical(state, p)->Reserved10 = (ushort)(0xa510 + p);
            FullX87RegisterFile.Physical(state, p)->Reserved12 = 0xd3c2b100u + (uint)p;
        }
    }
    static void CanonicalX87SeedTag(byte* fake, FullX87RegisterFile* state, int logical, int tag)
    {
        int p = (int)((((Blink.Machine*)fake)->fpu.sw >> 11) + (uint)logical) & 7;
        uint tw = unchecked((uint)((Blink.Machine*)fake)->fpu.tw);
        ((Blink.Machine*)fake)->fpu.tw = unchecked((int)((tw & ~(3u << (2 * p))) | (uint)tag << (2 * p)));
        var value = FullX87RegisterFile.Physical(state, p); value->Significand = CanonicalX87Value(p, tag).Significand;
        value->SignExponent = CanonicalX87Value(p, tag).SignExponent;
    }

    // Declarative raw-byte oracle, independent of emitter register allocation,
    // branches and helper implementation. Reserved bytes are never bulk copied.
    static void CanonicalX87Expected(JitInstruction i, byte[] m, byte[] raw)
    {
        CX op = (CX)(i.Options & 0xffff); int n = i.Source.Register;
        uint sw = CanonicalX87U32(m, 648), tw = CanonicalX87U32(m, 652), cw = CanonicalX87U32(m, 672);
        int top = (int)(sw >> 11) & 7;
        int P(int logical) => (top + logical) & 7;
        int Tag(int logical) => (int)(tw >> (2 * P(logical))) & 3;
        void SetTag(int logical, int tag) { int s = 2 * P(logical); tw = (tw & ~(3u << s)) | (uint)tag << s; }
        byte[] Read(int logical) => raw.AsSpan(64 + 16 * P(logical), 10).ToArray();
        byte[] Indefinite() => [0,0,0,0,0,0,0,0xc0,0xff,0xff];
        void Write(int logical, byte[] v)
        {
            v.CopyTo(raw, 64 + 16 * P(logical));
            ulong sig = BinaryPrimitives.ReadUInt64LittleEndian(v);
            int exp = BinaryPrimitives.ReadUInt16LittleEndian(v.AsSpan(8)) & 0x7fff;
            SetTag(logical, exp == 0 && sig == 0 ? 1 : exp is > 0 and < 0x7fff && (sig >> 63) != 0 ? 0 : 2);
        }
        void Pop() { SetTag(0, 3); top = (top + 1) & 7; }
        void Push(byte[] v) { top = (top + 7) & 7; Write(0, v); }
        bool nonwait = op is CX.Fninit or CX.Fnclex or CX.FnstswAx;
        if (!nonwait && (sw & ~cw & 63) != 0) return;
        if (op == CX.Fninit)
        {
            sw = 0; tw = 0xffff; top = 0; CanonicalX87W32(m, 672, 0x37f);
            CanonicalX87W32(m, 656, 0); CanonicalX87W64(m, 664, 0); CanonicalX87W64(m, 680, 0);
            raw.AsSpan(40, 4).Clear();
        }
        else if (op == CX.Fnclex) sw &= ~0x80ffu;
        else if (op == CX.FnstswAx) BinaryPrimitives.WriteUInt16LittleEndian(m.AsSpan(24), (ushort)sw);
        else if (op != CX.Fwait)
        {
            CanonicalX87W64(m, 664, i.Ip); CanonicalX87W32(m, 656, (uint)i.Extra.Immediate);
            BinaryPrimitives.WriteUInt16LittleEndian(raw.AsSpan(40), BinaryPrimitives.ReadUInt16LittleEndian(m.AsSpan(472)));
            if (op is not (CX.Ffree or CX.Ffreep or CX.Fnop)) sw &= ~0x200u;
            if (op == CX.Ffree) SetTag(n, 3);
            // AMD primary: only ST(i) is freed; advancing TOP does not free
            // the original ST0 or read an empty value through legacy FpuPop.
            else if (op == CX.Ffreep) { SetTag(n, 3); top = (top + 1) & 7; }
            else if (op == CX.Fincstp) top = (top + 1) & 7;
            else if (op == CX.Fdecstp) top = (top + 7) & 7;
            else if (op != CX.Fnop)
            {
                bool push = op == CX.Fld || op is >= CX.Fld1 and <= CX.Fldz;
                bool under = op == CX.Fld ? Tag(n) == 3 : op < CX.Fld1 &&
                    (Tag(0) == 3 || op == CX.Fxch && Tag(n) == 3);
                bool over = push && !under && Tag(7) != 3;
                bool fault = under || over;
                if (fault) sw = (sw & ~0x200u) | (over ? 0x241u : 0x41u);
                if (fault && (cw & 1) == 0) sw |= 0x8080;
                else if (push) Push(fault ? Indefinite() : op == CX.Fld ? Read(n) : CanonicalX87ConstantOracle(op, cw));
                else if (op is CX.Fst or CX.Fstp)
                { Write(n, under ? Indefinite() : Read(0)); if (op == CX.Fstp) Pop(); }
                else if (op == CX.Fxch)
                {
                    byte[] a = Tag(0) == 3 ? Indefinite() : Read(0), b = Tag(n) == 3 ? Indefinite() : Read(n);
                    Write(0, b); Write(n, a);
                }
                else if (under) Write(0, Indefinite());
                else
                {
                    int at = 64 + 16 * P(0) + 9;
                    if (op == CX.Fchs) raw[at] ^= 0x80; else raw[at] &= 0x7f;
                }
            }
        }
        sw = (sw & ~0x3800u) | (uint)top << 11;
        CanonicalX87W32(m, 648, sw); CanonicalX87W32(m, 652, tw);
        CanonicalX87W64(m, 0, i.Ip + (uint)i.Length); m[8] = 0;
    }

    static byte[] CanonicalX87ConstantOracle(CX op, uint cw)
    {
        // RC rows explicitly specify full architectural constants, rather than
        // calling the implementation's rounding/literal helper.
        int rc = (int)(cw >> 10) & 3;
        ulong[] rn = [0x8000000000000000,0xd49a784bcd1b8afe,0xb8aa3b295c17f0bc,
            0xc90fdaa22168c235,0x9a209a84fbcff799,0xb17217f7d1cf79ac,0];
        ulong[] rd = [0x8000000000000000,0xd49a784bcd1b8afe,0xb8aa3b295c17f0bb,
            0xc90fdaa22168c234,0x9a209a84fbcff798,0xb17217f7d1cf79ab,0];
        ulong[] ru = [0x8000000000000000,0xd49a784bcd1b8aff,0xb8aa3b295c17f0bc,
            0xc90fdaa22168c235,0x9a209a84fbcff799,0xb17217f7d1cf79ac,0];
        ushort[] exp = [0x3fff,0x4000,0x3fff,0x4000,0x3ffd,0x3ffe,0];
        int k = (int)op - (int)CX.Fld1; byte[] result = new byte[10];
        BinaryPrimitives.WriteUInt64LittleEndian(result, (rc == 0 ? rn : rc == 2 ? ru : rd)[k]);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(8), exp[k]); return result;
    }

    static void CanonicalX87LeafTests(byte* fake, FullX87RegisterFile* state)
    {
        foreach (byte[] form in CanonicalX87Forms())
        {
            var i = CanonicalX87Classify(form); using var code = CreateFullBlock([new(i)]);
            for (int top = 0; top < 8; ++top)
            for (int srcTag = 0; srcTag < 4; ++srcTag)
            for (int dstTag = 0; dstTag < 4; ++dstTag)
            foreach (bool masked in new[] { false, true })
            {
                CanonicalX87Seed(fake, state, top, masked ? 0x37fu : 0x37eu);
                CX op = (CX)(i.Options & 0xffff);
                CanonicalX87SeedTag(fake, state, op == CX.Fld ? i.Source.Register : 0, srcTag);
                CanonicalX87SeedTag(fake, state, (op == CX.Fld || op is >= CX.Fld1 and <= CX.Fldz) ? 7 : i.Source.Register, dstTag);
                byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
                CanonicalX87Expected(i, expected, raw);
                ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
                var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = state;
                Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1 &&
                    f.HelperIndex == FullBlockFrame.NoHelperIndex && f.MemoryAssists == 0 && f.NativeMemoryComputations == 0 &&
                    f.MemoryResume == 0 && f.FaultTrap == 0 && f.HostMxcsrSaved == 0,
                    "R143 ordinary raw80 form exact issue/complete including deferred unmasked stack faults");
                CanonicalX87Same(fake, state, expected, raw);
            }
        }
    }

    static void CanonicalX87RawTests(byte* fake, FullX87RegisterFile* state)
    {
        FullExtended80[] values = [new(0x8000000000000001,0x3fff),new(0xffffffffffffffff,0x7ffe),
            new(0,0),new(0,0x8000),new(1,0),new(0x8000000000000000,0), // pseudo denormal
            new(0x8000000000000000,0x7fff),new(0x8000000000000000,0xffff),
            new(0x8000000000000001,0x7fff),new(0xc123456789abcdef,0xffff), // signaling/quiet NaNs
            new(0x4123456789abcdef,0x4000),new(0x4000000000000000,0x7fff),new(0,0x4000)];
        foreach (byte[] form in new byte[][] { [0xd9,0xc0], [0xd9,0xc1], [0xdd,0xd1], [0xdd,0xd8],
            [0xdd,0xd9], [0xd9,0xc8], [0xd9,0xc9], [0xd9,0xe0], [0xd9,0xe1],
            [0xdf,0xc0], [0xdf,0xc1], [0xdf,0xc7], [0xd9,0xd8], [0xd9,0xdf],
            [0xdd,0xc8], [0xdd,0xcf], [0xdf,0xc8], [0xdf,0xcf], [0xdf,0xd0],
            [0xdf,0xd7], [0xdf,0xd8], [0xdf,0xdf], [0xd9,0xd1], [0xd9,0xd7] })
        {
            var i = CanonicalX87Classify(form); using var code = CreateFullBlock([new(i)]);
            for (int top = 0; top < 8; ++top)
            foreach (var v in values)
            {
                CanonicalX87Seed(fake, state, top, 0x340); // ALL six flags unmasked, no pending flags
                CanonicalX87SeedTag(fake, state, 7, 3);
                for (int n = 0; n < 2; ++n)
                {
                    var s = FullX87RegisterFile.Physical(state, (top + n) & 7);
                    s->Significand = v.Significand; s->SignExponent = v.SignExponent;
                    uint tag = v.Significand == 0 && (v.SignExponent & 0x7fff) == 0 ? 1u :
                        (v.SignExponent & 0x7fff) is > 0 and < 0x7fff && (v.Significand >> 63) != 0 ? 0u : 2u;
                    uint tw = unchecked((uint)((Blink.Machine*)fake)->fpu.tw); int shift = 2 * ((top + n) & 7);
                    ((Blink.Machine*)fake)->fpu.tw = unchecked((int)((tw & ~(3u << shift)) | tag << shift));
                }
                byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
                CanonicalX87Expected(i, expected, raw);
                ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
                var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = state;
                Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1,
                    "R143 raw SNaN/unsupported/denormal moves and sign operations never signal arithmetic IE/DE");
                CanonicalX87Same(fake, state, expected, raw);
            }
        }
    }

    static void CanonicalX87ConstantTests(byte* fake, FullX87RegisterFile* state)
    {
        for (byte opcode = 0xe8; opcode <= 0xee; ++opcode)
        {
            var i = CanonicalX87Classify([0xd9, opcode]); using var code = CreateFullBlock([new(i)]);
            for (int top = 0; top < 8; ++top)
            for (int pc = 0; pc < 4; ++pc)
            for (int rc = 0; rc < 4; ++rc)
            {
                CanonicalX87Seed(fake, state, top, 0x7fu | (uint)pc << 8 | (uint)rc << 10);
                CanonicalX87SeedTag(fake, state, 7, 3);
                byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
                CanonicalX87Expected(i, expected, raw);
                ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
                var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = state;
                Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1,
                    "R143 seven constants all RC/PC, no PE/rounded-up C1");
                CanonicalX87Same(fake, state, expected, raw);
            }
        }
    }

    static void CanonicalX87BoundaryTests(byte* fake, FullX87RegisterFile* state)
    {
        foreach (byte[] form in CanonicalX87Forms())
        {
            var i = CanonicalX87Classify(form); CX op = (CX)(i.Options & 0xffff); using var code = CreateFullBlock([new(i)]);
            foreach (string boundary in new[] { "stop", "attention", "budget", "epoch", "preissued", "preissued-stop",
                "mf1", "mf2", "mf4", "mf8", "mf16", "mf32", "sf-only", "masked-pending", "es-only", "null" })
            {
                CanonicalX87Seed(fake, state, 5); CanonicalX87SeedTag(fake, state, 7, 3);
                bool pending = boundary.StartsWith("mf", StringComparison.Ordinal);
                if (pending)
                {
                    uint bit = uint.Parse(boundary.AsSpan(2));
                    ((Blink.Machine*)fake)->fpu.sw |= bit;
                    ((Blink.Machine*)fake)->fpu.cw &= ~bit;
                }
                if (boundary == "sf-only") { ((Blink.Machine*)fake)->fpu.sw |= 64; ((Blink.Machine*)fake)->fpu.cw &= ~64u; }
                if (boundary == "masked-pending") ((Blink.Machine*)fake)->fpu.sw |= 63;
                if (boundary == "es-only") ((Blink.Machine*)fake)->fpu.sw |= 0x8080;
                if (boundary == "attention") ((Blink.Machine*)fake)->attention = true;
                byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
                bool nonwait = op is CX.Fninit or CX.Fnclex or CX.FnstswAx;
                bool works = boundary is "preissued" or "sf-only" or "masked-pending" or "es-only" || pending && nonwait;
                if (works) CanonicalX87Expected(i, expected, raw);
                ulong epoch = 11; long issued = boundary.StartsWith("preissued", StringComparison.Ordinal) ? 1 : 0, completed = 0;
                int stop = boundary is "stop" or "preissued-stop" ? 1 : 0; var h = FullHeader(Ip, code.Entry);
                var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop,
                    budget: boundary is "budget" or "preissued" ? 0UL : 1UL, limit: 1);
                f.X87 = boundary == "null" ? null : state;
                if (boundary.StartsWith("preissued", StringComparison.Ordinal)) f.FirstPreissued = 1;
                if (boundary == "epoch") epoch++;
                // Deliberately unreadable descriptors: this register family must
                // not invoke mapping, host selectors, MMX, or a memory helper.
                f.MemoryTlb = (FullMemoryTlbEntry*)1; f.MemoryTlbMask = 63;
                f.DataDescriptors = (JitDataDescriptor*)1; f.DataDescriptorCount = 4;
                f.StackMemory = (FullStackMemoryDescriptor*)1;
                FullBlockExit want = boundary switch
                {
                    "stop" or "preissued-stop" => FullBlockExit.Stop,
                    "attention" => FullBlockExit.Attention, "budget" => FullBlockExit.Budget,
                    "epoch" or "null" => FullBlockExit.Invalidated,
                    _ => pending && !nonwait ? FullBlockExit.GuestFault : FullBlockExit.Continue
                };
                Check(FloatRunProbe(code, &f) == want && completed == (works ? 1 : 0) &&
                    f.Executed == (works ? 1UL : 0UL) && issued == (boundary is "stop" or "attention" or "epoch" or "null" ? 0 : 1) &&
                    f.MemoryAssists == 0 && f.NativeMemoryComputations == 0 && f.MemoryResume == 0 &&
                    f.HelperIndex == FullBlockFrame.NoHelperIndex && f.HostMxcsrSaved == 0,
                    "R143 all forms poll/issue/budget/null-file/pending-six/nonwaiting paths");
                if (pending && !nonwait) Check(f.FaultTrap == 16 && f.FaultCode == 0 && f.FaultAddress == i.Ip,
                    "R143 pending #MF before raw output/TOP/tag/metadata effects");
                if (boundary.StartsWith("preissued", StringComparison.Ordinal))
                    Check(f.FirstPreissued == (boundary == "preissued-stop" ? 1u : 0u), "R143 preissued consumed once, retained on poll rejection");
                CanonicalX87Same(fake, state, expected, raw);
            }
        }
        // A new unmasked fault updates sticky status/metadata, suppresses pop,
        // completes once, then causes #MF at the next WAITING instruction.
        foreach (byte[] first in new byte[][] { [0xdd,0xd9], [0xd9,0xc1], [0xd9,0xe8], [0xd9,0xc9], [0xd9,0xe1] })
        {
            var a = CanonicalX87Classify(first);
            var b = CanonicalX87Classify([0xd9,0xd0]) with { Ip = Ip + (uint)a.Length };
            using var code = CreateFullBlock([new(a), new(b)]);
            CanonicalX87Seed(fake, state, 7, 0x37e);
            if ((CX)(a.Options & 0xffff) is not CX.Fld1) { CanonicalX87SeedTag(fake, state, 0, 3); CanonicalX87SeedTag(fake, state, 1, 3); }
            byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
            CanonicalX87Expected(a, expected, raw);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, budget: 2, limit: 2); f.X87 = state;
            Check(FloatRunProbe(code, &f) == FullBlockExit.GuestFault && issued == 2 && completed == 1 && f.Executed == 1 &&
                f.FaultTrap == 16 && f.FaultAddress == b.Ip && BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>(fake, 8)) == b.Ip,
                "R143 deferred new unmasked IE/SF, output/pop suppressed, next instruction #MF");
            CanonicalX87Same(fake, state, expected, raw);
        }
        // Prefix-bearing instruction records FIP at first prefix; REX bits have
        // no ST-extension effect and never contaminate the complete FOP.
        var prefixed = CanonicalX87Classify([0x66,0x67,0x64,0x4f,0xdd,0xd9]);
        using (var code = CreateFullBlock([new(prefixed)]))
        {
            CanonicalX87Seed(fake, state, 6);
            byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
            CanonicalX87Expected(prefixed, expected, raw);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = state;
            Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1, "R143 prefixed metadata native leaf");
            CanonicalX87Same(fake, state, expected, raw);
        }
    }

    static void CanonicalX87SelectorTests(byte* fake, FullX87RegisterFile* state)
    {
        foreach (byte[] form in CanonicalX87Forms())
        {
            var i = CanonicalX87Classify(form); CX op = (CX)(i.Options & 0xffff); using var code = CreateFullBlock([new(i)]);
            foreach (ushort cs in new ushort[] { 0, 0x33, 0xfffb })
            foreach (bool pending in new[] { false, true })
            {
                CanonicalX87Seed(fake, state, 6); CanonicalX87SeedTag(fake, state, 7, 3);
                ((Blink.Machine*)fake)->cs.sel = cs;
                if (pending) { ((Blink.Machine*)fake)->fpu.sw |= 1; ((Blink.Machine*)fake)->fpu.cw &= ~1u; }
                byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
                CanonicalX87Expected(i, expected, raw);
                bool mf = pending && op is not (CX.Fninit or CX.Fnclex or CX.FnstswAx);
                ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
                var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = state;
                Check(FloatRunProbe(code, &f) == (mf ? FullBlockExit.GuestFault : FullBlockExit.Continue) &&
                    issued == 1 && completed == (mf ? 0 : 1), "R143 selector updates obey pending/nonwaiting control order");
                CanonicalX87Same(fake, state, expected, raw);
                // Copy-only serializers after FULL native return. These are not
                // guest instructions or semantic callbacks from an emitted body.
                CanonicalX87SelectorImages(fake, state, raw);
                CanonicalX87Same(fake, state, expected, raw);
            }
        }
    }
    static void CanonicalX87SelectorImages(byte* fake, FullX87RegisterFile* state, byte[] expectedRaw)
    {
        Span<byte> fx = stackalloc byte[512], env = stackalloc byte[28];
        Check(FullX87StateBridge.Transfer(state, (nint)fake, 101, 73, 1, HostX87Transfer.FxLong32Save, fx) ==
            HostX87TransferResult.Complete &&
            FullX87StateBridge.Transfer(state, (nint)fake, 101, 73, 1, HostX87Transfer.Env28Save, env) ==
            HostX87TransferResult.Complete, "R143 returned-boundary selector serializer probes");
        ushort fcs = BinaryPrimitives.ReadUInt16LittleEndian(expectedRaw.AsSpan(40));
        ushort fds = BinaryPrimitives.ReadUInt16LittleEndian(expectedRaw.AsSpan(42));
        Check(BinaryPrimitives.ReadUInt16LittleEndian(fx.Slice(12)) == fcs &&
            BinaryPrimitives.ReadUInt16LittleEndian(env.Slice(16)) == fcs &&
            BinaryPrimitives.ReadUInt16LittleEndian(fx.Slice(20)) == fds &&
            BinaryPrimitives.ReadUInt16LittleEndian(env.Slice(24)) == fds,
            "R143 FXLong32/protected28 expose current instruction CS separately from retained data DS");
    }

    static void CanonicalX87EncodingTests()
    {
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity], common = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] { false, true })
        foreach (bool shared in new[] { false, true })
        {
            FullBlockEmitter.EmitScaffold(common, out var layout, windows); var scaffold = layout.Resolve((nint)0x10000000);
            foreach (byte[] form in CanonicalX87Forms())
            {
                var i = CanonicalX87Classify(form); FullBlockInstruction[] body = [new(i)];
                bool good = shared ? FullBlockEmitter.TryEmit(body, staging, in scaffold, out _, windows) :
                    FullBlockEmitter.TryEmit(body, staging, out _, windows);
                Check(good, "R143 every canonical stack form both ABI/scaffold encodes");
                int shortLength;
                Check(!(shared ? FullBlockEmitter.TryEmit(body, new byte[7], in scaffold, out shortLength, windows) :
                    FullBlockEmitter.TryEmit(body, new byte[7], out shortLength, windows)) && shortLength == 0,
                    "R143 tiny staging cannot publish partial native code");
                for (int warm = 0; warm < 8; ++warm)
                {
                    FullCanonicalX87StackLowering.TryClassify(Ip, form, out _, out _);
                    if (shared) FullBlockEmitter.TryEmit(body, staging, in scaffold, out _, windows);
                    else FullBlockEmitter.TryEmit(body, staging, out _, windows);
                }
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int n = 0; n < 16; ++n)
                {
                    good &= FullCanonicalX87StackLowering.TryClassify(Ip, form, out _, out _);
                    good &= shared ? FullBlockEmitter.TryEmit(body, staging, in scaffold, out _, windows) :
                        FullBlockEmitter.TryEmit(body, staging, out _, windows);
                }
                Check(good && GC.GetAllocatedBytesForCurrentThread() == before, "R143 warm span classifier/emission zero managed allocation");
            }
        }
    }
}
