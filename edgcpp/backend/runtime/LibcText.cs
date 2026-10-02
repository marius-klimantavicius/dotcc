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
    // Narrow ctype uses pinned musl 1.2.6 ASCII predicates in every supported locale.
    public static int IsAlnum(int value) => IsAlpha(value)!=0 || IsDigit(value)!=0 ? 1 : 0;
    public static int IsAlpha(int value) => unchecked(((uint)value|32U)-'a')<26 ? 1 : 0;
    public static int IsBlank(int value) => value==' ' || value=='\t' ? 1 : 0;
    public static int IsCntrl(int value) => unchecked((uint)value)<32 || value==127 ? 1 : 0;
    public static int IsDigit(int value) => unchecked((uint)(value-'0'))<10 ? 1 : 0;
    public static int IsGraph(int value) => unchecked((uint)(value-33))<94 ? 1 : 0;
    public static int IsLower(int value) => unchecked((uint)(value-'a'))<26 ? 1 : 0;
    public static int IsPrint(int value) => unchecked((uint)(value-32))<95 ? 1 : 0;
    public static int IsPunct(int value) => IsGraph(value)!=0 && IsAlnum(value)==0 ? 1 : 0;
    public static int IsSpace(int value) => value==' ' || unchecked((uint)(value-'\t'))<5 ? 1 : 0;
    public static int IsUpper(int value) => unchecked((uint)(value-'A'))<26 ? 1 : 0;
    public static int IsXdigit(int value) => IsDigit(value)!=0 || unchecked(((uint)value|32U)-'a')<6 ? 1 : 0;
    public static int IsAscii(int value) => unchecked((uint)value)<128 ? 1 : 0;
    public static int ToLower(int value) => IsUpper(value)!=0 ? value|32 : value;
    public static int ToUpper(int value) => IsLower(value)!=0 ? value&~32 : value;
    public int IsAlnumL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsAlnum(value) : 0;
    public int IsAlphaL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsAlpha(value) : 0;
    public int IsBlankL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsBlank(value) : 0;
    public int IsCntrlL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsCntrl(value) : 0;
    public int IsDigitL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsDigit(value) : 0;
    public int IsGraphL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsGraph(value) : 0;
    public int IsLowerL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsLower(value) : 0;
    public int IsPrintL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsPrint(value) : 0;
    public int IsPunctL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsPunct(value) : 0;
    public int IsSpaceL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsSpace(value) : 0;
    public int IsUpperL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsUpper(value) : 0;
    public int IsXdigitL(int value,void* locale) => Locales.Validate(locale)!=0 ? IsXdigit(value) : 0;
    public int ToLowerL(int value,void* locale) => Locales.Validate(locale)!=0 ? ToLower(value) : value;
    public int ToUpperL(int value,void* locale) => Locales.Validate(locale)!=0 ? ToUpper(value) : value;
    public static int Strcoll(sbyte* left,sbyte* right)
    {
        while(*left!=0 && *left==*right){++left;++right;}
        return (byte)*left-(byte)*right;
    }
    public static ulong Strxfrm(sbyte* destination,sbyte* source,ulong size)
    {
        ulong length=0;while(source[length]!=0)++length;
        // musl only writes when the entire transformed string plus NUL fits.
        if(size>length)for(ulong i=0;i<=length;++i)destination[i]=source[i];
        return length;
    }
    public int StrcollL(sbyte* left,sbyte* right,void* locale) => Locales.Validate(locale)!=0 ? Strcoll(left,right) : 0;
    public ulong StrxfrmL(sbyte* destination,sbyte* source,ulong size,void* locale) => Locales.Validate(locale)!=0 ? Strxfrm(destination,source,size) : 0;
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
