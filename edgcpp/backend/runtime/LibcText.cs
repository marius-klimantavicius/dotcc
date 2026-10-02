#nullable enable
using System;
using System.Text;
using System.Threading;

namespace Edg.Runtime;

// A bounded locale provider: C/POSIX and C.UTF-8 for LC_CTYPE or LC_ALL.
// Numeric/messages/collation remain C. This is not arbitrary locale support.
public sealed unsafe class LibcText : IDisposable
{
    private readonly Action<int> setErrno;
    private byte[]? storage;
    private readonly int[] messageOffsets;
    private readonly int cNameOffset, utf8NameOffset;
    private int utf8;
    public LibcText(Action<int> setErrno)
    {
        this.setErrno = setErrno;
        int bytes = 2 + 8, highest = 0;
        foreach(var entry in LibcErrorMessages.Entries) { bytes += Encoding.UTF8.GetByteCount(entry.Text) + 1; highest = Math.Max(highest,entry.Code); }
        storage = GC.AllocateArray<byte>(bytes,pinned:true);
        messageOffsets = new int[highest+1];
        int next=0;
        foreach(var entry in LibcErrorMessages.Entries) {
            messageOffsets[entry.Code]=next;
            next += Encoding.UTF8.GetBytes(entry.Text,storage.AsSpan(next));
            storage[next++]=0;
        }
        cNameOffset=next;storage[next++]=(byte)'C';storage[next++]=0;
        utf8NameOffset=next;"C.UTF-8\0"u8.CopyTo(storage.AsSpan(next));
    }
    private byte[] Storage => storage ?? throw new ObjectDisposedException(nameof(LibcText));
    private sbyte* Pointer(int offset) { fixed(byte* pointer=Storage)return (sbyte*)(pointer+offset); }
    public sbyte* Strerror(int error)
    {
        int offset=(uint)error<(uint)messageOffsets.Length?messageOffsets[error]:0;
        return Pointer(offset);
    }
    public static int IsDigit(int value) => unchecked((uint)(value-'0')) <= 9 ? 1 : 0;
    private static bool Spelling(sbyte* value, ReadOnlySpan<byte> expected)
    {
        for(int i=0;i<expected.Length;++i)if((byte)value[i]!=expected[i])return false;
        return value[expected.Length]==0;
    }
    public sbyte* SetLocale(int category, sbyte* locale)
    {
        _=Storage;
        if(category!=0 && category!=6) { setErrno(22);return null; }
        if(locale!=null) {
            int selected;
            if(Spelling(locale,"C"u8) || Spelling(locale,"POSIX"u8))selected=0;
            else if(Spelling(locale,"C.UTF-8"u8) || Spelling(locale,"C.utf8"u8))selected=1;
            else { setErrno(22);return null; }
            Volatile.Write(ref utf8,selected);
        }
        return Pointer(Volatile.Read(ref utf8)==0?cNameOffset:utf8NameOffset);
    }
    public ulong MbCurMax() { _=Storage;return Volatile.Read(ref utf8)==0?1UL:4UL; }
    public int Wctomb(sbyte* destination,int character)
    {
        _=Storage;
        // Both supplied encodings are stateless. A null destination resets
        // conversion state and ignores the character, as required by wctomb.
        if(destination==null)return 0;
        if(unchecked((uint)character)<128) { *destination=(sbyte)character;return 1; }
        if(Volatile.Read(ref utf8)==0) {
            // musl's C locale preserves arbitrary bytes with U+DF80..U+DFFF.
            if(unchecked((uint)(character-0xdf80))<128) { *destination=unchecked((sbyte)character);return 1; }
        } else if(Rune.TryCreate(character,out var rune)) {
            return rune.EncodeToUtf8(new Span<byte>(destination,4));
        }
        setErrno(84);return -1;
    }
    public void Dispose() => Interlocked.Exchange(ref storage,null);
}
