using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static class Program
{
    static void Check(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }
    static InstanceIo Owner() => new(new Dictionary<string, ReadOnlyMemory<byte>>());
    static void Drain(InstanceIo io) => io.DisposeAsync().AsTask().GetAwaiter().GetResult();

    // Cancellation and owner shutdown are private lifecycle behavior (125),
    // separate from the ordinary native timeout witness (EAGAIN=11).
    static void Lifecycle(bool accepting, bool dispose)
    {
        var io = Owner();
        var cancellation = new CancellationTokenSource();
        Thread? worker = null;
        bool joined = false;
        Exception? workerError = null;
        try
        {
            Blink.BindHostIo(io);
            int listener = Blink.TimeoutListener();
            Check(listener >= 0, "private listener");
            int descriptor = listener;
            if (!accepting)
            {
                Check(Blink.TimeoutConnect(listener) >= 0, "normal connected peer");
                unsafe { descriptor = Blink.blink_host_accept(listener, null, null); }
                Check(descriptor >= 0, "normal accept");
            }
            // Five seconds is the real fixture setting. Reset-to-zero is also
            // exercised as an actually pending operation, released by owner stop.
            Check(Blink.TimeoutSet(descriptor, 20, 5) == 0, "five-second receive option");
            if (dispose) Check(Blink.TimeoutSet(descriptor, 20, 0) == 0, "reset infinite wait");
            Blink.UnbindHostIo();
            int waitingDescriptor = descriptor;
            worker = new Thread(() =>
            {
                try
                {
                    Blink.BindHostIo(io, cancellation.Token);
                    Check(Blink.TimeoutWait(waitingDescriptor, accepting ? 0 : 4, 125, 0) == 0,
                          "private canceled call/unchanged output");
                }
                catch (Exception error) { workerError = error; }
                finally { Blink.UnbindHostIo(); }
            }) { IsBackground = true };
            worker.Start();
            Check(SpinWait.SpinUntil(() => io.PendingSocketOperations == 1 || !worker.IsAlive,
                TimeSpan.FromSeconds(5)), "operation admission");
            Check(io.PendingSocketOperations == 1, "actual pending operation");
            Task? drain = null;
            if (dispose) drain = io.DisposeAsync().AsTask();
            else cancellation.Cancel();
            joined = worker.Join(TimeSpan.FromSeconds(5));
            Check(joined, "worker joined");
            drain?.GetAwaiter().GetResult();
            if (workerError != null) throw workerError;
            Check(io.PendingSocketOperations == 0, "network operations drained");
        }
        finally
        {
            Blink.UnbindHostIo();
            if (worker == null || joined || (worker.ThreadState & ThreadState.Unstarted) != 0)
            { Drain(io); cancellation.Dispose(); }
            // A stuck worker retains its borrowed owners for failed-process discard.
        }
    }

    static int Main()
    {
        try
        {
            var io = Owner();
            try
            {
                Blink.BindHostIo(io);
                int status = Blink.TimeoutCommon();
                Check(status == 0, "common native timeout contract status=" + status);
            }
            finally { Blink.UnbindHostIo(); Drain(io); }
            Lifecycle(true, false); Lifecycle(false, false);
            Lifecycle(true, true); Lifecycle(false, true);
            Console.WriteLine("timeval16; five-second options; dup inheritance; accept recv read readv recvmsg expiry; normal transfers; zero reset");
            Console.WriteLine("private caller cancellation and owner drain passed");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}

namespace Managed.Emulation
{
    public static partial class Blink
    {
        public static long TimeoutMilliseconds() =>
            global::System.Diagnostics.Stopwatch.GetElapsedTime(0).Ticks / TimeSpan.TicksPerMillisecond;
    }
}
