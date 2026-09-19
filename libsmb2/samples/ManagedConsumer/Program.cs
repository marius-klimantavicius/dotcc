using Managed.Smb;

if (args.Length == 0 || args.Contains("--help"))
{
    Console.WriteLine("Usage: ManagedConsumer SERVER[:PORT] SHARE USER [DIALECT_HEX] [--encrypt]");
    Console.WriteLine("Set LIBSMB2_PASSWORD and optionally LIBSMB2_DOMAIN. The sample creates, verifies and deletes its own temporary file.");
    return 0;
}
if (args.Length < 3) { Console.Error.WriteLine("Server, share and user are required; use --help."); return 2; }
var password = Environment.GetEnvironmentVariable("LIBSMB2_PASSWORD");
if (password is null) { Console.Error.WriteLine("Set LIBSMB2_PASSWORD."); return 2; }
try
{
    ushort dialect = args.Length > 3 && !args[3].StartsWith("--") ? Convert.ToUInt16(args[3], 16)
        : (ushort)LibSmb2.smb2_negotiate_version.SMB2_VERSION_0311;
    await using var client = await SmbConnection.ConnectAsync(args[0], args[1], args[2], password,
        Environment.GetEnvironmentVariable("LIBSMB2_DOMAIN") ?? "WORKGROUP", dialect, args.Contains("--encrypt"));
    var entries = await client.ListAsync();
    var path = "dotcc-sample-" + Guid.NewGuid().ToString("N") + ".bin";
    bool created = false;
    try
    {
        using (var file = client.Open(path, create: true))
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
            file.Flush();
            if (file.Stat().Size != (ulong)payload.Length) throw new IOException("File length differs");
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
            file.Truncate(17);
            if (file.Stat().Size != 17) throw new IOException("Truncated length differs");
        }
        client.Rename(path, path + ".renamed");
        path += ".renamed";
        if (client.Stat(path).Size != 17 || !(await client.ListAsync()).Any(entry => entry.Name == path))
            throw new IOException("Renamed file missing or length differs");
        client.Delete(path);
        created = false;
        Console.WriteLine($"passed:dialect={client.Dialect:x4},protection={(args.Contains("--encrypt") ? "encrypt" : "sign")},bytes=65537,entries={entries.Count}");
    }
    finally { if (created) client.Delete(path); }
    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine(Environment.GetEnvironmentVariable("LIBSMB2_TRACE") == "1" ? ex.ToString() : ex.Message);
    return 1;
}
