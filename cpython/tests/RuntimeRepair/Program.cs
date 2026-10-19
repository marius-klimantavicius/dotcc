using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    static int count, depth, maxDepth, checks, callbackThread;
    static RuntimeContext? expectedOwner;
    static readonly ManualResetEventSlim entered = new(), resume = new();
    static void Check(bool condition, string name) { checks++; if (!condition) throw new Exception(name); }
    static void Handler(int sig) { Check(sig == 2, "signal number"); count++; errno = 35; }
    static void Second(int sig) { count++; maxDepth = Math.Max(maxDepth, ++depth); --depth; }
    static void First(int sig)
    {
        count++; maxDepth = Math.Max(maxDepth, ++depth);
        Check((nint)signal(2, &Second) == (nint)(delegate*<int, void>)&First, "previous installed disposition");
        Check(raise(2) == 0 && count == 1, "nested signal deferred");
        --depth;
    }
    static void OwnedHandler(int sig)
    {
        Check(RuntimeContext.Current == expectedOwner, "registration owner on raising thread");
        Check(Environment.CurrentManagedThreadId == callbackThread, "same raising thread");
        errno = 42; count++;
    }
    static void BlockingHandler(int sig)
    {
        Check(RuntimeContext.Current == expectedOwner, "in-flight owner");
        entered.Set(); Check(resume.Wait(10000), "release in-flight callback"); count++;
    }
    static void ThrowingHandler(int sig) => throw new ApplicationException("callback failure");
    [DllImport("libc", EntryPoint = "sigaction", SetLastError = true)]
    static extern int HostAction(int sig, LinuxSignalAction* action, LinuxSignalAction* old);
    [DllImport("libc", EntryPoint = "pthread_sigmask")]
    static extern int HostMask(int how, ulong* set, ulong* old);
    [DllImport("libc", EntryPoint = "sigpending")]
    static extern int HostPending(ulong* set);
    // Linux has 64 kernel signal bits. glibc's 128-byte sigset_t reserves
    // additional, unspecified output bytes; native controls expose that tail.
    static bool EqualMask(ulong* left, ulong* right) => left[0] == right[0];
    static int BlockedDefault()
    {
        using var owner = new RuntimeContext(); expectedOwner = owner;
        signal(2, null);
        LinuxSignalAction before = default, after = default;
        Check(HostAction(2, null, &before) == 0, "query host action");
        // Exercise a nonempty handler mask as well as flags/restorer/pointer.
        before.Mask[0] |= (1UL << 9) | (1UL << 11);
        before.Flags |= 0x10000000; // host Linux SA_RESTART
        Check(HostAction(2, &before, null) == 0, "install host mask/flags control");
        before = default;
        Check(HostAction(2, null, &before) == 0, "snapshot actual host action");
        ulong* mask = stackalloc ulong[16]; ulong* previous = stackalloc ulong[16]; ulong* observed = stackalloc ulong[16]; ulong* pending = stackalloc ulong[16];
        new Span<ulong>(mask, 16).Clear(); new Span<ulong>(previous, 16).Clear(); new Span<ulong>(observed, 16).Clear(); new Span<ulong>(pending, 16).Clear(); mask[0] = 2;
        Check(HostMask(0, mask, previous) == 0, "block actual SIGINT");
        try
        {
            Check(HostMask(2, null, observed) == 0, "query blocked mask");
            errno = 71; Check(raise(2) == 0 && errno == 71, "blocked default returns with errno");
            Check(HostAction(2, null, &after) == 0, "query restored action");
            Check(before.Handler == after.Handler && before.Flags == after.Flags && before.Restorer == after.Restorer && before.Mask[0] == after.Mask[0], "all defined host action fields restored");
            new Span<ulong>(pending, 16).Clear(); Check(HostMask(2, null, pending) == 0 && EqualMask(observed, pending), "thread mask unchanged");
            new Span<ulong>(pending, 16).Clear(); Check(HostPending(pending) == 0 && (pending[0] & 2) != 0, "SIGINT remains pending");
            using (owner.Enter()) signal(2, &HostHandler);
            Check(HostMask(2, previous, null) == 0, "unblock through original mask");
            Check(entered.Wait(10000), "pending signal delivered through restored CLR handler");
            signal(2, null); Check(count == 1, "pending signal exactly once");
            Check(HostPending(pending) == 0 && (pending[0] & 2) == 0, "pending signal consumed");
        }
        finally { HostMask(2, previous, null); signal(2, null); }
        Console.WriteLine("blocked=0 action-same=1 mask-same=1 pending-before=1 delivered=1 pending-after=0");
        return 0;
    }
    static void DisposeMustFail(RuntimeContext owner)
    {
        bool failed = false;
        try { owner.Dispose(); } catch (InvalidOperationException) { failed = true; }
        Check(failed, "owner retained during registration/delivery");
    }
    static int Main(string[] args)
    {
        string mode = args.Length == 0 ? "all" : args[0];
        if (mode == "host-abi")
        {
            Console.WriteLine($"host-action size={sizeof(LinuxSignalAction)} handler={Marshal.OffsetOf<LinuxSignalAction>(nameof(LinuxSignalAction.Handler))} mask={Marshal.OffsetOf<LinuxSignalAction>(nameof(LinuxSignalAction.Mask))} flags={Marshal.OffsetOf<LinuxSignalAction>(nameof(LinuxSignalAction.Flags))} restorer={Marshal.OffsetOf<LinuxSignalAction>(nameof(LinuxSignalAction.Restorer))}");
            return 0;
        }
        if (mode == "blocked") return BlockedDefault();
        if (mode == "directory")
        {
            fixed (byte* path = Encoding.UTF8.GetBytes(args[1] + "\0"))
            {
                errno = 71; void* d = opendir(path);
                if (d == null) { Console.WriteLine($"errno={errno}"); return 0; }
                var names = new HashSet<string>(); void* e;
                while ((e = readdir(d)) != null) names.Add(Marshal.PtrToStringUTF8((nint)e)!);
                Console.WriteLine($"directory={(names.Contains(".") ? 1 : 0)},{(names.Contains("..") ? 1 : 0)},{(names.Contains("item") ? 1 : 0)} close={closedir(d)}");
                return 0;
            }
        }
        if (mode == "default") { signal(2, null); Console.WriteLine("before-default"); raise(2); Console.WriteLine("unexpected-return"); Thread.Sleep(2000); return 91; }
        if (mode == "ignore") { signal(2, (delegate*<int, void>)1); errno = 71; int status = raise(2); Console.WriteLine($"ignore={status} errno={errno}"); signal(2, null); return 0; }
        if (mode == "error")
        {
            errno = 0; var p = signal(-1, &Handler); Console.WriteLine($"signal-error={((nint)p == -1 ? 1 : 0)} errno={errno}");
            errno = 0; int status = raise(-1); Console.WriteLine($"raise-error={(status != 0 ? 1 : 0)} errno={errno}"); return 0;
        }
        if (mode == "reentrant") { signal(2, &First); int status = raise(2); signal(2, null); Console.WriteLine($"reentrant={status} count={count} depth={maxDepth}"); return 0; }
        if (mode == "sync") { signal(2, &Handler); errno = 71; int status = raise(2); Console.WriteLine($"sync={status} count={count} errno={errno}"); signal(2, null); Thread.Sleep(100); return count != 1 ? 39 : 0; }
        if (mode == "host")
        {
            using var owner = new RuntimeContext(); expectedOwner = owner;
            using (owner.Enter()) for (int i = 0; i < 10; i++) signal(2, &HostHandler);
            Console.WriteLine("ready"); Console.Out.Flush();
            Check(entered.Wait(10000), "OS signal delivered"); Thread.Sleep(200);
            signal(2, null); Check(count == 1, "exactly one host callback");
            Console.WriteLine($"host={count}"); return 0;
        }
        RunOwnership(); RunDirectories();
        Console.WriteLine($"checks={checks} passed"); return 0;
    }
    static void HostHandler(int sig)
    {
        Check(RuntimeContext.Current == expectedOwner, "OS callback registration owner");
        Interlocked.Increment(ref count); entered.Set();
    }
    static void RunOwnership()
    {
        var owner = new RuntimeContext(); using var other = new RuntimeContext();
        try
        {
            using (owner.Enter())
            {
                Check((nint)signal(2, &OwnedHandler) == 0, "default prior disposition");
                Check((nint)signal(2, &OwnedHandler) == (nint)(delegate*<int, void>)&OwnedHandler, "reinstall prior disposition");
            }
            DisposeMustFail(owner);
            expectedOwner = owner;
            using (other.Enter())
            {
                errno = 73; callbackThread = Environment.CurrentManagedThreadId;
                Check(raise(2) == 0, "raise succeeds");
                Check(RuntimeContext.Current == other && errno == 73, "caller context and errno restored");
                Check(count == 1, "one synchronous callback");
                Check((nint)signal(2, (delegate*<int, void>)1) == (nint)(delegate*<int, void>)&OwnedHandler, "ignore prior handler");
            }
            owner.Dispose(); // replacement released owner; ignore belongs to other
            DisposeMustFail(other);
            Check(raise(2) == 0 && count == 1, "ignore has no callback");
            Check((nint)signal(2, null) == 1, "default prior ignore");
            Check((nint)signal(2, null) == 0, "repeat default");
        }
        finally { signal(2, null); owner.Dispose(); }
        using var flightOwner = new RuntimeContext(); expectedOwner = flightOwner;
        using (flightOwner.Enter()) signal(2, &BlockingHandler);
        Exception? workerError = null;
        var worker = new Thread(() => { try { Check(raise(2) == 0, "worker raise"); } catch (Exception e) { workerError = e; } });
        worker.Start();
        try
        {
            Check(entered.Wait(10000), "callback entered"); signal(2, null);
            DisposeMustFail(flightOwner); // retired registration, still in-flight
        }
        finally { resume.Set(); Check(worker.Join(10000), "callback drained"); signal(2, null); }
        if (workerError != null) throw workerError;
        flightOwner.Dispose();
        using var exceptionOwner = new RuntimeContext();
        using (exceptionOwner.Enter()) signal(2, &ThrowingHandler);
        using (other.Enter())
        {
            bool caught = false;
            try { raise(2); } catch (ApplicationException) { caught = true; }
            Check(caught && RuntimeContext.Current == other, "exception restores raising context");
        }
        signal(2, null); exceptionOwner.Dispose();
        count = depth = maxDepth = 0; signal(2, &First); Check(raise(2) == 0, "outer raise"); signal(2, null);
        Check(count == 2 && maxDepth == 1, "deferred callback observes new disposition");
        errno = 77; Check((nint)signal(2, (delegate*<int, void>)(nint)(-1)) == -1 && errno == EINVAL, "SIG_ERR rejected");
    }
    static void RunDirectories()
    {
        string root = Path.Combine(Path.GetTempPath(), "dotcc-runtime-r23-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root); File.WriteAllText(Path.Combine(root, "item"), "x");
        try
        {
            using var owner = new RuntimeContext(); using var other = new RuntimeContext();
            void* token;
            using (owner.Enter())
            fixed (byte* path = Encoding.UTF8.GetBytes(root + "\0"))
            {
                errno = 71; token = opendir(path); Check(token != null && errno == 71, "open preserves errno");
                Check(readdir(token) != null, "first entry"); rewinddir(token);
                var names = new HashSet<string>(); void* entry;
                while ((entry = readdir(token)) != null) names.Add(Marshal.PtrToStringUTF8((nint)entry)!);
                Check(names.SetEquals(new[] { ".", "..", "item" }), "actual snapshot entries");
                Check(errno == 71, "EOF preserves errno");
            }
            using (other.Enter()) { Check(closedir(token) == -1 && errno == EBADF, "foreign owner rejected"); }
            using (owner.Enter()) { Check(closedir(token) == 0, "owner closes"); Check(closedir(token) == -1 && errno == EBADF, "double close rejected"); }
            var retired = new RuntimeContext();
            using (retired.Enter()) fixed (byte* path = Encoding.UTF8.GetBytes(root + "\0")) { Check(opendir(path) != null, "outstanding snapshot"); }
            retired.Dispose();
            Check(!ReferenceEquals(RuntimeContext.Current, retired), "unbound disposal completed");
            int before = Directory.GetFileSystemEntries("/proc/self/fd").Length;
            for (int i = 0; i < 32; i++)
            fixed (byte* path = Encoding.UTF8.GetBytes(root + "\0")) { void* d = opendir(path); Check(d != null && closedir(d) == 0, "repeated open close"); }
            Check(Directory.GetFileSystemEntries("/proc/self/fd").Length == before, "host directory handles closed");
        }
        finally { Directory.Delete(root, true); }
    }
}
