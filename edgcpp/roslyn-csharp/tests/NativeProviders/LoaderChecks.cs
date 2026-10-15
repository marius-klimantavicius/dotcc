using System.Runtime.InteropServices;
using Application = Translated.Program;

internal static unsafe class LoaderProviderChecks
{
    [DllImport("raw-loader", EntryPoint = "native_loader_raw_fail")]
    private static extern void RawFail();
    private static void Require(bool condition, string check)
    {
        if (!condition) throw new InvalidOperationException("Generated loader provider contract failed: " + check);
    }
    private static string Bytes(sbyte* pointer) => Marshal.PtrToStringUTF8((nint)pointer) ?? "";
    private static void Churn()
    {
        GC.KeepAlive(new byte[128 * 1024]);
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    }
    private static int Main(string[] args)
    {
        if (args.Length != 3) throw new ArgumentException("Expected exact zero-symbol provider, raw-interference helper and libc paths.");
        var path = Marshal.StringToCoTaskMemUTF8(Path.GetFullPath(args[0]));
        *Application.loader_provider_path = (sbyte*)path;
        var helper = NativeLibrary.Load(Path.GetFullPath(args[1]));
        NativeLibrary.SetDllImportResolver(typeof(LoaderProviderChecks).Assembly, (name, _, _) => name == "raw-loader" ? helper : 0);
        for (var iteration = 0; iteration < 2; ++iteration)
        {
            Require(Application.fixture() == 17 && Application.loader_observation_count() == 17, "reference result/count");
            for (var index = 0; index < 17; ++index) Require(Application.loader_observe(index) == 1, "observation " + index);
        }
        var canonical = *Application.loader_error_address;
        Require(Application.loader_address_identity() == 1, "direct/constant-address identity");
        Application.loader_fail_pending();
        RawFail(); Churn();
        var message = Application.loader_read();
        var copied = Bytes(message);
        Require(message != null && copied.Contains("roslynobj-provider-application-missing") && !copied.Contains("raw-missing"), "native snapshot before raw/CLR interference");
        Require(Application.loader_read_indirect() == null, "logical read-clear");
        RawFail(); Churn();
        Require(Bytes(message) == copied, "read-clear and unrelated raw calls keep returned borrowed bytes alive");
        var libc = NativeLibrary.Load(Path.GetFullPath(args[2]));
        var rawAccessor = NativeLibrary.GetExport(libc, "dlerror");
        Require(rawAccessor != canonical, "explicit canonical logical accessor");
        *Application.loader_error_address = rawAccessor;
        Application.loader_fail_pending();
        RawFail(); Churn();
        var indirect = Application.loader_read_indirect();
        Require(indirect != null && Bytes(indirect).Contains("roslynobj-provider-application-missing") && Application.loader_read() == null,
            "unknown native indirect selected dlerror address uses logical read-clear");
        *Application.loader_error_address = canonical;
        Require(Application.loader_address_identity() == 1, "canonical restoration");
        Marshal.FreeCoTaskMem(path);
        Console.WriteLine("result=17");
        Console.WriteLine("observations=" + string.Join(",", Enumerable.Repeat(1, 17)));
        Console.WriteLine("loader-snapshot-read-clear-lifetime=passed");
        Console.WriteLine("loader-canonical-and-unknown-indirect=passed");
        return 0;
    }
}
