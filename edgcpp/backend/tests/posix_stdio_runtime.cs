#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Edg.Runtime;

unsafe class Program
{
    static int checks;
    static void Check(bool condition, string name) { if (!condition) throw new Exception(name); checks++; }
    static byte[] Z(string value) => Encoding.UTF8.GetBytes(value + '\0');
    static int Main()
    {
        string directory = Path.Combine(Path.GetTempPath(), "edg-posix-stdio-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        int error = 0;
        try
        {
            using var posix = new PosixContext(value => error = value, directory);
            using var input = new MemoryStream("input"u8.ToArray());
            using var output = new MemoryStream();
            using var stderr = new MemoryStream();
            using var files = new LibcFiles(value => error = value, input, output, stderr, directory, fileDescriptors: posix);
            Check(files.Fileno(files.Stdin) == 0 && files.Fileno(files.Stdout) == 1 && files.Fileno(files.Stderr) == 2, "standard fd namespace");
            Check(posix.OpenDescriptorCount == 0, "standard fds excluded from owned file count");
            Check(posix.Isatty(1) == 0 && error == 25, "custom output is not tty");
            Check(posix.Isatty(999) == 0 && error == 9, "invalid tty fd");
            Check(files.Fgetc(files.Stdin) == 'i', "stdio standard input");
            byte marker = 42;
            Check(posix.Write(1, &marker, 1) == 1 && output.ToArray()[0] == marker, "descriptor standard output");
            fixed (byte* template = Z("temp-XXXXXX"))
            {
                int fd = posix.Mkstemp((sbyte*)template);
                Check(fd >= 3, "mkstemp creates descriptor");
                string name = Marshal.PtrToStringUTF8((nint)template)!;
                Check(!name.EndsWith("XXXXXX") && File.Exists(Path.Combine(directory, name)), "mkstemp replaces template");
                if (!OperatingSystem.IsWindows()) Check((File.GetUnixFileMode(Path.Combine(directory, name)) & (UnixFileMode)511) == (UnixFileMode)384, "mkstemp0600");
                fixed (byte* mode = Z("w+"))
                {
                    void* file = files.Fdopen(fd, (sbyte*)mode);
                    Check(file != null && files.Fileno(file) == fd, "fdopen preserves fd");
                    Check(files.Fputc('A', file) == 'A' && files.Fflush(file) == 0, "stdio flush to fd");
                    Check(posix.Lseek(fd, 0, 1) == 1, "stdio and descriptor position shared");
                    Check(posix.Write(fd, &marker, 1) == 1, "fd write shares FILE");
                    PosixStat stat;
                    Check(posix.Fstat(fd, &stat) == 0 && stat.st_size == 2, "fstat FILE descriptor");
                    Check(files.Fseek(file, 0, 0) == 0 && files.Fgetc(file) == 'A' && files.Fgetc(file) == marker, "FILE reads fd bytes");
                    Check(files.Fclose(file) == 0 && posix.Fstat(fd, &stat) == -1 && error == 9, "fclose closes same fd");
                    Check(files.Fileno(file) == -1 && error == 9, "closed FILE fileno");
                }
                Check(posix.Unlink((sbyte*)template) == 0, "temporary unlink");
            }
            fixed (byte* bad = Z("badXXX")) Check(posix.Mkstemp((sbyte*)bad) == -1 && error == 22, "mkstemp template validation");
            fixed (byte* name = Z("ordinary"))
            fixed (byte* write = Z("w+"))
            fixed (byte* read = Z("r"))
            fixed (byte* append = Z("a"))
            {
                void* file = files.Fopen((sbyte*)name, (sbyte*)write);
                Check(file != null && files.Fileno(file) >= 3, "fopen fd registry");
                int fd = files.Fileno(file);
                Check(files.Fputc('X', file) == 'X' && files.Fclose(file) == 0, "fclose flush before descriptor close");
                file = files.Fopen((sbyte*)name, (sbyte*)read); fd = files.Fileno(file);
                Check(files.Fdopen(fd, (sbyte*)write) == null && error == 9, "fdopen mode cannot upgrade access");
                Check(files.Fgetc(file) == 'X', "failed fdopen keeps fd alive");
                void* mapped = posix.Mmap(null, 1, 1, 2, fd, 0);
                Check(mapped != (void*)(-1) && *(byte*)mapped == 'X', "FILE descriptor maps native backing");
                Check(posix.Munmap(mapped, 1) == 0 && files.Fclose(file) == 0, "mapped FILE cleanup");
                fd = posix.Open((sbyte*)name, 1);
                file = files.Fdopen(fd, (sbyte*)append);
                Check(file != null && files.Fputc('Y', file) == 'Y' && files.Fclose(file) == 0, "fdopen append");
                Check(File.ReadAllText(Path.Combine(directory, "ordinary")) == "XY", "fdopen append did not truncate");
            }
            void* temp = files.Tmpfile();
            Check(temp != null && files.Fileno(temp) >= 3 && files.Fclose(temp) == 0, "tmpfile fd registry");
            Directory.CreateDirectory(Path.Combine(directory, "sub"));
            fixed (byte* sub = Z("sub")) Check(posix.Chdir((sbyte*)sub) == 0, "virtual chdir");
            fixed (byte* local = Z("local")) fixed (byte* mode = Z("w"))
            {
                void* file = files.Fopen((sbyte*)local, (sbyte*)mode);
                Check(file != null && files.Fclose(file) == 0 && File.Exists(Path.Combine(directory, "sub/local")), "FILE follows virtual chdir");
            }
            fixed (byte* cwd = new byte[4096])
                Check(posix.Getcwd((sbyte*)cwd, 4096) != null && Marshal.PtrToStringUTF8((nint)cwd)!.Replace('\\', '/').EndsWith("/sub"), "virtual getcwd");
            fixed (byte* path = Z("."))
            {
                void* owner = posix.Opendir((sbyte*)path);
                Check(owner != null, "opendir");
                var names = new HashSet<string>();
                error = 0;
                while (true) { void* entry = posix.Readdir(owner); if (entry == null) break; names.Add(Marshal.PtrToStringUTF8((nint)entry)!); }
                Check(names.SetEquals(new[] { ".", "..", "local" }) && error == 0, "readdir portable entries and clean EOF");
                Check(posix.Closedir(owner) == 0 && posix.Readdir(owner) == null && error == 9, "closed directory tombstone");
                Check(posix.Closedir(owner) == -1 && error == 9, "double directory close");
            }
            fixed (byte* path = Z("local")) Check(posix.Opendir((sbyte*)path) == null && error == 20, "opendir regular file rejects");
            PosixRlimit limit;
            Check(posix.Getrlimit(0, &limit) == 0 && limit.rlim_cur <= limit.rlim_max, "get CPU limit");
            Check(posix.Setrlimit(0, &limit) == 0, "restore same CPU limit");
            PosixRlimit finite = limit; finite.rlim_cur = Math.Min(3600UL, limit.rlim_max);
            if (OperatingSystem.IsWindows())
                Check(posix.Setrlimit(0, &finite) == -1 && error == 95, "finite Windows CPU limit explicitly unsupported");
            else
            {
                Check(posix.Setrlimit(0, &finite) == 0, "install real finite CPU limit");
                PosixRlimit actual;
                Check(posix.Getrlimit(0, &actual) == 0 && actual.rlim_cur == finite.rlim_cur, "read actual finite CPU limit");
                Check(posix.Setrlimit(0, &limit) == 0, "restore original CPU limit");
            }
            PosixRlimit invalid = new() { rlim_cur = 2, rlim_max = 1 };
            Check(posix.Setrlimit(0, &invalid) == -1 && error == 22, "invalid CPU limit");
            Check(posix.Getrlimit(999, &limit) == -1 && error == 22, "unknown resource");
            Check(posix.OpenDescriptorCount == 0, "all FILE descriptors reclaimed");
            files.Fputc('Z', files.Stdout);
            files.Dispose();
            Check(output.ToArray()[^1] == 'Z', "FILE shutdown flushes against live fd registry");
            Check(output.CanWrite, "standard host stream not disposed");
            using (var nativePosix = new PosixContext(value => error = value, directory))
            using (var nativeFiles = new LibcFiles(value => error = value, fileDescriptors: nativePosix))
            {
                PosixStat nativeStat;
                Check(nativePosix.Fstat(nativeFiles.Fileno(nativeFiles.Stdout), &nativeStat) == 0 && nativeStat.st_mode != 0,
                    "real redirected standard stream native metadata");
            }
            Console.WriteLine($"POSIX stdio host integration: {checks} checks passed");
            return 0;
        }
        finally { Directory.Delete(directory, true); }
    }
}
