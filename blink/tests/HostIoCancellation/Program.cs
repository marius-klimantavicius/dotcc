using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Managed.Emulation;
using Managed.Emulation.Host;

static void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
static InstanceIo Owner() => new(new Dictionary<string, ReadOnlyMemory<byte>>(), "abcd"u8.ToArray(), pipeCapacity: 4096);
static async Task WaitPending(Func<bool> pending, Task operation)
{
    long start = Stopwatch.GetTimestamp();
    while (!pending() && !operation.IsCompleted && Stopwatch.GetElapsedTime(start) < TimeSpan.FromSeconds(5))
        await Task.Delay(1);
    if (operation.IsFaulted) await operation;
    Check(pending() && !operation.IsCompleted, "call must have an actual pending host operation");
}
static Work Start(InstanceIo owner, CancellationToken token, Func<long> call, HostSignalWake? wake = null)
{
    var completion = new TaskCompletionSource<long>(TaskCreationOptions.RunContinuationsAsynchronously);
    var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
    var thread = new Thread(() =>
    {
        try
        {
            Blink.BindHostIo(owner, token, wake);
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            entered.SetResult();
            completion.SetResult(call());
        }
        catch (Exception error) { entered.TrySetException(error); completion.TrySetException(error); }
        finally { Blink.UnbindHostIo(); }
    }) { IsBackground = true };
    thread.Start();
    return new(thread, completion.Task, entered.Task);
}
static async Task<long> Finish(Work work)
{
    long value = await work.Result.WaitAsync(TimeSpan.FromSeconds(10));
    Check(work.Thread.Join(TimeSpan.FromSeconds(5)), "C thread unwinds and unbinds");
    return value;
}
static async Task Dispose(InstanceIo owner)
{
    await owner.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
    Check(owner.PendingPipeOperations == 0 && owner.PendingSocketOperations == 0 && owner.PipeBytes == 0,
          "disposal drains operations and releases pipe storage");
}
static async Task PipeCancel(int operation, bool deadline = false, bool dispose = false)
{
    var owner = Owner();
    using var token = new CancellationTokenSource();
    try
    {
        var pair = owner.Pipe(); Check(pair.Succeeded, "create valid pipe");
        bool writing = operation is 2 or 3;
        if (writing)
        {
            var fill = await owner.WriteAsync(pair.Value.Write, Enumerable.Repeat((byte)0x77, 4096).ToArray());
            Check(fill.Succeeded && fill.Value == 4096, "fill valid pipe capacity");
        }
        int fd = writing ? pair.Value.Write : pair.Value.Read;
        Work work = Start(owner, token.Token, () => Blink.CanceledCall(operation, fd, 0));
        await WaitPending(() => owner.PendingPipeOperations == 1, work.Result);
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        if (dispose) await Dispose(owner);
        else if (deadline) token.CancelAfter(TimeSpan.FromMilliseconds(40));
        else token.Cancel();
        Check(await Finish(work) == 0, "translated pipe cancellation and unchanged outputs");
        Check(owner.PendingPipeOperations == 0, "canceled pipe call drained");
        if (writing && !dispose)
        {
            byte[] bytes = new byte[4096]; var read = await owner.ReadAsync(pair.Value.Read, bytes);
            Check(read.Succeeded && read.Value == bytes.Length && bytes.All(x => x == 0x77), "canceled full-pipe write commits no bytes");
        }
    }
    finally { token.Cancel(); await Dispose(owner); }
}
static async Task PartialPipe(int operation, bool signalWake = false)
{
    var owner = Owner(); using var token = new CancellationTokenSource();
    using var wake = signalWake ? new HostSignalWake() : null;
    try
    {
        var pair = owner.Pipe(); Check(pair.Succeeded, "partial pipe create");
        Work work = Start(owner, token.Token, () => Blink.PartialWrite(operation, pair.Value.Write), wake);
        await WaitPending(() => owner.PendingPipeOperations == 1, work.Result);
        long start = Stopwatch.GetTimestamp();
        bool committed = false;
        while (!committed && !work.Result.IsCompleted && Stopwatch.GetElapsedTime(start) < TimeSpan.FromSeconds(5))
        {
            var ready = await owner.PollAsync([new(pair.Value.Read, 1)], 0);
            Check(ready.Succeeded, "query valid partial pipe readiness");
            committed = ready.Value.Count == 1;
            if (!committed) await Task.Delay(1);
        }
        Check(committed && !work.Result.IsCompleted, "partial write committed capacity and remains pending");
        if (wake != null) wake.Request(); else token.Cancel();
        Check(await Finish(work) == 4096, "successful partial count survives cancellation");
        byte[] bytes = new byte[4096]; var read = await owner.ReadAsync(pair.Value.Read, bytes);
        Check(read.Succeeded && read.Value == 4096 && bytes.All(x => x == 0x5a), "exact successful partial bytes");
    }
    finally { token.Cancel(); await Dispose(owner); }
}
static int Listener(InstanceIo owner)
{
    var listener = owner.Socket(); Check(listener.Succeeded, "socket create");
    Check(owner.Bind(listener.Value, new(GuestEndpoint.Loopback, 8080)).Succeeded, "private bind");
    Check(owner.Listen(listener.Value, 4).Succeeded, "private listen");
    return listener.Value;
}
static async Task<(int Client, int Accepted)> Pair(InstanceIo owner, int listener)
{
    var client = owner.Socket(); Check(client.Succeeded, "client create");
    Check((await owner.ConnectAsync(client.Value, new(GuestEndpoint.Loopback, 8080))).Succeeded, "valid connection");
    var accepted = await owner.AcceptAsync(listener); Check(accepted.Succeeded, "valid accept");
    return (client.Value, accepted.Value.Handle);
}
static async Task SocketCancel(int operation, bool deadline = false, bool dispose = false)
{
    var owner = Owner(); using var token = new CancellationTokenSource();
    try
    {
        int listener = Listener(owner);
        int fd = operation == 8 ? listener : (await Pair(owner, listener)).Accepted;
        int descriptors = owner.OpenDescriptors;
        Work work = Start(owner, token.Token, () => Blink.CanceledCall(operation, fd, 0));
        await WaitPending(() => owner.PendingSocketOperations == 1, work.Result);
        if (dispose) await Dispose(owner);
        else if (deadline) token.CancelAfter(TimeSpan.FromMilliseconds(40));
        else token.Cancel();
        Check(await Finish(work) == 0, "translated socket cancellation and unchanged outputs");
        Check(owner.PendingSocketOperations == 0, "socket call drained");
        if (!dispose) Check(owner.OpenDescriptors == descriptors, "canceled accept does not publish descriptor");
    }
    finally { token.Cancel(); await Dispose(owner); }
}
static async Task PollCancel(bool deadline)
{
    var owner = Owner(); using var token = new CancellationTokenSource();
    try
    {
        var pair = owner.Pipe(); Check(pair.Succeeded, "poll pipe");
        Work work = Start(owner, token.Token, () => Blink.CanceledCall(9, pair.Value.Read, 0));
        await work.Entered.WaitAsync(TimeSpan.FromSeconds(5));
        await Task.Delay(50);
        // The public model has no poll registration counter. This observes an
        // incomplete C call on a known nonready source, not its precise entry time.
        Check(!work.Result.IsCompleted, "nonready poll remains incomplete before cancellation");
        if (deadline) token.CancelAfter(TimeSpan.FromMilliseconds(40)); else token.Cancel();
        Check(await Finish(work) == 0, "translated poll ECANCELED and unchanged revents");
    }
    finally { token.Cancel(); await Dispose(owner); }
}
static async Task PreCanceledSocket(int operation)
{
    var owner = Owner(); using var token = new CancellationTokenSource();
    try
    {
        int listener = Listener(owner); int fd;
        if (operation == 10) { var client = owner.Socket(); Check(client.Succeeded, "connect target"); fd = client.Value; }
        else fd = (await Pair(owner, listener)).Client;
        token.Cancel();
        Check(await Finish(Start(owner, token.Token, () => Blink.CanceledCall(operation, fd, 8080))) == 0,
              "valid pre-canceled send/message/connect call");
    }
    finally { await Dispose(owner); }
}
static async Task Defaults()
{
    var owner = Owner(); using var token = new CancellationTokenSource(); token.Cancel();
    try
    {
        Work reset = Start(owner, token.Token, () =>
        {
            Check(Blink.CanceledCall(0, 0, 0) == 0, "canceled input read");
            Blink.UnbindHostIo(); Blink.BindHostIo(owner);
            return Blink.ReadFour(0);
        });
        Check(await Finish(reset) == 0, "original overload and unbind clear cancellation");
        int listener = Listener(owner);
        for (int message = 0; message < 2; ++message)
        {
            var fd = owner.Socket(); Check(fd.Succeeded, "default client");
            int mode = message;
            Work call = Start(owner, default, () =>
            {
                Check(Blink.ConnectValid(fd.Value, 8080) == 0, "default translated connect");
                return Blink.SendFour(fd.Value, mode);
            });
            var accepted = await owner.AcceptAsync(listener).WaitAsync(TimeSpan.FromSeconds(5));
            Check(accepted.Succeeded, "default translated connection accepted");
            long count = await Finish(call); Check(count is > 0 and <= 4, "successful stream send count");
            byte[] bytes = new byte[(int)count]; int offset = 0;
            while (offset < bytes.Length)
            {
                var read = await owner.ReceiveAsync(accepted.Value.Handle, bytes.AsMemory(offset)).WaitAsync(TimeSpan.FromSeconds(5));
                Check(read.Succeeded && read.Value > 0, "receive successful send prefix"); offset += read.Value;
            }
            Check(bytes.AsSpan().SequenceEqual("abcd"u8[..(int)count]), "default send/message exact transferred prefix");
        }
    }
    finally { await Dispose(owner); }
}
static async Task Isolation()
{
    var first = Owner(); var second = Owner();
    using var a = new CancellationTokenSource(); using var b = new CancellationTokenSource();
    try
    {
        var ap = first.Pipe(); var bp = second.Pipe(); Check(ap.Succeeded && bp.Succeeded, "two pipes");
        Work aw = Start(first, a.Token, () => Blink.CanceledCall(0, ap.Value.Read, 0));
        Work bw = Start(second, b.Token, () => Blink.CanceledCall(1, bp.Value.Read, 0));
        await WaitPending(() => first.PendingPipeOperations == 1 && second.PendingPipeOperations == 1, Task.WhenAll(aw.Result, bw.Result));
        a.Cancel(); Check(await Finish(aw) == 0, "first owner canceled");
        Check(!bw.Result.IsCompleted && second.PendingPipeOperations == 1, "second owner remains independent");
        b.Cancel(); Check(await Finish(bw) == 0, "second owner canceled separately");
    }
    finally { a.Cancel(); b.Cancel(); await Dispose(first); await Dispose(second); }
}
static async Task BclReference()
{
    var owner = Owner(); using var token = new CancellationTokenSource();
    try
    {
        var pair = owner.Pipe(); Check(pair.Succeeded, "BCL pipe");
        Task<HostResult<int>> call = owner.ReadAsync(pair.Value.Read, new byte[8], token.Token);
        Check(owner.PendingPipeOperations == 1 && !call.IsCompleted, "BCL read actually pending");
        token.Cancel(); var canceled = await call.WaitAsync(TimeSpan.FromSeconds(5));
        Check(!canceled.Succeeded && canceled.Error == GuestError.Canceled, "private BCL ECANCELED contract");
    }
    finally { await Dispose(owner); }
}

