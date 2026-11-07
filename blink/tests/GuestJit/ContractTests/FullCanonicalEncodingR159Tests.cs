using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using R159Fixture = Managed.Emulation.Execution.ThreadedGuestExecution.FullControllerFixture;

unsafe partial class Program
{
    // AUTHORED_UNRUN. One entry for the entire prefix/encoding family. Parent
    // wires Main and runs its coalesced CoreCLR/NativeAOT qualification gate.
    static void CheckFullCanonicalEncodingR159(byte* machine)
    {
        CheckR159ProjectionCells();
        using (var oracle = new R159Fixture(interpreterOracle: true))
        {
            CheckR159FamilyGrammar(oracle.Program);
            CheckR159RepGrammar(oracle.Program);
            CheckR159RegisterOracle(oracle);
        }
        CheckR159RelativeAndEa(machine);
        CheckR159Polls(machine);
        CheckR159Controller();
        CheckR159FaultController();
        CheckR159X87Controller();
        CheckR159RepController();
        ResetFull(machine);
    }

    static byte[] R159Normalize(byte[] bytes)
    {
        Span<byte> target = stackalloc byte[15];
        Check(FullCanonicalEncoding.TryNormalize(bytes, target, out int length, out var reason),
            $"R159 projection {Convert.ToHexString(bytes)}: {reason}");
        return target[..length].ToArray();
    }

