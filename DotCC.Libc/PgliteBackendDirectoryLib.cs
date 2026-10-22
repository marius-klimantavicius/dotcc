#nullable enable
using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // A directory is an owned kernel descriptor, not a FileStream. Reusing the
    // descriptor table's shared Stream lifetime means dup/dup2 retain exactly
    // the same open-file description and the final close disposes its handle.
    private sealed class BackendDirectoryStream(SafeFileHandle handle) : Stream
    {
        internal SafeFileHandle Handle { get; } = handle;
        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override int Read(byte[] buffer, int offset, int count) => throw new IOException("Cannot read directory bytes.");
        public override int ReadByte() { errno = EISDIR; return -1; }
        public override void Write(byte[] buffer, int offset, int count) => throw new IOException("Cannot write directory bytes.");
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long length) => throw new NotSupportedException();
        public override void Flush()
        {
            if (BackendNativeDirectorySync(Handle) != 0)
                throw new IOException("Directory synchronization failed: " + Marshal.GetLastPInvokeError());
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) Handle.Dispose();
            base.Dispose(disposing);
        }
    }

    private static Stream? AdoptBackendLinuxStream(int descriptor, int flags)
    {
        var handle = new SafeFileHandle((IntPtr)descriptor, ownsHandle: true);
        try
        {
            LinuxX64Stat metadata;
            if (HostFStat(descriptor, &metadata) != 0)
            {
                int error = Marshal.GetLastPInvokeError();
                handle.Dispose(); errno = error; return null;
            }
            if ((metadata.Mode & 0xf000) == S_IFDIR) return new BackendDirectoryStream(handle);
            var access = (flags & 3) == 2 ? FileAccess.ReadWrite : (flags & 3) == 1 ? FileAccess.Write : FileAccess.Read;
            return new FileStream(handle, access, bufferSize: 1);
        }
        catch (Exception error) when (error is IOException or ArgumentException or UnauthorizedAccessException or OutOfMemoryException)
        {
            handle.Dispose();
            errno = error is UnauthorizedAccessException ? EACCES : error is ArgumentException ? EINVAL : error is OutOfMemoryException ? ENOMEM : EIO;
            return null;
        }
    }

    private static int BackendDirectoryFstat(BackendDirectoryStream directory, void* result)
    {
        try
        {
            LinuxX64Stat metadata;
            if (BackendNativeDirectoryStat(directory.Handle, &metadata) != 0)
            { errno = Marshal.GetLastPInvokeError(); return -1; }
            FillHostStat(result, in metadata);
            return 0;
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
    }

    private static int BackendDirectoryFsync(BackendDirectoryStream directory)
    {
        try
        {
            if (BackendNativeDirectorySync(directory.Handle) == 0) return 0;
            errno = Marshal.GetLastPInvokeError(); return -1;
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
    }

    private static long BackendDirectorySeek(BackendDirectoryStream directory, long offset, int whence)
    {
        try
        {
            long result = BackendNativeDirectorySeek(directory.Handle, offset, whence);
            if (result < 0) errno = Marshal.GetLastPInvokeError();
            return result;
        }
        catch (ObjectDisposedException) { errno = EBADF; return -1; }
    }

    [DllImport("libc", EntryPoint = "fstat", SetLastError = true)]
    private static extern int BackendNativeDirectoryStat(SafeFileHandle descriptor, LinuxX64Stat* metadata);
    [DllImport("libc", EntryPoint = "fsync", SetLastError = true)]
    private static extern int BackendNativeDirectorySync(SafeFileHandle descriptor);
    [DllImport("libc", EntryPoint = "lseek", SetLastError = true)]
    private static extern long BackendNativeDirectorySeek(SafeFileHandle descriptor, long offset, int whence);
}
