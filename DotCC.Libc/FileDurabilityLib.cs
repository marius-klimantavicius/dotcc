#nullable enable
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public static long ftello(FILE* stream) => ftell(stream);

    /// <summary>Flush the owner's file buffers through the operating system to storage.</summary>
    public static int fsync(int descriptor)
    {
        var slot = SlotByFd(descriptor);
        if (slot is null) { errno = EBADF; return -1; }
        if (slot.Stream is not FileStream file) { errno = EINVAL; return -1; }
        try { slot.Writer?.Flush(); file.Flush(flushToDisk: true); return 0; }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (UnauthorizedAccessException) { errno = EACCES; return -1; }
        catch (IOException) { errno = EIO; return -1; }
    }

    public static int truncate(byte* path, long length)
    {
        if (path == null) { errno = EFAULT; return -1; }
        if (length < 0) { errno = EINVAL; return -1; }
        int descriptor = open(path, 1);
        if (descriptor < 0) return -1;
        int result = ftruncate(descriptor, length);
        int error = errno;
        close(descriptor);
        if (result != 0) errno = error;
        return result;
    }

    public static int fchmod(int descriptor, uint mode)
    {
        var slot = SlotByFd(descriptor);
        if (slot is null) { errno = EBADF; return -1; }
        if (OperatingSystem.IsWindows()) { errno = ENOTSUP; return -1; }
        if (slot.Stream is not FileStream file) { errno = EINVAL; return -1; }
        try { File.SetUnixFileMode(file.SafeFileHandle, (UnixFileMode)(mode & 0xfff)); return 0; }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
        catch (UnauthorizedAccessException) { errno = EPERM; return -1; }
        catch (IOException) { errno = EIO; return -1; }
    }

    /// <summary>Atomically create a private file, replacing the template's final six X bytes.</summary>
    public static int mkstemp(byte* template)
    {
        if (template == null) { errno = EFAULT; return -1; }
        int length = strlen(template);
        if (length < 6) { errno = EINVAL; return -1; }
        for (int i = length - 6; i < length; i++)
            if (template[i] != (byte)'X') { errno = EINVAL; return -1; }
        string prefix = Encoding.UTF8.GetString(template, length - 6);
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Span<char> suffix = stackalloc char[6];
        for (int attempt = 0; attempt < 128; attempt++)
        {
            for (int i = 0; i < suffix.Length; i++) suffix[i] = alphabet[global::System.Security.Cryptography.RandomNumberGenerator.GetInt32(alphabet.Length)];
            string path = ResolvePath(prefix + new string(suffix));
            FileStream? file = null;
            try
            {
                var options = new FileStreamOptions { Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite,
                    Share = FileShare.ReadWrite | FileShare.Delete };
                if (!OperatingSystem.IsWindows()) options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
                file = new FileStream(path, options);
                int descriptor = RegisterFileSlot(file);
                file = null; // Ownership transferred to the descriptor table.
                for (int i = 0; i < suffix.Length; i++) template[length - 6 + i] = (byte)suffix[i];
                return descriptor;
            }
            catch (IOException) when (File.Exists(path) || Directory.Exists(path)) { }
            catch (DirectoryNotFoundException) { errno = ENOENT; return -1; }
            catch (UnauthorizedAccessException) { errno = EACCES; return -1; }
            catch (ArgumentException) { errno = EINVAL; return -1; }
            catch (IOException) { errno = EIO; return -1; }
            finally { file?.Dispose(); }
        }
        errno = EEXIST;
        return -1;
    }
}