    static void CheckR159ProjectionCells()
    {
        (byte[] Original, byte[] Expected)[] cells = [
            ([0x4f,0x66,0x66,0x88,0xe0], [0x88,0xe0]), // AH source: legacy canceled REX
            ([0x66,0x4f,0x40,0x88,0xe0], [0x40,0x88,0xe0]), // SPL, not AH; final REX wins
            ([0x66,0x66,0x4e,0x80,0xc4,1], [0x40,0x80,0xc4,1]), // ignored W/R/X, retain SPL
            ([0x66,0x67,0x65,0x4a,0x0f,0xef,0xc1], [0x66,0x0f,0xef,0xc1]),
            ([0x67,0x66,0x65,0x49,0x0f,0xef,0x05,0,0,0,0], [0x65,0x67,0x66,0x0f,0xef,0x05,0,0,0,0]),
            ([0x66,0x49,0x0f,0xef,0x04,0x25,0,0,0,0], [0x66,0x0f,0xef,0x04,0x25,0,0,0,0]),
            ([0x66,0x4b,0x0f,0xef,0x04,0x25,0,0,0,0], [0x66,0x42,0x0f,0xef,0x04,0x25,0,0,0,0]), // R12 index
            ([0x4f,0x0f,0x6f,0xc1], [0x0f,0x6f,0xc1]), // MMX fields do not extend
            ([0x4f,0x0f,0x6e,0xc1], [0x49,0x0f,0x6e,0xc1]), // MMX destination, GPR source
            ([0x4f,0x0f,0x2c,0xc1], [0x41,0x0f,0x2c,0xc1]), // MMX dest, XMM source
            ([0x4f,0x0f,0x2a,0xc1], [0x44,0x0f,0x2a,0xc1]), // XMM dest, MMX source
            ([0x67,0x65,0x66,0x4a,0x0f,0xf7,0xc1], [0x65,0x67,0x66,0x0f,0xf7,0xc1]), // implicit EDI
            ([0xf3,0x4f,0x0f,0x2c,0xc1], [0xf3,0x4d,0x0f,0x2c,0xc1]), // GPR width and both registers
            ([0x66,0x66,0x67,0x65,0x4f,0xd9,0xc0], [0xd9,0xc0]),
            ([0x67,0x66,0x66,0x64,0x4d,0xd9,0x23], [0x64,0x67,0x66,0x49,0xd9,0x23]), // REX.W selects ENV32, base R11
            ([0x67,0x66,0x36,0x4f,0x8b,0x05,0,0,0,0], [0x36,0x67,0x4c,0x8b,0x05,0,0,0,0]),
            ([0x65,0x67,0x4f,0xeb,0], [0xeb,0]),
            ([0x66,0x65,0x67,0x4f,0xe2,0], [0x67,0xe2,0]),
            ([0x67,0x4f,0x0f,0x95,0xfc], [0x41,0x0f,0x95,0xc4]), // SETcc unused reg, meaningful R12b extension
            ([0x66,0xf3,0x4a,0x0f,0xbc,0xc1], [0x66,0xf3,0x48,0x0f,0xbc,0xc1]),
            ([0x66,0xf2,0x4a,0x0f,0x38,0xf0,0xc4], [0x66,0xf2,0x48,0x0f,0x38,0xf0,0xc4]),
            ([0x67,0x66,0xf0,0x4c,0x83,0x03,1], [0xf0,0x67,0x48,0x83,0x03,1])
        ];
        foreach (var cell in cells)
        {
            byte[] preserved = cell.Original.ToArray();
            byte[] canonical = R159Normalize(cell.Original);
            Check(canonical.SequenceEqual(cell.Expected), $"R159 operand-role projection {Convert.ToHexString(cell.Original)}");
            Check(cell.Original.SequenceEqual(preserved), "R159 original bytes never mutated");
            Check(R159Normalize(canonical).SequenceEqual(canonical), "R159 projection is idempotent");
        }
        foreach (byte[] bytes in new byte[][] {
            [], [0x66], [0x0f], [0x0f,0x38], [0x66,0x0f,0xef], [0x66,0x0f,0xef,0x04],
            [0xf0,0x01,0xc1], [0xf0,0x39,0x03], [0xf0,0x83,0x3b,1], [0xf0,0xa4],
            [0xf2,0xf3,0x90], [0x64,0x65,0x8b,0x03], [0x66,0xf3,0x0f,0x58,0xc1],
            [0xf2,0xeb,0], [0xf3,0x01,0xc1], [0xf2,0xf0,0x01,0x03],
            [0xc5,0xf8,0x77], Enumerable.Repeat((byte)0x66,16).ToArray() })
        {
            Span<byte> target = stackalloc byte[15];
            Check(!FullCanonicalEncoding.TryNormalize(bytes, target, out _, out var why) && why != null,
                "R159 invalid/reserved/unproved policy cells retain exact rejection");
        }
        for (int rex = 0x40; rex <= 0x4f; ++rex)
        {
            Check(R159Normalize([(byte)rex,0x0f,0x6f,0xc1]).SequenceEqual(new byte[] {0x0f,0x6f,0xc1}), "R159 every ignored MMX REX field");
            byte[] sse = R159Normalize([0x66,(byte)rex,0x0f,0xef,0xc1]);
            byte meaningful = (byte)(rex & 5);
            byte[] expected = meaningful == 0 ? [0x66,0x0f,0xef,0xc1] : [0x66,(byte)(0x40|meaningful),0x0f,0xef,0xc1];
            Check(sse.SequenceEqual(expected), "R159 every SSE register extension and ignored W/X combination");
        }
        byte[] fifteen = [..Enumerable.Repeat((byte)0x66,13),0x88,0xe0];
        Check(R159Normalize(fifteen).SequenceEqual(new byte[] {0x88,0xe0}), "R159 exact maximum length redundant prefixes");
        Check(FullCanonicalEncoding.InspectPrefixDomain([0x66,0x48,0x89,0xc1]) == FullCanonicalEncoding.PrefixDomain.SinglePerGroup &&
            FullCanonicalEncoding.InspectPrefixDomain(fifteen) == FullCanonicalEncoding.PrefixDomain.RepeatedLegacy &&
            FullCanonicalEncoding.InspectPrefixDomain([0x4f,0x66,0x40,0x88,0xe0]) ==
                (FullCanonicalEncoding.PrefixDomain.MultipleRex | FullCanonicalEncoding.PrefixDomain.RexBeforeLegacy),
            "R159 selected-decoder aliases are distinguished from single-per-group architectural candidates");
    }

