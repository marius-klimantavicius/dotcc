using System;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;

unsafe partial class Program
{
    // Entire fixture is UNRUN. It executes byte transport only when the parent
    // selects x87-state; no guest, emitter, native allocation or host FP involved.
    static void CheckFullX87State()
    {
        Check(sizeof(FullX87RegisterFile) == 192 && sizeof(FullExtended80) == 16, "R133 fixed state sizes");
        Check(Marshal.OffsetOf<FullX87RegisterFile>(nameof(FullX87RegisterFile.R0)).ToInt32() == 64 &&
            Marshal.OffsetOf<FullX87RegisterFile>(nameof(FullX87RegisterFile.R7)).ToInt32() == 176, "R133 physical slot layout");
        byte* allocation = stackalloc byte[512];
        var source = (FullX87RegisterFile*)(((nuint)allocation + 15) & ~(nuint)15);
        var target = (FullX87RegisterFile*)((byte*)source + 208);
        byte* m = stackalloc byte[736]; byte* n = stackalloc byte[736];
        var ms = new Span<byte>(m, 736); var ns = new Span<byte>(n, 736);
        ms.Fill(0xa5); ns.Fill(0x5a);
        FullX87StateStorage.Initialize(source, (nint)m, 1, 133);
        FullX87StateStorage.Initialize(target, (nint)n, 2, 133);
        Span<byte> image = stackalloc byte[512]; image.Fill(0x96);
        Check(X87Transfer(source, m, 1, HostX87Transfer.LinuxSave, image) == HostX87TransferResult.Inactive && image[0] == 0x96 && ms[648] == 0xa5,
            "R133 inactive leaves compatibility state and image alone");
        Check(source->ProfileActive == 0 && FullX87StateStorage.GetActive(source, (nint)m, 1, 133, 1) == 0, "R133 default inactive");
        bool refused = false;
        try { FullX87StateStorage.RequireActivationQualified(); } catch (NotSupportedException) { refused = true; }
        Check(refused == (FullCanonicalX87Activation.Blockers != FullCanonicalX87ActivationBlocker.None), "canonical activation follows actual owner/platform/numerical qualification");
        // TEST-ONLY positive transport setup. Product has no activation writer.
        source->ProfileActive = 1; target->ProfileActive = 1;
        X87Transfer(source, m, 1, HostX87Transfer.CpuReset, Span<byte>.Empty);
        for (int p = 0; p < 8; ++p)
        {
            var slot = FullX87RegisterFile.Physical(source, p); *slot = X87Literal(p);
        }
        for (int i = 0; i < 256; ++i) ms[160 + i] = (byte)(i ^ 0x53);
        X87W16(ms, 672, 0x0b7f); X87W32(ms, 656, 0x5ab);
        X87W64(ms, 664, 0x123456789abcdef0); X87W64(ms, 680, 0xfedcba9876543210);
        source->CodeSelector = 0x23; source->DataSelector = 0x2b;
        for (int top = 0; top < 8; ++top)
        for (int ftw = 0; ftw < 256; ++ftw)
        {
            ushort tags = 0, reclassified = 0;
            for (int p = 0; p < 8; ++p)
            {
                // Deliberately full "valid" tags on nonempty specials to check
                // restore reclassifies values instead of preserving fake tags.
                tags |= (ushort)(((ftw & (1 << p)) == 0 ? 3 : 0) << (2 * p));
                reclassified |= (ushort)(((ftw & (1 << p)) == 0 ? 3 : X87ExpectedTag(p)) << (2 * p));
            }
            X87W32(ms, 648, (uint)(0x4500 | (top << 11))); X87W32(ms, 652, tags);
            Check(X87Transfer(source, m, 1, HostX87Transfer.LinuxSave, image) == HostX87TransferResult.Complete, "R133 Linux save");
            Check(image[4] == ftw && image[5] == 0 && X87U16(image, 2) == (0x4500 | top << 11), "R133 FTW physical, FSW TOP");
            Check(X87U16(image, 0) == 0x0b7f && X87U16(image, 6) == 0x5ab &&
                X87U64(image, 8) == 0x123456789abcdef0 && X87U64(image, 16) == 0xfedcba9876543210, "R133 metadata exact64");
            for (int logical = 0; logical < 8; ++logical)
            {
                var v = X87Literal((top + logical) % 8);
                Check(X87U64(image, 32 + logical * 16) == v.Significand && X87U16(image, 40 + logical * 16) == v.SignExponent,
                    "R133 saved ST logical permutation independent of FTW");
                for (int pad = 10; pad < 16; ++pad) Check(image[32 + logical * 16 + pad] == 0, "R133 wire slot padding");
            }
            Check(X87Transfer(target, n, 2, HostX87Transfer.LinuxRestore, image) == HostX87TransferResult.Complete, "R133 Linux restore");
            Check(X87U16(ns, 652) == reclassified && X87U16(ns, 648) == X87U16(ms, 648), "R133 reconstructed physical full tags");
            for (int p = 0; p < 8; ++p) X87CheckValue(*FullX87RegisterFile.Physical(target, p), X87Literal(p));
            Check(ns.Slice(160, 256).SequenceEqual(ms.Slice(160, 256)), "R133 independent XMM signal copy");
        }
        Check(ms.Slice(584, 64).IndexOfAnyExcept((byte)0xa5) == -1 && ns.Slice(584, 64).IndexOfAnyExcept((byte)0x5a) == -1,
            "R133 active never accesses double shadow or spills metadata through it");
        X87ParentTopologyLiteral(source, m, image);
        X87StatusImageLiteral(target, n, image);
        X87Malformed(source, m, image);
        X87Formats(source, m, target, n);
        X87LegacyRestoreProfileLiterals(target, n);
        X87Mmx(source, m);
        X87Lifetime(source, m, target, n);
        X87Quota();
    }
    static HostX87TransferResult X87Transfer(FullX87RegisterFile* s, byte* m, ulong generation, HostX87Transfer op, Span<byte> image)
        => FullX87StateBridge.Transfer(s, (nint)m, generation, 133, 1, op, image);
    static FullExtended80 X87Literal(int p) => p switch
    {
        0 => new(0x8000000000000001, 0x3fff), // below binary64 rounding unit
        1 => new(0xfedcba9876543210, 0x6000), // above binary64 exponent range
        2 => new(1, 0),                     // below binary64 exponent range
        3 => new(0, 0x8000),                // negative zero
        4 => new(0x8000000000000001, 0x7fff), // signaling NaN preserved
        5 => new(0x123456789abcdef0, 0x1234), // unsupported unnormal preserved
        6 => new(0x8000000000000000, 0),      // pseudo-denormal preserved
        _ => new(0x8000000000000000, 0xffff), // negative infinity
    };
    static ushort X87ExpectedTag(int p) => p switch { 0 or 1 => 0, 3 => 1, _ => 2 };
    static void X87CheckValue(FullExtended80 a, FullExtended80 b)
        => Check(a.Significand == b.Significand && a.SignExponent == b.SignExponent && a.Reserved10 == 0 && a.Reserved12 == 0, "R133 exact raw80 and canonical padding");
    static ushort X87U16(ReadOnlySpan<byte> b, int i) => BinaryPrimitives.ReadUInt16LittleEndian(b.Slice(i, 2));
    static uint X87U32(ReadOnlySpan<byte> b, int i) => BinaryPrimitives.ReadUInt32LittleEndian(b.Slice(i, 4));
    static ulong X87U64(ReadOnlySpan<byte> b, int i) => BinaryPrimitives.ReadUInt64LittleEndian(b.Slice(i, 8));
    static void X87W16(Span<byte> b, int i, ushort v) => BinaryPrimitives.WriteUInt16LittleEndian(b.Slice(i, 2), v);
    static void X87W32(Span<byte> b, int i, uint v) => BinaryPrimitives.WriteUInt32LittleEndian(b.Slice(i, 4), v);
    static void X87W64(Span<byte> b, int i, ulong v) => BinaryPrimitives.WriteUInt64LittleEndian(b.Slice(i, 8), v);

