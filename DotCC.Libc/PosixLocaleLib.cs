#nullable enable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    private const int PosixAllLocaleMask = 0x1fbf;
    private static long nextPosixLocale;
    private static readonly nint posixGlobalLocale = -1;

    internal sealed class PosixLocale(bool utf8)
    {
        internal bool Utf8 = utf8;
    }
    internal sealed class PosixLocaleSelection
    {
        internal nint Handle = posixGlobalLocale;
    }
    public sealed partial class RuntimeContext
    {
        internal readonly object PosixLocaleGate = new();
        internal readonly Dictionary<nint, PosixLocale> PosixLocales = new();
        internal readonly ConditionalWeakTable<Thread, PosixLocaleSelection> PosixLocaleThreads = new();
    }

    // Handles are non-dereferenceable monotonically assigned tokens. No native
    // storage is borrowed or retained, and freed tokens are never reused. The
    // owner table is authoritative, so foreign/stale addresses cannot alias a
    // later object. Zero and -1 retain their POSIX meanings.
    private static void* AllocatePosixLocale(RuntimeContext owner, bool utf8)
    {
        if (utf8 && posixUnicodeService.Value == null) { errno = ENOTSUP; return null; }
        long identity = Interlocked.Increment(ref nextPosixLocale);
        if (identity <= 0 || (IntPtr.Size == 4 && identity > int.MaxValue))
        { errno = ENOMEM; return null; }
        owner.PosixLocales.Add((nint)identity, new PosixLocale(utf8));
        return (void*)(nint)identity;
    }

    private static bool PosixLocaleInUse(RuntimeContext owner, nint handle)
    {
        foreach (var entry in owner.PosixLocaleThreads)
            if (entry.Value.Handle == handle && entry.Key.IsAlive) return true;
        return false;
    }

    private static bool TryPosixLocale(void* handle, out bool utf8)
    {
        var owner = RuntimeState;
        lock (owner.PosixLocaleGate)
        {
            if (owner.PosixLocales.TryGetValue((nint)handle, out var locale))
            { utf8 = locale.Utf8; return true; }
        }
        utf8 = false;
        errno = EINVAL;
        return false;
    }

    private static bool EffectivePosixUtf8Locale
    {
        get
        {
            var owner = RuntimeState;
            lock (owner.PosixLocaleGate)
            {
                if (owner.PosixLocaleThreads.TryGetValue(Thread.CurrentThread, out var selection)
                    && selection.Handle != posixGlobalLocale)
                {
                    if (owner.PosixLocales.TryGetValue(selection.Handle, out var locale)) return locale.Utf8;
                    errno = EINVAL;
                    return false;
                }
                return Volatile.Read(ref owner.Utf8MultibyteLocale);
            }
        }
    }

    // Called only when the owner's active-binding/worker count is zero.
    private static void DisposePosixLocales(RuntimeContext owner)
    {
        lock (owner.PosixLocaleGate)
        {
            owner.PosixLocaleThreads.Clear();
            owner.PosixLocales.Clear();
        }
    }

    /// <summary>Create or modify a C/POSIX or C.UTF-8 locale. An existing base
    /// is modified in place only after validation succeeds. Selected bases are
    /// rejected with EBUSY; unsupported names fail with ENOENT without changing
    /// the base. Empty names select C, consistently with setlocale.</summary>
    public static void* newlocale(int mask, byte* name, void* basis)
    {
        if ((mask & ~PosixAllLocaleMask) != 0 || name == null || (nint)basis == posixGlobalLocale)
        { errno = EINVAL; return null; }
        try
        {
            string value = Encoding.UTF8.GetString(name, strlen(name));
            bool utf8 = value is "C.UTF-8" or "C.utf8";
            if (!utf8 && value is not ("" or "C" or "POSIX")) { errno = ENOENT; return null; }
            var owner = RuntimeState;
            lock (owner.PosixLocaleGate)
            {
                if (basis == null) return AllocatePosixLocale(owner, (mask & 1) != 0 && utf8);
                if (!owner.PosixLocales.TryGetValue((nint)basis, out var locale)) { errno = EINVAL; return null; }
                if (PosixLocaleInUse(owner, (nint)basis)) { errno = EBUSY; return null; }
                if ((mask & 1) != 0)
                {
                    if (utf8 && posixUnicodeService.Value == null) { errno = ENOTSUP; return null; }
                    locale.Utf8 = utf8;
                }
                return basis;
            }
        }
        catch (OutOfMemoryException) { errno = ENOMEM; return null; }
    }

    public static void* duplocale(void* source)
    {
        var owner = RuntimeState;
        lock (owner.PosixLocaleGate)
        {
            bool utf8;
            if ((nint)source == posixGlobalLocale) utf8 = Volatile.Read(ref owner.Utf8MultibyteLocale);
            else if (owner.PosixLocales.TryGetValue((nint)source, out var locale)) utf8 = locale.Utf8;
            else { errno = EINVAL; return null; }
            try { return AllocatePosixLocale(owner, utf8); }
            catch (OutOfMemoryException) { errno = ENOMEM; return null; }
        }
    }

    /// <summary>Thread-and-owner-specific selection. NULL queries. -1 selects
    /// the owner's global locale. Switching owner bindings preserves each
    /// owner's selection independently on the same physical thread.</summary>
    public static void* uselocale(void* locale)
    {
        var owner = RuntimeState;
        lock (owner.PosixLocaleGate)
        {
            var current = owner.PosixLocaleThreads.GetValue(Thread.CurrentThread, static _ => new PosixLocaleSelection());
            nint previous = current.Handle;
            if (locale == null) return (void*)previous;
            if ((nint)locale != posixGlobalLocale && !owner.PosixLocales.ContainsKey((nint)locale))
            { errno = EINVAL; return null; }
            current.Handle = (nint)locale;
            return (void*)previous;
        }
    }

    /// <summary>Release an unselected handle. Invalid, double/foreign frees set
    /// EINVAL; a handle selected on a live thread sets EBUSY and remains valid.
    /// POSIX leaves these misuses undefined; this runtime diagnoses them.</summary>
    public static void freelocale(void* locale)
    {
        var owner = RuntimeState;
        lock (owner.PosixLocaleGate)
        {
            if (!owner.PosixLocales.ContainsKey((nint)locale)) { errno = EINVAL; return; }
            if (PosixLocaleInUse(owner, (nint)locale)) { errno = EBUSY; return; }
            owner.PosixLocales.Remove((nint)locale);
        }
    }

    public static byte* nl_langinfo_l(int item, void* locale)
    {
        if (!TryPosixLocale(locale, out bool utf8)) return null;
        if (item == 14) return (byte*)(utf8 ? utf8CodesetStorage : asciiCodesetStorage);
        // glibc extended monetary/numeric nl_item values. Keep the numeric
        // sentinel consistent with localeconv and dotcc's unsigned char ABI.
        if (item is >= 262144 and <= 262150 || item == 65538) return LiteralPool.Pointer + LiteralPool.Empty;
        if (item is >= 262151 and <= 262158 or >= 262160 and <= 262165) return (byte*)posixUnavailableMonetary;
        if (cLanginfo.Value.TryGetValue(item, out nint value)) return (byte*)value;
        errno = ENOTSUP;
        return LiteralPool.Pointer + LiteralPool.Empty;
    }
    private static readonly nint posixUnavailableMonetary = CreatePosixUnavailableMonetary();
    private static nint CreatePosixUnavailableMonetary()
    {
        byte* value = (byte*)NativeMemory.Alloc(2);
        value[0] = 255; value[1] = 0;
        return (nint)value;
    }

    public static int strcoll_l(byte* a, byte* b, void* locale) => TryPosixLocale(locale, out _) ? strcmp(a, b) : 0;
    public static ulong strxfrm_l(byte* destination, byte* source, ulong n, void* locale)
    {
        if (!TryPosixLocale(locale, out _)) return ulong.MaxValue;
        if (source == null || (destination == null && n != 0)) { errno = EFAULT; return ulong.MaxValue; }
        ulong length = 0;
        while (source[length] != 0)
        {
            if (length < n) destination[length] = source[length];
            length++;
        }
        if (length < n) destination[length] = 0;
        return length;
    }
    public static ulong strftime_l(byte* destination, ulong n, byte* format, tm* time, void* locale)
    {
        if (!TryPosixLocale(locale, out _)) return 0;
        if (format == null || time == null || (destination == null && n != 0)) { errno = EFAULT; return 0; }
        // CalendarLib implements this C formatting subset. Do not turn an
        // unimplemented specifier into a successful literal "%x" response.
        for (byte* p = format; *p != 0; p++)
        {
            if (*p != '%') continue;
            p++;
            if (*p == 0 || "YyCmdeHIMSpAaBbhjwuFTRDnt%".IndexOf((char)*p) < 0)
            { errno = ENOTSUP; return 0; }
        }
        return strftime(destination, n, format, time);
    }

    public static int isalnum_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isalnum(c) : 0;
    public static int isalpha_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isalpha(c) : 0;
    public static int isblank_l(int c, void* locale) => TryPosixLocale(locale, out _) && c is 9 or 32 ? 1 : 0;
    public static int iscntrl_l(int c, void* locale) => TryPosixLocale(locale, out _) ? iscntrl(c) : 0;
    public static int isdigit_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isdigit(c) : 0;
    public static int isgraph_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isgraph(c) : 0;
    public static int islower_l(int c, void* locale) => TryPosixLocale(locale, out _) ? islower(c) : 0;
    public static int isprint_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isprint(c) : 0;
    public static int ispunct_l(int c, void* locale) => TryPosixLocale(locale, out _) ? ispunct(c) : 0;
    public static int isspace_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isspace(c) : 0;
    public static int isupper_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isupper(c) : 0;
    public static int isxdigit_l(int c, void* locale) => TryPosixLocale(locale, out _) ? isxdigit(c) : 0;
    public static int tolower_l(int c, void* locale) => TryPosixLocale(locale, out _) ? tolower(c) : c;
    public static int toupper_l(int c, void* locale) => TryPosixLocale(locale, out _) ? toupper(c) : c;

    private enum PosixWideClass { Alnum, Alpha, Blank, Control, Digit, Graph, Lower, Print, Punct, Space, Upper, Xdigit }
    private static int ClassifyPosixWide(int c, void* locale, PosixWideClass kind)
    {
        if (!TryPosixLocale(locale, out bool utf8)) return 0;
        if (c < 128 || !utf8)
        {
            return kind switch
            {
                PosixWideClass.Alnum => isalnum(c), PosixWideClass.Alpha => isalpha(c),
                PosixWideClass.Blank => c is 9 or 32 ? 1 : 0, PosixWideClass.Control => iscntrl(c),
                PosixWideClass.Digit => isdigit(c), PosixWideClass.Graph => isgraph(c),
                PosixWideClass.Lower => islower(c), PosixWideClass.Print => isprint(c),
                PosixWideClass.Punct => ispunct(c), PosixWideClass.Space => isspace(c),
                PosixWideClass.Upper => isupper(c), PosixWideClass.Xdigit => isxdigit(c), _ => 0
            };
        }
        if ((uint)c > 0x10ffff || c is >= 0xd800 and <= 0xdfff) return 0;
        var service = posixUnicodeService.Value;
        if (service == null) { errno = ENOTSUP; return 0; }
        return PosixNativeIswctype((uint)c, service.Classes[(int)kind], service.Locale) != 0 ? 1 : 0;
    }
    public static int iswalnum_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Alnum);
    public static int iswalpha_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Alpha);
    public static int iswblank_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Blank);
    public static int iswcntrl_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Control);
    public static int iswdigit_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Digit);
    public static int iswgraph_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Graph);
    public static int iswlower_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Lower);
    public static int iswprint_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Print);
    public static int iswpunct_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Punct);
    public static int iswspace_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Space);
    public static int iswupper_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Upper);
    public static int iswxdigit_l(int c, void* locale) => ClassifyPosixWide(c, locale, PosixWideClass.Xdigit);
    public static int towlower_l(int c, void* locale)
    {
        if (!TryPosixLocale(locale, out bool utf8)) return c;
        if (!utf8 || c < 128) return tolower(c);
        return MapPosixWide(c, false);
    }
    public static int towupper_l(int c, void* locale)
    {
        if (!TryPosixLocale(locale, out bool utf8)) return c;
        if (!utf8 || c < 128) return toupper(c);
        return MapPosixWide(c, true);
    }
    private static int MapPosixWide(int c, bool upper)
    {
        if ((uint)c > 0x10ffff || c is >= 0xd800 and <= 0xdfff) return c;
        var service = posixUnicodeService.Value;
        if (service == null) { errno = ENOTSUP; return c; }
        return (int)PosixNativeTowctrans((uint)c, upper ? service.Upper : service.Lower, service.Locale);
    }

    // Only Unicode *scalar* host services cross this boundary: unsigned
    // 32-bit wint_t, unsigned-long wctype_t and opaque pointers on Linux LP64.
    // Never pass dotcc's UTF-16 wchar_t* to a native wide-string function.
    private sealed class PosixUnicodeService(nint locale, ulong[] classes, nint lower, nint upper)
    {
        internal readonly nint Locale = locale, Lower = lower, Upper = upper;
        internal readonly ulong[] Classes = classes;
    }
    private static readonly Lazy<PosixUnicodeService?> posixUnicodeService = new(CreatePosixUnicodeService);
    private static PosixUnicodeService? CreatePosixUnicodeService()
    {
        if (!OperatingSystem.IsLinux() || IntPtr.Size != 8) return null;
        nint locale = 0;
        try
        {
            fixed (byte* name = "C.UTF-8\0"u8) locale = PosixNativeNewlocale(1, name, 0);
            if (locale == 0) return null;
            string[] names = ["alnum", "alpha", "blank", "cntrl", "digit", "graph", "lower", "print", "punct", "space", "upper", "xdigit"];
            var classes = new ulong[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                byte[] name = Encoding.ASCII.GetBytes(names[i] + "\0");
                fixed (byte* text = name) classes[i] = PosixNativeWctype(text, locale);
                if (classes[i] == 0) return null;
            }
            nint lower, upper;
            fixed (byte* name = "tolower\0"u8) lower = PosixNativeWctrans(name, locale);
            fixed (byte* name = "toupper\0"u8) upper = PosixNativeWctrans(name, locale);
            if (lower == 0 || upper == 0) return null;
            if (PosixNativeIswctype('A', classes[(int)PosixWideClass.Alpha], locale) == 0
                || PosixNativeTowctrans('A', lower, locale) != 'a'
                || PosixNativeTowctrans('a', upper, locale) != 'A') return null;
            var service = new PosixUnicodeService(locale, classes, lower, upper);
            // Immutable process-lifetime service, like the borrowed locale name
            // strings. It never selects or modifies the host global/TLS locale.
            locale = 0;
            return service;
        }
        catch (DllNotFoundException) { return null; }
        catch (EntryPointNotFoundException) { return null; }
        finally { if (locale != 0) PosixNativeFreelocale(locale); }
    }
    [DllImport("libc", EntryPoint = "newlocale")]
    private static extern nint PosixNativeNewlocale(int mask, byte* name, nint basis);
    [DllImport("libc", EntryPoint = "freelocale")]
    private static extern void PosixNativeFreelocale(nint locale);
    [DllImport("libc", EntryPoint = "wctype_l")]
    private static extern ulong PosixNativeWctype(byte* name, nint locale);
    [DllImport("libc", EntryPoint = "wctrans_l")]
    private static extern nint PosixNativeWctrans(byte* name, nint locale);
    [DllImport("libc", EntryPoint = "iswctype_l")]
    private static extern int PosixNativeIswctype(uint c, ulong kind, nint locale);
    [DllImport("libc", EntryPoint = "towctrans_l")]
    private static extern uint PosixNativeTowctrans(uint c, nint transform, nint locale);

}
