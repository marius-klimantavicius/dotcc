using System.Runtime.InteropServices;
using Managed.Emulation.Execution;

namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        internal static void TestFullX87WorkStorage(Action<bool, string> check)
        {
            check(sizeof(FullX87NativeStorage) == 576 && FullX87NativeStorage.WorkOffset == 192 &&
                Marshal.OffsetOf<FullX87NativeStorage>(nameof(FullX87NativeStorage.Work)).ToInt32() == 192 &&
                sizeof(FullX87RegisterFile) == 192 && sizeof(FullExtended80WorkFrame) == 384,
                "register file and native workframe are distinct aligned regions");
            using var fixture = new FullControllerFixture();
            var owner = fixture.Owner; var main = owner.workers[0]; var child = owner.workers[1];
            var native = main.FullNativeState!; var file = native.X87Pointer; var work = native.X87WorkPointer;
            var other = child.FullNativeState!.X87WorkPointer;
            check(work != null && other != null && work != other && ((nuint)work & 15) == 0 &&
                (byte*)work - (byte*)file == 192 && native.NativeStorageBytes == FullNativeMemory.WorkerNativeStorageBytes,
                "each worker owns disjoint aligned native scratch behind its canonical file");
            check(new ReadOnlySpan<byte>(work, 384).IndexOfAnyExcept((byte)0) < 0 &&
                new ReadOnlySpan<byte>(other, 384).IndexOfAnyExcept((byte)0) < 0,
                "fresh worker scratch zeroed before any native use");
            foreach (int count in new[] { 2, 64, 256, 4096 })
            {
                long prefix = (long)count * FullMemoryTlbEntry.ByteSize +
                    FullNativeMemory.JumpCount * FullNativeJumpEntry.ByteSize + FullStackMemoryDescriptor.ByteSize;
                long storage = (prefix + 15 & ~15L) + FullX87NativeStorage.ByteSize;
                check(FullNativeMemory.WorkerNativeStorageBytesFor(count) == storage &&
                    FullNativeMemory.WorkerChargeFor(count) == storage + FullNativeMemory.X87BindingAndWorkerAllowance + FullWorkerNativeState.MappingCacheCharge,
                    "native scratch is charged exactly once to the existing total owner budget");
            }
            work->Version = FullExtended80WorkFrame.CurrentVersion;
            work->Size = FullExtended80WorkFrame.ByteSize;
            work->Left = new(0x8000000000000001UL, 0x3fff);
            work->Continuation = 0x12345678; // inert test bytes, never invoked
            work->Reserved256[0] = 0xa5;
            file->R5 = new(0x8000000000000007UL, 0x4001);
            fixture.At(FullControllerFixture.Base);
            owner.CloneX87WorkerForTest(main, child);
            check(child.FullNativeState.X87Pointer->R5.Significand == file->R5.Significand &&
                child.FullNativeState.X87Pointer->OwnerGeneration != file->OwnerGeneration &&
                new ReadOnlySpan<byte>(other, 384).IndexOfAnyExcept((byte)0) < 0 &&
                work->Continuation == 0x12345678 && work->Reserved256[0] == 0xa5,
                "clone copies register values but never parent native continuation/scratch");
            fixture.Dispose();
            check(native.NativeStorageBytes == 0 && native.X87Pointer == null && native.X87WorkPointer == null &&
                main.X87Retired && child.X87Retired && main.ResourcesDisposed && child.ResourcesDisposed,
                "returned detached owners retire and release aligned register/work storage together");
            fixture.Dispose();
        }
    }
}

unsafe partial class Program
{
    static void CheckFullX87WorkStorage() => ThreadedGuestExecution.TestFullX87WorkStorage(Check);
}