    static void X87ParentTopologyLiteral(FullX87RegisterFile* s, byte* m, Span<byte> image)
    {
        // Parent-owned AMD FXTRACT control, finite topology witness only.
        // This test copies literal raw state; it never executes FXTRACT or
        // adopts its arithmetic as a serializer oracle. Entire test UNRUN.
        Span<byte> ms = new(m, 736); ushort oldSw = X87U16(ms, 648), oldTw = X87U16(ms, 652);
        FullExtended80 old6 = s->R6, old7 = s->R7;
        s->R6 = new(0xc000000000000001, 0x3fff); s->R7 = new(0, 0);
        X87W32(ms, 648, 0x3000); X87W32(ms, 652, 0x4fff);
        X87Transfer(s, m, 1, HostX87Transfer.LinuxSave, image);
        Check(image[4] == 0xc0 && X87U64(image, 32) == 0xc000000000000001 && X87U16(image, 40) == 0x3fff &&
            X87U64(image, 48) == 0 && X87U16(image, 56) == 0, "R133 AMD literal TOP6: logical ST0/R6, ST1/R7, physical FTWc0");
        s->R7 = new(0x8000000000000123, 0x7fff); X87W32(ms, 648, 0xb881); X87W32(ms, 652, 0xbfff);
        X87Transfer(s, m, 1, HostX87Transfer.LinuxSave, image);
        Check(image[4] == 0x80 && X87U64(image, 32) == 0x8000000000000123 && X87U16(image, 40) == 0x7fff &&
            (image[4] & 1) == 0, "R133 AMD literal TOP7: original raw ST0/R7, physical R0 logical slot1 remains empty");
        s->R6 = old6; s->R7 = old7; X87W32(ms, 648, oldSw); X87W32(ms, 652, oldTw);
    }

