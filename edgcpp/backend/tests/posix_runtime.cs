#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Edg.Runtime;

internal static unsafe class Program
{
    private static int checks;
    private static void Check(bool value, string name)
    { if (!value) throw new Exception(name); checks++; }
    private static int Open(PosixContext context, string path, int flags)
    { byte[] bytes = Encoding.UTF8.GetBytes(path + "\0"); fixed (byte* p = bytes) return context.Open((sbyte*)p, flags); }
    private static PosixFlock Lock(short type, long start = 40, long length = 20) => new() { l_type = type, l_start = start, l_len = length };
    private static int Control(PosixContext context, int fd, int command, PosixFlock record) => context.Fcntl(fd, command, &record);
    private static void NativeProbe(string? executable, string path, bool available, string label)
    {
        if (executable is null) return;
        var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add(path); start.ArgumentList.Add("probe");
        using Process process = Process.Start(start)!;
        string output = process.StandardOutput.ReadToEnd(); process.WaitForExit();
        Check(process.ExitCode == 0 && output.Trim() == (available ? "free" : "busy"), label + ": " + output + process.StandardError.ReadToEnd());
    }

    private static void DescriptorsAndMappings(string directory)
    {
        int error = 0;
        using var context = new PosixContext(e => error = e, directory);
        int fd = Open(context, "mapped.bin", 2 | 64 | 128);
        Check(fd >= 3, "create descriptor");
        int page = Environment.SystemPageSize;
        Check(context.Ftruncate(fd, page * 4L) == 0, "extend file");
        byte* data = stackalloc byte[] { 11, 22, 33, 44 };
        Check(context.Write(fd, data, 4) == 4, "write advances cursor");
        Check(context.Lseek(fd, 0, 1) == 4, "cursor after write");
        Check(context.Pwrite(fd, data, 4, page) == 4, "pwrite");
        Check(context.Lseek(fd, 0, 1) == 4, "pwrite preserves cursor");
        byte* read = stackalloc byte[4];
        Check(context.Pread(fd, read, 4, page) == 4 && read[2] == 33, "pread bytes");
        Check(context.Lseek(fd, 0, 1) == 4, "pread preserves cursor");
        PosixStat stat;
        Check(context.Fstat(fd, &stat) == 0 && stat.st_size == page * 4L, "portable stat");
        Check(context.Mmap(null, 0, 1, 1, fd, 0) == (void*)(-1) && error == 22, "zero length rejected");
        Check(context.Mmap(null, 1, 1, 1, fd, 1) == (void*)(-1) && error == 22, "unaligned offset rejected");
        Check(context.Mmap(null, (nuint)page, 1, 0x11, fd, 0) == (void*)(-1) && error == 95, "fixed mapping rejected");
        byte* shared = (byte*)context.Mmap(null, (nuint)page, 3, 1, fd, page);
        Check(shared != (void*)(-1) && shared[1] == 22, "shared mapping at page offset");
        shared[1] = 81;
        Check(context.Pread(fd, read, 4, page) == 4 && read[1] == 81, "shared write visible to descriptor");
        byte* copy = (byte*)context.Mmap(null, (nuint)page, 3, 2, fd, page);
        Check(copy != (void*)(-1) && copy[1] == 81, "private map");
        copy[1] = 93;
        Check(shared[1] == 81 && context.Pread(fd, read, 4, page) == 4 && read[1] == 81, "copy on write does not change file");
        int readonlyFd = Open(context, "mapped.bin", 0);
        Check(readonlyFd >= 0, "readonly descriptor");
        Check(context.Mmap(null, (nuint)page, 3, 1, readonlyFd, 0) == (void*)(-1) && error == 13, "readonly fd rejects shared writer");
        byte* readonlyMap = (byte*)context.Mmap(null, (nuint)page, 1, 1, readonlyFd, 0);
        Check(readonlyMap != (void*)(-1) && readonlyMap[0] == 11, "readonly map");
        byte* readonlyCopy = (byte*)context.Mmap(null, (nuint)page, 3, 2, readonlyFd, 0);
        Check(readonlyCopy != (void*)(-1), "private writer from readonly fd");
        readonlyCopy[0] = 97;
        Check(readonlyMap[0] == 11, "readonly private copy isolation");
        Check(context.Close(readonlyFd) == 0 && readonlyMap[0] == 11, "readonly map survives close");
        Check(context.Close(fd) == 0 && shared[1] == 81 && copy[1] == 93, "maps survive descriptor close");
        Check(context.Munmap(shared, (nuint)page) == 0 && context.Munmap(copy, (nuint)page) == 0, "release file maps");
        Check(context.Munmap(readonlyMap, (nuint)page) == 0 && context.Munmap(readonlyCopy, (nuint)page) == 0, "release readonly maps");
        byte* anonymous = (byte*)context.Mmap(null, (nuint)(page * 3), 3, 0x22, -1, 0);
        Check(anonymous != (void*)(-1) && anonymous[0] == 0 && anonymous[page * 3 - 1] == 0, "anonymous zero storage");
        anonymous[0] = 51; anonymous[page * 2] = 71;
        if (OperatingSystem.IsWindows())
        {
            Check(context.Munmap(anonymous + page, (nuint)page) == -1 && error == 95, "Windows partial unmap explicitly unsupported");
            Check(anonymous[0] == 51 && anonymous[page * 2] == 71, "unsupported partial leaves view intact");
            Check(context.Munmap(anonymous, (nuint)(page * 3)) == 0, "Windows full unmap");
        }
        else
        {
            Check(context.Munmap(anonymous + page, (nuint)page) == 0, "Unix middle partial unmap");
            Check(context.MappedViewCount == 2 && anonymous[0] == 51 && anonymous[page * 2] == 71, "partial retains both live segments");
            Check(context.Munmap(anonymous, (nuint)page) == 0 && context.Munmap(anonymous + page * 2, (nuint)page) == 0, "release remaining segments");
        }
        Check(context.MappedViewCount == 0 && context.MappingCreatedCount == 5, "mapping counters");
        Check(context.Munmap(anonymous, (nuint)page) == -1 && error == 22, "stale unmap rejected");
        Check(context.Read(fd, read, 4) == -1 && error == 9, "stale descriptor rejected");
        Check(context.Mmap(null, (nuint)page, 3, 0x22, -1, 0) != (void*)(-1), "owner disposal mapping");
        context.Dispose();
        Check(context.MappedViewCount == 0 && context.OpenDescriptorCount == 0, "dispose releases mappings and descriptors");
        Check(context.Mmap(null, (nuint)page, 3, 0x22, -1, 0) == (void*)(-1) && error == 9, "disposed mapping rejected");
    }

