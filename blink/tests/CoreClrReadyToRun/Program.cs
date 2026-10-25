using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

if (args.Length != 1 || !ushort.TryParse(args[0], out ushort port) || port == 0) return 64;
var builder = new WebHostBuilder()
    .UseContentRoot(Directory.GetCurrentDirectory())
    .ConfigureLogging(logging => logging.ClearProviders())
    .UseKestrel(options =>
    {
        options.AddServerHeader = false;
        // Cold framework/JIT work is interpreted too. Keep a finite header
        // bound that accommodates this fixture's measured startup/read cost.
        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(30);
        options.Listen(IPAddress.Loopback, port);
    })
    .Configure(app =>
    {
        var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        app.Run(async context =>
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            if (context.Request.Method == "GET" && context.Request.Path == "/health")
                await context.Response.WriteAsync("CORECLR SERVICE OK\n");
            else if (context.Request.Method == "POST" && context.Request.Path == "/echo")
            {
                using var reader = new StreamReader(context.Request.Body);
                string body = await reader.ReadToEndAsync();
                await context.Response.WriteAsync("CORECLR:" + body + "\n");
            }
            else if (context.Request.Method == "POST" && context.Request.Path == "/stop")
            {
                context.Response.OnCompleted(() => { lifetime.StopApplication(); return Task.CompletedTask; });
                await context.Response.WriteAsync("STOP OK\n");
            }
            else context.Response.StatusCode = 404;
        });
    });
using var host = builder.Build();
await host.StartAsync();
Console.WriteLine("SERVICE READY");
await host.WaitForShutdownAsync();
Console.WriteLine("SERVICE STOPPED");
return 0;