    static void X87Malformed(FullX87RegisterFile* s, byte* m, Span<byte> image)
    {
        Span<byte> before = stackalloc byte[736]; new ReadOnlySpan<byte>(m, 736).CopyTo(before);
        Span<byte> values = stackalloc byte[192]; new ReadOnlySpan<byte>(s, 192).CopyTo(values);
        X87W32(image, 24, 0x10000); X87W32(image, 28, 0xffffffff); // attacker cannot widen mask
        foreach (var op in new[] { HostX87Transfer.LinuxValidate, HostX87Transfer.LinuxRestore, HostX87Transfer.Fx64Restore })
            Check(X87Transfer(s, m, 1, op, image) == HostX87TransferResult.BadImage, "R133 malformed MXCSR rejected");
        Check(before.SequenceEqual(new ReadOnlySpan<byte>(m, 736)) && values.SequenceEqual(new ReadOnlySpan<byte>(s, 192)), "R133 malformed restore atomic rejection");
        X87W32(image, 24, 0x1f80); X87W32(image, 464, 0x46505853);
        Check(X87Transfer(s, m, 1, HostX87Transfer.LinuxRestore, image) == HostX87TransferResult.BadImage, "R133 extended XSAVE not truncated");
        X87W32(image, 464, 0);
        bool failed = false;
        try { X87Transfer(s, m, 1, HostX87Transfer.LinuxRestore, image.Slice(0, 511)); } catch (InvalidOperationException) { failed = true; }
        Check(failed, "R133 short host image rejected before mutation");
        foreach (uint bad in new uint[] { 0, 2, uint.MaxValue })
        {
            failed = false; try { FullX87StateBridge.Transfer(s, (nint)m, 1, 133, bad, HostX87Transfer.LinuxRestore, image); }
            catch (InvalidOperationException) { failed = true; } Check(failed, "R133 strict producer version handshake");
        }
        failed = false; try { X87Transfer(s, m, 1, (HostX87Transfer)99, image); } catch (InvalidOperationException) { failed = true; }
        Check(failed, "R133 unknown transfer cannot fall back");
        FullX87RegisterFile original = *s;
        for (int badHeader = 0; badHeader < 7; ++badHeader)
        {
            *s = original;
            switch (badHeader)
            {
                case 0: s->Version = 0; break;
                case 1: s->Size = 191; break;
                case 2: s->ProducerProtocol = 2; break;
                case 3: s->ProfileActive = 2; break;
                case 4: s->OwnerGeneration = 2; break;
                case 5: s->OwnerToken = 134; break;
                case 6: s->Reserved[19] = 1; break;
            }
            failed = false;
            try { FullX87StateStorage.GetActive(s, (nint)m, 1, 133, 1); }
            catch (InvalidOperationException) { failed = true; }
            Check(failed, "R133 malformed bound identity/header never means inactive");
        }
        *s = original;
    }
    static void X87StatusImageLiteral(FullX87RegisterFile* t, byte* n, ReadOnlySpan<byte> valid)
    {
        Span<byte> image = stackalloc byte[512]; valid.CopyTo(image);
        X87W16(image, 2, 0x8000);
        X87Transfer(t, n, 2, HostX87Transfer.LinuxRestore, image);
        Check(X87U16(new ReadOnlySpan<byte>(n, 736), 648) == 0, "R133 restored B mirrors clear ES");
        X87W16(image, 2, 0x0080);
        X87Transfer(t, n, 2, HostX87Transfer.LinuxRestore, image);
        Check(X87U16(new ReadOnlySpan<byte>(n, 736), 648) == 0x8080, "R133 restored B mirrors set ES");
    }
    static void X87Formats(FullX87RegisterFile* s, byte* m, FullX87RegisterFile* t, byte* n)
    {
        Span<byte> ms = new(m, 736), ns = new(n, 736);
        Span<byte> image = stackalloc byte[512];
        for (int top = 0; top < 8; ++top)
        {
            X87W32(ms, 648, (uint)(top << 11)); X87W32(ms, 652, 0);
            X87W64(ms, 664, 0x123456789abcdef0); X87W64(ms, 680, 0xfedcba9876543210);
            s->CodeSelector = 0x23; s->DataSelector = 0x2b; X87W32(ms, 656, 0x5ab);
            X87Transfer(s, m, 1, HostX87Transfer.FxLong32Save, image);
            Check(X87U32(image, 8) == 0x9abcdef0 && X87U32(image, 16) == 0x76543210 &&
                X87U16(image, 12) == 0x23 && X87U16(image, 20) == 0x2b, "R133 long REX.W0 selectors and32bit pointers");
            X87Transfer(t, n, 2, HostX87Transfer.FxLong32Restore, image);
            Check(X87U64(ns, 664) == 0x9abcdef0 && X87U64(ns, 680) == 0x76543210 && t->CodeSelector == 0x23,
                "R133 legacy pointer zero extension");
            ns.Slice(288, 128).Fill(0xe7);
            X87Transfer(s, m, 1, HostX87Transfer.FxLegacySave, image);
            X87Transfer(t, n, 2, HostX87Transfer.FxLegacyRestore, image);
            Check(ns.Slice(288, 128).IndexOfAnyExcept((byte)0xe7) == -1, "R133 legacy codec keeps XMM8..15 separate");
            foreach (bool small in new[] { true, false })
            {
                int env = small ? 14 : 28;
                var save = small ? HostX87Transfer.Save94 : HostX87Transfer.Save108;
                var restore = small ? HostX87Transfer.Restore94 : HostX87Transfer.Restore108;
                X87Transfer(s, m, 1, save, image.Slice(0, env + 80));
                for (int l = 0; l < 8; ++l)
                    Check(X87U64(image, env + l * 10) == X87Literal((l + top) % 8).Significand,
                        "R133 FSAVE94/108 stores logical order for every TOP");
                X87W32(ns, 656, 0x7ff); // distinct prior omitted-field witness
                X87Transfer(t, n, 2, restore, image.Slice(0, env + 80));
                Check(X87U64(ns, 664) == (small?0xdef0UL:0x9abcdef0UL) &&
                    X87U64(ns, 680) == (small?0x3210UL:0x76543210UL) &&
                    X87U32(ns, 656) == (small?0u:0x5abu) && t->CodeSelector == 0x23 && t->DataSelector == 0x2b,
                    "R163r1 protected14/28 widths/selectors and deterministic small FOP0 profile");
                for (int p = 0; p < 8; ++p) X87CheckValue(*FullX87RegisterFile.Physical(t, p), X87Literal(p));
                X87W32(ms, 672, 0x0340);
                X87Transfer(s, m, 1, small ? HostX87Transfer.Env14Save : HostX87Transfer.Env28Save, image.Slice(0, env));
                Check(X87U16(ms, 672) == 0x0340 && X87U16(image, 0) == 0x0340, "R133 snapshot does not precommit FNSTENV masking");
                X87Transfer(s, m, 1, HostX87Transfer.EnvironmentSaved, Span<byte>.Empty);
                Check(X87U16(ms, 672) == 0x037f, "R133 mask only after successful checked copyout");
            }
        }
    }
    static void X87LegacyRestoreProfileLiterals(FullX87RegisterFile* target, byte* machine)
    {
        // Policy approved after the independent legacy-image AMD witness:
        // omitted protected16 FOP clears to0. This transports a finite virtual
        // CPU profile; it neither asserts an Intel rule nor executes host x87.
        byte[] env14=Convert.FromHexString("7F010098000088774433AA996655");
        byte[] env28=Convert.FromHexString("7F01FFFF0098FFFF0000FFFF21436587BC9A65F798BADCFE4523FFFF");
        byte[] payload=Convert.FromHexString(
            "0100000000000080FF3F1032547698BADCFE00600100000000000000000000000000000000000080"+
            "0100000000000080FF7FF0DEBC9A785634123412000000000000008000000000000000000080FFFF");
        // Literal normalized full tags for raw logical payload at TOP0..7.
        ushort[] rotatedTags=[0xaa60,0xa982,0xa60a,0x982a,0x60aa,0x82a9,0x0aa6,0x2a98];
        ushort[] mixedEmptyTags=[0xbb6c,0xbb8e,0xb70e,0xbb2e,0x73ae,0xb3ad,0x3bae,0x3b9c];
        foreach (bool small in new[] { true,false })
        foreach (bool registers in new[] { false,true })
        foreach (uint prior in new uint[] { 0,1,0x123,0x7ff })
        for (int top=0;top<8;++top)
        foreach (ushort cw in new ushort[] { 0x017f,0xffff })
        foreach (bool es in new[] { false,true })
        foreach (ushort tags in new ushort[] { 0,0x5555,0xaaaa,0xffff,0x7b1e })
        {
            byte[] env=(small?env14:env28).AsSpan().ToArray();
            X87W16(env,0,cw);X87W16(env,small?2:4,(ushort)((top<<11)|(es?0x80:0x8000)));X87W16(env,small?4:8,tags);
            byte[] image=registers?[..env,..payload]:env;
            var op=small?(registers?HostX87Transfer.Restore94:HostX87Transfer.Env14Restore):
                registers?HostX87Transfer.Restore108:HostX87Transfer.Env28Restore;
            for (int p=0;p<8;++p) { var slot=FullX87RegisterFile.Physical(target,p);*slot=X87Literal(p); }
            Span<byte> m=new(machine,736);X87W32(m,656,prior);X87W64(m,664,ulong.MaxValue);X87W64(m,680,ulong.MaxValue);
            target->CodeSelector=0xffff;target->DataSelector=0xffff;
            byte[] expected=m.ToArray(),file=new ReadOnlySpan<byte>(target,192).ToArray(),input=(byte[])image.Clone();
            X87W32(expected,672,cw);X87W32(expected,648,(uint)((top<<11)|(es?0x8080:0)));
            uint wantTags=tags == 0xffff?0xffffu:tags == 0x7b1e?
                registers?mixedEmptyTags[top]:0xbb6cu:registers?rotatedTags[top]:0xaa60u;
            X87W32(expected,652,wantTags);X87W32(expected,656,small?0u:0x765u);
            X87W64(expected,664,small?0x7788UL:0x87654321UL);X87W64(expected,680,small?0x99aaUL:0xfedcba98UL);
            X87W16(file,40,small?(ushort)0x3344:(ushort)0x9abc);X87W16(file,42,small?(ushort)0x5566:(ushort)0x2345);
            if (registers)
                for (int logical=0;logical<8;++logical)
                { file.AsSpan(64+((top+logical)&7)*16,16).Clear();payload.AsSpan(logical*10,10).CopyTo(file.AsSpan(64+((top+logical)&7)*16)); }
            Check(X87Transfer(target,machine,2,op,image) == HostX87TransferResult.Complete,
                "R163r1 all14/28/94/108 legacy restore transports complete");
            Check(m.SequenceEqual(expected) && new ReadOnlySpan<byte>(target,192).SequenceEqual(file) && image.AsSpan().SequenceEqual(input),
                "R163r1 literal full Machine/file/image: old nonzeroFOP, raw CW, ES-B/TOP, exact raw80/padding and selectors");
            // Exact-size enforcement precedes every legacy transfer mutation,
            // just as the native leaf's memory prechecks precede effects.
            foreach (int delta in new[] { -1,1 })
            {
                byte[] bad=new byte[image.Length+delta];bool refused=false;
                try { X87Transfer(target,machine,2,op,bad); }catch (InvalidOperationException) { refused=true; }
                Check(refused && m.SequenceEqual(expected) && new ReadOnlySpan<byte>(target,192).SequenceEqual(file),
                    "R163r1 wrong-size legacy restore retains all state before rejection");
            }
        }
    }

