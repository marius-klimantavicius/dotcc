#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Edg.Runtime;
using Microsoft.Win32.SafeHandles;

unsafe partial class Program
{
    private static int checks;
    private static void Check(bool value, string name) { if (!value) throw new Exception(name); checks++; }
    private static int Open(PosixContext p, string path, int flags) { fixed (byte* n = Encoding.UTF8.GetBytes(path + "\0")) return p.Open((sbyte*)n, flags, 384); }
    private static int Lock(PosixContext p, int fd, short type, long start = 40, long length = 20)
    { PosixFlock f = new() { l_type = type, l_start = start, l_len = length }; return p.Fcntl(fd, 6, &f); }
    private static string Probe(string helper, string file)
    {
        var start = new ProcessStartInfo(helper) { RedirectStandardOutput = true };
        start.ArgumentList.Add(file); start.ArgumentList.Add("probe");
        using var child = Process.Start(start)!; string output = child.StandardOutput.ReadToEnd().Trim();
        child.WaitForExit(); Check(child.ExitCode == 0, "native probe exit"); return output;
    }
    public static void Main(string[] args)
    {
        string file = Path.GetTempFileName();
        try
        {
            int error = 0;
            using var owner = new PosixContext(e => error = e);
            int ro = Open(owner, file, 0), rw = Open(owner, file, 2);
            Check(ro >= 0 && rw >= 0, "open descriptors");
            Check(Lock(owner, ro, 0) == 0, "readonly anchor read lock");
            Check(Lock(owner, rw, 1) == 0, "readwrite anchor migration and exclusive upgrade");
            Check(Probe(args[0], file) == "busy", "native process observes exclusive lock");
            using (var unrelated = File.OpenHandle(file, FileMode.Open, FileAccess.Read)) { }
            Check(Probe(args[0], file) == "busy", "unrelated BCL close cannot drop lock");
            using (var unrelated = File.OpenHandle(file, FileMode.Open, FileAccess.ReadWrite))
            {
                // Same-process traditional POSIX locks still conflict with OFD
                // locks. This is the native SQLite / translated SQLite boundary.
                PosixFlock native = new() { l_type = 1, l_start = 40, l_len = 20 };
                if (OperatingSystem.IsLinux())
                    Check(NativeFcntl(unrelated, 6, &native) == -1 && Marshal.GetLastPInvokeError() is 11 or 13,
                        "same-process native POSIX writer conflicts");
            }
            using var isolated = new Copied.Runtime.PosixContext(_ => { });
            int other;
            fixed (byte* name = Encoding.UTF8.GetBytes(file + "\0")) other = isolated.Open((sbyte*)name, 2);
            Copied.Runtime.PosixFlock peer = new() { l_type = 1, l_start = 40, l_len = 20 };
            Check(other >= 0 && isolated.Fcntl(other, 6, &peer) == -1, "copied runtime registry cannot grant conflicting lock");
            Check(owner.Close(ro) == 0, "close any owner descriptor releases migrated locks");
            Check(isolated.Fcntl(other, 6, &peer) == 0, "copied runtime acquires after release");
            Check(owner.Close(rw) == 0, "close other owner descriptor");
            Check(Probe(args[0], file) == "busy", "copied runtime lock survives unrelated owner close");
            Check(isolated.Close(other) == 0 && Probe(args[0], file) == "free", "copied runtime lock release");

            rw = Open(owner, file, 2);
            Check(Lock(owner, rw, 1, 0, 10) == 0 && Lock(owner, rw, 0, 10, 30) == 0, "mixed-range downgrade setup");
            var start = new ProcessStartInfo(args[0]) { RedirectStandardOutput = true, RedirectStandardInput = true };
            start.ArgumentList.Add(file); start.ArgumentList.Add("hold");
            using (var child = Process.Start(start)!)
            {
                try
                {
                    Check(child.StandardOutput.ReadLine() == "held", "native holds tail range");
                    Check(Lock(owner, rw, 0, 0, 60) == -1 && error is 11 or 13, "failed encompassing read conversion");
                    using var observer = new PosixContext(_ => { }); int fd = Open(observer, file, 2);
                    PosixFlock query = new() { l_type = 0, l_start = 0, l_len = 10 };
                    Check(observer.Fcntl(fd, 5, &query) == 0 && query.l_type == 1, "failed downgrade preserves original exclusive range");
                    observer.Close(fd);
                }
                finally { child.StandardInput.WriteLine(); child.StandardInput.Flush(); if (!child.WaitForExit(5000)) child.Kill(); }
            }
            Check(owner.Close(rw) == 0, "range cleanup");
            Console.WriteLine("POSIX edge verification passed: " + checks + " checks");
        }
        finally { File.Delete(file); }
    }
    [LibraryImport("libc", EntryPoint = "fcntl", SetLastError = true)]
    private static partial int NativeFcntl(SafeFileHandle fd, int command, PosixFlock* record);
}
