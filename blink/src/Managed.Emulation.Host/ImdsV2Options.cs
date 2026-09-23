using System.Net;
using System.Net.Sockets;

namespace Managed.Emulation.Host;

/// <summary>Explicit simulated instance metadata. No host or AWS credentials are discovered.</summary>
public sealed record ImdsV2Options
{
    public string InstanceId { get; init; } = "i-00000000000000000";
    public string AccountId { get; init; } = "000000000000";
    public string Region { get; init; } = "us-east-1";
    public string AvailabilityZone { get; init; } = "us-east-1a";
    public string Hostname { get; init; } = "blink.internal";
    public string PrivateAddress { get; init; } = "10.0.0.1";
    public string ImageId { get; init; } = "ami-00000000000000000";
    public string InstanceType { get; init; } = "t3.micro";
    public string? UserData { get; init; }
    public string? RoleName { get; init; }
    public string InstanceProfileId { get; init; } = "AIPABLINKSIMULATED0000";
    public ImdsRoleCredentials? Credentials { get; init; }

    /// <summary>Validate and copy the serializable configuration for one execution.</summary>
    public ImdsV2Options Snapshot()
    {
        foreach (string? value in new[] { InstanceId, AccountId, Region, AvailabilityZone, Hostname, PrivateAddress, ImageId, InstanceType, InstanceProfileId })
            if (string.IsNullOrWhiteSpace(value) || value.Length > 256 || value.Any(char.IsControl))
                throw new ArgumentException("Invalid IMDS metadata value.");
        if (AccountId.Length != 12 || AccountId.Any(c => c is < '0' or > '9') ||
            !IPAddress.TryParse(PrivateAddress, out var ip) || ip.AddressFamily != AddressFamily.InterNetwork ||
            UserData?.Length > 65536 || RoleName is { } role && (role.Length is < 1 or > 64 ||
                role.Any(c => !char.IsAsciiLetterOrDigit(c) && !"+=,.@_-".Contains(c))))
            throw new ArgumentException("Invalid IMDS identity, role or user data.");
        if (Credentials is { } credentials)
        {
            if (RoleName == null) throw new ArgumentException("IMDS credentials require a role name.");
            foreach (string? value in new[] { credentials.AccessKeyId, credentials.SecretAccessKey, credentials.Token })
                if (string.IsNullOrEmpty(value) || value.Length > 16384 || value.Any(char.IsControl))
                    throw new ArgumentException("Invalid explicitly supplied IMDS credentials.");
            if (credentials.Expiration == default) throw new ArgumentException("IMDS credentials require an expiration.");
        }
        return this with { Credentials = Credentials is null ? null : Credentials with { } };
    }
}

/// <summary>A caller-supplied credential response, including its actual expiration.</summary>
public sealed record ImdsRoleCredentials(string AccessKeyId, string SecretAccessKey, string Token, DateTimeOffset Expiration);
