using DotCC.Sqlite;
using Microsoft.Win32.SafeHandles;

// Exercise Darwin inode ownership on every CI platform with real SafeFileHandles
// and deterministic range-operation failures. Actual OS locking is covered by
// HostVfsTests and its separate-process/native SQLite oracle campaign.
internal static class Program
{
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static HostPlatform.FileHandle Lease(string path, HostPlatform.InodeState inode)
    {
        var handle = File.OpenHandle(path, FileMode.OpenOrCreate, FileAccess.ReadWrite,
            FileShare.ReadWrite | FileShare.Delete);
        var lease = new HostPlatform.FileHandle(handle, true, inode, default);
        inode.Files.Add(lease);
        return lease;
    }

    private static void ExpectCloseError(HostPlatform.FileHandle lease)
    {
        try { lease.Dispose(); }
        catch (IOException) { return; }
        throw new InvalidOperationException("failed unlock must surface an I/O close error");
    }

    private static void ImmediateClose(string path)
    {
        var inode = new HostPlatform.InodeState();
        var lease = Lease(path, inode);
        Require(lease.Lock(1) == 0, "solo reader");
        HostPlatform.RangeOperationForTests = (_, _, _, kind) => kind == 2 ? 10 | (8 << 8) : 0;
        ExpectCloseError(lease);
        Require(inode.Files.Count == 0, "failed close must detach the unreachable lease");
        Require(lease.Handle.IsClosed, "no peer locks: close must release descriptor despite unlock failure");
        Require(inode.Deferred.Count == 0, "no peer locks: no deferred descriptor");
        lease.Dispose(); // Failed close has nevertheless completed its ownership transfer.
        try { lease.Lock(1); }
        catch (ObjectDisposedException) { return; }
        throw new InvalidOperationException("closed lease must not be reused");
    }

    private static void DeferredClose(string path)
    {
        var inode = new HostPlatform.InodeState();
        var writer = Lease(path, inode);
        var reader = Lease(path, inode);
        var newcomer = Lease(path, inode);
        Require(writer.Lock(1) == 0 && reader.Lock(1) == 0 && writer.Lock(2) == 0, "writer and peer reader");
        HostPlatform.RangeOperationForTests = (handle, start, _, kind) =>
            ReferenceEquals(handle, writer.Handle) && start == 0x40000001 && kind == 2 ? 10 | (8 << 8) : 0;
        ExpectCloseError(writer);
        Require(!inode.Files.Contains(writer) && inode.Files.Count == 2, "failed close must remove ghost owner");
        Require(!writer.Handle.IsClosed && inode.Deferred.Count == 1, "peer reader keeps descriptor deferred");
        Require(reader.LockLevel == 1 && !reader.Handle.IsClosed, "peer ownership survives failed close");
        Require(newcomer.Lock(1) == (10 | (15 << 8)), "orphan locks prohibit new inode locks");
        Require(reader.CheckReserved(out _) == (10 | (14 << 8)), "orphan ownership cannot report false reservation state");
        writer.Dispose();
        Require(!writer.Handle.IsClosed, "duplicate close cannot bypass deferral");
        Require(reader.Unlock(0) == 0, "surviving reader can release locks");
        Require(writer.Handle.IsClosed && inode.Deferred.Count == 0, "last unlock drains orphan descriptor");
        Require(newcomer.Lock(1) == 0 && newcomer.Lock(2) == 0, "inode becomes reusable after orphan drain");
        newcomer.Dispose();
        reader.Dispose();
        Require(inode.Files.Count == 0 && inode.Deferred.Count == 0, "all ownership released");
    }

    private static int Main()
    {
        int failed = 0;
        foreach (var test in new (string Name, Action<string> Run)[]
            { ("immediate close", ImmediateClose), ("deferred close", DeferredClose) })
        {
            string path = Path.Combine(Path.GetTempPath(), "dotcc-close-failure-" + Guid.NewGuid().ToString("N"));
            HostPlatform.RangeOperationForTests = (_, _, _, _) => 0;
            try { test.Run(path); Console.WriteLine("PASS Darwin unlock failure: " + test.Name); }
            catch (Exception error) { failed++; Console.Error.WriteLine("FAIL " + test.Name + ": " + error.Message); }
            finally { HostPlatform.RangeOperationForTests = null; File.Delete(path); }
        }
        return failed == 0 ? 0 : 1;
    }
}
