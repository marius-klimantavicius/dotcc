# Simulated IMDSv2

Enable metadata explicitly on the machine. In-process execution is the default;
the same options work with `ExecutionMode.SeparateProcess`.

```csharp
using Managed.Emulation;
using Managed.Emulation.Host;

await using var machine = new BlinkMachine(new MachineOptions
{
    MemoryLimit = 128L << 20,
    Metadata = new ImdsV2Options
    {
        InstanceId = "i-0123456789abcdef0",
        AccountId = "123456789012",
        Region = "eu-west-1",
        AvailabilityZone = "eu-west-1a",
        PrivateAddress = "10.0.0.42",
        UserData = "hello from the host",
        RoleName = "my-service-role",
        Credentials = new ImdsRoleCredentials(
            "TESTACCESS", "TESTSECRET", "TESTSESSION",
            DateTimeOffset.UtcNow.AddHours(1))
    }
});
machine.MountDirectory("/work", "./work", MountAccess.ReadOnly);
await using var run = await machine.StartAsync(new ExecutionOptions
{
    Executable = "/work/my_app",
    WorkingDirectory = "/work",
    Console = ConsoleOptions.AttachCurrent()
});
var result = await run.WaitAsync();
```

The guest uses `http://169.254.169.254`, including unmodified AWS SDK instance
profile credential providers. The endpoint works with the default isolated
network policy; it grants access only to this simulator. Other destinations
still require explicit outbound grants. When metadata is enabled, its route
takes precedence over an outbound grant for the same address and port.
When metadata is absent, normal network policy applies.

Supported requests:

| Request | Result |
| --- | --- |
| `PUT /latest/api/token` | Opaque token; requires `X-aws-ec2-metadata-token-ttl-seconds` in 1–21600. Rejects `X-Forwarded-For`. |
| `GET`/`HEAD /latest/meta-data/…` | Basic identity fields, directory listings, `placement/region`, `placement/availability-zone`. |
| `GET`/`HEAD /latest/dynamic/instance-identity/document` | Unsigned identity JSON. |
| `GET`/`HEAD /latest/user-data` | Configured UTF-8 user data, or 404. |
| `GET`/`HEAD /latest/meta-data/iam/info` | Simulated instance profile information when a role is configured. |
| `GET`/`HEAD /latest/meta-data/iam/security-credentials/` | Configured role name. |
| `GET`/`HEAD /latest/meta-data/iam/security-credentials/{role}` | Explicit credentials and their expiration, or 404 if absent/expired. |

All metadata reads require `X-aws-ec2-metadata-token`; missing, expired or
another execution's tokens receive 401. IMDSv1 is disabled. Tokens can be reused
across connections until their requested lifetime ends. Each execution has a
fresh token key; restarting the same machine invalidates previous tokens.

Configuration is copied at machine creation and serialized to workers.
Credentials are supplied by the caller: there is no host credential discovery,
STS integration or automatic rotation. SDK refresh returns the configured
record while it remains valid. The SDK credential expiration and IMDS session
token lifetime are independent. Set expiration far enough ahead for the SDK's
refresh window. Treat configured credential values like any other secret passed
to a machine. Do not use the fake values above for actual AWS access.

The implementation uses a private BCL TCP listener on an ephemeral loopback
port, owned and drained by the execution. The guest sees its original metadata
peer address. No ASP.NET dependency, host link-local alias or direct P/Invoke is
needed. The HTTP subset accepts bodyless HTTP/1.0 and HTTP/1.1 requests and closes
each connection after its response; clients reconnect normally. Limits are 32
active connections, 16 KiB of headers, 64 headers and a 30-second request timeout.
This loopback listener is not an isolation boundary against other host processes.

IPv6 IMDS, signed identity documents, arbitrary metadata categories, live
credential replacement and complete EC2 behavior are not implemented. Linux x64
is the tested host; BCL portability does not constitute Windows qualification.
See the [executable example and test commands](../../tests/ImdsMachine/README.md).
