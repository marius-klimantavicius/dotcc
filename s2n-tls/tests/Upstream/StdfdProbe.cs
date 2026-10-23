using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Managed.Security.S2n;

public static unsafe partial class S2nTls
{
    public static unsafe partial class Libc
    {
        public static int StdfdProbeRegister(Socket socket) => RegisterSocketSlot(socket);
    }
}

// Subprocess-only byte/descriptor contract, against the actual embedded table.
internal static unsafe class StdfdProbe
{
    [DllImport("libc.so.6", EntryPoint = "close")]
    private static extern int CloseHostDescriptor(int descriptor);
    [StructLayout(LayoutKind.Sequential)]
    private struct PollFd { public int Fd; public short Events; public short Returned; }
    private static int Main(string[] arguments)
    {
        using var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        listener.Bind(new IPEndPoint(IPAddress.Loopback, 0)); listener.Listen(1);
        using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sender.Connect(listener.LocalEndPoint!);
        using var receiver = listener.Accept();
        int sendFd = S2nTls.Libc.StdfdProbeRegister(sender), receiveFd = S2nTls.Libc.StdfdProbeRegister(receiver);
        try
        {
            byte value = 69, incoming = 0;
            if (S2nTls.UpstreamWrite(sendFd, &value, 1) != 1) return 2;
            PollFd* entries = stackalloc PollFd[2];
            entries[0] = new() { Fd = 0, Events = 1 }; entries[1] = new() { Fd = receiveFd, Events = 1 };
            int ready = S2nTls.UpstreamPoll(entries, 2, 1000);
            Console.Error.WriteLine($"ready {ready} {entries[0].Returned} {entries[1].Returned}");
            uint available = 0;
            int queried = S2nTls.Libc.ioctl(0, 0x541b, &available);
            Console.Error.WriteLine($"available {queried} {available}");
            long read = S2nTls.UpstreamRead(receiveFd, &incoming, 1);
            Console.Error.WriteLine($"socket {read} {incoming}");
            byte* bytes = stackalloc byte[3];
            while (true)
            {
                long count = S2nTls.UpstreamRead(0, bytes, 3);
                Console.Error.WriteLine($"read {count}");
                if (count < 0) return 3;
                if (count == 0) break;
                for (long sent = 0; sent < count;)
                {
                    long n = S2nTls.UpstreamWrite(1, bytes + sent, (ulong)(count - sent));
                    if (n <= 0) return 4;
                    sent += n;
                }
            }
            entries[0] = new() { Fd = 0, Events = 1 };
            Console.Error.WriteLine($"eof {S2nTls.UpstreamRead(0, bytes, 3)}");
            ready = S2nTls.UpstreamPoll(entries, 1, 0);
            Console.Error.WriteLine($"hangup {ready} {entries[0].Returned}");
            entries[0] = new() { Fd = receiveFd, Events = 1 };
            Console.Error.WriteLine($"timeout {S2nTls.UpstreamPoll(entries, 1, 20)} {entries[0].Returned}");
            entries[0] = new() { Fd = -1, Events = 1 }; entries[1] = new() { Fd = 999999, Events = 1 };
            S2nTls.Libc.errno = 77;
            ready = S2nTls.UpstreamPoll(entries, 2, 1000);
            Console.Error.WriteLine($"invalid {ready} {entries[0].Returned} {entries[1].Returned} {S2nTls.Libc.errno}");
            ready = S2nTls.UpstreamPoll(null, 1, 0);
            Console.Error.WriteLine($"fault {ready} {S2nTls.Libc.errno}");
            long result = S2nTls.UpstreamRead(999999, bytes, 1);
            Console.Error.WriteLine($"bad-read {result} {S2nTls.Libc.errno}");
            nint library = NativeLibrary.Load(arguments[0]);
            var interrupt = (delegate* unmanaged[Cdecl]<int>)NativeLibrary.GetExport(library, "start_interrupt");
            var finish = (delegate* unmanaged[Cdecl]<void>)NativeLibrary.GetExport(library, "finish_interrupt");
            if (interrupt() != 0) return 5;
            ready = S2nTls.UpstreamPoll(null, 0, 1000);
            int error = S2nTls.Libc.errno;
            finish();
            Console.Error.WriteLine($"interrupt {ready} {error}");
            if (CloseHostDescriptor(0) != 0) return 6;
            result = S2nTls.UpstreamRead(0, bytes, 1);
            Console.Error.WriteLine($"closed-read {result} {S2nTls.Libc.errno}");
            entries[0] = new() { Fd = 0, Events = 1 };
            ready = S2nTls.UpstreamPoll(entries, 1, 0);
            Console.Error.WriteLine($"closed-poll {ready} {entries[0].Returned}");
            return 0;
        }
        finally { S2nTls.Libc.close(sendFd); S2nTls.Libc.close(receiveFd); }
    }
}
