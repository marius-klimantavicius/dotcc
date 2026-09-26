using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Managed.Database;
using CoreLibc = Managed.Database.ValkeyCore.Libc;

internal static unsafe class NativeModuleBoundaryProbe
{
    internal static string Run()
    {
        // Use an existing native library, not a missing path that the old loader
        // would already reject. Never call a native export with the managed ABI.
        string[] candidates = OperatingSystem.IsWindows()
            ? [Path.Combine(Environment.SystemDirectory, "kernel32.dll")]
            : OperatingSystem.IsMacOS()
                ? ["/usr/lib/libSystem.B.dylib"]
                : ["/lib/x86_64-linux-gnu/libc.so.6", "/lib/aarch64-linux-gnu/libc.so.6",
                    "/usr/lib64/libc.so.6", "/lib64/libc.so.6"];
        string library = candidates.FirstOrDefault(File.Exists)
            ?? throw new InvalidOperationException("No existing platform native library found for the module ABI probe.");
        using var core = new ValkeyCore();
        using var binding = core.__DotCcEnter();
        core.initServerConfig();
        byte[] path = Encoding.UTF8.GetBytes(library + '\0');
        fixed (byte* nativePath = path)
        {
            for (int loadEx = 0; loadEx <= 1; ++loadEx)
            {
                CoreLibc.errno = 0;
                Check.Equal(core.moduleLoad(nativePath, null, 0, loadEx), ValkeyCore.C_ERR);
                Check.Equal(CoreLibc.errno, CoreLibc.ENOTSUP);
                Check.True(core.__DotCcRuntime.Termination is null, "Native module rejection terminated the owner.");
            }
        }
        QueuedStartup(library);
        return $"Existing native library {library}: LOAD and LOADEX returned C_ERR/ENOTSUP before native ABI entry; " +
            "startup loadmodule exited only its owner with status 1, and partial-startup cleanup completed before disposal.";
    }

    private static void QueuedStartup(string library)
    {
        int port;
        using (var reservation = new TcpListener(IPAddress.Loopback, 0))
        {
            reservation.Start();
            port = ((IPEndPoint)reservation.LocalEndpoint).Port;
        }
        string quotedLibrary = library.Replace("\\", "\\\\").Replace("\"", "\\\"");
        byte[] configuration = Encoding.UTF8.GetBytes(
            $"port {port.ToString(CultureInfo.InvariantCulture)}\nloadmodule \"{quotedLibrary}\"\n\0");
        using var core = new ValkeyCore();
        using var binding = core.__DotCcEnter();
        try
        {
            fixed (byte* options = configuration)
            {
                int result = ValkeyHost.Start(core, options);
                throw new InvalidOperationException($"Startup loadmodule was not rejected through owned exit (returned {result}).");
            }
        }
        catch (CoreLibc.RuntimeTerminationException error) when (ReferenceEquals(error.Owner, core.__DotCcRuntime))
        {
            Check.Equal(error.Termination.Kind, CoreLibc.RuntimeTerminationKind.Exit);
            Check.Equal(error.Termination.Status, 1);
        }
        finally
        {
            Check.Equal(ValkeyHost.Cleanup(core, 1), 0);
        }
        Check.Equal(ValkeyHost.State(core), 4);
    }
}
