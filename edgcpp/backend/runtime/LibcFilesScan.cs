using System;

namespace Edg.Runtime;

// Keep the private FILE monitor across the complete scan. C# lock lowers
// to Monitor.Enter with finally-based release, including foreign CLR exceptions.
public sealed unsafe partial class LibcFiles
{
    public int WithFileReadLock(void* file,
        delegate* managed<void*, sbyte*, VaList*, int> scanner,
        void* context, sbyte* format, VaList* arguments)
    {
        if(scanner==null) { setErrno(Einval);return Eof; }
        var entry=Find(file);if(entry is null)return Eof;
        lock(entry.Gate)
        {
            if(!Orient(entry,-1) || !PrepareRead(entry))return Eof;
            return scanner(context,format,arguments);
        }
    }
    public int WithFileReadLock<TOwner>(void* file,
        delegate* managed<TOwner, void*, sbyte*, VaList*, int> scanner,
        TOwner owner, void* context, sbyte* format, VaList* arguments) where TOwner:class
    {
        if(scanner==null) { setErrno(Einval);return Eof; }
        var entry=Find(file);if(entry is null)return Eof;
        lock(entry.Gate)
        {
            if(!Orient(entry,-1) || !PrepareRead(entry))return Eof;
            return scanner(owner,context,format,arguments);
        }
    }
}
