using static Managed.Smb.LibSmb2;

namespace Managed.Smb;

public sealed partial class SmbConnection
{
    public static SmbConnection ConnectKerberos(string server, string share, string principal,
        string password, string realm, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
        => ConnectKerberosAsync(server, share, principal, password, realm, dialect, encrypt, timeoutSeconds).GetAwaiter().GetResult();

    public static Task<SmbConnection> ConnectKerberosAsync(string server, string share, string principal,
        string password, string realm, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        return ConnectKerberosCoreAsync(server, share, principal, password, realm, null, null,
            dialect, encrypt, timeoutSeconds, cancellationToken);
    }

    public static SmbConnection ConnectKerberosWithExistingCredentials(string server, string share,
        string? user = null, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
        => ConnectKerberosWithExistingCredentialsAsync(server, share, user, dialect, encrypt, timeoutSeconds).GetAwaiter().GetResult();

    public static Task<SmbConnection> ConnectKerberosWithExistingCredentialsAsync(string server, string share,
        string? user = null, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
        => ConnectKerberosCoreAsync(server, share, user ?? Environment.UserName, null, null, null, null,
            dialect, encrypt, timeoutSeconds, cancellationToken);

    public static SmbConnection ConnectKerberosWithKeytab(string server, string share, string principal,
        string realm, string keytabPath, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
        => ConnectKerberosWithKeytabAsync(server, share, principal, realm, keytabPath, dialect, encrypt, timeoutSeconds).GetAwaiter().GetResult();

    public static Task<SmbConnection> ConnectKerberosWithKeytabAsync(string server, string share, string principal,
        string realm, string keytabPath, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principal);
        ArgumentException.ThrowIfNullOrWhiteSpace(realm);
        ArgumentException.ThrowIfNullOrWhiteSpace(keytabPath);
        return ConnectKerberosCoreAsync(server, share, principal, null, realm, keytabPath, null,
            dialect, encrypt, timeoutSeconds, cancellationToken);
    }

    public static SmbConnection ConnectKerberosWithCredentialCache(string server, string share,
        string credentialCachePath, string? user = null, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10)
        => ConnectKerberosWithCredentialCacheAsync(server, share, credentialCachePath, user, dialect, encrypt, timeoutSeconds).GetAwaiter().GetResult();

    public static Task<SmbConnection> ConnectKerberosWithCredentialCacheAsync(string server, string share,
        string credentialCachePath, string? user = null, ushort dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0311,
        bool encrypt = false, int timeoutSeconds = 10, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(credentialCachePath);
        return ConnectKerberosCoreAsync(server, share, user ?? Environment.UserName, null, null, null,
            credentialCachePath, dialect, encrypt, timeoutSeconds, cancellationToken);
    }

    private static async Task<SmbConnection> ConnectKerberosCoreAsync(string server, string share,
        string user, string? password, string? realm, string? keytab, string? cache, ushort dialect,
        bool encrypt, int timeoutSeconds, CancellationToken token)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(timeoutSeconds, 1);
        using var prepared = await PrepareKerberosAsync(DnsName(server), user, password, realm, keytab, cache, token).ConfigureAwait(false);
        return await ConnectCoreAsync(server, share, user, null, realm, dialect, encrypt, timeoutSeconds,
            token, smb2_sec.SMB2_SEC_KRB5, prepared).ConfigureAwait(false);
    }
}
