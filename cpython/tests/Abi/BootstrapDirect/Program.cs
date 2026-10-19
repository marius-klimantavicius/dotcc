using System.Text;
using static DotCC.Libc.Libc;

internal static unsafe class Program
{
    private static int assertions;
    private static void Check(bool value, string message)
    {
        assertions++;
        if (!value) throw new Exception(message);
    }

    private static void Main()
    {
        long epoch = 0;
        tm* calendar = gmtime(&epoch);
        Check(calendar != null, "real gmtime");
        byte* bytes = stackalloc byte[66];
        fixed (byte* format = "%Y-%m-%d %H:%M:%S\0"u8)
        fixed (byte* year = "%Y\0"u8)
        fixed (byte* empty = "\0"u8)
        {
            delegate*<byte*, ulong, byte*, tm*, ulong> call = &strftime;
            foreach (ulong capacity in new ulong[] { 20, 64, 2147483713, 4294967361, ulong.MaxValue })
            {
                new Span<byte>(bytes, 66).Fill(0x5a);
                errno = EDOM;
                Check(call(bytes + 1, capacity, format, calendar) == 19, "LP64 result/capacity");
                Check(errno == EDOM, "success errno");
                Check(Encoding.ASCII.GetString(new ReadOnlySpan<byte>(bytes + 1, 19)) == "1970-01-01 00:00:00", "exact bytes");
                Check(bytes[20] == 0 && bytes[21] == 0x5a && bytes[0] == 0x5a && bytes[65] == 0x5a, "NUL/sentinels");
            }
            foreach (ulong capacity in new ulong[] { 0, 1, 19 })
            {
                new Span<byte>(bytes, 66).Fill(0x5a);
                errno = EDOM;
                Check(call(bytes + 1, capacity, format, calendar) == 0, "insufficient capacity");
                Check(errno == EDOM, "failure errno");
                for (int i = 0; i < 66; i++) Check(bytes[i] == 0x5a, "failed call leaves buffer");
            }
            Check(call(bytes + 1, 1, empty, calendar) == 0 && bytes[1] == 0, "empty format NUL");
            Check(call(null, 64, format, calendar) == 0, "null output");
            Check(call(bytes, 64, null, calendar) == 0, "null format");
            Check(call(bytes, 64, format, null) == 0, "null calendar");
            Check(strftime(bytes, -1, year, calendar) == 0, "legacy negative capacity");
            Check(strftime(bytes, 5, year, calendar) == 4, "legacy managed caller");
        }
        int status = 0x12345678;
        errno = 0;
        Check(waitpid(-1, &status, 1) == -1 && errno == EPERM && status == 0x12345678, "waitpid EPERM/output ownership");
        errno = 0;
        Check(wait(&status) == -1 && errno == EPERM && status == 0x12345678, "wait EPERM/output ownership");
        Console.WriteLine($"{assertions} direct shared-service assertions; dynamicCode={System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported}");
    }
}
