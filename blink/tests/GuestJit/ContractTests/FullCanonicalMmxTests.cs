using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;
using CM = Managed.Emulation.Execution.FullBlockEmitter.FullMmxOperation;

unsafe partial class Program
{
    // AUTHORED_UNRUN. Parent selects this entry after adopting the five files.
    // Active1 exists ONLY in these separately allocated synthetic native leaves.
    // The supplied Machine is never reset, written, or used for active execution.
    static void CheckFullCanonicalMmx(byte* machine)
    {
        _ = machine;
        Check(sizeof(FullBlockFrame) == 416 && FullBlockFrame.AbiVersion == 0x46420007 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87)).ToInt32() == 400 &&
            Marshal.OffsetOf<FullBlockFrame>(nameof(FullBlockFrame.X87Reserved)).ToInt32() == 408 &&
            sizeof(FullX87RegisterFile) == 192 && sizeof(FullExtended80) == 16,
            "R141 immutable ABI7 and R133 slot contract");
        byte* fake = (byte*)NativeMemory.AlignedAlloc((nuint)((sizeof(Blink.Machine) + 15) & ~15), 16);
        var state = (FullX87RegisterFile*)NativeMemory.AlignedAlloc(192, 16);
        byte* pages = (byte*)NativeMemory.AlignedAlloc(8192, 16);
        try
        {
            Check(fake != null && state != null && pages != null, "R141 isolated aligned allocations");
            Check(!fullOwnerScaffold.IsValid, "R141 standalone initial scaffold");
            CanonicalMmxLeaves(fake, state, pages);
            CanonicalMmxConversions(fake, state, pages);
            CanonicalMmxMasked(fake, state, pages);
            CanonicalMmxNoEffects(fake, state);
            CanonicalMmxIngress(fake, state);
            using (var common = CreateFullScaffold(out var addresses))
            {
                fullOwnerScaffold = addresses;
                try
                {
                    CanonicalMmxLeaves(fake, state, pages);
                    CanonicalMmxConversions(fake, state, pages);
                    CanonicalMmxMasked(fake, state, pages);
                    CanonicalMmxNoEffects(fake, state);
                    CanonicalMmxIngress(fake, state);
                }
                finally { fullOwnerScaffold = default; }
            }
            CanonicalMmxEncoding();
        }
        finally
        {
            NativeMemory.AlignedFree(pages);
            NativeMemory.AlignedFree(state);
            NativeMemory.AlignedFree(fake);
        }
        CanonicalMmxInactiveController();
    }

    static byte[] CanonicalMmxBytes(byte* p, int n) => new ReadOnlySpan<byte>(p, n).ToArray();
    static void CanonicalMmxSeed(byte* fake, FullX87RegisterFile* state, int top = 7, bool active = true)
    {
        // Only our unpublished fake allocation is passed to the old test seeder.
        // Initialize context bytes too: VectorSeed preserves real system fields,
        // while AlignedAlloc does not promise zero metal/context metadata.
        ResetFull(fake);
        MmxSeed(fake);
        var m = (Blink.Machine*)fake;
        m->fpu.sw = 0xa5a54000u | (uint)top << 11;
        m->fpu.tw = unchecked((int)0x89abcdef);
        FullX87StateStorage.Initialize(state, (nint)fake, 101, 73);
        state->ProfileActive = active ? 1u : 0u;
        state->CodeSelector = 0x1357; state->DataSelector = 0x2468;
        for (int p = 0; p < 8; ++p)
        {
            // Low precision bits absent from binary64, arbitrary raw80 classes,
            // and distinctive untouched slot padding are intentional witnesses.
            var slot = FullX87RegisterFile.Physical(state, p);
            *slot = new FullExtended80(0x8001000300050001UL ^ ((ulong)p * 0x1111222233334444UL),
                (ushort)(p == 0 ? 0x3fff : p == 1 ? 0x7fff : 0x8000 + p));
            slot->Reserved10 = (ushort)(0xa510 + p); slot->Reserved12 = 0xd3c2b100u + (uint)p;
        }
    }

    static void CanonicalMmxComplete(byte[] expected, JitInstruction i, bool transition, bool emms = false)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(expected, i.Ip + (uint)i.Length); expected[8] = 0;
        if (emms) BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(652), 0xffff);
        else if (transition)
        {
            uint sw = BinaryPrimitives.ReadUInt32LittleEndian(expected.AsSpan(648));
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(648), sw & ~0x3800u);
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(652), 0);
        }
    }

    // Existing independent scalar lane equations receive physical bytes ONLY
    // for MMX operands. XMM inputs retain their separate original bytes.
    static byte[] CanonicalMmxOperands(byte[] machine, byte[] state, JitInstruction i)
    {
        byte[] operands = (byte[])machine.Clone();
        if (FullBlockEmitter.MmxOperation(i) != CM.XmmToMmx)
            for (int p = 0; p < 8; ++p)
                state.AsSpan(64 + 16 * p, 8).CopyTo(operands.AsSpan(160 + 16 * p));
        return operands;
    }

    static void CanonicalMmxExpected(JitInstruction i, byte[] before, byte[] raw, ReadOnlySpan<byte> memory,
        out byte[] expected, out byte[] expectedRaw, out byte[] result)
    {
        expected = (byte[])before.Clone(); expectedRaw = (byte[])raw.Clone(); result = [];
        var op = FullBlockEmitter.MmxOperation(i);
        if (op != CM.Emms)
        {
            result = MmxResult(i, CanonicalMmxOperands(before, raw, i), memory);
            if (!i.Destination.IsMemory)
            {
                if (op is CM.MmxToScalar or CM.MaskByte or CM.ExtractWord)
                {
                    int width = op == CM.MmxToScalar ? i.SourceWidth : 32;
                    ulong value = width == 64 ? MmxU(result) : BinaryPrimitives.ReadUInt32LittleEndian(result);
                    BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(24 + i.Destination.Register * 8), value);
                }
                else if (op == CM.MmxToXmm)
                {
                    result.CopyTo(expected, 160 + i.Destination.Register * 16);
                    expected.AsSpan(168 + i.Destination.Register * 16, 8).Clear();
                }
                else
                {
                    int at = 64 + 16 * i.Destination.Register;
                    expectedRaw.AsSpan(at, 16).Clear(); result.CopyTo(expectedRaw, at);
                    BinaryPrimitives.WriteUInt16LittleEndian(expectedRaw.AsSpan(at + 8), 0xffff);
                }
            }
        }
        CanonicalMmxComplete(expected, i, true, op == CM.Emms);
    }

    static void CanonicalMmxSame(byte* fake, FullX87RegisterFile* state, byte[] expected, byte[] raw, string why)
    {
        Check(new ReadOnlySpan<byte>(fake, expected.Length).SequenceEqual(expected), why + " exact Machine");
        Check(new ReadOnlySpan<byte>(state, 192).SequenceEqual(raw), why + " header/every physical slot/pad");
    }

    static void CanonicalMmxLeaves(byte* fake, FullX87RegisterFile* state, byte* pages)
    {
        const ulong guest = 0x600000;
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        foreach (var form in MmxForms())
        foreach (bool memory in new[] { false, true })
        foreach (bool alias in new[] { false, true })
        foreach (bool high in new[] { false, true })
        foreach (int top in new[] { 0, 1, 2, 3, 4, 5, 6, 7 })
        foreach (bool active in new[] { false, true })
        {
            if (memory ? form.RegisterOnly : form.MemoryOnly) continue;
            CanonicalMmxSeed(fake, state, top, active);
            var i = MmxClassify(MmxEncoding(form, memory, high, alias, control: 0xe4));
            // Rotate physical MMX identities independently of TOP; preserve the
            // REX-selected GPR/XMM identities on crossing namespace moves.
            if (form.Operation != CM.Emms)
            {
                if (!i.Destination.IsMemory && form.Operation is not (CM.MmxToScalar or CM.MaskByte or CM.ExtractWord or CM.MmxToXmm))
                    i = i with { Destination = new((i.Destination.Register + top) & 7, 0) };
                if (!i.Source.IsMemory && !i.Source.IsImmediate && form.Operation is not (CM.ScalarToMmx or CM.InsertWord or CM.XmmToMmx))
                    i = i with { Source = new((i.Source.Register + top) & 7, 0) };
            }
            for (int b = 0; b < 8192; ++b) pages[b] = (byte)(b * 17 + 0x39);
            if (memory) Write64(fake, 24 + i.Address.Base * 8, guest + 4095);
            byte[] before = CanonicalMmxBytes(fake, sizeof(Blink.Machine)), raw = CanonicalMmxBytes((byte*)state, 192);
            byte[] memoryBefore = CanonicalMmxBytes(pages, 8192), expectedRaw, expected, result;
            ReadOnlySpan<byte> input = new(pages + 4095, 8);
            if (active) CanonicalMmxExpected(i, before, raw, input, out expected, out expectedRaw, out result);
            else
            {
                expected = MmxExpected(i, before, input); expectedRaw = raw;
                result = form.Operation == CM.Emms ? [] : MmxResult(i, before, input);
            }
            if (i.Destination.IsMemory) result.AsSpan(0, i.SourceWidth / 8).CopyTo(memoryBefore.AsSpan(4095));
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            SetFullMemoryEntry(tlb, guest, pages, FullMemoryAccess.Read | FullMemoryAccess.Write);
            SetFullMemoryEntry(tlb, guest + 4096, pages + 4096, FullMemoryAccess.Read | FullMemoryAccess.Write);
            using var code = CreateFullBlock([new(i)]);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1);
            f.X87 = (FullX87RegisterFile*)FullX87StateStorage.GetActive(state, (nint)fake, 101, 73, 1);
            f.MemoryTlb = memory ? tlb : null; f.MemoryTlbMask = 63;
            Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1 &&
                f.NativeMemoryComputations == (memory ? 1UL : 0UL), "R141 every MMX form issue/commit/host MXCSR");
            CanonicalMmxSame(fake, state, expected, expectedRaw, "R141 physical alias/TOP/independent XMM/null compatibility");
            Check(new ReadOnlySpan<byte>(pages, 8192).SequenceEqual(memoryBefore), "R141 checked crossing MMX direction/width");
        }
    }

    static void CanonicalMmxConversions(byte* fake, FullX87RegisterFile* state, byte* pages)
    {
        const ulong guest = 0x600000;
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        foreach (bool pd in new[] { false, true })
        foreach (byte opcode in new byte[] { 0x2a, 0x2c, 0x2d })
        foreach (bool alias in new[] { false, true })
        foreach (bool high in new[] { false, true })
        foreach (bool memory in new[] { false, true })
        foreach (bool active in new[] { false, true })
        foreach (int rc in new[] { 0, 1, 2, 3 })
        foreach (string scenario in new[] { "normal", "masked-invalid", "unmasked" })
        {
            bool inputPi = opcode == 0x2a;
            if (inputPi && scenario == "masked-invalid") continue;
            CanonicalMmxSeed(fake, state, 5, active);
            var bytes = new List<byte>();
            if (pd) bytes.Add(0x66); if (high) bytes.Add(0x45);
            bytes.AddRange([0x0f, opcode, memory ? (byte)0x03 : alias ? (byte)0xc0 : (byte)0xc1]);
            var i = FloatMmxClassify(bytes.ToArray());
            uint mxcsr = 0x1f80u | (uint)rc << 13;
            if (scenario == "unmasked") mxcsr &= inputPi ? ~0x1000u : ~0x80u;
            ((Blink.Machine*)fake)->mxcsr = mxcsr;
            bool invalid = !inputPi && scenario != "normal";
            uint status = inputPi ? pd ? 0u : 32u : invalid ? 1u : 32u;
            bool fault = scenario == "unmasked" && status != 0;
            byte[] input = new byte[16];
            if (inputPi)
            {
                BinaryPrimitives.WriteInt32LittleEndian(input, 16777217);
                BinaryPrimitives.WriteInt32LittleEndian(input.AsSpan(4), -16777217);
            }
            else if (pd)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(input, invalid ? 0x7ff0000000000001UL : 0x3ff8000000000000UL);
                BinaryPrimitives.WriteUInt64LittleEndian(input.AsSpan(8), invalid ? 0x3ff0000000000000UL : 0xbff8000000000000UL);
            }
            else
            {
                BinaryPrimitives.WriteUInt32LittleEndian(input, invalid ? 0x7f800001u : 0x3fc00000u);
                BinaryPrimitives.WriteUInt32LittleEndian(input.AsSpan(4), invalid ? 0x3f800000u : 0xbfc00000u);
            }
            int at = i.SourceWidth == 128 ? 4080 : 4095;
            new Span<byte>(pages, 8192).Fill(0x5b);
            if (memory)
            {
                input.AsSpan(0, i.SourceWidth / 8).CopyTo(new Span<byte>(pages + at, i.SourceWidth / 8));
                Write64(fake, 24 + i.Address.Base * 8, guest + (uint)at);
            }
            else if (inputPi && active)
                input.AsSpan(0, 8).CopyTo(new Span<byte>(FullX87RegisterFile.Physical(state, i.Source.Register), 8));
            else input.AsSpan(0, i.SourceWidth / 8).CopyTo(new Span<byte>(fake + 160 + i.Source.Register * 16, i.SourceWidth / 8));
            byte[] before = CanonicalMmxBytes(fake, sizeof(Blink.Machine)), raw = CanonicalMmxBytes((byte*)state, 192);
            byte[] expected = (byte[])before.Clone(), expectedRaw = (byte[])raw.Clone(), result = new byte[16];
            BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(688), mxcsr | status);
            if (!fault)
            {
                if (inputPi)
                {
                    if (pd)
                    {
                        BinaryPrimitives.WriteUInt64LittleEndian(result, BitConverter.DoubleToUInt64Bits(16777217.0));
                        BinaryPrimitives.WriteUInt64LittleEndian(result.AsSpan(8), BitConverter.DoubleToUInt64Bits(-16777217.0));
                    }
                    else
                    {
                        BinaryPrimitives.WriteUInt32LittleEndian(result, rc == 2 ? 0x4b800001u : 0x4b800000u);
                        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), rc == 1 ? 0xcb800001u : 0xcb800000u);
                        before.AsSpan(168 + i.Destination.Register * 16, 8).CopyTo(result.AsSpan(8));
                    }
                    result.CopyTo(expected, 160 + i.Destination.Register * 16);
                }
                else
                {
                    int rounded = opcode == 0x2c ? 3 : rc;
                    BinaryPrimitives.WriteUInt32LittleEndian(result, invalid ? 0x80000000u : rounded is 0 or 2 ? 2u : 1u);
                    BinaryPrimitives.WriteInt32LittleEndian(result.AsSpan(4), invalid ? 1 : rounded is 0 or 1 ? -2 : -1);
                    if (active)
                    {
                        int dst = 64 + i.Destination.Register * 16;
                        expectedRaw.AsSpan(dst, 16).Clear(); result.AsSpan(0, 8).CopyTo(expectedRaw.AsSpan(dst));
                        BinaryPrimitives.WriteUInt16LittleEndian(expectedRaw.AsSpan(dst + 8), 0xffff);
                    }
                    else result.AsSpan(0, 8).CopyTo(expected.AsSpan(160 + i.Destination.Register * 16));
                }
                CanonicalMmxComplete(expected, i, active);
            }
            using var code = CreateFullBlock([new(i)]);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1);
            f.X87 = active ? state : null;
            new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
            SetFullMemoryEntry(tlb, guest, pages, FullMemoryAccess.Read);
            SetFullMemoryEntry(tlb, guest + 4096, pages + 4096, FullMemoryAccess.Read);
            f.MemoryTlb = memory ? tlb : null; f.MemoryTlbMask = 63;
            byte[] memoryBefore = CanonicalMmxBytes(pages, 8192);
            Check(FloatRunProbe(code, &f) == (fault ? FullBlockExit.GuestFault : FullBlockExit.Continue) &&
                issued == 1 && completed == (fault ? 0 : 1) && f.Executed == (fault ? 0UL : 1UL) &&
                f.NativeMemoryComputations == (memory && !fault ? 1UL : 0UL), "R141 six 0F2A/2C/2D forms precise completion/host FP");
            if (fault) Check(f.FaultTrap == 19 && f.FaultAddress == Ip, "R141 unmasked #XM suppresses all destination and tag effects");
            CanonicalMmxSame(fake, state, expected, expectedRaw, "R141 conversions allRC/alias/REX/memory/masked/noeffects");
            Check(new ReadOnlySpan<byte>(pages, 8192).SequenceEqual(memoryBefore), "R141 conversion input memory unchanged");
        }
    }

    static void CanonicalMmxMasked(byte* fake, FullX87RegisterFile* state, byte* pages)
    {
        const ulong guest = 0x600000;
        foreach (bool xmm in new[] { false, true })
        foreach (bool active in new[] { false, true })
        foreach (bool alias in new[] { false, true })
        foreach (bool ticket in new[] { false, true })
        foreach (bool descriptor in new[] { false, true })
        foreach (uint bits in new uint[] { 0, 1, 2, 0x80, 0x81, 0x55, 0xff, 0x8000, 0xffff })
        {
            CanonicalMmxSeed(fake, state, 6, active);
            var i = MaskedClassify(MaskedEncoding(xmm, alias: alias));
            i = i with { Source = new(7, 0), Extra = new(alias ? 7 : 6, 0) };
            byte* data = !xmm && active ? (byte*)FullX87RegisterFile.Physical(state, i.Source.Register) : fake + 160 + i.Source.Register * 16;
            byte* mask = !xmm && active ? (byte*)FullX87RegisterFile.Physical(state, i.Extra.Register) : fake + 160 + i.Extra.Register * 16;
            int count = i.Width / 8; uint enabled = 0;
            for (int b = 0; b < count; ++b) data[b] = (byte)(0x31 + b * 19);
            for (int b = 0; b < count; ++b)
            {
                mask[b] = (byte)((bits >> b & 1) != 0 ? 0x80 + b : b);
                enabled |= (uint)(mask[b] >> 7) << b;
            }
            // Poison the independent XMM source/mask oppositely to canonical MMX.
            if (!xmm && active)
            {
                new Span<byte>(fake + 160 + i.Source.Register * 16, 8).Fill(0xee);
                new Span<byte>(fake + 160 + i.Extra.Register * 16, 8).Fill(enabled == 0 ? (byte)0xff : (byte)0);
            }
            Write64(fake, 80, enabled == 0 ? 0x800000000000UL : guest + 4095);
            new Span<byte>(pages, 8192).Fill(0x5b);
            byte[] expectedMemory = CanonicalMmxBytes(pages, 8192);
            for (int b = 0; b < count; ++b) if ((enabled >> b & 1) != 0) expectedMemory[4095 + b] = data[b];
            byte[] before = CanonicalMmxBytes(fake, sizeof(Blink.Machine)), raw = CanonicalMmxBytes((byte*)state, 192);
            using var code = CreateFullBlock([new(i)]);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
            var d = new FullMaskedStoreDescriptor();
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = active ? state : null;
            f.StackMemory = descriptor ? (FullStackMemoryDescriptor*)&d : null;
            var first = FloatRunProbe(code, &f);
            if (enabled != 0)
            {
                CanonicalMmxSame(fake, state, before, raw, "R141 sparse preparation no tag or payload effects");
                Check(new ReadOnlySpan<byte>(pages, 8192).IndexOfAnyExcept((byte)0x5b) < 0, "R141 sparse preparation no writes");
                if (!descriptor)
                {
                    Check(first == FullBlockExit.Invalidated && completed == 0, "R141 missing sparse descriptor no transition");
                    continue;
                }
                Check(first == FullBlockExit.NeedMemory && issued == 1 && completed == 0 && d.EnabledBits == enabled &&
                    d.DataLo == MmxU(new ReadOnlySpan<byte>(data, 8)) && d.MaskLo == MmxU(new ReadOnlySpan<byte>(mask, 8)) &&
                    d.DataHi == (xmm ? MmxU(new ReadOnlySpan<byte>(data + 8, 8)) : 0) && d.MaskHi == (xmm ? MmxU(new ReadOnlySpan<byte>(mask + 8, 8)) : 0),
                    "R141 exact physical MMX snapshot with zero high half versus independent XMM snapshot");
                MaskedPublishSynthetic(ref d, pages, pages + 4096, guest, epoch, ticket);
                f.Budget = 0; FullNativeMemory.ResumeSameIssue(&f);
                Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && d.State == FullMaskedStoreState.Consumed && d.Ticket == 0,
                    "R141 successful same-issue sparse writes/ticket consume");
            }
            else Check(first == FullBlockExit.Continue && f.MemoryAddress == 0, "R141 zero mask suppresses noncanonical DI and mapping");
            byte[] expected = (byte[])before.Clone(); CanonicalMmxComplete(expected, i, !xmm);
            CanonicalMmxSame(fake, state, expected, raw, "R141 sparse MMX transition including zero/null and XMM independence");
            Check(issued == 1 && completed == 1 && f.Executed == 1 && f.NativeMemoryComputations == 1 &&
                new ReadOnlySpan<byte>(pages, 8192).SequenceEqual(expectedMemory), "R141 sparse selected bytes only and single commit");
        }

        foreach (string stale in new[] { "source", "mask", "snapshot", "epoch", "ticket" })
        {
            CanonicalMmxSeed(fake, state); Write64(fake, 80, guest + 4095);
            var i = MaskedClassify([0x0f, 0xf7, 0xc1]);
            byte* mask = (byte*)FullX87RegisterFile.Physical(state, 1);
            new Span<byte>(mask, 8).Fill(0x80); new Span<byte>(pages, 8192).Fill(0x5b);
            using var code = CreateFullBlock([new(i)]);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
            var d = new FullMaskedStoreDescriptor();
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = state; f.StackMemory = (FullStackMemoryDescriptor*)&d;
            Check(FloatRunProbe(code, &f) == FullBlockExit.NeedMemory, "R141 restage initial request");
            MaskedPublishSynthetic(ref d, pages, pages + 4096, guest, epoch);
            switch (stale)
            {
                case "source": ((byte*)FullX87RegisterFile.Physical(state, 0))[0] ^= 1; break;
                case "mask": mask[0] ^= 1; break; // even a disabled low bit authenticates
                case "snapshot": d.DataLo ^= 1; break;
                case "epoch": d.Epoch--; d.HostPage0 = (byte*)1; break;
                case "ticket": d.State = FullMaskedStoreState.TicketReady; d.Ticket = 0; break;
            }
            byte[] before = CanonicalMmxBytes(fake, sizeof(Blink.Machine)), raw = CanonicalMmxBytes((byte*)state, 192);
            FullNativeMemory.ResumeSameIssue(&f);
            Check(FloatRunProbe(code, &f) == FullBlockExit.NeedMemory && issued == 1 && completed == 0 &&
                d.State == FullMaskedStoreState.Request && d.Ticket == 0 && d.HostPage0 == null,
                "R141 changed canonical snapshots/authentication restage same issue");
            CanonicalMmxSame(fake, state, before, raw, "R141 stale sparse attempt suppresses all guest effects");
            Check(new ReadOnlySpan<byte>(pages, 8192).IndexOfAnyExcept((byte)0x5b) < 0, "R141 stale sparse attempt writes nothing");
            MaskedPublishSynthetic(ref d, pages, pages + 4096, guest, epoch, ticket: true);
            FullNativeMemory.ResumeSameIssue(&f);
            Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1, "R141 restaged physical snapshots complete once");
            byte[] expected = (byte[])before.Clone(); CanonicalMmxComplete(expected, i, true);
            CanonicalMmxSame(fake, state, expected, raw, "R141 restaged transition preserves every payload");
        }
    }

    static JitInstruction[] CanonicalMmxInstructions()
    {
        var result = new List<JitInstruction>();
        foreach (var form in MmxForms())
        {
            if (!form.MemoryOnly) result.Add(MmxClassify(MmxEncoding(form, control: 255)));
            if (!form.RegisterOnly) result.Add(MmxClassify(MmxEncoding(form, memory: true, control: 255)));
        }
        foreach (bool pd in new[] { false, true })
        foreach (byte op in new byte[] { 0x2a, 0x2c, 0x2d })
        foreach (bool memory in new[] { false, true })
            result.Add(FloatMmxClassify(pd ? [0x66, 0x0f, op, memory ? (byte)0x03 : (byte)0xc1] :
                [0x0f, op, memory ? (byte)0x03 : (byte)0xc1]));
        result.Add(MaskedClassify([0x0f, 0xf7, 0xc1]));
        return result.ToArray();
    }

    static void CanonicalMmxNoEffects(byte* fake, FullX87RegisterFile* state)
    {
        FullMemoryTlbEntry* tlb = stackalloc FullMemoryTlbEntry[64];
        foreach (var i in CanonicalMmxInstructions())
        foreach (bool zeroMask in new[] { false, true })
        foreach (string boundary in new[] { "stop", "attention", "budget", "epoch", "mf1", "mf2", "mf4", "mf8", "mf16", "mf32", "canonical", "missing", "readonly" })
        {
            bool sparse = FullBlockEmitter.CanInlineMaskedStore(i);
            bool memory = i.Source.IsMemory || i.Destination.IsMemory;
            if (zeroMask && !sparse) continue;
            if (boundary is "canonical" or "missing" or "readonly" && (!memory || sparse && zeroMask)) continue;
            if (boundary == "readonly" && (!i.Destination.IsMemory || sparse)) continue;
            CanonicalMmxSeed(fake, state);
            if (sparse) new Span<byte>(FullX87RegisterFile.Physical(state, 1), 8).Fill(zeroMask ? (byte)0 : (byte)0x80);
            if (memory) Write64(fake, 24 + i.Address.Base * 8, boundary == "canonical" ? 0x800000000000UL : 0x600000);
            var m = (Blink.Machine*)fake;
            if (boundary.StartsWith("mf"))
            {
                uint bit = uint.Parse(boundary.AsSpan(2)); m->fpu.sw |= bit; m->fpu.cw &= ~bit;
                // Even the noncanonical memory and zero-mask schedules must #MF first.
                if (memory) Write64(fake, 24 + i.Address.Base * 8, 0x800000000000UL);
            }
            if (boundary == "attention") m->attention = true;
            byte[] before = CanonicalMmxBytes(fake, sizeof(Blink.Machine)), raw = CanonicalMmxBytes((byte*)state, 192);
            using var code = CreateFullBlock([new(i)]);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = boundary == "stop" ? 1 : 0; var h = FullHeader(Ip, code.Entry);
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, budget: boundary == "budget" ? 0UL : 1UL, limit: 1);
            f.X87 = state;
            var d = new FullMaskedStoreDescriptor(); f.StackMemory = (FullStackMemoryDescriptor*)&d;
            if (boundary == "epoch") f.ObservedGlobalEpoch--;
            if (boundary == "readonly")
            {
                new Span<FullMemoryTlbEntry>(tlb, 64).Clear();
                SetFullMemoryEntry(tlb, 0x600000, (byte*)1, FullMemoryAccess.Read);
                f.MemoryTlb = tlb; f.MemoryTlbMask = 63; // must refuse write BEFORE poisoned pointer use
            }
            else if (boundary == "canonical" || boundary.StartsWith("mf"))
            { f.MemoryTlb = (FullMemoryTlbEntry*)1; f.MemoryTlbMask = 63; }
            var expected = boundary switch
            {
                "stop" => FullBlockExit.Stop, "attention" => FullBlockExit.Attention,
                "budget" => FullBlockExit.Budget, "epoch" => FullBlockExit.Invalidated,
                "missing" or "readonly" => FullBlockExit.NeedMemory, _ => FullBlockExit.GuestFault
            };
            Check(FloatRunProbe(code, &f) == expected && completed == 0 && f.Executed == 0 &&
                f.NativeMemoryComputations == 0 && f.HostMxcsrSaved == 0 &&
                issued == (boundary is "stop" or "attention" or "epoch" ? 0 : 1),
                "R141 all MMX families stop/poll/budget/epoch/fault/mapping precise no completion");
            if (boundary.StartsWith("mf")) Check(f.FaultTrap == 16 && f.FaultAddress == Ip && f.MemoryResume == 0 &&
                f.MemoryAddress == 0 && d.State == FullMaskedStoreState.Empty, "R141 #MF before reads, zero mask, EA, descriptor, MMU, host FP");
            if (boundary == "canonical") Check(f.FaultTrap == 13 && f.FaultCode == 128 && f.MemoryResume == 0,
                "R141 noncanonical memory before mapping and effects");
            CanonicalMmxSame(fake, state, before, raw, "R141 fault/poll/budget preserves all raw80/TOP/tags/XMM/shadow");
        }
    }

    static void CanonicalMmxIngress(byte* fake, FullX87RegisterFile* state)
    {
        foreach (string bad in new[] { "version", "size", "protocol", "inactive", "machine", "generation-zero", "token-zero", "token-owner", "reserved", "frame-version", "frame-size", "frame-reserved", "unaligned" })
        {
            CanonicalMmxSeed(fake, state);
            using var code = CreateFullBlock([new(MmxClassify([0x0f, 0x6f, 0xc1]))]);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
            var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop); f.X87 = state;
            switch (bad)
            {
                case "version": state->Version++; break; case "size": state->Size--; break;
                case "protocol": state->ProducerProtocol++; break; case "inactive": state->ProfileActive = 0; break;
                case "machine": state->Machine++; break; case "generation-zero": state->OwnerGeneration = 0; break;
                case "token-zero": state->OwnerToken = 0; break; case "token-owner": state->OwnerToken++; break;
                case "reserved": state->Reserved[19] = 1; break;
                case "frame-version": f.Version = 0x46420006; break; case "frame-size": f.Size = 400; break;
                case "frame-reserved": f.X87Reserved = 1; break;
                case "unaligned": f.X87 = (FullX87RegisterFile*)((byte*)state + 1); break;
            }
            byte[] before = CanonicalMmxBytes(fake, sizeof(Blink.Machine)), raw = CanonicalMmxBytes((byte*)state, 192);
            Check(FloatRunProbe(code, &f) == FullBlockExit.Invalidated && issued == 0 && completed == 0 && f.Executed == 0,
                "R141 parent ABI7 live-file ingress fails before issue/guest effects");
            CanonicalMmxSame(fake, state, before, raw, "R141 malformed live frame no effects");
        }
        // Exact nonzero generation/token authentication is the managed binding's
        // prepublication responsibility, rather than an inferred native compare.
        CanonicalMmxSeed(fake, state);
        var binding = new FullX87WorkerBinding(state, (nint)fake, 101, 73); binding.Bind();
        foreach (bool token in new[] { false, true })
        {
            if (token) state->OwnerToken++; else state->OwnerGeneration++;
            bool denied = false; try { binding.Get((nint)fake, 1); } catch (InvalidOperationException) { denied = true; }
            Check(denied, "R141 exact managed binding identity rejects publication");
            state->OwnerToken = 73; state->OwnerGeneration = 101;
        }
        binding.EnterNative();
        bool inFlight = false; try { binding.Get((nint)fake, 1); } catch (InvalidOperationException) { inFlight = true; }
        Check(inFlight, "R141 bound state inaccessible while native frame is live");
        binding.NativeReturned(); state->ProfileActive = 0;
        Check(binding.Get((nint)fake, 1) == 0, "R141 exact inactive binding publishes null");
        binding.Detach(); binding.RetireAfterJoin(allFramesDetached: true, workerJoined: true);
    }

    static void CanonicalMmxEncoding()
    {
        byte[] staging = new byte[FullBlockEmitter.CodeCapacity], common = new byte[FullBlockEmitter.ScaffoldCapacity];
        foreach (bool windows in new[] { false, true })
        foreach (bool shared in new[] { false, true })
        {
            FullBlockEmitter.EmitScaffold(common, out var layout, windows); var scaffold = layout.Resolve((nint)0x10000000);
            foreach (var i in CanonicalMmxInstructions())
            {
                FullBlockInstruction[] body = [new(i)];
                bool good = shared ? FullBlockEmitter.TryEmit(body, staging, in scaffold, out _, windows) :
                    FullBlockEmitter.TryEmit(body, staging, out _, windows);
                Check(good, "R141 every MMX family both ABI/scaffold encodes");
                int shortLength;
                Check(!(shared ? FullBlockEmitter.TryEmit(body, new byte[7], in scaffold, out shortLength, windows) :
                    FullBlockEmitter.TryEmit(body, new byte[7], out shortLength, windows)) && shortLength == 0,
                    "R141 tiny staging cannot publish partial MMX code");
                for (int warm = 0; warm < 8; ++warm)
                {
                    if (shared) FullBlockEmitter.TryEmit(body, staging, in scaffold, out _, windows);
                    else FullBlockEmitter.TryEmit(body, staging, out _, windows);
                }
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int n = 0; n < 16; ++n)
                    good &= shared ? FullBlockEmitter.TryEmit(body, staging, in scaffold, out _, windows) :
                        FullBlockEmitter.TryEmit(body, staging, out _, windows);
                Check(good && GC.GetAllocatedBytesForCurrentThread() == before, "R141 native selectors add no warmed managed allocation");
            }
        }
    }

    static void CanonicalMmxInactiveController()
    {
        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc); fixture.Fill(pc); WriteControllerCode(fixture, pc, [0x0f, 0x77]); fixture.At(pc);
#if BLINK_FULL_X87_STATE_OWNER
        var callback = (IHostGuestX87State)fixture.Owner;
        Check(callback.GetX87RegisterFile((nint)fixture.Machine(), 1) == 0, "R141 actual controller remains Active0");
#endif
        int prepared = 0;
        fixture.NativePrepared = pointer =>
        {
            var f = (FullBlockFrame*)pointer;
            Check(f->X87 == null && f->X87Reserved == 0, "R141 actual inactive publisher uses null ABI7 file"); ++prepared;
        };
        fixture.Counts(1); fixture.Step(pc);
        Check(prepared != 0 && fixture.Completed == 1 && fixture.Machine()->fpu.tw == -1,
            "R141 actual controller legacy EMMS compatibility with Active0");
#if BLINK_FULL_X87_STATE_OWNER
        Check(callback.GetX87RegisterFile((nint)fixture.Machine(), 1) == 0, "R141 controller is still inactive after native return");
#endif
    }
}
