#nullable enable
using System;
using System.Runtime.InteropServices;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    public static int isascii(int c) => (uint)c <= 127 ? 1 : 0;

    // Reuse the explicit-locale scalar service, selecting the current owner's
    // effective global/thread locale. No host setlocale/uselocale is performed.
    private static int ClassifyBackendWide(int c, PosixWideClass kind)
    {
        if (c < 128 || !EffectivePosixUtf8Locale)
            return kind switch
            {
                PosixWideClass.Alnum => isalnum(c), PosixWideClass.Alpha => isalpha(c),
                PosixWideClass.Blank => c is 9 or 32 ? 1 : 0, PosixWideClass.Control => iscntrl(c),
                PosixWideClass.Digit => isdigit(c), PosixWideClass.Graph => isgraph(c),
                PosixWideClass.Lower => islower(c), PosixWideClass.Print => isprint(c),
                PosixWideClass.Punct => ispunct(c), PosixWideClass.Space => isspace(c),
                PosixWideClass.Upper => isupper(c), PosixWideClass.Xdigit => isxdigit(c), _ => 0
            };
        if ((uint)c > 0x10ffff || c is >= 0xd800 and <= 0xdfff) return 0;
        var service = posixUnicodeService.Value;
        if (service == null) { errno = ENOTSUP; return 0; }
        return PosixNativeIswctype((uint)c, service.Classes[(int)kind], service.Locale) != 0 ? 1 : 0;
    }

    public static int iswalnum(int c) => ClassifyBackendWide(c, PosixWideClass.Alnum);
    public static int iswalpha(int c) => ClassifyBackendWide(c, PosixWideClass.Alpha);
    public static int iswblank(int c) => ClassifyBackendWide(c, PosixWideClass.Blank);
    public static int iswcntrl(int c) => ClassifyBackendWide(c, PosixWideClass.Control);
    public static int iswdigit(int c) => ClassifyBackendWide(c, PosixWideClass.Digit);
    public static int iswgraph(int c) => ClassifyBackendWide(c, PosixWideClass.Graph);
    public static int iswlower(int c) => ClassifyBackendWide(c, PosixWideClass.Lower);
    public static int iswprint(int c) => ClassifyBackendWide(c, PosixWideClass.Print);
    public static int iswpunct(int c) => ClassifyBackendWide(c, PosixWideClass.Punct);
    public static int iswspace(int c) => ClassifyBackendWide(c, PosixWideClass.Space);
    public static int iswupper(int c) => ClassifyBackendWide(c, PosixWideClass.Upper);
    public static int iswxdigit(int c) => ClassifyBackendWide(c, PosixWideClass.Xdigit);

    public static int towlower(int c) => !EffectivePosixUtf8Locale || c < 128 ? tolower(c) : MapPosixWide(c, false);
    public static int towupper(int c) => !EffectivePosixUtf8Locale || c < 128 ? toupper(c) : MapPosixWide(c, true);

    private static readonly Lazy<nint> backendNumericLocale = new(() =>
    {
        if (!OperatingSystem.IsLinux()) return 0;
        try { fixed (byte* name = "C\0"u8) return PosixNativeNewlocale(2, name, 0); }
        catch (EntryPointNotFoundException) { return 0; }
        catch (DllNotFoundException) { return 0; }
    });

    /// <summary>Correctly rounded binary32 parsing in the runtime's C numeric
    /// locale, retaining native range, signed-zero, hex and end-pointer rules.
    /// A binary64 parse followed by a cast would introduce double rounding.</summary>
    public static float strtof(byte* text, byte** end)
    {
        if (text == null) { if (end != null) *end = null; errno = EFAULT; return 0; }
        nint locale = backendNumericLocale.Value;
        if (locale == 0) { if (end != null) *end = text; errno = ENOTSUP; return float.NaN; }
        try
        {
            float value = BackendNativeStrtof(text, end, locale);
            int error = Marshal.GetLastPInvokeError();
            if (error != 0) errno = error;
            return value;
        }
        catch (EntryPointNotFoundException) { if (end != null) *end = text; errno = ENOSYS; return float.NaN; }
        catch (DllNotFoundException) { if (end != null) *end = text; errno = ENOSYS; return float.NaN; }
    }

    [DllImport("libc", EntryPoint = "strtof_l", SetLastError = true)]
    private static extern float BackendNativeStrtof(byte* text, byte** end, nint locale);
}
