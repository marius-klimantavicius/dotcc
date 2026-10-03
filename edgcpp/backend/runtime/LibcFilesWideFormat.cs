using System;

namespace Edg.Runtime;

public sealed unsafe partial class LibcFiles
{
    // The selected musl core validates positional arguments before entering
    // this session. Preserve prior FILE errors while observing new failures.
    public int WithFileWideLock(void* file,
        delegate* managed<void*, int*, VaList*, int> formatter,
        void* context, int* format, VaList* arguments)
    {
        if(formatter==null) {setErrno(Einval);return Eof;}
        var entry=Find(file);if(entry is null)return Eof;
        lock(entry.Gate)
        {
            if(!Orient(entry,1))return Eof;
            bool previousError=entry.Error;entry.Error=false;
            try
            {
                if(!PrepareWrite(entry))return Eof;
                int result=formatter(context,format,arguments);
                return entry.Error?Eof:result;
            }
            finally {entry.Error|=previousError;}
        }
    }
    public int WithFileWideLock<TOwner>(void* file,
        delegate* managed<TOwner, void*, int*, VaList*, int> formatter,
        TOwner owner, void* context, int* format, VaList* arguments) where TOwner:class
    {
        if(formatter==null) {setErrno(Einval);return Eof;}
        var entry=Find(file);if(entry is null)return Eof;
        lock(entry.Gate)
        {
            if(!Orient(entry,1))return Eof;
            bool previousError=entry.Error;entry.Error=false;
            try
            {
                if(!PrepareWrite(entry))return Eof;
                int result=formatter(owner,context,format,arguments);
                return entry.Error?Eof:result;
            }
            finally {entry.Error|=previousError;}
        }
    }
}
