using System;
using System.Linq;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using CX = Managed.Emulation.Execution.FullCanonicalX87StackOperation;

unsafe partial class Program
{
    // Independent ISA masks, not a call to the product decoder. Bit m-C0.
    // AMD's documented FFREEP is included; reserved selected aliases are separate.
    static bool CanonicalX87R174OrdinaryRegister(byte op, byte m) => op switch
    {
        0xd8 => m >= 0xc0,
        0xd9 => m is >= 0xc0 and <= 0xcf or 0xd0 or 0xe0 or 0xe1 or 0xe4 or 0xe5 or
            >= 0xe8 and <= 0xee or >= 0xf0,
        0xda => m is >= 0xc0 and <= 0xdf or 0xe9,
        0xdb => m is >= 0xc0 and <= 0xdf or 0xe2 or 0xe3 or >= 0xe8 and <= 0xf7,
        0xdc => m is >= 0xc0 and <= 0xcf or >= 0xe0,
        0xdd => m is >= 0xc0 and <= 0xc7 or >= 0xd0 and <= 0xef,
        0xde => m is >= 0xc0 and <= 0xcf or 0xd9 or >= 0xe0,
        0xdf => m is >= 0xc0 and <= 0xc7 or 0xe0 or >= 0xe8 and <= 0xf7,
        _ => false
    };
    static bool CanonicalX87R174Alias(byte op, byte m) => (op, m) is
        (0xd9, >= 0xd1 and <= 0xdf) or (0xdd, >= 0xc8 and <= 0xcf) or
        (0xdf, >= 0xc8 and <= 0xdf) or (0xdf, >= 0xe1 and <= 0xe7);
    static bool CanonicalX87R174OrdinaryMemory(byte op, int group) => op switch
    {
        0xd8 or 0xda or 0xdc or 0xde or 0xdf => true,
        0xd9 => group != 1,
        0xdb => group is 0 or 1 or 2 or 3 or 5 or 7,
        0xdd => group != 5,
        _ => false
    };
    static (uint Options, int Index) CanonicalX87R174StackReference(byte[] bytes)
    {
        // Independent form-to-operation table authenticates the operation fed
        // to the byte-array oracle; a wrong product alias mapping cannot make
        // the emitter and reference agree on the wrong primitive.
        if (bytes[^1] == 0x9b) return ((uint)CX.Fwait, 0);
        byte op = bytes[^2], m = bytes[^1];
        CX operation = (op, m) switch
        {
            (0xd9, >= 0xc0 and <= 0xc7) => CX.Fld,
            (0xd9, >= 0xc8 and <= 0xcf) or (0xdd or 0xdf, >= 0xc8 and <= 0xcf) => CX.Fxch,
            (0xdd, >= 0xc0 and <= 0xc7) => CX.Ffree,
            (0xdf, >= 0xc0 and <= 0xc7) => CX.Ffreep,
            (0xdd, >= 0xd0 and <= 0xd7) => CX.Fst,
            (0xd9, >= 0xd8 and <= 0xdf) or (0xdd, >= 0xd8 and <= 0xdf) or
                (0xdf, >= 0xd0 and <= 0xdf) => CX.Fstp,
            (0xd9, >= 0xd0 and <= 0xd7) => CX.Fnop,
            (0xd9, 0xe0) => CX.Fchs, (0xd9, 0xe1) => CX.Fabs,
            (0xd9, 0xf6) => CX.Fdecstp, (0xd9, 0xf7) => CX.Fincstp,
            (0xdb, 0xe2) => CX.Fnclex, (0xdb, 0xe3) => CX.Fninit,
            (0xdf, >= 0xe0 and <= 0xe7) => CX.FnstswAx,
            (0xd9, >= 0xe8 and <= 0xee) => (CX)((uint)CX.Fld1 + m - 0xe8),
            _ => throw new ArgumentException("Not a reference stack form")
        };
        int index = operation is CX.Fld or CX.Fst or CX.Fstp or CX.Fxch or CX.Ffree or CX.Ffreep ? m & 7 : 0;
        return ((uint)operation | (CanonicalX87R174Alias(op, m) ? 0x10000u : 0), index);
    }

