using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Managed.Smb;
using static Managed.Smb.LibSmb2;

// Deliberately JIT-only: private reflection supplies disconnected synthetic
// handles and counts the runtime's checked native heap, without public test hooks.
unsafe class Program
{
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static readonly Type ConnectionType = typeof(SmbConnection);
    private static readonly Type FileType = typeof(SmbConnection.SmbFile);

    private static int Main(string[] args)
    {
        if (Environment.GetEnvironmentVariable("DOTCC_DEBUG_HEAP_SCAN") != "1")
            throw new InvalidOperationException("Run through facade-lifetime.py to enable the checked heap");
        Libc.EnableDebugHeap();
        string name = args.Single();
        int before = CountAllocations();
        switch (name)
        {
            case "finalizer-cleanup":
                var owner = MakeConnection();
                AddFile(owner); AddFile(owner); AddFile(owner);
                DestroyWithoutClose(owner);
                GC.SuppressFinalize(owner);
                break;
            case "actual-finalizer":
                var weak = AbandonConnection();
                for (int i = 0; i < 5; i++)
                {
                    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                }
                if (weak.IsAlive) throw new Exception("Abandoned connection was kept alive");
                break;
            case "failed-first-close":
                using (var connection = MakeConnection())
                {
                    AddFile(connection); AddFile(connection);
                    bool failed = false;
                    try { connection.Dispose(); } catch (SmbException) { failed = true; }
                    if (!failed) throw new Exception("Disconnected close unexpectedly succeeded");
                }
                break;
            case "pending-read-abort":
                var readOwner = MakeConnection();
                var file = AddFile(readOwner);
                byte[] buffer = Enumerable.Repeat((byte)0x5a, 32).ToArray();
                try
                {
                    bool failed = false;
                    try { file.Read(buffer); } catch (SmbException) { failed = true; }
                    if (!failed) throw new Exception("Disconnected read unexpectedly succeeded");
                    bool disposed = false;
                    try { _ = readOwner.Dialect; } catch (ObjectDisposedException) { disposed = true; }
                    if (!disposed) throw new Exception("Failed pending read left context active");
                    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
                    if (buffer.Any(value => value != 0x5a)) throw new Exception("Failed read changed its buffer");
                }
                finally { DestroyWithoutClose(readOwner); GC.SuppressFinalize(readOwner); }
                break;
            default: throw new ArgumentException("Unknown lifetime case: " + name);
        }
        int after = CountAllocations();
        Console.WriteLine($"{name}: native allocations before={before} after={after}");
        return before == after ? 0 : 1;
    }

    private static SmbConnection MakeConnection()
    {
        var owner = (SmbConnection)Activator.CreateInstance(ConnectionType, nonPublic: true)!;
        // Old regression binaries do not have a managed deadline field.
        ConnectionType.GetField("_timeoutSeconds", PrivateInstance)?.SetValue(owner, 1);
        var context = (smb2_context*)Pointer.Unbox(ConnectionType.GetField("_context", PrivateInstance)!.GetValue(owner)!);
        context->max_read_size = 4096;
        context->credits = 1;
        context->dialect = (ushort)smb2_negotiate_version.SMB2_VERSION_0202;
        return owner;
    }

    private static SmbConnection.SmbFile AddFile(SmbConnection owner)
    {
        var context = (smb2_context*)Pointer.Unbox(ConnectionType.GetField("_context", PrivateInstance)!.GetValue(owner)!);
        byte* id = stackalloc byte[16];
        new Span<byte>(id, 16).Clear();
        var handle = smb2_fh_from_file_id(context, id);
        if (handle == null) throw new OutOfMemoryException();
        fixed (byte* path = Encoding.UTF8.GetBytes("synthetic-file\0")) handle->path = Libc.strdup(path);
        var file = (SmbConnection.SmbFile)RuntimeHelpers.GetUninitializedObject(FileType);
        FileType.GetField("_owner", PrivateInstance)!.SetValue(file, owner);
        FileType.GetField("_handle", PrivateInstance)!.SetValue(file, Pointer.Box(handle, typeof(smb2fh*)));
        ((HashSet<SmbConnection.SmbFile>)ConnectionType.GetField("_files", PrivateInstance)!.GetValue(owner)!).Add(file);
        return file;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference AbandonConnection()
    {
        var connection = MakeConnection();
        AddFile(connection); AddFile(connection);
        return new WeakReference(connection);
    }
    private static void DestroyWithoutClose(SmbConnection connection)
        => ConnectionType.GetMethod("DisposeCore", PrivateInstance)!.Invoke(connection, [false]);

    private static int CountAllocations()
    {
        var head = (byte*)Pointer.Unbox(typeof(Libc).GetField("_dbgHead", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!);
        int count = 0;
        for (byte* block = head; block != null; block = *(byte**)(block + 16)) count++;
        return count;
    }
}