    private static void Locks(string directory, string? nativeProbe)
    {
        int errorA = 0, errorB = 0;
        using var a = new PosixContext(e => errorA = e, directory);
        using var b = new PosixContext(e => errorB = e, directory);
        int first = Open(a, "locks.bin", 2 | 64), sameOwner = Open(a, "locks.bin", 2), other = Open(b, "locks.bin", 2);
        Check(first >= 0 && sameOwner >= 0 && other >= 0, "lock descriptors");
        Check(Control(a, first, 6, Lock(0)) == 0 && Control(b, other, 6, Lock(0)) == 0, "shared locks coexist");
        NativeProbe(nativeProbe, Path.Combine(directory, "locks.bin"), false, "native writer sees shared lock");
        Check(Control(a, first, 6, Lock(1)) == -1 && errorA == 11, "upgrade conflict");
        PosixFlock query = Lock(1);
        Check(b.Fcntl(other, 5, &query) == 0 && query.l_type == 0 && query.l_start == 40 && query.l_len == 20, "GETLK reports other virtual owner");
        Check(Control(b, other, 6, Lock(2)) == 0 && Control(a, first, 6, Lock(1)) == 0, "exclusive upgrade after peer unlock");
        fixed (byte* path = "locks.bin\0"u8)
        {
            PosixTimeval* times = stackalloc PosixTimeval[2];
            times[0] = new() { tv_sec = 1700000000, tv_usec = 123456 };
            times[1] = new() { tv_sec = 1700001000, tv_usec = 654321 };
            Check(a.Utimes((sbyte*)path, times) == 0, "utimes while exclusively locked");
            NativeProbe(nativeProbe, Path.Combine(directory, "locks.bin"), false, "utimes preserves process-owned kernel lock");
            Check(a.Utimes((sbyte*)path, null) == 0, "utimes current timestamp while exclusively locked");
            NativeProbe(nativeProbe, Path.Combine(directory, "locks.bin"), false, "utimes current timestamp preserves kernel lock");
        }
        Check(Control(b, other, 6, Lock(0)) == -1 && errorB == 11, "exclusive blocks reader");
        Check(a.Close(sameOwner) == 0, "closing any owner descriptor releases its locks");
        Check(Control(b, other, 6, Lock(1)) == 0, "peer acquires after same owner descriptor close");
        Check(a.Close(first) == 0, "close unrelated owner descriptor while peer locked");
        NativeProbe(nativeProbe, Path.Combine(directory, "locks.bin"), false, "close preserves other virtual owner kernel lock");
        Check(Control(b, other, 6, Lock(2)) == 0, "release exclusive");
        NativeProbe(nativeProbe, Path.Combine(directory, "locks.bin"), true, "kernel lock released");
        first = Open(a, "locks.bin", 2);
        Check(Control(a, first, 6, Lock(1)) == 0, "blocking wait setup");
        using var waiting = new ManualResetEventSlim();
        Task<int> pending = Task.Run(() => { waiting.Set(); return Control(b, other, 7, Lock(1)); });
        waiting.Wait(); Thread.Sleep(50);
        Check(!pending.IsCompleted, "F_SETLKW blocks on conflict");
        Check(Control(a, first, 6, Lock(2)) == 0 && pending.Wait(5000) && pending.Result == 0, "blocking lock resumes after release");
        Check(Control(b, other, 6, Lock(2)) == 0, "blocking lock release");
        Check(Control(a, first, 6, Lock(1)) == 0 && Control(a, first, 6, Lock(2, 45, 5)) == 0, "partial unlock splits range");
        Check(Control(b, other, 6, Lock(1, 45, 5)) == 0 && Control(b, other, 6, Lock(0, 40, 5)) == -1 && errorB == 11, "partial unlock frees only selected bytes");
        query = Lock(1, 42, 1);
        Check(b.Fcntl(other, 5, &query) == 0 && query.l_type == 1 && query.l_start == 40 && query.l_len == 5, "GETLK reports split extent");
        Check(Control(a, first, 6, Lock(2, 0, 0)) == 0 && Control(b, other, 6, Lock(2, 0, 0)) == 0, "zero length unlock extends to EOF");
        if (nativeProbe is not null)
        {
            var start = new ProcessStartInfo(nativeProbe) { RedirectStandardOutput = true, RedirectStandardInput = true, RedirectStandardError = true };
            start.ArgumentList.Add(Path.Combine(directory, "locks.bin")); start.ArgumentList.Add("hold");
            using Process process = Process.Start(start)!;
            try
            {
                Check(process.StandardOutput.ReadLine() == "held", "native process holds lock");
                query = Lock(1);
                Check(a.Fcntl(first, 5, &query) == 0 && query.l_type == 1 && query.l_start == 40 && query.l_len == 20 && query.l_pid == process.Id, "GETLK sees native process and pid");
                Check(Control(a, first, 6, Lock(1)) == -1 && errorA is 11 or 13, "native process blocks managed owner");
            }
            finally
            {
                process.StandardInput.WriteLine("release"); process.StandardInput.Flush();
                if (!process.WaitForExit(5000)) { process.Kill(); throw new Exception("native lock holder did not finish"); }
            }
            Check(process.ExitCode == 0 && Control(a, first, 6, Lock(1)) == 0, "managed owner acquires after native close");
            Check(Control(a, first, 6, Lock(2)) == 0, "native interoperability cleanup");
        }
    }