    static byte[] CanonicalX87R174Tuple(byte op, byte m)
    {
        // One complete representative per selector tuple. A finite selector
        // closure is distinct from proof over every displacement/SIB/prefix.
        int mod = m >> 6, rm = m & 7;
        if (mod == 3) return [op, m];
        byte[] sib = rm == 4 ? [0x24] : [];
        byte[] displacement = mod == 1 ? [0x7f] : mod == 2 || rm == 5 ? [0x80,0xff,0xff,0xff] : [];
        return [op, m, ..sib, ..displacement];
    }

    static void CanonicalX87R174DomainTests()
    {
        int tuples = 0, ordinaryRegisters = 0, ordinaryMemory = 0, aliases = 0;
        using var decoder = new Blink(); using var binding = decoder.__DotCcEnter();
        for (int op = 0xd8; op <= 0xdf; ++op)
        for (int m = 0; m <= 255; ++m)
        {
            byte[] bytes = CanonicalX87R174Tuple((byte)op, (byte)m);
            bool ordinary = m >= 0xc0 ? CanonicalX87R174OrdinaryRegister((byte)op, (byte)m) :
                CanonicalX87R174OrdinaryMemory((byte)op, m >> 3 & 7);
            bool alias = m >= 0xc0 && CanonicalX87R174Alias((byte)op, (byte)m);
            Blink.XedDecodedInst decoded = default;
            fixed (byte* p = bytes) Check(decoder.DecodeInstruction(&decoded, p, (ulong)bytes.Length,
                Blink.XED_MODE_LONG) == 0 && decoded.length == bytes.Length,
                "R174 authoritative exact-span length for all2048 conservative selector tuples");
            bool found = FullTranslationJit.R159Classify(Ip, bytes, out var i);
            Check(found && (ordinary || alias || i.Options == FullCanonicalX87InvalidLowering.ReservedUd),
                "R174/R179 composition preserves defined forms/54 aliases; remaining selectors have explicit software UD: " + Convert.ToHexString(bytes));
            if (found)
            {
                Check(FullBlockEmitter.CanInline(i) && i.Operation == JitOperation.X87 && i.Length == bytes.Length &&
                    i.Extra == new JitOperand(0, (ulong)((op & 7) << 8 | m), IsImmediate: true),
                    "R174 selected tuple preserves full original FOP in composed native IR");
                if (alias) Check((i.Options & FullCanonicalX87StackLowering.SelectedAlias) != 0,
                    "R174 reserved aliases never acquire ordinary IR identity");
            }
            if (ordinary && m >= 0xc0) ++ordinaryRegisters;
            if (ordinary && m < 0xc0) ++ordinaryMemory;
            if (alias) ++aliases;
            ++tuples;
        }
        Check(tuples == 2048 && ordinaryRegisters == 353 && ordinaryMemory == 1440 && aliases == 54,
            "R174 exhaustive selector partition; cardinality follows exact masks, never witnesses");

        // Every recognized stack operation is checked against every FOP, with
        // both alias bits. Only a real decoded form with matching op/index/flag
        // may pass; source-register and metadata mutations cannot borrow an alias.
        var forms = CanonicalX87Forms().Select(CanonicalX87Classify).ToArray();
        var valid = forms.Select(g => (g.Options, g.Source.Register, g.Extra.Immediate)).ToHashSet();
        foreach (var original in forms.GroupBy(g => (g.Options & 0xffff, g.Source.Register)).Select(g => g.First()))
        {
            CX op = (CX)(original.Options & 0xffff);
            foreach (bool flag in new[] { false, true })
            for (uint fop = 0; fop <= 0x7ff; ++fop)
            {
                var bad = original with { Options = (uint)op | (flag ? FullCanonicalX87StackLowering.SelectedAlias : 0),
                    Extra = new(0, fop, IsImmediate: true) };
                bool expected = valid.Contains((bad.Options, bad.Source.Register, bad.Extra.Immediate));
                bool accepted;
                try { accepted = FullBlockEmitter.ValidateCanonicalX87Stack(bad); }
                catch (ArgumentException) { accepted = false; }
                Check(accepted == expected, "R174 exhaustive operation/index/FOP/profile validation relation");
            }
            byte[] body = CanonicalX87Forms().Single(b => CanonicalX87Classify(b) == original);
            byte[] fifteen = [..Enumerable.Repeat((byte)0x66, 15 - body.Length), ..body];
            Check(FullCanonicalX87StackLowering.TryClassify(Ip, fifteen, out var longest, out _) && longest.Length == 15 &&
                !FullCanonicalX87StackLowering.TryClassify(Ip, [0x66, ..fifteen], out _, out _) &&
                !FullCanonicalX87StackLowering.TryClassify(Ip, [..body, 0], out _, out _),
                "R174 exact1..15 span and trailing/truncated instruction rejection");
        }
        Check(!FullCanonicalX87StackLowering.IsConstant(CX.Ffreep), "R174 appended free cannot become constant/push");
    }

