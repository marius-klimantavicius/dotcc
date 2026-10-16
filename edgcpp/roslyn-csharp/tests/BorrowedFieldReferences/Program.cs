using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;
using Member = Translated.Member;
using Owner = Translated.Owner;

if (args.Length != 2) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON");
RuntimeContract.RequireLayout(8, true);
var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations");
var provenance = JsonNode.Parse(File.ReadAllText(args[1])) ?? throw new InvalidOperationException("Missing capture provenance");
var sourceHash = provenance["source"]?["sha256"]?.GetValue<string>() ?? throw new InvalidOperationException("Missing source hash");
var inputHash = provenance["input"]?["sha256"]?.GetValue<string>() ?? throw new InvalidOperationException("Missing input hash");
var nativeHash = Hash(args[0]);
var provenanceHash = Hash(args[1]);
if (provenance["nativeObservations"]?["sha256"]?.GetValue<string>() != nativeHash)
    throw new InvalidOperationException("The native observations do not match the supplied capture provenance");
string variant;
if (sourceHash == "231d266a0bcfbb7f9bc1bb6d1897e5e4b68d44792ec0d27071b68d5d7c7f3110" &&
    inputHash == "2b5f1c47b0951f6244fd46757a10a6624819e67d7471060f4f00e75713a8be78" &&
    provenanceHash == "3303ed839b7cfc172b503af832a996efa2d6e13e3650db61abdc5775c96861fb")
    variant = "original";
else if (sourceHash == "563d9142640c5df9639d5b63904c79eda99db79fd8a9eac2d0e5f4d7a00b3a7a" &&
         inputHash == "ee91bb52a2a545678a7fde7140ced9d9463eccd160a9bf37de5bef225d9b6d8c" &&
         provenanceHash == "c72d27c0bb3e62e72a405db213025423ee65240bb392012930f3e9fcea5b8958")
    variant = "mutable-field";
else throw new InvalidOperationException("The provenance is not one of the two frozen borrowed-field-reference captures");

var runs = new JsonArray();
for (var run = 0; run < 2; ++run)
{
    var result = Application.fixture();
    var count = Application.lifetime_count();
    var overflow = Application.lifetime_overflow();
    var protocolLive = Application.lifetime_live();
    if (count is < 0 or > 64 || overflow != 0 || protocolLive != 0 || Application.lifetime_value(8) != 0)
        throw new InvalidOperationException("Borrowed-reference event bounds, overflow or zero-live protocol check failed; the records have no counted resources");
    var values = new JsonArray();
    for (var index = 0; index < 9; ++index)
        values.Add((JsonNode?)JsonValue.Create(Application.lifetime_value(index)));
    var events = new JsonArray();
    for (var index = 0; index < count; ++index)
        events.Add((JsonNode?)JsonValue.Create(Application.lifetime_event(index)));
    runs.Add((JsonNode)new JsonObject
    {
        ["result"] = result, ["overflow"] = overflow, ["live"] = protocolLive,
        ["values"] = values, ["events"] = events
    });
}
if (!JsonNode.DeepEquals(runs[0], runs[1]))
    throw new InvalidOperationException("Repeated generated borrowed-field-reference observations differ");
var observations = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observations, native))
    throw new InvalidOperationException("Generated borrowed-field-reference observations differ from the separate native oracle");
Console.WriteLine(observations.ToJsonString());
Console.WriteLine(new JsonObject { ["borrowedFieldReferencePhysical"] = BorrowedFieldReferencePhysical.Measure() }.ToJsonString());
Console.WriteLine(new JsonObject
{
    ["suppliedCaptureProvenance"] = new JsonObject
    {
        ["variant"] = variant, ["sourceSha256"] = sourceHash, ["inputSha256"] = inputHash,
        ["captureProvenanceSha256"] = provenanceHash, ["nativeSha256"] = nativeHash
    }
}.ToJsonString());

static string Hash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexStringLower(SHA256.HashData(stream));
}

internal static unsafe class BorrowedFieldReferencePhysical
{
    [StructLayout(LayoutKind.Sequential)]
    private struct MemberAlignmentProbe
    {
        public byte Prefix;
        public Member Value;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct OwnerAlignmentProbe
    {
        public byte Prefix;
        public Owner Value;
    }

