using System.Net;
using System.Text;
using System.Runtime.InteropServices;

try
{
    if (args.Length != 1 || !ushort.TryParse(args[0], out ushort port) || port == 0) return 64;
    NativeDiagnostic.Report("DIAGNOSTIC: create builder\n");
    var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], ContentRootPath = Directory.GetCurrentDirectory() });
    builder.Logging.ClearProviders();
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.AddServerHeader = false;
        options.Listen(IPAddress.Loopback, port);
    });
    NativeDiagnostic.Report("DIAGNOSTIC: build app\n");
    await using var app = builder.Build();
    app.MapGet("/health", () => Results.Text("CORECLR SERVICE OK\n"));
    app.MapPost("/echo", async (HttpContext context) =>
    {
        using var reader = new StreamReader(context.Request.Body);
        string body = await reader.ReadToEndAsync();
        return Results.Text("CORECLR:" + body + "\n");
    });
    app.MapPost("/stop", (HttpContext context) =>
    {
        context.Response.OnCompleted(() => { app.Lifetime.StopApplication(); return Task.CompletedTask; });
        return Results.Text("STOP OK\n");
    });
    NativeDiagnostic.Report("DIAGNOSTIC: start app\n");
    try { await app.StartAsync(); }
    catch (Exception error)
    {
        NativeDiagnostic.Report("DIAGNOSTIC START FAILURE: " + error.GetType().FullName + " hresult=" + error.HResult.ToString("x8") + " message=" + error.Message + "\n");
        throw;
    }
    NativeDiagnostic.Report("DIAGNOSTIC: app started\n");
    Console.WriteLine("SERVICE READY");
    await app.WaitForShutdownAsync();
    Console.WriteLine("SERVICE STOPPED");
    return 0;
}
catch (Exception error)
{
    try { NativeDiagnostic.Report("DIAGNOSTIC EXCEPTION: " + error.GetType().FullName + " hresult=" + error.HResult.ToString("x8") + " message=" + error.Message + "\n"); }
    catch { NativeDiagnostic.Report("DIAGNOSTIC EXCEPTION: detail unavailable\n"); }
    return 70;
}

internal static class NativeDiagnostic
{
    [DllImport("libc.so.6", EntryPoint = "write", SetLastError = true)]
    private static extern nint Write(int descriptor, byte[] bytes, nuint count);
    internal static void Report(string text)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(text);
        if (Write(2, bytes, (nuint)bytes.Length) != bytes.Length)
            throw new IOException("Diagnostic write failed");
    }
}
