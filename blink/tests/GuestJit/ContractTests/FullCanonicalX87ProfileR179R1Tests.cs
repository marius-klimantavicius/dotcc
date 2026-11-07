using System.Buffers.Binary;
using Managed.Emulation.Execution;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal sealed partial class FullControllerFixture
        {
            internal void AlignmentMask(bool enabled)
            {
                var system = Machine()->blink_host_system;
                if (enabled) system->cr0 |= FullCanonicalX87ExecutionProfile.AlignmentMask;
                else system->cr0 &= ~(ulong)FullCanonicalX87ExecutionProfile.AlignmentMask;
            }
            internal void InvokeNonStateHelper(byte[] bytes)
            {
                Blink.XedDecodedInst decoded = default;
                fixed (byte* p = bytes)
                    Require(Program.DecodeInstruction(&decoded, p, (ulong)bytes.Length, Blink.XED_MODE_LONG) == 0,
                        "R179r1 exact helper closure decode");
                Machine()->ip = Base + decoded.length; Machine()->oplen = decoded.length;
                Owner.ExecuteFullHelper(testWorkers[0], new(Base, decoded,
                    Program.BlinkFullJitSelect(decoded.op.rde, decoded.op.disp, decoded.op.uimm0)));
            }
        }
    }
}

unsafe partial class Program
{
    static void R179R1Seed(ThreadedGuestExecution.FullControllerFixture fixture, ushort cw, uint flags = 0x202)
    {
        var m = fixture.Machine(); var f = fixture.CanonicalFile();
        m->flags = flags; m->fpu.cw = cw; m->fpu.sw = 0; m->fpu.tw = 0xfff0;
        m->fpu.op = 0; m->fpu.ip = 0; m->fpu.dp = 0;
        for (int r = 0; r < 8; ++r) *FullX87RegisterFile.Physical(f, r) = default;
        *FullX87RegisterFile.Physical(f, 0) = new(0x8000000000000000, 0x3fff);
        *FullX87RegisterFile.Physical(f, 1) = new(0x8000000000000000, 0x4000);
        fixture.Counts(100);
    }

    static void R179R1One(ThreadedGuestExecution.FullControllerFixture fixture, ulong pc)
    {
        Action<nint> cap = pointer => ((FullBlockFrame*)pointer)->NativeLimit = 1;
        fixture.NativePrepared = cap; fixture.NativeReturned = _ => fixture.NativePrepared = cap;
        Check(fixture.Step(pc), "R179r1 real native controller entry");
        for (int retry = 0; fixture.Completed < 1 && retry < 16; ++retry)
            Check(fixture.Continue(), "R179r1 bounded same-issue map resume");
        Check(fixture.Issued == 1 && fixture.Completed == 1 && fixture.Statistics.InterpreterAttempts == 0,
            "R179r1 one native computation, no reissue or interpreter");
        fixture.NativePrepared = null; fixture.NativeReturned = null;
    }

