using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using RoslynObj.CSharp.Runtime;
using Application = Translated.Program;
using Owner = Translated.Owner;
using Wrapper = Translated.Wrapper;
using DerivedStorage = Translated.DerivedStorage;
using Storage = Translated.Storage;
using RefCountCarrier = Translated.RefCountCarrier;

if (args.Length != 2) throw new InvalidOperationException("Checks NATIVE_OBSERVATIONS_JSON CAPTURE_PROVENANCE_JSON");
RuntimeContract.RequireLayout(8, true);
var native = JsonNode.Parse(File.ReadAllText(args[0])) ?? throw new InvalidOperationException("Missing native observations");
var provenance = JsonNode.Parse(File.ReadAllText(args[1])) ?? throw new InvalidOperationException("Missing capture provenance");
var sourceHash = provenance["source"]?["sha256"]?.GetValue<string>() ?? throw new InvalidOperationException("Missing source hash");
var inputHash = provenance["input"]?["sha256"]?.GetValue<string>() ?? throw new InvalidOperationException("Missing input hash");
var nativeHash = Hash(args[0]);
var provenanceHash = Hash(args[1]);
if (sourceHash != "c2f5bf85e5ca97c5e4c95b8464712927b7f02aba25f26eca6bf72c18ab42b3da" ||
    inputHash != "07c3198fd8d09f6dda99baeebdcf9cff98ef814bb4387ae5cff9be0b56c6bfed" ||
    provenanceHash != "1db6ad5547497a76e78a0dd8214f78fe326f670cd3490f3c7b76fbb591cbb6ea")
    throw new InvalidOperationException("The supplied provenance is not the frozen positive Boolean call-chain capture");
if (provenance["nativeObservations"]?["sha256"]?.GetValue<string>() != nativeHash)
    throw new InvalidOperationException("Native observations differ from the supplied capture provenance");

var runs = new JsonArray();
for (var run = 0; run < 2; ++run)
{
    var result = Application.fixture();
    var count = Application.lifetime_count();
    var overflow = Application.lifetime_overflow();
    var protocolLive = Application.lifetime_live();
    if (count is < 0 or > 64 || overflow != 0 || protocolLive != 0 || Application.lifetime_value(18) != 0)
        throw new InvalidOperationException("Boolean call-chain event bounds, overflow or zero-live protocol failed; the inert carriers have no counted resources");
    var values = new JsonArray();
    for (var index = 0; index < 19; ++index)
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
    throw new InvalidOperationException("Repeated generated Boolean call-chain observations differ");
var observations = new JsonObject { ["runs"] = runs };
if (!JsonNode.DeepEquals(observations, native))
    throw new InvalidOperationException("Generated Boolean call-chain observations differ from the separate native oracle");
Console.WriteLine(observations.ToJsonString());
Console.WriteLine(new JsonObject { ["booleanCallChainPhysical"] = BooleanCallChainPhysical.Measure() }.ToJsonString());
Console.WriteLine(new JsonObject
{
    ["suppliedCaptureProvenance"] = new JsonObject
    {
        ["sourceSha256"] = sourceHash, ["inputSha256"] = inputHash,
        ["captureProvenanceSha256"] = provenanceHash, ["nativeSha256"] = nativeHash
    }
}.ToJsonString());

static string Hash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexStringLower(SHA256.HashData(stream));
}

internal static unsafe class BooleanCallChainPhysical
{
    [StructLayout(LayoutKind.Sequential)] private struct OwnerPrefix { public byte Prefix; public Owner Value; }
    [StructLayout(LayoutKind.Sequential)] private struct WrapperPrefix { public byte Prefix; public Wrapper Value; }
    [StructLayout(LayoutKind.Sequential)] private struct DerivedStoragePrefix { public byte Prefix; public DerivedStorage Value; }
    [StructLayout(LayoutKind.Sequential)] private struct StoragePrefix { public byte Prefix; public Storage Value; }
    [StructLayout(LayoutKind.Sequential)] private struct RefCountCarrierPrefix { public byte Prefix; public RefCountCarrier Value; }

    private static bool Aligned8(void* address) => ((nuint)address) % 8 == 0;