    readonly record struct R159Form(string Family, byte[] Bytes, bool RegisterOnly = false);
    static IEnumerable<R159Form> R159Forms()
    {
        foreach (byte op in new byte[] {0x00,0x02,0x08,0x0a,0x10,0x12,0x18,0x1a,0x20,0x22,0x28,0x2a,0x30,0x32,0x38,0x3a,0x84,0x86,0x88,0x8a})
        {
            yield return new("scalar byte", [op,0xe1], true);
            yield return new("scalar byte memory", [0x64,0x67,op,0x21]);
        }
        foreach (byte op in new byte[] {0x01,0x03,0x09,0x0b,0x11,0x13,0x19,0x1b,0x21,0x23,0x29,0x2b,0x31,0x33,0x39,0x3b,0x85,0x87,0x89,0x8b})
        foreach (byte[] width in new byte[][] {[],[0x66],[0x48]})
        {
            yield return new("scalar width", [..width,op,0xc1], true);
            yield return new("scalar memory", [0x65,0x67,..width,op,0x03]);
        }
        foreach (byte[] bytes in new byte[][] {
            [0xf3,0x0f,0x1e,0xfa],[0x66,0x0f,0x1f,0x00],
            [0x66,0x83,0xc0,1],[0x48,0xf7,0xd8],[0x66,0xc1,0xe0,1],[0x48,0x69,0xc1,2,0,0,0],
            [0x66,0x0f,0xb6,0xc4],[0x48,0x0f,0xbe,0xc4],[0x66,0x0f,0xa4,0xc1,1],
            [0x48,0x0f,0xaf,0xc1],[0x66,0x0f,0xba,0xe8,1],[0x48,0x0f,0xab,0xc1],
            [0xf3,0x66,0x0f,0xb8,0xc1],[0xf3,0x48,0x0f,0xbc,0xc1],[0xf3,0x48,0x0f,0xbd,0xc1],
            [0xf2,0x66,0x0f,0x38,0xf1,0xc1],[0xf2,0x48,0x0f,0x38,0xf0,0xc4],
            [0x9e],[0x9f],[0x66,0x98],[0x48,0x99],[0x48,0x0f,0xc8],[0x40,0x0f,0x95,0xc4],
            [0x66,0x8c,0xc0] }) yield return new("scalar extended",bytes,true);
        foreach (byte[] bytes in new byte[][] {
            [0x66,0x50],[0x41,0x57],[0x66,0x5b],[0x66,0x6a,1],[0xc2,8,0], [0xc3],
            [0xe8,0,0,0,0],[0xe9,0,0,0,0],[0xeb,0],[0x75,0],[0x0f,0x85,0,0,0,0],
            [0x67,0xe2,0],[0x67,0xe3,0],[0x66,0x9c],[0x66,0x9d],[0xc8,0,0,0],[0xc9],
            [0x64,0x67,0x66,0xff,0x33],[0x65,0x67,0x66,0x8f,0x03],
            [0x64,0x67,0x48,0xff,0x13],[0x65,0x67,0xff,0x23] }) yield return new("stack/control",bytes);
        foreach (byte op in new byte[] {0xa4,0xa5,0xa6,0xa7,0xaa,0xab,0xac,0xad,0xae,0xaf,0xd7})
            yield return new("ordinary string/xlat",[0x64,0x67,0x66,op]);
        foreach (byte op in new byte[] {0x60,0x61,0x62,0x63,0x64,0x65,0x66,0x67,0x68,0x69,0x6a,0x6b,0x74,0x75,0x76,0x6f,0x7f,0xdb,0xdf,0xeb,0xef,0xfc,0xfd,0xfe,0xd4,0xf8,0xf9,0xfa,0xfb})
        foreach (byte[] mandatory in new byte[][] {[],[0x66]})
            yield return new("MMX/XMM integer",[..mandatory,0x0f,op,0xc1],true);
        foreach (byte op in new byte[] {0x10,0x11,0x28,0x29,0x54,0x55,0x56,0x57,0x58,0x59,0x5a,0x5c,0x5d,0x5e,0x5f,0x51})
        foreach (byte[] mandatory in new byte[][] {[],[0x66]})
            yield return new("XMM move/FP",[..mandatory,0x0f,op,0xc1],true);
        foreach (byte op in new byte[] {0x10,0x11,0x58,0x59,0x5a,0x5c,0x5d,0x5e,0x5f,0x51,0x2a,0x2c,0x2d})
        foreach (byte mandatory in new byte[] {0xf2,0xf3})
            yield return new("scalar FP",[mandatory,0x0f,op,0xc1],true);
        foreach (byte op in new byte[] {0x6e,0x7e,0xd7,0xc4,0xc5,0x70})
        foreach (byte[] mandatory in new byte[][] {[],[0x66]})
            yield return new("MMX/XMM roles",op is 0xc4 or 0xc5 or 0x70 ? [..mandatory,0x0f,op,0xc1,1] : [..mandatory,0x0f,op,0xc1],true);
        foreach (byte op in new byte[] {0x00,0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,0x0a,0x0b,0x1c,0x1d,0x1e})
        foreach (byte[] mandatory in new byte[][] {[],[0x66]})
            yield return new("SSSE3 MMX/XMM",[..mandatory,0x0f,0x38,op,0xc1],true);
        foreach (byte[] bytes in new byte[][] {
            [0x66,0x0f,0x38,0x40,0xc1],[0x66,0x0f,0x3a,0x44,0xc1,1],
            [0x0f,0x3a,0x0f,0xc1,1],[0x66,0x0f,0x3a,0x0f,0xc1,1],
            [0xf2,0x0f,0xd6,0xc1],[0xf3,0x0f,0xd6,0xc1],
            [0x66,0x0f,0x71,0xd1,1],[0x0f,0x71,0xd1,1],[0x66,0x0f,0x73,0xf9,1],
            [0x66,0x0f,0x7c,0xc1],[0xf2,0x0f,0xd0,0xc1],[0xf3,0x0f,0x53,0xc1] })
            yield return new("extended vector/FP",bytes,true);
        yield return new("masked store implicit address",[0x64,0x67,0x66,0x0f,0xf7,0xc1]);
        yield return new("masked store MMX",[0x65,0x67,0x0f,0xf7,0xc1]);
        foreach (byte[] bytes in new byte[][] {
            [0xd9,0xc0],[0xd9,0xe8],[0xd9,0xe0],[0xdd,0xd0],[0xdb,0xe2],[0xdf,0xe0],
            [0x64,0x67,0xd9,0x03],[0x65,0x67,0xdd,0x1b],[0x66,0x64,0x67,0xd9,0x33],
            [0x64,0x67,0xd8,0x03],[0x65,0x67,0xdf,0x23],[0x65,0x67,0x48,0x0f,0xae,0x03] })
            yield return new("canonical x87/state",bytes);
    }

