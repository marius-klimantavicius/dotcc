using System.Text;
using System.Runtime.InteropServices;
using Managed.Emulation;
using Managed.Emulation.Host;
using Managed.Emulation.Execution;
using Blink = Managed.Emulation.BlinkCore;
using Libc = Managed.Emulation.BlinkCore.Libc;

static unsafe class Program
{
    static void Check(bool value, string why) { if (!value) throw new Exception(why); }
    static int Open(int flags = 0)
    {
        byte[] path = Encoding.UTF8.GetBytes("/proc/self/maps\0");
        fixed (byte* p = path) return Blink.blink_io_open(p, flags);
    }
    static byte[] Read(InstanceIo io, int fd)
    {
        using var result = new MemoryStream();
        byte[] chunk = new byte[17];
        for (;;) {
            var n = io.ReadAsync(fd, chunk).GetAwaiter().GetResult();
            Check(n.Succeeded, "read");
            if (n.Value == 0) return result.ToArray();
            result.Write(chunk, 0, n.Value);
        }
    }
    public static void Main()
    {
        using var tables = new BlinkCore();
        var execution = new ThreadedGuestExecution(tables);
        tables.Map(0x1000, 3); tables.Map(0x2000, 3);
        tables.Map(0x4000, 0); // a real reserved PROT_NONE leaf
        tables.Map(0x5000, 0, shared: true, reservedBacking: true);
        tables.Map(0x6000, 5);
        tables.Map(0xffff800000001000UL, 1);
        string first = Encoding.ASCII.GetString(execution.Capture());
        Check(first == "1000-3000 rw-p 00000000 00:00 0\n" +
            "4000-5000 ---p 00000000 00:00 0\n" +
            "5000-6000 ---s 00000000 00:00 0\n" +
            "6000-7000 r-xp 00000000 00:00 0\n" +
            "ffff800000001000-ffff800000002000 r--p 00000000 00:00 0\n", "guest walk, coalescing, holes, sharing, canonical ordering");
        var io = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        Blink.BindHostIo(io);
        Blink.BindHostProcessMaps(execution.Capture, 8UL << 20);
        try
        {
            Blink.rlimit limit = default;
            Check(Blink.blink_host_getrlimit(3, &limit) == 0 && limit.rlim_cur == 8UL << 20 && limit.rlim_max == 8UL << 20, "stack limit");
            Check(Blink.blink_host_setrlimit(3, &limit) == 0, "idempotent limit");
            --limit.rlim_cur;
            Check(Blink.blink_host_setrlimit(3, &limit) == -1 && Libc.errno == 1, "immutable limit");
            Check(Open(1) == -1 && Libc.errno == 13 && Open(2) == -1 && Libc.errno == 13, "deny write opens");
            Check(Open(65536) == -1 && Libc.errno == 20, "directory mode");
            var before = io.MemoryFilesUsage();
            int fd = Open(524288), dup = Blink.blink_io_dup(fd);
            Check(fd >= 3 && dup >= 3 && io.GetStatusFlags(fd).Value == 0 && io.GetDescriptorFlags(fd).Value == 1, "read-only CLOEXEC descriptor");
            Check(io.MemoryFilesUsage().Bytes > before.Bytes && io.MemoryFilesUsage().Files == before.Files + 1, "charged ownership");
            Check(io.FStat(fd).Value is { Immutable: true, Mode: 0x8124 } && io.FStat(fd).Value.Length == first.Length, "fstat");
            byte[] prefix = new byte[5];
            Check(io.ReadAsync(fd, prefix).GetAwaiter().GetResult().Value == 5, "initial read");
            Check(Encoding.ASCII.GetString(Read(io, dup)) == first[5..], "dup shares position");
            Check(io.WriteAsync(fd, "x"u8.ToArray()).GetAwaiter().GetResult().Error == GuestError.BadDescriptor, "deny write");
            Check(io.WriteAt(fd, "x"u8, 0).Error == GuestError.BadDescriptor && io.TruncateFile(fd, 0).Error == GuestError.Invalid, "deny pwrite/truncate");
            Check(io.SetStatusFlags(fd, 2 | 1024).Succeeded && (io.GetStatusFlags(fd).Value & 3) == 0, "F_SETFL preserves read-only access");
            Check(io.WriteAsync(dup, "x"u8.ToArray()).GetAwaiter().GetResult().Error == GuestError.BadDescriptor, "dup remains immutable");
            Check(io.SharedFileReadOnly(fd).Error == GuestError.Unsupported && io.AcquireMemoryFilePage(fd, 0).Error == GuestError.Unsupported, "deny shared mapping and writable lease");
            tables.Map(0x1000, 1); tables.Unmap(0x2000);
            Check(io.Seek(fd, 0, SeekOrigin.Begin).Value == 0 && Encoding.ASCII.GetString(Read(io, fd)) == first, "snapshot stays fixed after mapping changes");
            int next = Open();
            Check(Encoding.ASCII.GetString(Read(io, next)).StartsWith("1000-2000 r--p\n".Replace("\n", " 00000000 00:00 0\n")), "new open observes changes");
            Check(io.CloseOnExecDescriptors().Value == 1 && io.GetDescriptorFlags(fd).Error == GuestError.BadDescriptor, "close-on-exec only original");
            Check(io.Close(dup).Succeeded && io.Close(next).Succeeded && io.MemoryFilesUsage() == before, "last close reclaims all charges");
            Check(io.SetMemoryFileLimit(5120).Succeeded, "small budget");
            Check(Open() == -1 && Libc.errno == 12 && io.OpenDescriptors == 3 && io.MemoryFilesUsage().Bytes == 0, "allocation failure rollback");
            Check(io.OpenProcessMaps(new byte[(1 << 20) + 1]).Error == GuestError.NoMemory && io.OpenDescriptors == 3, "oversized snapshot denied");
            Check(io.SetMemoryFileLimit(2 << 20).Succeeded, "restore test budget");
            int canceledCount = io.OpenDescriptors;
            Check(io.OpenProcessMaps("x"u8, cancellation: new CancellationToken(true)).Error == GuestError.Canceled && io.OpenDescriptors == canceledCount, "cancellation rollback");
            Blink.UnbindHostProcessMaps();
            Check(Open() == -1 && Libc.errno == 2 && Blink.blink_host_getrlimit(3, &limit) == -1 && Libc.errno == 95, "unbind clears process callbacks");
            using var fragmented = new BlinkCore();
            for (ulong page = 0; page < 26000; ++page) fragmented.Map(0x100000000 + page * 8192, 1);
            var large = new ThreadedGuestExecution(fragmented);
            bool bounded = false;
            try { large.Capture(); } catch (OutOfMemoryException) { bounded = true; }
            Check(bounded && BlinkCore.LockDepth == 0, "fragmentation byte limit and lock cleanup");
        }
        finally { Blink.UnbindHostIo(); io.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
        Check(io.OpenDescriptors == 0 && io.MemoryFilesUsage().Bytes == 0, "final cleanup");
        var combined = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>());
        try {
            var budget = combined.BindMemoryFileBudget(8192, 4096);
            Check(budget.Succeeded, "combined budget bind");
            Check(combined.OpenProcessMaps("x"u8).Error == GuestError.NoMemory && combined.OpenDescriptors == 3 && combined.MemoryFilesUsage().Bytes == 0, "combined charge rollback");
            Check(InstanceIo.AccountMemoryFileBudget(budget.Value, 2048).Succeeded, "combined reserve room");
            var snapshot = combined.OpenProcessMaps("x"u8);
            Check(snapshot.Succeeded && InstanceIo.AccountMemoryFileBudget(budget.Value, 4096).Error == GuestError.NoMemory, "snapshot and guest backing share one budget");
            Check(combined.Close(snapshot.Value).Succeeded && InstanceIo.AccountMemoryFileBudget(budget.Value, 4096).Succeeded, "combined close releases charge");
            Check(InstanceIo.AccountMemoryFileBudget(budget.Value, 0, release: true).Succeeded, "combined ledger cleanup");
        } finally { combined.DisposeAsync().AsTask().GetAwaiter().GetResult(); }
        Console.WriteLine("process-maps focused: guest walk, snapshots, readonly, quota, stack limit, cleanup: PASS");
    }
}

