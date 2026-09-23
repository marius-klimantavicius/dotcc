using System.Net;

if (args.Length != 2) return 80;
try
{
    using var handler = new SocketsHttpHandler { UseProxy = false };
    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(60) };
    client.DefaultRequestVersion = HttpVersion.Version11;
    // Every request establishes a fresh connection, including the parallel group.
    client.DefaultRequestHeaders.ConnectionClose = true;
    async Task Request(int index)
    {
        string path = args[1] + "/" + index;
        string body = await client.GetStringAsync(args[0] + "/" + path);
        if (body != "reply:" + path + "\n") throw new InvalidOperationException("Unexpected HTTP response: " + body);
    }
    await Request(0);
    await Request(1);
    await Task.WhenAll(Enumerable.Range(2, 4).Select(Request));
    Console.WriteLine("HTTP PASS " + args[1] + " 6");
    return 0;
}
catch (Exception error)
{
    Console.Error.WriteLine(error);
    return 1;
}
