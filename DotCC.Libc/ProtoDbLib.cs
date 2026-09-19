#nullable enable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    /// <summary>Look up explicitly supported Internet protocols by canonical
    /// name or standard alias. Returned Linux LP64 protoent records are borrowed
    /// stable storage owned by the runtime; unknown names return null. This is
    /// a portable protocol table, not an OS-specific /etc/protocols reader.</summary>
    public static void* getprotobyname(byte* name)
    {
        if (name == null) { errno = EINVAL; return null; }
        var text = Str(name);
        for (int i = 0; i < ProtocolDatabase.Entries.Length; ++i)
        {
            var entry = ProtocolDatabase.Entries[i];
            if (text == entry.Name || text == entry.Alias) return ProtocolDatabase.Pointer + i * ProtocolDatabase.Stride;
        }
        return null;
    }

    private static class ProtocolDatabase
    {
        internal static readonly (string Name, string Alias, int Number)[] Entries =
        [
            ("ip", "IP", 0), ("icmp", "ICMP", 1), ("igmp", "IGMP", 2),
            ("tcp", "TCP", 6), ("udp", "UDP", 17), ("ipv6", "IPv6", 41),
            ("ipv6-icmp", "IPv6-ICMP", 58),
        ];
        internal const int Stride = 80;
        private static readonly byte[] Storage;
        internal static readonly byte* Pointer;

        static ProtocolDatabase()
        {
            Storage = GC.AllocateUninitializedArray<byte>(Entries.Length * Stride, pinned: true);
            Storage.AsSpan().Clear();
            Pointer = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetArrayDataReference(Storage));
            for (int i = 0; i < Entries.Length; ++i)
            {
                byte* record = Pointer + i * Stride;
                byte* name = record + 40;
                int length = Encoding.UTF8.GetBytes(Entries[i].Name, new Span<byte>(name, Stride - 40));
                byte* alias = name + length + 1;
                Encoding.UTF8.GetBytes(Entries[i].Alias, new Span<byte>(alias, Stride - 41 - length));
                // protoent: p_name at 0, p_aliases at 8, p_proto at 16,
                // size 24, alignment 8. Two alias pointers follow the record.
                *(byte**)record = name;
                *(byte***)(record + 8) = (byte**)(record + 24);
                *(int*)(record + 16) = Entries[i].Number;
                *(byte**)(record + 24) = alias;
            }
        }
    }
}