// ABI-only declarations for the unchanged authored bridges. The page-table
// fixture supplies actual four-level tables; this is focused software coverage,
// not a substitute for the parent's generated Blink integration.
namespace Managed.Emulation
{
    public unsafe partial class BlinkCore
    {
        internal static HostIdentity? identity;
        public struct blink_host_iovec { public void* iov_base; public ulong iov_len; }
        public struct rlimit { public ulong rlim_cur, rlim_max; }
        public struct rusage { public ulong value; }
        public struct tms { public ulong value; }
        public static ulong BlinkHostMemoryLimit() => 256UL << 20;
    }
    public unsafe partial class BlinkCore : IDisposable
    {
            public const int PAGE_V = 1, PAGE_HOST = 1024, PAGE_RSRV = 512, PAGE_PS = 128;
            public const ulong PAGE_XD = 1UL << 63, PAGE_TA = 0x0000fffffffff000UL;
        public struct System { public ulong cr3; public int mmap_lock; }
        public static int LockDepth;
        public static unsafe class Libc {
            [ThreadStatic] public static int errno;
            public static int pthread_mutex_lock(int* pointer) { ++LockDepth; return 0; }
            public static int pthread_mutex_unlock(int* pointer) { --LockDepth; return 0; }
        }
        readonly List<nint> allocations = new();
        readonly HashSet<nint> shared = new();
        public System* State;
        public BlinkCore() { State = (System*)Allocate(); State->cr3 = (ulong)Allocate() | PAGE_HOST; }
        byte* Allocate() { var p = (byte*)NativeMemory.AlignedAlloc(4096,4096); NativeMemory.Clear(p,4096); allocations.Add((nint)p); return p; }
        public byte* GetPageAddress(System* state, ulong entry, bool root) => (byte*)(entry & PAGE_TA);
        public byte* FindHostPage(ulong entry) => (byte*)(entry & PAGE_TA);
        public ulong LoadPte(byte* slot) => *(ulong*)slot;
        public int GetProtection(ulong entry) => ((entry & 4) != 0 ? 1 : 0) | ((entry & 2) != 0 ? 2 : 0) | ((entry & PAGE_XD) == 0 ? 4 : 0);
        public int BlinkHostMemoryFilePageShared(void* pointer) => shared.Contains((nint)pointer) ? 1 : 0;
        ulong* Slot(ulong address) {
            ulong table = State->cr3;
            for (int shift = 39; ; shift -= 9) {
                var slot = (ulong*)GetPageAddress(State, table, shift == 39) + ((address >> shift) & 511);
                if (shift == 12) return slot;
                if ((*slot & PAGE_V) == 0) *slot = (ulong)Allocate() | PAGE_HOST | PAGE_V;
                table = *slot;
            }
        }
        public void Map(ulong address, int prot, bool shared = false, bool reservedBacking = false) {
            ulong entry = 1UL | ((prot & 1) != 0 ? 4UL : 0) | ((prot & 2) != 0 ? 2UL : 0) | ((prot & 4) == 0 ? PAGE_XD : 0);
            if (shared) { var p = Allocate(); this.shared.Add((nint)p); entry |= (ulong)p | PAGE_HOST; }
            if (!shared || reservedBacking) entry |= PAGE_RSRV;
            *Slot(address) = entry;
        }
        public void Unmap(ulong address) => *Slot(address) = 0;
        public void Dispose() { foreach (var p in allocations) NativeMemory.AlignedFree((void*)p); }
    }
}
namespace Managed.Emulation.Execution
{
    public sealed unsafe partial class ThreadedGuestExecution
    {
        BlinkCore? program;
        BlinkCore.System* system;
        public ThreadedGuestExecution(BlinkCore program) { this.program = program; system = program.State; }
        public byte[] Capture() => SnapshotProcessMaps();
    }
}
