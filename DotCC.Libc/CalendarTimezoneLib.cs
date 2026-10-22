#nullable enable

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DotCC.Libc;

public static unsafe partial class Libc
{
    // The guest tm is the Linux x64 LP64 layout (56 bytes, gmtoff/zone at
    // 40/48). Other hosts retain the managed DateTime/TimeZoneInfo calendar
    // path. Their unavailable C abbreviations are explicitly empty, never
    // fabricated from a display name or replaced by a UTC placeholder.
    private static bool UseNativeCalendar => OperatingSystem.IsLinux()
        && RuntimeInformation.ProcessArchitecture == Architecture.X64;

    // OS calendar host services only: no database code and no global TZ,
    // locale or environment mutation. Pointer/scalar imports are AOT-safe.
    [DllImport("libc", EntryPoint = "localtime_r")]
    private static extern tm* NativeCalendarLocaltime(long* timer, tm* result);
    [DllImport("libc", EntryPoint = "mktime", SetLastError = true)]
    private static extern long NativeCalendarMktime(tm* value);
    [DllImport("libc", EntryPoint = "strftime")]
    private static extern nuint NativeCalendarStrftime(byte* output, nuint capacity, byte* format, tm* value);

    private static void RetainCalendarZone(tm* value)
    {
        if (value->tm_zone != null)
            value->tm_zone = CalendarZoneNames.Intern(Marshal.PtrToStringUTF8((nint)value->tm_zone) ?? "");
    }

    private static string CalendarZoneName(tm* value)
    {
        // GNU/Linux gives an explicitly supplied nonempty tm_zone priority,
        // including when the DST flag is unknown. Do not recompute that name
        // from the offset, local zone, or calendar date.
        if (value->tm_zone != null && value->tm_zone[0] != 0)
            return Marshal.PtrToStringUTF8((nint)value->tm_zone) ?? "";
        if (value->tm_isdst < 0 || !UseNativeCalendar) return "";

        // GNU/Linux falls back to its actual standard/daylight zone name for
        // NULL/empty tm_zone. Ask the host without reading mutable tzname data
        // or inventing an abbreviation. Delimiters distinguish an empty name
        // from an insufficient buffer, so long POSIX TZ names need no cap.
        fixed (byte* format = "[%Z]\0"u8)
        {
            for (int capacity = 64; ; capacity = checked(capacity * 2))
            {
                var bytes = new byte[capacity];
                fixed (byte* output = bytes)
                {
                    nuint length = NativeCalendarStrftime(output, (nuint)capacity, format, value);
                    if (length != 0) return Encoding.UTF8.GetString(bytes, 1, checked((int)length) - 2);
                }
            }
        }
    }

    // tm values (including caller-owned *_r results) may survive the calling
    // thread and RuntimeContext. These literal-like, immutable allocations
    // intentionally have process lifetime, independent of the guest heap.
    // Locking prevents duplicate allocations and publication races. No native
    // borrowed tm_zone pointer escapes a calendar conversion.
    private static class CalendarZoneNames
    {
        private static readonly Dictionary<string, nint> Names = new(StringComparer.Ordinal);
        public static byte* Intern(string name)
        {
            lock (Names)
            {
                if (Names.TryGetValue(name, out nint existing)) return (byte*)existing;
                byte[] bytes = Encoding.UTF8.GetBytes(name);
                byte* storage = (byte*)NativeMemory.Alloc((nuint)bytes.Length + 1);
                bytes.CopyTo(new Span<byte>(storage, bytes.Length));
                storage[bytes.Length] = 0;
                Names.Add(name, (nint)storage);
                return storage;
            }
        }
    }
}
