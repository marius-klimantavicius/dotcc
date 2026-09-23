using System.Text;
using System.Text.Json;
using Managed.Emulation.Host;

if (args.Length == 1)
{
    await NativeOracle.Run(args[0]);
    return;
}

using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var endpoint = new GuestEndpoint(0xa9fea9fe, 80);
var options = new ImdsV2Options { InstanceId = "i-first", RoleName = "test-role", UserData = "hello žemė\n",
    Credentials = new("TESTACCESS", "TESTSECRET", "TESTSESSION", DateTimeOffset.UtcNow.AddHours(1)) };
await using var first = New(options);
await using var second = New(options with { InstanceId = "i-second" });
var issued = await Request(first, "PUT", "/latest/api/token", extra: "x-aws-ec2-metadata-token-ttl-seconds: 60\r\n", fragment: true);
Check(issued.Status == 200 && issued.Head.Contains("ttl-seconds: 60"), "token issuance and TTL response");
string token = issued.Body;
Check((await Request(first, "GET", "/latest/meta-data/instance-id", token)).Body == "i-first", "instance identity");
Check((await Request(first, "GET", "/latest/meta-data/instance-id")).Status == 401, "v2 required");
Check((await Request(second, "GET", "/latest/meta-data/instance-id", token)).Status == 401, "token belongs to its execution");
Check((await Request(first, "GET", "/latest/user-data", token)).Body == options.UserData, "UTF8 user data");
var head = await Request(first, "HEAD", "/latest/user-data", token);
Check(head.Status == 200 && head.Body == "" && head.Head.Contains("Content-Length: " + Encoding.UTF8.GetByteCount(options.UserData)), "HEAD length and no body");
Check((await Request(first, "GET", "/latest/meta-data/iam/security-credentials/", token)).Body == "test-role\n", "role discovery");
using (var identity = JsonDocument.Parse((await Request(first, "GET", "/latest/dynamic/instance-identity/document", token)).Body))
    Check(identity.RootElement.GetProperty("instanceId").GetString() == "i-first", "identity document");
using (var credentials = JsonDocument.Parse((await Request(first, "GET", "/latest/meta-data/iam/security-credentials/test-role", token)).Body))
    Check(credentials.RootElement.GetProperty("AccessKeyId").GetString() == "TESTACCESS", "explicit credentials");
using (var info = JsonDocument.Parse((await Request(first, "GET", "/latest/meta-data/iam/info", token)).Body))
    Check(info.RootElement.GetProperty("InstanceProfileId").GetString() == options.InstanceProfileId, "instance profile");
await Task.WhenAll(Enumerable.Range(0, 12).Select(async _ =>
    Check((await Request(first, "GET", "/latest/meta-data/placement/region", token)).Body == options.Region, "concurrent token reuse")));
string shortToken = (await Request(first, "PUT", "/latest/api/token", extra: "X-aws-ec2-metadata-token-ttl-seconds: 1\r\n")).Body;
await Task.Delay(1100, deadline.Token);
Check((await Request(first, "GET", "/latest/meta-data/instance-id", shortToken)).Status == 401, "normal token expiration");
int fd = Ok(first.Socket());
Check((await first.ConnectAsync(fd, new(0x7f000001, 80), deadline.Token)).Error == GuestError.Access, "metadata grants no other destination");
Ok(first.Close(fd));
// Normal stop owns accepted idle connections as well as active guest sockets.
var stopping = New(options);
fd = Ok(stopping.Socket());
Ok(await stopping.ConnectAsync(fd, endpoint, deadline.Token));
await stopping.DisposeAsync().AsTask().WaitAsync(deadline.Token);
await using var restarted = New(options);
Check((await Request(restarted, "GET", "/latest/meta-data/instance-id", token)).Status == 401, "fresh execution does not retain tokens");
await using var isolated = new InstanceIo(new Dictionary<string, ReadOnlyMemory<byte>>(), networkPolicy: new GuestNetworkPolicy());
Check(isolated.Socket().Error == GuestError.Access, "disabled by default");
Console.WriteLine("PASS IMDSv2 guest routing, token lifecycle/isolation/reuse, metadata, credentials, HEAD, concurrent requests and disposal");

InstanceIo New(ImdsV2Options config) => new(new Dictionary<string, ReadOnlyMemory<byte>>(), networkPolicy: new GuestNetworkPolicy(metadata: config));
async Task<(int Status, string Head, string Body)> Request(InstanceIo io, string method, string path, string? token = null, string extra = "", bool fragment = false)
{
    int socket = Ok(io.Socket());
    try
    {
        Ok(await io.ConnectAsync(socket, endpoint, deadline.Token));
        Check(Ok(io.PeerEndpoint(socket)) == endpoint, "guest peer address retained");
        byte[] request = Encoding.ASCII.GetBytes($"{method} {path} HTTP/1.1\r\nHost: 169.254.169.254\r\n{extra}" +
            (token == null ? "" : $"X-aws-ec2-metadata-token: {token}\r\n") + "Connection: close\r\n\r\n");
        for (int sent = 0; sent < request.Length;)
            sent += Ok(await io.SendAsync(socket, request.AsMemory(sent, fragment ? 1 : request.Length - sent), deadline.Token));
        using var bytes = new MemoryStream(); byte[] buffer = new byte[1024];
        while (true)
        {
            int read = Ok(await io.ReceiveAsync(socket, buffer, deadline.Token));
            if (read == 0) break;
            bytes.Write(buffer, 0, read);
        }
        string response = Encoding.UTF8.GetString(bytes.ToArray());
        int separator = response.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        Check(separator >= 0, "complete HTTP response");
        return (int.Parse(response.Split(' ')[1]), response[..separator], response[(separator + 4)..]);
    }
    finally { Ok(io.Close(socket)); }
}
static T Ok<T>(HostResult<T> result) => result.Succeeded ? result.Value : throw new InvalidOperationException(result.Error.ToString());
static void Check(bool success, string message) { if (!success) throw new InvalidOperationException(message); }