static async Task TransientWake(int operation, bool beforeRegistration = false)
{
    var owner = Owner(); using var permanent = new CancellationTokenSource();
    using var wake = new HostSignalWake();
    try
    {
        int fd;
        if (operation is 0 or 1 or 9)
        {
            var pair = owner.Pipe(); Check(pair.Succeeded, "signal wake pipe"); fd = pair.Value.Read;
        }
        else
        {
            int listener = Listener(owner);
            fd = operation == 8 ? listener : (await Pair(owner, listener)).Accepted;
        }
        int descriptors = owner.OpenDescriptors;
        if (beforeRegistration) { wake.Request(); wake.Request(); }
        Work work = Start(owner, permanent.Token, () =>
        {
            Check(Blink.InterruptedCall(operation, fd) == 0, "transient wake returns EINTR with unchanged outputs");
            wake.Checkpoint(); // Explicit counterpart of the guest's signal-consumption checkpoint.
            return Blink.ReadFour(0);
        }, wake);
        if (!beforeRegistration)
        {
            if (operation == 9)
            {
                await work.Entered.WaitAsync(TimeSpan.FromSeconds(5));
                await Task.Delay(50);
                Check(!work.Result.IsCompleted, "signal poll is actually incomplete");
            }
            else await WaitPending(() => operation < 2 ? owner.PendingPipeOperations == 1 : owner.PendingSocketOperations == 1, work.Result);
            // A second request here could arrive after the worker's checkpoint
            // and legitimately interrupt its subsequent read. Repeated pending
            // requests are covered before registration above.
            wake.Request();
        }
        Check(await Finish(work) == 0, "checkpoint allows subsequent ordinary read");
        Check(owner.OpenDescriptors == descriptors && owner.PendingPipeOperations == 0 && owner.PendingSocketOperations == 0,
            "transient notification drains without closing descriptors");
        wake.Dispose();
        Check(wake.NotificationFailure == null, "transient notification callbacks completed cleanly");
    }
    finally { permanent.Cancel(); await Dispose(owner); }
}
static async Task PermanentAndTransient()
{
    var owner = Owner(); using var permanent = new CancellationTokenSource();
    using var wake = new HostSignalWake();
    try
    {
        var pair = owner.Pipe(); Check(pair.Succeeded, "valid priority pipe");
        permanent.Cancel(); wake.Request();
        Work work = Start(owner, permanent.Token, () => Blink.CanceledCall(0, pair.Value.Read, 0), wake);
        Check(await Finish(work) == 0, "permanent cancellation retains ECANCELED despite transient request");
        wake.Dispose(); Check(wake.NotificationFailure == null, "priority notification drained");
    }
    finally { await Dispose(owner); }
}

Check(Blink.LayoutProbe() == 0, "translated ABI");
await BclReference();
for (int operation = 0; operation < 4; ++operation) await PipeCancel(operation);
await PipeCancel(0, deadline: true); await PipeCancel(1, dispose: true);
await PartialPipe(2); await PartialPipe(3);
foreach (int operation in new[] { 4, 6, 8 }) await SocketCancel(operation);
await SocketCancel(4, deadline: true); await SocketCancel(6, dispose: true);
await PollCancel(false); await PollCancel(true);
foreach (int operation in new[] { 5, 7, 10 }) await PreCanceledSocket(operation);
await Defaults(); await Isolation();
foreach (int operation in new[] { 0, 1, 4, 6, 8, 9 }) await TransientWake(operation);
await TransientWake(0, beforeRegistration: true);
await PermanentAndTransient();
await PartialPipe(2, signalWake: true); await PartialPipe(3, signalWake: true);
Console.WriteLine("callback cancellation: pipes, vectors, sockets, messages, poll, deadlines, partial writes, reset, isolation, drain, transient wake/checkpoint: PASS");

sealed record Work(Thread Thread, Task<long> Result, Task Entered);
