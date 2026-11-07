using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        // TEST ONLY profile seeding: no activation API, guest execution, emitter,
        // semantic fallback or f64 synchronization occurs in this test. It calls
        // the real freshly generated SigRestore and actual typed owner bridge.
        internal static void TestFullX87SignalTransport(Action<bool, string> check)
        {
#if BLINK_FULL_X87_STATE_OWNER
            check(sizeof(Blink.SignalFrame) == 952 && sizeof(Blink.ucontext_linux) == 304 &&
                Marshal.OffsetOf<Blink.SignalFrame>(nameof(Blink.SignalFrame.uc)).ToInt32() == 136 &&
                Marshal.OffsetOf<Blink.SignalFrame>(nameof(Blink.SignalFrame.fp)).ToInt32() == 440,
                "signal transport actual generated target layout");
            for (int witness = 0; witness < 6; ++witness)
            {
                using var f = new FullControllerFixture(workers: 1);
                var owner = f.Owner; var worker = owner.workers[0]; var machine = worker.Machine;
                f.At(FullControllerFixture.Base);
                var file = worker.FullNativeState!.X87Pointer;
                ulong frameBase = FullControllerFixture.Base + 0xe48;
                if (witness == 4) frameBase += 8; // required prefix crosses absent page
                ulong external = FullControllerFixture.Base + (witness == 5 ? 0x3f00UL : 0x3000UL);
                ulong fp = witness == 0 ? 0UL : witness == 2 ? FullControllerFixture.Base + 0x1000 : external;
                f.Map(FullControllerFixture.Base); f.Map(FullControllerFixture.Base + 0x3000);
                file->ProfileActive = 1; // isolated test transport only; restored to0 before cleanup
                try
                {
                    machine->fpu.cw = 0x37f; machine->fpu.sw = 3 << 11; machine->fpu.tw = 0;
                    machine->fpu.op = 0x5ab; machine->fpu.ip = 0x123456789abcdef;
                    machine->fpu.dp = 0x23456789abcdef0; machine->mxcsr = 0x1f80;
                    for (int r = 0; r < 8; ++r)
                    {
                        var slot = FullX87RegisterFile.Physical(file, r);
                        slot->Significand = 0x8000000000000001UL + (uint)r;
                        slot->SignExponent = (ushort)(0x3fff + r);
                    }
                    new Span<byte>((byte*)machine + 160, 256).Fill(0xa5);
                    var callbacks = (IHostGuestX87State)owner;
                    byte[] image = new byte[512];
                    check(callbacks.TransferX87State((nint)machine, 1, HostX87Transfer.LinuxSave, image) ==
                        HostX87TransferResult.Complete, "actual owner saves raw80 test image");
                    if (witness == 3) BinaryPrimitives.WriteUInt32LittleEndian(image.AsSpan(24), 0x80001f80);
                    Blink.SignalFrame staged = default;
                    BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(staged.uc.fpstate, 8), fp);
                    BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(staged.uc.rip, 8), 0x405678UL);
                    BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(staged.uc.rsp, 8), 0x407ff0UL);
                    BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(staged.uc.rax, 8), 0x1122334455667788UL);
                    BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>(staged.uc.eflags, 8), 0x202UL);
                    byte[] prefix = new ReadOnlySpan<byte>(&staged, 440).ToArray();
                    f.DataScope(0, () =>
                    {
                        fixed (byte* bytes = prefix)
                            check(f.Program.CopyToUserWrite(machine, (long)frameBase, bytes,
                                witness == 4 ? 432UL : 440UL) == 0, "install required prefix only");
                        if (witness is 1 or 3 or 5)
                            fixed (byte* bytes = image)
                                check(f.Program.CopyToUserWrite(machine, (long)external, bytes,
                                    witness == 5 ? 256UL : 512UL) == 0, "install external image independently");
                    });
                    BinaryPrimitives.WriteUInt64LittleEndian(new Span<byte>((byte*)machine + 56, 8), frameBase + 8);
                    machine->ip = 0x400123; machine->restored = false;
                    byte[] beforeGprs = new ReadOnlySpan<byte>((byte*)machine + 24, 128).ToArray();
                    byte[] beforeMetadata = new ReadOnlySpan<byte>((byte*)machine + 648, 56).ToArray();
                    byte[] beforeXmm = new ReadOnlySpan<byte>((byte*)machine + 160, 256).ToArray();
                    byte[] beforeFile = new ReadOnlySpan<byte>(file, FullX87RegisterFile.ByteSize).ToArray();
                    bool faulted = false;
                    try { f.DataScope(0, () => f.Program.SigRestore(machine)); }
                    catch (GuestSignal) { faulted = true; }
                    bool success = witness <= 1;
                    check(faulted != success, $"SigRestore witness {witness} follows required actual reads");
                    if (success)
                    {
                        check(machine->ip == 0x405678 && (int)machine->restored != 0 &&
                            BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>((byte*)machine + 24, 8)) == 0x1122334455667788UL &&
                            BinaryPrimitives.ReadUInt64LittleEndian(new ReadOnlySpan<byte>((byte*)machine + 56, 8)) == 0x407ff0UL,
                            "SigRestore required context restored without inline access");
                        if (witness == 0)
                        {
                            check(machine->fpu.cw == 0x37f && machine->fpu.sw == 0 && machine->fpu.tw == 0xffff &&
                                machine->mxcsr == 0x1f80, "null actual pointer resets canonical metadata");
                            for (int r = 0; r < 8; ++r)
                                check(FullX87RegisterFile.Physical(file, r)->Significand == 0 &&
                                    FullX87RegisterFile.Physical(file, r)->SignExponent == 0,
                                    "null actual pointer resets raw80 slots");
                        }
                        else
                        {
                            byte[] roundtrip = new byte[512];
                            check(callbacks.TransferX87State((nint)machine, 1, HostX87Transfer.LinuxSave, roundtrip) ==
                                HostX87TransferResult.Complete && image.AsSpan().SequenceEqual(roundtrip),
                                "external actual image preserves raw80/TOP/tags/metadata/XMM");
                        }
                    }
                    else
                    {
                        check(worker.Signal == 11 && machine->ip == 0x400123 && (int)machine->restored == 0 &&
                            beforeGprs.AsSpan().SequenceEqual(new ReadOnlySpan<byte>((byte*)machine + 24, 128)) &&
                            beforeMetadata.AsSpan().SequenceEqual(new ReadOnlySpan<byte>((byte*)machine + 648, 56)) &&
                            beforeXmm.AsSpan().SequenceEqual(new ReadOnlySpan<byte>((byte*)machine + 160, 256)) &&
                            beforeFile.AsSpan().SequenceEqual(new ReadOnlySpan<byte>(file, FullX87RegisterFile.ByteSize)),
                            "denied/truncated/malformed actual transport faults before architectural mutation");
                    }
                    check(owner.fullJit!.Authority.PendingAtomicWorker == -1 && !worker.X87NativeInFlight &&
                        worker.X87Binding!.Get((nint)machine, 1) == (nint)file,
                        "transport returns with exact owner and no native/ticket leak");
                }
                finally { file->ProfileActive = 0; }
            }
#else
            throw new NotSupportedException("Fresh x87 producer + explicit owner required for transport fixture");
#endif
        }
    }
}

unsafe partial class Program
{
    static void CheckFullX87SignalTransport() => Managed.Emulation.Execution.ThreadedGuestExecution.TestFullX87SignalTransport(Check);
}