    static bool R159Prefix(byte b) => b is 0x66 or 0x67 or 0xf0 or 0xf2 or 0xf3 or 0x26 or 0x2e or 0x36 or 0x3e or 0x64 or 0x65 or >= 0x40 and <= 0x4f;
    static IEnumerable<byte[]> R159Permute(byte[] bytes)
    {
        if (bytes.Length == 0) { yield return []; yield break; }
        for (int k = 0; k < bytes.Length; ++k)
        foreach (byte[] tail in R159Permute(bytes.Where((_,n) => n != k).ToArray())) yield return [bytes[k],..tail];
    }
    static IEnumerable<byte[]> R159Variants(R159Form form)
    {
        int n = 0; while (n < form.Bytes.Length && R159Prefix(form.Bytes[n])) ++n;
        byte[] prefix = form.Bytes[..n], body = form.Bytes[n..];
        byte rex = prefix.LastOrDefault(b => b is >= 0x40 and <= 0x4f);
        byte[] legacy = prefix.Where(b => b is < 0x40 or > 0x4f).ToArray();
        foreach (byte[] order in R159Permute(legacy))
        {
            byte[] r = rex == 0 ? [] : [rex];
            yield return [..order,..r,..body];
            yield return [..order.SelectMany(b => new byte[] {b,b}),..r,..body];
            if (order.Length != 0) yield return [0x4f,..order,..r,..body];
            if (rex != 0) yield return [..order,0x4f,0x40,rex,..body];
        }
        if (form.RegisterOnly) yield return [0x65,0x67,..form.Bytes];
    }

    static void R159Decode(Blink program, byte[] bytes)
    {
        Blink.XedDecodedInst decoded = default;
        fixed (byte* p = bytes)
            Check(program.DecodeInstruction(&decoded,p,(ulong)bytes.Length,Blink.XED_MODE_LONG) == 0 && decoded.length == bytes.Length,
                $"R159 authoritative original decode {Convert.ToHexString(bytes)}");
    }
    static JitInstruction R159Classify(ulong ip, byte[] bytes)
    {
        Check(FullTranslationJit.R159Classify(ip,bytes,out var i) && FullBlockEmitter.CanInline(i),
            $"R159 existing native IR {Convert.ToHexString(bytes)}");
        Check(i.Ip == ip && i.Length == bytes.Length, "R159 published instruction metadata uses original bytes");
        return i;
    }
    static void CheckR159FamilyGrammar(Blink program)
    {
        foreach (var form in R159Forms())
        {
            R159Decode(program,form.Bytes);
            Check(FullTranslationJit.R159Core(Ip,form.Bytes,out var baseline), $"R159 established {form.Family} baseline {Convert.ToHexString(form.Bytes)}");
            foreach (byte[] bytes in R159Variants(form).Where(b => b.Length <= 15))
            {
                R159Decode(program,bytes);
                var actual = R159Classify(Ip,bytes);
                // Relative targets change by exactly the number of added bytes.
                ulong target = baseline.Target == 0 ? 0 : unchecked(baseline.Target + (uint)(bytes.Length - form.Bytes.Length));
                Check(actual == (baseline with { Length = bytes.Length, Target = target }),
                    $"R159 family {form.Family} permutation/redundancy keeps all operand roles {Convert.ToHexString(bytes)}");
            }
        }
    }

