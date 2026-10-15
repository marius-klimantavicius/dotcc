using System.Runtime.InteropServices;
using Application = Translated.Program;

internal static unsafe class ErrnoProviderChecks
{
    [DllImport("raw-errno", EntryPoint = "raw_errno_address")]
    private static extern nuint RawAddress();
    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Generated host errno protocol failed: " + check);
    }
    private static void Churn()
    {
        for (var index = 0; index < 4; ++index) GC.KeepAlive(new byte[128 * 1024]);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    }
    private static int Main(string[] args)
    {
        if (args.Length != 1) throw new ArgumentException("Expected exact raw-interference helper path.");
        var helper = NativeLibrary.Load(Path.GetFullPath(args[0]));
        NativeLibrary.SetDllImportResolver(typeof(ErrnoProviderChecks).Assembly, (name, _, _) => name == "raw-errno" ? helper : 0);
        int[] expected = [1201, 1201, -1, 20, 1, 555, 777, 777, 444, 333, 2, 9, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 901, 1];
        for (var iteration = 0; iteration < 2; ++iteration)
        {
            Require(Application.fixture() == 6223 && Application.provider_observation_count() == expected.Length, "reference result/count");
            for (var index = 0; index < expected.Length; ++index)
                Require(Application.provider_observe(index) == expected[index], $"reference observation {index}");
        }
        var location = (delegate* unmanaged[Cdecl]<int*>)(*Application.errno_address);
        var logical = location();
        var raw = (int*)RawAddress();
        Require(logical != null && raw != null && logical != raw && location() == logical, "explicit logical TLS address contract");
        Application.provider_errno_write(2134);
        *raw = 777;
        Require(*raw == 777 && *logical == 2134, "raw interference is distinct from application writes");
        Churn();
        Require(Application.provider_errno_read() == 2134 && Application.provider_errno_identity() == 1, "logical value/address after allocation and GC");
        *raw = 888;
        Require(Application.provider_errno_success() == 2134, "success leaving errno unchanged uses the application value");
        *raw = 999;
        Require(Application.provider_errno_failure() == -1 && *logical == 20, "immediate native failure snapshot");
        *raw = 1111;
        Churn();
        Require(Application.provider_errno_read() == 20, "failure survives unrelated raw overwrite and GC");
        *logical = 3214;
        *raw = 2222;
        Require(Application.provider_errno_read() == 3214 && Application.provider_errno_success() == 3214 && Application.provider_errno_identity() == 1,
            "application pointer stores feed direct/indirect provider calls");
        Console.WriteLine("result=6223");
        Console.WriteLine("observations=" + string.Join(",", expected));
        Console.WriteLine("logical-errno-interference=passed");
        Console.WriteLine("nested-qsort-foreign-pthread=passed");
        return 0;
    }
}
