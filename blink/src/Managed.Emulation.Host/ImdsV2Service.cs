using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Managed.Emulation.Host;

internal sealed record ImdsResponse(int Status, byte[] Body, string ContentType = "text/plain", int? TokenTtl = null);

/// <summary>Transport-independent, execution-private IMDSv2 protocol and metadata.</summary>
internal sealed class ImdsV2Service(ImdsV2Options options)
{
    private readonly byte[] key = RandomNumberGenerator.GetBytes(32);
    private readonly string started = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    internal ImdsResponse Handle(string method, string path, IReadOnlyDictionary<string, string> headers)
    {
        if (path == "/latest/api/token")
        {
            if (method != "PUT") return Text(405, "Method Not Allowed");
            if (headers.ContainsKey("X-Forwarded-For")) return Text(403, "Forbidden");
            if (!headers.TryGetValue("X-aws-ec2-metadata-token-ttl-seconds", out string? value) ||
                !int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int ttl) || ttl is < 1 or > 21600)
                return Text(400, "Invalid token lifetime");
            byte[] token = new byte[56];
            System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(token,
                checked(Stopwatch.GetTimestamp() + ttl * Stopwatch.Frequency));
            RandomNumberGenerator.Fill(token.AsSpan(8, 16));
            HMACSHA256.HashData(key, token.AsSpan(0, 24), token.AsSpan(24));
            return new(200, Encoding.ASCII.GetBytes(Convert.ToBase64String(token)), TokenTtl: ttl);
        }
        if (method is not ("GET" or "HEAD")) return Text(405, "Method Not Allowed");
        if (!headers.TryGetValue("X-aws-ec2-metadata-token", out string? supplied) || !ValidToken(supplied))
            return Text(401, "Unauthorized");

        if (path == "/latest/user-data") return options.UserData is { } user ? Text(200, user) : Text(404, "Not Found");
        if (path == "/latest/dynamic/instance-identity/document")
            return Json(new Dictionary<string, string>
            {
                ["accountId"] = options.AccountId, ["architecture"] = "x86_64", ["availabilityZone"] = options.AvailabilityZone,
                ["imageId"] = options.ImageId, ["instanceId"] = options.InstanceId, ["instanceType"] = options.InstanceType,
                ["pendingTime"] = started, ["privateIp"] = options.PrivateAddress, ["region"] = options.Region, ["version"] = "2017-09-30"
            });
        if (path == "/latest/meta-data/iam/info" && options.RoleName is { } role)
            return Json(new Dictionary<string, string> { ["Code"] = "Success", ["LastUpdated"] = started,
                ["InstanceProfileArn"] = $"arn:aws:iam::{options.AccountId}:instance-profile/{role}",
                ["InstanceProfileId"] = options.InstanceProfileId });
        if (options.RoleName != null && path == "/latest/meta-data/iam/security-credentials/" + options.RoleName)
        {
            if (options.Credentials is not { } credentials || credentials.Expiration <= DateTimeOffset.UtcNow)
                return Text(404, "No current credentials");
            return Json(new Dictionary<string, string> { ["Code"] = "Success", ["LastUpdated"] = started, ["Type"] = "AWS-HMAC",
                ["AccessKeyId"] = credentials.AccessKeyId, ["SecretAccessKey"] = credentials.SecretAccessKey,
                ["Token"] = credentials.Token, ["Expiration"] = credentials.Expiration.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture) });
        }
        var values = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["instance-id"] = options.InstanceId, ["ami-id"] = options.ImageId, ["instance-type"] = options.InstanceType,
            ["hostname"] = options.Hostname, ["local-hostname"] = options.Hostname, ["local-ipv4"] = options.PrivateAddress,
            ["placement/region"] = options.Region, ["placement/availability-zone"] = options.AvailabilityZone
        };
        if (options.RoleName != null) values["iam/security-credentials/" + options.RoleName] = "";
        if (options.RoleName != null) values["iam/info"] = "";
        if (path == "/") return Text(200, "latest\n");
        if (path is "/latest" or "/latest/") return Text(200, "dynamic/\nmeta-data/\n" + (options.UserData != null ? "user-data\n" : ""));
        if (path == "/latest/dynamic/") return Text(200, "instance-identity/\n");
        if (path == "/latest/dynamic/instance-identity/") return Text(200, "document\n");
        const string prefix = "/latest/meta-data/";
        if (path.StartsWith(prefix, StringComparison.Ordinal))
        {
            string relative = path[prefix.Length..];
            if (values.TryGetValue(relative, out string? value)) return Text(200, value);
            // Listings accept both directory spellings, as used by AWS SDKs.
            string directory = relative.Length == 0 || relative.EndsWith('/') ? relative : relative + "/";
            var children = values.Keys.Where(p => p.StartsWith(directory, StringComparison.Ordinal))
                .Select(p => p[directory.Length..]).Select(p => p.Contains('/') ? p[..(p.IndexOf('/') + 1)] : p)
                .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
            if (children.Length > 0) return Text(200, string.Join('\n', children) + "\n");
        }
        return Text(404, "Not Found");
    }

    private bool ValidToken(string value)
    {
        Span<byte> bytes = stackalloc byte[56];
        if (!Convert.TryFromBase64String(value, bytes, out int length) || length != bytes.Length) return false;
        Span<byte> signature = stackalloc byte[32];
        HMACSHA256.HashData(key, bytes[..24], signature);
        return CryptographicOperations.FixedTimeEquals(signature, bytes[24..]) &&
            System.Buffers.Binary.BinaryPrimitives.ReadInt64LittleEndian(bytes) > Stopwatch.GetTimestamp();
    }
    private static ImdsResponse Text(int status, string text) => new(status, Encoding.UTF8.GetBytes(text));
    private static ImdsResponse Json(Dictionary<string, string> fields)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (var pair in fields) writer.WriteString(pair.Key, pair.Value);
            writer.WriteEndObject();
        }
        return new(200, stream.ToArray(), "application/json");
    }
}
