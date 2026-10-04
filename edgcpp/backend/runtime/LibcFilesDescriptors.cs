#nullable enable
using System;
using System.IO;

namespace Edg.Runtime;

public sealed unsafe partial class LibcFiles
{
    public void* Fdopen(int descriptor, sbyte* mode)
    {
        if (fileDescriptors is null) { setErrno(95); return null; }
        Stream? stream = null;
        try
        {
            string spelling = Decode(mode);
            stream = fileDescriptors.Fdopen(descriptor, spelling);
            bool update = spelling.Contains('+');
            var entry = new Entry(stream, spelling[0] == 'r' || update,
                spelling[0] != 'r' || update, spelling[0] == 'a', true);
            void* result = Add(entry); stream = null; return result;
        }
        catch (Exception error) when (PlatformFailure(error)) { setErrno(ErrorNumber(error)); return null; }
        // Association failure must not close the caller's descriptor.
    }

    public int Fileno(void* file)
    {
        var entry = Find(file); if (entry is null) return -1;
        lock (entry.Gate)
        {
            if (entry.Closed) return Fail(null, Ebadf);
            if (fileDescriptors is null) { setErrno(95); return -1; }
            try { return fileDescriptors.Fileno(entry.Stream); }
            catch (Exception error) when (PlatformFailure(error)) { setErrno(ErrorNumber(error)); return -1; }
        }
    }
}
