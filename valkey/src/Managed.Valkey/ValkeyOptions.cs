using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Managed.Valkey;

public enum ValkeyShutdownMode { Save, NoSave }
public enum ValkeyAppendFsync { Always, EverySecond, No }

/// <summary>Startup options for the translated standalone server.</summary>
public sealed record ValkeyOptions
{
    public required string DataDirectory { get; init; }
    public IPAddress BindAddress { get; init; } = IPAddress.Loopback;
    public int Port { get; init; } = 6379;
    public string? Password { get; init; }
    public string RdbFileName { get; init; } = "dump.rdb";
    /// <summary>Enable AOF from startup. Runtime enabling is outside this profile.</summary>
    public bool AppendOnly { get; init; }
    public ValkeyAppendFsync AppendFsync { get; init; } = ValkeyAppendFsync.Always;
    /// <summary>A failed SAVE leaves the server running and disposal faults.</summary>
    public ValkeyShutdownMode DisposeMode { get; init; } = ValkeyShutdownMode.Save;

    internal Snapshot Validate()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(DataDirectory);
        ArgumentNullException.ThrowIfNull(BindAddress);
        if (BindAddress.AddressFamily is not (AddressFamily.InterNetwork or AddressFamily.InterNetworkV6))
            throw new ArgumentException("BindAddress must be an IPv4 or IPv6 address.", nameof(BindAddress));
        if (Port is < 1 or > 65535)
            throw new ArgumentOutOfRangeException(nameof(Port), "Valkey requires a positive TCP port; port zero disables TCP.");
        if (!Enum.IsDefined(AppendFsync)) throw new ArgumentOutOfRangeException(nameof(AppendFsync));
        if (!Enum.IsDefined(DisposeMode)) throw new ArgumentOutOfRangeException(nameof(DisposeMode));
        ArgumentException.ThrowIfNullOrWhiteSpace(RdbFileName);
        if (RdbFileName is "." or ".." || RdbFileName.IndexOfAny(['/', '\\', '\0']) >= 0)
            throw new ArgumentException("RdbFileName must be a file name without directory separators or NUL.", nameof(RdbFileName));
        RejectNul(DataDirectory, nameof(DataDirectory));
        if (Password is not null) RejectNul(Password, nameof(Password));
        var address = BindAddress.AddressFamily == AddressFamily.InterNetworkV6
            ? new IPAddress(BindAddress.GetAddressBytes(), BindAddress.ScopeId)
            : new IPAddress(BindAddress.GetAddressBytes());
        var config = new StringBuilder()
            .Append("bind ").Append(Quote(address.ToString())).Append('\n')
            .Append("port ").Append(Port.ToString(CultureInfo.InvariantCulture)).Append('\n')
            .Append("dbfilename ").Append(Quote(RdbFileName)).Append('\n')
            .Append("appendonly ").Append(AppendOnly ? "yes" : "no").Append('\n')
            .Append("appendfsync ").Append(AppendFsync switch
            { ValkeyAppendFsync.Always => "always", ValkeyAppendFsync.EverySecond => "everysec", _ => "no" }).Append('\n')
            .Append("loglevel warning\n");
        if (Password is not null) config.Append("requirepass ").Append(Quote(Password)).Append('\n');
        // Strict UTF-8 rejects unmatched surrogates before passing C strings.
        var utf8 = new UTF8Encoding(false, true);
        string directory = Path.GetFullPath(DataDirectory);
        return new(directory, address, DisposeMode, utf8.GetBytes(directory + '\0'), utf8.GetBytes(config + "\0"));
    }

    private static void RejectNul(string value, string parameter)
    {
        if (value.Contains('\0')) throw new ArgumentException("C string options cannot contain NUL.", parameter);
    }

    private static string Quote(string value)
    {
        RejectNul(value, nameof(value));
        var result = new StringBuilder("\"");
        foreach (char character in value)
        {
            switch (character)
            {
                case '\\': result.Append("\\\\"); break;
                case '"': result.Append("\\\""); break;
                case '\n': result.Append("\\n"); break;
                case '\r': result.Append("\\r"); break;
                case '\t': result.Append("\\t"); break;
                default:
                    if (character < 32 || character == 127)
                        result.Append("\\x").Append(((int)character).ToString("x2", CultureInfo.InvariantCulture));
                    else result.Append(character);
                    break;
            }
        }
        return result.Append('"').ToString();
    }

    internal sealed record Snapshot(string Directory, IPAddress Address, ValkeyShutdownMode DisposeMode,
        byte[] DirectoryUtf8, byte[] ConfigurationUtf8);
}
