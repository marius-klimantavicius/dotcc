using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

if (args.Length != 1 || !int.TryParse(args[0], NumberStyles.None,
        CultureInfo.InvariantCulture, out var port) || port is < 0 or > 65535)
    throw new ArgumentException("Expected one TCP port, 0 through 65535.");

var builder = WebApplication.CreateSlimBuilder(Array.Empty<string>());
builder.Logging.ClearProviders();
builder.WebHost.ConfigureKestrel(options =>
    options.Listen(IPAddress.Loopback, port, listen => listen.Protocols = HttpProtocols.Http1));
await using var app = builder.Build();

app.MapGet("/health", (HttpContext context) => Reply(context, 200, "ok\n"));
app.MapPost("/stop", (HttpContext context) =>
{
    context.Response.OnCompleted(() =>
    {
        app.Lifetime.StopApplication();
        return Task.CompletedTask;
    });
    return Reply(context, 200, "stopped\n");
});
app.MapFallback((HttpContext context) => Reply(context, 404, "not found\n"));

await app.StartAsync();
var addresses = app.Services.GetRequiredService<IServer>().Features
    .Get<IServerAddressesFeature>() ?? throw new InvalidOperationException("No server address feature.");
var address = new Uri(addresses.Addresses.Single());
Console.WriteLine($"READY {address.Port.ToString(CultureInfo.InvariantCulture)}");
await app.WaitForShutdownAsync();
Console.WriteLine("STOPPED");

static Task Reply(HttpContext context, int status, string body)
{
    context.Response.StatusCode = status;
    context.Response.ContentType = "text/plain";
    context.Response.ContentLength = System.Text.Encoding.UTF8.GetByteCount(body);
    return context.Response.WriteAsync(body);
}
