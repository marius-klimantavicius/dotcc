using System.Runtime.InteropServices;
using System.Text;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    private static int assertions;
    private static string root = "";
    private static void Equal<T>(T actual, T expected, string what) where T : IEquatable<T>
    {
        if (!actual.Equals(expected)) throw new Exception($"{what}: expected {expected}, got {actual}; errno={errno}");
        assertions++;
    }
    private static void Check(bool actual, string what) => Equal(actual, true, what);
    private static int Open(string path, int flags, int mode = 438)
    { fixed (byte* p = Encoding.UTF8.GetBytes(path + "\0")) return open(p, flags, mode); }
    private static int Mkdir(string path, uint mode)
    { fixed (byte* p = Encoding.UTF8.GetBytes(path + "\0")) return mkdir(p, mode); }
    private static FILE* Fopen(string path, string mode)
    {
        fixed (byte* p = Encoding.UTF8.GetBytes(path + "\0"))
        fixed (byte* m = Encoding.UTF8.GetBytes(mode + "\0")) return fopen(p, m);
    }
    private static uint Mode(string path) => (uint)File.GetUnixFileMode(path) & 0xfff;
    private static uint HostMask() => Convert.ToUInt32(File.ReadLines("/proc/self/status").Single(x => x.StartsWith("Umask:")).Split(':')[1].Trim(), 8);
    private static string? KernelLinkTarget(string path)
    {
        byte* buffer = stackalloc byte[4096];
        fixed (byte* name = Encoding.UTF8.GetBytes(path + "\0"))
        {
            long length = NativeReadlink(name, buffer, 4096);
            return length < 0 ? null : Encoding.UTF8.GetString(buffer, (int)length);
        }
    }

    private static void Masks()
    {
        uint host = HostMask();
        errno = 901;
        Equal(umask(0x3f), host, "initial owner mask");
        Equal(errno, 901, "umask preserves errno");
        Equal(ApplyCreationMode(0xfff), 0xfc0u, "mask only permission bits");
        using (var other = new RuntimeContext())
        using (other.Enter())
        {
            Equal(umask(0), host, "independent owner initial mask");
            Equal(EffectiveCreationMask, 0u, "nested owner mask");
        }
        Equal(EffectiveCreationMask, 0x3fu, "outer owner mask retained");
        Equal(umask(uint.MaxValue), 0x3fu, "high input bits ignored");
        Equal(EffectiveCreationMask, 0x1ffu, "masked input");
        Equal(HostMask(), host, "host process mask unchanged");
        using var fresh = new RuntimeContext();
        using (fresh.Enter()) Equal(EffectiveCreationMask, host, "fresh owner inherits host policy");
        umask(0);
    }

    private static void Files()
    {
        string file = Path.Combine(root, "created");
        int fd = Open(file, 2 | 0x40 | 0x80, 0x1a0); // 0640
        Check(fd >= 3, "exclusive new open"); close(fd);
        Equal(Mode(file), 0x1a0u, "exact 0640 with guest mask zero");
        umask(0);
        string loose = Path.Combine(root, "loose");
        fd = Open(loose, 2 | 0x40, 0x1b6); Check(fd >= 3, "nonexclusive create"); close(fd);
        Equal(Mode(loose), 0x1b6u, "guest zero mask can relax host mask");
        umask(0x3f);
        fd = Open(file, 2 | 0x40, 0x1ff); Check(fd >= 3, "open existing"); close(fd);
        Equal(Mode(file), 0x1a0u, "existing mode unchanged");
        fd = Open(file, 2 | 0x40 | 0x200, 0); Check(fd >= 3, "truncate existing"); close(fd);
        Equal(Mode(file), 0x1a0u, "truncate does not chmod");
        Equal(Open(file, 2 | 0x40 | 0x80, 0), -1, "exclusive existing fails"); Equal(errno, EEXIST, "exclusive errno");
        string stdio = Path.Combine(root, "stdio");
        FILE* stream = Fopen(stdio, "wb"); Check(stream != null, "stdio create"); fclose(stream);
        Equal(Mode(stdio), 0x180u, "stdio respects owner mask");
        umask(0);
        stream = Fopen(stdio, "wb"); Check(stream != null, "stdio truncate existing"); fclose(stream);
        Equal(Mode(stdio), 0x180u, "stdio existing mode unchanged");
        string stdioLoose = Path.Combine(root, "stdio-loose");
        stream = Fopen(stdioLoose, "wb"); Check(stream != null, "stdio loose create"); fclose(stream);
        Equal(Mode(stdioLoose), 0x1b6u, "stdio zero mask exact mode");
        string zero = Path.Combine(root, "zero-file");
        fd = Open(zero, 2 | 0x40, 0); Check(fd >= 3, "zero mode create"); close(fd);
        Equal(Mode(zero), 0u, "zero file permissions retained");
    }

    private static void Symlinks()
    {
        string target = Path.Combine(root, "link-target");
        string link = Path.Combine(root, "dangling");
        File.CreateSymbolicLink(link, "link-target");
        Equal(Open(link, 2 | 0x40 | 0x80), -1, "exclusive dangling symlink"); Equal(errno, EEXIST, "dangling exclusive errno");
        Equal(Open(link, 2 | 0x40 | 0x20000), -1, "nofollow dangling symlink"); Equal(errno, ELOOP, "nofollow errno");
        int fd = Open(link, 2 | 0x40, 0x1a0); Check(fd >= 3, "follow dangling symlink and create target"); close(fd);
        Check(new FileInfo(link).LinkTarget != null, "preserve symlink");
        Equal(Mode(target), 0x1a0u, "dangling target exact mode");
        fd = Open(link, 2 | 0x40, 0x1ff); Check(fd >= 3, "follow existing symlink"); close(fd);
        Equal(Mode(target), 0x1a0u, "existing symlink target mode unchanged");
        string absolute = Path.Combine(root, "absolute");
        File.CreateSymbolicLink(absolute, Path.Combine(root, "absolute-target"));
        fd = Open(absolute, 2 | 0x40, 0x180); Check(fd >= 3, "absolute dangling link"); close(fd);
        Equal(Mode(Path.Combine(root, "absolute-target")), 0x180u, "absolute target mode");
        string cycle = Path.Combine(root, "cycle"); File.CreateSymbolicLink(cycle, "cycle");
        Equal(Open(cycle, 2 | 0x40), -1, "symlink cycle fails"); Equal(errno, ELOOP, "cycle errno");
        string missing = Path.Combine(root, "missing-link"); File.CreateSymbolicLink(missing, "missing-parent/leaf");
        Equal(Open(missing, 2 | 0x40), -1, "dangling link missing parent"); Equal(errno, ENOENT, "dangling missing parent errno");
    }

    private static void Directories()
    {
        string directory = Path.Combine(root, "directory");
        umask(0);
        Equal(Mkdir(directory, 0x1ff), 0, "atomic mkdir"); Equal(Mode(directory), 0x1ffu, "directory relaxes host mask");
        umask(0x3f);
        Equal(Mkdir(directory, 0x180), -1, "existing directory fails"); Equal(errno, EEXIST, "mkdir existing errno");
        Equal(Mode(directory), 0x1ffu, "existing directory mode unchanged");
        string strict = Path.Combine(root, "strict-directory");
        Equal(Mkdir(strict, 0x1ff), 0, "masked mkdir"); Equal(Mode(strict), 0x1c0u, "directory owner mask");
        string missing = Path.Combine(root, "absent-parent", "child");
        Equal(Mkdir(missing, 0x1ff), -1, "mkdir missing parent"); Equal(errno, ENOENT, "missing parent errno");
        Check(!Directory.Exists(Path.GetDirectoryName(missing)), "mkdir never creates ancestors");
        Equal(Mkdir(Path.Combine(root, "created"), 0x1ff), -1, "mkdir existing file"); Equal(errno, EEXIST, "mkdir file errno");
        string link = Path.Combine(root, "directory-link"); Directory.CreateSymbolicLink(link, directory);
        Equal(Mkdir(link, 0x180), -1, "mkdir existing symlink"); Equal(errno, EEXIST, "mkdir symlink errno");
        string zero = Path.Combine(root, "zero-directory");
        Equal(Mkdir(zero, 0), 0, "mode zero mkdir"); Equal(Mode(zero), 0u, "mode zero exact");
        File.SetUnixFileMode(zero, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        Equal(mkdir(null, 0), -1, "null mkdir"); Equal(errno, EFAULT, "null mkdir errno");
    }

    private static void Storage()
    {
        umask(0x3f);
        string path = Path.Combine(root, "allocation");
        int fd = Open(path, 2 | 0x40 | 0x80);
        Check(fd >= 3, "allocation file");
        try
        {
            byte marker = 0xa5; Equal(write(fd, &marker, 1), 1L, "initial byte");
            errno = 902;
            Equal(posix_fallocate(fd, 4096, 4096), 0, "reserve space"); Equal(errno, 902, "allocation preserves errno");
            Equal(new FileInfo(path).Length, 8192L, "allocation extends size");
            Equal(lseek(fd, 0, 1), 1L, "allocation preserves offset");
            byte* bytes = stackalloc byte[2]; Equal(pread(fd, bytes, 1, 0), 1L, "read marker"); Equal(bytes[0], (byte)0xa5, "allocation preserves data");
            Equal(pread(fd, bytes, 1, 8191), 1L, "read extension"); Equal(bytes[0], (byte)0, "extension zeroes");
            Equal(posix_fallocate(fd, -1, 1), EINVAL, "negative offset"); Equal(errno, 902, "invalid allocation preserves errno");
            Equal(posix_fallocate(fd, 0, 0), EINVAL, "zero allocation length");
            Equal(posix_fallocate(fd, long.MaxValue, 1), EFBIG, "allocation overflow");
            Equal(fdatasync(fd), 0, "real fdatasync");
            using (var foreign = new RuntimeContext())
            using (foreign.Enter())
            { Equal(posix_fallocate(fd, 0, 1), EBADF, "foreign descriptor rejected"); Equal(fdatasync(fd), -1, "foreign fdatasync rejected"); Equal(errno, EBADF, "foreign errno"); }
        }
        finally { close(fd); }
        Equal(posix_fallocate(fd, 0, 1), EBADF, "closed allocation descriptor");
        int readOnly = Open(path, 0);
        try { Equal(posix_fallocate(readOnly, 0, 1), EBADF, "readonly allocation"); }
        finally { close(readOnly); }
        int* pipes = stackalloc int[2]; Equal(pipe(pipes), 0, "owned pipe");
        try { Equal(posix_fallocate(pipes[1], 0, 1), ESPIPE, "nonseekable allocation"); }
        finally { close(pipes[0]); close(pipes[1]); }
        FILE* stream = Fopen(Path.Combine(root, "large-seek"), "w+b");
        Check(stream != null, "seek stream");
        try
        {
            Equal(fseeko(stream, (1L << 32) + 7, 0), 0, "large fseeko");
            Equal(ftello(stream), (1L << 32) + 7, "large ftello");
            Equal(fseeko(stream, 0, 77), -1, "invalid seek origin"); Equal(errno, EINVAL, "seek errno");
        }
        finally { fclose(stream); }
        Equal(fseeko(null, 0, 0), -1, "null fseeko"); Equal(errno, EBADF, "null stream errno");
    }

    private static void Limits()
    {
        ulong* native = stackalloc ulong[2];
        ulong* guest = stackalloc ulong[4]; guest[0] = 0xabcdef; guest[3] = 0x123456;
        for (int resource = 0; resource < 16; resource++)
        {
            Equal(NativeGetrlimit(resource, native), 0, "native rlimit query"); errno = 903;
            Equal(getrlimit(resource, guest + 1), 0, "guest rlimit query");
            Equal(guest[1], native[0], "real soft limit"); Equal(guest[2], native[1], "real hard limit"); Equal(errno, 903, "getrlimit success errno");
        }
        Equal(guest[0], 0xabcdefUL, "leading limit canary"); Equal(guest[3], 0x123456UL, "trailing limit canary");
        ulong previous = guest[1];
        Equal(getrlimit(-1, guest + 1), -1, "invalid resource"); Equal(errno, EINVAL, "invalid resource errno"); Equal(guest[1], previous, "failure preserves output");
        Equal(getrlimit(3, null), -1, "null rlimit"); Equal(errno, EFAULT, "null limit errno");
    }

    private static void Copies()
    {
        string input = Path.Combine(root, "copy-source"), output = Path.Combine(root, "copy-target");
        File.WriteAllText(input, "0123456789");
        int from = Open(input, 0), to = Open(output, 2 | 0x40 | 0x80);
        try
        {
            long offsetFrom = 3, offsetTo = 2;
            long copied = copy_file_range(from, &offsetFrom, to, &offsetTo, 4, 0);
            Equal(copied, 4L, "real filesystem copy"); Equal(offsetFrom, 7L, "explicit source offset advances"); Equal(offsetTo, 6L, "explicit target offset advances");
            Equal(lseek(from, 0, 1), 0L, "explicit copy does not seek source"); Equal(lseek(to, 0, 1), 0L, "explicit copy does not seek target");
            Equal(lseek(from, 1, 0), 1L, "source cursor setup"); Equal(lseek(to, 6, 0), 6L, "target cursor setup");
            Equal(copy_file_range(from, null, to, null, 3, 0), 3L, "implicit offset copy");
            Equal(lseek(from, 0, 1), 4L, "implicit copy advances source"); Equal(lseek(to, 0, 1), 9L, "implicit copy advances target");
            byte v = 0; Equal(read(from, &v, 1), 1L, "read follows copied cursor"); Equal(v, (byte)'4', "read sees correct next byte");
            byte* bytes = stackalloc byte[9]; Equal(pread(to, bytes, 9, 0), 9L, "copy result read");
            Check(new ReadOnlySpan<byte>(bytes, 9).SequenceEqual(new byte[] {0,0,51,52,53,54,49,50,51}), "exact copied bytes and holes");
            Equal(copy_file_range(from, null, to, null, 1, 1), -1L, "invalid copy flags"); Equal(errno, EINVAL, "copy flags errno");
            offsetFrom = -1; offsetTo = 0;
            Equal(copy_file_range(from, &offsetFrom, to, &offsetTo, 1, 0), -1L, "negative explicit copy offset"); Equal(errno, EOVERFLOW, "native Linux negative copy errno");
            Equal(offsetFrom, -1L, "failed copy preserves offset");
            Equal(copy_file_range(-1, null, to, null, 1, 0), -1L, "unknown copy descriptor"); Equal(errno, EBADF, "copy bad fd errno");
        }
        finally { close(from); close(to); }
    }

    private static void DirectoryDescriptors()
    {
        string path = Path.Combine(root, "native-directory"); Directory.CreateDirectory(path);
        File.SetUnixFileMode(path, (UnixFileMode)0x1e8); // 0750
        int fd = Open(path, 0x10000 | 0x80000), raw;
        fixed (byte* p = Encoding.UTF8.GetBytes(path + "\0")) raw = NativeOpen(p, 0x10000 | 0x80000, 0);
        Check(fd >= 3 && raw >= 3 && fd != raw, "separate guest/native directory descriptors");
        int ownedNative = -1;
        foreach (string entry in Directory.EnumerateFileSystemEntries("/proc/self/fd"))
        {
            string? target = KernelLinkTarget(entry);
            if (target == path && int.Parse(Path.GetFileName(entry)) != raw) ownedNative = int.Parse(Path.GetFileName(entry));
        }
        Check(ownedNative >= 3, "owned kernel directory descriptor");
        byte* metadata = stackalloc byte[112], control = stackalloc byte[144];
        new Span<byte>(metadata, 112).Fill(0xa7);
        try
        {
            Equal(NativeDirectoryStat(raw, control), 0, "native directory metadata");
            Equal(fstat(fd, metadata + 8), 0, "guest directory metadata");
            Equal(*(ulong*)(metadata + 8), *(ulong*)control, "directory device identity");
            Equal(*(ulong*)(metadata + 16), *(ulong*)(control + 8), "directory inode identity");
            Equal(*(uint*)(metadata + 24), *(uint*)(control + 24), "directory mode identity");
            Equal(*(long*)(metadata + 48), *(long*)(control + 48), "directory native size");
            Equal(*(ulong*)metadata, 0xa7a7a7a7a7a7a7a7UL, "directory leading canary");
            Equal(*(ulong*)(metadata + 104), 0xa7a7a7a7a7a7a7a7UL, "directory trailing canary");
            Equal(fcntl(fd, 1), 1, "directory cloexec");
            Equal(fsync(fd), NativeDirectorySync(raw), "real native directory fsync");
            byte value = 0;
            Equal(NativeDirectoryRead(raw, &value, 1), -1L, "native directory read fails"); int error = Marshal.GetLastPInvokeError();
            Equal(read(fd, &value, 1), -1L, "guest directory read fails"); Equal(errno, error, "directory read native errno");
            Equal(NativeDirectoryWrite(raw, &value, 1), -1L, "native directory write fails"); error = Marshal.GetLastPInvokeError();
            Equal(write(fd, &value, 1), -1L, "guest directory write fails"); Equal(errno, error, "directory write native errno");
            Equal(lseek(fd, 0, 0), NativeDirectorySeek(raw, 0, 0), "native directory seek");
            using (var foreign = new RuntimeContext()) using (foreign.Enter())
            { Equal(fsync(fd), -1, "foreign directory fd rejected"); Equal(errno, EBADF, "foreign directory errno"); }
            int duplicate = dup(fd); Check(duplicate >= 3 && duplicate != fd, "directory duplicate");
            Equal(fcntl(duplicate, 1), 0, "directory dup clears cloexec");
            Equal(fcntl(duplicate, 4, 0x800), 0, "directory status flags"); Equal(fcntl(fd, 3) & 0x800, 0x800, "directory dup shares status");
            Equal(close(fd), 0, "close original directory"); fd = -1;
            Check(NativeFcntl(ownedNative, 1) >= 0, "duplicate retains original kernel handle");
            Equal(fsync(duplicate), 0, "sync retained duplicate");
            string renamed = path + "-renamed"; Directory.Move(path, renamed);
            Equal(fstat(duplicate, metadata), 0, "metadata after rename"); Equal(*(ulong*)(metadata + 8), *(ulong*)(control + 8), "rename retains inode");
            Directory.Delete(renamed);
            Equal(fsync(duplicate), 0, "sync unlinked but open directory");
            Equal(fstat(duplicate, metadata), 0, "metadata after unlink"); Equal(*(ulong*)(metadata + 8), *(ulong*)(control + 8), "unlink retains inode");
            Equal(close(duplicate), 0, "last directory duplicate close");
            Equal(NativeFcntl(ownedNative, 1), -1, "last close releases OS handle"); Equal(Marshal.GetLastPInvokeError(), EBADF, "closed kernel handle error");
            Equal(fsync(duplicate), -1, "closed guest directory rejected"); Equal(errno, EBADF, "closed guest errno");
        }
        finally { if (fd >= 0) close(fd); NativeClose(raw); }

        string survivor = Path.Combine(root, "surviving-directory"); Directory.CreateDirectory(survivor);
        fd = Open(survivor, 0);
        try
        {
            int retiredNative = -1;
            var retired = new RuntimeContext();
            using (retired.Enter())
            {
                int abandoned = Open(root, 0); Check(abandoned >= 3, "retired directory");
                Check(dup(abandoned) >= 3, "retired directory duplicate");
                foreach (string entry in Directory.EnumerateFileSystemEntries("/proc/self/fd"))
                    if (KernelLinkTarget(entry) == root) retiredNative = int.Parse(Path.GetFileName(entry));
            }
            retired.Dispose();
            Equal(NativeFcntl(retiredNative, 1), -1, "retired owner releases duplicated kernel directory");
            Equal(fsync(fd), 0, "foreign ambient owner backing preserved");
            int target = Open(Path.Combine(root, "directory-dup-target"), 2 | 0x40 | 0x80);
            Equal(dup2(fd, target), target, "directory dup2 replaces regular file");
            Equal(fstat(target, metadata), 0, "directory dup2 metadata"); Equal(*(uint*)(metadata + 16) & 0xf000u, 0x4000u, "dup2 actual directory kind");
            Equal(close(target), 0, "dup2 target closes independently"); Equal(fsync(fd), 0, "dup2 close retains source");
            using var standard = new RuntimeContext();
            using (standard.Enter())
            {
                int directory = Open(survivor, 0); Equal(dup2(directory, 0), 0, "directory dup2 stdin");
                Equal(fstat(0, metadata), 0, "directory stdin real metadata"); Equal(*(uint*)(metadata + 16) & 0xf000u, 0x4000u, "standard fd directory kind");
                Equal(fsync(0), 0, "directory stdin real sync");
                Equal(close(0), 0, "directory stdin close"); Equal(fstat(0, metadata), -1, "closed standard directory metadata rejected"); Equal(errno, EBADF, "closed standard errno");
                FILE* stream; fixed (byte* mode = "r\0"u8) stream = fdopen(directory, mode);
                Check(stream != null, "directory fdopen read");
                Equal(fgetc(stream), -1, "directory stdio read rejects"); Equal(errno, EISDIR, "directory stdio errno"); Check(ferror(stream) != 0, "directory stdio error indicator");
                Equal(fclose(stream), 0, "directory stdio close");
            }
        }
        finally { close(fd); }
        Equal(Open(survivor, 1), -1, "writable directory open"); Equal(errno, EISDIR, "writable directory errno");
        Equal(Open(survivor, 2), -1, "read/write directory open"); Equal(errno, EISDIR, "read/write directory errno");
        Equal(Open(Path.Combine(root, "created"), 0x10000), -1, "O_DIRECTORY rejects regular file"); Equal(errno, ENOTDIR, "non-directory errno");
        string link = Path.Combine(root, "native-directory-link"); Directory.CreateSymbolicLink(link, survivor);
        Equal(Open(link, 0x10000 | 0x20000), -1, "directory nofollow symlink"); Equal(errno, ENOTDIR, "directory nofollow native error");
    }

    private static void NumericText()
    {
        nint nativeLocale;
        fixed (byte* name = "C\0"u8) nativeLocale = NativeNewlocale(2, name, 0);
        Check(nativeLocale != 0, "native C numeric locale");
        try
        {
            string[] inputs = ["0", "-0", "  12.5tail", "0x1.23456p-12!", "nan(payload)", "-Infinity", "1e1000", "-1e1000", "1e-1000", "0x1p-149", "0x1p-150", "1.00000005960464477539062500001", "", "bad", "1,5"];
            foreach (string input in inputs)
            {
                fixed (byte* text = Encoding.UTF8.GetBytes(input + "\0"))
                {
                    byte* expectedEnd; float expected = NativeStrtof(text, &expectedEnd, nativeLocale); int expectedError = Marshal.GetLastPInvokeError();
                    errno = 904; byte* actualEnd; float actual = strtof(text, &actualEnd);
                    if (float.IsNaN(expected)) Check(float.IsNaN(actual), "strtof NaN");
                    else Equal(BitConverter.SingleToInt32Bits(actual), BitConverter.SingleToInt32Bits(expected), "strtof exact binary32");
                    Equal((long)(actualEnd - text), (long)(expectedEnd - text), "strtof end pointer");
                    Equal(errno, expectedError == 0 ? 904 : expectedError, "strtof range errno");
                }
            }
        }
        finally { NativeFreelocale(nativeLocale); }
        Equal(strtof(null, null), 0f, "null strtof"); Equal(errno, EFAULT, "null parse errno");
        foreach (double input in new[] { -3.5, -2, -0.5, -0.0, 0, 0.5, 1, 5, 171, 172, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
        {
            int sign;
            double expected = NativeLgamma(input, &sign); int error = Marshal.GetLastPInvokeError();
            errno = 905; double actual = lgamma(input);
            Equal(BitConverter.DoubleToInt64Bits(actual), BitConverter.DoubleToInt64Bits(expected), "lgamma native bits"); Equal(errno, error == 0 ? 905 : error, "lgamma errno");
            expected = NativeTgamma(input); error = Marshal.GetLastPInvokeError();
            errno = 906; actual = tgamma(input);
            Equal(BitConverter.DoubleToInt64Bits(actual), BitConverter.DoubleToInt64Bits(expected), "tgamma native bits"); Equal(errno, error == 0 ? 906 : error, "tgamma errno");
        }
    }

    private static void WideText()
    {
        int[] scalars = [-1, 0, 9, 32, 48, 65, 97, 127, 128, 0xe9, 0x3a3, 0x2003, 0xd800, 0x1f600, 0x10400, 0x10ffff, 0x110000];
        string[] names = ["alnum", "alpha", "blank", "cntrl", "digit", "graph", "lower", "print", "punct", "space", "upper", "xdigit"];
        delegate*<int, int>[] functions = [&iswalnum, &iswalpha, &iswblank, &iswcntrl, &iswdigit, &iswgraph, &iswlower, &iswprint, &iswpunct, &iswspace, &iswupper, &iswxdigit];
        foreach (string localeName in new[] {"C", "C.UTF-8"})
        {
            fixed (byte* name = Encoding.UTF8.GetBytes(localeName + "\0"))
            {
                Check(setlocale(0, name) != null, "owner locale selection");
                nint native = NativeNewlocale(1, name, 0); Check(native != 0, "native scalar locale");
                try
                {
                    for (int i = 0; i < names.Length; i++)
                    {
                        ulong kind; fixed (byte* kindName = Encoding.ASCII.GetBytes(names[i] + "\0")) kind = NativeWctype(kindName, native);
                        foreach (int scalar in scalars)
                            Equal(functions[i](scalar) != 0, NativeIswctype((uint)scalar, kind, native) != 0, "wide class native " + localeName + " " + names[i]);
                    }
                }
                finally { NativeFreelocale(native); }
            }
        }
        Equal(towlower(0x3a3), 0x3c3, "owner unicode lower"); Equal(towupper(0x10428), 0x10400, "owner supplementary upper");
        using (var other = new RuntimeContext()) using (other.Enter())
        { Equal(iswalpha(0xe9), 0, "new owner starts in C locale"); }
        Check(iswalpha(0xe9) != 0, "outer owner Unicode locale retained");
        foreach (int value in new[] {-1, 0, 127, 128, 255, int.MaxValue}) Equal(isascii(value), (uint)value < 128 ? 1 : 0, "isascii domain");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Interface
    {
        public Interface* Next; public byte* Name; public uint Flags;
        public byte* Address; public byte* Mask; public byte* Broad; public void* Data;
    }

    private static HashSet<string> InterfaceRows(Interface* list)
    {
        var result = new HashSet<string>();
        for (Interface* p = list; p != null; p = p->Next)
        {
            string name = Marshal.PtrToStringUTF8((nint)p->Name)!;
            int family = p->Address == null ? -1 : *(ushort*)p->Address;
            int length = family == 2 ? 16 : family == 10 ? 28 : family == 17 ? 20 : 0;
            string address = length == 0 ? "" : Convert.ToHexString(new ReadOnlySpan<byte>(p->Address, length));
            result.Add(name + ":" + p->Flags + ":" + family + ":" + address);
        }
        return result;
    }

    private static void InterfacesAndDiagnostics()
    {
        Interface* native; Equal(NativeGetifaddrs(&native), 0, "native interface enumeration");
        try
        {
            Interface* guest; Equal(getifaddrs(&guest), 0, "owner interface enumeration");
            Check(InterfaceRows(guest).SetEquals(InterfaceRows(native)), "native interface address/name/flag records");
            Equal(sizeof(Interface), 56, "Linux interface size");
            using (var foreign = new RuntimeContext()) using (foreign.Enter())
            { freeifaddrs(guest); Equal(errno, EINVAL, "foreign list release rejected"); }
            Check(InterfaceRows(guest).Count > 0, "foreign release leaves list live");
            freeifaddrs(guest); freeifaddrs(guest); Equal(errno, EINVAL, "double interface free rejected");
        }
        finally { NativeFreeifaddrs(native); }
        using (var retired = new RuntimeContext())
        {
            using (retired.Enter()) { void* list; Equal(getifaddrs(&list), 0, "abandoned list"); Check(retired.BackendInterfaceLists.Count > 0, "list registered to owner"); }
            retired.Dispose(); Equal(retired.BackendInterfaceLists.Count, 0, "owner disposal drains lists");
        }
        Equal(getifaddrs(null), -1, "null interface result"); Equal(errno, EFAULT, "interface null errno");
        void** frames = stackalloc void*[32]; int count = backtrace(frames, 32);
        Check(count > 0 && count <= 32, "actual native stack frames");
        var owner = RuntimeContext.Current!; int before = owner.NativeAllocations.Count;
        byte** names = backtrace_symbols(frames, count); Check(names != null, "actual native symbols");
        for (int i = 0; i < count; i++) Check(names[i] != null && names[i][0] != 0, "symbol string");
        Check(owner.NativeAllocations.Count > before, "symbol buffer is guest owned"); free(names);
        Equal(owner.NativeAllocations.Count, before, "symbol guest free pairs");
        string trace = Path.Combine(root, "native-trace"); int fd = Open(trace, 2 | 0x40 | 0x80);
        try { backtrace_symbols_fd(frames, count, fd); Equal(fdatasync(fd), 0, "symbol file sync"); }
        finally { close(fd); }
        Check(File.ReadAllText(trace).Contains('\n'), "symbol output uses guest descriptor");
        backtrace_symbols_fd(frames, count, -1); Equal(errno, EBADF, "symbol output bad descriptor");
        Equal(owner.NativeAllocations.Count, before, "symbol output frees temporary block");
        closelog(); Equal(errno, ENOTSUP, "syslog unsupported contract retained");
    }

    public static int Main()
    {
        if (!OperatingSystem.IsLinux() || RuntimeInformation.ProcessArchitecture != Architecture.X64)
        { Console.WriteLine("SKIP Linux x64 backend host services"); return 77; }
        root = Path.Combine(Path.GetTempPath(), "dotcc-backendhost-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            using var owner = new RuntimeContext(); using var binding = owner.Enter();
            Masks(); Files(); Symlinks(); Directories(); Storage(); Limits(); Copies(); DirectoryDescriptors(); NumericText(); WideText(); InterfacesAndDiagnostics();
            Console.WriteLine($"PASS backend host {assertions} assertions");
            return 0;
        }
        finally { Directory.Delete(root, true); }
    }

    [DllImport("libc", EntryPoint = "getrlimit", SetLastError = true)]
    private static extern int NativeGetrlimit(int resource, void* result);
    [DllImport("libc", EntryPoint = "newlocale")]
    private static extern nint NativeNewlocale(int mask, byte* name, nint basis);
    [DllImport("libc", EntryPoint = "freelocale")]
    private static extern void NativeFreelocale(nint locale);
    [DllImport("libc", EntryPoint = "strtof_l", SetLastError = true)]
    private static extern float NativeStrtof(byte* text, byte** end, nint locale);
    [DllImport("libm.so.6", EntryPoint = "lgamma_r", SetLastError = true)]
    private static extern double NativeLgamma(double value, int* sign);
    [DllImport("libm.so.6", EntryPoint = "tgamma", SetLastError = true)]
    private static extern double NativeTgamma(double value);
    [DllImport("libc", EntryPoint = "wctype_l")]
    private static extern ulong NativeWctype(byte* name, nint locale);
    [DllImport("libc", EntryPoint = "iswctype_l")]
    private static extern int NativeIswctype(uint value, ulong kind, nint locale);
    [DllImport("libc", EntryPoint = "getifaddrs")]
    private static extern int NativeGetifaddrs(Interface** result);
    [DllImport("libc", EntryPoint = "freeifaddrs")]
    private static extern void NativeFreeifaddrs(Interface* list);
    [DllImport("libc", EntryPoint = "open", SetLastError = true)]
    private static extern int NativeOpen(byte* path, int flags, int mode);
    [DllImport("libc", EntryPoint = "close", SetLastError = true)]
    private static extern int NativeClose(int descriptor);
    [DllImport("libc", EntryPoint = "fcntl", SetLastError = true)]
    private static extern int NativeFcntl(int descriptor, int command);
    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int NativeDirectoryStat(int descriptor, void* metadata);
    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int NativeDirectorySync(int descriptor);
    [DllImport("libc", EntryPoint = "read", SetLastError = true)]
    private static extern long NativeDirectoryRead(int descriptor, void* buffer, ulong count);
    [DllImport("libc", EntryPoint = "write", SetLastError = true)]
    private static extern long NativeDirectoryWrite(int descriptor, void* buffer, ulong count);
    [DllImport("libc", EntryPoint = "lseek", SetLastError = true)]
    private static extern long NativeDirectorySeek(int descriptor, long offset, int whence);
    [DllImport("libc", EntryPoint = "readlink", SetLastError = true)]
    private static extern long NativeReadlink(byte* path, byte* result, ulong capacity);
}
