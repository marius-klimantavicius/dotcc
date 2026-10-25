using System.Net;

if (args.Length != 1 || !ushort.TryParse(args[0], out ushort port) || port == 0) return 64;
var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = [], ContentRootPath = Directory.GetCurrentDirectory() });
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Listen(IPAddress.Loopback, port);
});
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
await app.StartAsync();
Console.WriteLine("SERVICE READY");
await app.WaitForShutdownAsync();
Console.WriteLine("SERVICE STOPPED");
return 0;