    static void X87Mmx(FullX87RegisterFile* s, byte* m)
    {
        Span<byte> ms = new(m, 736), xmm = stackalloc byte[256]; ms.Slice(160, 256).CopyTo(xmm);
        for (int p = 0; p < 8; ++p)
        {
            X87W32(ms, 648, 0x7d55); X87W32(ms, 652, 0xffff);
            FullX87StateBridge.MmxWrite(s, (nint)m, p, 0x0123456789abcdefUL + (uint)p);
            Check(FullX87StateBridge.MmxRead(s, p) == 0x0123456789abcdefUL + (uint)p &&
                FullX87RegisterFile.Physical(s, p)->SignExponent == 0xffff && X87U16(ms, 652) == 0 &&
                X87U16(ms, 648) == (0x7d55 & ~0x3800), "R133 MMX physical alias/upper16/TOP/allvalid");
        }
        Check(xmm.SequenceEqual(ms.Slice(160, 256)), "R133 MMX never aliases independent XMM");
        ushort sw = X87U16(ms, 648); FullX87StateBridge.Emms((nint)m);
        Check(X87U16(ms, 652) == 0xffff && X87U16(ms, 648) == sw, "R133 EMMS empties tags without TOP mutation");
    }
    static void X87Lifetime(FullX87RegisterFile* s, byte* m, FullX87RegisterFile* t, byte* n)
    {
        new ReadOnlySpan<byte>(m, 736).CopyTo(new Span<byte>(n, 736)); // NewMachine metadata value copy model
        FullX87StateStorage.CloneValues(s, (nint)m, 1, t, (nint)n, 2, 133);
        Check(t->Machine == (ulong)(nuint)n && t->OwnerGeneration == 2 && t->ProfileActive == 1, "R133 clone preserves child identity");
        ulong prior = s->R0.Significand; t->R0.Significand ^= 1;
        Check(s->R0.Significand == prior, "R133 clone has no register pointer alias");
        bool rejected = false;
        try { FullX87StateStorage.CloneValues(s, (nint)m, 1, s, (nint)m, 1, 133); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "R133 clone alias rejection");
        var binding = new FullX87WorkerBinding(s, (nint)m, 1, 133); binding.Bind();
        Span<byte> nested = stackalloc byte[512];
        binding.Transfer((nint)m, 1, HostX87Transfer.LinuxSave, nested);
        binding.Transfer((nint)m, 1, HostX87Transfer.LinuxHandlerEnter, Span<byte>.Empty);
        Check(s->R0.Significand == 0 && X87U16(new ReadOnlySpan<byte>(m, 736), 652) == 0xffff,
            "R133 Linux handler starts with initial FP state after frame install");
        FullX87StateBridge.MmxWrite(s, (nint)m, 0, 42); // handler changes same physical CPU
        binding.Transfer((nint)m, 1, HostX87Transfer.LinuxRestore, nested);
        Check(s->R0.Significand == prior, "R133 recursive signal frame restores interrupted physical values");
        rejected = false; try { binding.Get((nint)n, 1); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "R133 exact Machine identity");
        binding.EnterNative(); rejected = false;
        try { binding.Get((nint)m, 1); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "R133 callback refuses live native leaf"); binding.NativeReturned();
        rejected = false; try { binding.RejectLegacy((nint)m, 1); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected, "R133 no active binary64 fallback");
        FullExtended80 value = s->R0;
        binding.Transfer((nint)m, 1, HostX87Transfer.Finit, Span<byte>.Empty);
        X87CheckValue(s->R0, value);
        Check(X87U16(new ReadOnlySpan<byte>(m, 736), 672) == 0x037f && X87U16(new ReadOnlySpan<byte>(m, 736), 652) == 0xffff,
            "R133 FINIT metadata and empty tags, payload retained");
        binding.Transfer((nint)m, 1, HostX87Transfer.CpuReset, Span<byte>.Empty);
        Check(s->R0.Significand == 0 && X87U32(new ReadOnlySpan<byte>(m, 736), 688) == 0x1f80, "R133 reset/exec initial state");
        binding.Detach(); rejected = false;
        try { binding.RetireAfterJoin(false, true); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected && s->Version == 1, "R133 no retirement before all frame detachment");
        binding.RetireAfterJoin(true, true);
        Check(s->Version == 0 && s->Machine == 0, "R133 terminal poison before free");
    }
    static void X87Quota()
    {
        ulong used = 100;
        Check(FullX87StateStorage.FixedCharge == 192 && !FullX87StateStorage.TryCharge(ref used, 291) && used == 100, "R133 quota denies exact shortfall");
        Check(FullX87StateStorage.TryCharge(ref used, 292) && used == 292, "R133 fixed bytes charged before allocation");
        FullX87StateStorage.ReleaseCharge(ref used); Check(used == 100, "R133 allocation rollback/terminal release amount");
        bool rejected = false; try { FullX87StateStorage.ReleaseCharge(ref used); } catch (InvalidOperationException) { rejected = true; }
        Check(rejected && used == 100, "R133 quota duplicate-release underflow");
        used = ulong.MaxValue - 100;
        Check(!FullX87StateStorage.TryCharge(ref used, ulong.MaxValue), "R133 quota overflow fails closed");
        Check(!FullX87StateStorage.TryNextGeneration(ulong.MaxValue, out ulong zero) && zero == 0 &&
            FullX87StateStorage.TryNextGeneration(0, out ulong one) && one == 1, "R133 identity generation never wraps");
    }
}
