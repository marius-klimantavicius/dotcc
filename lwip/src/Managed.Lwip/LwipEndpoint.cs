using System.Net;
using System.Net.Sockets;
using System.Buffers.Binary;
using static Managed.Networking.Lwip;
namespace Managed.Networking;

/// <summary>A copied IP address, port and numeric owner-relative lwIP IPv6 interface index (0..255). No OS DNS or interface mapping is performed.</summary>
public sealed class LwipEndpoint
{
    private readonly byte[] bytes;
    public ushort Port {get;}
    public uint Zone {get;}
    public bool IsIPv6=>bytes.Length==16;
    public IPAddress Address=>IsIPv6?new IPAddress(bytes,Zone):new IPAddress(bytes);
    public LwipEndpoint(IPAddress address,ushort port)
    {
        ArgumentNullException.ThrowIfNull(address);bytes=address.GetAddressBytes();
        if(bytes.Length!=4&&bytes.Length!=16)throw new ArgumentException("An IPv4 or IPv6 address is required.",nameof(address));
        if(bytes.Length==16){if(address.ScopeId<0||address.ScopeId>byte.MaxValue)throw new ArgumentOutOfRangeException(nameof(address),"lwIP interface zone indices fit in one byte.");Zone=(uint)address.ScopeId;}
        Port=port;
    }
    internal unsafe void WriteIp(ip_addr*target)
    {
        if(Zone>byte.MaxValue)throw new ArgumentOutOfRangeException(nameof(Zone),"lwIP interface zone indices fit in one byte.");
        *target=default;target->type=(byte)(IsIPv6?6:0);
        if(IsIPv6){bytes.CopyTo(new Span<byte>(&target->u_addr.ip6.addr[0],16));target->u_addr.ip6.zone=(byte)Zone;}
        else bytes.CopyTo(new Span<byte>(&target->u_addr.ip4.addr,4));
    }
    internal static unsafe LwipEndpoint ReadIp(ip_addr*source,ushort port)=>source->type==6
        ?new(new IPAddress(new ReadOnlySpan<byte>(&source->u_addr.ip6.addr[0],16),source->u_addr.ip6.zone),port)
        :new(new IPAddress(new ReadOnlySpan<byte>(&source->u_addr.ip4.addr,4)),port);
#if LWIP_HAS_SOCKETS
    internal unsafe uint Write(sockaddr_storage*target)
    {
        *target=default;
        if(IsIPv6){sockaddr_in6*p=(sockaddr_in6*)target;p->sin6_len=(byte)sizeof(sockaddr_in6);p->sin6_family=AF_INET6;p->sin6_port=BinaryPrimitives.ReverseEndianness(Port);p->sin6_scope_id=Zone;bytes.CopyTo(new Span<byte>(&p->sin6_addr,16));return (uint)sizeof(sockaddr_in6);}
        sockaddr_in*v4=(sockaddr_in*)target;v4->sin_len=(byte)sizeof(sockaddr_in);v4->sin_family=AF_INET;v4->sin_port=BinaryPrimitives.ReverseEndianness(Port);bytes.CopyTo(new Span<byte>(&v4->sin_addr,4));return (uint)sizeof(sockaddr_in);
    }
    internal static unsafe LwipEndpoint Read(sockaddr_storage*source,uint length)
    {
        if(source->ss_family==AF_INET6&&length>=sizeof(sockaddr_in6)){sockaddr_in6*p=(sockaddr_in6*)source;return new(new IPAddress(new ReadOnlySpan<byte>(&p->sin6_addr,16),p->sin6_scope_id),BinaryPrimitives.ReverseEndianness(p->sin6_port));}
        if(source->ss_family==AF_INET&&length>=sizeof(sockaddr_in)){sockaddr_in*p=(sockaddr_in*)source;return new(new IPAddress(new ReadOnlySpan<byte>(&p->sin_addr,4)),BinaryPrimitives.ReverseEndianness(p->sin_port));}
        throw new InvalidOperationException("Translated socket returned an invalid address family/length.");
    }
#endif
}
