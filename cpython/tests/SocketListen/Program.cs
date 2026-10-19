using static DotCC.Libc.Libc;

// The driver supplies the measured native header value; this is a direct
// current-runtime test, not a translated C/header or CPython product claim.
internal static unsafe class Program
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message + " errno=" + errno);
    }

    public static int Main(string[] args)
    {
        int headerValue = int.Parse(args[0]);
        int requested = args.Length > 1 ? int.Parse(args[1]) : Math.Min(headerValue, 128);
        byte* address = stackalloc byte[16];
        new Span<byte>(address, 16).Clear();
        *(ushort*)address = 2; address[4] = 127; address[7] = 1;
        int listener = socket(2, 1, 0);
        Require(listener >= 0, "socket");
        int accepted = -1, datagram = -1;
        try
        {
            Require(bind(listener, address, 16) == 0, "bind");
            // Exercise the public API's own negative-backlog normalization too.
            Require(listen(listener, requested) == 0, "listen");
            uint length = 16;
            Require(getsockname(listener, address, &length) == 0 && length == 16, "local address");
            int port = address[2] * 256 + address[3];
            Require(port > 0, "ephemeral port");
            int backlog = Math.Max(requested, 0);
            Console.WriteLine($"READY port={port} backlog={backlog}"); Console.Out.Flush();
            Require(Console.Read() == 'g', "driver release");
            byte* ready = stackalloc byte[8];
            new Span<byte>(ready, 8).Clear();
            *(int*)ready = listener; *(short*)(ready + 4) = 1;
            Require(poll(ready, 1, 2000) == 1 && (*(short*)(ready + 6) & 1) != 0, "listener readiness");
            byte* peer = stackalloc byte[16]; length = 16;
            accepted = accept(listener, peer, &length);
            Require(accepted >= 0 && length == 16 && *(ushort*)peer == 2, "accept");
            Require(peer[4] == 127 && peer[5] == 0 && peer[6] == 0 && peer[7] == 1, "peer address");
            byte* message = stackalloc byte[2];
            Require(recv(accepted, message, 2, 0x100) == 2 && message[0] == 0x5a && message[1] == 0xa5, "receive bytes");
            Require(send(accepted, message, 2, 0) == 2, "send bytes");
            Require(close(accepted) == 0, "close accepted"); accepted = -1;
            Require(close(listener) == 0, "close listener"); listener = -1;
            errno = 0; Require(listen(-1, backlog) == -1 && errno == 9, "bad descriptor");
            datagram = socket(2, 2, 0); Require(datagram >= 0, "UDP socket");
            errno = 0; Require(listen(datagram, backlog) == -1 && errno == 95, "UDP rejected");
            Require(close(datagram) == 0, "close UDP"); datagram = -1;
            Console.WriteLine($"PASS backlog={backlog} echo=5aa5 badfd=9 datagram=95");
            return 0;
        }
        finally
        {
            if (accepted >= 0) close(accepted);
            if (datagram >= 0) close(datagram);
            if (listener >= 0) close(listener);
        }
    }
}
