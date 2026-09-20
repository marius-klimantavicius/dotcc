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

    static async Task Lifecycle(bool disposeOwner)
    {
        var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        using var cancellation = new CancellationTokenSource();
        int epfd = io.CreateEpoll(0).Value;
        int listener = io.Socket().Value;
        Check(io.Bind(listener, new(GuestEndpoint.Loopback, 0)).Succeeded, "bind");
        Check(io.Listen(listener, 4).Succeeded, "listen");
        Check(io.SetStatusFlags(listener, 2048).Succeeded, "nonblocking");
        Check(io.ControlEpoll(epfd, 1, listener, 0x80000001, 0x123456789abcdef0).Succeeded, "register");
        var wait = io.WaitEpollEventsAsync(epfd, 1, -1, cancellation.Token);
        Check(SpinWait.SpinUntil(() => io.PendingEpollOperations == 1, TimeSpan.FromSeconds(3)), "wait admitted");
        if (disposeOwner) await io.DisposeAsync();
        else cancellation.Cancel();
        var result = await wait.WaitAsync(TimeSpan.FromSeconds(3));
        Check(!result.Succeeded && result.Error == GuestError.Canceled, "ordinary cancellation");
        await io.DisposeAsync();
        Check(io.PendingEpollOperations == 0 && io.OpenDescriptors == 0, "owner drained");
    }

    static async Task<int> Main()
    {
        try
        {
            var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
            try
            {
                Blink.BindHostIo(io);
                Blink.BlinkHostDeliveryMaskReset();
                int result = Blink.AsyncSocketsCommon();
                Check(result == 0, "async socket contract stage " + result);
            }
            finally { Blink.UnbindHostIo(); await io.DisposeAsync(); }
            Check(io.OpenDescriptors == 0 && io.PendingEpollOperations == 0, "common owner drained");
            Console.WriteLine("nonblocking listener and accepted sockets passed");
            Console.WriteLine("drain-to-EAGAIN edges and MSG_PEEK passed");
            Console.WriteLine("opaque data maxEvents alias lifetime and descriptor reuse passed");
            await Lifecycle(false);
            await Lifecycle(true);
            Console.WriteLine("private requested stop and owner drain passed");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