    internal static JsonObject Measure()
    {
        Member member = default;
        Owner owner = default;
        var firstOffset = (byte*)&member.first - (byte*)&member;
        var secondOffset = (byte*)&member.second - (byte*)&member;
        var canaryOffset = (byte*)&owner.canary - (byte*)&owner;
        var payloadOffset = (byte*)&owner.payload - (byte*)&owner;
        var memberPrefix = new MemberAlignmentProbe { Prefix = 0, Value = default };
        var ownerPrefix = new OwnerAlignmentProbe { Prefix = 0, Value = default };
        var memberPrefixOffset = (byte*)&memberPrefix.Value - (byte*)&memberPrefix;
        var ownerPrefixOffset = (byte*)&ownerPrefix.Value - (byte*)&ownerPrefix;
        Member* members = stackalloc Member[2];
        Owner* owners = stackalloc Owner[2];
        var memberStride = (byte*)&members[1] - (byte*)&members[0];
        var ownerStride = (byte*)&owners[1] - (byte*)&owners[0];
        var memberAddressesAligned = ((nuint)(&members[0])) % 4 == 0 && ((nuint)(&members[1])) % 4 == 0;
        var ownerAddressesAligned = ((nuint)(&owners[0])) % 8 == 0 && ((nuint)(&owners[1])) % 8 == 0;
        if (sizeof(Member) != 8 || firstOffset != 0 || secondOffset != 4 || memberPrefixOffset != 4 || memberStride != 8 || !memberAddressesAligned ||
            sizeof(Owner) != 16 || canaryOffset != 0 || payloadOffset != 8 || ownerPrefixOffset != 8 || ownerStride != 16 || !ownerAddressesAligned)
            throw new InvalidOperationException("Published Member/Owner layout, sequential-prefix alignment or actual array storage differs from captured 8/4 and 16/8 representations");

        const long firstCanary = -0x6123456789ABCDE0L;
        const long secondCanary = 0x5123456789ABCDE0L;
        owners[0].canary = firstCanary;
        owners[0].payload.first = 5;
        owners[0].payload.second = 7;
        owners[1].canary = secondCanary;
        owners[1].payload.first = 101;
        owners[1].payload.second = 103;
        Member* firstBorrow = Application.view(&owners[0]);
        Member* secondBorrow = Application.view(&owners[1]);
        var exactFieldPointers = firstBorrow == &owners[0].payload && secondBorrow == &owners[1].payload;
        var distinctPointers = firstBorrow != secondBorrow;
        var firstReturnedOffset = (byte*)firstBorrow - (byte*)&owners[0];
        var secondReturnedOffset = (byte*)secondBorrow - (byte*)&owners[1];
        if (!exactFieldPointers || !distinctPointers || firstReturnedOffset != 8 || secondReturnedOffset != 8)
            throw new InvalidOperationException("The actual translated getter did not return the distinct corresponding payload fields at byte offset 8");
        if (firstBorrow->first != 5 || firstBorrow->second != 7 || secondBorrow->first != 101 || secondBorrow->second != 103)
            throw new InvalidOperationException("Returned field pointers did not expose each owner's initial values");

        owners[0].payload.first = 11;
        owners[0].payload.second = 13;
        owners[1].payload.first = 107;
        owners[1].payload.second = 109;
        var ownerMutationVisible = firstBorrow->first == 11 && firstBorrow->second == 13 && secondBorrow->first == 107 && secondBorrow->second == 109;
        var canariesPreserved = owners[0].canary == firstCanary && owners[1].canary == secondCanary;
        if (!ownerMutationVisible || !canariesPreserved)
            throw new InvalidOperationException("Stored borrowed pointers did not reflect both owner mutations or full-width canaries changed");

        return new JsonObject
        {
            ["memberSize"] = sizeof(Member), ["firstOffset"] = firstOffset, ["secondOffset"] = secondOffset,
            ["memberSequentialPrefixOffset"] = memberPrefixOffset, ["memberArrayStride"] = memberStride,
            ["ownerSize"] = sizeof(Owner), ["canaryOffset"] = canaryOffset, ["payloadOffset"] = payloadOffset,
            ["ownerSequentialPrefixOffset"] = ownerPrefixOffset, ["ownerArrayStride"] = ownerStride,
            ["firstReturnedOffset"] = firstReturnedOffset, ["secondReturnedOffset"] = secondReturnedOffset,
            ["memberAddressesAligned"] = memberAddressesAligned, ["ownerAddressesAligned"] = ownerAddressesAligned,
            ["exactFieldPointers"] = exactFieldPointers, ["returnedPointersDistinct"] = distinctPointers,
            ["ownerMutationVisible"] = ownerMutationVisible, ["canariesPreserved"] = canariesPreserved
        };
    }
}
