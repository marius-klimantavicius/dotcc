using System;

namespace Edg.Runtime;

// Keep the private FILE monitor across every callback chunk. C# lock lowers
// to Monitor.Enter with finally-based release, including foreign CLR exceptions.
public sealed unsafe partial class LibcFiles
{
    public int WithFileLock(void* file,
        delegate* managed<void*, sbyte*, VaList*, int> formatter,
        void* context, sbyte* format, VaList* arguments)
    {
        if(formatter==null) { setErrno(Einval);return Eof; }
        var entry=Find(file);if(entry is null)return Eof;
        lock(entry.Gate)
        {
            if(!Orient(entry,-1) || !PrepareWrite(entry))return Eof;
            return formatter(context,format,arguments);
        }
    }
    public int WithFileLock<TOwner>(void* file,
        delegate* managed<TOwner, void*, sbyte*, VaList*, int> formatter,
        TOwner owner, void* context, sbyte* format, VaList* arguments) where TOwner:class
    {
        if(formatter==null) { setErrno(Einval);return Eof; }
        var entry=Find(file);if(entry is null)return Eof;
        lock(entry.Gate)
        {
            if(!Orient(entry,-1) || !PrepareWrite(entry))return Eof;
            return formatter(owner,context,format,arguments);
        }
    }
}
