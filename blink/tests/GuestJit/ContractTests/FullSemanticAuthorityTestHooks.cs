namespace Managed.Emulation.Execution;

// Erasable hooks compiled only by ContractTests. No production callbacks,
// per-admission delegates, dynamic state, or alternate semantic implementation.
internal sealed unsafe partial class FullCodeAuthority
{
    internal Action<int, bool>? TestSemanticAdmission;
    internal Action<int>? TestSemanticPending;
    internal Action<bool>? TestHostAdmission;
    internal Action<int>? TestReservationPublished;
    partial void SemanticReaderAdmissionStep(int worker, bool published) => TestSemanticAdmission?.Invoke(worker, published);
    partial void SemanticReaderPending(int worker) => TestSemanticPending?.Invoke(worker);
    partial void HostReaderAdmissionStep(bool published) => TestHostAdmission?.Invoke(published);
    partial void NativeReservationPublished(int worker) => TestReservationPublished?.Invoke(worker);
}

public sealed unsafe partial class ThreadedGuestExecution
{
    internal Action<nint>? TestHelperMappingEnded;
    partial void FullControllerHelperMappingEnded(Managed.Emulation.BlinkCore.Machine* machine)
        => TestHelperMappingEnded?.Invoke((nint)machine);
}
