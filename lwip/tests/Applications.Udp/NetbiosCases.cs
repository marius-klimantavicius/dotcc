using System.Buffers.Binary;
using System.Text;
using Managed.Networking;
using static UdpPeer;

internal static unsafe class NetbiosCases
{
    internal static void Run(VirtualPair pair)
    {
        const ushort transaction = 0x4c57;
        using var peer = new UdpPeer(pair.A, 31137);
        bool started = false;
        try
        {
            pair.B.Invoke(owner =>
            {
                fixed (byte* name = Encoding.ASCII.GetBytes("DOTCCUDP\0")) owner.netbiosns_set_name(name);
                owner.netbiosns_init(); started = true;
            });
            // RFC 1002 first-level name encoding: 15 padded uppercase characters
            // and the workstation suffix, followed by an empty scope label.
            byte[] nameBytes = Enumerable.Repeat((byte)' ', 16).ToArray();
            "DOTCCUDP"u8.CopyTo(nameBytes); nameBytes[15] = 0;
            byte[] query = new byte[50];
            BinaryPrimitives.WriteUInt16BigEndian(query, transaction);
            BinaryPrimitives.WriteUInt16BigEndian(query.AsSpan(4), 1);
            query[12] = 32;
            for (int i = 0; i < 16; i++)
            {
                query[13 + 2 * i] = (byte)('A' + (nameBytes[i] >> 4));
                query[14 + 2 * i] = (byte)('A' + (nameBytes[i] & 15));
            }
            BinaryPrimitives.WriteUInt16BigEndian(query.AsSpan(46), 0x20); // NB address query.
            BinaryPrimitives.WriteUInt16BigEndian(query.AsSpan(48), 1);
            peer.Send("10.0.0.2", 137, query);
            var reply = peer.Next(pair);
            Require(reply.Port == 137 && reply.Bytes.Length == 62, "NetBIOS response port/size");
            Require(BinaryPrimitives.ReadUInt16BigEndian(reply.Bytes) == transaction, "NetBIOS transaction");
            ushort flags = BinaryPrimitives.ReadUInt16BigEndian(reply.Bytes.AsSpan(2));
            Require((flags & 0x800f) == 0x8000, "NetBIOS successful response");
            Require(BinaryPrimitives.ReadUInt16BigEndian(reply.Bytes.AsSpan(6)) == 1, "NetBIOS answer count");
            Require(reply.Bytes.AsSpan(12, 34).SequenceEqual(query.AsSpan(12, 34)), "NetBIOS returned name");
            Require(BinaryPrimitives.ReadUInt16BigEndian(reply.Bytes.AsSpan(46)) == 0x20, "NetBIOS answer type");
            Require(BinaryPrimitives.ReadUInt16BigEndian(reply.Bytes.AsSpan(54)) == 6, "NetBIOS RDATA length");
            Require(reply.Bytes.AsSpan(58, 4).SequenceEqual(new byte[] { 10, 0, 0, 2 }), "NetBIOS owned interface address");
        }
        finally { if (started) pair.B.Invoke(owner => owner.netbiosns_stop()); }
    }
}
