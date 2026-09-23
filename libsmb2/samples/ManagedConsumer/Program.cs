using Managed.Smb;

if (args.Length == 0 || args.Contains("--help"))
{
    Console.WriteLine("Usage: ManagedConsumer list UNC_PATH [--ccache PATH | --keytab PATH --principal NAME --realm REALM] [--encrypt]");
    Console.WriteLine("       ManagedConsumer cat UNC_PATH [--ccache PATH | --keytab PATH --principal NAME --realm REALM] [--encrypt]");
    Console.WriteLine("Usage: ManagedConsumer SERVER[:PORT] SHARE USER [DIALECT_HEX] [--encrypt]");
    Console.WriteLine("DFS list/cat use the current Windows credentials or KRB5CCNAME FILE cache.");
    Console.WriteLine("Set LIBSMB2_AUTH=ntlmssp|kerberos (default ntlmssp), LIBSMB2_PASSWORD, and LIBSMB2_DOMAIN.");
    Console.WriteLine("Kerberos without LIBSMB2_PASSWORD uses SSPI on Windows or the FILE cache named by KRB5CCNAME elsewhere.");
    Console.WriteLine("The sample creates, verifies and deletes its own temporary file.");
    return 0;
}

if (args[0] is "list" or "cat")
{
    string? keytab = Option(args, "--keytab");
    string? ccache = Option(args, "--ccache");
    string? principal = Option(args, "--principal");
    string? realm = Option(args, "--realm");
    var known = new HashSet<string> { "--encrypt", "--ccache", "--keytab", "--principal", "--realm" };
    bool invalid = args.Length < 2;
    for (int i = 2; i < args.Length && !invalid; i++)
    {
        invalid = !known.Contains(args[i]);
        if (args[i] is "--ccache" or "--keytab" or "--principal" or "--realm")
        {
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--") ||
                Array.IndexOf(args, args[i]) != i) invalid = true;
            i++;
        }
    }
    if (invalid || keytab != null && (string.IsNullOrWhiteSpace(principal) || string.IsNullOrWhiteSpace(realm)) ||
        keytab == null && (principal != null || realm != null) || keytab != null && ccache != null)
    {
        Console.Error.WriteLine("Usage: ManagedConsumer list|cat UNC_PATH [--ccache PATH | --keytab PATH --principal NAME --realm REALM] [--encrypt]");
        return 2;
    }
    try
    {
        await using var client = ccache != null
            ? SmbDfsClient.CreateKerberosWithCredentialCache(ccache,
                encrypt: args.Contains("--encrypt"))
            : keytab != null
                ? SmbDfsClient.CreateKerberosWithKeytab(principal!, realm!, keytab,
                    encrypt: args.Contains("--encrypt"))
                : SmbDfsClient.CreateKerberosWithExistingCredentials(
                    encrypt: args.Contains("--encrypt"));
        if (args[0] == "list")
        {
            foreach (var entry in await client.ListAsync(args[1]))
                Console.WriteLine($"{(entry.IsDirectory ? 'd' : '-')} {entry.Size,12} {entry.Name}");
        }
        else
        {
            await using var file = await client.OpenReadAsync(args[1]);
            using Stream output = Console.OpenStandardOutput();
            var buffer = new byte[64 * 1024];
            ulong offset = 0;
            int count;
            while ((count = await file.ReadAsync(buffer, offset)) != 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, count));
                offset += checked((uint)count);
            }
        }
        return 0;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine(Environment.GetEnvironmentVariable("LIBSMB2_TRACE") == "1" ? ex.ToString() : ex.Message);
        return 1;
    }
}

static string? Option(string[] values, string name)
{
    int index = Array.IndexOf(values, name);
    return index < 0 || index + 1 >= values.Length ? null : values[index + 1];
}