    static void CheckR159RepGrammar(Blink program)
    {
        foreach (byte opcode in new byte[] {0xa4,0xa5,0xa6,0xa7,0xaa,0xab,0xac,0xad,0xae,0xaf})
        foreach (byte repeat in new byte[] {0xf2,0xf3})
        foreach (byte[] width in new byte[][] {[],[0x66],[0x48],[0x66,0x48]})
        foreach (byte segment in new byte[] {0x64,0x65})
        {
            bool compares = opcode is 0xa6 or 0xa7 or 0xae or 0xaf;
            var form = new R159Form("REP",[repeat,segment,0x67,..width,opcode]);
            Check(FullRepLowering.TryClassify(Ip,form.Bytes,out var baseline,out _) == (repeat == 0xf3 || compares),
                "R159 base REP classifier keeps its original F2 non-comparison limit");
            if (repeat == 0xf2 && !compares) continue;
            foreach (byte[] bytes in R159Variants(form).Where(b=>b.Length<=15))
            {
                R159Decode(program,bytes);
                Check(FullCanonicalEncoding.TryClassifyRep(Ip,bytes,out var actual,out var why), why ?? "R159 canonical REP");
                Check(actual == (baseline with { Length = bytes.Length }) && FullRepLowering.IsCanonical(in actual),
                    "R159 REP projection retains repeat mode, width, address size, source segment and original identity");
            }
        }
        Check(!FullCanonicalEncoding.TryClassifyRep(Ip,[0x66,0x66,0xf2,0xa4],out _,out var excludedReason) && excludedReason != null,
            "R159 wrapper preserves F2 noncompare exclusion");
        Check(FullCanonicalEncoding.TryClassifyRep(Ip,[0x66,0x66,0xf3,0x36,0xa4],out var stackSource,out _) &&
            stackSource.SourceSegment==2 && stackSource.Width==8 && stackSource.Length==5 &&
            stackSource.Repeat==FullRepRepeat.Count && FullRepLowering.IsCanonical(in stackSource),
            "R159 normalization preserves R162 source-SS support and exact original span");
        Check(!FullRepLowering.TryClassify(Ip,[0x66,0x66,0xf3,0xa4],out _,out _),
            "R159 base-only duplicate-prefix rejection remains unchanged");
    }

    static void CheckR159RegisterOracle(R159Fixture oracle)
    {
        List<byte[]> rows = [ [0x66,0x88,0xe0], [0x66,0x40,0x88,0xe0], [0x66,0x89,0xc1],
            [0x66,0x0f,0xb6,0xc4], [0x66,0x40,0x0f,0xb6,0xc4], [0x66,0x01,0xc1],
            [0xf3,0x66,0x0f,0xb8,0xc1], [0x66,0x0f,0xef,0xc1], [0x0f,0xef,0xc1],
            [0xf3,0x0f,0x58,0xc1], [0x66,0x0f,0x6e,0xc1], [0x66,0x0f,0xc4,0xc1,1] ];
        for (int rex = 0x40; rex <= 0x4f; ++rex)
        {
            rows.Add([(byte)rex,0x88,0xe0]);
            rows.Add([0x66,(byte)rex,0x0f,0xb6,0xc4]);
            rows.Add([0x66,(byte)rex,0x0f,0xef,0xc1]);
            rows.Add([(byte)rex,0x0f,0x6f,0xc1]);
            rows.Add([0xf3,(byte)rex,0x0f,0x58,0xc1]);
        }
        byte* machine = (byte*)oracle.Machine();
        foreach (byte[] row in rows)
        foreach (byte[] bytes in R159Variants(new("oracle",row,true)).Where(b => b.Length <= 15))
        {
            R159Decode(oracle.Program,bytes); var i = R159Classify(Ip,bytes);
            MemoryIntegerSeed(machine); ((Blink.Machine*)machine)->mxcsr = 0x1f80;
            ((Blink.Machine*)machine)->flags = 0x01000881; // canonical source POPCNT undefined-flag representation
            for (int r = 0; r < 16; ++r)
            for (int lane = 0; lane < 4; ++lane) FloatWrite(machine,r,lane,32,0x3f800000u+(uint)(r*0x10000+lane*0x100));
            byte[] before = new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            oracle.DataScope(0,() => IntegerReference(oracle.Program,(byte*)oracle.Machine(),bytes));
            byte[] expected = new ReadOnlySpan<byte>(machine,sizeof(Blink.Machine)).ToArray();
            uint expectedMxcsr = ((Blink.Machine*)machine)->mxcsr;
            before.CopyTo(new Span<byte>(machine,before.Length));
            using var code = CreateFullBlock([new(i)]);
            ulong epoch = 11; long issued = 0, completed = 0; int stop = 0;
            var h = FullHeader(Ip,code.Entry); var f = FullFrame(machine,&h,&epoch,&issued,&completed,&stop,limit:1);
            Check(RunFull(code,&f) == FullBlockExit.Continue && issued == 1 && completed == 1 && f.Executed == 1,
                "R159 actual emitted body completes exactly one original instruction");
            Check(new ReadOnlySpan<byte>(machine+24,128).SequenceEqual(expected.AsSpan(24,128)) &&
                new ReadOnlySpan<byte>(machine+160,256).SequenceEqual(expected.AsSpan(160,256)) &&
                ((Blink.Machine*)machine)->flags == BinaryPrimitives.ReadUInt32LittleEndian(expected.AsSpan(12,4)) &&
                ((Blink.Machine*)machine)->mxcsr == expectedMxcsr &&
                ((Blink.Machine*)machine)->ip == Ip+(uint)bytes.Length && ((Blink.Machine*)machine)->oplen == 0,
                "R159 selected GetOp(original bytes) versus existing native computation, all GPR/XMM/flags/MXCSR/IP");
        }
    }

