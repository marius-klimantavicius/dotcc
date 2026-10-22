using System.Text.Json;
using PG = Managed.PGlite.Probes.Locale;

internal static unsafe class Program
{
    private static void Require(bool condition, string operation)
    {
        if (!condition) throw new InvalidOperationException(operation);
    }
    public static int Main(string[] args)
    {
        try
        {
            Require(args.Length == 2, "Expected NATIVE_JSON MANAGED_JSON");
            using var native = JsonDocument.Parse(File.ReadAllText(args[0]));
            using var owner = new PG.Libc.RuntimeContext();
            using var binding = owner.Enter();
            ulong[] layout = Enumerable.Range(0, 8).Select(i => PG.dpl_layout((uint)i)).ToArray();
            ulong[] expected = native.RootElement.GetProperty("layout").EnumerateArray().Select(x => x.GetUInt64()).ToArray();
            Require(layout.SequenceEqual(expected), "native/emitted actual pg_locale_struct and opaque locale_t layout");
            PG.pg_locale_struct value = default;
            Require((ulong)sizeof(PG.pg_locale_struct) == expected[2], "actual CLR pg_locale_struct storage size");
            Require((ulong)((byte*)&value.collate - (byte*)&value) == expected[4], "actual CLR collate field offset");
            Require((ulong)((byte*)&value.info - (byte*)&value) == expected[5], "actual CLR union offset");
            Require((ulong)((byte*)&value.info.lt - (byte*)&value) == expected[7], "actual CLR opaque locale_t field offset");
            int status = PG.dpl_contracts();
            Require(status == 0, $"actual emitted locale contract assertion at pg_locale.c:{status}");
            ulong digest = PG.dpl_unicode_digest();
            Require(digest == native.RootElement.GetProperty("unicode_digest").GetUInt64(), "all 1,112,064 Unicode scalar classifications and simple case mappings");
            Require(PG.dpl_supplementary_units() == 2, "intended UTF-16 multibyte conversion uses two units for supplementary scalars");
            Require(native.RootElement.GetProperty("wchar_bytes").GetInt32() == 4 && native.RootElement.GetProperty("supplementary_units").GetInt32() == 1,
                "separate native wchar32 profile is recorded, never compared as UTF-16 ABI parity");
            nint stale;
            fixed (byte* name = "C\0"u8)
            {
                stale = (nint)PG.Libc.newlocale(0x1fbf, name, null);
                Require(stale != 0, "emitted handle creation");
                using var foreign = new PG.Libc.RuntimeContext();
                using (foreign.Enter())
                {
                    Require(PG.Libc.duplocale((void*)stale) == null && PG.Libc.errno == PG.Libc.EINVAL, "emitted foreign-owner rejection");
                    PG.Libc.freelocale((void*)stale);
                    Require(PG.Libc.errno == PG.Libc.EINVAL, "emitted foreign free rejection");
                }
                PG.Libc.freelocale((void*)stale);
                Require(PG.Libc.duplocale((void*)stale) == null && PG.Libc.errno == PG.Libc.EINVAL, "emitted freed-handle rejection");
            }
            File.WriteAllText(args[1], "{\"layout\":[" + string.Join(",", layout)
                + "],\"unicode_digest\":" + digest + ",\"unicode_scalars\":1112064,"
                + "\"behavior\":true,\"owner_errors\":true,\"wchar_bytes\":2,\"supplementary_units\":2,"
                + "\"native_wchar_bytes\":4,\"native_supplementary_units\":1}");
            Console.WriteLine("PASS actual PG locale layout, libc operations, owner errors and all Unicode scalar classes/case; UTF16/UTF32 distinction retained");
            return 0;
        }
        catch (Exception error) { Console.Error.WriteLine(error); return 1; }
    }
}
