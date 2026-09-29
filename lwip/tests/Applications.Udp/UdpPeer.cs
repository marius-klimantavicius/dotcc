using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Networking;
using static Managed.Networking.Lwip;

// A fixture peer sends application payloads through original translated UDP/IP.
// It does not implement or replace any product application service.
internal sealed unsafe class UdpPeer : IDisposable
{
    internal sealed record Datagram(byte[] Bytes, ushort Port);
    private readonly LwipStack stack;
    private readonly GCHandle root;
    private readonly ConcurrentQueue<Datagram> received = new();
    private nint pcb;
    private Exception? error;

    internal UdpPeer(LwipStack stack, ushort port)
    {
        this.stack = stack;
        root = GCHandle.Alloc(this);
        try
        {
            stack.Invoke(owner =>
            {
                pcb = (nint)owner.udp_new_ip_type(0);
                Require(pcb != 0, "UDP peer allocation");
                ip_addr any = default;
                Require(owner.udp_bind((udp_pcb*)pcb, &any, port) == 0, "UDP peer bind");
                owner.udp_recv((udp_pcb*)pcb, &Receive, (void*)GCHandle.ToIntPtr(root));
            });
        }
        catch { if (pcb != 0) stack.Invoke(owner => owner.udp_remove((udp_pcb*)pcb)); root.Free(); throw; }
    }

    private static void Receive(Lwip owner, void* argument, udp_pcb* pcb, pbuf* packet, ip_addr* source, ushort port)
    {
        var self = (UdpPeer)GCHandle.FromIntPtr((nint)argument).Target!;
        try
        {
            Require(ReferenceEquals(owner, self.stack.Core), "UDP callback owner");
            byte[] bytes = new byte[packet->tot_len];
            fixed (byte* data = bytes)
                Require(owner.pbuf_copy_partial(packet, data, packet->tot_len, 0) == packet->tot_len, "UDP payload copy");
            self.received.Enqueue(new(bytes, port));
        }
        catch (Exception failure) { self.error = failure; }
        finally { owner.pbuf_free(packet); }
    }

    internal void Send(string destination, ushort port, byte[] bytes)
    {
        Require(bytes.Length <= ushort.MaxValue, "UDP fixture payload bound");
        byte[] addressText = Encoding.ASCII.GetBytes(destination + "\0");
        stack.Invoke(owner =>
        {
            ip_addr address;
            fixed (byte* text = addressText) Require(owner.ipaddr_aton(text, &address) == 1, "UDP address parse");
            var packet = owner.pbuf_alloc(pbuf_layer.PBUF_TRANSPORT, (ushort)bytes.Length, pbuf_type.PBUF_RAM);
            Require(packet != null, "UDP fixture pbuf allocation");
            try
            {
                fixed (byte* data = bytes) Require(owner.pbuf_take(packet, data, (ushort)bytes.Length) == 0, "UDP fixture payload write");
                Require(owner.udp_sendto((udp_pcb*)pcb, packet, &address, port) == 0, "UDP fixture send");
            }
            finally { owner.pbuf_free(packet); }
        });
    }

    internal Datagram Next(VirtualPair pair)
    {
        Datagram? result = null;
        Wait(pair, () => { if (error != null) throw error; return received.TryDequeue(out result); });
        return result!;
    }

    internal static void Wait(VirtualPair pair, Func<bool> completed)
    {
        var deadline = Stopwatch.StartNew();
        while (true)
        {
            pair.Check(); pair.Drain();
            if (completed()) return;
            Require(deadline.Elapsed < TimeSpan.FromSeconds(10), "bounded application progress");
            Thread.Sleep(1);
        }
    }

    internal static void Require(bool value, string message)
    { if (!value) throw new InvalidOperationException(message); }

    public void Dispose()
    {
        if (pcb == 0) return;
        stack.Invoke(owner => { owner.udp_recv((udp_pcb*)pcb, null, null); owner.udp_remove((udp_pcb*)pcb); pcb = 0; });
        root.Free();
        if (error != null) throw error;
    }
}