    private static void PathsAndAppend(string directory)
    {
        Check(PosixContext.PortableWindowsPath(@"C:\dir\name") == "/C:/dir/name", "Windows portable drive path");
        Check(PosixContext.ResolveWindowsPath("/C:/dir/name", @"D:\cwd") == @"C:\dir\name", "Windows native drive path");
        Check(PosixContext.PortableWindowsPath(@"\\host\share\name") == "/unc/host/share/name", "Windows portable UNC path");
        Check(PosixContext.ResolveWindowsPath("/unc/host/share/name", @"D:\cwd") == @"\\host\share\name", "Windows native UNC path");
        Check(PosixContext.ResolveWindowsPath("/tmp", @"D:\cwd") == Path.GetTempPath(), "Windows portable temp mount");
        int error = 0;
        using var context = new PosixContext(e => error = e, directory);
        fixed (byte* name = "append.bin\0"u8)
        {
            int fd = context.Open((sbyte*)name, 0x42, 384);
            byte* data = stackalloc byte[] { 123, 45 };
            Check(fd >= 3 && context.Write(fd, data, 2) == 2 && context.FcntlFlags(fd, 4, 0x400) == 0, "enable append after open");
            Check(context.Lseek(fd, 0, 0) == 0 && context.Write(fd, data, 2) == 2, "append ignores cursor");
            PosixStat stat;
            Check(context.Fstat(fd, &stat) == 0 && stat.st_size == 4, "append grows file");
            Check(context.Pwrite(fd, data, 2, 0) == 2 && context.Fstat(fd, &stat) == 0 && stat.st_size == 4, "pwrite ignores O_APPEND");
            Check(context.Fsync(fd) == 0 && context.Close(fd) == 0, "sync and close");
            Check(context.Open((sbyte*)name, 0xc2, 384) == -1 && error == 17, "exclusive create existing");
            Check(context.Access((sbyte*)name, 6) == 0 && context.Unlink((sbyte*)name) == 0, "access and unlink");
            Check(context.Unlink((sbyte*)name) == -1 && error == 2, "missing unlink errno");
        }
        fixed (byte* name = "sub\0"u8)
        {
            Check(context.Mkdir((sbyte*)name, 448) == 0, "mkdir");
            Check(context.Mkdir((sbyte*)name, 448) == -1 && error == 17, "mkdir existing errno");
            Check(context.Rmdir((sbyte*)name) == 0, "rmdir");
        }
        fixed (byte* name = "/dev/urandom\0"u8)
        {
            int fd = context.Open((sbyte*)name, 0); byte* random = stackalloc byte[128];
            Check(fd >= 3 && context.Read(fd, random, 128) == 128, "portable random device");
            Check(context.Close(fd) == 0, "close random device");
        }
    }