    static void CanonicalX87R174ConditionTests(byte* fake, FullX87RegisterFile* state)
    {
        foreach (byte[] form in CanonicalX87Forms())
        {
            var i = CanonicalX87Classify(form); using var code = CreateFullBlock([new(i)]);
            for (uint cc = 0; cc < 16; ++cc)
            {
                CanonicalX87Seed(fake, state, (int)(cc & 7)); CanonicalX87SeedTag(fake, state, 7, 3);
                uint status = ((Blink.Machine*)fake)->fpu.sw & ~0x4700u;
                ((Blink.Machine*)fake)->fpu.sw = status | (cc & 7) << 8 | (cc & 8) << 11;
                byte[] expected = CanonicalX87Bytes(fake, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)state, 192);
                CanonicalX87Expected(i, expected, raw);
                ulong epoch = 11; long issued = 0, completed = 0; int stop = 0; var h = FullHeader(Ip, code.Entry);
                var f = FullFrame(fake, &h, &epoch, &issued, &completed, &stop, limit: 1); f.X87 = state;
                Check(FloatRunProbe(code, &f) == FullBlockExit.Continue && issued == 1 && completed == 1,
                    "R174 C1 and all undefinedCC inputs follow the explicit preserve-or-clear profile");
                CanonicalX87Same(fake, state, expected, raw);
            }
        }
    }

    static void CanonicalX87R174ControllerTests()
    {
        foreach (byte[] body in CanonicalX87Forms())
        foreach (byte[] prefix in new byte[][] { [], [0xf3,0x4f,0x66,0x40] })
        {
            using var f = new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
            byte[] bytes = [..prefix, ..body];
            f.Map(pc); f.Fill(pc); WriteControllerCode(f, pc, [..bytes,0xeb,0xfe]);
            f.StartCanonicalFamily();
            try
            {
                var m = f.Machine(); var file = f.CanonicalFile();
                // Preserve real fixture identity/header. Never reset a live Machine.
                m->fpu.sw = 0x4700 | 7u << 11; m->fpu.cw = 0x37f; m->fpu.tw = unchecked((int)0x1234fffc);
                for (int p = 0; p < 8; ++p)
                    *FullX87RegisterFile.Physical(file, p) = CanonicalX87Value(p, p == 7 ? 0 : 3);
                file->CodeSelector = 0x1357; file->DataSelector = 0x2468;
                Write64((byte*)m, 656, 0x765); Write64((byte*)m, 664, 0x1234567812345678UL);
                Write64((byte*)m, 680, 0xabcdef01fedcba98UL);
                Check(FullTranslationJit.R159Classify(pc, bytes, out var i), "R174 actual compiler prefix projection");
                byte[] expected = CanonicalX87Bytes((byte*)m, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)file, 192);
                CanonicalX87Expected(i, expected, raw);
                f.Counts(100);
                Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
                f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
                Check(f.Step(pc), "R174 real compiler/controller register-family entry");
                for (int n = 0; f.Completed < 1 && n < 16; ++n) Check(f.Continue(), "R174 bounded same-issue continuation");
                Check(f.Issued == 1 && f.Completed == 1 && m->ip == pc + (uint)bytes.Length && m->oplen == 0 &&
                    f.Statistics.InlineNativeInstructions == 1 && f.Statistics.SemanticHelperInstructions == 0 &&
                    f.Statistics.InterpreterAttempts == 0, "R174 all stack/control selectors run actual native plans without semantic fallback");
                Check(new ReadOnlySpan<byte>((byte*)m + 648, 40).SequenceEqual(expected.AsSpan(648, 40)) &&
                    new ReadOnlySpan<byte>((byte*)m + 24, 128).SequenceEqual(expected.AsSpan(24, 128)) &&
                    new ReadOnlySpan<byte>((byte*)m + 160, 256).SequenceEqual(expected.AsSpan(160, 256)) &&
                    new ReadOnlySpan<byte>(file, 192).SequenceEqual(raw),
                    "R174 actual return reference SW/TW/CW/FIP/FDP/FOP, GPR/XMM and full canonical file");
            }
            finally { f.NativePrepared = null; f.NativeReturned = null; f.EndCanonicalFamily(); }
        }
    }

    static void CanonicalX87R174CancellationTests()
    {
        // One representative for every new control-flow equivalence class;
        // the direct native boundary table already checks every selector.
        foreach (byte[] bytes in new byte[][] { [0xdf,0xc0], [0xdf,0xc7], [0xd9,0xd1],
            [0xd9,0xd8], [0xdd,0xc8], [0xdf,0xc8], [0xdf,0xd0], [0xdf,0xd8], [0xdf,0xe1], [0xdf,0xe7] })
        {
            using var f = new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
            f.Map(pc); f.Fill(pc); WriteControllerCode(f, pc, [..bytes,0xeb,0xfe]); f.StartCanonicalFamily();
            try
            {
                var m = f.Machine(); var file = f.CanonicalFile();
                m->fpu.sw = 0x47ff; m->fpu.cw = 0x340;
                byte[] before = CanonicalX87Bytes((byte*)m, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)file, 192);
                f.Counts(100); f.Preissued = (_, _) => f.Stop.RequestStop();
                Check(f.Step(pc) && f.Issued == 1 && f.Completed == 0 && m->ip == pc &&
                    new ReadOnlySpan<byte>((byte*)m + 648, 40).SequenceEqual(before.AsSpan(648, 40)) &&
                    new ReadOnlySpan<byte>((byte*)m + 24, 128).SequenceEqual(before.AsSpan(24, 128)) &&
                    new ReadOnlySpan<byte>(file, 192).SequenceEqual(raw) &&
                    f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0,
                    "R174 actual cancellation after preissue precedes pendingMF, tag/TOP and FN state effects");
            }
            finally { f.Preissued = null; f.EndCanonicalFamily(); }
        }
    }

    static void CanonicalX87R174WaitPriorityTests()
    {
        // WAIT is its own instruction. Test each FN form, including all selected
        // status aliases, against each independently pending unmasked exception.
        foreach (byte[] fn in CanonicalX87Forms().Where(b => b.Length == 2 &&
            (b[0] == 0xdb && b[1] is 0xe2 or 0xe3 || b[0] == 0xdf && b[1] is >= 0xe0 and <= 0xe7)))
        foreach (uint bit in new uint[] { 1,2,4,8,16,32 })
        foreach (bool wait in new[] { false, true })
        {
            using var f = new ThreadedGuestExecution.FullControllerFixture();
            const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
            byte[] bytes = wait ? [0x9b,..fn] : fn;
            f.Map(pc); f.Fill(pc); WriteControllerCode(f, pc, [..bytes,0xeb,0xfe]); f.StartCanonicalFamily();
            try
            {
                var m = f.Machine(); var file = f.CanonicalFile();
                m->fpu.sw = 0x4700 | 3u << 11 | bit; m->fpu.cw = 0x37f & ~bit;
                byte[] expected = CanonicalX87Bytes((byte*)m, sizeof(Blink.Machine)), raw = CanonicalX87Bytes((byte*)file, 192);
                f.Counts(100);
                if (wait)
                {
                    var fault = f.StepSynchronousFault(pc);
                    Check(fault.Signal == 8 && f.Issued == 1 && f.Completed == 0 && m->ip == pc &&
                        new ReadOnlySpan<byte>((byte*)m + 648, 40).SequenceEqual(expected.AsSpan(648, 40)) &&
                        new ReadOnlySpan<byte>(file, 192).SequenceEqual(raw),
                        "R174 separate FWAIT pendingMF prevents following FN effects/metadata");
                }
                else
                {
                    Check(FullTranslationJit.R159Classify(pc, fn, out var i), "R174 FN classify");
                    CanonicalX87Expected(i, expected, raw);
                    Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
                    f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
                    Check(f.Step(pc) && f.Issued == 1 && f.Completed == 1 &&
                        new ReadOnlySpan<byte>((byte*)m + 648, 40).SequenceEqual(expected.AsSpan(648, 40)) &&
                        new ReadOnlySpan<byte>((byte*)m + 24, 128).SequenceEqual(expected.AsSpan(24, 128)) &&
                        new ReadOnlySpan<byte>(file, 192).SequenceEqual(raw),
                        "R174 FN executes under pendingMF without waiting or changing prior pointers");
                }
            }
            finally { f.NativePrepared = null; f.NativeReturned = null; f.EndCanonicalFamily(); }
        }
    }
}