    internal static JsonObject Measure()
    {
        Owner owner = default;
        Wrapper wrapper = default;
        DerivedStorage derived = default;
        Storage storage = default;
        RefCountCarrier refcount = default;
        var canaryOffset = (byte*)&owner.canary - (byte*)&owner;
        var payloadOffset = (byte*)&owner.payload - (byte*)&owner;
        var internalOffset = (byte*)&wrapper.@internal - (byte*)&wrapper;
        var generatedBaseOffset = (byte*)&derived.__b_7Storage - (byte*)&derived;
        var pointerOffset = (byte*)&storage.pointer - (byte*)&storage;
        var refcountOffset = (byte*)&storage.refcount - (byte*)&storage;
        var controlOffset = (byte*)&refcount.control - (byte*)&refcount;
        var ownerPrefix = new OwnerPrefix { Prefix = 0, Value = default };
        var wrapperPrefix = new WrapperPrefix { Prefix = 0, Value = default };
        var derivedPrefix = new DerivedStoragePrefix { Prefix = 0, Value = default };
        var storagePrefix = new StoragePrefix { Prefix = 0, Value = default };
        var refcountPrefix = new RefCountCarrierPrefix { Prefix = 0, Value = default };
        var ownerPrefixOffset = (byte*)&ownerPrefix.Value - (byte*)&ownerPrefix;
        var wrapperPrefixOffset = (byte*)&wrapperPrefix.Value - (byte*)&wrapperPrefix;
        var derivedPrefixOffset = (byte*)&derivedPrefix.Value - (byte*)&derivedPrefix;
        var storagePrefixOffset = (byte*)&storagePrefix.Value - (byte*)&storagePrefix;
        var refcountPrefixOffset = (byte*)&refcountPrefix.Value - (byte*)&refcountPrefix;
        Owner* owners = stackalloc Owner[2];
        Wrapper* wrappers = stackalloc Wrapper[2];
        DerivedStorage* derivedItems = stackalloc DerivedStorage[2];
        Storage* storageItems = stackalloc Storage[2];
        RefCountCarrier* refcountItems = stackalloc RefCountCarrier[2];
        var ownerStride = (byte*)(owners + 1) - (byte*)owners;
        var wrapperStride = (byte*)(wrappers + 1) - (byte*)wrappers;
        var derivedStride = (byte*)(derivedItems + 1) - (byte*)derivedItems;
        var storageStride = (byte*)(storageItems + 1) - (byte*)storageItems;
        var refcountStride = (byte*)(refcountItems + 1) - (byte*)refcountItems;
        var alignedStorage = Aligned8(owners) && Aligned8(owners + 1) && Aligned8(wrappers) && Aligned8(wrappers + 1) &&
            Aligned8(derivedItems) && Aligned8(derivedItems + 1) && Aligned8(storageItems) && Aligned8(storageItems + 1) &&
            Aligned8(refcountItems) && Aligned8(refcountItems + 1);
        if (sizeof(Owner) != 24 || sizeof(Wrapper) != 16 || sizeof(DerivedStorage) != 16 || sizeof(Storage) != 16 || sizeof(RefCountCarrier) != 8 ||
            canaryOffset != 0 || payloadOffset != 8 || internalOffset != 0 || generatedBaseOffset != 0 || pointerOffset != 0 || refcountOffset != 8 || controlOffset != 0 ||
            ownerPrefixOffset != 8 || wrapperPrefixOffset != 8 || derivedPrefixOffset != 8 || storagePrefixOffset != 8 || refcountPrefixOffset != 8 ||
            ownerStride != 24 || wrapperStride != 16 || derivedStride != 16 || storageStride != 16 || refcountStride != 8 || !alignedStorage)
            throw new InvalidOperationException("Published carrier sizes, explicit field/base offsets, sequential-prefix alignment or array strides differ from the captured storage");

        const long firstCanary = -0x6123456789ABCDE0L;
        const long secondCanary = 0x5123456789ABCDE0L;
        owners[0].canary = firstCanary;
        owners[1].canary = secondCanary;
        void* firstSentinel = &owners[0].canary;
        void* secondSentinel = &owners[1].canary;
        owners[0].payload.@internal.__b_7Storage.pointer = null;
        owners[0].payload.@internal.__b_7Storage.refcount.control = secondSentinel;
        owners[1].payload.@internal.__b_7Storage.pointer = secondSentinel;
        owners[1].payload.@internal.__b_7Storage.refcount.control = firstSentinel;
        Wrapper* firstBorrow = Application.view(&owners[0]);
        Wrapper* secondBorrow = Application.view(&owners[1]);
        var exactFieldPointers = firstBorrow == &owners[0].payload && secondBorrow == &owners[1].payload;
        var distinctPointers = firstBorrow != secondBorrow;
        var firstReturnedOffset = (byte*)firstBorrow - (byte*)&owners[0];
        var secondReturnedOffset = (byte*)secondBorrow - (byte*)&owners[1];
        if (!exactFieldPointers || !distinctPointers || firstReturnedOffset != 8 || secondReturnedOffset != 8)
            throw new InvalidOperationException("The actual getter did not return each distinct owner's payload at offset 8");
        Storage* firstStorage = &(firstBorrow->@internal.__b_7Storage);
        Storage* secondStorage = &(secondBorrow->@internal.__b_7Storage);
        var aliasAndBaseAddressIdentity = firstStorage == &owners[0].payload.@internal.__b_7Storage &&
            secondStorage == &owners[1].payload.@internal.__b_7Storage &&
            (void*)firstStorage == (void*)(&(firstBorrow->@internal)) &&
            (void*)secondStorage == (void*)(&(secondBorrow->@internal));
        var alignedScalarAddresses = Aligned8(firstSentinel) && Aligned8(secondSentinel) && firstSentinel != secondSentinel;
        if (!aliasAndBaseAddressIdentity || !alignedScalarAddresses)
            throw new InvalidOperationException("Actual member/base storage identity or scalar-address alignment differs from the captured address-only view");

        var wrapperNull = Application.operator_bool(firstBorrow);
        var storageNull = Application.operator_bool__2(firstStorage);
        var wrapperOtherInitial = Application.operator_bool(secondBorrow);
        var storageOtherInitial = Application.operator_bool__2(secondStorage);
        owners[0].payload.@internal.__b_7Storage.pointer = firstSentinel;
        var wrapperNonNull = Application.operator_bool(firstBorrow);
        var storageNonNull = Application.operator_bool__2(firstStorage);
        var nonNullAddressBitsPreserved = firstStorage->pointer == firstSentinel && secondStorage->pointer == secondSentinel;
        owners[0].payload.@internal.__b_7Storage.pointer = null;
        var wrapperCleared = Application.operator_bool(firstBorrow);
        var storageCleared = Application.operator_bool__2(firstStorage);
        var wrapperOtherFinal = Application.operator_bool(secondBorrow);
        var storageOtherFinal = Application.operator_bool__2(secondStorage);
        var scalarAddressBitsPreserved = nonNullAddressBitsPreserved && firstStorage->pointer == null && secondStorage->pointer == secondSentinel;
        var cookiesPreserved = firstStorage->refcount.control == secondSentinel && secondStorage->refcount.control == firstSentinel;
        var fullWidthCanariesPreserved = owners[0].canary == firstCanary && owners[1].canary == secondCanary;
        var borrowedViewsStable = Application.view(&owners[0]) == firstBorrow && Application.view(&owners[1]) == secondBorrow;
        if (wrapperNull != 0 || storageNull != 0 || wrapperOtherInitial != 1 || storageOtherInitial != 1 ||
            wrapperNonNull != 1 || storageNonNull != 1 || wrapperCleared != 0 || storageCleared != 0 ||
            wrapperOtherFinal != 1 || storageOtherFinal != 1 || !scalarAddressBitsPreserved || !cookiesPreserved || !fullWidthCanariesPreserved || !borrowedViewsStable)
            throw new InvalidOperationException("Both actual signed-byte Boolean bodies must return exact 0/1 after each owner mutation while scalar addresses, inert cookies and full-width canaries remain intact");

        var wrapperResults = new JsonArray();
        foreach (var value in new sbyte[] { wrapperNull, wrapperOtherInitial, wrapperNonNull, wrapperCleared, wrapperOtherFinal })
            wrapperResults.Add((JsonNode?)JsonValue.Create((int)value));
        var storageResults = new JsonArray();
        foreach (var value in new sbyte[] { storageNull, storageOtherInitial, storageNonNull, storageCleared, storageOtherFinal })
            storageResults.Add((JsonNode?)JsonValue.Create((int)value));
        return new JsonObject
        {
            ["ownerSize"] = sizeof(Owner), ["wrapperSize"] = sizeof(Wrapper), ["derivedStorageSize"] = sizeof(DerivedStorage),
            ["storageSize"] = sizeof(Storage), ["refCountCarrierSize"] = sizeof(RefCountCarrier),
            ["canaryOffset"] = canaryOffset, ["payloadOffset"] = payloadOffset, ["internalOffset"] = internalOffset,
            ["generatedBaseOffset"] = generatedBaseOffset, ["pointerOffset"] = pointerOffset, ["refcountOffset"] = refcountOffset, ["controlOffset"] = controlOffset,
            ["ownerSequentialPrefixOffset"] = ownerPrefixOffset, ["wrapperSequentialPrefixOffset"] = wrapperPrefixOffset,
            ["derivedStorageSequentialPrefixOffset"] = derivedPrefixOffset, ["storageSequentialPrefixOffset"] = storagePrefixOffset,
            ["refCountCarrierSequentialPrefixOffset"] = refcountPrefixOffset,
            ["ownerArrayStride"] = ownerStride, ["wrapperArrayStride"] = wrapperStride, ["derivedStorageArrayStride"] = derivedStride,
            ["storageArrayStride"] = storageStride, ["refCountCarrierArrayStride"] = refcountStride,
            ["alignedStorage"] = alignedStorage, ["alignedScalarAddresses"] = alignedScalarAddresses,
            ["firstReturnedOffset"] = firstReturnedOffset, ["secondReturnedOffset"] = secondReturnedOffset,
            ["exactFieldPointers"] = exactFieldPointers, ["returnedPointersDistinct"] = distinctPointers,
            ["aliasAndBaseAddressIdentity"] = aliasAndBaseAddressIdentity, ["borrowedViewsStable"] = borrowedViewsStable,
            ["scalarAddressBitsPreserved"] = scalarAddressBitsPreserved, ["cookiesPreserved"] = cookiesPreserved,
            ["fullWidthCanariesPreserved"] = fullWidthCanariesPreserved, ["wrapperResults"] = wrapperResults, ["storageResults"] = storageResults
        };
    }
}
