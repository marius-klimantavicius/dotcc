using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    static void Lifecycle(int scenario)
    {
        var io = Owner();
        var cancellation = new CancellationTokenSource();
        int fd = io.CreateEpoll(0).Value;
        int alias = io.Duplicate(fd).Value;
        Exception? workerError = null;
        bool joined = false;
        var worker = new Thread(() =>
        {
            try
            {
                Blink.BindHostIo(io, cancellation.Token);
                Blink.BlinkHostDeliveryMaskReset();
                Check(Blink.EpollWaitOnly(fd, -1, 4) == 0, "interrupted wait/mask/canary");
            }
            catch (Exception error) { workerError = error; }
            finally { Blink.UnbindHostIo(); }
        }) { IsBackground = true };
        try
        {
            worker.Start();
            Check(SpinWait.SpinUntil(() => io.PendingEpollOperations == 1 || !worker.IsAlive,
                TimeSpan.FromSeconds(5)), "wait admission");
            Check(io.PendingEpollOperations == 1, "worker entered actual wait");
            Check(io.Close(fd).Succeeded, "close first alias");
            Check(io.PendingEpollOperations == 1 && worker.IsAlive, "surviving alias preserves wait");
            Task? drain = null;
            if (scenario == 0) cancellation.Cancel();
            else if (scenario == 1) Check(io.Close(alias).Succeeded, "final alias close");
            else drain = io.DisposeAsync().AsTask();
            joined = worker.Join(TimeSpan.FromSeconds(5));
            Check(joined, "worker joined");
            drain?.GetAwaiter().GetResult();
            if (workerError != null) throw workerError;
            Check(io.PendingEpollOperations == 0, "wait drained");
        }
        finally
        {
            // On a stuck worker retain both owners for failed-process discard.
            if (joined || worker.ThreadState == System.Threading.ThreadState.Unstarted)
            { Drain(io); cancellation.Dispose(); }
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
                Blink.BlinkHostDeliveryMaskReset();
                Check(Blink.EpollEventSize() == 16 && Blink.EpollEventAlign() == 8 &&
                      Blink.EpollDataOffset() == 8, "private C layout");
                unsafe
                {
                    Blink.blink_host_epoll_event value = default;
                    Check(sizeof(Blink.blink_host_epoll_event) == 16 &&
                        (byte*)&value.data - (byte*)&value == 8, "actual generated record");
                }
                var timer = Stopwatch.StartNew();
                Check(Blink.EpollCommon() == 0, "common native contract");
                timer.Stop();
                Check(timer.ElapsedMilliseconds >= 10 && timer.Elapsed < TimeSpan.FromSeconds(30),
                      "finite wait elapsed");
                int epfd = io.CreateEpoll(0).Value;
                var pipe = io.Pipe().Value;
                Check(Blink.EpollUnsupportedRegistration(epfd, pipe.Read) == 0,
                      "registration explicitly unsupported");
                Check(Blink.EpollWaitOnly(epfd, 0, 0) == 0, "refused registration leaves empty set");
                var poll = io.PollAsync(new[] { new PollRequest(epfd, 1 | 4) }, 0).GetAwaiter().GetResult();
                Check(poll.Succeeded && poll.Value.Count == 0 && poll.Value.Events[0] == 0,
                      "empty epoll is not readable or writable");
                Check(io.Close(pipe.Read).Succeeded && io.Close(pipe.Write).Succeeded &&
                      io.Close(epfd).Succeeded, "normal descriptor close");
            }
            finally { Blink.UnbindHostIo(); Drain(io); }
            Lifecycle(0); Lifecycle(1); Lifecycle(2);
            Console.WriteLine("private layout 16 8 8");
            Console.WriteLine("empty epoll common passed");
            Console.WriteLine("private cancellation final-close owner-drain mask canary passed");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
