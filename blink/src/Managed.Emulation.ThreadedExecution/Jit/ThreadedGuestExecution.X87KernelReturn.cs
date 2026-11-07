namespace Managed.Emulation.Execution;

public sealed unsafe partial class ThreadedGuestExecution
{
    // Recheck after prepare callbacks under the SAME admission gate. The
    // invocation can publish only this worker's exact registered576 allocation;
    // a copied/bare192 header grants no workframe capability.
    private static void ValidateOwnedX87KernelFrame(Worker worker, in FullBlockFrame frame)
    {
        var expected = GetX87NativePointer(worker);
        if (frame.Machine != worker.Machine || frame.X87 != expected)
            throw new InvalidOperationException("Native x87 frame differs from its exact registered worker.");
        if (expected == null) return;
        var native = worker.FullNativeState;
        if (native == null || frame.OwnerCookie == 0 || frame.OwnerCookie != worker.X87OwnerToken ||
            native.X87Pointer != expected || native.X87WorkPointer == null ||
            (byte*)native.X87WorkPointer != (byte*)expected + FullX87NativeStorage.WorkOffset ||
            native.NativeStorageBytes < FullX87NativeStorage.ByteSize)
            throw new InvalidOperationException("Native x87 frame has no exact owned work allocation.");
    }

    // Call ONLY after a proven native RET and the binding's NativeReturned,
    // before callbacks, REP adoption, fallback or any architectural delivery.
    private static void ValidateX87KernelReturn(Worker worker, in FullBlockFrame frame)
    {
        if (frame.X87 == null) return;
        var native = worker.FullNativeState ??
            throw new InvalidOperationException("Returned canonical computation lost its owned storage.");
        if (frame.X87 != native.X87Pointer || worker.X87NativeInFlight || native.X87WorkPointer == null)
            throw new InvalidOperationException("Returned canonical computation has an invalid owned workframe.");
        byte* work = (byte*)native.X87WorkPointer;
        if (*(uint*)(work + 280) != 0)
            throw new InvalidOperationException("Native x87 kernel protocol failed internally; admission=" +
                (*(uint*)(work + 284)).ToString());
    }
}
