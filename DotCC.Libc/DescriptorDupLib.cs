#nullable enable
using System;
using System.IO;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    private sealed class SharedFileDescription(Stream stream, int flags)
    {
        internal readonly Stream Stream = stream;
        internal int References = 1;
        internal int StatusFlags = flags;
    }

    private static void ReleaseFileBacking(FileSlot slot)
    {
        Stream? release;
        if (slot.Description is { } description)
        {
            lock (description)
            {
                release = --description.References == 0 ? description.Stream : null;
                slot.Description = null;
            }
        }
        else release = slot.Stream;
        slot.Stream = null;
        release?.Dispose();
    }

    private static int DuplicateDescriptor(int oldfd, int? requested)
    {
        lock (_filesLock)
        {
            var source = SlotByFd(oldfd);
            if (source is null || source.Kind is FileSlot.K.File or FileSlot.K.Directory && source.Stream == null)
            { errno = EBADF; return -1; }
            if (requested is < 0 or >= 1048576) { errno = EBADF; return -1; }
            if (requested == oldfd) return oldfd;
            // Pipe/socket descriptions require their own shared endpoint
            // lifetime accounting. Reject them explicitly until implemented.
            if (source.Kind is FileSlot.K.Pipe or FileSlot.K.Socket)
            { errno = ENOTSUP; return -1; }
            int descriptor = requested ?? _files.FindIndex(3, slot => slot == null);
            if (descriptor < 0) descriptor = _files.Count;
            int savedError = errno;
            if (descriptor < _files.Count && _files[descriptor] is { } target)
            {
                try { CloseSlot(target); }
                catch (IOException) { ReleaseFileBacking(target); }
            }
            if (source.Stream is { } stream)
            {
                source.Description ??= new SharedFileDescription(stream, source.StatusFlags);
                lock (source.Description) source.Description.References++;
            }
            var duplicate = new FileSlot
            {
                Kind = source.Kind, Stream = source.Stream, Description = source.Description,
                StatusFlags = source.StatusFlags, DescriptorFlags = 0,
                StdioAccess = source.StdioAccess
            };
            while (_files.Count <= descriptor) _files.Add(null);
            _files[descriptor] = duplicate;
            errno = savedError;
            return descriptor;
        }
    }
}
