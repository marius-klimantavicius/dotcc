using System.Diagnostics;
using Managed.Emulation.Host;

internal static class NativeOracle
{
    internal static async Task Run(string elf)
    {
        using var deadline = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        for (int i = 0; i < 12; ++i)
        {
            await using var server = new ImdsV2Server(new ImdsV2Options { InstanceId = "i-native", RoleName = "test-role",
                Credentials = new("TESTACCESSnative", "TESTSECRET", "TESTSESSION", DateTimeOffset.UtcNow.AddHours(1)) });
            string endpoint = "http://127.0.0.1:" + server.Endpoint.Port;
            var start = new ProcessStartInfo(Path.GetFullPath(elf)) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add("i-native"); start.ArgumentList.Add("TESTACCESSnative"); start.ArgumentList.Add(endpoint);
            start.Environment.Clear();
            foreach (var (name, value) in new Dictionary<string, string>
            {
                ["LANG"] = "C", ["DOTNET_GCHeapHardLimit"] = "1000000",
                ["DOTNET_GCRegionRange"] = "2000000", ["DOTNET_GCRegionSize"] = "100000",
                ["DOTNET_EnableDiagnostics"] = "0", ["AWS_EC2_METADATA_V1_DISABLED"] = "true",
                ["AWS_EC2_METADATA_SERVICE_ENDPOINT"] = endpoint
            }) start.Environment[name] = value;
            using var process = Process.Start(start) ?? throw new InvalidOperationException("Native oracle did not start.");
            var stdout = process.StandardOutput.ReadToEndAsync(); var stderr = process.StandardError.ReadToEndAsync();
            try { await process.WaitForExitAsync(deadline.Token); }
            finally { if (!process.HasExited) { process.Kill(entireProcessTree: true); await process.WaitForExitAsync(); } }
            if (process.ExitCode != 0 || await stdout != "IMDS PASS i-native\n" || await stderr != "")
                throw new InvalidOperationException("Native oracle: " + await stdout + await stderr);
        }
        Console.WriteLine("PASS 12 native AWS SDK runs against the actual private IMDS listener (explicit loopback endpoint override)");
    }
}
