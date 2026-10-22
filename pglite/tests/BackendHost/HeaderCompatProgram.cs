using Core = Managed.PGlite.Probes.BackendHostHeaderFixture;

internal static unsafe class HeaderCompatProgram
{
    public static int Main(string[] args)
    {
        ulong[] expected = File.ReadAllText(args[0]).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray();
        if (expected.Length != 5) throw new Exception("Incomplete native header observations");
        ulong* actual = stackalloc ulong[5];
        for (int iteration = 0; iteration < 2; iteration++)
        {
            using var owner = Core.__DotCcCreateContext(); using var binding = owner.Enter();
            int result = Core.dph_header_compat(actual);
            if (result != 0) throw new Exception($"Emitted C assertion failed at line {result}");
            for (int field = 0; field < expected.Length; field++)
                if (actual[field] != expected[field]) throw new Exception($"Header observation {field}: expected {expected[field]}, got {actual[field]}");
        }
        Console.WriteLine("PASS native/emitted header compatibility: SIGURG, IPv6 unspecified and two-unit remapped shmctl/shmat typing, two owners");
        return 0;
    }
}