if (args.Length < 3) { Console.Error.WriteLine("Server, share and user are required; use --help."); return 2; }
var password = Environment.GetEnvironmentVariable("LIBSMB2_PASSWORD");
var authentication = Environment.GetEnvironmentVariable("LIBSMB2_AUTH") ?? "ntlmssp";
var domain = Environment.GetEnvironmentVariable("LIBSMB2_DOMAIN");
if (authentication == "ntlmssp" && password is null) { Console.Error.WriteLine("Set LIBSMB2_PASSWORD."); return 2; }
if (authentication == "kerberos" && password is not null && string.IsNullOrWhiteSpace(domain))
{
    Console.Error.WriteLine("Set LIBSMB2_DOMAIN to the Kerberos realm when using a password.");
    return 2;
}
if (authentication is not ("ntlmssp" or "kerberos"))
{
    Console.Error.WriteLine("LIBSMB2_AUTH must be ntlmssp or kerberos.");
    return 2;
}
try
{
    ushort dialect = args.Length > 3 && !args[3].StartsWith("--") ? Convert.ToUInt16(args[3], 16)
        : (ushort)LibSmb2.smb2_negotiate_version.SMB2_VERSION_0311;
    await using var client = authentication == "kerberos"
        ? password is null
            ? await SmbConnection.ConnectKerberosWithExistingCredentialsAsync(args[0], args[1], args[2], dialect,
                args.Contains("--encrypt"))
            : await SmbConnection.ConnectKerberosAsync(args[0], args[1], args[2], password, domain!, dialect,
                args.Contains("--encrypt"))
        : await SmbConnection.ConnectAsync(args[0], args[1], args[2], password!, domain ?? "WORKGROUP", dialect,
            args.Contains("--encrypt"));
    var entries = await client.ListAsync();
    var path = "dotcc-sample-" + Guid.NewGuid().ToString("N") + ".bin";
    bool created = false;
    try
    {
        await using (var file = await client.OpenAsync(path, create: true))
        {
            created = true;
            if (await file.ReadAsync(Array.Empty<byte>()) != 0 || await file.WriteAsync(Array.Empty<byte>()) != 0)
                throw new IOException("Empty I/O must return zero");
            var payload = Enumerable.Range(0, 65537).Select(i => (byte)(i * 31 + 17)).ToArray();
            int offset = 0;
            while (offset < payload.Length)
            {
                int written = await file.WriteAsync(payload[offset..], (ulong)offset);
                if (written == 0) throw new IOException("Write made no progress");
                offset += written;
            }
            await file.FlushAsync();
            if ((await file.StatAsync()).Size != (ulong)payload.Length) throw new IOException("File length differs");
            var actual = new byte[payload.Length];
            offset = 0;
            while (offset < actual.Length)
            {
                var part = new byte[actual.Length - offset];
                int count = await file.ReadAsync(part, (ulong)offset);
                if (count == 0) throw new IOException("Unexpected EOF");
                part.AsSpan(0, count).CopyTo(actual.AsSpan(offset));
                offset += count;
            }
            if (!payload.AsSpan().SequenceEqual(actual)) throw new IOException("Read-back bytes differ");
            if (await file.ReadAsync(new byte[1], (ulong)actual.Length) != 0) throw new IOException("Expected EOF");
            await file.TruncateAsync(17);
            if ((await file.StatAsync()).Size != 17) throw new IOException("Truncated length differs");
        }
        await client.RenameAsync(path, path + ".renamed");
        path += ".renamed";
        if ((await client.StatAsync(path)).Size != 17 || !(await client.ListAsync()).Any(entry => entry.Name == path))
            throw new IOException("Renamed file missing or length differs");
        await client.DeleteAsync(path);
        created = false;
        Console.WriteLine($"passed:dialect={client.Dialect:x4},protection={(args.Contains("--encrypt") ? "encrypt" : "sign")},bytes=65537,entries={entries.Count}");
    }
    finally { if (created) await client.DeleteAsync(path); }
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(Environment.GetEnvironmentVariable("LIBSMB2_TRACE") == "1" ? ex.ToString() : ex.Message);
    return 1;
}
