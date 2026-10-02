#nullable enable
using System;
using System.Text;
using System.Threading;

namespace Edg.Runtime;

// A bounded locale provider: C/POSIX and C.UTF-8 with owner/thread selection.
// Numeric/messages/collation remain C. This is not arbitrary locale support.
public sealed unsafe class LibcText : IDisposable
{
    private readonly Action<int> setErrno;
    private byte[]? storage;
    private readonly int[] messageOffsets;
    public LibcLocale Locales { get; }
    public LibcText(Action<int> setErrno)
    {
        this.setErrno = setErrno;
        Locales = new LibcLocale(setErrno);
        int bytes = 0, highest = 0;
        foreach(var entry in LibcErrorMessages.Entries) { bytes += Encoding.UTF8.GetByteCount(entry.Text) + 1; highest = Math.Max(highest,entry.Code); }
        storage = GC.AllocateArray<byte>(bytes,pinned:true);
        messageOffsets = new int[highest+1];
        int next=0;
        foreach(var entry in LibcErrorMessages.Entries) {
            messageOffsets[entry.Code]=next;
            next += Encoding.UTF8.GetBytes(entry.Text,storage.AsSpan(next));
            storage[next++]=0;
        }
    }
    private byte[] Storage => storage ?? throw new ObjectDisposedException(nameof(LibcText));
    private sbyte* Pointer(int offset) { fixed(byte* pointer=Storage)return (sbyte*)(pointer+offset); }
    public sbyte* Strerror(int error)
    {
        int offset=(uint)error<(uint)messageOffsets.Length?messageOffsets[error]:0;
        return Pointer(offset);
    }
    // POSIX int-returning interface, with pinned musl's bounded copy policy.
    // Unknown codes use the existing fallback message; errors return a number,
    // not an errno mutation, and a nonempty truncated buffer is terminated.
    public int StrerrorR(int error,sbyte* destination,ulong size)
    {
        sbyte* source=Strerror(error);int length=0;while(source[length]!=0)++length;
        int copied=(ulong)length<size?length:size==0?0:(int)(size-1);
        if(size!=0) {new ReadOnlySpan<sbyte>(source,copied).CopyTo(new Span<sbyte>(destination,copied));destination[copied]=0;}
        return (ulong)length>=size?34:0;
    }
    public static int IsDigit(int value) => unchecked((uint)(value-'0')) <= 9 ? 1 : 0;
    public sbyte* SetLocale(int category,sbyte* locale) { _=Storage;return Locales.SetLocale(category,locale); }
    public ulong MbCurMax() { _=Storage;return Locales.Utf8?4UL:1UL; }
    public void ReleaseThread() => Locales.ReleaseThread();
    public int Wctomb(sbyte* destination,int character)
    {
        _=Storage;
        // Both supplied encodings are stateless. A null destination resets
        // conversion state and ignores the character, as required by wctomb.
        if(destination==null)return 0;
        if(unchecked((uint)character)<128) { *destination=(sbyte)character;return 1; }
        if(!Locales.Utf8) {
            // musl's C locale preserves arbitrary bytes with U+DF80..U+DFFF.
            if(unchecked((uint)(character-0xdf80))<128) { *destination=unchecked((sbyte)character);return 1; }
        } else if(Rune.TryCreate(character,out var rune)) {
            return rune.EncodeToUtf8(new Span<byte>(destination,4));
        }
        setErrno(84);return -1;
    }
    public void Dispose() { Locales.Dispose();Interlocked.Exchange(ref storage,null); }
}