    static void CheckCanonicalX87PC01Profile()
    {
        // A separate independent literal precision table; no guest CW rewrite.
        foreach (int pc in new[] { 0, 1, 2, 3 })
            Check(FullCanonicalX87PrecisionProfile.ArithmeticBits((ushort)(pc << 8)) == new[] { 24, 64, 53, 64 }[pc],
                "R179r1 explicit reserved-PC software precision partition");
        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        const ulong pcBase = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pcBase); fixture.Fill(pcBase);
        byte[][] rows = [[0xd8,0xc1],[0xd8,0xc9],[0xd8,0xe1],[0xd8,0xe9],[0xd8,0xf1],[0xd8,0xf9],[0xd9,0xfa]];
        for (int row = 0; row < rows.Length; ++row)
            WriteControllerCode(fixture, pcBase + (uint)(row * 16), [..rows[row], 0xeb, 0xfe]);
        fixture.StartCanonicalFamily();
        try
        {
            foreach (int row in Enumerable.Range(0, rows.Length))
            foreach (int rc in new[] { 0,1,2,3 })
            foreach (int mask in Enumerable.Range(0,64))
            {
                byte[]? raw64 = null; uint sw64 = 0; int tw64 = 0;
                // Execute PC11 first, then PC01 with the same exact inputs.
                foreach (int pc in new[] { 3, 1 })
                {
                    ushort cw = (ushort)(0x40 | pc << 8 | rc << 10 | mask);
                    R179R1Seed(fixture, cw);
                    // Half-ulp addition and irrational sqrt distinguish rounding;
                    // other operators retain the all-PC exact-rational grids.
                    if (row == 0) *FullX87RegisterFile.Physical(fixture.CanonicalFile(), 1) = new(0x8000000000000000, 0x3fbf);
                    if (row == 6) *FullX87RegisterFile.Physical(fixture.CanonicalFile(), 0) = new(0x8000000000000000, 0x4000);
                    R179R1One(fixture, pcBase + (uint)(row * 16));
                    var m = fixture.Machine(); byte* work = (byte*)fixture.CanonicalFile() + 192;
                    Check(m->fpu.cw == cw && *(ushort*)(work + 8) == cw && *(uint*)(work + 12) == 0,
                        "R179r1 raw CW01 remains exact in Machine/Work, never silent Admission3");
                    var raw = new ReadOnlySpan<byte>(fixture.CanonicalFile(), 192).ToArray();
                    if (pc == 3) { raw64 = raw; sw64 = m->fpu.sw; tw64 = m->fpu.tw; }
                    else Check(raw.AsSpan().SequenceEqual(raw64) && m->fpu.sw == sw64 && m->fpu.tw == tw64,
                        "R179r1 actual PC01 equals PC64 raw80/status/tags for all RC and all64 masks");
                }
            }
            // Copy/validate/restore paths preserve the literal CW, independently
            // of the numerical precision decoder and its mask/RC decisions.
#if BLINK_FULL_X87_STATE_OWNER
            foreach (HostX87Transfer save in new[] { HostX87Transfer.Fx64Save, HostX87Transfer.Env14Save, HostX87Transfer.Env28Save, HostX87Transfer.LinuxSave })
            {
                R179R1Seed(fixture, 0x17f);
                byte[] image = new byte[FullX87StateBridge.ImageSize(save)];
                var owner = (IHostGuestX87State)fixture.Owner;
                Check(owner.TransferX87State((nint)fixture.Machine(),1,save,image) == HostX87TransferResult.Complete &&
                    BinaryPrimitives.ReadUInt16LittleEndian(image) == 0x17f && fixture.Machine()->fpu.cw == 0x17f,
                    "R179r1 canonical state/transport preserves raw CW01 without normalization");
                var restore = save switch { HostX87Transfer.Fx64Save => HostX87Transfer.Fx64Restore,
                    HostX87Transfer.Env14Save => HostX87Transfer.Env14Restore,
                    HostX87Transfer.Env28Save => HostX87Transfer.Env28Restore, _ => HostX87Transfer.LinuxRestore };
                fixture.Machine()->fpu.cw=0x37f;
                Check(owner.TransferX87State((nint)fixture.Machine(),1,restore,image)==HostX87TransferResult.Complete &&
                    fixture.Machine()->fpu.cw==0x17f,"R179r1 every paired bridge restore preserves raw PC01");
            }
#endif
            Check(fixture.Statistics.SemanticHelperInstructions == 0, "R179r1 arithmetic/sqrt/copy matrix has no managed computation fallback");
            RecordQualificationController(fixture.Statistics);
        }
        finally { fixture.NativePrepared = null; fixture.NativeReturned = null; fixture.EndCanonicalFamily(); }
    }

    static void CheckCanonicalX87SelectedContexts()
    {
        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base, data = pc + 8192;
        fixture.Map(pc); fixture.Fill(pc); fixture.Map(data); fixture.Fill(data);
        byte[][] rows = [[0xd9,0xe8],[0xd8,0xc1],[0xd8,0x03],[0x0f,0xef,0xc0],[0xb8,0x7b,0,0,0],
            [0xf2,0xf3,0xd8,0xc1],[0x64,0x65,0xf3,0xf3,0xd8,0x03]];
        for (int row=0;row<rows.Length;++row) WriteControllerCode(fixture,pc+(uint)(row*16),[..rows[row],0xeb,0xfe]);
        fixture.DataScope(0, () =>
        {
            byte* p = fixture.Program.LookupAddress2(fixture.Machine(),(long)data,0,0);
            p[1]=0;p[2]=0;p[3]=0;p[4]=0x40; // deliberately unaligned binary32 two
        });
        fixture.StartCanonicalFamily();
        try
        {
            foreach (int row in Enumerable.Range(0,rows.Length))
            foreach (int flags in Enumerable.Range(0,8))
            {
                uint selected = 0x202u | ((flags&1)!=0?1u<<Blink.FLAGS_TF:0) |
                    ((flags&2)!=0?1u<<Blink.FLAGS_RF:0) | ((flags&4)!=0?1u<<Blink.FLAGS_AC:0);
                R179R1Seed(fixture,0x17f,selected); fixture.AlignmentMask(false);
                Write64((byte*)fixture.Machine(),48,data+1);
                byte[] xmm = new ReadOnlySpan<byte>((byte*)fixture.Machine()+160,256).ToArray();
                R179R1One(fixture,pc+(uint)(row*16));
                Check(fixture.Machine()->ip==pc+(uint)(row*16+rows[row].Length) &&
                    fixture.Machine()->flags == selected && fixture.Statistics.SemanticHelperInstructions == 0 &&
                    new ReadOnlySpan<byte>((byte*)fixture.Machine()+160,256).SequenceEqual(xmm),
                    "R179r1 native12 TF/RF retained, AC with AM0 admitted including unaligned x87, MMX raw alias and scalar");
            }
            R179R1Seed(fixture,0x17f,0x202|(1u<<Blink.FLAGS_AC));
            ulong withoutAm = FullTranslationJit.Context(fixture.Program,fixture.Machine());
            fixture.AlignmentMask(true);
            ulong withAm = FullTranslationJit.Context(fixture.Program,fixture.Machine());
            Check((withoutAm^withAm)==FullCanonicalX87ExecutionProfile.ContextAm,
                "R179r1 retained context independently keys CR0.AM eligibility");
            byte[] raw = new ReadOnlySpan<byte>(fixture.CanonicalFile(),192).ToArray();
            bool outside = false;
            try { fixture.Step(pc+32); }
            catch (NotSupportedException) { outside=true; }
            Check(outside && fixture.Completed==0 && fixture.Statistics.SemanticHelperInstructions==0 &&
                new ReadOnlySpan<byte>(fixture.CanonicalFile(),192).SequenceEqual(raw),
                "R179r1 AM1/AC1 is an explicit outside-profile context, never converted to UD or managed x87");
            // Reserved encodings remain UD before data/alignment eligibility.
            WriteControllerCode(fixture,pc+384,[0xd9,0xe2]);fixture.Counts(100);fixture.ResetFaultObservation();
            var ud=fixture.StepSynchronousFault(pc+384);
            Check(ud.Signal==4 && fixture.Completed==0 && fixture.Machine()->ip==pc+384,
                "R179r1 reserved UD outranks eligible AC, not a conversion of a legal instruction into UD");
            R179R1Seed(fixture,0x17f); // AC clear while AM remains set
            R179R1One(fixture,pc+16);
            Check(fixture.Completed==1,"R179r1 AM alone is not AC eligibility");
            fixture.AlignmentMask(false);
            // Real selected POPF imports TF/AC but clears RF; ordinary native
            // x87 must not clear it merely by executing an instruction.
            WriteControllerCode(fixture,pc+256,[0x9d,0xeb,0xfe]);
            R179R1Seed(fixture,0x17f,0x202|(1u<<Blink.FLAGS_RF));
            fixture.DataScope(0,()=> {
                byte* p=fixture.Program.LookupAddress2(fixture.Machine(),(long)(data+128),0,0);
                BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(p,8),0x202UL|(1UL<<Blink.FLAGS_TF)|(1UL<<Blink.FLAGS_RF)|(1UL<<Blink.FLAGS_AC));
            });
            Write64((byte*)fixture.Machine(),56,data+128);
            R179R1One(fixture,pc+256);
            Check((fixture.Machine()->flags&(1u<<Blink.FLAGS_RF))==0 &&
                (fixture.Machine()->flags&((1u<<Blink.FLAGS_TF)|(1u<<Blink.FLAGS_AC)))==((1u<<Blink.FLAGS_TF)|(1u<<Blink.FLAGS_AC)),
                "R179r1 native flags-import boundary follows exact selected TF/AC/RF policy");
            RecordQualificationController(fixture.Statistics);
        }
        finally { fixture.AlignmentMask(false); fixture.NativePrepared=null;fixture.NativeReturned=null;fixture.EndCanonicalFamily(); }
    }

    static void CheckCanonicalX87HelperClosure()
    {
        using var fixture = new ThreadedGuestExecution.FullControllerFixture();
        const ulong pc = ThreadedGuestExecution.FullControllerFixture.Base;
        fixture.Map(pc);fixture.Fill(pc);fixture.StartCanonicalFamily();
        try
        {
            (byte[] Bytes, FullCanonicalX87HelperKind Expected)[] policyRows = [
                ([0x90],FullCanonicalX87HelperKind.NonState),([0x0f,0x0b],FullCanonicalX87HelperKind.NonState),
                ([0x0f,0x05],FullCanonicalX87HelperKind.SyscallBoundary),
                ([0xd8,0xc1],FullCanonicalX87HelperKind.NativeOnlyState),([0x9b],FullCanonicalX87HelperKind.NativeOnlyState),
                ([0x0f,0x2a,0xc0],FullCanonicalX87HelperKind.NativeOnlyState),
                ([0xf3,0x0f,0x2a,0xc0],FullCanonicalX87HelperKind.NonState),
                ([0x66,0x0f,0x71,0xd0,1],FullCanonicalX87HelperKind.NativeOnlyState),
                ([0x48,0x0f,0xae,0x03],FullCanonicalX87HelperKind.NativeOnlyState),
                ([0x0f,0xae,0x13],FullCanonicalX87HelperKind.NonState)
            ];
            foreach(var row in policyRows)
            {
                Blink.XedDecodedInst decoded=default;
                fixed(byte* bytes=row.Bytes) Check(fixture.Program.DecodeInstruction(&decoded,bytes,(ulong)row.Bytes.Length,Blink.XED_MODE_LONG)==0,
                    "R179r1 helper policy exact selected decode");
                Check(FullCanonicalX87HelperPolicy.Classify(decoded.op.rde)==row.Expected,
                    "R179r1 selector-specific helper closure distinguishes MMX, scalar SSE, MXCSR, syscall and fault boundaries");
            }
            foreach (byte[] bytes in new byte[][] { [0xd8,0xc1],[0x9b],[0x0f,0xef,0xc0],[0x66,0x0f,0x71,0xd0,1],
                [0x0f,0x2a,0xc0],[0x48,0x0f,0xae,0x03] })
            {
                bool denied=false;
                try { fixture.TryLegacyX87Helper(bytes); }
                catch (InvalidOperationException error) { denied=error.Message.Contains("managed computation helper"); }
                Check(denied && fixture.Statistics.SemanticHelperInstructions==0,
                    "R179r1 exact native12 x87/MMX/shared-guard SSE closure cannot call managed computation");
            }
            R179R1Seed(fixture,0x17f); byte[] raw=new ReadOnlySpan<byte>(fixture.CanonicalFile(),192).ToArray();
            fixture.InvokeNonStateHelper([0x90]);
            Check(fixture.Completed==1 && new ReadOnlySpan<byte>(fixture.CanonicalFile(),192).SequenceEqual(raw),
                "R179r1 unrelated selected scalar helper no longer trips broad canonical legacy guard");
            // A real named fault boundary remains usable between canonical ops.
            WriteControllerCode(fixture,pc,[0x0f,0x0b]);fixture.Counts(100);fixture.ResetFaultObservation();
            var fault=fixture.StepSynchronousFault(pc);
            Check(fault.Signal==4 && fixture.Completed==0 && new ReadOnlySpan<byte>(fixture.CanonicalFile(),192).SequenceEqual(raw),
                "R179r1 selected UD2 uses real guest SIGILL boundary without losing raw80 ownership");
            // Linux syscall transport is an authorized boundary, not x87 compute.
            WriteControllerCode(fixture,pc+16,[0x0f,0x05,0xeb,0xfe]);
            R179R1Seed(fixture,0x17f);Write64((byte*)fixture.Machine(),24,39); // getpid
            byte[] syscallRaw=new ReadOnlySpan<byte>(fixture.CanonicalFile(),192).ToArray();
            fixture.NativePrepared=pointer=>((FullBlockFrame*)pointer)->NativeLimit=1;
            Check(fixture.Step(pc+16,signalActor:true) && fixture.Completed==1 && fixture.CanonicalFile()->ProfileActive==1 &&
                fixture.Machine()->fpu.cw==0x17f && new ReadOnlySpan<byte>(fixture.CanonicalFile(),192).SequenceEqual(syscallRaw),
                "R179r1 active canonical state crosses real getpid syscall boundary without raw80 loss");
        }
        finally { fixture.NativePrepared=null;fixture.NativeReturned=null;fixture.EndCanonicalFamily(); }
    }
}