    static void CheckR159RelativeAndEa(byte* machine)
    {
        foreach (ulong ip in new ulong[] {Ip,0xfffffff8,ulong.MaxValue-3})
        foreach (byte[] bytes in new byte[][] { [0x65,0x67,0x4f,0xe8,1,0,0,0],
            [0x65,0x67,0x4f,0xe9,0xff,0xff,0xff,0xff], [0x66,0x65,0x67,0x4f,0xeb,0xfe],
            [0x65,0x67,0x4f,0x0f,0x85,1,0,0,0] })
        {
            var i = R159Classify(ip,bytes);
            long displacement = bytes[^1] == 0xfe ? -2 : bytes[^1] == 0xff ? -1 : 1;
            Check(i.Target == unchecked(ip+(uint)bytes.Length+(ulong)displacement), "R159 original next PC relative target wraps modulo64");
        }
        foreach (bool address32 in new[] {false,true})
        foreach (byte segment in new byte[] {0x36,0x64,0x65})
        foreach (ulong ip in new ulong[] {Ip,0xfffffff8,ulong.MaxValue-3})
        {
            byte[] bytes = address32 ? [0x66,0x66,0x67,segment,0x4b,0x8b,0x05,0xf0,0xff,0xff,0xff] :
                [0x66,0x66,segment,0x4b,0x8b,0x05,0xf0,0xff,0xff,0xff];
            var i = R159Classify(ip,bytes);
            Check(i.Address.RipRelative && i.Address.AddressWidth == (address32 ? 32 : 64) &&
                i.Address.Segment == (segment == 0x36 ? 2 : segment == 0x64 ? 4 : 5), "R159 preserves RIP/EIP and SS/FS/GS roles");
            ulong offset = unchecked(ip+(uint)bytes.Length-16);
            if (address32) offset = (uint)offset;
            Check(MemoryIntegerAddress(i,machine,0x1000,0x2000) == unchecked(offset+(segment == 0x64 ? 0x1000UL : segment == 0x65 ? 0x2000UL : 0)),
                "R159 original next PC then EA32 truncation then FS/GS base");
        }
    }

    static void CheckR159Polls(byte* machine)
    {
        byte[] bytes = [0x65,0x67,0x66,0x66,0xb8,7,0]; var i = R159Classify(Ip,bytes);
        using var code = CreateFullBlock([new(i)]);
        foreach (string poll in new[] {"normal","stop","attention","budget"})
        {
            ResetFull(machine); ((Blink.Machine*)machine)->oplen=0; Write64(machine,24,0x12340000);
            ((Blink.Machine*)machine)->attention = poll == "attention";
            ulong epoch = 11; long issued = 0, completed = 0; int stop = poll == "stop" ? 1 : 0;
            var h = FullHeader(Ip,code.Entry); var f = FullFrame(machine,&h,&epoch,&issued,&completed,&stop,budget:poll == "budget" ? 0UL : 100UL,limit:1);
            var expected = poll == "normal" ? FullBlockExit.Continue : poll == "stop" ? FullBlockExit.Stop : poll == "attention" ? FullBlockExit.Attention : FullBlockExit.Budget;
            Check(RunFull(code,&f) == expected && completed == (poll == "normal" ? 1 : 0) &&
                issued == (poll is "normal" or "budget" ? 1 : 0) &&
                ((Blink.Machine*)machine)->ip == (poll == "normal" ? Ip+(uint)bytes.Length : Ip) &&
                ((Blink.Machine*)machine)->oplen == 0 && ((ulong*)(machine+24))[0] == (poll == "normal" ? 0x12340007UL : 0x12340000UL),
                "R159 polls/budget preserve original start/length and one-instruction accounting");
        }
    }

