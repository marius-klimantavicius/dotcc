// Parent enables this integration symbol only after adding Worker.X87Binding
// and all allocation/native-entry/return/cleanup seams in D/PARENT-SEAMS.md.
// This symbol DOES NOT enable ProfileActive or guest-x87 admission.
#if BLINK_FULL_X87_STATE_OWNER
using System;
using Managed.Emulation.Host;
using Blink = Managed.Emulation.BlinkCore;

namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution : IHostGuestX87State
{
    private FullX87WorkerBinding? CurrentX87(nint machine)
    {
        Worker worker = RequireCurrent();
        if ((nint)worker.Machine != machine || machine == 0 || worker.Released || !worker.Bound)
            throw new InvalidOperationException("x87 Machine is not the current live worker");
        if (fullJit != null && worker.X87Binding == null)
            throw new InvalidOperationException("Full worker is missing its canonical x87 binding");
        return worker.X87Binding;
    }
    nint IHostGuestX87State.GetX87RegisterFile(nint machine, uint protocol)
    {
        if (protocol != 1) throw new InvalidOperationException("x87 producer protocol mismatch");
        return CurrentX87(machine)?.Get(machine, protocol) ?? 0;
    }
    HostX87TransferResult IHostGuestX87State.TransferX87State(nint machine, uint protocol,
        HostX87Transfer operation, Span<byte> image)
    {
        if (protocol != 1 || FullX87StateBridge.ImageSize(operation) != image.Length)
            throw new InvalidOperationException("x87 producer protocol/image mismatch");
        return CurrentX87(machine)?.Transfer(machine, protocol, operation, image) ?? HostX87TransferResult.Inactive;
    }
    void IHostGuestX87State.RejectLegacyX87State(nint machine, uint protocol)
    {
        if (protocol != 1) throw new InvalidOperationException("x87 producer protocol mismatch");
        CurrentX87(machine)?.RejectLegacy(machine, protocol);
    }
    private static void CheckX87StateProducer(Blink program)
    {
        if (program.BlinkGuestX87StateProtocol() != FullX87RegisterFile.CurrentProducerProtocol)
            throw new InvalidOperationException("Full-JIT canonical x87 producer protocol differs");
    }
}
#endif
