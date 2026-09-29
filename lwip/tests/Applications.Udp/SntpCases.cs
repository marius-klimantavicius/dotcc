using System.Buffers.Binary;
using System.Text;
using System.Net;
using Managed.Networking;
using static Managed.Networking.Lwip;
using static UdpPeer;

internal static unsafe class SntpCases
{
    internal static void Run(VirtualPair pair)
    {
        const uint seconds = 3_999_999_123, fraction = 0x40000000; // Exactly one quarter second.
        using var server = new UdpPeer(pair.B, 123);
        LwipSntpClient? client = null;
        try
        {
            client = LwipSntpClient.Start(pair.A, IPAddress.Parse("10.0.0.2"));
            bool duplicate = false;
            try { LwipSntpClient.Start(pair.A, IPAddress.Parse("10.0.0.2")); }
            catch (InvalidOperationException) { duplicate = true; }
            Require(duplicate, "SNTP duplicate registration preserves existing client");
            var request = server.Next(pair);
            Require(request.Bytes.Length == 48 && (request.Bytes[0] & 7) == 3, "SNTP client request");
            using(var victim=new LwipStack())
            {
                victim.Start();victim.AddInterface("10.9.0.1","",new byte[]{2,9,0,0,0,1},_=>{});
                var other=LwipSntpClient.Start(victim,IPAddress.Parse("10.9.0.2"));
                var waiting=other.ReadTimeAsync().AsTask();victim.Dispose();
                Wait(pair,()=>waiting.IsCompleted);
                try{waiting.GetAwaiter().GetResult();throw new InvalidOperationException("Disposed owner SNTP read succeeded");}
                catch(ObjectDisposedException){}
                catch(System.Threading.Channels.ChannelClosedException){}
            }
            GC.Collect();GC.WaitForPendingFinalizers();
            byte[] response = new byte[48];
            response[0] = 0x24; // No leap warning, NTPv4 server.
            response[1] = 2; response[2] = 6; response[3] = 0xec;
            BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(8), 1 << 16); // Root dispersion: one second.
            "TEST"u8.CopyTo(response.AsSpan(12));
            BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(16), seconds - 1);
            request.Bytes.AsSpan(40, 8).CopyTo(response.AsSpan(24)); // Exact originate timestamp.
            BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(32), seconds);
            BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(36), fraction);
            BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(40), seconds);
            BinaryPrimitives.WriteUInt32BigEndian(response.AsSpan(44), fraction);
            server.Send("10.0.0.1", request.Port, response);
            LwipNtpTime after = default;
            Wait(pair, () => client.TryReadTime(out after));
            Require(after == new LwipNtpTime(seconds, fraction) && client.ObservedNotifications == 1, "original SNTP delivers exact product time notification");
            var pending = client.ReadTimeAsync().AsTask();
            client.Dispose();
            Wait(pair,()=>pending.IsCompleted);
            try { pending.GetAwaiter().GetResult(); throw new InvalidOperationException("Disposed SNTP read unexpectedly succeeded"); }
            catch (ObjectDisposedException) { }
            catch (System.Threading.Channels.ChannelClosedException) { }
            pair.A.Invoke(owner => Require(owner.sntp_enabled() == 0, "SNTP stopped"));
            pair.Drain();
            Require(client.ObservedNotifications == 1, "SNTP stop preserves final observation");
            LwipUdpApplications.ThrowErrors(pair.A.Core);
        }
        finally { client?.Dispose(); }
    }
}
