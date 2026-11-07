using Managed.Emulation.Execution;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal static void TestCanonicalX87ProbeMaterialization(Action<bool, string> check)
        {
            foreach (uint width in new uint[] { 16, 32, 64, 80 })
            {
                using var fixture = new FullControllerFixture();
                const ulong pc = FullControllerFixture.Base;
                fixture.Map(pc, 8192); fixture.Fill(pc);
                fixture.DataScope(0, () =>
                {
                    byte* page = fixture.Program.LookupAddress2(fixture.Machine(), (long)pc, 0, 0);
                    page[0] = 0x90; page[1] = 0xeb; page[2] = 0xfe; page[4095] = 0x5a;
                });
                check(fixture.Acquire(pc, 0, out var lease, out _, out _), "x87 probe watched first page setup");
                using (lease)
                {
                    var jit = fixture.Jit; var owner = fixture.Owner; var machine = fixture.Machine();
                    var worker = owner.workers[0];
                    var d = new FullStackMemoryDescriptor
                    {
                        Version = FullCanonicalX87MemoryLowering.MemoryProbeTag, Count = 1,
                        State = FullStackMemoryState.Request,
                        Request0 = new() { Address = pc + 4095, Access = FullMemoryAccess.Write, Width = width }
                    };
                    var frame = new FullBlockFrame { Machine = machine, StackMemory = &d,
                        MemoryAddress = pc + 4095, MemoryAccess = FullMemoryAccess.Write,
                        MemoryWidth = width, MemoryResume = 1, ObservedGlobalEpoch = jit.Authority.Epoch };
                    FullMemoryAssistResult result;
                    using (var data = new FullDataScope(owner, worker))
                    lock (jit.Authority.Gate)
                    {
                        fixture.Program.ResetTlb(machine);
                        result = FullNativeMemory.TryFill(fixture.Program, machine, &frame, jit.Authority);
                    }
                    check(result == FullMemoryAssistResult.DemandPage && d.State == FullStackMemoryState.Request &&
                        frame.MemoryAddress == pc + 4095 && frame.MemoryWidth == width,
                        "suppressed x87 full-span proof preserves lazy second endpoint request");
                    int borrows = jit.Authority.ActiveBorrows;
                    ulong runtime = jit.Authority.Epoch, code = jit.Authority.CodeEpoch;
                    long revocations = jit.Store.GetStatistics().AuthorityRevocations;
                    uint sw = machine->fpu.sw; int tw = machine->fpu.tw; uint cw = machine->fpu.cw;
                    var registers = worker.FullNativeState!.X87Pointer;
                    byte[] raw = new ReadOnlySpan<byte>(registers, 192).ToArray();
                    check(owner.MaterializeNativeMemory(worker, &frame) == FullMemoryAssistResult.Resolved,
                        "actual controller materializer commits lazy page through no-writer copy lookup");
                    check(jit.Authority.ActiveBorrows == borrows && jit.Authority.Epoch == runtime &&
                        jit.Authority.CodeEpoch == code && jit.Store.GetStatistics().AuthorityRevocations == revocations &&
                        jit.Store.GetHeaderCopy(in lease).Live == 1,
                        "suppressed destination materialization neither enrolls writers nor revokes watched aliases");
                    using (var data = new FullDataScope(owner, worker))
                    lock (jit.Authority.Gate)
                    {
                        fixture.Program.ResetTlb(machine);
                        result = FullNativeMemory.TryFill(fixture.Program, machine, &frame, jit.Authority);
                    }
                    check(result == FullMemoryAssistResult.Resolved && d.State == FullStackMemoryState.Ready &&
                        d.Epoch == runtime && d.Ticket == 0 && d.Request0.Host0 == null && d.Request0.Host1 == null &&
                        d.Request0.Length0 == 0 && frame.MemoryFlags == FullMemoryFlags.None,
                        "materialized proof publishes only Ready epoch, without writable capability");
                    check(machine->fpu.sw == sw && machine->fpu.tw == tw && machine->fpu.cw == cw &&
                        new ReadOnlySpan<byte>(registers, 192).SequenceEqual(raw),
                        "permission-only controller route preserves all canonical values and FP controls/status");
                }
            }
        }
    }
}

unsafe partial class Program
{
    static void CheckFullCanonicalX87ProbeMaterialization() =>
        ThreadedGuestExecution.TestCanonicalX87ProbeMaterialization(Check);
}