    private static void Time(string directory)
    {
        int error = 0;
        using var context = new PosixContext(e => error = e, directory);
        PosixTimeval now;
        Check(context.Gettimeofday(&now, null) == 0 && Math.Abs(now.tv_sec - DateTimeOffset.UtcNow.ToUnixTimeSeconds()) < 2 && now.tv_usec >= 0 && now.tv_usec < 1000000, "gettimeofday");
        Check(context.Sysconf(30) == Environment.SystemPageSize && context.Sysconf(-1) == -1 && error == 22, "sysconf page size and invalid name");
        Check(context.Getpid() == Environment.ProcessId, "process id");
        var watch = Stopwatch.StartNew();
        Check(context.Usleep(3000) == 0 && watch.Elapsed.TotalMilliseconds >= 2.5, "usleep waits");
        Check(context.Sleep(0) == 0, "sleep zero");
        byte[] path = Encoding.UTF8.GetBytes("mapped.bin\0");
        PosixTimeval* times = stackalloc PosixTimeval[2];
        times[0] = new() { tv_sec = 1700000000, tv_usec = 123456 };
        times[1] = new() { tv_sec = 1700001000, tv_usec = 654321 };
        fixed (byte* p = path)
        {
            Check(context.Utimes((sbyte*)p, times) == 0, "utimes");
            PosixStat stat;
            Check(context.Stat((sbyte*)p, &stat) == 0 && stat.st_atim.tv_sec == 1700000000 &&
                stat.st_atim.tv_nsec == 123456000 && stat.st_mtim.tv_sec == 1700001000 &&
                stat.st_mtim.tv_nsec == 654321000,
                $"portable stat preserves timestamp fractions: {stat.st_atim.tv_sec}/{stat.st_atim.tv_nsec}, {stat.st_mtim.tv_sec}/{stat.st_mtim.tv_nsec}");
            string name = Path.Combine(directory, "mapped.bin");
            Check(Math.Abs((File.GetLastWriteTimeUtc(name) - DateTime.UnixEpoch).TotalSeconds - 1700001000.654321) < .00001, "utimes write microsecond precision");
            Check(Math.Abs((File.GetLastAccessTimeUtc(name) - DateTime.UnixEpoch).TotalSeconds - 1700000000.123456) < .00001, "utimes access microsecond precision");
            DateTime access = File.GetLastAccessTimeUtc(name), write = File.GetLastWriteTimeUtc(name);
            times[0].tv_sec += 100;
            times[1].tv_usec = 1000000;
            Check(context.Utimes((sbyte*)p, times) == -1 && error == 22, "utimes validates both values before updating");
            Check(File.GetLastAccessTimeUtc(name) == access && File.GetLastWriteTimeUtc(name) == write, "invalid second timestamp leaves both timestamps intact");
        }
    }

    public static int Main(string[] args)
    {
        Check(sizeof(PosixStat) == 120 && sizeof(PosixTimespec) == 16, "portable stat/timespec layout");
        var beforeEpoch = PosixPlatform.WindowsTimespec(116444736000000000L - 1);
        Check(beforeEpoch.tv_sec == -1 && beforeEpoch.tv_nsec == 999999900, "Windows pre-epoch timestamp normalization");
        string directory = Path.Combine(Path.GetTempPath(), "edg-posix-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try { DescriptorsAndMappings(directory); PathsAndAppend(directory); Locks(directory, args.Length == 0 ? null : args[0]); Time(directory); }
        finally { Directory.Delete(directory, true); }
        Console.WriteLine("POSIX runtime checks passed: " + checks);
        return 0;
    }
}
