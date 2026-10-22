using System.Text;
using Core = Managed.PGlite.Probes.BackendHostFixture;

internal static unsafe class EmittedProgram
{
    public static int Main(string[] args)
    {
        if (args.Length != 2) throw new ArgumentException("expected native observations and output directory");
        ulong[] expected = File.ReadAllText(args[0]).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray();
        if (expected.Length != 18) throw new Exception("Incomplete native observation list");
        for (int iteration = 0; iteration < 2; iteration++)
        {
            string directory = Path.Combine(args[1], "owner-" + iteration);
            Directory.CreateDirectory(directory);
            ulong* actual = stackalloc ulong[18];
            using var owner = Core.__DotCcCreateContext(); using var binding = owner.Enter();
            fixed (byte* path = Encoding.UTF8.GetBytes(directory + "\0"))
            {
                int result = Core.dph_backend_probe(path, actual);
                if (result != 0) throw new Exception($"Emitted C assertion failed at line {result}; errno={Core.Libc.errno}");
            }
            for (int field = 0; field < expected.Length; field++)
                if (actual[field] != expected[field]) throw new Exception($"Native/emitted observation {field}: expected {expected[field]}, got {actual[field]}");
        }
        Console.WriteLine("PASS actual C backend-host native comparison, two generated owners, 18 observations each");
        return 0;
    }
}
