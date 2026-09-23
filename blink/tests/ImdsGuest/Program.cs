using System.Net;
using System.Text.Json;
using Amazon.Runtime;

if (args.Length is not (2 or 3)) return 80;
try
{
    using var handler = new SocketsHttpHandler { UseProxy = false };
    using var client = new HttpClient(handler) { BaseAddress = new Uri(args.Length == 3 ? args[2] : "http://169.254.169.254"), Timeout = TimeSpan.FromSeconds(60) };
    using var put = new HttpRequestMessage(HttpMethod.Put, "/latest/api/token");
    put.Headers.Add("X-aws-ec2-metadata-token-ttl-seconds", "300");
    using var issued = await client.SendAsync(put);
    issued.EnsureSuccessStatusCode();
    string token = await issued.Content.ReadAsStringAsync();
    client.DefaultRequestHeaders.Add("X-aws-ec2-metadata-token", token);
    for (int i = 0; i < 3; ++i)
        Check(await client.GetStringAsync("/latest/meta-data/instance-id") == args[0], "per-machine identity and token reuse");
    using var document = JsonDocument.Parse(await client.GetStringAsync("/latest/dynamic/instance-identity/document"));
    Check(document.RootElement.GetProperty("instanceId").GetString() == args[0], "identity document");
    // Use the real SDK's role discovery and IMDSv2 credential provider. No SDK
    // metadata endpoint override or environment credentials are supplied.
    using var provider = new InstanceProfileAWSCredentials();
    CheckCredentials(await provider.GetCredentialsAsync());
    CheckCredentials(await provider.GetCredentialsAsync());
    provider.ClearCredentials();
    CheckCredentials(await provider.GetCredentialsAsync());
    Console.WriteLine("IMDS PASS " + args[0]);
    return 0;
    void CheckCredentials(ImmutableCredentials credentials)
    {
        Check(credentials.AccessKey == args[1] && credentials.SecretKey == "TESTSECRET" && credentials.Token == "TESTSESSION", "AWS SDK credentials and refresh");
    }
}
catch (Exception error) { Console.Error.WriteLine(error); return 1; }
static void Check(bool success, string message) { if (!success) throw new InvalidOperationException(message); }
