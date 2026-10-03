#nullable enable
using System.Threading;
namespace Edg.Runtime;
public sealed unsafe partial class LibcLocale
{
    // Source wrappers resolve and validate every requested environment category
    // before entering either method. No host environment is consulted here.
    public sbyte* SetCategories(int category, int values)
    {
        lock(gate) {
            Alive();if((uint)category>6 || (uint)values>63){setErrno(22);return null;}
            int mask=category==6?63:1<<category;
            Volatile.Write(ref categories,(categories&~mask)|(values&mask));
            return NamePointer(category==6?categories:(categories&(1<<category))==0?0:63);
        }
    }
    public void* NewCategories(int mask, int values, void* basis)
    {
        lock(gate) {
            Alive();if((mask&~63)!=0 || (uint)values>63 || basis==(void*)(nint)(-1)) {setErrno(22);return null;}
            Locale? old=basis==null?null:Find(basis);if(basis!=null && old==null)return null;
            if(old?.Users>0){setErrno(16);return null;}
            int result=((old?.Categories??0)&~mask)|(values&mask);
            if(old!=null){old.Categories=result;return basis;}
            return Allocate(result);
        }
    }
}