    static void CheckR159Controller()
    {
        foreach (string kind in new[] {"byte","xmm","push","call","eip-gs","rip-fs","branch","loop"})
        {
            using var f = new R159Fixture(); const ulong pc = R159Fixture.Base, data = pc+8192;
            f.Map(pc,12288); f.Fill(pc,12288);
            byte[] bytes = kind switch
            {
                "byte" => [0x4f,0x66,0x66,0x67,0x65,0x88,0xe0],
                "xmm" => [0x67,0x66,0x66,0x4a,0x0f,0xef,0xc1],
                "push" => [0x67,0x3e,0x66,0x66,0x44,0xff,0x33],
                "call" => [0x67,0x3e,0x44,0xe8,8,0,0,0],
                "eip-gs" => [0x66,0x66,0x67,0x65,0x4b,0x8b,0x05,0,0,0,0],
                "rip-fs" => [0x66,0x66,0x64,0x4b,0x8b,0x05,0,0,0,0],
                "branch" => [0x66,0x67,0x65,0x4f,0xeb,0],
                _ => [0x66,0x65,0x67,0x4f,0xe2,0]
            };
            if (kind is "eip-gs" or "rip-fs") BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(bytes.Length-4),checked((int)(data-pc-(uint)bytes.Length-32)));
            WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            var m = f.Machine(); Write64((byte*)m,24,0x1234); Write64((byte*)m,32,0xffffffff00000002);
            Write64((byte*)m,48,data); Write64((byte*)m,56,data+1024);
            m->fs.@base = 32; m->gs.@base = 32;
            f.DataScope(0,() => { byte* at = f.Program.LookupAddress2(f.Machine(),(long)data,0,0); ((ulong*)at)[0] = 0x123456789abcdef0; });
            for (int b = 0; b < 16; ++b) { ((byte*)m)[160+b] = 0x55; ((byte*)m)[176+b] = 0x0f; }
            Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap;
            try
            {
                f.Counts(100); Check(f.Step(pc),"R159 real controller starts");
                for (int n = 0; f.Completed < 1 && n < 16; ++n) Check(f.Continue(),"R159 bounded same-issue mapping continuation");
                ulong expectedPc = pc+(uint)bytes.Length+(kind == "call" ? 8UL : 0);
                Check(f.Issued == 1 && f.Completed == 1 && m->ip == expectedPc && m->oplen == 0 &&
                    f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0 && f.Statistics.NativeEntries > 0,
                    $"R159 actual {kind} compiler/controller uses original completion boundary without fallback");
                if (kind == "byte") Check(((ulong*)((byte*)m+24))[0] == 0x1212,"R159 AH source survives canceled REX");
                if (kind == "xmm") Check(new ReadOnlySpan<byte>((byte*)m+160,16).ToArray().All(b=>b==0x5a),"R159 ignored W/X SIMD executes PXOR");
                if (kind is "eip-gs" or "rip-fs") Check(((ulong*)((byte*)m+24))[0] == 0x123456789abcdef0,"R159 RIP/EIP original next PC and segment base load");
                if (kind == "loop") Check(((ulong*)((byte*)m+32))[0] == 1,"R159 LOOP67 decrements and zero-extends ECX");
                if (kind is "call" or "push")
                {
                    ulong address = data+1024-(kind == "call" ? 8UL : 2UL);
                    Check(((ulong*)((byte*)m+56))[0] == address,"R159 original stack width and RSP");
                    byte* page = (byte*)f.Physical(data);
                    Check(kind == "call" ? ((ulong*)(page+address-data))[0] == pc+(uint)bytes.Length : ((ushort*)(page+address-data))[0] == 0xdef0,
                        "R159 original CALL return PC or push16 source stored by actual native body");
                }
            }
            finally { f.NativePrepared = null; f.NativeReturned = null; }
            var s = f.Jit.Store.GetStatistics();
            Check(s.ActiveNativeEpochs == 0 && s.ActiveManagedLeases == 0 && f.Statistics.ActiveDataBorrows == 0,"R159 controller leaves no capabilities");
        }
    }

    static void CheckR159FaultController()
    {
        using var f = new R159Fixture(); const ulong pc = R159Fixture.Base;
        f.Map(pc); f.Fill(pc);
        byte[] bytes = [0x66,0x66,0x67,0x48,0xf7,0xf3];
        WriteControllerCode(f,pc,[0x90,..bytes]);
        var m = f.Machine(); Write64((byte*)m,24,17); Write64((byte*)m,40,0); Write64((byte*)m,48,0);
        uint flags = m->flags; f.Counts(100);
        var signal = f.StepSynchronousFault(pc);
        Check(signal.Signal == 8 && m->trapno == 0 && m->ip == pc+1 && m->oplen == 0 && m->faultaddr == (long)(pc+1) &&
            ((ulong*)((byte*)m+24))[0] == 17 && m->flags == flags && f.Issued == 2 && f.Completed == 1 &&
            f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0,
            "R159 real divide fault reports original prefix start and exactly one uncompleted original instruction");
    }

    static void CheckR159X87Controller()
    {
        // Existing test-only fresh owner activation; production gate untouched.
        using var f = new R159Fixture(); const ulong pc = R159Fixture.Base;
        f.Map(pc); f.Fill(pc); byte[] bytes = [0x66,0x66,0x67,0x65,0x4f,0xd9,0xe8];
        WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]); f.StartCanonicalFamily();
        try
        {
            var m = f.Machine(); m->fpu.cw = 0x37f; m->fpu.sw = 0; m->fpu.tw = 0xffff;
            Action<nint> cap = p => ((FullBlockFrame*)p)->NativeLimit = 1;
            f.NativePrepared = cap; f.NativeReturned = _ => f.NativePrepared = cap; f.Counts(100);
            Check(f.Step(pc),"R159 x87 actual controller entry");
            for (int n = 0; f.Completed < 1 && n < 8; ++n) Check(f.Continue(),"R159 bounded x87 native continuation");
            var value = FullX87RegisterFile.Physical(f.CanonicalFile(),7);
            Check(f.Issued == 1 && f.Completed == 1 && m->ip == pc+(uint)bytes.Length && m->oplen == 0 &&
                value->Significand == 0x8000000000000000 && value->SignExponent == 0x3fff &&
                f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0,
                "R159 canonical raw80 native FLD1 preserves original prefix completion metadata");
        }
        finally { f.NativePrepared = null; f.NativeReturned = null; f.EndCanonicalFamily(); }
    }

    static void CheckR159RepController()
    {
        // Nonzero iterations force CaptureIssuedRep and returned mapping
        // authentication, rather than testing only the count-zero fast path.
        foreach (byte repeat in new byte[] {0xf2,0xf3})
        {
            using var f = new R159Fixture(); const ulong pc = R159Fixture.Base, data = pc+8192;
            f.Map(pc,12288); f.Fill(pc,12288);
            byte[] bytes = [0x4f,0x67,0x67,0x66,0x66,repeat,repeat,0xa6];
            WriteControllerCode(f,pc,[..bytes,0xeb,0xfe]);
            f.DataScope(0,() =>
            {
                byte* p = f.Program.LookupAddress2(f.Machine(),(long)data,0,0);
                for (int k = 0; k < 16; ++k) p[k] = (byte)k;
            });
            RepSeed(f,data,data+8,0,1,f.Machine()->flags & ~0x10500u);
            f.Counts(1);
            Check(f.Step(pc,signalActor:true),"R159 actual REP compiler/latch/controller returns");
            for (int n = 0; f.Completed < 1 && n < 16; ++n) Check(f.Continue(signalActor:true),"R159 bounded REP same-issue continuation");
            Check(f.Issued == 1 && f.Completed == 1 && f.Machine()->ip == pc+(uint)bytes.Length &&
                f.Machine()->oplen == 0 && RepRegister(f,32) == 0 &&
                f.Statistics.SemanticHelperInstructions == 0 && f.Statistics.InterpreterAttempts == 0,
                "R159 original bytes survive all REP binding, CaptureIssuedRep and continuation identity checks");
            RepClean(f);
            lock (f.Jit.Authority.Gate) f.Jit.RepKernels.AssertQuiescent();
        }
    }
}
