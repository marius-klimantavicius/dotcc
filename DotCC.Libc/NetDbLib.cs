#nullable enable
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ResolverInfo
    {
        public int Flags, Family, SocketType, Protocol;
        public uint AddressLength;
        public byte* Address;
        public byte* CanonicalName;
        public ResolverInfo* Next;
    }

    /// <summary>Resolve BCL addresses into caller-owned Linux LP64 addrinfo
    /// nodes. Each node, address and optional canonical name share one allocation.</summary>
    public static int getaddrinfo(byte* node, byte* service, void* hints, void* result)
    {
        const int passive = 1, canon = 2, numericHost = 4, mapped = 8, all = 16,
            configured = 32, numericService = 1024;
        if (result == null) return -4;
        *(void**)result = null;
        var hint = hints == null ? default : *(ResolverInfo*)hints;
        if ((hint.Flags & ~(passive | canon | numericHost | mapped | all | configured | numericService)) != 0)
            return -1;
        if (hint.Family != 0 && hint.Family != AF_INET && hint.Family != AF_INET6) return -6;
        if (hint.SocketType != 0 && hint.SocketType != SOCK_STREAM && hint.SocketType != SOCK_DGRAM) return -7;
        if (hint.Protocol != 0 && hint.Protocol != IPPROTO_TCP && hint.Protocol != IPPROTO_UDP) return -8;
        if ((hint.SocketType == SOCK_STREAM && hint.Protocol == IPPROTO_UDP) ||
            (hint.SocketType == SOCK_DGRAM && hint.Protocol == IPPROTO_TCP)) return -7;
        if (node == null && service == null) return -2;
        string host = Str(node), serviceName = Str(service);
        int port = 0;
        if (service != null && !int.TryParse(serviceName, global::System.Globalization.NumberStyles.None,
            global::System.Globalization.CultureInfo.InvariantCulture, out port))
        {
            if ((hint.Flags & numericService) != 0) return -2;
            // Explicitly supported well-known services; unknown service names
            // return EAI_SERVICE rather than probing a native resolver library.
            port = serviceName switch { "http" => 80, "https" => 443, "domain" => 53,
                "ssh" => 22, "microsoft-ds" => 445, "netbios-ssn" => 139, _ => -1 };
        }
        if (port < 0 || port > 65535) return -8;
        ResolverInfo* head = null;
        ResolverInfo* tail = null;
        try
        {
            IPAddress[] addresses;
            string canonicalHost = host;
            if (node == null)
                addresses = (hint.Flags & passive) != 0
                    ? [IPAddress.IPv6Any, IPAddress.Any] : [IPAddress.IPv6Loopback, IPAddress.Loopback];
            else if (IPAddress.TryParse(host, out var parsedAddress)) addresses = [parsedAddress];
            else
            {
                if ((hint.Flags & numericHost) != 0) return -2;
                if ((hint.Flags & canon) != 0)
                {
                    var entry = Dns.GetHostEntry(host);
                    canonicalHost = entry.HostName;
                    addresses = entry.AddressList;
                }
                else addresses = Dns.GetHostAddresses(host);
            }
            bool ipv4 = true, ipv6 = true;
            if ((hint.Flags & configured) != 0)
            {
                ipv4 = false; ipv6 = false;
                foreach (var network in global::System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (network.OperationalStatus != global::System.Net.NetworkInformation.OperationalStatus.Up ||
                        network.NetworkInterfaceType == global::System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;
                    foreach (var unicast in network.GetIPProperties().UnicastAddresses)
                    {
                        if (unicast.Address.AddressFamily == AddressFamily.InterNetwork) ipv4 = true;
                        if (unicast.Address.AddressFamily == AddressFamily.InterNetworkV6) ipv6 = true;
                    }
                }
            }
            bool hasV6 = Array.Exists(addresses, a => a.AddressFamily == AddressFamily.InterNetworkV6);
            var seen = new HashSet<IPAddress>();
            foreach (var original in addresses)
            {
                var address = original;
                if (original.AddressFamily == AddressFamily.InterNetwork && !ipv4 ||
                    original.AddressFamily == AddressFamily.InterNetworkV6 && !ipv6) continue;
                if (hint.Family == AF_INET6 && original.AddressFamily == AddressFamily.InterNetwork &&
                    (hint.Flags & mapped) != 0 && (!hasV6 || (hint.Flags & all) != 0)) address = original.MapToIPv6();
                int family = address.AddressFamily == AddressFamily.InterNetwork ? AF_INET : AF_INET6;
                if (hint.Family != 0 && family != hint.Family || !seen.Add(address)) continue;
                foreach (int kind in new[] { SOCK_STREAM, SOCK_DGRAM })
                {
                    int protocol = kind == SOCK_STREAM ? IPPROTO_TCP : IPPROTO_UDP;
                    if (hint.SocketType != 0 && hint.SocketType != kind || hint.Protocol != 0 && hint.Protocol != protocol) continue;
                    uint length = family == AF_INET ? 16U : 28U;
                    byte[]? canonical = (hint.Flags & canon) != 0 && head == null && node != null
                        ? Encoding.UTF8.GetBytes(canonicalHost) : null;
                    var bytes = address.GetAddressBytes();
                    int size = checked(sizeof(ResolverInfo) + (int)length + (canonical?.Length + 1 ?? 0));
                    var current = (ResolverInfo*)calloc(1, size);
                    if (current == null) { freeaddrinfo(head); return -10; }
                    current->Flags = hint.Flags;
                    current->Family = family;
                    current->SocketType = kind;
                    current->Protocol = protocol;
                    current->AddressLength = length;
                    current->Address = (byte*)(current + 1);
                    *(ushort*)current->Address = (ushort)family;
                    current->Address[2] = (byte)(port >> 8);
                    current->Address[3] = (byte)port;
                    bytes.AsSpan().CopyTo(new Span<byte>(current->Address + (family == AF_INET ? 4 : 8), bytes.Length));
                    if (family == AF_INET6) *(uint*)(current->Address + 24) = (uint)address.ScopeId;
                    if (canonical != null)
                    {
                        current->CanonicalName = current->Address + length;
                        canonical.AsSpan().CopyTo(new Span<byte>(current->CanonicalName, canonical.Length));
                    }
                    if (tail == null) head = current; else tail->Next = current;
                    tail = current;
                }
            }
            if (head == null) return -2;
            *(void**)result = head;
            return 0;
        }
        catch (SocketException ex)
        {
            freeaddrinfo(head);
            return ex.SocketErrorCode is SocketError.HostNotFound or SocketError.NoData ? -2 :
                ex.SocketErrorCode == SocketError.TryAgain ? -3 : -4;
        }
        catch (OutOfMemoryException) { freeaddrinfo(head); return -10; }
        catch (OverflowException) { freeaddrinfo(head); return -10; }
        catch (ArgumentException) { freeaddrinfo(head); return -2; }
        catch (global::System.Net.NetworkInformation.NetworkInformationException)
        { freeaddrinfo(head); errno = EIO; return -11; }
    }

    public static void freeaddrinfo(void* result)
    {
        var current = (ResolverInfo*)result;
        while (current != null)
        {
            var next = current->Next;
            free(current);
            current = next;
        }
    }

    // Borrowed, immutable process-lifetime messages, like strerror(). They are
    // independent of the caller-owned addrinfo allocation/free domain.
    private static readonly nint[] ResolverMessages = CreateResolverMessages();
    private static nint[] CreateResolverMessages()
    {
        string[] messages = ["Success", "Bad resolver flags", "Name or service not known", "Temporary resolver failure",
            "Resolver failure", "No address data", "Address family unsupported", "Socket type unsupported",
            "Service unsupported", "Address family unavailable", "Resolver out of memory", "Resolver system error",
            "Resolver result overflow", "Unknown resolver error"];
        var pointers = new nint[messages.Length];
        for (int i = 0; i < messages.Length; i++)
            pointers[i] = Marshal.StringToCoTaskMemUTF8(messages[i]);
        return pointers;
    }
    public static byte* gai_strerror(int error) => (byte*)ResolverMessages[error <= 0 && error >= -12 ? -error : 13];
}
