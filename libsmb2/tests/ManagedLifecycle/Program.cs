using Managed.Smb;

internal static class Program
{
    private sealed record Settings(string Server, string Share, string User, string Password,
        string Domain, ushort Dialect, bool Encrypt)
    {
        public Task<SmbConnection> ConnectAsync(CancellationToken token = default) =>
            SmbConnection.ConnectAsync(Server, Share, User, Password, Domain, Dialect, Encrypt,
                cancellationToken: token);
    }

    private static async Task<int> Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine("Usage: ManagedLifecycle SERVER[:PORT] SHARE USER [DIALECT_HEX] [--encrypt]");
            return 2;
        }
        var password = Environment.GetEnvironmentVariable("LIBSMB2_PASSWORD");
        if (password is null) { Console.Error.WriteLine("Set LIBSMB2_PASSWORD."); return 2; }
        try
        {
            ushort dialect = args.Length > 3 && !args[3].StartsWith("--") ? Convert.ToUInt16(args[3], 16)
                : (ushort)LibSmb2.smb2_negotiate_version.SMB2_VERSION_0311;
            var settings = new Settings(args[0], args[1], args[2], password,
                Environment.GetEnvironmentVariable("LIBSMB2_DOMAIN") ?? "WORKGROUP", dialect, args.Contains("--encrypt"));
            using var canceled = new CancellationTokenSource();
            canceled.Cancel();
            await ExpectAsync<OperationCanceledException>(async () =>
            {
                using var unexpected = await settings.ConnectAsync(canceled.Token);
            }, "pre-cancelled connect");

            await using var client = await settings.ConnectAsync();
            Require(client.Dialect == dialect, "Negotiated dialect differs");
            await VerifyIdleAsync();
            var directory = "dotcc-lifecycle-Ž-東京-" + Guid.NewGuid().ToString("N");
            client.CreateDirectory(directory);
            var cleanup = new HashSet<string>(StringComparer.Ordinal);
            bool preserveFailure = true;
            try
            {
                Require(client.Stat(directory).Type == LibSmb2.SMB2_TYPE_DIRECTORY, "Directory metadata type differs");
                var space = client.GetSpaceInfo(directory);
                Require(space.BlockSize > 0 && space.FragmentSize > 0 && space.Blocks > 0 &&
                    space.FreeBlocks <= space.Blocks && space.AvailableBlocks <= space.Blocks,
                    "Filesystem space geometry is invalid");
                Expect<SmbException>(() => client.Stat(directory + "/does-not-exist"), "missing path stat");
                Require(client.Dialect == dialect, "Ordinary protocol error invalidated the connection");
                _ = await client.ListAsync(directory);
                var fileName = "duomenys-ą-雪.bin";
                var path = directory + "/" + fileName;
                var payload = Payload(1024 * 1024 + 131101, 29);
                int writes, reads;
                var file = client.Open(path, create: true);
                cleanup.Add(path);
                try
                {
                    Require(file.Read(Array.Empty<byte>()) == 0 && file.Write(Array.Empty<byte>()) == 0,
                        "Empty synchronous I/O must return zero");
                    Require(await file.ReadAsync(Array.Empty<byte>()) == 0 && await file.WriteAsync(Array.Empty<byte>()) == 0,
                        "Empty asynchronous I/O must return zero");
                    var untouched = Enumerable.Repeat((byte)0xa5, 37).ToArray();
                    await ExpectAsync<OperationCanceledException>(() => file.ReadAsync(untouched,
                        cancellationToken: canceled.Token), "pre-cancelled read");
                    Require(untouched.All(value => value == 0xa5), "Cancelled read changed the buffer");
                    await ExpectAsync<OperationCanceledException>(() => file.WriteAsync(payload,
                        cancellationToken: canceled.Token), "pre-cancelled write");
                    await ExpectAsync<OperationCanceledException>(() => file.ReadAsync(Array.Empty<byte>(),
                        cancellationToken: canceled.Token), "pre-cancelled empty read");
                    await ExpectAsync<OperationCanceledException>(() => file.WriteAsync(Array.Empty<byte>(),
                        cancellationToken: canceled.Token), "pre-cancelled empty write");
                    Require(file.Stat().Size == 0, "Pre-cancelled write changed remote file length");
                    writes = await WriteAll(file, payload);
                    file.Flush();
                    Require(file.Stat().Size == (ulong)payload.Length && client.Stat(path).Size == (ulong)payload.Length,
                        "stat/fstat differ from written length");
                    Require(file.Stat().Type == LibSmb2.SMB2_TYPE_FILE, "File metadata type differs");
                    reads = await ReadAll(file, payload);
                    if (dialect == (ushort)LibSmb2.smb2_negotiate_version.SMB2_VERSION_0202)
                        Require(writes > 1 && reads > 1, "SMB2.02 did not exercise negotiated short-transfer loops");
                    file.Truncate(113);
                    Require(file.Stat().Size == 113 && client.Stat(path).Size == 113, "Truncate metadata differs");
                }
                finally { file.Dispose(); }
                file.Dispose();
                Expect<ObjectDisposedException>(() => file.Read(Array.Empty<byte>()), "disposed file empty read");
                Expect<ObjectDisposedException>(() => file.Write(Array.Empty<byte>()), "disposed file empty write");
                Expect<ObjectDisposedException>(() => file.Flush(), "disposed file flush");
                await ExpectAsync<ObjectDisposedException>(() => file.ReadAsync(new byte[1]), "disposed file async read");
                await ExpectAsync<ObjectDisposedException>(() => file.WriteAsync(new byte[1]), "disposed file async write");

                var renamed = directory + "/pervadintas-λ.bin";
                client.Rename(path, renamed); cleanup.Remove(path); cleanup.Add(renamed);
                var listed = await client.ListAsync(directory);
                Require(listed.Any(entry => entry.Name == "pervadintas-λ.bin" && entry.Size == 113 && !entry.IsDirectory),
                    "Unicode rename/list metadata differs");
                Require(!listed.Any(entry => entry.Name == fileName), "Old file name remains after rename");
                Require(client.Stat(renamed).Size == 113, "Renamed file size differs");
                client.Delete(renamed); cleanup.Remove(renamed);

                // Separate live connections intentionally overlap authentication,
                // file transfer and destruction against this same isolated share.
                await Task.WhenAll(Independent(settings, directory + "/parallel-a.bin", 11),
                                   Independent(settings, directory + "/parallel-b.bin", 73));
                var race = await DisposePending(settings, client, directory + "/pending.bin");
                Require(!(await client.ListAsync(directory)).Any(entry => entry.Name is not "." and not ".."),
                    "Lifecycle cleanup left remote files behind");
                client.RemoveDirectory(directory);
                directory = "";
                client.Dispose(); client.Dispose(); await client.DisposeAsync();
                Expect<ObjectDisposedException>(() => _ = client.Dialect, "disposed connection dialect");
                Expect<ObjectDisposedException>(() => client.List(), "disposed connection list");
                await ExpectAsync<ObjectDisposedException>(() => client.ListAsync(), "disposed connection async list");
                Console.WriteLine($"passed:lifecycle,dialect={dialect:x4},protection={(settings.Encrypt ? "encrypt" : "sign")}," +
                    $"bytes={payload.Length},writes={writes},reads={reads},parallel=2,pending={race},idleNotifications=0");
                preserveFailure = false;
            }
            catch (Exception primary)
            {
                Console.Error.WriteLine("lifecycle-primary: " + primary);
                throw;
            }
            finally
            {
                foreach (var path in cleanup) Cleanup(() => client.Delete(path), preserveFailure);
                if (directory.Length != 0) Cleanup(() => client.RemoveDirectory(directory), preserveFailure);
            }
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(Environment.GetEnvironmentVariable("LIBSMB2_TRACE") == "1" ? error.ToString() : error.Message);
            return 1;
        }
    }

    private static async Task Independent(Settings settings, string path, int seed)
    {
        await using var connection = await settings.ConnectAsync();
        bool created = false;
        try
        {
            using (var file = connection.Open(path, create: true))
            {
                created = true;
                var payload = Payload(65539, seed);
                await WriteAll(file, payload);
                await ReadAll(file, payload);
            }
            connection.Delete(path); created = false;
        }
        finally { if (created) connection.Delete(path); }
    }

    private static async Task<string> DisposePending(Settings settings, SmbConnection cleanupConnection, string path)
    {
        var connection = await settings.ConnectAsync();
        SmbConnection.SmbFile? file = null;
        bool created = false;
        try
        {
            file = connection.Open(path, create: true); created = true;
            var payload = Payload(1024 * 1024 + 31, 97);
            // A batch makes pending work observable without private-field access
            // or claiming interception at a specific socket instruction. Dispose
            // can win the lock before a queued transfer, or wait for one to drain.
            var tasks = Enumerable.Range(0, 16).Select(_ => file.WriteAsync(payload)).ToArray();
            int pending = tasks.Count(task => !task.IsCompleted);
            Require(pending > 0, "No pending operation was observed for the disposal race");
            await connection.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(30));
            int completed = 0, disposed = 0;
            foreach (var task in tasks)
            {
                try
                {
                    int written = await task.WaitAsync(TimeSpan.FromSeconds(30));
                    Require(written > 0 && written <= payload.Length, "Pending write returned an invalid count");
                    completed++;
                }
                catch (ObjectDisposedException) { disposed++; }
            }
            Require(completed + disposed == tasks.Length, "Pending operations did not drain");
            Expect<ObjectDisposedException>(() => file.Read(Array.Empty<byte>()), "connection-disposed file empty read");
            Expect<ObjectDisposedException>(() => file.Write(Array.Empty<byte>()), "connection-disposed file empty write");
            file.Dispose(); file.Dispose(); connection.Dispose();
            cleanupConnection.Delete(path); created = false;
            return $"{pending}/{completed}/{disposed}";
        }
        finally
        {
            file?.Dispose(); connection.Dispose();
            if (created) cleanupConnection.Delete(path);
        }
    }

    private static byte[] Payload(int size, int seed) => Enumerable.Range(0, size)
        .Select(index => unchecked((byte)(index * seed + 17))).ToArray();

    private static async Task VerifyIdleAsync()
    {
        // Let already queued connect notifications settle, then observe an idle
        // authenticated connection. This delay is a measurement window, not a
        // transport readiness mechanism or a simulated network failure.
        await Task.Delay(100);
        var before = HostSockets.Snapshot();
        await Task.Delay(250);
        var after = HostSockets.Snapshot();
        Require(after.ServiceNotifications == before.ServiceNotifications,
            "Idle context generated repeated service notifications");
        Require(after.IoCompletions == before.IoCompletions,
            "Idle context performed unexpected socket I/O");
    }

    private static async Task<int> WriteAll(SmbConnection.SmbFile file, byte[] payload)
    {
        int offset = 0, calls = 0;
        while (offset < payload.Length)
        {
            var part = payload[offset..];
            int count = await file.WriteAsync(part, (ulong)offset);
            Require(count > 0 && count <= part.Length, "Write made no progress or exceeded its request");
            offset += count; calls++;
        }
        return calls;
    }

    private static async Task<int> ReadAll(SmbConnection.SmbFile file, byte[] expected)
    {
        int offset = 0, calls = 0;
        while (offset < expected.Length)
        {
            // Request past EOF: all dialects exercise a real short read and
            // must leave bytes outside the returned count untouched.
            var part = Enumerable.Repeat((byte)0xa5, expected.Length - offset + 31).ToArray();
            int count = await file.ReadAsync(part, (ulong)offset);
            Require(count > 0 && count <= expected.Length - offset, "Unexpected EOF or excess read bytes");
            Require(part.AsSpan(0, count).SequenceEqual(expected.AsSpan(offset, count)), "Read-back bytes differ");
            Require(part.AsSpan(count).IndexOfAnyExcept((byte)0xa5) == -1, "Read changed bytes beyond returned count");
            offset += count; calls++;
        }
        Require(await file.ReadAsync(new byte[1], (ulong)expected.Length) == 0, "EOF did not return zero");
        return calls;
    }

    private static void Cleanup(Action action, bool preserveFailure)
    {
        try { action(); } catch when (preserveFailure) { /* Keep the failed assertion as the diagnostic. */ }
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new IOException(message);
    }
    private static void Expect<T>(Action action, string operation) where T : Exception
    {
        try { action(); } catch (T) { return; }
        throw new IOException(operation + " did not throw " + typeof(T).Name);
    }
    private static async Task ExpectAsync<T>(Func<Task> action, string operation) where T : Exception
    {
        try { await action(); } catch (T) { return; }
        throw new IOException(operation + " did not throw " + typeof(T).Name);
    }
}
