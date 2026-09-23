using System.Net;
using System.Net.Sockets;

namespace Managed.Emulation.Host;

public sealed record GuestPortGrant(ushort GuestPort, string HostAddress = "127.0.0.1", int HostPort = 0);
public sealed record GuestOutboundGrant(string Address, ushort Port);

/// <summary>Immutable numeric IPv4/TCP grants. Empty policy denies network use.
/// These mediate guest calls; they do not sandbox the embedding application's own sockets.</summary>
public sealed class GuestNetworkPolicy
{
    public static GuestNetworkPolicy Isolated { get; } = new();
    public IReadOnlyList<GuestPortGrant> Publications { get; }
    public IReadOnlyList<GuestOutboundGrant> Outbound { get; }
    public ImdsV2Options? Metadata { get; }
    internal readonly Dictionary<ushort, IPEndPoint> Listening = new();
    internal readonly HashSet<GuestEndpoint> Destinations = new();
    public GuestNetworkPolicy(IEnumerable<GuestPortGrant>? publications = null, IEnumerable<GuestOutboundGrant>? outbound = null,
        ImdsV2Options? metadata = null)
    {
        var incoming = publications?.ToArray() ?? Array.Empty<GuestPortGrant>();
        var outgoing = outbound?.ToArray() ?? Array.Empty<GuestOutboundGrant>();
        foreach (var grant in incoming)
        {
            ArgumentNullException.ThrowIfNull(grant);
            if (grant.HostPort is < 0 or > 65535) throw new ArgumentOutOfRangeException(nameof(publications));
            var address = Address(grant.HostAddress);
            if (!Listening.TryAdd(grant.GuestPort, new(address, grant.HostPort)))
                throw new ArgumentException("Each guest port requires one unambiguous publication grant.", nameof(publications));
        }
        foreach (var grant in outgoing)
        {
            ArgumentNullException.ThrowIfNull(grant);
            if (grant.Port == 0) throw new ArgumentOutOfRangeException(nameof(outbound));
            byte[] bytes = Address(grant.Address).GetAddressBytes();
            uint address = System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(bytes);
            if (!Destinations.Add(new(address, grant.Port))) throw new ArgumentException("Duplicate outbound grant.", nameof(outbound));
        }
        Publications = Array.AsReadOnly(incoming);
        Outbound = Array.AsReadOnly(outgoing);
        Metadata = metadata?.Snapshot();
    }
    private static IPAddress Address(string value)
    {
        if (!IPAddress.TryParse(value, out var address) || address.AddressFamily != AddressFamily.InterNetwork)
            throw new ArgumentException("Network grants require numeric IPv4 addresses.");
        return address;
    }
}
