using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using Managed.Database;
using CoreLibc = Managed.Database.ValkeyCore.Libc;

internal static unsafe class StartupConfigurationProbe
{
    internal static string Run(string directory)
    {
        string[] forbidden = ["save 60 1", "auto-aof-rewrite-percentage 1", "daemonize yes", "replicaof 127.0.0.1 6379"];
        for (int index = 0; index < forbidden.Length; index++)
            IncludedConfiguration(Path.Combine(directory, "fork-" + index), forbidden[index] + "\n", requiresFork: true);
        IncludedConfiguration(Path.Combine(directory, "ordinary"), "latency-monitor-threshold 7\nio-threads 2\n", requiresFork: false);
        return "Four included fork-dependent settings failed with State=1 and clean abandoned-startup cleanup; " +
            "ordinary included latency threshold=7 and io-threads=2 started, stopped and cleaned up successfully.";
    }

    private static void IncludedConfiguration(string directory, string contents, bool requiresFork)
    {
        Directory.CreateDirectory(directory);
        string include = Path.Combine(directory, "included settings.conf");
        File.WriteAllText(include, contents);
        int port;
        using (var reservation = new TcpListener(IPAddress.Loopback, 0))
        {
            reservation.Start();
            port = ((IPEndPoint)reservation.LocalEndpoint).Port;
        }
        string quotedPath = Path.GetFullPath(include).Replace("\\", "\\\\").Replace("\"", "\\\"");
        byte[] configuration = Encoding.UTF8.GetBytes(
            $"port {port.ToString(CultureInfo.InvariantCulture)}\ninclude \"{quotedPath}\"\n\0");
        byte[] workingDirectory = Encoding.UTF8.GetBytes(Path.GetFullPath(directory) + '\0');
        using var core = new ValkeyCore();
        using var binding = core.__DotCcEnter();
        bool cleaned = false;
        try
        {
            fixed (byte* path = workingDirectory) Check.Equal(CoreLibc.chdir(path), 0);
            int result;
            fixed (byte* options = configuration) result = ValkeyHost.Start(core, options);
            if (requiresFork)
            {
                Check.Equal(result, -1);
                Check.Equal(ValkeyHost.State(core), 1);
                string error = Marshal.PtrToStringUTF8((nint)ValkeyHost.LastError(core)) ?? "";
                Check.True(error.Contains("fork", StringComparison.OrdinalIgnoreCase), "Included fork setting did not return a fork-specific error.");
                Check.Equal(ValkeyHost.Cleanup(core, 1), 0);
            }
            else
            {
                Check.Equal(result, 0);
                Check.Equal(ValkeyHost.State(core), 2);
                Check.Equal((long)core.Globals.server.latency_monitor_threshold, 7L);
                Check.Equal(core.Globals.server.io_threads_num, 2);
                Check.Equal(ValkeyHost.Port(core), port);
                Check.Equal(ValkeyHost.Stop(core, ValkeyCore.SHUTDOWN_NOSAVE), 0);
                Check.Equal(ValkeyHost.Cleanup(core, 0), 0);
            }
            cleaned = true;
            Check.Equal(ValkeyHost.State(core), 4);
        }
        finally
        {
            if (!cleaned) Check.Equal(ValkeyHost.Cleanup(core, 1), 0);
        }
    }
}
