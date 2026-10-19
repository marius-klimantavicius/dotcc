using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using static DotCC.Libc.Libc;
#if !RECORD_LOCK_DIRECT
using Xunit;
#endif

namespace DotCC.Tests;

#if !RECORD_LOCK_DIRECT
[Collection("Runtime")]
public sealed class CPythonR9RecordLockTests
{
    [Fact] public void Invalid_arguments_and_pointer_widths() => CPythonR9RecordLockChecks.Arguments();
    [Fact] public void Descriptor_owners_and_close() => CPythonR9RecordLockChecks.Owners();
    [Fact] public void Record_locks_survive_queries_and_keep_position() => CPythonR9RecordLockChecks.Position();
    [Fact] public void Blocking_call_retains_original_handle_and_releases_table() => CPythonR9RecordLockChecks.BlockingClose();
    [Fact] public void Flock_has_real_conflicts_and_close_ownership() => CPythonR9RecordLockChecks.Flock();
}
#endif

// Shared by ordinary unit discovery and the standalone JIT/AOT consumer of
// the actual DotCC.Libc project. Actual C ABI proof is in the separate C probe.
public static unsafe class CPythonR9RecordLockChecks
{
    private static int assertions;
    private static void Check(bool value)
    {
        if (!value) throw new Exception("record-lock assertion " + (assertions + 1) + " errno=" + errno);
        assertions++;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct LockRecord
    {
        public short Type, Whence;
        public long Start, Length;
        public int Pid;
    }

    [DllImport("libc", EntryPoint = "fcntl", SetLastError = true)]
    private static extern int NativeFcntl(int fd, int command, LockRecord* value);
    [DllImport("libc", EntryPoint = "flock", SetLastError = true)]
    private static extern int NativeFlock(int fd, int operation);
    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int NativeOpen(byte* path, int flags);

    private static int Open(string path, int flags = 2)
    {
        fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0")) return open(name, flags);
    }

    // OFD queries are an independent kernel observer: unlike F_GETLK they see
    // this process's traditional locks, without starting another managed engine.
    private static LockRecord Observe(SafeFileHandle observer, long start, long length = 1)
    {
        var value = new LockRecord { Type = 1, Start = start, Length = length };
        Check(NativeFcntl((int)observer.DangerousGetHandle(), 36, &value) == 0);
        return value;
    }

    private static SafeFileHandle Observer(string path)
    {
        // A normal File.OpenHandle observer would itself silently acquire a
        // BCL shared flock, unlike this independent native open control.
        fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0"))
        {
            int fd = NativeOpen(name, 2 | 0x80000);
            Check(fd >= 0);
            return new SafeFileHandle((nint)fd, ownsHandle: true);
        }
    }

    public static int RunAll()
    {
        assertions = 0;
        Arguments(); Owners(); Position(); BlockingClose(); Flock();
        return assertions;
    }

    public static void Arguments()
    {
        using var owner = new RuntimeContext();
        using var scope = owner.Enter();
        var file = tmpfile();
        Check(file != null);
        int fd = fileno(file);
        try
        {
            Check(fcntl(-1, 5, (void*)0) == -1 && errno == EBADF);
            Check(fcntl(fd, 5, (void*)0) == -1 && errno == EFAULT);
            Check(fcntl(fd, 5, (void*)1) == -1 && errno == EFAULT);
            Check(fcntl(fd, 5, 1) == -1 && errno == EFAULT);
            Check(fcntl(fd, 5, 1L) == -1 && errno == EFAULT);
            Check(fcntl(fd, 5, 1UL) == -1 && errno == EFAULT);
            var value = new LockRecord { Type = 1, Start = 5368709123, Length = 10 };
            Check(fcntl(fd, 6, (long)(nint)(&value)) == 0);
            value.Type = 2;
            Check(fcntl(fd, 6, (ulong)(nuint)(&value)) == 0);
            Check(fcntl(fd, 3, (void*)1) == 2);
            Check(fcntl(fd, 2, (void*)1) == 0 && fcntl(fd, 1) == 1);
            Check(fcntl(fd, 999, &value) == -1 && errno == EINVAL);
            Check(fcntl(0, 5, &value) == -1 && errno == ENOTSUP);
            Check(flock(-1, 2) == -1 && errno == EBADF);
            Check(flock(0, 2) == -1 && errno == ENOTSUP);
            Check(flock(fd, 16) == -1 && errno == EINVAL);
        }
        finally { fclose(file); }
    }

    public static void Owners()
    {
        string a = Path.GetTempFileName(), b = Path.GetTempFileName();
        using var observerA = Observer(a);
        using var observerB = Observer(b);
        var first = new RuntimeContext();
        using var second = new RuntimeContext();
        int descriptor;
        var value = new LockRecord { Type = 1, Start = 9, Length = 7 };
        try
        {
            using (first.Enter())
            {
                descriptor = Open(a);
                Check(descriptor == 3 && fcntl(descriptor, 6, &value) == 0);
            }
            using (second.Enter())
            {
                int own = Open(b);
                Check(own == descriptor && fcntl(own, 6, &value) == 0);
                Check(Observe(observerA, 9).Pid == Environment.ProcessId);
                Check(Observe(observerB, 9).Pid == Environment.ProcessId);
                first.Dispose();
                Check(Observe(observerA, 9).Type == 2);
                Check(Observe(observerB, 9).Type == 1);
                Check(close(own) == 0);
                Check(Observe(observerB, 9).Type == 2);
                Check(fcntl(own, 5, &value) == -1 && errno == EBADF);
            }
        }
        finally { first.Dispose(); File.Delete(a); File.Delete(b); }
    }

    public static void Position()
    {
        string path = Path.GetTempFileName();
        File.WriteAllBytes(path, new byte[256]);
        using var observer = Observer(path);
        using var owner = new RuntimeContext();
        using var scope = owner.Enter();
        int fd = Open(path);
        var value = new LockRecord { Type = 1, Whence = 1, Start = -5, Length = 10 };
        try
        {
            Check(lseek(fd, 45, 0) == 45);
            Check(fcntl(fd, 6, &value) == 0);
            var found = Observe(observer, 40);
            Check(found.Type == 1 && found.Start == 40 && found.Length == 10 && found.Pid == Environment.ProcessId);
            Check(value.Whence == 1 && value.Start == -5 && lseek(fd, 0, 1) == 45);
            for (int i = 0; i < 3; i++)
            {
                value.Type = 1;
                Check(fcntl(fd, 5, &value) == 0 && value.Type == 2);
                Check(Observe(observer, 40).Type == 1); // no hidden dup/close
            }
            byte data = 0x6d;
            Check(write(fd, &data, 1) == 1 && lseek(fd, 0, 1) == 46);
            value = new() { Type = 1, Whence = 1, Start = 10, Length = 1 };
            Check(fcntl(fd, 6, &value) == 0);
            Check(Observe(observer, 56).Start == 56);
            Check(lseek(fd, 45, 0) == 45);
            data = 0;
            Check(read(fd, &data, 1) == 1 && data == 0x6d);
            Check(close(fd) == 0 && Observe(observer, 40).Type == 2);
        }
        finally { close(fd); File.Delete(path); }
    }

    public static void BlockingClose()
    {
        string path = Path.GetTempFileName();
        using var observer = Observer(path);
        using var owner = new RuntimeContext();
        int fd;
        using (owner.Enter()) fd = Open(path);
        var blocker = new LockRecord { Type = 1, Length = 1 };
        Check(NativeFcntl((int)observer.DangerousGetHandle(), 37, &blocker) == 0);
        using var entered = new ManualResetEventSlim();
        using var done = new ManualResetEventSlim();
        int result = -99;
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var scope = owner.Enter();
                var value = new LockRecord { Type = 1, Length = 1 };
                entered.Set();
                result = fcntl(fd, 7, &value);
            }
            catch (Exception error) { failure = error; }
            finally { done.Set(); }
        });
        thread.IsBackground = true;
        try
        {
            thread.Start();
            Check(entered.Wait(3000));
            Check(!done.Wait(150));
            using (owner.Enter())
            {
                // This operation must not wait on a table lock held by SETLKW.
                Check(close(fd) == 0);
                var unrelated = tmpfile();
                Check(unrelated != null && fclose(unrelated) == 0);
            }
        }
        finally
        {
            blocker.Type = 2;
            NativeFcntl((int)observer.DangerousGetHandle(), 37, &blocker);
            bool joined = thread.Join(5000);
            File.Delete(path);
            Check(joined);
        }
        Check(failure == null && result == 0);
        Check(Observe(observer, 0).Type == 2); // retained close completes on return
    }

    public static void Flock()
    {
        string path = Path.GetTempFileName();
        using var observer = Observer(path);
        using var owner = new RuntimeContext();
        using var scope = owner.Enter();
        int fd = Open(path), other = Open(path);
        int host = (int)observer.DangerousGetHandle();
        try
        {
            Check(fd >= 0 && other >= 0);
            Check(flock(fd, 2 | 4) == 0);
            Check(NativeFlock(host, 2 | 4) == -1 && Marshal.GetLastPInvokeError() == 11);
            Check(flock(other, 1 | 4) == -1 && errno == EAGAIN);
            Check(close(other) == 0); // distinct open description must not unlock fd
            Check(NativeFlock(host, 2 | 4) == -1 && Marshal.GetLastPInvokeError() == 11);
            Check(flock(fd, 1 | 4) == 0 && NativeFlock(host, 1 | 4) == 0);
            Check(flock(fd, 2 | 4) == -1 && errno == EAGAIN);
            Check(NativeFlock(host, 8) == 0);
            Check(flock(fd, 2 | 4) == 0);
            Check(close(fd) == 0 && NativeFlock(host, 2 | 4) == 0);
            Check(NativeFlock(host, 8) == 0);
        }
        finally { close(fd); close(other); File.Delete(path); }
    }
}
