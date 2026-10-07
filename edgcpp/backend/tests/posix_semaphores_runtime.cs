using System;
using System.Runtime.InteropServices;
using System.Threading;
using Edg.Runtime;

// Test-only access to synchronization state avoids sleeps/races when checking
// destruction of a semaphore with a thread actually blocked inside its wait.
namespace Edg.Runtime {
    public sealed unsafe partial class PosixContext {
        internal int TestSemaphoreWaiters(ulong token) {
            lock (semaphoreGate) {
                if (!semaphores.TryGetValue(token, out var entry)) return 0;
                lock (entry.Gate) return entry.Waiters;
            }
        }
    }
}

unsafe class Program {
    [ThreadStatic] private static int error;
    private static int checks;
    private static void Check(bool value) { if (!value) throw new Exception("Semaphore runtime check " + checks); ++checks; }
    private static void WaitFor(PosixContext context, ulong token, int count) {
        Check(SpinWait.SpinUntil(() => context.TestSemaphoreWaiters(token) == count, 10000));
    }
    static void Main() {
        using var first = new PosixContext(value => error = value);
        using var second = new PosixContext(value => error = value);
        nint address = (nint)NativeMemory.AllocZeroed(8);
        ulong* slot = (ulong*)address;
        try {
            Check(first.SemaphoreInit(slot, 1, 0) == -1 && error == 38);
            Check(first.SemaphoreInit(slot, 0, uint.MaxValue) == -1 && error == 22);
            Check(first.SemaphoreInit(slot, 0, 0) == 0);
            Check(second.SemaphorePost(slot) == -1 && error == 22);
            Check(first.SemaphoreTryWait(slot) == -1 && error == 11);
            int result = 99, workerError = 0;
            var worker = new Thread(() => { result = first.SemaphoreWait((ulong*)address); workerError = error; });
            worker.Start(); WaitFor(first, *slot, 1);
            Check(first.SemaphoreDestroy(slot) == -1 && error == 16);
            Check(first.SemaphorePost(slot) == 0);
            Check(worker.Join(10000) && result == 0);
            int count = -1;
            Check(first.SemaphoreGetValue(slot, &count) == 0 && count == 0);

            worker = new Thread(() => { result = first.SemaphoreWait((ulong*)address); workerError = error; });
            worker.Start(); WaitFor(first, *slot, 1); worker.Interrupt();
            Check(worker.Join(10000) && result == -1 && workerError == 4);
            Check(first.SemaphorePost(slot) == 0 && first.SemaphoreTryWait(slot) == 0);
            Check(first.SemaphoreDestroy(slot) == 0 && *slot == 0);
            Check(first.SemaphoreTryWait(slot) == -1 && error == 22);

            Check(first.SemaphoreInit(slot, 0, 0) == 0);
            ulong stale = *slot;
            Check(first.SemaphoreDestroy(slot) == 0 && first.SemaphoreInit(slot, 0, 0) == 0 && *slot != stale);
            Check(first.SemaphorePost(&stale) == -1 && error == 22);

            var deadline = new PosixSemaphoreTimespec { tv_sec=long.MaxValue, tv_nsec=999999999 };
            nint deadlineAddress = (nint)NativeMemory.Alloc((nuint)sizeof(PosixSemaphoreTimespec));
            *(PosixSemaphoreTimespec*)deadlineAddress = deadline;
            try {
                worker = new Thread(() => { result=first.SemaphoreTimedWait((ulong*)address, (PosixSemaphoreTimespec*)deadlineAddress); });
                worker.Start(); WaitFor(first, *slot, 1);
                Check(first.SemaphorePost(slot) == 0 && worker.Join(10000) && result == 0);
            } finally { NativeMemory.Free((void*)deadlineAddress); }

            worker = new Thread(() => { result = first.SemaphoreWait((ulong*)address); workerError = error; });
            worker.Start(); WaitFor(first, *slot, 1); first.Dispose();
            Check(worker.Join(10000) && result == -1 && workerError == 22);
            Check(first.SemaphoreInit(slot, 0, 1) == -1 && error == 22);
            Check(second.SemaphoreInit(slot, 0, 1) == 0 && second.SemaphoreWait(slot) == 0);
            Check(second.SemaphoreDestroy(slot) == 0);
        } finally { NativeMemory.Free(slot); }
        Console.WriteLine("POSIX semaphore runtime isolation, interruption and disposal: " + checks + " checks passed");
    }
}
