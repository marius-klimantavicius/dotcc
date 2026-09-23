using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using static Managed.Smb.LibSmb2;
using C = Managed.Smb.LibSmb2.Libc;

namespace Managed.Smb;

public static partial class HostSockets
{
    internal static unsafe int GetAddresses(byte* node, byte* service, addrinfo* hints, addrinfo** result)
    {
        if (result == null) return EAI_FAIL;
        *result = null;
        string? hostname = Marshal.PtrToStringUTF8((nint)node);
        var prepared = _prepared;
        if (_current is null || prepared is null || !StringComparer.OrdinalIgnoreCase.Equals(hostname, prepared.Hostname))
        { C.errno = C.EOPNOTSUPP; return EAI_FAIL; }
        string? portText = Marshal.PtrToStringUTF8((nint)service);
        if (!ushort.TryParse(portText, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out ushort port))
            return EAI_NONAME;
        int family = hints == null ? 0 : hints->ai_family;
        if (family != 0 && family != AF_INET && family != AF_INET6) return EAI_FAMILY;
        addrinfo* head = null;
        addrinfo* tail = null;
        try
        {
            foreach (var address in prepared.Addresses)
            {
                bool ipv6 = address.AddressFamily == AddressFamily.InterNetworkV6;
                if (!ipv6 && address.AddressFamily != AddressFamily.InterNetwork) continue;
                int af = ipv6 ? AF_INET6 : AF_INET;
                if (family != 0 && family != af) continue;
                var item = (addrinfo*)C.calloc(1, sizeof(addrinfo));
                if (item == null) throw new OutOfMemoryException();
                if (head == null) head = item; else tail->ai_next = item;
                tail = item;
                item->ai_family = af; item->ai_socktype = SOCK_STREAM; item->ai_protocol = IPPROTO_TCP;
                item->ai_addrlen = (uint)(ipv6 ? sizeof(sockaddr_in6) : sizeof(sockaddr_in));
                item->ai_addr = (sockaddr*)C.calloc(1, checked((int)item->ai_addrlen));
                if (item->ai_addr == null) throw new OutOfMemoryException();
                if (ipv6)
                {
                    var value = (sockaddr_in6*)item->ai_addr;
                    value->sin6_family = (ushort)AF_INET6;
                    BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(&value->sin6_port, 2), port);
                    address.GetAddressBytes().CopyTo(new Span<byte>(&value->sin6_addr, 16));
                    value->sin6_scope_id = checked((uint)address.ScopeId);
                }
                else
                {
                    var value = (sockaddr_in*)item->ai_addr;
                    value->sin_family = (ushort)AF_INET;
                    BinaryPrimitives.WriteUInt16BigEndian(new Span<byte>(&value->sin_port, 2), port);
                    address.GetAddressBytes().CopyTo(new Span<byte>(&value->sin_addr.s_addr, 4));
                }
            }
            if (head == null) return EAI_NONAME;
            *result = head;
            return 0;
        }
        catch (OutOfMemoryException) { FreeAddresses(head); return EAI_MEMORY; }
        catch (OverflowException) { FreeAddresses(head); return EAI_FAIL; }
    }
    internal static unsafe void FreeAddresses(addrinfo* value)
    {
        while (value != null)
        {
            var next = value->ai_next;
            C.free(value->ai_addr); C.free(value->ai_canonname); C.free(value);
            value = next;
        }
    }
}
